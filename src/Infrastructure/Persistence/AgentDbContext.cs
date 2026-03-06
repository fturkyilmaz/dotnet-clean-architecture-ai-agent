using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Pgvector.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AgentDbContext : DbContext
{
    public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options)
    {
    }

    public DbSet<AgentLog> AgentLogs => Set<AgentLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AiMemoryEntry> AiMemoryEntries => Set<AiMemoryEntry>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // pgvector eklentisini etkinleştir.
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<AgentLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<AiMemoryEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Embedding)
                .HasColumnType("vector")
                .HasVectorDimensions(1536);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ProcessedAt);
            entity.HasIndex(e => e.RetryCount);
        });
    }

    public async Task<int> SaveChangesWithAuditAsync(string? userId = null, string? ipAddress = null)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            if (entry.Entity is AgentLog)
            {
                var auditLog = new AuditLog
                {
                    EntityType = nameof(AgentLog),
                    EntityId = ((AgentLog)entry.Entity).Id,
                    Action = entry.State.ToString(),
                    UserId = userId,
                    IpAddress = ipAddress,
                    NewValues = entry.State == EntityState.Added ? Serialize(entry.CurrentValues) : null,
                    OldValues = entry.State == EntityState.Deleted ? Serialize(entry.OriginalValues) : null
                };
                AuditLogs.Add(auditLog);
            }
        }

        return await base.SaveChangesAsync();
    }

    private static string Serialize(PropertyValues values)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var property in values.Properties)
        {
            dict[property.Name] = values[property.Name];
        }
        return System.Text.Json.JsonSerializer.Serialize(dict);
    }
}
