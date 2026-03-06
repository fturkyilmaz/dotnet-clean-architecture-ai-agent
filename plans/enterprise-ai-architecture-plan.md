# Enterprise AI-Native Clean Architecture Plan

## Project Overview

This document outlines a comprehensive plan to transform the existing Clean Architecture project into an Enterprise AI-Native application. The plan is built upon Jason Taylor's CleanArchitecture template and enhanced with modern AI capabilities, distributed systems patterns, and enterprise-grade infrastructure.

---

## Executive Summary

The primary objective of this initiative is to build a scalable, maintainable, and enterprise-ready AI agent platform. By leveraging Clean Architecture principles combined with AI-native patterns, we will create a system that can handle complex AI workflows while maintaining separation of concerns and testability.

### Key Strategic Goals

1. **AI-Native Domain Modeling**: Design domain entities and events specifically tailored for AI workloads
2. **CQRS Implementation**: Implement Command Query Responsibility Segregation for optimized read/write operations
3. **Event-Driven Architecture**: Utilize MassTransit with RabbitMQ for robust message handling and the Outbox Pattern
4. **Advanced Logging**: Integrate Serilog with ELK stack and OpenTelemetry for comprehensive observability
5. **Real-Time Communication**: Add SignalR for live updates and interactions
6. **API Security**: Implement OAuth2/JWT authentication with Swagger/OpenAPI documentation
7. **Containerization**: Provide Docker Compose and Kubernetes manifests for deployment flexibility

---

## Current Project State

### Existing Architecture

```
src/
├── Domain/              # Domain entities and interfaces
├── Application/         # CQRS handlers, MediatR, DTOs
├── Infrastructure/      # AI services, caching, messaging, persistence
└── Presentation/        # API endpoints and UI (Next.js)
```

### Current Technologies

- **Domain Layer**: Core business entities
- **Application Layer**: MediatR for CQRS, FluentValidation
- **Infrastructure Layer**:
  - Semantic Kernel for AI agent capabilities
  - MassTransit + RabbitMQ for messaging
  - Redis for caching
  - Entity Framework Core for persistence
- **Presentation Layer**: ASP.NET Core Web API + Next.js 14 frontend

---

## Detailed Implementation Plan

### 1. Domain Layer: AI-Native Extensions

> **⚠️ REVISION NEEDED**: See Gap Analysis Section - Domain split into minimal (Phase 1) vs full (Phase 2)

**Objective**: Extend the Domain layer with AI-specific entities, events, and value objects that capture the essence of AI agent workflows.

> **Labels**: 
> - ✅ **Must have (Phase 1)**: Minimal domain with existing AgentLog, AiMemoryEntry
> - 🔄 **Nice to have (Phase 2)**: Full Agent/Conversation model
> - 🔮 **Future Ideas**: Complete repository interfaces, domain services

#### 1.1 AI-Native Entities

Create new domain entities to represent AI-related concepts:

| Entity | Purpose |
|--------|---------|
| `Agent` | Represents an AI agent with configuration, capabilities, and state |
| `Conversation` | Tracks multi-turn conversations between users and agents |
| `MemoryEntry` | Stores persistent memory for AI agents |
| `PromptTemplate` | Manages reusable prompt templates |
| `AIAgentExecution` | Tracks individual AI agent executions |

#### 1.2 Domain Events

Define domain events for AI workflows:

| Event | Description |
|-------|-------------|
| `AgentCreatedEvent` | Fired when a new AI agent is registered |
| `AgentExecutionStartedEvent` | Triggered when an agent begins processing |
| `AgentExecutionCompletedEvent` | Fired upon successful execution completion |
| `MemoryStoredEvent` | Raised when new memory is persisted |
| `ConversationStartedEvent` | Triggered at the beginning of a conversation |

#### 1.3 Value Objects

Implement immutable value objects:

| Value Object | Description |
|--------------|-------------|
| `AgentConfiguration` | Encapsulates agent settings (model, temperature, max tokens) |
| `Prompt` | Immutable prompt content with metadata |
| `TokenCount` | Represents token usage metrics |
| `ConfidenceScore` | Captures AI response confidence levels |

#### 1.4 Domain Services

| Service | Responsibility |
|---------|----------------|
| `IAgentDomainService` | Business logic for agent management |
| `IMemoryDomainService` | Memory aggregation and retrieval logic |

#### 1.5 Repository Interfaces

Add new repository interfaces:

```csharp
public interface IAgentRepository : IRepository<Agent>
{
    Task<Agent?> GetByIdWithIncludesAsync(Guid id);
    Task<IEnumerable<Agent>> GetActiveAgentsAsync();
}

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetWithMessagesAsync(Guid id);
    Task<IEnumerable<Conversation>> GetRecentConversationsAsync(Guid userId, int count);
}
```

---

### 2. Application Layer: CQRS Handlers

**Objective**: Implement comprehensive CQRS handlers for all AI-related operations.

#### 2.1 Commands

| Command | Handler | Description |
|---------|---------|-------------|
| `CreateAgentCommand` | `CreateAgentCommandHandler` | Creates a new AI agent |
| `UpdateAgentCommand` | `UpdateAgentCommandHandler` | Updates agent configuration |
| `DeleteAgentCommand` | `DeleteAgentCommandHandler` | Removes an agent |
| `ExecuteAgentCommand` | `ExecuteAgentCommandHandler` | Triggers agent execution |
| `StoreMemoryCommand` | `StoreMemoryCommandHandler` | Persists agent memory |
| `StartConversationCommand` | `StartConversationCommandHandler` | Initiates new conversation |

#### 2.2 Queries

| Query | Handler | Description |
|-------|---------|-------------|
| `GetAgentByIdQuery` | `GetAgentByIdQueryHandler` | Retrieves agent details |
| `GetAllAgentsQuery` | `GetAllAgentsQueryHandler` | Lists all agents |
| `GetConversationQuery` | `GetConversationQueryHandler` | Gets conversation with messages |
| `GetMemoriesQuery` | `GetMemoriesQueryHandler` | Retrieves agent memories |
| `SearchMemoriesQuery` | `SearchMemoriesQueryHandler` | Vector search in memory |

#### 2.3 Request/Response DTOs

Create comprehensive DTOs:

```csharp
public record AgentDto(
    Guid Id,
    string Name,
    string Description,
    AgentConfiguration Configuration,
    AgentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ConversationDto(
    Guid Id,
    Guid AgentId,
    Guid UserId,
    List<MessageDto> Messages,
    DateTime StartedAt,
    DateTime? EndedAt
);

public record ExecutionResultDto(
    Guid ExecutionId,
    string Response,
    TokenCount TokensUsed,
    ConfidenceScore Confidence,
    TimeSpan Duration
);
```

#### 2.4 Interface Definitions

Add new application interfaces:

| Interface | Description |
|-----------|-------------|
| `IAgentService` | Orchestrates agent operations |
| `IMemoryService` | Manages AI memory storage and retrieval |
| `IPromptTemplateService` | Handles prompt template management |
| `IConversationService` | Manages conversation lifecycle |

---

### 3. Infrastructure: MassTransit + RabbitMQ + Outbox Pattern

> **⚠️ REVISION NEEDED**: Match with existing message contracts - see Gap Analysis

**Objective**: Implement robust event-driven messaging with guaranteed delivery.

> **Labels**:
> - ✅ **Done**: OutboxMessage, MassTransitAiRequestBus, OutboxProcessor, AIRequestMessage, AIResponseMessage, AiRequestConsumer
> - 🔄 **Nice to have**: Event-driven HTTP API (accepted endpoint with CorrelationId)
> - 🔮 **Future Ideas**: Separate consumer types (AgentExecutionConsumer, MemoryStorageConsumer)

#### 3.1 MassTransit Configuration

Configure MassTransit with RabbitMQ:

```csharp
public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], configuration["RabbitMQ:VirtualHost"], h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]);
                    h.Password(configuration["RabbitMQ:Password"]);
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
}
```

#### 3.2 Outbox Pattern Implementation

Implement the Outbox Pattern for reliable messaging:

| Component | Description |
|-----------|-------------|
| `OutboxMessage` Entity | Stores messages awaiting delivery |
| `OutboxProcessor` Background Service | Processes and publishes outbox messages |
| `IOutboxRepository` | Repository for outbox operations |

#### 3.3 Message Contracts

Define message contracts:

```csharp
public record AgentExecutionRequested(
    Guid CorrelationId,
    Guid AgentId,
    string UserId,
    string Input,
    DateTime Timestamp
);

public record AgentExecutionCompleted(
    Guid CorrelationId,
    string Response,
    TokenCount TokensUsed,
    DateTime Timestamp
);

public record MemoryStored(
    Guid CorrelationId,
    Guid AgentId,
    string Content,
    MemoryType Type,
    DateTime Timestamp
);
```

#### 3.4 Consumer Definitions

Implement consumers:

| Consumer | Message Type | Description |
|----------|--------------|-------------|
| `AgentExecutionConsumer` | `AgentExecutionRequested` | Processes agent execution requests |
| `MemoryStorageConsumer` | `MemoryStored` | Handles memory persistence |
| `NotificationConsumer` | Domain Events | Sends notifications to subscribers |

---

### 4. Infrastructure: Semantic Kernel Prompt Templates

> **⚠️ REVISION NEEDED**: Mark as Optional Advanced - see Gap Analysis

