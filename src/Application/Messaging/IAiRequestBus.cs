using Application.DTOs;
using Application.Common.Results;

namespace Application.Messaging;

/// <summary>
/// AI isteklerini event-driven hatta göndermek için Application seviyesindeki abstraction.
/// Altında MassTransit + RabbitMQ + Outbox implementasyonu çalışır.
/// </summary>
public interface IAiRequestBus
{
    Task<Result<Guid>> EnqueueAsync(
        AIRequest request,
        string? sessionId,
        string? userId,
        CancellationToken cancellationToken = default);
}

