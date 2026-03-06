using System.Text.Json;
using Application.Common.Errors;
using Application.Common.Results;
using Application.DTOs;
using Application.Messaging;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging;

/// <summary>
/// Outbox pattern kullanarak AIRequestMessage'ları kuyruğa yazmak için publisher.
/// Şu an sadece Outbox tablosuna yazar; gerçek publish işlemi için OutboxProcessor background service çalışır.
/// </summary>
public sealed class MassTransitAiRequestBus : IAiRequestBus
{
    private readonly AgentDbContext _dbContext;
    private readonly ILogger<MassTransitAiRequestBus> _logger;

    public MassTransitAiRequestBus(AgentDbContext dbContext, ILogger<MassTransitAiRequestBus> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> EnqueueAsync(
        AIRequest request,
        string? sessionId,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationId = Guid.NewGuid();

            var message = new AiRequestOutboxPayload(
                correlationId,
                request.Question,
                sessionId,
                userId,
                DateTime.UtcNow);

            var outbox = new OutboxMessage
            {
                Type = "AIRequestMessage",
                Content = JsonSerializer.Serialize(message),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.OutboxMessages.Add(outbox);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Enqueued AI request {CorrelationId} to outbox", correlationId);

            return Result<Guid>.Success(correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue AI request");
            return Result<Guid>.Failure(
                Error.Failure("Outbox.EnqueueFailed", "Failed to enqueue AI request", ex.Message));
        }
    }
}

