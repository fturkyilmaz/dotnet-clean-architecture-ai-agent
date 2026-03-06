using AI.Agents;
using Application.AI;
using Application.Messaging;
using Application.Security;
using Infrastructure.AI;
using Infrastructure.Caching;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Security;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Pgvector.EntityFrameworkCore;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL + pgvector
        services.AddDbContext<AgentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? "Host=localhost;Port=5432;Database=agent_db;Username=postgres;Password=postgres";

            options.UseNpgsql(connectionString, o => o.UseVector());
        });

        // Redis (ConnectionMultiplexer + session cache + conversation history)
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<ISessionCache, RedisSessionCache>();
        services.AddScoped<IConversationHistory, RedisConversationHistory>();

        // Semantic Kernel + IAIAgent
        services.AddSingleton<Kernel>(_ =>
        {
            var builder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    modelId: "gpt-4o",
                    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "",
                    orgId: Environment.GetEnvironmentVariable("OPENAI_ORG_ID"));

            return builder.Build();
        });

        services.AddScoped<IAIAgent, SemanticKernelAgent>();
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IPromptSanitizer, PromptSanitizer>();

        // Messaging & Outbox
        services.AddScoped<IAiRequestBus, MassTransitAiRequestBus>();

        // MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<AiRequestConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration.GetValue<string>("RabbitMq:Host") ?? "localhost";
                var username = configuration.GetValue<string>("RabbitMq:Username") ?? "guest";
                var password = configuration.GetValue<string>("RabbitMq:Password") ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ReceiveEndpoint("ai-requests", e =>
                {
                    e.ConfigureConsumer<AiRequestConsumer>(context);
                });
            });
        });

        // Outbox background processor
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}

