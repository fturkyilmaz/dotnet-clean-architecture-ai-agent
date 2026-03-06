using Application.Messaging;

namespace Infrastructure.Messaging;

public sealed record AiResponseMessagePayload(
    Guid CorrelationId,
    string Answer,
    bool Success,
    string? Error,
    long LatencyMs,
    DateTime Timestamp
) : AIResponseMessage;

