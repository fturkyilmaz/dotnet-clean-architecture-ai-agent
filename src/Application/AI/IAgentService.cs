using Application.DTOs;
using Application.Common.Results;

namespace Application.AI;

/// <summary>
/// Application katmanının AI agent ile konuşurken kullandığı boundary.
/// </summary>
public interface IAgentService
{
    Task<Result<AIResponse>> AskAsync(AIRequest request, CancellationToken cancellationToken = default);
}