**Objective**: Leverage Semantic Kernel for advanced prompt engineering and function calling.

> **Labels**:
> - ✅ **Done**: Basic SemanticKernelAgent with `InvokePromptAsync`
> - 🔄 **Nice to have**: Specific use cases (ConversationSummary, MemoryRecall templates)
> - 🔮 **Optional Advanced**: Full PromptTemplateStore, PromptTemplateEngine, SK plugins (mark as optional - enter only after memory + retrieval + resilience are complete)

#### 4.1 Prompt Template Management

| Component | Description |
|-----------|-------------|
| `PromptTemplateStore` | File-based and database storage for templates |
| `PromptTemplateEngine` | Renders templates with variable substitution |
| `IPromptTemplateRepository` | Repository for template CRUD operations |

#### 4.2 Built-in Templates

| Template | Use Case |
|----------|----------|
| `SystemPrompt` | Default system instructions for agents |
| `ConversationSummary` | Summarizes conversation history |
| `MemoryRecall` | Retrieves relevant memories |
| `FunctionCalling` | Defines function calling behavior |

#### 4.3 Semantic Kernel Integration

```csharp
public interface IPromptTemplateService
{
    Task<string> RenderAsync(string templateName, Dictionary<string, object> variables);
    Task<KernelFunction> CreateKernelFunctionAsync(PromptTemplate template);
    Task<IEnumerable<PromptTemplate>> GetAllTemplatesAsync();
}
```

#### 4.4 Function Plugins

Implement Semantic Kernel plugins:

| Plugin | Functions |
|--------|-----------|
| `MemoryPlugin` | `Recall`, `Store`, `Search` |
| `ConversationPlugin` | `GetHistory`, `Summarize` |
| `ToolPlugin` | `ExecuteTool`, `ListTools` |

---

### 5. Infrastructure: Serilog + ELK + OpenTelemetry

> **⚠️ REVISION NEEDED**: Already partially implemented - see Gap Analysis

**Objective**: Implement comprehensive observability with distributed tracing and centralized logging.

> **Labels**:
> - ✅ **Done**: Serilog + Elasticsearch sink, TraceId enrichment, OpenTelemetry (AspNetCore/HttpClient/Runtime), OTLP export
> - 🔄 **Nice to have**: Prometheus/Grafana dashboards, health check endpoints
> - 🔮 **Future Ideas**: Custom enrichers (AgentEnricher, ConversationEnricher)

> **Note**: Serilog configuration code examples are already implemented inline in `Program.cs` + `appsettings.json`. Can be extracted to `LoggingConfiguration` class in future.

#### 5.1 Serilog Configuration

Configure structured logging:

```csharp
public static class LoggingConfiguration
{
    public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["Elasticsearch:Uri"]))
            {
                AutoRegisterTemplate = true,
                IndexFormat = "ai-agent-logs-{0:yyyy.MM}"
            })
            .CreateLogger();
            
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
        
        return services;
    }
}
```

#### 5.2 Log Sinks

| Sink | Purpose |
|------|---------|
| Console | Development debugging |
| Elasticsearch | Log aggregation and search |
| File | Local file backup |
| Seq | Structured log viewer (optional) |

#### 5.3 OpenTelemetry Integration

Configure distributed tracing:

| Component | Configuration |
|-----------|---------------|
| ActivitySource | Initialize for each service |
| TracerProvider | Configure with exporters |
| Metrics | Add custom metrics for AI operations |
| Exporters | OTLP, Prometheus, Jaeger |

#### 5.4 Custom Enrichers

| Enricher | Adds |
|----------|------|
| `AgentEnricher` | Agent ID, Name, Type |
| `ConversationEnricher` | Conversation ID, Turn Count |
| `ExecutionEnricher` | Execution ID, Duration, Tokens |

---

### 6. Infrastructure: Redis Stack

**Objective**: Implement high-performance caching and session management with Redis Stack.

#### 6.1 Redis Stack Components

| Component | Use Case |
|-----------|----------|
| Redis Cache | General-purpose caching |
| Redis Stack (RediSearch) | Vector similarity search |
| Redis Stack (JSON) | JSON document storage |
| Redis Stack (Bloom Filters) | Deduplication |

#### 6.2 Cache Implementation

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}
```

#### 6.3 Session Management

| Feature | Implementation |
|---------|----------------|
| User Sessions | Store in Redis with sliding expiration |
| Conversation State | JSON storage in Redis Stack |
| Agent State | In-memory with Redis backup |

#### 6.4 Vector Memory Store

Implement semantic memory using Redis Stack:

```csharp
public interface IVectorMemoryStore
{
    Task<Guid> StoreAsync(VectorMemoryEntry entry);
    Task<IEnumerable<VectorMemoryEntry>> SearchAsync(ReadOnlySpan<float> queryVector, int topK);
    Task DeleteAsync(Guid id);
}
```

---

### 7. Presentation: SignalR Hub

**Objective**: Enable real-time bidirectional communication for AI interactions.

#### 7.1 SignalR Hubs

| Hub | Methods | Description |
|-----|---------|-------------|
| `AgentHub` | `ExecuteAgent`, `SubscribeToAgent` | Real-time agent execution |
| `ConversationHub` | `SendMessage`, `ReceiveMessage` | Live conversation updates |
| `NotificationHub` | `Subscribe`, `PushNotification` | System notifications |

#### 7.2 Hub Implementation

```csharp
public class AgentHub : Hub
{
    public async Task ExecuteAgent(ExecuteAgentRequest request)
    {
        var execution = await _agentService.ExecuteAsync(request);
        
        // Stream results back to client
        await Clients.Caller.SendAsync("ExecutionStarted", execution.Id);
        
        await foreach (var chunk in execution.StreamResults())
        {
            await Clients.Caller.SendAsync("ExecutionChunk", chunk);
        }
        
        await Clients.Caller.SendAsync("ExecutionCompleted", execution.Result);
    }
    
    public async Task SubscribeToAgent(Guid agentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"agent-{agentId}");
    }
}
```

#### 7.3 Authentication

Secure SignalR hubs with JWT:

```csharp
services.AddSignalR()
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
```

---

### 8. Presentation: Swagger/OpenAPI + OAuth2/JWT

**Objective**: Provide secure API documentation with OAuth2 authentication.

#### 8.1 Swagger Configuration

```csharp
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AI Agent API",
                Version = "v1",
                Description = "Enterprise AI-Native Clean Architecture API"
            });
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        return services;
    }
}
```

#### 8.2 Authentication Configuration

| Component | Configuration |
|-----------|---------------|
| JWT Bearer | Token validation, issuer, audience |
| OAuth2 | Authorization code flow |
| Policies | Role-based authorization |

#### 8.3 API Versioning

Configure API versioning:

```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
```

---

### 9. Docker Compose Configuration

**Objective**: Provide a complete local development environment.

#### 9.1 Services

| Service | Image | Ports | Purpose |
|---------|-------|-------|---------|
| `api` | Custom | 5000, 5001 | Main API |
| `ui` | Custom | 3000 | Next.js UI |
| `rabbitmq` | RabbitMQ | 5672, 15672 | Message broker |
| `elasticsearch` | Elasticsearch | 9200, 9300 | Log storage |
| `kibana` | Kibana | 5601 | Log visualization |
| `redis` | Redis Stack | 6379, 8001 | Cache + Vector store |
| `postgres` | Postgres | 5432 | Primary database |
| `jaeger` | Jaeger | 16686 | Distributed tracing |
| `seq` | Seq | 5341 | Log aggregator (optional) |

#### 9.2 Compose File Structure

```yaml
version: '3.9'

services:
  api:
    build:
      context: .
      dockerfile: src/Presentation/Api/Dockerfile
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=...
      - RabbitMQ__Host=rabbitmq
      - Redis__Host=redis
    depends_on:
      - postgres
      - rabbitmq
      - redis

  ui:
    build:
      context: ./src/Presentation/UI
      dockerfile: Dockerfile
    ports:
      - "3000:3000"
    environment:
      - API_URL=http://api:80

  # Infrastructure services
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data

  rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data

  redis:
    image: redis/redis-stack:latest
    ports:
      - "6379:6379"
      - "8001:8001"

  # Observability
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
```

---

### 10. Kubernetes Manifests

**Objective**: Provide production-ready Kubernetes deployments.

#### 10.1 Namespace

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: ai-agent
  labels:
    name: ai-agent
```

#### 10.2 Deployments

| Deployment | Replicas | Resources |
|------------|----------|-----------|
| `api` | 3 | CPU: 500m, Memory: 512Mi |
| `ui` | 2 | CPU: 250m, Memory: 256Mi |
| `worker` | 2 | CPU: 1000m, Memory: 1Gi |

#### 10.3 Services

| Service | Type | Ports |
|---------|------|-------|
| `api` | ClusterIP | 80, 443 |
| `ui` | ClusterIP | 3000 |
| `rabbitmq` | ClusterIP | 5672 |
| `elasticsearch` | ClusterIP | 9200 |
| `redis` | ClusterIP | 6379 |

#### 10.4 Ingress

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ai-agent-ingress
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
    - hosts:
        - api.ai-agent.example.com
        - ai-agent.example.com
      secretName: ai-agent-tls
  rules:
    - host: api.ai-agent.example.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: api
                port:
                  number: 80
    - host: ai-agent.example.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: ui
                port:
                  number: 3000
