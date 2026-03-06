using System.Diagnostics;
using System.Threading.RateLimiting;
using Application;
using Application.AI.Commands.AskAi;
using Application.DTOs;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Context;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication & Authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.RequireHttpsMetadata = bool.TryParse(builder.Configuration["Auth:RequireHttpsMetadata"], out var requireHttps)
            ? requireHttps
            : true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUser", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("RequireAdmin", policy =>
    {
        policy.RequireRole("Admin", "admin");
    });
});

// Rate limiting
var permitPerMinute = builder.Configuration.GetValue<int?>("RateLimiting:PermitLimitPerMinute") ?? 60;

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = permitPerMinute;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

// OpenTelemetry (traces + metrics)
var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "DotnetCleanArchitectureAiAgent.Api";
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
            });
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

// CorrelationId / TraceId log enrichment
app.Use(async (context, next) =>
{
    var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    using (LogContext.PushProperty("TraceId", traceId))
    {
        await next();
    }
});

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/ai/ask", async (string question, ISender sender, CancellationToken cancellationToken) =>
{
    var result = await sender.Send(new AskAiCommand(question), cancellationToken);

    if (result.IsFailure)
    {
        var error = result.Error;
        return Results.BadRequest(new AIResponse("", false, error?.Message));
    }

    return Results.Ok(result.Value);
})
.WithName("AskAI")
.RequireAuthorization("RequireUser")
.RequireRateLimiting("api");

app.MapPost("/ai/ask", async (AIRequest request, ISender sender, CancellationToken cancellationToken) =>
{
    var result = await sender.Send(new AskAiCommand(request.Question), cancellationToken);

    if (result.IsFailure)
    {
        var error = result.Error;
        return Results.BadRequest(new AIResponse("", false, error?.Message));
    }

    return Results.Ok(result.Value);
})
.WithName("AskAIPost")
.RequireAuthorization("RequireUser")
.RequireRateLimiting("api");

app.Run();
