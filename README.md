# .NET 10 Clean Architecture + AI Agent + Next.js UI

Modern .NET 10 backend (ASP.NET Core, EF Core, PostgreSQL, Redis, RabbitMQ, Semantic Kernel AI Agent) ve Next.js 15 frontend ile full-stack Clean Architecture örnek projesi.

## 🚀 Proje Yapısı

```
/
  /src
    /Domain                     -> Entities, Value Objects
    /Application                -> Business logic, DTOs, Use Cases
    /Infrastructure             -> EF Core, PostgreSQL, Redis, RabbitMQ
    /Presentation
      /Api                      -> ASP.NET Core Minimal API
      /UI                       -> Next.js 15 (App Router, TypeScript, Tailwind)
  /AI                           -> Semantic Kernel, OpenAI SDK
  /tests
    /UnitTests
    /IntegrationTests
  /deploy
    docker-compose.yml           -> Full-stack deployment
    k8s-manifests/              -> Kubernetes manifests
```

## 🛠️ Teknoloji Stack

### Backend
- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core 10
- PostgreSQL
- Redis (Caching)
- RabbitMQ (Message Queue)
- Semantic Kernel (AI Agent)
- OpenAI SDK

### Frontend
- Next.js 15
- TypeScript
- Tailwind CSS
- App Router

## 🧱 Architecture Blueprint (C4 Model)

Bu proje, modern AI-native ve cloud-native bir .NET 10 backend’i örneklemek için tasarlanmış, Clean Architecture tabanlı bir mimari blueprint uygular.

### C1 – System Context

- **Odak Sistem**: AI-Enabled Clean Architecture Platform
- **Aktörler**:
  - Son kullanıcı: Next.js UI üzerinden AI soruları sorar.
  - Ops / Admin: Grafana, Kibana ve .NET Aspire ile sistemi izler.
- **Dış Sistemler**:
  - Identity Provider (OpenIddict veya harici OAuth2/OIDC)
  - LLM Providers (OpenAI, Azure OpenAI, Anthropic, Llama 3)

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

Person(user, "End User", "Web UI üzerinden sistemi kullanan son kullanıcı")
Person(admin, "Ops / Admin", "Sistemin sağlığını izleyen ve logları analiz eden kişi")

System_Boundary(ai_sys, "AI-Enabled Clean Architecture Platform") {
  System(api, "API Backend", "ASP.NET Core 10 Minimal API")
  System(ui, "Web UI", "Next.js 15 + Tailwind")
  System(observability, "Observability Stack", "Prometheus, Grafana, OpenTelemetry, ELK, .NET Aspire")
}

System_Ext(idp, "Identity Provider", "OpenIddict veya harici OAuth2/OIDC sağlayıcı")
System_Ext(llm, "LLM Providers", "OpenAI, Azure OpenAI, Anthropic, Llama 3")
System_Ext(mail, "Notification Service", "E-posta / mesajlaşma sağlayıcısı (opsiyonel)")

Rel(user, ui, "Kullanır", "HTTPS")
Rel(ui, api, "REST / gRPC istekleri", "HTTPS")
Rel(api, idp, "Kimlik doğrulama / token doğrulama", "OIDC / OAuth2")
Rel(api, llm, "LLM çağrıları", "HTTPS / SDK")
Rel(admin, observability, "Metrix, log ve trace’leri izler", "HTTPS")
Rel(api, mail, "Bildirim gönderir", "SMTP / HTTP API")

