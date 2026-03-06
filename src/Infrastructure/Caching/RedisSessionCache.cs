using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Caching;

public interface ISessionCache
{
    Task<T?> GetAsync<T>(string sessionId);
    Task SetAsync<T>(string sessionId, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string sessionId);
    Task<bool> ExistsAsync(string sessionId);
}

public class RedisSessionCache : ISessionCache
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisSessionCache> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public RedisSessionCache(IConnectionMultiplexer redis, ILogger<RedisSessionCache> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    private static string GetKey(string sessionId) => $"session:{sessionId}";

    public async Task<T?> GetAsync<T>(string sessionId)
    {
        try
        {
            var key = GetKey(sessionId);
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Session {SessionId} not found", sessionId);
                return default;
            }

            _logger.LogDebug("Retrieved session {SessionId}", sessionId);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            return default;
        }
    }

    public async Task SetAsync<T>(string sessionId, T value, TimeSpan? expiration = null)
    {
        try
        {
            var key = GetKey(sessionId);
            var json = JsonSerializer.Serialize(value);
            var expiry = expiration ?? _defaultExpiration;

            await _database.StringSetAsync(key, json, expiry);
            _logger.LogDebug("Session {SessionId} stored with expiry {Expiry}", sessionId, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing session {SessionId}", sessionId);
        }
    }

    public async Task RemoveAsync(string sessionId)
    {
        try
        {
            var key = GetKey(sessionId);
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Session {SessionId} removed", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing session {SessionId}", sessionId);
        }
    }

    public async Task<bool> ExistsAsync(string sessionId)
    {
        try
        {
            var key = GetKey(sessionId);
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking session {SessionId}", sessionId);
            return false;
        }
    }
}

public record ChatMessage(string Role, string Content, DateTime Timestamp);

public interface IConversationHistory
{
    Task AddMessage(string sessionId, ChatMessage message);
    Task<List<ChatMessage>> GetHistory(string sessionId, int maxMessages = 10);
    Task ClearHistory(string sessionId);
}

public class RedisConversationHistory : IConversationHistory
{
    private readonly ISessionCache _cache;
    private readonly ILogger<RedisConversationHistory> _logger;

    public RedisConversationHistory(ISessionCache cache, ILogger<RedisConversationHistory> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task AddMessage(string sessionId, ChatMessage message)
    {
        var history = await GetHistory(sessionId);
        history.Add(message);
        
        if (history.Count > 20)
        {
            history = history.TakeLast(20).ToList();
        }

        await _cache.SetAsync(sessionId, history, TimeSpan.FromMinutes(30));
    }

    public async Task<List<ChatMessage>> GetHistory(string sessionId, int maxMessages = 10)
    {
        var history = await _cache.GetAsync<List<ChatMessage>>(sessionId);
        return history?.TakeLast(maxMessages).ToList() ?? new List<ChatMessage>();
    }

    public async Task ClearHistory(string sessionId)
    {
        await _cache.RemoveAsync(sessionId);
    }
}
