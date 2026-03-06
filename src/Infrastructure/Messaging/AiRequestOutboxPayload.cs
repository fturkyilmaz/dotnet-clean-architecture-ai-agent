using Application.Messaging;

namespace Infrastructure.Messaging;

public sealed record AiRequestOutboxPayload(
    Guid CorrelationId,
    string Question,
    string? SessionId,
    string? UserId,
    DateTime Timestamp
) : AIRequestMessage;

