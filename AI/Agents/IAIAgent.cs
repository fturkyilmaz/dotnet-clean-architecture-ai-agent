namespace AI.Agents;

public interface IAIAgent
{
    Task<string> AskAsync(string question, CancellationToken cancellationToken = default);
}
