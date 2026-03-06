using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace Infrastructure.Persistence.Entities;

[Table("ai_memory_entries")]
public sealed class AiMemoryEntry
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Kullanıcıya veya tenant'a göre hafızayı segmentlemek için.
    /// </summary>
    [Column("user_id")]
    [MaxLength(256)]
    public string? UserId { get; set; }

    /// <summary>
    /// İsteğe bağlı oturum / conversation id'si.
    /// </summary>
    [Column("session_id")]
    [MaxLength(256)]
    public string? SessionId { get; set; }

    /// <summary>
    /// "user", "assistant" vb. roller.
    /// </summary>
    [Column("role")]
    [MaxLength(32)]
    public string Role { get; set; } = "user";

    /// <summary>
    /// Ham metin içerik (soru, cevap, not vs.).
    /// </summary>
    [Column("content")]
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// pgvector kolonu; embedding boyutunu migration tarafında belirleyeceğiz (örn. 1536).
    /// </summary>
    [Column("embedding", TypeName = "vector")]
    public Vector Embedding { get; set; } = Vector.Empty;

    [Column("model")]
    [MaxLength(128)]
    public string? Model { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

