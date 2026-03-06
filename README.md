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
