## part 1

Excellent question — and you’re thinking exactly like someone who’s designing a **correlated logging / trace context** system 👍

In Serilog, what you’re describing is usually called **“enriching the log context”**, and it’s the *right* way to ensure that every log message in the same logical operation (e.g., request) carries the same unique identifier (correlation ID, trace ID, request ID, etc.).

Let’s go step-by-step 👇

---

## 🧩 1. The Idea: Use LogContext or Enrichers

In Serilog, you can attach extra properties to every log entry that’s part of a given **scope** or **request**.

You do this using:

```csharp
LogContext.PushProperty("RequestId", requestId);
```

Then, every log call made inside that scope will automatically include `RequestId`.

---

## 🧠 2. Example in Middleware (ASP.NET Core)

If your API is ASP.NET Core, the cleanest way is to create a middleware that:

1. Checks if the request has a correlation ID header (or generates one if not),
2. Adds it to the Serilog context,
3. Logs everything downstream with that ID attached.

Example:

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get the incoming correlation ID header
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        // Add it to the response headers too
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Push the ID into the Serilog context
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Handling request {Path}", context.Request.Path);
            await _next(context);
            _logger.LogInformation("Finished handling request");
        }
    }
}
```

Then register it in `Program.cs`:

```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
```

---

## ⚙️ 3. Enrich Your Log Output

You need to make sure your log output template includes this property:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

Now your console output will look like this:

```
[10:33:14 INF] (abc123) Handling request /api/user
[10:33:15 INF] (abc123) Finished handling request
```

---

## 💾 4. If You Log to File / Seq / Database

All sinks that support structured logging (like JSON file, Seq, Elasticsearch, etc.) will automatically include `CorrelationId` as a property.
That means you can later query:

```
CorrelationId = "abc123"
```

and retrieve all related events instantly.

---

## 🧩 5. Optional: Make It Available Everywhere

If you’re using services or background tasks, you can:

* Store the correlation ID in `AsyncLocal<T>` to access it anywhere,
* Or explicitly pass it into your service methods,
* Or enrich globally via a custom enricher class.

Here’s an example of a global enricher:

```csharp
public class CorrelationIdEnricher : ILogEventEnricher
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public static void Set(string id) => _correlationId.Value = id;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_correlationId.Value != null)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("CorrelationId", _correlationId.Value));
        }
    }
}
```

And register:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.With<CorrelationIdEnricher>()
    .CreateLogger();
```

Then from middleware:

```csharp
CorrelationIdEnricher.Set(correlationId);
```

---

## ✅ TL;DR Summary

| Step | Purpose                        | Example                                                |
| ---- | ------------------------------ | ------------------------------------------------------ |
| 1    | Generate correlation ID        | `var id = Guid.NewGuid().ToString();`                  |
| 2    | Push it into context           | `using (LogContext.PushProperty("CorrelationId", id))` |
| 3    | Add `.Enrich.FromLogContext()` | In logger configuration                                |
| 4    | Include in output template     | `({CorrelationId})`                                    |
| 5    | Query logs later               | `CorrelationId = "abc123"`                             |

---

Would you like me to show how to **automatically propagate that correlation ID from controller → service → repository** without explicitly passing it through every method parameter?


## part 2

Ah — perfect clarification 👏
You’re absolutely right: in a **background service** or **message-driven** architecture (e.g., queue consumers, Kafka, RabbitMQ, SQS, etc.), there is **no HTTP context or middleware pipeline**, so you can’t use the classic ASP.NET-style correlation middleware.

But don’t worry — Serilog still gives you the tools to do this elegantly, and there are **two great patterns** for your situation.

Let’s break them down carefully. 👇

---

## 🧠 Problem Recap

You have:

* A background service subscribed to a message queue.
* Each incoming message has a unique ID (e.g., in headers like `MessageId`, `CorrelationId`, `X-Trace-ID`, etc.).
* You want all logs produced while processing that message — no matter what class or method — to include that ID automatically.

---

## ✅ Pattern 1: Use `LogContext.PushProperty` Around Your Message Processing Scope

When you pick up a message, you can push the correlation ID into the Serilog context **for the entire duration of that message handling**.

### Example