```

#### 10.5 ConfigMaps and Secrets

| Resource | Description |
|----------|-------------|
| `configmap` | Non-sensitive configuration |
| `secret` | API keys, passwords, tokens |
| `pvc` | Persistent volume claims |

#### 10.6 Horizontal Pod Autoscaling

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: api-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: api
  minReplicas: 3
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
```

---

## Implementation Phases

### Phase 1: Foundation (Week 1-2)
1. Set up Domain layer entities and interfaces
2. Implement Application layer CQRS handlers
3. Configure basic Infrastructure services

### Phase 2: Messaging & Events (Week 3)
1. Implement MassTransit with RabbitMQ
2. Build Outbox Pattern
3. Add domain event handlers

### Phase 3: AI Capabilities (Week 4)
1. Enhance Semantic Kernel integration
2. Implement Prompt Templates
3. Build Vector Memory Store with Redis Stack

### Phase 4: Observability (Week 5)
1. Configure Serilog with Elasticsearch
2. Add OpenTelemetry tracing
3. Implement custom enrichers

### Phase 5: Presentation (Week 6)
1. Add SignalR Hubs
2. Configure Swagger with OAuth2/JWT
3. Build UI components

### Phase 6: Deployment (Week 7)
1. Create Docker Compose
2. Build Kubernetes manifests
3. Configure CI/CD pipelines

---

## Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `MediatR` | 12.x | CQRS implementation |
| `MassTransit.RabbitMQ` | 8.x | Message broker |
| `Serilog.Sinks.Elasticsearch` | 10.x | Log shipping |
| `OpenTelemetry.*` | 1.x | Distributed tracing |
| `Microsoft.AspNetCore.SignalR` | 8.x | Real-time communication |
| `Swashbuckle.AspNetCore` | 6.x | API documentation |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.x | JWT authentication |
| `StackExchange.Redis` | 2.x | Redis client |
| `Microsoft.SemanticKernel` | 1.x | AI capabilities |

### Infrastructure Images

| Image | Version | Purpose |
|-------|---------|---------|
| `postgres` | 15-alpine | Primary database |
| `rabbitmq` | 3-management | Message broker |
| `elasticsearch` | 8.11.0 | Log storage |
| `redis/redis-stack` | latest | Cache + Vector store |
| `jaegertracing/all-in-one` | latest | Distributed tracing |

---

## Testing Strategy

### Unit Tests
- Domain entity behavior
- Command/Query handlers
- Value object validation

### Integration Tests
- API endpoint testing
- Message publishing/consuming
- Database operations

### End-to-End Tests
- Complete user workflows
- SignalR communication
- Docker Compose environment

---

## Security Considerations

### ✅ Core Security (Already Implemented)
1. **Authentication**: JWT Bearer tokens with short expiration
2. **Authorization**: Role-based access control (RBAC) - policies `RequireUser`, `RequireAdmin`
3. **Rate Limiting**: Prevent abuse of AI endpoints (`api` limiter)
4. **Input Validation**: Sanitize all user inputs (`IPromptSanitizer`)

### 🔄 Identity Provider Integration (To Do)
5. **OpenIddict / External IdP**: Token issuance, refresh tokens, user management
6. **Advanced Audit Logging**: Security events (authz failure, rate limit violations, critical AI actions)
7. **Enhanced Prompt Security**: Comprehensive pattern set, model-context aware rules

### 🔮 Future Ideas
8. **Encryption**: TLS for all external communication
9. **Secrets**: Kubernetes Secrets or HashiCorp Vault
10. **Audit Logging**: Track sensitive operations

---

## Monitoring & Alerts

### Key Metrics

| Metric | Target | Alert Threshold |
|--------|--------|-----------------|
| API Response Time | < 200ms | > 500ms |
| Agent Execution Time | < 5s | > 30s |
| Error Rate | < 0.1% | > 1% |
| Queue Depth | < 100 | > 500 |
| CPU Usage | < 70% | > 85% |
| Memory Usage | < 80% | > 90% |

### Health Checks

| Endpoint | Check |
|----------|-------|
| `/health` | Overall health |
| `/health/ready` | Dependencies ready |
| `/health/live` | Process alive |

---

## Remaining Work (Gap Analysis – 2026-03-06)

Bu bölüm, mevcut kod ile bu dokümandaki hedef mimari arasındaki **eksik veya kısmen uygulanmış** alanları özetler.

### 1. AI Memory & Retrieval

- **Durum (mevcut)**:
  - `AiMemoryEntry` entity’si ve `AgentDbContext` içinde pgvector konfigürasyonu hazır.
  - `IAgentService` ve `AiRequestConsumer` şu an `Vector.Empty` ile kayıt atıyor; gerçek embedding ve semantic search yok.
- **Eksikler**:
  - `IMemoryStore` için PostgreSQL + pgvector tabanlı bir implementasyon (ve opsiyonel Redis hybrid cache).
  - Embedding üretimi için Semantic Kernel / OpenAI Embeddings entegrasyonu.
  - `IAgentService` / consumer seviyesinde cevap üretmeden önce semantic retrieval ile context enrichment.

### 2. Event-Driven API Flow

- **Durum (mevcut)**:
  - Outbox pattern (`OutboxMessage`, `AgentDbContext`, `MassTransitAiRequestBus`, `OutboxProcessor`) uygulanmış durumda.
  - `AiRequestConsumer` MassTransit ile `AIRequestMessage` tüketip `AIResponseMessage` üretiyor.
- **Eksikler**:
  - HTTP API henüz tamamen event-driven değil; `/ai/ask` senkron olarak `IAgentService` üzerinden cevap dönüyor.
  - İhtiyaçlar:
    - `AskAiCommand`’in `IAiRequestBus.EnqueueAsync` kullanarak sadece `CorrelationId` dönen bir “accepted” endpoint’ine dönüştürülmesi.
    - `AIResponseMessage`’ları okuyup UI’nin poll/SignalR/long-polling ile erişebileceği bir read-model veya response store.

### 3. Resilience (Polly Pipelines)

- **Durum (mevcut)**:
  - Application ve Api projelerinde Polly referansı var.
- **Eksikler**:
  - Aşağıdaki bağımlılıklar için central policy registry ve named policy’ler:
    - AI / LLM çağrıları (retry + jitter, timeout, circuit breaker, hedging).
    - RabbitMQ (geçici broker hatalarına karşı retry).
    - PostgreSQL / Redis bağlantı hataları için geri kazanım stratejileri.
  - Bu policy’leri `IAgentService`, HTTP clients ve messaging client’ları etrafında uygulayan wrapper’lar.

### 4. Observability Derinliği

- **Durum (mevcut)**:
  - Serilog + Elasticsearch sink, request logging, TraceId log enrichment eklendi.
  - OpenTelemetry ile AspNetCore/HttpClient/Runtime tracing & metrics ve OTLP export yapılandırıldı.
- **Eksikler**:
  - Prometheus/Grafana tarafında:
    - OTLP’den Prometheus’a bridge veya doğrudan Prometheus Scraping ayarları.
    - Hazır dashboard’ların (API latency, queue depth, AI latency, error rate) oluşturulması.
  - Health check endpoint’leri (`/health`, `/health/ready`, `/health/live`) ve bunların Aspire/Grafana ile entegrasyonu.

### 5. Security & Governance Genişlemesi

- **Durum (mevcut)**:
  - JWT Bearer auth, policy-based authorization (`RequireUser`, `RequireAdmin`), rate limiting (`api` limiter) ve `IPromptSanitizer` ile temel prompt injection koruması devrede.
- **Eksikler**:
  - OpenIddict veya harici IdP ile tam entegrasyon (token issuance, refresh tokens, user management).
  - Audit logging’in güvenlik olaylarını (authz failure, rate limit ihlali, kritik AI aksiyonları) kapsayacak şekilde genişletilmesi.
  - Gelişmiş prompt güvenliği (daha kapsamlı pattern seti, model-context aware kurallar, configurable policy’ler).

### 6. Testing & DevOps

- **Durum (mevcut)**:
  - Temel CI/CD pipeline dosyası mevcut, fakat yeni bileşenlerin tamamını kapsamıyor.
- **Eksikler**:
  - TestContainers ile PostgreSQL / Redis / RabbitMQ + AI worker için entegrasyon test senaryoları.
  - CI’de:
    - Migration / health check / smoke test adımları,
    - Worker service’in de build/test/deploy zincirine dahil edilmesi.
  - Docker Compose / K8s manifestlerinin:
    - API + Worker + RabbitMQ + Redis + PostgreSQL + Elasticsearch + OTLP collector + (opsiyonel) Grafana/Prometheus için güncellenmesi.

---

### Phase 7: Enterprise Scale

**Objective**: Add advanced enterprise patterns for when the system scales beyond microservices to full enterprise-grade distributed architecture.

#### 7.1 API Gateway / BFF (Backend for Frontend)

Implement an API Gateway to handle cross-cutting concerns and provide optimized endpoints for different clients.

| Component | Technology | Purpose |
|-----------|------------|---------|
| API Gateway | Ocelot / YARP | Request routing, rate limiting, authentication |
| BFF for Web | Next.js API Routes | Frontend-specific aggregations |
| BFF for Mobile | Dedicated API | Mobile-optimized responses |

