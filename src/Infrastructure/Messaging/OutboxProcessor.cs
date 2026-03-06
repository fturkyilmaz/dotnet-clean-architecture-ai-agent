using System.Text.Json;
using Application.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging;

/// <summary>
/// Outbox tablosundaki bekleyen AIRequestMessage kayıtlarını RabbitMQ'ya publish eden background service.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var pending = await dbContext.OutboxMessages
                    .Where(x => x.ProcessedAt == null && x.RetryCount < x.MaxRetries && x.Type == "AIRequestMessage")
                    .OrderBy(x => x.CreatedAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (pending.Count == 0)
                {
                    await Task.Delay(PollInterval, stoppingToken);
                    continue;
                }

                foreach (var message in pending)
                {
                    try
                    {
                        var payload = JsonSerializer.Deserialize<AiRequestOutboxPayload>(message.Content);
                        if (payload is null)
                        {
                            throw new InvalidOperationException("Failed to deserialize outbox message content.");
                        }

                        await publishEndpoint.Publish<AIRequestMessage>(payload, stoppingToken);

                        message.ProcessedAt = DateTime.UtcNow;
                        message.Error = null;
                    }
                    catch (Exception ex)
                    {
                        message.RetryCount += 1;
                        message.Error = ex.Message;
                        _logger.LogError(ex, "Error processing outbox message {OutboxId}", message.Id);
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Outbox processor loop failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox processor stopped");
    }
}