```csharp
public class QueueConsumer : BackgroundService
{
    private readonly ILogger<QueueConsumer> _logger;

    public QueueConsumer(ILogger<QueueConsumer> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await ReceiveMessageAsync(); // however you do it
            var correlationId = message.Headers["X-Correlation-ID"] ?? Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                _logger.LogInformation("Received message {MessageId}", message.Id);

                try
                {
                    await ProcessMessageAsync(message);
                    _logger.LogInformation("Successfully processed message {MessageId}", message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message {MessageId}", message.Id);
                }
            }
        }
    }

    private Task<Message> ReceiveMessageAsync() => ...;
    private Task ProcessMessageAsync(Message msg) => ...;
}
```

✅ Now every log written during `ProcessMessageAsync` automatically carries the correlation ID — even deep inside services or repositories — **as long as** you use Serilog’s ambient logging (or any `ILogger<T>` with `Serilog.Extensions.Logging`).

---

## ⚙️ Logger Configuration

Make sure your logger setup includes:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

Example output:

```
[12:14:05 INF] (abc123) Received message 42
[12:14:06 INF] (abc123) Starting to process message 42
[12:14:07 ERR] (abc123) Error processing message 42
```

---

## 🧩 Pattern 2: Use an AsyncLocal-based Global Enricher (If You Don’t Want to Push/Pop)

If you have a lot of nested calls and you don’t want to wrap everything in `using (LogContext.PushProperty(...))`, you can store the correlation ID in an `AsyncLocal<T>` variable that Serilog reads automatically on every log event.

### Step 1: Create an Enricher

```csharp
public class CorrelationIdEnricher : ILogEventEnricher
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public static void Set(string id) => _correlationId.Value = id;
    public static void Clear() => _correlationId.Value = null;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_correlationId.Value is { } id)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("CorrelationId", id));
        }
    }
}
```

### Step 2: Register it with Serilog

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.With<CorrelationIdEnricher>()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

### Step 3: Use it in your consumer

```csharp
var correlationId = message.Headers["X-Correlation-ID"] ?? Guid.NewGuid().ToString();
CorrelationIdEnricher.Set(correlationId);

_logger.LogInformation("Started handling message {MessageId}", message.Id);

// ... process

CorrelationIdEnricher.Clear(); // optional cleanup
```

Now you can log anywhere — without wrapping scopes — and every log event during this async flow will automatically include that ID.

---

## 🧭 When to Use Which

| Scenario                                      | Recommended Pattern                           |
| --------------------------------------------- | --------------------------------------------- |
| Simple “process one message in a scope”       | `LogContext.PushProperty`                     |
| Long-running async tasks / deep method chains | AsyncLocal-based enricher                     |
| Want flexibility (both HTTP + queue)          | Use enricher, set from middleware or consumer |

---

## 🧠 Bonus: Combine With Structured Logging

Because Serilog is structured, this `CorrelationId` becomes a searchable field in:

* **Seq** (`CorrelationId = 'abc123'`)
* **ElasticSearch**
* **Azure Application Insights**
* **Console filters (via grep or jq if JSON logs)**

---

Would you like me to show how to combine both approaches — so your app can automatically attach correlation IDs **both from HTTP requests and queue messages**, without duplicating logic?

## part 3

Perfect — staying focused on the **queue-based correlation** path 👍

Now let’s get into how you’d **store those enriched logs (with CorrelationId and other custom fields)** in **SQL Server** using `ILogger` + Serilog.

---

## 🧠 The Big Picture

You’ll use Serilog with:

* **Microsoft.Extensions.Logging** (so you can use `ILogger<T>` anywhere)
* **Serilog.Sinks.MSSqlServer** (the sink that writes logs to SQL Server)

Then, all those enriched properties (like `CorrelationId`, `MessageId`, etc.) will be written as **columns** in a database table.

---

## 🧩 Step 1: Install Required NuGet Packages

In your project (where `Program.cs` or `HostBuilder` is configured):

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.MSSqlServer
dotnet add package Serilog.Extensions.Logging
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Thread
```

---

## ⚙️ Step 2: Configure Logger (in `Program.cs`)

Here’s a sample configuration that:

* Logs to console
* Logs to SQL Server
* Includes enriched context (CorrelationId, etc.)
* Creates the table automatically if it doesn’t exist (optional)

```csharp
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;

var connectionString = "Server=localhost;Database=LogsDB;Trusted_Connection=True;";

