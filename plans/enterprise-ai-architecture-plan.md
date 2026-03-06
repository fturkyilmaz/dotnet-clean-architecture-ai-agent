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

**Objective**: Extend the Domain layer with AI-specific entities, events, and value objects that capture the essence of AI agent workflows.

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

**Objective**: Implement robust event-driven messaging with guaranteed delivery.

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

**Objective**: Leverage Semantic Kernel for advanced prompt engineering and function calling.

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

**Objective**: Implement comprehensive observability with distributed tracing and centralized logging.

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

1. **Authentication**: JWT tokens with short expiration
2. **Authorization**: Role-based access control (RBAC)
3. **Encryption**: TLS for all external communication
4. **Secrets**: Kubernetes Secrets or HashiCorp Vault
5. **Rate Limiting**: Prevent abuse of AI endpoints
6. **Input Validation**: Sanitize all user inputs
7. **Audit Logging**: Track sensitive operations

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
// Chaos Engineering Example with Polly
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
    .AddBulkhead(maxParallelization: 10)
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
    expr: sum by (user_id) (rate(token_usage_total[1h])) > 10000
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

---

## Conclusion

This plan provides a comprehensive roadmap for building an Enterprise AI-Native Clean Architecture application. By following this structured approach, we will create a scalable, maintainable, and production-ready system that leverages the best practices in .NET development and AI engineering.

The implementation should be done incrementally, with each phase building upon the previous one. Regular reviews and adjustments will ensure the project stays on track and meets the evolving requirements.

---

*Document Version: 1.1*  
*Last Updated: 2026-03-06*  
*Author: Enterprise Architecture Team*