@enduml
```

### C2 – Container View

Başlıca container’lar:

- **Web UI**: Next.js 15, TypeScript, Tailwind
- **API Backend**: ASP.NET Core 10 Minimal API, CQRS + MediatR, FluentValidation
- **AI Worker / Agent Service**: .NET Worker + MassTransit + Semantic Kernel
- **Message Broker**: RabbitMQ + MassTransit (event-driven, Outbox Pattern)
- **Veri Katmanı**:
  - PostgreSQL + pgvector (AI hafıza + domain verileri)
  - MSSQL (opsiyonel kurumsal veri)
  - Redis Stack (cache + rate limiting + vector search)
- **Observability**:
  - OpenTelemetry + .NET Aspire
  - Prometheus + Grafana
  - ELK Stack (Elasticsearch, Logstash, Kibana)

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

Person(user, "End User")
Person(admin, "Ops / Admin")

System_Boundary(ai_sys, "AI-Enabled Clean Architecture Platform") {

  Container(ui, "Web UI", "Next.js 15, TypeScript, Tailwind", "Kullanıcı arayüzü, AI soruları gönderir, sonuçları gösterir")
  Container(api, "API Backend", "ASP.NET Core 10 Minimal API", "REST/gRPC endpoint’leri, CQRS, validation, security")
  Container(worker, "AI Worker / Agent Service", ".NET 10 Worker Service + MassTransit", "AI isteklerini event-driven olarak işler")
  Container(mq, "Message Broker", "RabbitMQ + MassTransit", "Event-driven iletişim, Outbox Pattern")
  ContainerDb(pg, "PostgreSQL", "Relational DB + pgvector", "Domain verileri, AI hafızası, audit loglar")
  ContainerDb(mssql, "MSSQL", "Relational DB", "Kurumsal / finansal veriler (opsiyonel)")
  ContainerDb(redis, "Redis Stack", "In-memory cache + vector search", "Caching, rate limiting, ek vector search")
  Container(elasticsearch, "Elasticsearch", "Search engine", "Log ve event aramaları")
  Container(logstash, "Logstash", "Ingest pipeline", "Logları toplar ve Elasticsearch’e yazar")
  Container(kibana, "Kibana", "Dashboard", "Log görselleştirme ve arama")
  Container(prometheus, "Prometheus", "Metrics DB", "Uygulama ve altyapı metrikleri")
  Container(grafana, "Grafana", "Dashboard", "Metriklerin görselleştirilmesi")
  Container(otel, "OpenTelemetry Collector / .NET Aspire", "Telemetry pipeline", "Trace, metric ve log toplama ve yönlendirme")

}

System_Ext(idp, "Identity Provider", "OpenIddict / Harici IdP")
System_Ext(llm, "LLM Providers", "OpenAI, Azure OpenAI, Anthropic, Llama 3")

Rel(user, ui, "Kullanır", "HTTPS")
Rel(ui, api, "REST/JSON istekleri", "HTTPS")

Rel(api, idp, "Token doğrulama, kullanıcı bilgisi alma", "OIDC / OAuth2")
Rel(api, mq, "Komut/event yayınlar", "AMQP")
Rel(worker, mq, "Event consume eder", "AMQP")

Rel(api, pg, "CRUD, query, pgvector semantic search", "EF Core 10 / Dapper")
Rel(worker, pg, "AI memory read/write, outbox/inbox", "EF Core 10")
Rel(api, mssql, "Kurumsal veri", "EF Core 10 / Dapper")
Rel(api, redis, "Cache, rate limiting, session", "Redis client")

Rel(api, llm, "LLM çağrıları", "HTTPS / SDK")
Rel(worker, llm, "Agent workflow LLM çağrıları", "HTTPS / SDK")

Rel(api, otel, "Trace, metric, log export", "OTel protocol")
Rel(worker, otel, "Trace, metric, log export", "OTel protocol")
Rel(otel, prometheus, "Metrics yönlendirir", "OTel exporter")
Rel(otel, elasticsearch, "Log ve trace’leri yönlendirir", "OTel exporter")
Rel(prometheus, grafana, "Metrics okur", "HTTP")
Rel(elasticsearch, kibana, "Log ve event araması için kullanır", "HTTP")
Rel(api, logstash, "Serilog ile log yazar (opsiyon)", "TCP/HTTP")
Rel(logstash, elasticsearch, "Logları indexler", "HTTP")

Rel(admin, grafana, "Metrikleri izler", "HTTPS")
Rel(admin, kibana, "Logları arar", "HTTPS")
Rel(admin, ".NET Aspire Dashboard", "Servis sağlığını ve bağımlılıkları görür", "HTTPS")

@enduml
```

### C3 – Backend & AI Component View

Backend ve AI worker bileşenleri:

