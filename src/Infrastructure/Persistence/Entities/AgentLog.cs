using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities;

[Table("ai_agent_logs")]
public sealed class AgentLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("session_id")]
    [MaxLength(256)]
    public string? SessionId { get; set; }

    [Column("user_question")]
    [Required]
    public string UserQuestion { get; set; } = string.Empty;

    [Column("ai_response")]
    [Required]
    public string AiResponse { get; set; } = string.Empty;

    [Column("model_used")]
    [MaxLength(128)]
    public string? ModelUsed { get; set; }

    [Column("tokens_used")]
    public int? TokensUsed { get; set; }

    [Column("response_time_ms")]
    public long? ResponseTimeMs { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("user_id")]
    [MaxLength(256)]
    public string? UserId { get; set; }

    [Column("metadata")]
    public string? Metadata { get; set; }
}

[Table("audit_logs")]
public sealed class AuditLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("entity_type")]
    [Required]
    [MaxLength(128)]
    public string EntityType { get; set; } = string.Empty;

    [Column("entity_id")]
    [Required]
    public Guid EntityId { get; set; }

    [Column("action")]
    [Required]
    [MaxLength(32)]
    public string Action { get; set; } = string.Empty;

    [Column("user_id")]
    [MaxLength(256)]
    public string? UserId { get; set; }

    [Column("old_values", TypeName = "jsonb")]
    public string? OldValues { get; set; }

    [Column("new_values", TypeName = "jsonb")]
    public string? NewValues { get; set; }

    [Column("ip_address")]
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [MaxLength(512)]
    public string? UserAgent { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
