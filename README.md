# Rag_bot (.NET 10 RAG Implementation)

This is a RAG (Retrieval-Augmented Generation) bot built with .NET Web API following Clean Architecture principles.

## Tech Stack
- **LLM**: OpenAI (official v2 SDK)
- **Vector DB**: Postgres with `pgvector`
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, WebApi)
- **Framework**: .NET 10.0
- **Mediator**: MediatR

## Project Structure
- `src/RagBot.Domain`: Core entities and value objects.
- `src/RagBot.Application`: Use cases (Commands/Queries) and interfaces.
- `src/RagBot.Infrastructure`: Database context, external service implementations.
- `src/RagBot.WebApi`: API controllers and configuration.

## Setup Instructions

### 1. Database Setup
Ensure you have a Postgres instance running with the `pgvector` extension installed.
```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

### 2. Configuration
Update `src/RagBot.WebApi/appsettings.json` with:
- `ConnectionStrings:DefaultConnection`: Your Postgres connection string.
- `OpenAI:ApiKey`: Your OpenAI API key.

### 3. Database Migrations
Run the following commands to create the database schema:
```bash
cd Rag_bot/src/RagBot.WebApi
dotnet ef migrations add InitialCreate --project ../RagBot.Infrastructure
dotnet ef database update
```

### 4. Running the App
```bash
cd Rag_bot/src/RagBot.WebApi
dotnet run
```

## API Endpoints
- **POST** `/api/KnowledgeBase/ingest`: Add text chunks to the vector database.
- **POST** `/api/Chat/ask`: Ask a question using the retrieved context from the vector database.
