namespace AI.Agents;

public interface IAIAgent
{
    Task<string> AskAsync(string question, CancellationToken cancellationToken = default);
    Task<string> AskWithContextAsync(string question, string sessionId, CancellationToken cancellationToken = default);
}
