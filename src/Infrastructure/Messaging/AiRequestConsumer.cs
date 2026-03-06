using AI.Agents;
using Application.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace Infrastructure.Messaging;

/// <summary>
/// RabbitMQ'dan AIRequestMessage tüketip IAIAgent ile yanıt üreten ve sonucu log + memory + AIResponseMessage olarak yayınlayan consumer.
/// </summary>
public sealed class AiRequestConsumer : IConsumer<AIRequestMessage>
{
    private readonly IAIAgent _aiAgent;
    private readonly AgentDbContext _dbContext;
    private readonly ILogger<AiRequestConsumer> _logger;

    public AiRequestConsumer(
        IAIAgent aiAgent,
        AgentDbContext dbContext,
        ILogger<AiRequestConsumer> logger)
    {
        _aiAgent = aiAgent;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AIRequestMessage> context)
    {
        var message = context.Message;
        var started = DateTime.UtcNow;

        _logger.LogInformation("Consuming AIRequestMessage {CorrelationId}", message.CorrelationId);

        string answer;
        bool success = true;
        string? error = null;

        try
        {
            answer = await _aiAgent.AskAsync(message.Question, context.CancellationToken);
        }
        catch (Exception ex)
        {
            success = false;
            error = ex.Message;
            answer = string.Empty;
            _logger.LogError(ex, "Error processing AIRequestMessage {CorrelationId}", message.CorrelationId);
        }

        var finished = DateTime.UtcNow;
        var latencyMs = (long)(finished - started).TotalMilliseconds;

        // AgentLog kaydı
        var log = new AgentLog
        {
            SessionId = message.SessionId,
            UserQuestion = message.Question,
            AiResponse = answer,
            ResponseTimeMs = latencyMs,
            ErrorMessage = error,
            UserId = message.UserId,
            CreatedAt = finished
        };

        _dbContext.AgentLogs.Add(log);

        // Basit memory entry (embedding şimdilik boş)
        var memoryEntry = new AiMemoryEntry
        {
            UserId = message.UserId,
            SessionId = message.SessionId,
            Role = "assistant",
            Content = answer,
            Embedding = Vector.Empty,
            CreatedAt = finished
        };

        _dbContext.AiMemoryEntries.Add(memoryEntry);

        await _dbContext.SaveChangesAsync(context.CancellationToken);

        // Response event publish et
        var response = new AiResponseMessagePayload(
            message.CorrelationId,
            answer,
            success,
            error,
            latencyMs,
            finished);

        await context.Publish<AIResponseMessage>(response, context.CancellationToken);

        _logger.LogInformation("Published AIResponseMessage {CorrelationId}", message.CorrelationId);
    }
}

