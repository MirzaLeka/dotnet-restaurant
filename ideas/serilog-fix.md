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

---
