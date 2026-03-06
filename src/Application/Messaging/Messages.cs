namespace Application.Messaging;

public interface AIRequestMessage
{
    Guid CorrelationId { get; }
    string Question { get; }
    string? SessionId { get; }
    string? UserId { get; }
    DateTime Timestamp { get; }
}

public interface AIResponseMessage
{
    Guid CorrelationId { get; }
    string Answer { get; }
    bool Success { get; }
    string? Error { get; }
    long LatencyMs { get; }
    DateTime Timestamp { get; }
}

public interface AIRequestAccepted
{
    Guid CorrelationId { get; }
    DateTime Timestamp { get; }
}
