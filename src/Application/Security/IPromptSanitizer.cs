namespace Application.Security;

public interface IPromptSanitizer
{
    /// <summary>
    /// Gerekli temizlikleri uygulayıp güvenli prompt döndürür.
    /// </summary>
    string Sanitize(string input);

    /// <summary>
    /// Bilinen prompt injection kalıplarına göre input'un riskli olup olmadığını döndürür.
    /// </summary>
    bool IsMalicious(string input);
}

