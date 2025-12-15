# FalconAPI - Copilot Instructions

## Project Overview
**Falcon Competition** is a real-time programming competition platform built with **.NET 10 / C# 13**. The backend provides REST APIs, WebSocket communication (SignalR), background processing, and integration with an external Judge API for code evaluation.

## Architecture

### Layered Pattern
- **Controllers** → HTTP endpoints + input validation (see [Controllers/](ProjetoTccBackend/Controllers/))
- **Services** → Business logic + transaction coordination (see [Services/](ProjetoTccBackend/Services/))
- **Repositories** → Data access abstraction via EF Core (see [Repositories/](ProjetoTccBackend/Repositories/))
- **Hubs** → SignalR real-time communication ([CompetitionHub.cs](ProjetoTccBackend/Hubs/CompetitionHub.cs))
- **Workers** → Background task processing ([Workers/](ProjetoTccBackend/Workers/))

### Repository Pattern
- All repositories inherit from `GenericRepository<T>` providing base CRUD operations
- Repositories are injected into services via interfaces (`I*Repository`)
- Example: `ICompetitionRepository` → `CompetitionRepository` → `GenericRepository<Competition>`

### SignalR Real-Time Architecture
- **Hub**: [CompetitionHub.cs](ProjetoTccBackend/Hubs/CompetitionHub.cs) manages connections
- **Groups**: Users auto-join role-based groups (`Admins`, `Teachers`, `Students`, `{userId}`)
- **Cache**: Competition data cached with 5s TTL (`CompetitionCacheService`)
- **Events**: See [SIGNALR_COMPETITION_HUB_DOCUMENTATION.md](SIGNALR_COMPETITION_HUB_DOCUMENTATION.md) for protocol details

### Background Processing
- **ExerciseSubmissionWorker**: Processes submissions from queue → Judge API → broadcasts results
- **CompetitionStateWorker**: Monitors competition state transitions (configurable polling: `CompetitionWorker:IdleSeconds` / `OperationalSeconds`)
- **Queue**: `ExerciseSubmissionQueue` decouples submission intake from processing (parallel workers: max 8)

### External Integration
- **Judge API**: External service for code evaluation ([JudgeService.cs](ProjetoTccBackend/Services/JudgeService.cs))
- **Token caching**: JWT tokens cached in `IMemoryCache` to minimize auth overhead
- **Configuration**: `appsettings.json` → `JudgeApi:Url`, `JudgeApi:SecurityKey`

## Development Workflows

### Running Locally
```powershell
# Use the launcher script (sets ASPNETCORE_ENVIRONMENT)
.\ProjetoTccBackend\run.ps1

# Direct run (Development environment)
dotnet run -lp https --no-self-contained -c Debug --project ProjetoTccBackend
```

### Running Tests
```powershell
# Integration tests (uses Testcontainers + MariaDB)
dotnet test ProjetoTccBackend.Integration.Test

# Unit tests
dotnet test ProjetoTCCBackend.Unit.Test
```
**Note**: Integration tests spin up MariaDB container via `TCCWebApplicationFactory` and apply migrations automatically.

### Database
- **Local Dev**: MariaDB 11 (connection string in `appsettings.Development.json`)
- **Production**: Azure SQL Server (env var `AZURE_SQL_CONNECTIONSTRING`)
- **Tests**: In-memory database via Testcontainers
- **Migrations**: Standard EF Core (`dotnet ef migrations add <Name>`)

## Code Conventions

### XML Documentation
**ALWAYS** add XML comments to public APIs following [.github/instructions/docsCsharpInstructions.instructions.md](.github/instructions/docsCsharpInstructions.instructions.md):
```csharp
/// <summary>
/// Brief description of what this does.
/// </summary>
/// <param name="userId">Description of parameter.</param>
/// <returns>Description of return value.</returns>
public async Task<User?> GetUserByIdAsync(string userId)
```
- Document ALL public methods, classes, interfaces, and properties
- Use `<remarks>` for complex implementation details
- Document interface properties inside the interface definition

### Async Patterns
- **Always** use async/await for I/O operations (database, HTTP, SignalR)
- Suffix async methods with `Async` (e.g., `GetCurrentCompetitionAsync()`)
- Avoid blocking calls like `.Result` or `.Wait()`

### Dependency Injection
- Inject services via **constructor** (never use `new` for services/repositories)
- Register services in [Program.cs](ProjetoTccBackend/Program.cs) with appropriate lifetimes:
  - `Transient`: Stateless services (e.g., `JudgeService`)
  - `Scoped`: Per-request services (e.g., repositories, most services)
  - `Singleton`: Caches, queues (e.g., `ExerciseSubmissionQueue`, `IMemoryCache`)

### Error Handling
- Custom exceptions in [Exceptions/](ProjetoTccBackend/Exceptions/) (e.g., `NotValidCompetitionException`)
- Global error handling via [ExceptionHandlingMiddleware.cs](ProjetoTccBackend/Middlewares/ExceptionHandlingMiddleware.cs)
- Use `FormException` for validation errors (returns structured JSON)

### Authorization
- Use ASP.NET Core Identity roles: `Admin`, `Teacher`, `Student`
- Apply `[Authorize(Roles = "Admin,Teacher")]` to controllers/actions
- SignalR: Use `[Authorize]` on hubs; check roles in methods

## Configuration

### Local Development
Key settings in [appsettings.json](ProjetoTccBackend/appsettings.json):
```json
{
  "ConnectionStrings:DefaultConnection": "Server=localhost,1433;...",
  "JudgeApi:Url": "https://localhost:8000/v0",
  "Cors:FrontendURL": "https://falconcompetitions.azurewebsites.net",
  "CompetitionWorker:IdleSeconds": 600,
  "CompetitionWorker:OperationalSeconds": 5
}
```

### Azure Deployment
**Critical**: See [AZURE_CONFIGURATION.md](AZURE_CONFIGURATION.md) for production setup:
- Use `__` (double underscore) for nested config (e.g., `JudgeApi__Url`)
- **MUST** enable ARR Affinity for SignalR (WebSocket sticky sessions)
- Connection string from env var `AZURE_SQL_CONNECTIONSTRING`

## Key Files Reference
- **Entry Point**: [Program.cs](ProjetoTccBackend/Program.cs) - DI registration, middleware pipeline, role/user seeding
- **SignalR Hub**: [CompetitionHub.cs](ProjetoTccBackend/Hubs/CompetitionHub.cs) - Real-time competition events
- **Base Repository**: [GenericRepository.cs](ProjetoTccBackend/Repositories/GenericRepository.cs) - CRUD operations template
- **Judge Integration**: [JudgeService.cs](ProjetoTccBackend/Services/JudgeService.cs) - External API calls
- **Background Jobs**: [ExerciseSubmissionWorker.cs](ProjetoTccBackend/Workers/ExerciseSubmissionWorker.cs), [CompetitionStateWorker.cs](ProjetoTccBackend/Workers/CompetitionStateWorker.cs)
- **Test Setup**: [TCCWebApplicationFactory.cs](ProjetoTccBackend.Integration.Test/TCCWebApplicationFactory.cs) - Integration test infrastructure

## Additional Documentation
- [README.md](README.md) - Comprehensive project documentation
- [SIGNALR_COMPETITION_HUB_DOCUMENTATION.md](SIGNALR_COMPETITION_HUB_DOCUMENTATION.md) - WebSocket API reference
- [AZURE_CONFIGURATION.md](AZURE_CONFIGURATION.md) - Production deployment guide
- [.github/agents/](d:\TCC\FalconAPI\.github\agents\README.md) - Specialized Copilot agents for code review
