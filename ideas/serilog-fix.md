Yeah — that’s a classic gotcha with Serilog in .NET.
What’s happening is that even though **you** set your minimum level to *Warning*, some **internal framework loggers** (like Microsoft.Hosting, ASP.NET Core, EF Core, etc.) are still being picked up at the *Information* level because of **sub-loggers or overrides** in configuration.

Let’s go step-by-step and fix it properly 👇

---

### ✅ 1. Make sure your `appsettings.json` minimum level is structured correctly

Here’s how it should look:

```json
"Serilog": {
  "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.MSSqlServer" ],
  "MinimumLevel": {
    "Default": "Warning",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "WriteTo": [
    {
      "Name": "MSSqlServer",
      "Args": {
        "connectionString": "YourConnectionString",
        "tableName": "Logs",
        "autoCreateSqlTable": true
      }
    }
  ],
  "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
}
```

🔸 The **critical piece** is the `"Override"` section.
If you don’t override `Microsoft` and `System`, ASP.NET Core logs all its internal stuff at `Information` by default.

---

### ✅ 2. Make sure you actually load Serilog from configuration

In your `Program.cs`, this is the proper pattern (especially for .NET 6+):

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
```

If you did something like `Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)` manually *before* building, it can sometimes pick up defaults before your overrides are applied.

---

### ✅ 3. Check if `MinimumLevel.ControlledBy` or dynamic overrides are active

If you use something like this:

```json
"MinimumLevel": {
  "ControlledBy": "SomeSwitch"
}
```

It can override your static settings. Make sure you’re not mixing dynamic control levels (via API or Serilog.Settings.Configuration bugs).

---

### ✅ 4. Check for other sinks initializing earlier

Sometimes, people configure another sink before configuration is read (for example in early startup diagnostics):

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
```

That’s fine — *but* you should immediately call `.UseSerilog()` after configuration load to replace it:

```csharp
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
```

If you forget to override it later, the bootstrap logger will keep logging everything (including Info).

---

### ✅ 5. (Optional) Explicit filter to drop anything below Warning

As an extra guardrail, you can filter globally like this:

```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Filter.ByIncludingOnly(e => e.Level >= Serilog.Events.LogEventLevel.Warning));
```

That’s the “nuclear” option — ensures *nothing* below `Warning` ever reaches the sink, no matter what other settings do.

---

### ✅ 6. Verify effective levels at runtime

You can test your configuration with this:

```csharp
Log.ForContext<Program>().Information("This is info");
Log.ForContext<Program>().Warning("This is warning");
```

Then inspect the table — only the Warning should appear.

---

### TL;DR

To stop the flood of `Information` logs in SQL Server:

1. Add `"Override": { "Microsoft": "Warning", "System": "Warning" }` in `appsettings.json`.
2. Use `builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration));`
3. (Optional) Add `.Filter.ByIncludingOnly(e => e.Level >= Warning)` if you want a hard global cutoff.

# Part 2

Excellent follow-up — and that’s a very nuanced distinction 👏

You’re right:

* `"Override": { "Microsoft": "Warning" }` *changes the minimum level* for Microsoft loggers — meaning they’ll still log **warnings and errors**.
* But if you want to **keep other information logs** (like your own app’s) while **completely excluding Microsoft’s `Information` noise** — you can do it with a **filter**, not with an override.

Let’s break it down.

---

## 🧠 Goal

Keep:

* Your own `Information`, `Warning`, `Error`, etc. logs.
  Exclude:
* Only Microsoft logs **at the Information level**.

---

## ✅ Option 1 — Use a `Filter.ByExcluding()` rule

This is the most straightforward and precise way.

In `Program.cs`:

```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Filter.ByExcluding(logEvent =>
        logEvent.Level == Serilog.Events.LogEventLevel.Information &&
        logEvent.Properties.TryGetValue("SourceContext", out var source) &&
        source.ToString().Contains("Microsoft"))
);
```

### What this does:

* Checks each log event.
* If the level is `Information`
* And the `SourceContext` (the logger’s category) contains `"Microsoft"`,
* That log event is dropped before it ever hits your sinks.

So:
✅ Your own `Information` logs stay.
🚫 Microsoft info logs vanish.

---

## ✅ Option 2 — Configuration-based filtering (no code)

If you prefer to keep it inside **appsettings.json**, you can use `Serilog.Filters.Expressions` package:

Install:

```bash
dotnet add package Serilog.Filters.Expressions
```

