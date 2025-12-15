# FalconAPI - TCC Project

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED)](https://www.docker.com/)

This repository contains the backend for **Falcon Competition**, a complete programming competition platform developed as a Final Course Project (TCC). The system provides robust infrastructure for real-time competitions, with automatic code evaluation, WebSocket communication, and comprehensive management of users, groups, and exercises.

**[🇧🇷 Versão em Português](README.pt-br.md)**

## 📋 Table of Contents

- [Technologies Used](#-technologies-used)
- [System Architecture](#-system-architecture)
- [Project Structure](#-project-structure)
- [Main Features](#-main-features)
- [Running Locally](#-running-locally)
  - [Test Users Configuration](#test-users-configuration)
- [Azure Deployment and Configuration](#-azure-deployment-and-configuration)
- [API and Endpoints](#-api-and-endpoints)
- [SignalR and Real-Time Communication](#-signalr-and-real-time-communication)
- [Workers and Queue System](#-workers-and-queue-system)
- [Tests](#-tests)
- [Additional Documentation](#-additional-documentation)

## 🚀 Technologies Used

### Framework and Runtime
- **.NET 10** (ASP.NET Core) - Main framework
- **C# 13** - Programming language
- **Entity Framework Core 10.0** - ORM for data access

### Database
- **SQL Server** (Production - Azure)
- **MariaDB 11** (Local development)
- **In-Memory Database** (Tests)

### Authentication and Security
- **ASP.NET Core Identity** - User and role management
- **JWT Bearer Authentication** - Token-based authentication
- **Cookie Authentication** - Frontend integration

### Real-Time Communication
- **SignalR** - WebSocket for bidirectional communication
- **JSON Protocol** - Message serialization

### Infrastructure
- **Docker** and **Docker Compose** - Containerization
- **Azure App Service** - Cloud hosting
- **Serilog** - Structured logging and tracing

### Documentation and Quality
- **Swagger/OpenAPI 3.1** - Interactive API documentation
- **xUnit** - Testing framework
- **Moq** - Mocking library for tests

### Main Dependencies
```xml
Microsoft.EntityFrameworkCore.SqlServer (10.0.0)
Microsoft.AspNetCore.Identity.EntityFrameworkCore (10.0.0)
Microsoft.AspNetCore.Authentication.JwtBearer (10.0.0)
Microsoft.AspNetCore.SignalR (10.0.0)
Swashbuckle.AspNetCore (10.0.1)
Serilog.AspNetCore (10.0.0)
Microsoft.OpenApi (2.3.0)
```

## 🏗️ System Architecture

The project follows a **layered architecture** with clear separation of responsibilities:

```
┌─────────────────────────────────────────────────────────┐
│                    Controllers Layer                     │
│          (REST API Endpoints + SignalR Hubs)            │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                    Services Layer                        │
│        (Business Logic + Validation Rules)              │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  Repositories Layer                      │
│           (Data Access + EF Abstractions)               │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  Database Layer                          │
│         (SQL Server / MariaDB / In-Memory)              │
└─────────────────────────────────────────────────────────┘

        ┌──────────────────────────────────┐
        │     Background Workers           │
        │  (Asynchronous Processing)      │
        └──────────────────────────────────┘
```

### Design Patterns Used
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Inversion of control
- **Service Layer Pattern** - Business logic encapsulation
- **Middleware Pipeline** - Global request handling
- **Background Services** - Asynchronous processing with queues

## 📁 Project Structure

```
FalconAPI/
├── ProjetoTccBackend/                     # Main API project
│   ├── Controllers/                       # REST endpoints
│   │   ├── AuthController.cs             # Authentication (login, register)
│   │   ├── UserController.cs             # User management
│   │   ├── CompetitionController.cs      # Competitions
│   │   ├── ExerciseController.cs         # Exercises and submissions
│   │   ├── GroupController.cs            # Student groups
│   │   ├── LogController.cs              # Logs and audit
│   │   ├── QuestionController.cs         # Questions and answers
│   │   ├── FileController.cs             # File upload/download
│   │   └── TokenController.cs            # Token renewal
│   │
│   ├── Hubs/                             # SignalR Hubs
│   │   └── CompetitionHub.cs             # Real-time competition hub
│   │
│   ├── Services/                         # Business logic layer
│   │   ├── Interfaces/                   # Service contracts
│   │   ├── UserService.cs
│   │   ├── CompetitionService.cs
│   │   ├── ExerciseService.cs
│   │   ├── GroupService.cs
│   │   ├── JudgeService.cs              # Judge API integration
│   │   ├── TokenService.cs              # JWT generation and validation
│   │   ├── LogService.cs
│   │   ├── CompetitionRankingService.cs
│   │   ├── GroupAttemptService.cs
│   │   └── ...
│   │
│   ├── Repositories/                     # Data access layer
│   │   ├── Interfaces/                   # Repository contracts
│   │   ├── GenericRepository.cs          # Generic base repository
│   │   ├── UserRepository.cs
│   │   ├── CompetitionRepository.cs
│   │   ├── ExerciseRepository.cs
│   │   ├── GroupRepository.cs
│   │   └── ...
│   │
│   ├── Workers/                          # Background Services
│   │   ├── ExerciseSubmissionWorker.cs  # Submission processing
│   │   ├── CompetitionStateWorker.cs    # State management
│   │   └── Queues/                      # Queue system
│   │       └── ExerciseSubmissionQueue.cs
│   │
│   ├── Models/                           # Domain entities
│   │   ├── User.cs
│   │   ├── Competition.cs
│   │   ├── Exercise.cs
│   │   ├── Group.cs
│   │   ├── GroupExerciseAttempt.cs
│   │   └── ...
│   │
│   ├── Database/                         # DTOs and DbContext
│   │   ├── TccDbContext.cs              # EF Core context
│   │   ├── Requests/                    # Input DTOs
│   │   └── Responses/                   # Output DTOs
│   │
│   ├── Middlewares/                      # Custom middlewares
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── RequestBodyLoggingMiddleware.cs
│   │
│   ├── Filters/                          # Action Filters
│   │   └── ValidateModelStateFilter.cs
│   │
│   ├── Swagger/                          # Swagger configuration
│   │   ├── Examples/                     # Payload examples
│   │   ├── Filters/                      # Custom filters
│   │   └── Extensions/
│   │
│   ├── Enums/                            # Enumerations
│   │   ├── Competition/
│   │   ├── Exercise/
│   │   ├── Judge/
│   │   └── ...
│   │
│   ├── Exceptions/                       # Custom exceptions
│   │   ├── Judge/
│   │   ├── User/
│   │   ├── Group/
│   │   └── ...
│   │
│   ├── Migrations/                       # EF Core migrations
│   ├── Validation/                       # Validators
│   ├── UserUploads/                      # User files
│   │
│   ├── Program.cs                        # Application entry point
│   ├── ProjetoTccBackend.csproj
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Dockerfile
│   └── docker-compose.development.yml
│
├── ProjetoTccBackend.Integration.Test/   # Integration tests
│   ├── Exercise_GET.cs
│   ├── Exercise_POST.cs
│   ├── UserAuth_POST.cs
│   ├── TCCWebApplicationFactory.cs
│   └── DataBuilders/
│
├── ProjetoTCCBackend.Unit.Test/          # Unit tests
│   ├── Services/
│   │   └── CompetitionRankingServiceTests.cs
│   └── ProjetoTCCBackend.Unit.Test.csproj
│
├── README.md
├── README.pt-br.md                       # Portuguese version
├── AZURE_CONFIGURATION.md                # Azure deployment guide
├── SIGNALR_COMPETITION_HUB_DOCUMENTATION.md
└── LICENSE.txt
```

## ✨ Main Features

### 🔐 Authentication and Authorization
- **User registration** with data validation
- **Login** with JWT token generation
- **Automatic token renewal** (refresh token)
- **Logout** with session invalidation
- **Role system**: Admin, Teacher, Student
- **Cookie-based authentication** for frontend integration
- **Endpoint protection** based on roles

### 👥 User Management
- Complete CRUD for users
- Filter by role (Admin, Teacher, Student)
- Paginated queries with sorting
- Profile updates
- Group association
- Login history and activities

### 🏆 Competition Management
- Competition creation and configuration
- Period control (registration, start, end)
- Rules configuration (penalties, limits)
- Exercise management per competition
- Real-time ranking system
- Competition states (Not Started, In Progress, Finished)
- Group submission blocking

### 📝 Exercise Management
- Programming exercise CRUD
- Attached file upload (PDFs, images)
- Test case definition (inputs/outputs)
- Customizable exercise types
- Judge API integration for automatic evaluation
- Multiple programming language support

### 👨‍👩‍👦 Group Management
- Student group creation
- Invitation system with approval
- Group leaders with special permissions
- Competition participation history
- Maximum member control

### 💬 Questions and Answers System
- Students can ask questions during competitions
- Questions can be general or exercise-specific
- Teachers and admins receive real-time notifications
- Public or private answers
- Complete Q&A history per competition

### 📊 Logs and Audit
- Detailed logging of all relevant actions
- Paginated queries by user, group, or competition
- Login/logout tracking
- Exercise attempt logging
- Administrative block and change logging
- IP and timestamp information

### ⚡ Real-Time Communication (SignalR)
- **WebSocket** for bidirectional communication
- **Group separation** (Admin, Teacher, Student)
- **Real-time events**:
  - Ranking updates
  - Exercise submissions
  - Questions and answers
  - Competition notifications
- **Automatic reconnection**
- **Health checks** (Ping/Pong)

### 🔄 Asynchronous Processing
- **ExerciseSubmissionWorker**: Background code evaluation
- **CompetitionStateWorker**: Competition state management
- **Thread-safe queue system** for submissions
- **Parallel processing** (up to 8 simultaneous submissions)
- **Retry logic** for temporary failures

### 🛡️ Error Handling
- **Global middleware** for exception catching
- **Standardized error responses**
- **Automatic model validation**
- **Structured logging** with Serilog
- **Clear and descriptive error messages**

### 📖 Automatic Documentation
- **Interactive Swagger UI** at `/swagger`
- **OpenAPI 3.1** specification
- **Payload examples** for all endpoints
- **Detailed Request/Response schemas**
- Available only in development environment

## 🔧 Running Locally

### Prerequisites

- **Docker** and **Docker Compose** installed
- **.NET 10 SDK** (optional for development without Docker)
- **Visual Studio 2022** or **VS Code** (recommended)

### Option 1: Run with Docker Compose (Recommended)

1. **Clone the repository**:
   ```bash
   git clone https://github.com/FalconCompetitions/FalconAPI.git
   cd FalconAPI/ProjetoTccBackend
   ```

2. **Configure environment variables**:
   
   Create the `.env.development` file:
   ```env
   MARIADB_ROOT_PASSWORD=your_secure_password
   MARIADB_DATABASE=falcon_dev
   ```

3. **Start containers**:
   ```bash
   docker compose -f docker-compose.development.yml up --build
   ```

4. **Configure test users** (optional):
   
   By default, the system automatically creates test users. You can customize credentials in `appsettings.Development.json`:
   ```json
   {
     "Admin": {
       "Email": "admin@gmail.com",
       "Password": "Admin#1234"
     },
     "Local": {
       "TestUsers": true,
       "TestUsersPassword": "00000000#Ra"
     }
   }
   ```
   
   **Available settings**:
   - `Admin:Email` - Admin user email (default: `admin@gmail.com`)
   - `Admin:Password` - Admin password (default: `Admin#1234`)
   - `Local:TestUsers` - Create test users? (default: `true`)
   - `Local:TestUsersPassword` - Test users password (default: `00000000#Ra`)

5. **Access the application**:
   - **Swagger**: http://localhost:8080/swagger
   - **API**: http://localhost:8080
   - **HTTPS**: https://localhost:7163

6. **Automatically created users** (if `Local:TestUsers` = `true`):
   
   **Admin**:
   - Email: Configurable in `Admin:Email` (default: `admin@gmail.com`)
   - Password: Configurable in `Admin:Password` (default: `Admin#1234`)
   - Role: `Admin`
   
   **Teachers** (4 users):
   - Emails: `professor1@gmail.com` to `professor4@gmail.com`
   - Password: Configurable in `Local:TestUsersPassword` (default: `00000000#Ra`)
   - Role: `Teacher`
   - Names: João, Álvaro, Manuel, Renato Coach
   
   **Students** (4 users):
   - Emails: `aluno1@gmail.com` to `aluno4@gmail.com`
   - Password: Configurable in `Local:TestUsersPassword` (default: `00000000#Ra`)
   - Role: `Student`
   - Names: Diego Júnior, Canário Arregaçado, Roberto, Coach Júnior

### Option 2: Run Locally without Docker

1. **Make sure you have SQL Server or MariaDB installed**

2. **Configure the connection string** in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=falcon-dev;User ID=sa;Password=YourPassword;TrustServerCertificate=True;"
     },
     "Admin": {
       "Email": "admin@gmail.com",
       "Password": "Admin#1234"
     },
     "Local": {
       "TestUsers": true,
       "TestUsersPassword": "00000000#Ra"
     }
   }
   ```
   
   > **💡 Tip**: Set `Local:TestUsers` to `false` in production to disable automatic test user creation.

3. **Run migrations**:
   ```bash
   dotnet ef database update
   ```

4. **Start the application**:
   ```bash
   dotnet run
   ```
   
   Or use the PowerShell script:
   ```powershell
   .\run.ps1
   ```

5. **Access Swagger**: https://localhost:7163/swagger

### Test Users Configuration

The system allows test user configuration through the `appsettings.Development.json` or `appsettings.json` file:

#### Available Settings

| Setting | Description | Default | Environment |
|---------|-------------|---------|-------------|
| `Admin:Email` | Admin user email | `admin@gmail.com` | All |
| `Admin:Password` | Admin password | `Admin#1234` | All |
| `Local:TestUsers` | Automatically create test users | `true` | Development/Test |
| `Local:TestUsersPassword` | Default password for all test users | `00000000#Ra` | Development/Test |

#### Automatically Created Users

When `Local:TestUsers` is set to `true`, the following users are created at startup:

**Administrator** (1 user):
- Email: Configurable via `Admin:Email`
- Password: Configurable via `Admin:Password`
- RA: `999999`
- Role: `Admin`

**Teachers** (4 users):
- Fixed emails: `professor1@gmail.com`, `professor2@gmail.com`, `professor3@gmail.com`, `professor4@gmail.com`
- Password: Configurable via `Local:TestUsersPassword`
- RAs: `222222`, `222223`, `222224`, `222225`
- Role: `Teacher`

**Students** (4 users):
- Fixed emails: `aluno1@gmail.com`, `aluno2@gmail.com`, `aluno3@gmail.com`, `aluno4@gmail.com`
- Password: Configurable via `Local:TestUsersPassword`
- RAs: `111111`, `111112`, `111113`, `111114`
- Role: `Student`

#### Security Recommendations

⚠️ **IMPORTANT**: 
- In **production**, set `Local:TestUsers` to `false` in `appsettings.json`
- Change `Admin:Password` to a strong and secure password
- Use **User Secrets** or **Azure Key Vault** to store sensitive credentials
- Never commit real passwords to the Git repository

**Production configuration example**:
```json
{
  "Admin": {
    "Email": "admin@yourcompany.com",
    "Password": "YourStrongPasswordHere!@#123"
  },
  "Local": {
    "TestUsers": false
  }
}
```

### Run Migrations Manually

```bash
# Apply all migrations
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName

# Revert last migration
dotnet ef database update MigrationName
```

## ☁️ Azure Deployment and Configuration

The project is configured for deployment to **Azure App Service**. See the [AZURE_CONFIGURATION.md](AZURE_CONFIGURATION.md) file for detailed instructions on:

- Environment variable configuration
- Frontend CORS setup
- WebSocket and Session Affinity enablement
- Common issue troubleshooting
- Logging and monitoring

### Required Environment Variables (Azure)

```
JudgeApi__Url=https://your-judge-api-url/v0
JudgeApi__SecurityKey=your-security-key
Cors__FrontendURL=https://your-frontend.azurewebsites.net
ConnectionStrings__DefaultConnection=your-sql-server-connection-string
Jwt__Key=your-jwt-key
Jwt__Issuer=System
Jwt__Audience=System
```

> **⚠️ Production User Configuration:**
> - Set `Local__TestUsers=false` to **disable** automatic test user creation
> - Configure `Admin__Email` and `Admin__Password` with **secure** credentials (different from defaults)
> - Default passwords (`00000000#Ra`, `Admin#1234`) must be changed in production
> - See [Test Users Configuration](#test-users-configuration) section for more details

## 🌐 API and Endpoints

### Authentication (`/api/Auth`)

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| POST | `/api/Auth/register` | Register new user | Public |
| POST | `/api/Auth/login` | User login | Public |
| POST | `/api/Auth/logout` | User logout | Authenticated |

### Users (`/api/User`)

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/User` | List users (paginated) | Authenticated |
| GET | `/api/User/{id}` | Find user by ID | Authenticated |
| GET | `/api/User/role/{role}` | Filter by role | Admin, Teacher |
| PUT | `/api/User/{id}` | Update user | Own user or Admin |
| DELETE | `/api/User/{id}` | Delete user | Admin |

### Competitions (`/api/Competition`)

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/Competition` | List competitions | Authenticated |
| GET | `/api/Competition/{id}` | Find competition | Authenticated |
| POST | `/api/Competition` | Create competition | Admin, Teacher |
| PUT | `/api/Competition/{id}` | Update competition | Admin, Teacher |
| DELETE | `/api/Competition/{id}` | Delete competition | Admin |
| POST | `/api/Competition/{id}/start` | Start competition | Admin, Teacher |
| POST | `/api/Competition/{id}/stop` | End competition | Admin, Teacher |

### Exercises (`/api/Exercise`)

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/Exercise` | List exercises | Authenticated |
| GET | `/api/Exercise/{id}` | Find exercise | Authenticated |
| POST | `/api/Exercise` | Create exercise | Admin, Teacher |
| PUT | `/api/Exercise/{id}` | Update exercise | Admin, Teacher |
| DELETE | `/api/Exercise/{id}` | Delete exercise | Admin, Teacher |

### Groups (`/api/Group`)

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/Group` | List groups | Authenticated |
| GET | `/api/Group/{id}` | Find group | Authenticated |
| POST | `/api/Group` | Create group | Student |
| PUT | `/api/Group/{id}` | Update group | Group leader |
| DELETE | `/api/Group/{id}` | Delete group | Leader or Admin |
| POST | `/api/Group/{id}/invite` | Invite member | Group leader |
| POST | `/api/Group/invite/{inviteId}/accept` | Accept invitation | Student |

### Logs (`/api/Log`)

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/api/Log` | List logs (paginated) | Admin |
| GET | `/api/Log/{id}` | Find log by ID | Admin |
| GET | `/api/Log/user/{userId}` | Logs by user | Admin or own user |
| GET | `/api/Log/competition/{competitionId}` | Logs by competition | Admin, Teacher |
| GET | `/api/Log/group/{groupId}` | Logs by group | Admin, Teacher, Leader |

### Tokens (`/api/Token`)

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| POST | `/api/Token/refresh` | Renew token | Authenticated |

### Files (`/api/File`)

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| POST | `/api/File/upload` | File upload | Authenticated |
| GET | `/api/File/{id}` | File download | Authenticated |
| DELETE | `/api/File/{id}` | Delete file | Admin, Teacher |

## 🔌 SignalR and Real-Time Communication

The system uses **SignalR** for real-time communication during competitions. See [SIGNALR_COMPETITION_HUB_DOCUMENTATION.md](SIGNALR_COMPETITION_HUB_DOCUMENTATION.md) for complete documentation.

### SignalR Endpoint
```
/hub/competition
```

### Main Methods

#### Client Methods (Invoked by frontend)

| Method | Description | Authorization |
|--------|-------------|---------------|
| `GetAllCompetitionQuestions` | Fetch all questions | All |
| `GetCompetitionRanking` | Fetch complete ranking | All |
| `SendExerciseAttempt` | Submit code | Student |
| `SendCompetitionQuestion` | Send question | Student |
| `AnswerQuestion` | Answer question | Admin, Teacher |
| `ChangeJudgeSubmissionResponse` | Change submission result | Admin, Teacher |
| `BlockGroupSubmission` | Block group | Admin, Teacher |
| `Ping` | Health check | All |

#### Server Events (Received by frontend)

| Event | Description | Recipients |
|-------|-------------|------------|
| `OnConnectionResponse` | Initial competition data | Connected client |
| `ReceiveRankingUpdate` | Ranking update | All |
| `ReceiveExerciseAttemptResponse` | Submission result | Student who submitted |
| `ReceiveExerciseAttempt` | Submission notification | Admin, Teacher |
| `ReceiveQuestionCreation` | New question | Admin, Teacher |
| `ReceiveQuestionAnswer` | New answer | Admin, Teacher |
| `Pong` | Ping response | Client who sent ping |

## ⚙️ Workers and Queue System

### ExerciseSubmissionWorker

Responsible for processing code submissions asynchronously:

- **Parallel processing**: Up to 8 simultaneous submissions
- **Judge API integration**: Automatic code evaluation
- **Ranking update**: Notifies all participants via SignalR
- **Automatic retry**: Reprocesses temporary failures
- **Detailed logging**: Complete process tracking

**Processing flow**:
1. Submission is added to queue via `SendExerciseAttempt`
2. Worker consumes item from queue
3. Sends code to Judge API
4. Awaits evaluation result
5. Updates database
6. Recalculates ranking
7. Notifies users via SignalR

### CompetitionStateWorker

Automatically manages competition states:

- **Continuous monitoring**: Checks states every 5-20 seconds
- **Automatic transitions**: Starts and ends competitions at configured times
- **Adaptive mode**: Shorter interval during active competitions
- **Memory cache**: Reduces database queries

### Queue System

```csharp
public class ExerciseSubmissionQueue
{
    private readonly ConcurrentQueue<ExerciseSubmissionQueueItem> _queue;
    
    public void Enqueue(ExerciseSubmissionQueueItem item);
    public bool TryDequeue(out ExerciseSubmissionQueueItem item);
    public int Count { get; }
}
```

## 🧪 Tests

### Integration Tests

Located in `ProjetoTccBackend.Integration.Test/`:

```bash
# Run all integration tests
dotnet test ProjetoTccBackend.Integration.Test/

# Run specific test
dotnet test --filter "FullyQualifiedName~Exercise_GET"
```

**Available tests**:
- `Exercise_GET.cs` - Exercise search tests
- `Exercise_POST.cs` - Exercise creation tests
- `UserAuth_POST.cs` - Authentication tests

### Unit Tests

Located in `ProjetoTCCBackend.Unit.Test/`:

```bash
# Run all unit tests
dotnet test ProjetoTCCBackend.Unit.Test/

# With code coverage
dotnet test /p:CollectCoverage=true
```

**Available tests**:
- `CompetitionRankingServiceTests.cs` - Ranking calculation tests
  - ✅ Penalty calculation
  - ✅ Point counting
  - ✅ Ranking sorting
  - ✅ Multiple attempts

### Run All Tests

```bash
# All solution tests
dotnet test ProjetoTccBackend.sln

# With detailed report
dotnet test --logger "console;verbosity=detailed"
```

## 📚 Additional Documentation

- **[AZURE_CONFIGURATION.md](AZURE_CONFIGURATION.md)** - Complete Azure deployment guide
- **[SIGNALR_COMPETITION_HUB_DOCUMENTATION.md](SIGNALR_COMPETITION_HUB_DOCUMENTATION.md)** - Detailed SignalR Hub documentation
- **[LICENSE.txt](LICENSE.txt)** - Project license
- **Swagger UI** - Interactive documentation at `/swagger` (development only)

## 🔒 Security

- **JWT tokens** with configurable expiration
- **HTTPS** mandatory in production
- **CORS** configured for specific origins
- **Input validation** on all endpoints
- **SQL Injection** protected by Entity Framework
- **XSS** protected by data sanitization
- **Rate limiting** (configurable)
- **Secrets** managed via User Secrets and Azure Key Vault

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## 👥 Authors

- **Falcon Competitions Team** - [FalconCompetitions](https://github.com/FalconCompetitions)
