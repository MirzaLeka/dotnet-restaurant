using DotNet8Starter.IoC;
using DotNet8Starter.IoC.Extensions.Health;
using DotNet8Starter.IoC.Extensions.Swagger;
using DotNet8Starter.IoC.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Dependency Injection
builder.Services.AddServicesExtensions(builder.Configuration, builder.Environment, builder.Host);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwaggerExtension();
}

app.UseMiddleware<TrackRequestDurationMiddleware>();

app.UseExceptionHandler();

app.MapCustomHealthChecks();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
