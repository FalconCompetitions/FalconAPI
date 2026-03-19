# Projeto TCC - FalconAPI

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED)](https://www.docker.com/)

Este repositório contém o backend da aplicação **Falcon Competition**, uma plataforma completa de competições de programação desenvolvida como Trabalho de Conclusão de Curso (TCC). O sistema oferece infraestrutura robusta para competições em tempo real, com avaliação automática de código, comunicação via WebSocket e gerenciamento completo de usuários, grupos e exercícios.

**[🇺🇸 English Version](README.md)**

## 📋 Índice

- [Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [Arquitetura do Sistema](#-arquitetura-do-sistema)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Funcionalidades Principais](#-funcionalidades-principais)
- [Como Executar Localmente](#-como-executar-localmente)
  - [Configuração de Usuários de Teste](#configuração-de-usuários-de-teste)
- [Deploy e Configuração Azure](#-deploy-e-configuração-azure)
- [API e Endpoints](#-api-e-endpoints)
- [SignalR e Comunicação em Tempo Real](#-signalr-e-comunicação-em-tempo-real)
- [Sistema de Workers e Filas](#-sistema-de-workers-e-filas)
- [Testes](#-testes)
- [Documentação Adicional](#-documentação-adicional)

## 🚀 Tecnologias Utilizadas

### Framework e Runtime
- **.NET 10** (ASP.NET Core) - Framework principal
- **C# 13** - Linguagem de programação
- **Entity Framework Core 10.0** - ORM para acesso a dados

### Banco de Dados
- **SQL Server** (Produção - Azure)
- **MariaDB 11** (Desenvolvimento local)
- **In-Memory Database** (Testes)

### Autenticação e Segurança
- **ASP.NET Core Identity** - Gerenciamento de usuários e roles
- **JWT Bearer Authentication** - Autenticação baseada em tokens
- **Cookie Authentication** - Integração com frontend

### Comunicação em Tempo Real
- **SignalR** - WebSocket para comunicação bidirecional
- **JSON Protocol** - Serialização de mensagens

### Infraestrutura
- **Docker** e **Docker Compose** - Containerização
- **Azure App Service** - Hospedagem em nuvem
- **Serilog** - Logging estruturado e rastreamento

### Documentação e Qualidade
- **Swagger/OpenAPI 3.1** - Documentação interativa da API
- **xUnit** - Framework de testes
- **Moq** - Biblioteca de mocking para testes

### Dependências Principais
```xml
Microsoft.EntityFrameworkCore.SqlServer (10.0.0)
Microsoft.AspNetCore.Identity.EntityFrameworkCore (10.0.0)
Microsoft.AspNetCore.Authentication.JwtBearer (10.0.0)
Microsoft.AspNetCore.SignalR (10.0.0)
Swashbuckle.AspNetCore (10.0.1)
Serilog.AspNetCore (9.0.0)
Microsoft.OpenApi (2.3.0)
```

## 🏗️ Arquitetura do Sistema

O projeto segue uma **arquitetura em camadas** com separação clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────┐
│                    Controllers Layer                     │
│          (API REST Endpoints + SignalR Hubs)            │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                    Services Layer                        │
│        (Lógica de Negócio + Regras de Validação)       │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  Repositories Layer                      │
│           (Acesso a Dados + Abstrações EF)              │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  Database Layer                          │
│         (SQL Server / MariaDB / In-Memory)              │
└─────────────────────────────────────────────────────────┘

        ┌──────────────────────────────────┐
        │     Background Workers           │
        │  (Processamento Assíncrono)     │
        └──────────────────────────────────┘
```

### Padrões de Design Utilizados
- **Repository Pattern** - Abstração de acesso a dados
- **Dependency Injection** - Inversão de controle
- **Service Layer Pattern** - Encapsulamento de lógica de negócio
- **Middleware Pipeline** - Tratamento global de requisições
- **Background Services** - Processamento assíncrono com filas

## 📁 Estrutura do Projeto

```
FalconAPI/
├── ProjetoTccBackend/                     # Projeto principal da API
│   ├── Controllers/                       # Endpoints REST
│   │   ├── AuthController.cs             # Autenticação (login, registro)
│   │   ├── UserController.cs             # Gerenciamento de usuários
│   │   ├── CompetitionController.cs      # Competições
│   │   ├── ExerciseController.cs         # Exercícios e submissões
│   │   ├── GroupController.cs            # Grupos de estudantes
│   │   ├── LogController.cs              # Logs e auditoria
│   │   ├── QuestionController.cs         # Perguntas e respostas
│   │   ├── FileController.cs             # Upload/download de arquivos
│   │   └── TokenController.cs            # Renovação de tokens
│   │
│   ├── Hubs/                             # SignalR Hubs
│   │   └── CompetitionHub.cs             # Hub de competições em tempo real
│   │
│   ├── Services/                         # Camada de lógica de negócio
│   │   ├── Interfaces/                   # Contratos de serviços
│   │   ├── UserService.cs
│   │   ├── CompetitionService.cs
│   │   ├── ExerciseService.cs
│   │   ├── GroupService.cs
│   │   ├── JudgeService.cs              # Integração com Judge API
│   │   ├── TokenService.cs              # Geração e validação JWT
│   │   ├── LogService.cs
│   │   ├── CompetitionRankingService.cs
│   │   ├── GroupAttemptService.cs
│   │   └── ...
│   │
│   ├── Repositories/                     # Camada de acesso a dados
│   │   ├── Interfaces/                   # Contratos de repositórios
│   │   ├── GenericRepository.cs          # Repositório base genérico
│   │   ├── UserRepository.cs
│   │   ├── CompetitionRepository.cs
│   │   ├── ExerciseRepository.cs
│   │   ├── GroupRepository.cs
│   │   └── ...
│   │
│   ├── Workers/                          # Background Services
│   │   ├── ExerciseSubmissionWorker.cs  # Processamento de submissões
│   │   ├── CompetitionStateWorker.cs    # Gerenciamento de estados
│   │   └── Queues/                      # Sistema de filas
│   │       └── ExerciseSubmissionQueue.cs
│   │
│   ├── Models/                           # Entidades do domínio
│   │   ├── User.cs
│   │   ├── Competition.cs
│   │   ├── Exercise.cs
│   │   ├── Group.cs
│   │   ├── GroupExerciseAttempt.cs
│   │   └── ...
│   │
│   ├── Database/                         # DTOs e DbContext
│   │   ├── TccDbContext.cs              # Contexto do EF Core
│   │   ├── Requests/                    # DTOs de entrada
│   │   └── Responses/                   # DTOs de saída
│   │
│   ├── Middlewares/                      # Middleware customizados
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── RequestBodyLoggingMiddleware.cs
│   │
│   ├── Filters/                          # Action Filters
│   │   └── ValidateModelStateFilter.cs
│   │
│   ├── Swagger/                          # Configuração Swagger
│   │   ├── Examples/                     # Exemplos de payloads
│   │   ├── Filters/                      # Filtros customizados
│   │   └── Extensions/
│   │
│   ├── Enums/                            # Enumerações
│   │   ├── Competition/
│   │   ├── Exercise/
│   │   ├── Judge/
│   │   └── ...
│   │
│   ├── Exceptions/                       # Exceções customizadas
│   │   ├── Judge/
│   │   ├── User/
│   │   ├── Group/
│   │   └── ...
│   │
│   ├── Migrations/                       # Migrações EF Core
│   ├── Validation/                       # Validadores
│   ├── UserUploads/                      # Arquivos de usuários
│   │
│   ├── Program.cs                        # Entry point da aplicação
│   ├── ProjetoTccBackend.csproj
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Dockerfile
│   └── docker-compose.development.yml
│
├── ProjetoTccBackend.Integration.Test/   # Testes de integração
│   ├── Exercise_GET.cs
│   ├── Exercise_POST.cs
│   ├── UserAuth_POST.cs
│   ├── TCCWebApplicationFactory.cs
│   └── DataBuilders/
│
├── ProjetoTCCBackend.Unit.Test/          # Testes unitários
│   ├── Services/
│   │   └── CompetitionRankingServiceTests.cs
│   └── ProjetoTCCBackend.Unit.Test.csproj
│
├── README.md                             # Documentação em inglês
├── README.pt-br.md                       # Documentação em português
├── AZURE_CONFIGURATION.md                # Guia de deploy Azure
├── SIGNALR_COMPETITION_HUB_DOCUMENTATION.md
└── LICENSE.txt
```

## ✨ Funcionalidades Principais

### 🔐 Autenticação e Autorização
- **Registro de usuários** com validação de dados
- **Login** com geração de JWT token
- **Renovação automática** de tokens (refresh token)
- **Logout** com invalidação de sessão
- **Sistema de roles**: Admin, Teacher, Student
- **Autenticação via cookie** para integração com frontend
- **Proteção de endpoints** baseada em roles

### 👥 Gestão de Usuários
- CRUD completo de usuários
- Filtragem por role (Admin, Teacher, Student)
- Consulta paginada com ordenação
- Atualização de perfil
- Associação a grupos
- Histórico de login e atividades

### 🏆 Gestão de Competições
- Criação e configuração de competições
- Controle de períodos (inscrição, início, fim)
- Configuração de regras (penalidades, limites)
- Gerenciamento de exercícios por competição
- Sistema de ranking em tempo real
- Estados da competição (Não Iniciada, Em Progresso, Finalizada)
- Bloqueio de submissões por grupo

### 📝 Gestão de Exercícios
- CRUD de exercícios de programação
- Upload de arquivos anexos (PDFs, imagens)
- Definição de casos de teste (inputs/outputs)
- Tipos de exercícios customizáveis
- Integração com Judge API para avaliação automática
- Suporte a múltiplas linguagens de programação

### 👨‍👩‍👦 Gestão de Grupos
- Criação de grupos de estudantes
- Sistema de convites com aprovação
- Líderes de grupo com permissões especiais
- Histórico de participação em competições
- Controle de número máximo de membros

### 💬 Sistema de Perguntas e Respostas
- Estudantes podem fazer perguntas durante competições
- Perguntas podem ser gerais ou sobre exercícios específicos
- Professores e admins recebem notificações em tempo real
- Respostas públicas ou privadas
- Histórico completo de Q&A por competição

### 📊 Logs e Auditoria
- Registro detalhado de todas as ações relevantes
- Consulta paginada por usuário, grupo ou competição
- Rastreamento de login/logout
- Log de tentativas de exercícios
- Registro de bloqueios e alterações administrativas
- Informações de IP e timestamp

### ⚡ Comunicação em Tempo Real (SignalR)
- **WebSocket** para comunicação bidirecional
- **Separação por grupos** (Admin, Teacher, Student)
- **Eventos em tempo real**:
  - Atualizações de ranking
  - Submissões de exercícios
  - Perguntas e respostas
  - Notificações de competições
- **Reconexão automática**
- **Health checks** (Ping/Pong)

### 🔄 Processamento Assíncrono
- **ExerciseSubmissionWorker**: Avaliação de código em background
- **CompetitionStateWorker**: Gerenciamento de estados de competições
- **Sistema de filas** thread-safe para submissões
- **Processamento paralelo** (até 8 submissões simultâneas)
- **Retry logic** para falhas temporárias

### 🛡️ Tratamento de Erros
- **Middleware global** para captura de exceções
- **Respostas padronizadas** de erro
- **Validação automática** de modelos
- **Logging estruturado** com Serilog
- **Mensagens de erro** claras e descritivas

### 📖 Documentação Automática
- **Swagger UI** interativo em `/swagger`
- **OpenAPI 3.1** specification
- **Exemplos de payloads** para todos os endpoints
- **Schemas** detalhados de Request/Response
- Disponível apenas em ambiente de desenvolvimento

## 🔧 Como Executar Localmente

### Pré-requisitos

- **Docker** e **Docker Compose** instalados
- **.NET 10 SDK** (opcional para desenvolvimento sem Docker)
- **Visual Studio 2022** ou **VS Code** (recomendado)

### Opção 1: Executar com Docker Compose (Recomendado)

1. **Clone o repositório**:
   ```bash
   git clone https://github.com/FalconCompetitions/FalconAPI.git
   cd FalconAPI/ProjetoTccBackend
   ```

2. **Configure as variáveis de ambiente**:
   
   Crie o arquivo `.env.development`:
   ```env
   MSSQL_SA_PASSWORD=sua_senha_segura
   ```

3. **Inicie os containers**:
   ```bash
   docker-compose --env-file .env.development --file .\docker-compose.development.yml up -d
   ```

4. **Configure usuários de teste** (opcional):
   
   Por padrão, o sistema cria automaticamente usuários de teste. Você pode personalizar as credenciais em `appsettings.Development.json`:
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
   
   **Configurações disponíveis**:
   - `Admin:Email` - Email do usuário administrador (padrão: `admin@gmail.com`)
   - `Admin:Password` - Senha do administrador (padrão: `Admin#1234`)
   - `Local:TestUsers` - Criar usuários de teste? (padrão: `true`)
   - `Local:TestUsersPassword` - Senha dos usuários de teste (padrão: `00000000#Ra`)

5. **Acesse a aplicação**:
   - **Swagger**: http://localhost:7163/swagger
   - **API**: http://localhost:7163
   - **HTTPS**: https://localhost:7163

6. **Usuários criados automaticamente** (se `Local:TestUsers` = `true`):
   
   **Admin**:
   - Email: Configurável em `Admin:Email` (padrão: `admin@gmail.com`)
   - Senha: Configurável em `Admin:Password` (padrão: `Admin#1234`)
   - Role: `Admin`
   
   **Professores** (4 usuários):
   - Emails: `professor1@gmail.com` até `professor4@gmail.com`
   - Senha: Configurável em `Local:TestUsersPassword` (padrão: `00000000#Ra`)
   - Role: `Teacher`
   - Nomes: João, Álvaro, Manuel, Renato Coach
   
   **Estudantes** (4 usuários):
   - Emails: `aluno1@gmail.com` até `aluno4@gmail.com`
   - Senha: Configurável em `Local:TestUsersPassword` (padrão: `00000000#Ra`)
   - Role: `Student`
   - Nomes: Diego Júnior, Canário Arregaçado, Roberto, Coach Júnior

### Opção 2: Executar Localmente sem Docker

1. **Certifique-se de ter o SQL Server ou MariaDB instalado**

2. **Configure a connection string** em `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=falcon-dev;User ID=sa;Password=SuaSenha;TrustServerCertificate=True;"
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
   
   > **💡 Dica**: Configure `Local:TestUsers` como `false` em produção para desabilitar a criação automática de usuários de teste.

3. **Execute as migrations**:
   ```bash
   dotnet ef database update
   ```

4. **Inicie a aplicação**:
   ```bash
   dotnet run
   ```
   
   Ou use o script PowerShell:
   ```powershell
   .\run.ps1
   ```

5. **Acesse o Swagger**: https://localhost:7163/swagger

### Configuração de Usuários de Teste

O sistema permite a configuração de usuários de teste através do arquivo `appsettings.Development.json` ou `appsettings.json`:

#### Configurações Disponíveis

| Configuração | Descrição | Padrão | Ambiente |
|-------------|-----------|--------|----------|
| `Admin:Email` | Email do usuário administrador | `admin@gmail.com` | Todos |
| `Admin:Password` | Senha do administrador | `Admin#1234` | Todos |
| `Local:TestUsers` | Criar usuários de teste automaticamente | `true` | Development/Test |
| `Local:TestUsersPassword` | Senha padrão para todos os usuários de teste | `00000000#Ra` | Development/Test |

#### Usuários Criados Automaticamente

Quando `Local:TestUsers` está configurado como `true`, os seguintes usuários são criados na inicialização:

**Administrador** (1 usuário):
- Email: Configurável via `Admin:Email`
- Senha: Configurável via `Admin:Password`
- RA: `999999`
- Role: `Admin`

**Professores** (4 usuários):
- Emails fixos: `professor1@gmail.com`, `professor2@gmail.com`, `professor3@gmail.com`, `professor4@gmail.com`
- Senha: Configurável via `Local:TestUsersPassword`
- RAs: `222222`, `222223`, `222224`, `222225`
- Role: `Teacher`

**Estudantes** (4 usuários):
- Emails fixos: `aluno1@gmail.com`, `aluno2@gmail.com`, `aluno3@gmail.com`, `aluno4@gmail.com`
- Senha: Configurável via `Local:TestUsersPassword`
- RAs: `111111`, `111112`, `111113`, `111114`
- Role: `Student`

#### Recomendações de Segurança

⚠️ **IMPORTANTE**: 
- Em **produção**, defina `Local:TestUsers` como `false` no `appsettings.json`
- Altere `Admin:Password` para uma senha forte e segura
- Use **User Secrets** ou **Azure Key Vault** para armazenar credenciais sensíveis
- Nunca commite senhas reais no repositório Git

**Exemplo de configuração para produção**:
```json
{
  "Admin": {
    "Email": "admin@suaempresa.com",
    "Password": "SuaSenhaForteAqui!@#123"
  },
  "Local": {
    "TestUsers": false
  }
}
```

### Executar Migrações Manualmente

```bash
# Aplicar todas as migrations
dotnet ef database update

# Criar nova migration
dotnet ef migrations add NomeDaMigracao

# Reverter última migration
dotnet ef database update NomeDaMigracao
```

## ☁️ Deploy e Configuração Azure

O projeto está configurado para deploy no **Azure App Service**. Consulte o arquivo [AZURE_CONFIGURATION.md](AZURE_CONFIGURATION.md) para instruções detalhadas sobre:

- Configuração de variáveis de ambiente
- Setup de CORS para frontend
- Habilitação de WebSockets e Session Affinity
- Troubleshooting de problemas comuns
- Logs e monitoramento

### Variáveis de Ambiente Obrigatórias (Azure)

```
JudgeApi__Url=https://sua-judge-api-url/v0
JudgeApi__SecurityKey=sua-chave-seguranca
Cors__FrontendURL=https://seu-frontend.azurewebsites.net
ConnectionStrings__DefaultConnection=sua-connection-string-sql-server
Jwt__Key=sua-chave-jwt
Jwt__Issuer=System
Jwt__Audience=System
```

> **⚠️ Configuração de Usuários em Produção:**
> - Defina `Local__TestUsers=false` para **desabilitar** a criação automática de usuários de teste
> - Configure `Admin__Email` e `Admin__Password` com credenciais **seguras** (diferentes dos defaults)
> - As senhas padrão (`00000000#Ra`, `Admin#1234`) devem ser alteradas em produção
> - Consulte a seção [Configuração de Usuários de Teste](#configuração-de-usuários-de-teste) para mais detalhes

## 🌐 API e Endpoints

### Autenticação (`/api/Auth`)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/Auth/register` | Registrar novo usuário | Público |
| POST | `/api/Auth/login` | Login de usuário | Público |
| POST | `/api/Auth/logout` | Logout de usuário | Autenticado |

### Usuários (`/api/User`)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/User` | Listar usuários (paginado) | Autenticado |
| GET | `/api/User/{id}` | Buscar usuário por ID | Autenticado |
| GET | `/api/User/role/{role}` | Filtrar por role | Admin, Teacher |
| PUT | `/api/User/{id}` | Atualizar usuário | Próprio usuário ou Admin |
| DELETE | `/api/User/{id}` | Deletar usuário | Admin |

### Competições (`/api/Competition`)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/Competition` | Listar competições | Autenticado |
| GET | `/api/Competition/{id}` | Buscar competição | Autenticado |
| POST | `/api/Competition` | Criar competição | Admin, Teacher |
| PUT | `/api/Competition/{id}` | Atualizar competição | Admin, Teacher |
| DELETE | `/api/Competition/{id}` | Deletar competição | Admin |
| POST | `/api/Competition/{id}/start` | Iniciar competição | Admin, Teacher |
| POST | `/api/Competition/{id}/stop` | Finalizar competição | Admin, Teacher |

### Exercícios (`/api/Exercise`)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/Exercise` | Listar exercícios | Autenticado |
| GET | `/api/Exercise/{id}` | Buscar exercício | Autenticado |
| POST | `/api/Exercise` | Criar exercício | Admin, Teacher |
| PUT | `/api/Exercise/{id}` | Atualizar exercício | Admin, Teacher |
| DELETE | `/api/Exercise/{id}` | Deletar exercício | Admin, Teacher |

### Grupos (`/api/Group`)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/Group` | Listar grupos | Autenticado |
| GET | `/api/Group/{id}` | Buscar grupo | Autenticado |
| POST | `/api/Group` | Criar grupo | Student |
| PUT | `/api/Group/{id}` | Atualizar grupo | Líder do grupo |
| DELETE | `/api/Group/{id}` | Deletar grupo | Líder ou Admin |
| POST | `/api/Group/{id}/invite` | Convidar membro | Líder do grupo |
| POST | `/api/Group/invite/{inviteId}/accept` | Aceitar convite | Student |

### Logs (`/api/Log`)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/Log` | Listar logs (paginado) | Admin |
| GET | `/api/Log/{id}` | Buscar log por ID | Admin |
| GET | `/api/Log/user/{userId}` | Logs por usuário | Admin ou próprio usuário |
| GET | `/api/Log/competition/{competitionId}` | Logs por competição | Admin, Teacher |
| GET | `/api/Log/group/{groupId}` | Logs por grupo | Admin, Teacher, Líder |

### Tokens (`/api/Token`)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/Token/refresh` | Renovar token | Autenticado |

### Arquivos (`/api/File`)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/File/upload` | Upload de arquivo | Autenticado |
| GET | `/api/File/{id}` | Download de arquivo | Autenticado |
| DELETE | `/api/File/{id}` | Deletar arquivo | Admin, Teacher |

## 🔌 SignalR e Comunicação em Tempo Real

O sistema utiliza **SignalR** para comunicação em tempo real durante competições. Consulte [SIGNALR_COMPETITION_HUB_DOCUMENTATION.md](SIGNALR_COMPETITION_HUB_DOCUMENTATION.md) para documentação completa.

### Endpoint SignalR
```
/hub/competition
```

### Principais Métodos

#### Métodos do Cliente (Invocados pelo frontend)

| Método | Descrição | Autorização |
|--------|-----------|-------------|
| `GetAllCompetitionQuestions` | Buscar todas as perguntas | Todos |
| `GetCompetitionRanking` | Buscar ranking completo | Todos |
| `SendExerciseAttempt` | Enviar submissão de código | Student |
| `SendCompetitionQuestion` | Enviar pergunta | Student |
| `AnswerQuestion` | Responder pergunta | Admin, Teacher |
| `ChangeJudgeSubmissionResponse` | Alterar resultado de submissão | Admin, Teacher |
| `BlockGroupSubmission` | Bloquear grupo | Admin, Teacher |
| `Ping` | Health check | Todos |

#### Eventos do Servidor (Recebidos pelo frontend)

| Evento | Descrição | Destinatários |
|--------|-----------|---------------|
| `OnConnectionResponse` | Dados iniciais da competição | Cliente que conectou |
| `ReceiveRankingUpdate` | Atualização de ranking | Todos |
| `ReceiveExerciseAttemptResponse` | Resultado da submissão | Estudante que submeteu |
| `ReceiveExerciseAttempt` | Notificação de submissão | Admin, Teacher |
| `ReceiveQuestionCreation` | Nova pergunta | Admin, Teacher |
| `ReceiveQuestionAnswer` | Nova resposta | Admin, Teacher |
| `Pong` | Resposta ao ping | Cliente que enviou ping |

## ⚙️ Sistema de Workers e Filas

### ExerciseSubmissionWorker

Responsável por processar submissões de código de forma assíncrona:

- **Processamento paralelo**: Até 8 submissões simultâneas
- **Integração com Judge API**: Avaliação automática de código
- **Atualização de ranking**: Notifica todos os participantes via SignalR
- **Retry automático**: Reprocessa falhas temporárias
- **Logging detalhado**: Rastreamento completo do processo

**Fluxo de processamento**:
1. Submissão é adicionada à fila via `SendExerciseAttempt`
2. Worker consome item da fila
3. Envia código para Judge API
4. Aguarda resultado da avaliação
5. Atualiza banco de dados
6. Recalcula ranking
7. Notifica usuários via SignalR

### CompetitionStateWorker

Gerencia o estado das competições automaticamente:

- **Monitoramento contínuo**: Verifica estados a cada 5-20 segundos
- **Transições automáticas**: Inicia e finaliza competições no horário configurado
- **Modo adaptativo**: Intervalo menor durante competições ativas
- **Cache em memória**: Reduz consultas ao banco

### Sistema de Filas

```csharp
public class ExerciseSubmissionQueue
{
    private readonly ConcurrentQueue<ExerciseSubmissionQueueItem> _queue;
    
    public void Enqueue(ExerciseSubmissionQueueItem item);
    public bool TryDequeue(out ExerciseSubmissionQueueItem item);
    public int Count { get; }
}
```

## 🧪 Testes

### Testes de Integração

Localizados em `ProjetoTccBackend.Integration.Test/`:

```bash
# Executar todos os testes de integração
dotnet test ProjetoTccBackend.Integration.Test/

# Executar teste específico
dotnet test --filter "FullyQualifiedName~Exercise_GET"
```

**Testes disponíveis**:
- `Exercise_GET.cs` - Testes de busca de exercícios
- `Exercise_POST.cs` - Testes de criação de exercícios
- `UserAuth_POST.cs` - Testes de autenticação

### Testes Unitários

Localizados em `ProjetoTCCBackend.Unit.Test/`:

```bash
# Executar todos os testes unitários
dotnet test ProjetoTCCBackend.Unit.Test/

# Com cobertura de código
dotnet test /p:CollectCoverage=true
```

**Testes disponíveis**:
- `CompetitionRankingServiceTests.cs` - Testes de cálculo de ranking
  - ✅ Cálculo de penalidades
  - ✅ Contagem de pontos
  - ✅ Ordenação de ranking
  - ✅ Tentativas múltiplas

### Executar Todos os Testes

```bash
# Todos os testes do solution
dotnet test ProjetoTccBackend.sln

# Com relatório detalhado
dotnet test --logger "console;verbosity=detailed"
```

## 📚 Documentação Adicional

- **[AZURE_CONFIGURATION.md](AZURE_CONFIGURATION.md)** - Guia completo de deploy no Azure
- **[SIGNALR_COMPETITION_HUB_DOCUMENTATION.md](SIGNALR_COMPETITION_HUB_DOCUMENTATION.md)** - Documentação detalhada do SignalR Hub
- **[LICENSE.txt](LICENSE.txt)** - Licença do projeto
- **Swagger UI** - Documentação interativa em `/swagger` (apenas desenvolvimento)

## 🔒 Segurança

- **JWT tokens** com expiração configurável
- **HTTPS** obrigatório em produção
- **CORS** configurado para origens específicas
- **Validação de entrada** em todos os endpoints
- **SQL Injection** protegido pelo Entity Framework
- **XSS** protegido por sanitização de dados
- **Rate limiting** (configurável)
- **Secrets** gerenciados via User Secrets e Azure Key Vault

## 📄 Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE.txt](LICENSE.txt) para detalhes.

## 👥 Autores

- **Equipe Falcon Competitions** - [FalconCompetitions](https://github.com/FalconCompetitions)
