using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;

namespace AI.Agents;

public class SemanticKernelAgent : IAIAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<SemanticKernelAgent> _logger;

    public SemanticKernelAgent(Kernel kernel, ILogger<SemanticKernelAgent> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<string> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing question: {Question}", question);

            var systemPrompt = @"You are a helpful AI assistant. Answer the user's question clearly and concisely.";

            var result = await _kernel.InvokePromptAsync(
                systemPrompt + "\n\nUser: " + question,
                cancellationToken: cancellationToken
            );

            var answer = result.ToString();
            _logger.LogInformation("Generated answer: {Answer}", answer);

            return answer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question: {Question}", question);
            return $"Error: {ex.Message}";
        }
    }
}
