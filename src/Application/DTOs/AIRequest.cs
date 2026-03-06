namespace Application.DTOs;

public record AIRequest(string Question);

public record AIResponse(string Answer, bool Success, string? Error = null);