// Define the SQL column mappings
var columnOptions = new ColumnOptions
{
    AdditionalColumns = new Collection<SqlColumn>
    {
        new SqlColumn("CorrelationId", System.Data.SqlDbType.NVarChar, dataLength: 100),
        new SqlColumn("MessageId", System.Data.SqlDbType.NVarChar, dataLength: 100),
        new SqlColumn("MachineName", System.Data.SqlDbType.NVarChar, dataLength: 128)
    }
};

// Optional: automatically create table if not exists
columnOptions.Store.Remove(StandardColumn.Properties); // keep Properties only if you want JSON payloads
columnOptions.PrimaryKey = columnOptions.IdColumn;
columnOptions.TimeStamp.NonClusteredIndex = true;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "Logs",
            AutoCreateSqlTable = true // <-- this line makes it create the table automatically
        },
        restrictedToMinimumLevel: LogEventLevel.Information,
        columnOptions: columnOptions)
    .CreateLogger();

builder.Host.UseSerilog();
```

---

## 🧱 Step 3: Table Creation (Automatic vs Manual)

### ✅ Option A: Auto-create

If you set:

```csharp
AutoCreateSqlTable = true
```

Serilog will automatically create the `Logs` table with the default schema and your `AdditionalColumns`.

### 🧰 Option B: Manual creation

If you prefer to control it manually, here’s the typical schema:

```sql
CREATE TABLE [dbo].[Logs]
(
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Message] NVARCHAR(MAX),
    [MessageTemplate] NVARCHAR(MAX),
    [Level] NVARCHAR(128),
    [TimeStamp] DATETIMEOFFSET NOT NULL,
    [Exception] NVARCHAR(MAX) NULL,
    [Properties] NVARCHAR(MAX),
    [CorrelationId] NVARCHAR(100) NULL,
    [MessageId] NVARCHAR(100) NULL,
    [MachineName] NVARCHAR(128) NULL
);
```

You can add any custom columns you want — Serilog maps them by name.

---

## 🔁 Step 4: Enrich Logs (Your Correlation Logic)

Since you’re in a background service, you can combine the earlier approach:

```csharp
using (LogContext.PushProperty("CorrelationId", correlationId))
using (LogContext.PushProperty("MessageId", message.Id))
{
    _logger.LogInformation("Processing started for message {MessageId}", message.Id);
    ...
}
```

or, if you went with the `AsyncLocal`-based enricher:

```csharp
CorrelationIdEnricher.Set(correlationId);
_logger.LogInformation("Started message processing.");
```

Either way, the correlation data will automatically appear in SQL columns.

---

## 🧾 Step 5: Verify Your Output

When logs start coming in, you’ll see something like this in SQL Server:

| Id | TimeStamp           | Level | Message                     | CorrelationId | MessageId | Exception | MachineName |
| -- | ------------------- | ----- | --------------------------- | ------------- | --------- | --------- | ----------- |
| 1  | 2025-10-20 12:14:05 | INF   | Received message 42         | abc123        | msg-42    | NULL      | MyServer    |
| 2  | 2025-10-20 12:14:07 | ERR   | Error processing message 42 | abc123        | msg-42    | ...       | MyServer    |

---

## ⚡ Summary

| Step | Description                                               |
| ---- | --------------------------------------------------------- |
| 1    | Install Serilog packages                                  |
| 2    | Configure `MSSqlServerSink` and enrichers                 |
| 3    | Add `AutoCreateSqlTable = true` (or create manually)      |
| 4    | Push properties (CorrelationId, MessageId, etc.) in scope |
| 5    | Logs automatically include those fields in SQL            |

---

Would you like me to show an example of **how to dynamically add different custom properties** (e.g. `QueueName`, `HandlerName`, etc.) per message, so they appear as SQL columns too?

## part 4

💯 Excellent — you’re absolutely on the right track.

Let’s tackle both questions clearly and precisely:

---

## ✅ 1️⃣ How to log with `ILogger` (while Serilog adds the properties)

Once Serilog is set up as your logging provider (via `builder.Host.UseSerilog()`), **you don’t need to reference `Log` or `Serilog` directly anymore**.

You just use the built-in `ILogger<T>` (from Microsoft.Extensions.Logging).

Example:

```csharp
public class MyMessageHandler
{
    private readonly ILogger<MyMessageHandler> _logger;