Then in your `appsettings.json`:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information"
  },
  "Filter": [
    {
      "Name": "ByExcluding",
      "Args": {
        "expression": "(@Level = 'Information' and SourceContext like 'Microsoft%')"
      }
    }
  ],
  "WriteTo": [
    {
      "Name": "MSSqlServer",
      "Args": {
        "connectionString": "YourConnectionString",
        "tableName": "Logs"
      }
    }
  ]
}
```

This uses Serilog’s expression filter syntax and works entirely from configuration.

---

## ✅ Option 3 — Fine-grained filter on specific sink

You can also apply that exclusion only to one sink (for example, exclude from SQL but still show in console):

```csharp
.WriteTo.MSSqlServer(
    connectionString: "...",
    tableName: "Logs",
    restrictedToMinimumLevel: LogEventLevel.Information,
    sinkOptions: new MSSqlServerSinkOptions { AutoCreateSqlTable = true }
)
.Filter.ByExcluding(logEvent =>
    logEvent.Level == LogEventLevel.Information &&
    logEvent.Properties.TryGetValue("SourceContext", out var src) &&
    src.ToString().Contains("Microsoft"))
.WriteTo.Console() // still shows everything
```

---

## TL;DR — Summary

| Approach                          | Description                                | Keeps your Info logs? | Code or Config |
| --------------------------------- | ------------------------------------------ | --------------------- | -------------- |
| Override `"Microsoft": "Warning"` | Changes min level for Microsoft            | ✅                     | Config         |
| Filter.ByExcluding                | Dynamically drops only Microsoft info logs | ✅                     | Code           |
| Serilog.Filters.Expressions       | Same as above, but via appsettings.json    | ✅                     | Config         |
| Per-sink filter                   | Filter only for certain sinks              | ✅                     | Code           |

---

👉 **Best practical setup** for your case:

```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Filter.ByExcluding(e =>
        e.Level == LogEventLevel.Information &&
        e.Properties.ContainsKey("SourceContext") &&
        e.Properties["SourceContext"].ToString().StartsWith("\"Microsoft"))
);
```

This way you keep all your app’s `Information` logs, and your SQL Server sink stays clean of noisy `Microsoft.Hosting.Lifetime` and `Microsoft.AspNetCore` entries.

---

# Part 3

Perfect — here’s a **clean, production-ready setup** that:
✅ Logs to SQL Server
✅ Keeps your app’s `Information`, `Warning`, `Error`, etc.
🚫 Excludes *only* Microsoft’s `Information` logs (but still keeps Microsoft warnings/errors)
✅ Loads everything from configuration
✅ Uses the standard `Program.cs` style for .NET 6–8

---

## 🗂️ `appsettings.json`

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.MSSqlServer" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "Server=.;Database=LogsDb;Trusted_Connection=True;",
          "tableName": "Logs",
          "autoCreateSqlTable": true
        }
      }
    ]
  }
}
```

🟢 Notice:

* We *don’t* override `Microsoft` to `Warning`.
* We only override `System`, because it’s rarely useful at `Information`.
* The filter to exclude Microsoft info logs will be in code.

---

## ⚙️ `Program.cs`

```csharp
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// --- Configure Serilog ---
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        // 🧠 Filter: Drop only Microsoft logs that are at Information level
        .Filter.ByExcluding(logEvent =>
            logEvent.Level == LogEventLevel.Information &&
            logEvent.Properties.TryGetValue("SourceContext", out var src) &&
            src.ToString().StartsWith("\"Microsoft"));
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
```

---

### 🧩 What happens at runtime

| Log Source                            | Level       | Result          |
| ------------------------------------- | ----------- | --------------- |
| `Microsoft.AspNetCore.Hosting`        | Information | 🚫 Filtered out |
| `Microsoft.AspNetCore.Server.Kestrel` | Warning     | ✅ Logged        |
| `System.Data.SqlClient`               | Warning     | ✅ Logged        |
| `YourApp.Controllers.HomeController`  | Information | ✅ Logged        |
| `YourApp.Services.MyService`          | Error       | ✅ Logged        |

---

### 💡 Bonus: Optional console theme & table column schema

If you want your SQL table to look nicer and store more structured data, you can add this under the MSSqlServer sink:

```json
"Args": {
  "connectionString": "Server=.;Database=LogsDb;Trusted_Connection=True;",
  "tableName": "Logs",
  "autoCreateSqlTable": true,
  "columnOptionsSection": {
    "additionalColumns": [
      { "ColumnName": "MachineName", "DataType": "nvarchar", "AllowNull": true },
      { "ColumnName": "ThreadId", "DataType": "int", "AllowNull": true }
    ]
  }
}
```

---

### 🧠 Key takeaway

* Use **filters** for selective exclusions (Microsoft info only).
* Use **overrides** when you want to change the minimum level globally for a namespace.
* Filters are more precise and don’t block warnings/errors from that namespace.

---