```csharp
// YARP Configuration Example
services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// routes:
//   - routeId: api-route
//     match:
//       path: /api/**
//     cluster: api-cluster
//     transforms:
//       - PathRemovePrefix: /api
```

#### 7.2 Feature Flags & Configuration Management

Implement controlled feature rollouts and dynamic configuration.

| Feature | Implementation | Use Case |
|---------|----------------|----------|
| Feature Flags | LaunchDarkly / Unleash | A/B testing, gradual rollouts |
| Configuration | Azure App Configuration | Runtime configuration changes |
| Secrets | HashiCorp Vault / Azure Key Vault | Secure secret management |

```csharp
// Feature Flag Example
public class FeatureFlags
{
    public const string NewAIEngine = "new-ai-engine";
    public const string VectorSearch = "vector-search";
    public const string AdvancedAnalytics = "advanced-analytics";
}

if (await _featureManager.IsEnabledAsync(FeatureFlags.NewAIEngine))
{
    // Use new AI engine
}
```

#### 7.3 Data Governance & Compliance

Ensure GDPR/KVKK compliance and data privacy.

| Component | Implementation |
|-----------|----------------|
| PII Masking | Custom Serilog enricher for sensitive data |
| Audit Logs | Extended audit trail with retention policies |
| Data Retention | Automated cleanup jobs |
| Consent Management | User consent tracking |

```csharp
// PII Masking Enricher
public class PiiMaskingEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.ContainsKey("Email"))
        {
            var email = logEvent.Properties["Email"].ToString();
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("Email", MaskEmail(email)));
        }
    }
    
    private string MaskEmail(string email)
    {
        var parts = email.Split('@');
        var name = parts[0];
        return $"{name[0]}***@{parts[1]}";
    }
}
```

#### 7.4 Advanced Testing

Implement comprehensive testing strategies for enterprise reliability.

| Testing Type | Tool | Description |
|-------------|------|-------------|
| Contract Testing | Pact | Consumer-driven contract testing between services |
| Chaos Testing | Polly + k6 | Resilience and fault tolerance testing |
| Load Testing | k6 / Locust | Performance under load |
| Security Testing | OWASP ZAP | Security vulnerability scanning |

```csharp
// Chaos Engineering Example with Polly v8
// Fixed: AddBulkhead replaced with AddConcurrencyLimiter in v8
var chaosPipeline = new ResiliencePipelineBuilder()
    .AddTimeout(TimeSpan.FromSeconds(30))
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential
    })
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromMinutes(1)
    })
    .AddConcurrencyLimiter(new ConcurrencyLimiterOptions
    {
        MaxParallelization = 10,
        QueueLimit = 100
    })
    .Build();
```

#### 7.5 Scalability Patterns

Implement advanced patterns for handling complex AI workflows.

##### CQRS + Event Sourcing

```csharp
// Event Store Interface
public interface IEventStore
{
    Task AppendAsync<T>(Guid aggregateId, T event, CancellationToken ct);
    Task<IEnumerable<T>> GetEventsAsync(Guid aggregateId, CancellationToken ct);
}

// AI Execution Event Sourcing
public record AIExecutionStartedEvent(Guid ExecutionId, string Input);
public record TokenConsumedEvent(Guid ExecutionId, int Tokens);
public record ExecutionCompletedEvent(Guid ExecutionId, string Response);
```

##### Saga Pattern

For orchestrating long-running AI workflows:

| Saga | Steps |
|------|-------|
| DocumentProcessingSaga | Upload → Parse → Embed → Index → Notify |
| AgentExecutionSaga | Validate → Execute → StoreMemory → UpdateUI → Log |

```csharp
// Saga Orchestrator
public class AgentExecutionSaga : Orchestrator
{
    public async Task<SagaResult> ExecuteAsync(ExecuteAgentRequest request)
    {
        return await DefineSaga()
            .Step(() => ValidateInputAsync(request))
            .Step(() => ExecuteAgentAsync(request))
            .Step(() => StoreMemoryAsync(request))
            .Step(() => NotifyUserAsync(request))
            .Step(() => LogExecutionAsync(request))
            .ExecuteAsync();
    }
}
```

#### 7.6 Monitoring & Alerting Enhancements

Advanced monitoring specific to AI operations.

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| Embedding Latency | Time to generate embeddings | > 2s |
| Token Usage per User | Daily token consumption | > 10K tokens |
| Model API Latency | LLM response time | > 30s |
| Vector Search Recall | Retrieval accuracy | < 80% |

```yaml
# Prometheus Alert Rules
groups:
- name: ai-metrics
  rules:
  - alert: HighEmbeddingLatency
    expr: histogram_quantile(0.95, rate(embedding_latency_seconds_bucket[5m])) > 2
    for: 5m
    labels:
      severity: warning
  
  - alert: TokenUsageExceeded
    # Fixed: Use increase() for total over time window, not rate()
    expr: sum by (user_id) (increase(token_usage_total[1h])) > 10000
    for: 10m
    labels:
      severity: critical
```

#### 7.7 Developer Experience (DevEx)

Improve developer productivity with tooling.

| Tool | Purpose |
|------|---------|
| Makefile | Single-command build/test/deploy |
| dotnet tools | Project-specific CLI tools |
| devcontainer | Consistent development environment |
| GitHub Codespaces | Cloud-based development |
| Hot Reload | Fast iteration during development |

```makefile
# Makefile Example
.PHONY: build test run docker-up docker-down

build:
	dotnet build src/Presentation/Api/Api.csproj

test:
	dotnet test tests/

run:
	dotnet run --project src/Presentation/Api/Api.csproj

docker-up:
	cd deploy && docker-compose up -d

docker-down:
	cd deploy && docker-compose down

dev:
	dotnet watch --project src/Presentation/Api/Api.csproj
```

```yaml
# .devcontainer/devcontainer.json
{
  "name": "AI Agent Dev",
  "image": "mcr.microsoft.com/dotnet/sdk:8.0",
  "features": {
    "docker-from-docker": "latest",
    "git": "latest"
  },
  "postCreateCommand": "dotnet restore && cd src/Presentation/UI && npm install"
}
```

### 7. SignalR Redis Backplane (P1 - Important)

For scaling SignalR across multiple pods:

```csharp
// SignalR with Redis Backplane (for K8s scaling)
services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.ConnectionFactory = async writer =>
        {
            var config = ConfigurationOptions.Parse(_redisConnectionString);
            config.AbortOnConnectFail = false;
            var connection = await ConnectionMultiplexer.ConnectAsync(config);
            return connection;
        };
        options.Configuration.ChannelPrefix = RedisChannel.Literal("ai-agent-signalr");
    });

// Alternative: Azure SignalR Service (simpler for cloud)
services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        options.ConnectionString = _azureSignalRConnectionString;
    });
```

### 8. Multi-Tenancy Implementation (P1 - Important)

**ITenantContext Service**:

```csharp
public interface ITenantContext
{
    string TenantId { get; }
    string? UserId { get; }
}

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string TenantId => 
        _httpContextAccessor.HttpContext?.User
            .FindFirst("tenant_id")?.Value 
        ?? throw new UnauthorizedAccessException("Tenant not found");
    
    public string? UserId => 
        _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
}

// Middleware for tenant resolution
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    
    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract from JWT, header, or subdomain
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault()
            ?? context.User.FindFirst("tenant_id")?.Value;
            
        if (string.IsNullOrEmpty(tenantId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Tenant ID required");
            return;
        }
        
        // Set tenant context in Items for later retrieval
        context.Items["TenantId"] = tenantId;
        
        await _next(context);
    }
}

// EF Core Query Filter - Fixed for runtime resolution
public static class TenantQueryFilters
{
    // Note: Query filters with runtime values require a workaround
    // Option 1: Use EF Core 7+ WithExpression syntax
    // Option 2: Manual filtering in repository
    
    public static void Apply<TTenantContext>(ModelBuilder modelBuilder)
        where TTenantContext : ITenantContext
    {
        // Apply to entities with TenantId - use null as placeholder
        // Runtime value will be injected via IAsyncLocal or scoped service
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tenantIdProperty = entityType.ClrType.GetProperty("TenantId");
            if (tenantIdProperty != null && tenantIdProperty.PropertyType == typeof(string))
            {
                // Use parameterless query filter - filter manually in repository
                var param = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Constant(true); // No filter by default
                var lambda = Expression.Lambda(body, param);
                
                entityType.SetQueryFilter(lambda);
            }
        }
    }
}

// In DbContext - manually apply tenant filter
public partial class AgentDbContext
{
    public override IQueryable<AgentLog> AgentLogs => 
        base.AgentLogs.Where(x => x.TenantId == TenantContext.TenantId);
        
    private ITenantContext TenantContext => 
        this.GetService<ITenantContext>();
}
```

### 9. Outbox Poison Message Handling (P1 - Important)

```csharp
public class OutboxProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await _context.OutboxMessages
                    .Where(m => m.ProcessedAt == null && m.RetryCount < 3)
                    .OrderBy(m => m.CreatedAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);
                
                foreach (var message in messages)
                {
                    try
                    {
                        await ProcessMessageAsync(message, stoppingToken);
                        message.ProcessedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        message.RetryCount++;
                        message.LastError = ex.Message;
                        
                        if (message.RetryCount >= 3)
                        {
                            // Move to dead letter
                            await MoveToDeadLetterAsync(message, ex);
                        }
                    }
                }
                
                await _context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processor error");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
    
    private async Task MoveToDeadLetterAsync(OutboxMessage message, Exception ex)
    {
        // Log to alerting system
        _logger.LogCritical(
            "Poison message moved to DLQ: {MessageId}, {Error}", 
            message.Id, 
            ex.Message);
        
        // Could also create a DeadLetterMessage entity
    }
}
```