    public MyMessageHandler(ILogger<MyMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleMessageAsync(MyMessage message)
    {
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", message.CorrelationId))
        using (Serilog.Context.LogContext.PushProperty("MessageId", message.Id))
        {
            _logger.LogInformation("Processing message: {Id}", message.Id);

            try
            {
                await DoWorkAsync(message);
                _logger.LogInformation("Finished processing message {Id}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {Id}", message.Id);
            }
        }
    }
}
```

✅ When you log like this, Serilog automatically:

* Captures `MessageId` and `CorrelationId` (from the `LogContext`)
* Writes them as columns to SQL Server (or properties in structured sinks like Seq)
* Works fine even though you’re using `ILogger<T>` — because Serilog has registered as the underlying provider.

You never need to write `Log.Information()` or deal with Serilog APIs directly, unless you want to.

---

## 🧠 2️⃣ Adding a custom “Payload” property (serialized JSON object)

This is a **great** use case — logging structured payloads (e.g. message bodies or DTOs).
You can absolutely do it, either as:

### Option A — Pass it as a property

Serilog automatically serializes complex objects:

```csharp
_logger.LogInformation("Received message with payload: {@Payload}", myObject);
```

* The `@` symbol tells Serilog to *serialize the object as structured JSON*.
* If your sink is JSON-based (e.g., Seq or Elastic), it will store full JSON.
* For SQL Server, it will store the stringified JSON if you define a column for it.

To make sure it’s stored properly in SQL, add a column:

```csharp
new SqlColumn("Payload", SqlDbType.NVarChar, dataLength: -1) // -1 = MAX
```

Then when you log:

```csharp
using (LogContext.PushProperty("Payload", JsonSerializer.Serialize(myObject)))
{
    _logger.LogInformation("Processing message {Id}", message.Id);
}
```

or, equivalently:

```csharp
_logger.LogInformation("Processing message {Id} with payload {Payload}", message.Id, JsonSerializer.Serialize(myObject));
```

✅ This will end up in SQL as a `Payload` column containing the JSON string.

---

## 🧩 3️⃣ Pro Tip: You can push multiple properties at once

If you have several related properties to attach for this message, you can push them all together:

```csharp
using (LogContext.PushProperty("CorrelationId", correlationId))
using (LogContext.PushProperty("MessageId", message.Id))
using (LogContext.PushProperty("QueueName", queueName))
using (LogContext.PushProperty("Payload", JsonSerializer.Serialize(message.Body)))
{
    _logger.LogInformation("Handling message {MessageId} from {QueueName}", message.Id, queueName);
}
```

Every log inside that block — even deep inside your called methods — will carry all these properties automatically.

---

## 🧾 Example SQL Output

| Id | Timestamp           | Level | Message                                 | CorrelationId | MessageId | QueueName  | Payload                       | Exception |
| -- | ------------------- | ----- | --------------------------------------- | ------------- | --------- | ---------- | ----------------------------- | --------- |
| 1  | 2025-10-20 12:14:05 | INF   | Handling message msg-42 from main-queue | abc123        | msg-42    | main-queue | {"User":"Mirza","Amount":100} | NULL      |

---

## 🧱 Summary

| Task                       | How to do it                                               |
| -------------------------- | ---------------------------------------------------------- |
| Log normally               | `_logger.LogInformation("Processing {Id}", message.Id)`    |
| Attach custom properties   | `using (LogContext.PushProperty("CorrelationId", id))`     |
| Serialize payload          | `JsonSerializer.Serialize(obj)` or `@Payload` syntax       |
| Store payload in SQL       | Add NVARCHAR(MAX) column via `AdditionalColumns`           |
| Keep everything correlated | Push the same `CorrelationId` for all logs of that message |

---

Would you like me to show you a **fully working end-to-end example** — one that processes a message from a queue, logs it using `ILogger`, and results in actual rows in SQL (with correlation, message ID, and payload)?

## part 5

🔥 Excellent thinking — that’s exactly the right intuition. You’re noticing that `using (LogContext.PushProperty(...))` everywhere gets **repetitive** and breaks DRY.
The good news: Serilog is flexible enough to let you encapsulate this beautifully — either with a **custom logger wrapper** or a **contextual logger factory/enricher**.

Let’s walk through how to do this properly.

---

## 🧩 The Goal

We want to log with:

```csharp
_sqlLogger.LogInformation("Processing message {@Payload}", payload);
```

…and **automatically** include:

* `CorrelationId`
* `MessageId`
* `QueueName`
* maybe even `Payload` (if not passed directly)

…without manually calling `PushProperty` each time.

---

## ✅ Option 1 — Custom `ContextualLogger<T>` Wrapper (Simple and Clean)

We can wrap the standard `ILogger<T>` and automatically enrich every log call with message-specific context.

### Step 1: Create a wrapper class

```csharp
public class ContextualLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _innerLogger;
    private readonly string _correlationId;
    private readonly string _messageId;
    private readonly string _queueName;
    private readonly object? _payload;

