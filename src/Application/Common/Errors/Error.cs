namespace Application.Common.Errors;

public sealed class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public string? Details { get; }

    private Error(string code, string message, ErrorType type, string? details = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Details = details;
    }

    public static Error Failure(string code, string message, string? details = null)
        => new(code, message, ErrorType.Failure, details);

    public static Error Validation(string code, string message, string? details = null)
        => new(code, message, ErrorType.Validation, details);

    public static Error NotFound(string code, string message, string? details = null)
        => new(code, message, ErrorType.NotFound, details);

    public static Error Unauthorized(string code, string message, string? details = null)
        => new(code, message, ErrorType.Unauthorized, details);

    public static Error Conflict(string code, string message, string? details = null)
        => new(code, message, ErrorType.Conflict, details);
}

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Unauthorized,
    Conflict
}