---

## ✅ Previously Added (v1.4 - v1.5)

> These were already added in previous versions:
- ✅ Multi-tenancy: ITenantContext, Query Filters, Redis Key Isolation
- ✅ Embedding versioning: IEmbeddingMigrationService
- ✅ Idempotency: IdempotencyMiddleware with distributed lock
- ✅ Cost governance: CostGovernanceService with atomic Redis operations
- ✅ Async API: SignalR vs SSE comparison
- ✅ Saga: MassTransit State Machine implementation
- ✅ Semantic caching: SemanticCacheService with RediSearch
- ✅ Context window: ContextWindowManager with summarization
- ✅ Docker healthchecks: condition: service_healthy
- ✅ .NET 8: LTS version
- ✅ PodDisruptionBudget

### 10. Database Migration Strategy (P2)

```csharp
// Program.cs - Ensure migrations run at startup
public static IHost MigrateDatabase(this IHost host)
{
    using var scope = host.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
    
    // Apply pending migrations
    context.Database.Migrate();
    
    // Ensure pgvector extension (run once per database)
    context.Database.ExecuteSqlRaw(@"
        CREATE EXTENSION IF NOT EXISTS vector;
    ");
    
    return host;
}

// Zero-downtime migration strategy
public class MigrationStrategy
{
    // 1. Add new column as nullable
    // 2. Update application code to write both old and new
    // 3. Backfill new column data
    // 4. Make column non-nullable
    // 5. Remove old column (later release)
}

// K8s migration job
// apiVersion: batch/v1
kind: Job
metadata:
  name: db-migration
spec:
  template:
    spec:
      containers:
      - name: migrate
        image: your-api:latest
        command: ["dotnet", "ef", "database", "update"]
      restartPolicy: OnFailure
```

### 11. Frontend-Backend Contract (P2)

```csharp
// NSwag configuration for type generation
// Add to Api.csproj
<PackageReference Include="NSwag.MSBuild" Version="14.1.0" />

// In .csproj
<Target Name="Nswag" AfterTargets="Build">
  <CallTask Task="Nswag"/>
</Target>

// nswag.json
{
  "runtime": "Net80",
  "documentGenerator": {
    "fromDocument": {
      "url": "https://localhost:5000/swagger/v1/swagger.json",
      "output": "../UI/src/lib/api-types.ts"
    }
  },
  "codeGenerators": {
    "typescriptClient": {
      "typeScriptVersion": 5.0,
      "module": "ESNext",
      "output": "../UI/src/lib/api-types.ts"
    }
  }
}

// GitHub Actions - generate types on build
- name: Generate API Types
  run: dotnet run --project src/Presentation/Api --no-build
- name: Commit API Types
  uses: stefanzweifel/git-auto-commit-action@v4
  with:
    file_pattern: src/Presentation/UI/src/lib/api-types.ts
```

### 12. Secret Rotation Strategy (P2)

```csharp
// Azure Key Vault or HashiCorp Vault
public interface ISecretRotationService
{
    Task<string> GetSecretAsync(string secretName);
    Task RotateSecretAsync(string secretName);
}

// Key Vault with automatic rotation
public class KeyVaultSecretService : ISecretRotationService
{
    private readonly SecretClient _client;
    
    public KeyVaultSecretService(SecretClient client)
    {
        _client = client;
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _client.GetSecretAsync(secretName);
        return secret.Value.Value;
    }
    
    // Azure Key Vault handles rotation automatically
    // Just configure in Azure Portal:
    // - Enable automatic rotation
    // - Set rotation frequency (days)
    // - Update application to use Key Vault reference
}

// Environment variables for secrets
// ConnectionStrings__DefaultConnection="@Microsoft.KeyVault(SecretUri=...)"
```

### 13. Kubernetes Resource Limits (P2)

```yaml
# api-deployment.yaml - Updated with AI-appropriate resources
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: api
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "2Gi"
            cpu: "1000m"  # Increased for AI operations
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
---

### 14. HPA with Custom Metrics (P2)

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: api-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: api
  minReplicas: 3
  maxReplicas: 20
  metrics:
  # Custom metric: pending AI executions
  - type: Pods
    pods:
      metric:
        name: ai_executions_pending
      target:
        type: AverageValue
        averageValue: "10"
  # CPU fallback
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
      - type: Percent
        value: 100
        periodSeconds: 15
```

---

## 🚀 .NET 10 Modern Features

All code examples in this plan use modern .NET 10 features:

### 1. Primary Constructors

```csharp
// Records with primary constructors
public record AgentDto(
    Guid Id,
    string Name,
    string Description,
    AgentConfiguration Configuration);

// Classes with primary constructors
public class AgentService(
    IAIAgent aiAgent,
    AgentDbContext dbContext,
    ILogger<AgentService> logger) : IAgentService;
```

### 2. Collection Expressions

```csharp
// Modern collection syntax
var messages = [new Message("Hello"), new Message("World")];
var dict = ["key1" : "value1", "key2" : "value2"];
```

### 3. Pattern Matching Improvements

```csharp
// Extended pattern matching
public string Describe(object obj) => obj switch
{
    int i when i > 0 => $"Positive {i}",
    string { Length: > 10 } longString => $"Long: {longString}",
    null => "Null!",
    _ => "Unknown"
};

// List patterns
if (numbers is [1, 2, .., 5]) { }
```

### 4. System.Text.Json Source Generator

```csharp
[JsonSerializable(typeof(AIRequest))]
[JsonSerializable(typeof(AIResponse))]
public partial class AiJsonContext : JsonSerializerContext;
```

### 5. Regex Source Generator

```csharp
[RegexGenerator(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b")]
public static partial class EmailRegex
{
    [GeneratedRegex]
    public static partial Regex EmailPattern();
}
```

### 6. Native AOT

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <TrimmerRemoveSymbols>true</TrimmerRemoveSymbols>
</PropertyGroup>
```

### 7. UnsafeAccessor

```csharp
[UnsafeAccessor(UnsafeAccessorKind.Method, Target = "...")]
public static extern Kernel BuildKernel(Kernel kernel);
```

---

## ⚠️ Critical Architectural Decisions (Required Before Implementation)

> **IMPORTANT**: These decisions must be made BEFORE implementation begins as they fundamentally affect the entire architecture.

### 1. Multi-Tenancy Strategy (P0 - Critical)

| Option | Description | Use Case |
|--------|-------------|----------|
| **Single Tenant** | One database, one deployment | Internal tools, single organization |
| **Database-per-Tenant** | Separate DB per customer | Strong isolation required |
| **Schema-per-Tenant** | Same DB, separate schemas | Shared infrastructure, moderate isolation |
| **Row-Level Security (RLS)** | Same tables, filter by tenant_id | SaaS, shared resources |

**Recommendation**: Start with Single Tenant, design for RLS from the beginning.

```csharp
// EF Core Global Query Filter for Multi-Tenancy
modelBuilder.Entity<AgentLog>()
    .HasQueryFilter(x => x.TenantId == _currentTenantId);

// Redis Key Isolation
$"ai:{tenantId}:session:{sessionId}"
```

### 2. Embedding Model Selection & Versioning (P0 - Critical)

| Model | Dimensions | Use Case |
|-------|------------|----------|
| text-embedding-3-small | 1536 | Fast, cost-effective |
| text-embedding-3-large | 3072 | Higher accuracy |
| Azure OpenAI | 1536 | Enterprise compliance |

**Versioning Strategy**:

```csharp
public class EmbeddingConfig
{
    public string ModelName { get; set; } = "text-embedding-3-small";
    public int Dimensions { get; set; } = 1536;
    public string Version { get; set; } = "v1";
}

// Re-indexing strategy for model changes
public interface IEmbeddingMigrationService
{
    Task ReEmbedAllAsync(EmbeddingConfig newConfig);
    Task ValidateVectorDimensionsAsync();
}
```

### 3. Distributed Lock & Idempotency (P0 - Critical)

**Idempotency Key Pattern (Fixed)**:

```csharp
public class IdempotencyMiddleware
{
    public async Task<T> ExecuteAsync<T>(
        string idempotencyKey,
        Func<Task<T>> operation,  // Changed to async Func
        TimeSpan expiry,
        CancellationToken ct)
    {
        var lockKey = $"lock:{idempotencyKey}";
        var resultKey = $"result:{idempotencyKey}";
        
        // Try to acquire distributed lock
        var acquired = await _redis.SetAsync(lockKey, "1", 
            expiry, When.NotExists);
        
        if (!acquired)
        {
            // Check if already processed
            var existing = await _redis.GetAsync<T>(resultKey);
            if (existing != null) return existing;
            
            // Still processing, wait and check again
            await Task.Delay(100, ct);
            var retryResult = await _redis.GetAsync<T>(resultKey);
            if (retryResult != null) return retryResult;
            
            throw new IdempotencyConflictException(
                $"Request with key {idempotencyKey} is still processing");
        }
        
        try
        {
            var result = await operation();
            await _redis.SetAsync(resultKey, result, expiry);
            return result;
        }
        finally
        {
            // Only delete if we actually acquired the lock
            if (acquired)
            {
                await _redis.KeyDeleteAsync(lockKey);
            }
        }
    }
}