- **Presentation**: Minimal API endpoint’leri (`/ai/ask`, `/health`, `/auth` vb.)
- **Application**: CQRS (MediatR) + FluentValidation + business rules
- **Domain**: Entities, Value Objects, Domain Events
- **Infrastructure**: EF Core, Dapper, Redis, MassTransit, Serilog, AI Client (Semantic Kernel)
- **AI Worker**: MassTransit consumers, Agent Orchestrator (Researcher & Summarizer agents), Memory Store abstraction

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

Container(api, "API Backend", "ASP.NET Core 10 Minimal API") {
  
  Component(presentation, "Presentation Layer", "Minimal API endpoints", "HTTP endpoint’leri, DTO mapping, model binding")
  Component(app_layer, "Application Layer", "MediatR + CQRS + FluentValidation", "Use case’ler, komut/sorgu handler’ları")
  Component(domain, "Domain Layer", "Entities, Value Objects, Domain Events", "Saf domain kuralları")
  Component(infra, "Infrastructure Layer", "EF Core, Dapper, Redis, MassTransit, Serilog", "Persistence, messaging, logging, caching")
  Component(security, "Security Layer", "AuthZ/AuthN, token validation", "JWT, OAuth2, OIDC entegrasyonu")
  Component(resilience, "Resilience Pipelines", "Polly v8", "Retry, circuit breaker, timeout, hedging")
}

Container(worker, "AI Worker / Agent Service", ".NET 10 Worker + MassTransit") {
  
  Component(mt_consumers, "Message Consumers", "MassTransit", "UserQueryReceived, AIResponseGenerated, vb.")
  Component(agent_orchestrator, "Agent Orchestrator", "Semantic Kernel", "Researcher & Summarizer agent workflow’ları")
  Component(memory_store, "Memory Store", "Abstraction", "Postgres pgvector + Redis vector search kombinasyonu")
  Component(ai_client, "LLM Client", "Semantic Kernel Connectors", "OpenAI, Azure OpenAI, Anthropic, Llama 3 adapter’ları")
  Component(ai_resilience, "AI Resilience Pipelines", "Polly v8", "LLM çağrıları için özel retry, timeout, circuit breaker, hedging")
}

Rel(presentation, app_layer, "MediatR üzerinden komut/sorgu gönderir")
Rel(app_layer, domain, "Domain kurallarını kullanır")
Rel(app_layer, infra, "Repository, Unit of Work, Outbox kullanır")
Rel(app_layer, resilience, "Polly pipeline’ları ile sarmalanmış işlemler")

Rel(infra, "PostgreSQL", "EF Core 10")
Rel(infra, "MSSQL", "EF Core / Dapper")
Rel(infra, "Redis", "Caching, rate limiting")
Rel(infra, "RabbitMQ", "MassTransit ile event publish")

Rel(presentation, security, "Token doğrulama, claim kontrolü", "JWT / OIDC")
Rel(app_layer, security, "Authorization policy kontrolü")

Rel(mt_consumers, "RabbitMQ", "Event consume")
Rel(mt_consumers, agent_orchestrator, "AI iş akışlarını tetikler")
Rel(agent_orchestrator, ai_client, "LLM çağrıları yapar")
Rel(agent_orchestrator, memory_store, "Semantic search, context fetch/store")
Rel(memory_store, "PostgreSQL (pgvector)", "Embedding read/write")
Rel(memory_store, "Redis Stack", "Ek vector search / cache")
Rel(ai_client, "LLM Providers", "HTTPS / SDK")

@enduml
```

## 🏃‍♂️ Çalıştırma

### Docker Compose (Tüm Servisler)

```bash
cd deploy
docker-compose up --build
```

Servisler:
- **API**: http://localhost:5000
- **UI**: http://localhost:3000
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379
- **RabbitMQ Management**: http://localhost:15672

### Manuel Çalıştırma

#### Backend
```bash
dotnet run --project src/Presentation/Api
```

#### Frontend
```bash
cd src/Presentation/UI
npm run dev
```

## 📡 API Endpoints

### AI Agent
```
GET /ai/ask?question={question}
```

## 📝 Lisans

MIT
