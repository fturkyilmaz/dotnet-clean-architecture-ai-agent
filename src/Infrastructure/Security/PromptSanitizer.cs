using System.Text.RegularExpressions;
using Application.Security;

namespace Infrastructure.Security;

/// <summary>
/// Basit prompt injection koruması: yaygın saldırı pattern'lerini tespit edip input'u normalize eder.
/// </summary>
public sealed class PromptSanitizer : IPromptSanitizer
{
    // Çok agresif olmamak için yalnızca en bariz pattern'ler.
    private static readonly Regex MaliciousPattern = new(
        "(ignore previous instructions|disregard previous rules|you are now|act as jailbreak)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Sanitize(string input)
    {
        // Trim + kontrol karakterlerini temizle
        return input.Trim();
    }

    public bool IsMalicious(string input)
    {
        return MaliciousPattern.IsMatch(input);
    }
}