public class IdempotencyConflictException : Exception
{
    public IdempotencyConflictException(string message) : base(message) { }
}
```

### 4. AI Cost Governance (P0 - Critical)

> **Note**: These code examples are illustrative pseudocode - not production-ready

```csharp
public interface ICostGovernanceService
{
    Task<bool> CanExecuteAsync(string userId, string model, int estimatedTokens);
    Task RecordUsageAsync(string userId, string model, int tokens);
    Task<decimal> GetCurrentCostAsync(string userId);
}

public class CostGovernanceOptions
{
    public Dictionary<string, decimal> ModelPricingPer1K { get; set; } = new()
    {
        ["gpt-4o"] = 0.03m,  // per 1K input tokens
        ["gpt-4o-mini"] = 0.001m,
    };
    
    public int MonthlyBudgetPerUser { get; set; } = 100; // USD
    public string FallbackModel { get; set; } = "gpt-4o-mini";
}

// Cost Governance with atomic operations (Fixed race condition)
public class CostGovernanceService : ICostGovernanceService
{
    private readonly IDatabase _redis;
    private readonly CostGovernanceOptions _options;
    
    public CostGovernanceService(
        IConnectionMultiplexer redis,
        IOptions<CostGovernanceOptions> options)
    {
        _redis = redis.GetDatabase();
        _options = options.Value;
    }
    
    public async Task<bool> CanExecuteAsync(
        string userId, 
        string model, 
        int estimatedTokens)
    {
        var budgetKey = $"budget:{userId}:monthly";
        var usedKey = $"budget:{userId}:used";
        
        // Get current usage
        var used = await _redis.StringGetAsync(usedKey);
        var currentUsed = used.HasValue ? decimal.Parse(used!) : 0;
        
        // Calculate estimated cost
        var pricePer1K = _options.ModelPricingPer1K.GetValueOrDefault(model, 0.03m);
        var estimatedCost = (estimatedTokens / 1000m) * pricePer1K;
        
        // Check if within budget (atomic with Lua script)
        var remaining = _options.MonthlyBudgetPerUser - currentUsed;
        return remaining >= estimatedCost;
    }
    
    public async Task RecordUsageAsync(
        string userId, 
        string model, 
        int tokens)
    {
        var pricePer1K = _options.ModelPricingPer1K.GetValueOrDefault(model, 0.03m);
        var cost = (tokens / 1000m) * pricePer1K;
        
        // Atomic increment - prevents race condition
        var usedKey = $"budget:{userId}:used";
        var newValue = await _redis.StringIncrementAsync(
            usedKey, 
            (double)cost);
        
        // Set expiry at end of month
        var daysLeft = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month) 
            - DateTime.UtcNow.Day;
        await _redis.KeyExpireAsync(usedKey, TimeSpan.FromDays(daysLeft));
    }
    
    public async Task<decimal> GetCurrentCostAsync(string userId)
    {
        var used = await _redis.StringGetAsync($"budget:{userId}:used");
        return used.HasValue ? decimal.Parse(used!) : 0;
    }
}

