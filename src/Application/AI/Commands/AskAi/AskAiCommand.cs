using Application.Common.Errors;
using Application.Common.Results;
using Application.DTOs;
using Application.Security;
using FluentValidation;
using MediatR;

namespace Application.AI.Commands.AskAi;

public sealed record AskAiCommand(string Question) : IRequest<Result<AIResponse>>;

public sealed class AskAiCommandValidator : AbstractValidator<AskAiCommand>
{
    public AskAiCommandValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question cannot be empty")
            .MaximumLength(4000).WithMessage("Question is too long");
    }
}

public sealed class AskAiCommandHandler : IRequestHandler<AskAiCommand, Result<AIResponse>>
{
    private readonly IAgentService _agentService;
    private readonly IPromptSanitizer _promptSanitizer;

    public AskAiCommandHandler(IAgentService agentService, IPromptSanitizer promptSanitizer)
    {
        _agentService = agentService;
        _promptSanitizer = promptSanitizer;
    }

    public async Task<Result<AIResponse>> Handle(AskAiCommand request, CancellationToken cancellationToken)
    {
        if (_promptSanitizer.IsMalicious(request.Question))
        {
            return Result<AIResponse>.Failure(
                Error.Validation("Prompt.Malicious", "Prompt rejected due to potential prompt injection attempt."));
        }

        var sanitized = _promptSanitizer.Sanitize(request.Question);

        var result = await _agentService.AskAsync(new AIRequest(sanitized), cancellationToken);

        if (result.IsFailure)
        {
            return Result<AIResponse>.Failure(
                result.Error ?? Error.Failure("AI.UnknownError", "Unknown AI error"));
        }

        return result;
    }
}

