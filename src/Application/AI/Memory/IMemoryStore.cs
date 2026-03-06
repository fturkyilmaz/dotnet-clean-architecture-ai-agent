using Application.DTOs;

namespace Application.AI.Memory;

public sealed record MemorySearchResult(
    Guid Id,
    string Content,
    string Role,
    string? UserId,
    string? SessionId,
    string? Model,
    DateTime CreatedAt,
    double Distance);

/// <summary>
/// AI hafızası için abstraction: altında PostgreSQL + pgvector, Redis vb. implementasyonlar olabilir.
/// </summary>
public interface IMemoryStore
{
    Task SaveAsync(string? userId, string? sessionId, string role, string content, float[] embedding, string? model, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemorySearchResult>> SearchAsync(
        string? userId,
        string? sessionId,
        float[] embedding,
        int limit = 5,
        CancellationToken cancellationToken = default);
}

