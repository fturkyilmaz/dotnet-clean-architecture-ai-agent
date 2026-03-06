namespace Application.Common.Configuration;

public sealed class AIOptions
{
    public const string SectionName = "AI";
    
    public string Provider { get; set; } = "OpenAI";
    public string ModelId { get; set; } = "gpt-4o";
    public string? ApiKey { get; set; }
    public string? OrgId { get; set; }
    public string? Endpoint { get; set; }
    
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.7;
    public double TopP { get; set; } = 1.0;
    
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 1000;
    
    public bool EnableMemory { get; set; } = true;
    public int MaxConversationHistory { get; set; } = 10;
}

public sealed class RedisOptions
{
    public const string SectionName = "Redis";
    
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public int Database { get; set; } = 0;
    public int DefaultExpirationMinutes { get; set; } = 60;
}

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string? Username { get; set; } = "guest";
    public string? Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    
    public string ExchangeName { get; set; } = "ai-events";
    public string QueueName { get; set; } = "ai-requests";
}

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string Provider { get; set; } = "PostgreSQL";
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}
