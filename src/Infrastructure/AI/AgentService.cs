using AI.Agents;
using Application.AI;
using Application.AI.Memory;
using Application.Common.Errors;
using Application.Common.Results;
using Application.DTOs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace Infrastructure.AI;

/// <summary>
/// IAIAgent'i ve pgvector tabanlı hafızayı Application katmanına adapt eden servis.
/// Şimdilik embedding üretimini dışarıdan bekliyoruz; ileride Semantic Kernel ile zenginleştirilebilir.
/// </summary>
public sealed class AgentService : IAgentService
{
    private readonly IAIAgent _aiAgent;
    private readonly AgentDbContext _dbContext;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        IAIAgent aiAgent,
        AgentDbContext dbContext,
        ILogger<AgentService> logger)
    {
        _aiAgent = aiAgent;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<AIResponse>> AskAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var answer = await _aiAgent.AskAsync(request.Question, cancellationToken);

            // Basit bir log/hafıza kaydı: embedding henüz yok, boş vector ile kaydediyoruz.
            var log = new AgentLog
            {
                UserQuestion = request.Question,
                AiResponse = answer,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.AgentLogs.Add(log);

            var memoryEntry = new AiMemoryEntry
            {
                Content = request.Question,
                Role = "user",
                Embedding = Vector.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.AiMemoryEntries.Add(memoryEntry);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<AIResponse>.Success(new AIResponse(answer, true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI agent call failed");
            return Result<AIResponse>.Failure(
                Error.Failure("AI.Failure", "AI agent call failed", ex.Message));
        }
    }
}