    public ContextualLogger(
        ILogger<T> innerLogger,
        string correlationId,
        string messageId,
        string queueName,
        object? payload = null)
    {
        _innerLogger = innerLogger;
        _correlationId = correlationId;
        _messageId = messageId;
        _queueName = queueName;
        _payload = payload;
    }

    private IDisposable PushContext()
    {
        var props = new List<IDisposable>
        {
            Serilog.Context.LogContext.PushProperty("CorrelationId", _correlationId),
            Serilog.Context.LogContext.PushProperty("MessageId", _messageId),
            Serilog.Context.LogContext.PushProperty("QueueName", _queueName)
        };

        if (_payload != null)
        {
            props.Add(Serilog.Context.LogContext.PushProperty("Payload", 
                JsonSerializer.Serialize(_payload)));
        }

        return new CompositeDisposable(props);
    }

    public IDisposable BeginScope<TState>(TState state) => _innerLogger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        using (PushContext())
        {
            _innerLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    private sealed class CompositeDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;
        public CompositeDisposable(IEnumerable<IDisposable> disposables) => _disposables = disposables;
        public void Dispose() { foreach (var d in _disposables) d.Dispose(); }
    }
}
```

---

### Step 2: Use it in your queue consumer

```csharp
public class QueueConsumer
{
    private readonly ILogger<QueueConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public QueueConsumer(ILogger<QueueConsumer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessMessageAsync(MyMessage message)
    {
        // Create a contextual logger for this message
        var contextualLogger = new ContextualLogger<QueueConsumer>(
            _logger,
            correlationId: message.Headers["CorrelationId"] ?? Guid.NewGuid().ToString(),
            messageId: message.Id,
            queueName: "MyQueue",
            payload: message.Body
        );

        contextualLogger.LogInformation("Started processing {@Payload}", message.Body);

        try
        {
            // Process
            await Task.Delay(100);
            contextualLogger.LogInformation("Finished message {MessageId}", message.Id);
        }
        catch (Exception ex)
        {
            contextualLogger.LogError(ex, "Error processing message {MessageId}", message.Id);
        }
    }
}
```

✅ **Result:**
All logs go through your wrapper →
they automatically push `CorrelationId`, `MessageId`, `QueueName`, and `Payload` into Serilog’s context →
and you can still log using standard `ILogger<T>` syntax.

---

### Step 3: SQL Sink setup stays the same

No changes needed; your table will still capture the enriched properties:

| Id | Timestamp | Level | Message | CorrelationId | MessageId | QueueName | Payload | Exception |
| -- | --------- | ----- | ------- | ------------- | --------- | --------- | ------- | --------- |

---

## 🧠 Option 2 — Custom `ILoggerProvider` (Advanced & DI-based)

If you want to go **full enterprise-level**, you can even register your own provider that automatically wraps `ILogger<T>` in a contextual logger depending on message metadata (e.g., from an `AsyncLocal<MessageContext>`).

That’s how you’d scale this if your background service has dozens of handlers.

But for now, the `ContextualLogger<T>` wrapper is ideal — small, testable, and DRY.

---

## 🧱 Summary

| Problem                               | Solution                                                |
| ------------------------------------- | ------------------------------------------------------- |
| Too much `PushProperty` boilerplate   | Wrap logger in `ContextualLogger<T>`                    |
| Need automatic enrichment per message | Inject message-specific context into wrapper            |
| Want SQL to capture JSON payloads     | Add NVARCHAR(MAX) `Payload` column                      |
| Use as normal                         | `_logger.LogInformation("Message {@Payload}", payload)` |

---

Would you like me to show how to **inject this `ContextualLogger<T>` automatically** via dependency injection (so you can just ask for `IContextualLogger<T>` in constructors)?

## part 6

Perfect — let’s make this clean, reusable, and fully automatic.

Below is a **complete pattern** for how to implement your own custom `SqlLogger<T>` that wraps the standard `ILogger<T>` and **automatically enriches** log messages with context (like CorrelationId, QueueName, Payload, etc.), so you never repeat that `using (LogContext.PushProperty(...))` boilerplate again.

---

### 🧩 Step 1: Create an `IMessageContextAccessor`

This lets you store contextual info (like correlation ID, queue name, payload) per message — similar to `HttpContextAccessor` in ASP.NET.

```csharp
public interface IMessageContextAccessor
{
    MessageContext Current { get; set; }
}

public class MessageContext
{
    public string CorrelationId { get; set; }
    public string MessageId { get; set; }
    public string QueueName { get; set; }
    public object Payload { get; set; }
}

public class MessageContextAccessor : IMessageContextAccessor
{
    private static readonly AsyncLocal<MessageContext?> _context = new();