// Alternative: Use Lua script for true atomic check-and-decrement
public class AtomicCostCheckLua
{
    // Lua script for atomic budget check
    private const string CheckAndDecrementScript = @"
        local remaining = tonumber(redis.call('GET', KEYS[1])) or 0
        local cost = tonumber(ARGV[1])
        if remaining >= cost then
            redis.call('DECRBYFLOAT', KEYS[1], cost)
            return 1
        end
        return 0
    ";
}
```

### 5. Async API Pattern Selection (P1 - Important)

| Pattern | Pros | Cons | Best For |
|---------|------|------|----------|
| **SignalR** | Real-time, bi-directional | Connection management | Chat, live updates |
| **Server-Sent Events (SSE)** | Simple, HTTP/1.1 compatible | One-way | Streaming AI responses |
| **Polling** | Simplest implementation | Latency, overhead | Low-frequency updates |
| **WebSocket** | Full-duplex | Complexity | High-frequency real-time |

**Recommendation**: Use SignalR for conversation, SSE for streaming AI responses.

### 6. Saga Orchestration (P1 - Important)

Use MassTransit Saga State Machine instead of custom Orchestrator:

```csharp
// Saga State
public class AgentExecutionState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public string? UserId { get; set; }
    public string? Input { get; set; }
    public string? Result { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// Saga State Machine (Fixed)
public class AgentExecutionSaga : MassTransitStateMachine<AgentExecutionState>
{
    public State Executing { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
    
    public Event<StartExecution> StartExecution { get; private set; } = null!;
    public Event<ExecutionCompleted> ExecutionCompleted { get; private set; } = null!;
    public Event<ExecutionFailed> ExecutionFailed { get; private set; } = null!;
    
    public AgentExecutionSaga()
    {
        InstanceState(x => x.CurrentState);
        
        // Use SetCompletedWhenFinalized to auto-delete after final state
        SetCompletedWhenFinalized();
        
        Initially(
            When(StartExecution)
                .Then(context => 
                {
                    // CorrelationId is automatically set from message - don't reassign
                    context.Saga.UserId = context.Message.UserId;
                    context.Saga.Input = context.Message.Input;
                    context.Saga.StartedAt = DateTime.UtcNow;
                })
                .TransitionTo(Executing)
                .Respond(x => new ExecutionStartedResponse(x.Saga.CorrelationId))
        );
        
        During(Executing,
            When(ExecutionCompleted)
                .Then(context => 
                {
                    context.Saga.Result = context.Message.Result;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Completed),
            When(ExecutionFailed)
                .TransitionTo(Failed)
        );
    }
}

// Message Contracts
public record StartExecution(Guid CorrelationId, string UserId, string Input);
public record ExecutionStartedResponse(Guid CorrelationId);
public record ExecutionCompleted(Guid CorrelationId, string Result);
public record ExecutionFailed(Guid CorrelationId, string Error);
```

### 7. Database Migration Strategy (P2)

```csharp
// Program.cs - Ensure pgvector extension
public static IHost MigrateDatabase(this IHost host)
{
    using var scope = host.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
    
    context.Database.Migrate();
    
    // Ensure pgvector extension (run once)
    context.Database.ExecuteSqlRaw(@"
        CREATE EXTENSION IF NOT EXISTS vector;
    ");
    
    return host;
}
```

### 8. Semantic Response Caching (P2)

> **Note**: These code examples are illustrative pseudocode - not production-ready

```csharp
public interface ISemanticCacheService
{
    Task<string?> GetSimilarResponseAsync(string question, double threshold = 0.95);
    Task SetAsync(string question, string response, ReadOnlySpan<float> embedding);
}

// Redis Stack RediSearch for semantic cache (Fixed)
public class SemanticCacheService : ISemanticCacheService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IDatabase _db;
    
    public SemanticCacheService(
        IEmbeddingService embeddingService,
        IConnectionMultiplexer redis)
    {
        _embeddingService = embeddingService;
        _db = redis.GetDatabase();
    }
    
    public async Task<string?> GetSimilarResponseAsync(
        string question, 
        double threshold = 0.95)
    {
        var questionEmbedding = await _embeddingService.EmbedAsync(question);
        var vectorBytes = VectorToBytes(questionEmbedding.ToArray());
        
        // KNN search with proper RediSearch 2.x syntax
        var results = await _db.ExecuteAsync(
            "FT.SEARCH", 
            "semantic-cache",
            $"*=>[KNN 5 @question_vector $vec AS distance]",
            "PARAMS", "vec", vectorBytes,
            "DIALECT", "2"
        );
        
        return null; // Placeholder
    }
    
    // Fixed: Use float[] instead of ReadOnlySpan<float> for async
    public async Task SetAsync(
        string question, 
        string response, 
        float[] embedding)
    {
        var vectorBytes = VectorToBytes(embedding);
        
        // RediSearch 2.x: Use HSET with schema
        var key = $"cache:{Guid.NewGuid():N}";
        await _db.HashSetAsync(key, new HashEntry[]
        {
            new("question", question),
            new("response", response),
            new("embedding", vectorBytes)
        });
        
        // Note: Index must exist - use FT.CREATE separately
        // FT.ADD is deprecated in RediSearch 2.x
    }
    
    private static byte[] VectorToBytes(float[] vector)
    {
        var bytes = new byte[vector.Length * 4];
        var span = bytes.AsSpan();
        for (int i = 0; i < vector.Length; i++)
        {
            BitConverter.TryWriteBytes(span.Slice(i * 4, 4), vector[i]);
        }
        return bytes;
    }
}

// Alternative: Use vector cache with exact match (simpler)
public class ExactMatchCacheService : ISemanticCacheService
{
    // Use MD5 hash of question as key - simpler but no semantic similarity
    public async Task<string?> GetSimilarResponseAsync(string question, double threshold)
    {
        var key = $"cache:exact:{GetHash(question)}";
        return await _db.StringGetAsync(key);
    }
    
    public Task SetAsync(string question, string response, ReadOnlySpan<float> embedding)
    {
        var key = $"cache:exact:{GetHash(question)}";
        return _db.StringSetAsync(key, response, TimeSpan.FromHours(24));
    }
    
    private static string GetHash(string input) => 
        Convert.ToHexString(System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes(input)));
}
```

### 9. Conversation Context Window Management (P2)

```csharp
public interface IContextWindowManager
{
    Task<IEnumerable<ChatMessage>> OptimizeContextAsync(
        IEnumerable<ChatMessage> messages,
        int maxTokens,
        CancellationToken ct = default);
}

public class ContextWindowManager : IContextWindowManager
{
    private readonly ISummarizationService _summarizer;
    
    public ContextWindowManager(ISummarizationService summarizer)
    {
        _summarizer = summarizer;
    }
    
    // Fixed: Proper async implementation
    public async Task<IEnumerable<ChatMessage>> OptimizeContextAsync(
        IEnumerable<ChatMessage> messages, 
        int maxTokens,
        CancellationToken ct = default)
    {
        var orderedMessages = messages.OrderByDescending(x => x.Timestamp).ToList();
        var selectedMessages = new List<ChatMessage>();
        var currentTokens = 0;
        
        foreach (var message in orderedMessages)
        {
            if (currentTokens + message.TokenCount > maxTokens)
            {
                // Summarize older messages instead of dropping
                if (selectedMessages.Any())
                {
                    var summary = await _summarizer.SummarizeAsync(
                        selectedMessages, ct);
                    selectedMessages.Clear();
                    selectedMessages.Add(new ChatMessage(
                        "system", 
                        $"Previous conversation summary: {summary}"));
                    currentTokens = summary.Length / 4; // rough token estimate
                }
                else
                {
                    // Can't fit even one message, skip oldest
                    continue;
                }
            }
            
            selectedMessages.Add(message);
            currentTokens += message.TokenCount;
        }
        
        return selectedMessages.AsEnumerable().Reverse();
    }
}

// Fixed: Use default values for optional parameters
public record ChatMessage(
    string Role, 
    string Content, 
    int TokenCount = 0, 
    DateTime Timestamp = default)
{
    public ChatMessage(string role, string content) 
        : this(role, content, 0, DateTime.UtcNow) { }
}

public interface ISummarizationService
{
    Task<string> SummarizeAsync(IEnumerable<ChatMessage> messages, CancellationToken ct);
}
```

### 10. Revised Phase Ordering

| Phase | Component | Reason |
|-------|-----------|--------|
| **Phase 0** | Architectural Decisions | Multi-tenancy, Auth, Embedding model |
| **Phase 1** | Database + Infrastructure | PostgreSQL, Redis, RabbitMQ |
| **Phase 2** | Messaging + Outbox | Async processing foundation |
| **Phase 3** | AI Core | Semantic Kernel, Embedding |
| **Phase 4** | Observability | Logging, Metrics, Health Checks |
| **Phase 5** | Auth + Security | JWT, Rate Limiting |
| **Phase 6** | API (Sync) | REST endpoints |
| **Phase 7** | Real-time | SignalR, SSE |
| **Phase 8** | UI | Next.js integration |
| **Phase 9** | Enterprise Scale | API Gateway, BFF, Cost Governance |

---

## Conclusion

This plan provides a comprehensive roadmap for building an Enterprise AI-Native Clean Architecture application. By following this structured approach, we will create a scalable, maintainable, and production-ready system that leverages the best practices in .NET development and AI engineering.

The implementation should be done incrementally, with each phase building upon the previous one. Regular reviews and adjustments will ensure the project stays on track and meets the evolving requirements.

---

### 🔬 Testing Infrastructure (Enhanced)

**Objective**: Build comprehensive test infrastructure to validate all processes with enterprise-grade quality.

> **Labels**:
> - ✅ **Must have**: Unit tests for handlers, integration tests with TestContainers
> - 🔄 **Nice to have**: E2E tests, contract testing
> - 🔮 **Future Ideas**: Chaos testing, security scanning

#### Test Categories (Enhanced)

| Category | Framework | Coverage |
|----------|-----------|----------|
| **Unit Tests** | xUnit, FluentAssertions, Moq | Domain behavior, Command/Query handlers, Value objects |
| **Integration Tests** | TestContainers, Respawn | API endpoints, Message publishing/consuming, Database operations |
| **E2E Tests** | Playwright / Supertest | Complete user workflows, SignalR communication |
| **Contract Tests** | Pact | Microservice integrations |
| **Load Tests** | k6, BenchmarkDotNet | Performance under load, stress testing, AI latency benchmarks |
| **Security Tests** | OWASP ZAP, NSwag Studio | Vulnerability scanning, API contract validation |
| **Snapshot Tests** | Verify | Complex object serialization, AI response validation |
| **Mutation Tests** | Stryker.NET | Test quality and coverage |

#### Test Projects Structure (Enhanced)

```
tests/
├── UnitTests/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── AgentLogTests.cs
│   │   │   └── AiMemoryEntryTests.cs
│   │   ├── Events/
│   │   │   └── DomainEventsTests.cs
│   │   └── ValueObjects/
│   │       └── EmbeddingVectorTests.cs
│   ├── Application/
│   │   ├── Commands/
│   │   │   ├── AskAiCommandTests.cs
│   │   │   └── UploadDocumentCommandTests.cs
│   │   ├── Queries/
│   │   │   ├── GetConversationQueryTests.cs
│   │   │   └── SearchMemoriesQueryTests.cs
│   │   ├── Validators/
│   │   │   ├── AskAiCommandValidatorTests.cs
│   │   │   └── AIRequestValidatorTests.cs
│   │   └── Behaviors/
│   │       └── ValidationBehaviorTests.cs
│   └── Infrastructure/
│       ├── Services/
│       │   ├── AgentServiceTests.cs
│       │   └── SemanticKernelAgentTests.cs
│       └── Messaging/
│           └── OutboxProcessorTests.cs
├── IntegrationTests/
│   ├── Api/
│   │   ├── AgentEndpointsTests.cs
│   │   ├── ConversationEndpointsTests.cs
│   │   ├── HealthEndpointsTests.cs
│   │   └── AuthenticationTests.cs
│   ├── Messaging/
│   │   ├── MassTransitTests.cs
│   │   ├── ConsumerTests.cs
│   │   └── OutboxPatternTests.cs
│   ├── Persistence/
│   │   ├── AgentDbContextTests.cs
│   │   ├── VectorSearchTests.cs
│   │   └── MigrationTests.cs
│   └── AI/
│       ├── SemanticKernelTests.cs
│       ├── EmbeddingGenerationTests.cs
│       └── PromptTemplateTests.cs
├── E2ETests/
│   ├── Workflows/
│   │   ├── AskAiWorkflowTests.cs
│   │   ├── MultiTurnConversationTests.cs
│   │   └── DocumentUploadWorkflowTests.cs
│   ├── SignalR/
│   │   ├── RealTimeCommunicationTests.cs
│   │   └── StreamingResponseTests.cs
│   └── UI/
│       └── NextJsPlaywrightTests.cs
├── ContractTests/
│   ├── ApiContractTests.cs
│   └── MessagingContractTests.cs
├── LoadTests/
│   ├── K6/
│   │   ├── ai-endpoints.js
│   │   ├── messaging.js
│   │   └── stress-test.js
│   └── Benchmark/
│       ├── EmbeddingBenchmark.cs
│       └── SemanticSearchBenchmark.cs
├── SecurityTests/
│   ├── AuthenticationTests.cs
│   ├── AuthorizationTests.cs
│   ├── RateLimitingTests.cs
│   └── PromptInjectionTests.cs
└── TestHelpers/
    ├── Fixtures/
    │   ├── DatabaseFixture.cs
    │   ├── MessageBusFixture.cs
    │   └── AiServiceFixture.cs
    ├── Builders/
    │   ├── AskAiCommandBuilder.cs
    │   └── AgentLogBuilder.cs
    ├── Mocks/
    │   ├── MockAIAgent.cs
    │   └── MockVectorStore.cs
    └── Utilities/
        ├── TestDataGenerator.cs
        └── SnapshotComparer.cs
```

#### TestContainers Configuration (Enhanced)

```csharp
public class AiAgentTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .WithEnvironment("POSTGRES_USER", "test")
        .WithEnvironment("POSTGRES_PASSWORD", "test")
        .WithEnvironment("POSTGRES_DB", "test")
        .Build();
    
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("test")
        .WithPassword("test")
        .Build();
    
    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis/redis-stack:7.2")
        .Build();
    
    // Elasticsearch for log testing
    private readonly ElasticsearchContainer _elasticsearch = new ElasticsearchBuilder()
        .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.11.0")
        .WithEnvironment("discovery.type", "single-node")
        .WithEnvironment("xpack.security.enabled", "false")
        .Build();
    
    public string PostgresConnectionString => _postgres.GetConnectionString();
    public string RabbitConnectionString => _rabbit.GetConnectionString();
    public string RedisConnectionString => _redis.GetConnectionString();
    public string ElasticsearchUri => _elasticsearch.GetUri();
    
    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgres.StartAsync(),
            _rabbit.StartAsync(),
            _redis.StartAsync(),
            _elasticsearch.StartAsync()
        );
        
        // Initialize database schema
        await InitializeDatabaseAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _rabbit.DisposeAsync().AsTask(),
            _redis.DisposeAsync().AsTask(),
            _elasticsearch.DisposeAsync().AsTask()
        );
    }
    
    private async Task InitializeDatabaseAsync()
    {
        // Run EF Core migrations
    }
}