    public MessageContext Current
    {
        get => _context.Value!;
        set => _context.Value = value;
    }
}
```

This will allow your message processing logic (e.g., your consumer or handler) to set the context once, and all logs inside that handler will use the same enrichment automatically.

---

### 🧩 Step 2: Create a custom `SqlLogger<T>`

```csharp
public class SqlLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _innerLogger;
    private readonly IMessageContextAccessor _contextAccessor;

    public SqlLogger(ILogger<T> innerLogger, IMessageContextAccessor contextAccessor)
    {
        _innerLogger = innerLogger;
        _contextAccessor = contextAccessor;
    }

    public IDisposable BeginScope<TState>(TState state)
        => _innerLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel)
        => _innerLogger.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var ctx = _contextAccessor.Current;
        if (ctx != null)
        {
            using (LogContext.PushProperty("CorrelationId", ctx.CorrelationId))
            using (LogContext.PushProperty("MessageId", ctx.MessageId))
            using (LogContext.PushProperty("QueueName", ctx.QueueName))
            using (LogContext.PushProperty("Payload", JsonSerializer.Serialize(ctx.Payload)))
            {
                _innerLogger.Log(logLevel, eventId, state, exception, formatter);
                return;
            }
        }

        _innerLogger.Log(logLevel, eventId, state, exception, formatter);
    }
}
```

This ensures that every log call automatically includes the context properties **if they’re set**.

---

### 🧩 Step 3: Register it in DI

In your `Program.cs` or `Startup.cs`:

```csharp
builder.Services.AddSingleton<IMessageContextAccessor, MessageContextAccessor>();
builder.Services.AddScoped(typeof(ILogger<>), typeof(SqlLogger<>));
```

This replaces the default `ILogger<T>` injection with your wrapped version.

---

### 🧩 Step 4: Use it in your message handler

You can now just log as usual, and all contextual properties will be automatically enriched.

```csharp
public class MyQueueHandler
{
    private readonly ILogger<MyQueueHandler> _logger;
    private readonly IMessageContextAccessor _contextAccessor;

    public MyQueueHandler(ILogger<MyQueueHandler> logger, IMessageContextAccessor contextAccessor)
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
    }

    public async Task HandleAsync(MyMessage message)
    {
        _contextAccessor.Current = new MessageContext
        {
            CorrelationId = Guid.NewGuid().ToString(),
            MessageId = message.Id,
            QueueName = "OrderQueue",
            Payload = message.Body
        };

        _logger.LogInformation("Processing message...");
        // ... your logic ...
        _logger.LogInformation("Processing complete");
    }
}
```

---

### ✅ Result

When you log with:

```csharp
_logger.LogInformation("Processing message...");
```

Serilog automatically writes into SQL Server with columns like:

```
Timestamp | Level | Message | CorrelationId | MessageId | QueueName | Payload
```

…and you never repeat the push-property boilerplate.

---

Would you like me to extend this example so that it **automatically creates the SQL table if it doesn’t exist**, using Serilog’s schema creation feature (instead of manual table setup)?