// Collection fixture for parallel test execution
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestsCollection : ICollectionFixture<AiAgentTestFixture>
{
}
```

#### AI-Specific Test Scenarios

| Test Scenario | Description | Expected Outcome |
|---------------|-------------|------------------|
| **Embedding Generation** | Test vector embedding creation | Valid 1536-dim vector |
| **Semantic Search** | Test similarity search with pgvector | Relevant results ranked |
| **Prompt Injection** | Test malicious prompt patterns | Blocked/sanitized |
| **Token Usage Tracking** | Test token counting | Accurate token metrics |
| **Conversation Context** | Test multi-turn context retention | Previous context preserved |
| **Rate Limiting** | Test API rate limits | 429 after threshold |
| **Outbox Processing** | Test message retry logic | Messages eventually delivered |
| **Circuit Breaker** | Test LLM failure handling | Graceful degradation |

#### Mock Strategies

```csharp
// Mock AI Agent for unit tests
public class MockAIAgent : IAIAgent
{
    public Task<string> AskAsync(string question, CancellationToken cancellationToken)
    {
        // Return deterministic response for testing
        return Task.FromResult($"Mocked response for: {question}");
    }
}

// Mock Vector Store
public class MockVectorStore : IVectorStore
{
    private readonly List<VectorEntry> _entries = new();
    
    public Task<IEnumerable<VectorEntry>> SearchAsync(ReadOnlySpan<float> query, int topK)
    {
        // Return mock similarity results
    }
}
```

#### CI/CD Test Pipeline (Enhanced)

```yaml
# .github/workflows/test.yml
name: Test

on: [push, pull_request]

env:
  DOTNET_VERSION: '10.0.x'
  NODE_VERSION: '20'

jobs:
  # Code Quality Checks
  code-quality:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Analyze
        run: |
          dotnet tool restore
          dotnet format --verify-no-changes --verbosity diagnostic
          dotnet build --no-restore
      - name: Security Analysis
        run: |
          dotnet tool install --global dotnet-outdated-tool
          dotnet outdated src/**/*.csproj

  # Unit Tests
  unit-tests:
    needs: code-quality
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Cache
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Unit Tests
        run: dotnet test tests/UnitTests \
          --verbosity normal \
          --collect:"XPlat Code Coverage" \
          --datacollectors:"IDataCoverageReporter"
      - name: Upload Coverage
        uses: codecov/codecov-action@v4
        with:
          files: ./coverage.cobertura.xml
          fail_ci_if_error: false

  # Mutation Tests
  mutation-tests:
    needs: unit-tests
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Stryker
        run: dotnet tool install --global dotnet-stryker
        continue-on-error: true
      - name: Run Mutation Tests
        run: dotnet stryker -c stryker-config.json
        continue-on-error: true

  # Integration Tests
  integration-tests:
    needs: unit-tests
    runs-on: ubuntu-latest
    services:
      postgres:
        image: pgvector/pgvector:pg16
        env:
          POSTGRES_PASSWORD: test
          POSTGRES_USER: test
          POSTGRES_DB: test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
      redis:
        image: redis/redis-stack:7.2
        ports:
          - 6379:6379
      rabbitmq:
        image: rabbitmq:3-management-alpine
        env:
          RABBITMQ_DEFAULT_USER: test
          RABBITMQ_DEFAULT_PASS: test
        ports:
          - 5672:5672
          - 15672:15672
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Integration Tests
        run: dotnet test tests/IntegrationTests \
          --verbosity normal \
          --filter "Category=Integration"

  # Security Tests
  security-tests:
    needs: integration-tests
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Run Security Tests
        run: dotnet test tests/SecurityTests

  # E2E Tests
  e2e-tests:
    needs: security-tests
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
      - name: Start Docker Compose
        run: cd deploy && docker-compose up -d
      - name: Wait for services
        run: sleep 30
      - name: E2E Tests
        run: dotnet test tests/E2ETests
      - name: UI Tests
        run: |
          cd src/Presentation/UI
          npm ci
          npx playwright install --with-deps
          npx playwright test
      - name: Cleanup
        if: always()
        run: cd deploy && docker-compose down

  # Load Tests (Nightly)
  load-tests:
    if: github.event_name == 'schedule'
    needs: e2e-tests
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup k6
        run: sudo gvm k6@latest
      - name: Run Load Tests
        run: |
          k6 run tests/LoadTests/K6/ai-endpoints.js \
            --out influxdb=http://localhost:8086/k6
      - name: Benchmark Tests
        run: dotnet test tests/LoadTests/Benchmark

  # Contract Tests (Weekly)
  contract-tests:
    if: github.event_name == 'schedule'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Run Pact Tests
        run: |
          dotnet tool install --global PactNet.CI
          dotnet test tests/ContractTests
```

#### k6 Load Test Scenarios

```javascript
// tests/LoadTests/K6/ai-endpoints.js
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

export const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '2m', target: 100 },  // Ramp up
    { duration: '5m', target: 100 },  // Stay at 100
    { duration: '2m', target: 200 },  // Spike to 200
    { duration: '5m', target: 200 },  // Stay at 200
    { duration: '2m', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% under 500ms
    errors: ['rate<0.1'],               // Error rate under 10%
  },
};

export default function () {
  const payload = JSON.stringify({
    question: 'What is the capital of France?',
    sessionId: 'test-session-123',
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer test-token',
    },
  };

  const response = http.post(
    'http://localhost:5000/ai/ask',
    payload,
    params
  );

  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);

  sleep(1);
}
```

## 📌 Phase 7: Enterprise Scale

### 7.8 Advanced Observability & Alerts
- **Prometheus + Grafana** → hazır dashboard şablonları (API latency, queue depth, AI latency, error rate).  
- **Alertmanager** → otomatik uyarılar (CPU > 85%, Memory > 90%, Queue Depth > 500).  
- **AI-specific metrics** → embedding latency, token usage per user, semantic search hit ratio.  
- **Health Checks** → `/health`, `/health/ready`, `/health/live` endpoint’leri Aspire/Grafana ile entegre.

### 7.9 Data Governance & Compliance
- **GDPR / KVKK uyumluluğu** → PII maskeleme, consent management.  
- **Audit Trail** → retention policy ile log yönetimi.  
- **Data Retention Jobs** → otomatik temizlik ve arşivleme.  
- **Security Events** → authz failure, rate limit violation, critical AI actions loglanır.

### 7.10 Scalability Patterns
- **Saga Pattern** → uzun süren AI workflow orchestration.  
- **Event Sourcing** → replay edilebilir event store.  
- **CQRS + Read Models** → performanslı sorgular için ayrı read DB.  
- **Resilience Pipelines (Polly v8)** → retry + jitter, timeout, circuit breaker, hedging.

### 7.11 Developer Experience (DevEx)
- **Makefile / dotnet tool** → tek komut build/test/deploy.  
- **GitHub Codespaces / devcontainer** → hızlı onboarding.  
- **CI/CD Pipeline** → smoke test + health check adımları.  
- **TestContainers** → PostgreSQL, Redis, RabbitMQ ile gerçekçi integration testleri.

### 7.12 API Gateway / BFF
- **YARP / Ocelot** → API Gateway ile routing, rate limiting, auth.  
- **BFF Pattern** → Next.js için frontend’e özel endpoint optimizasyonu, mobil için ayrı BFF.  
- **Reverse Proxy Config** → cross-cutting concerns (auth, logging, throttling).

### 7.13 Feature Flags & Configuration Management
- **LaunchDarkly / Unleash** → A/B testing, gradual rollout.  
- **Azure App Configuration** → runtime config değişiklikleri.  
- **HashiCorp Vault / Azure Key Vault** → secrets management.  
- **Dynamic Rollouts** → yeni AI engine veya vector search özelliğini kontrollü açma.

---

## 🎯 Showcase Keywords
- **Event-Driven AI Orchestration**  
- **RAG Implementation with pgvector & Redis**  
- **Resilient Microservices with Polly v8**  
- **Clean Architecture & DDD Principles**  
- **Enterprise AI-Native Infrastructure**  

---

## 🏗️ Sprint Plan
- **Sprint 1:** Database + Persistence  
- **Sprint 2:** Messaging Layer  
- **Sprint 3:** AI Layer  
- **Sprint 4:** Presentation  
- **Sprint 5:** Observability + Security  
- **Sprint 6:** Cloud & DevOps  
- **Sprint 7:** Enterprise Scale (Phase 7.8–7.13)

---

## 📊 Monitoring Metrics
| Metric                | Target   | Alert Threshold |
|------------------------|----------|-----------------|
| API Response Time      | <200ms   | >500ms          |
| Agent Execution Time   | <5s      | >30s            |
| Error Rate             | <0.1%    | >1%             |
| Queue Depth            | <100     | >500            |
| CPU Usage              | <70%     | >85%            |
| Memory Usage           | <80%     | >90%            |
---

*Document Version: 1.8*  
*Last Updated: 2026-03-06*  
*Author: Enterprise Architecture Team*
*Revision Notes: Fixed code bugs - TenantResolutionMiddleware (_next), TenantQueryFilters (runtime scoping), ReadOnlySpan in async (float[]), ChatMessage constructor, FT.ADD deprecated, Polly v8 AddBulkhead (AddConcurrencyLimiter), Prometheus increase() vs rate()*
