# Projeto TCC - FalconAPI

Este repositório contém o backend da aplicação de competição de programação para o Trabalho de Conclusão de Curso (TCC). O sistema é desenvolvido em .NET 8, utiliza MariaDB como banco de dados e segue arquitetura baseada em repositórios, serviços, controllers e comunicação em tempo real via SignalR.

## Tecnologias Utilizadas

- .NET 8 (ASP.NET Core)
- Entity Framework Core
- MariaDB
- SignalR (para comunicação em tempo real)
- Docker e Docker Compose
- Swagger/OpenAPI para documentação
- JWT para autenticação

## Estrutura do Projeto

- **Controllers/**: Endpoints da API REST (ex: Auth, User, Competition, Exercise, Log, Group)
- **Models/**: Modelos de dados e entidades do banco
- **Repositories/**: Implementação dos padrões de acesso a dados
- **Services/**: Regras de negócio e lógica da aplicação
- **Hubs/**: Comunicação em tempo real via SignalR (CompetitionHub)
- **Middlewares/**: Tratamento de exceções e funcionalidades globais
- **Enums/**: Tipos e status utilizados em entidades e regras de negócio
- **Swagger/**: Exemplos e filtros para documentação dos endpoints
- **Migrations/**: Controle de versão do banco de dados via Entity Framework
- **Workers/**: Implementação de workers para processamento assíncrono e sistema de fila
- **ProjetoTccBackend.Integration.Test/**: Testes de integração automatizados dos principais fluxos da API
- **ProjetoTCCBackend.Unit.Test/**: Testes unitários

## Funcionalidades Principais

- **Autenticação e Autorização**: Usuários podem se registrar, autenticar e renovar tokens JWT. Roles suportadas: Admin, Teacher, Student
- **Gestão de Usuários**: CRUD de usuários, filtragem por role, atualização de perfil
- **Gestão de Competições**: Criação, consulta e controle de competições
- **Gestão de Exercícios**: CRUD de exercícios, inputs/outputs, tipos e submissões
- **Gestão de Grupos**: Criação e gerenciamento de grupos de estudantes
- **Log de Ações**: Registro de ações relevantes (login, logout, tentativas, etc) com endpoints para consulta paginada por usuário, grupo ou competição
- **Comunicação em Tempo Real**: SignalR Hub para envio de tentativas, perguntas e respostas durante a competição, com separação por roles e grupos
- **Processamento Assíncrono e Sistema de Fila**: Utilização de workers para processamento de tarefas em segundo plano, como avaliação de exercícios, envio de notificações e outras rotinas que exigem escalabilidade e não bloqueiam o fluxo principal da API. O sistema de fila garante que tarefas sejam processadas de forma eficiente e resiliente.
- **Tratamento de Erros**: Middleware para tratamento global de exceções e validação de modelos
- **Documentação via Swagger**: Endpoints documentados e exemplos de payloads disponíveis em `/swagger`
## Workers e Sistema de Fila

O projeto implementa um sistema de fila para processamento assíncrono de tarefas, utilizando a pasta `Workers/`. Os workers são responsáveis por executar rotinas que não devem bloquear o fluxo principal da API, como:

- Avaliação automática de exercícios submetidos pelos usuários
- Envio de notificações e atualizações em tempo real
- Processamento de logs e auditoria

O sistema de fila pode ser implementado com bibliotecas como `BackgroundService` do .NET ou integrações com sistemas externos (ex: RabbitMQ, Redis, etc), conforme a necessidade do projeto. Os workers consomem tarefas da fila e executam o processamento em segundo plano, garantindo escalabilidade e melhor desempenho.

Exemplo de uso:
- Ao submeter uma resposta de exercício, a API adiciona uma tarefa na fila para avaliação automática.
- O worker consome essa tarefa, executa a avaliação e atualiza o status do exercício, podendo notificar o usuário via SignalR.

Consulte a pasta `Workers/` para detalhes de implementação e exemplos de workers disponíveis.

## Como executar localmente

### Pré-requisitos

- Docker e Docker Compose instalados
- .NET 8 SDK instalado (opcional para desenvolvimento fora do container)

### Subindo o ambiente com Docker Compose

1. Clone o repositório e acesse a pasta do projeto backend.
2. Configure o arquivo `.env.development` com as variáveis de ambiente necessárias, por exemplo:
   ```
   MARIADB_ROOT_PASSWORD=suasenha
   MARIADB_DATABASE=projetotcc
   ```
3. Execute o comando para subir os containers:
   ```
   docker compose -f docker-compose.development.yml up --build
   ```
4. Acesse o Swagger da API para testar os endpoints:
   - HTTP: [http://localhost:8080/swagger](http://localhost:8080/swagger)
   - HTTPS (se configurado): [https://localhost:7163/swagger](https://localhost:7163/swagger)

### Executando migrations do banco

As migrations são aplicadas automaticamente ao iniciar a API (veja `Program.cs`). Para aplicar manualmente:
   ```
   dotnet ef database update
   ```

## Principais Endpoints

- `/api/Auth` - Autenticação de usuários (login, registro, renovação de token, logout)
- `/api/User` - Gerenciamento de usuários (consulta, atualização, filtragem por role)
- `/api/Competition` - Gerenciamento de competições
- `/api/Exercise` - Gerenciamento de exercícios
- `/api/Log` - Consulta de logs de ações
- `/api/Group` - Gerenciamento de grupos
- `/hub/competition` - Comunicação em tempo real via SignalR

## Integração e Testes

- O projeto inclui testes de integração em `ProjetoTccBackend.Integration.Test` para validação dos principais fluxos da API.
- Para rodar os testes:
   ```
   dotnet test ProjetoTccBackend.Integration.Test/ProjetoTccBackend.Integration.Test.csproj
   ```
- Também há testes unitários em `ProjetoTCCBackend.Unit.Test`.

## Observações Importantes

- O ambiente de desenvolvimento é definido pela variável `ASPNETCORE_ENVIRONMENT=Development` no Docker Compose.
- O backend está preparado para rodar em ambiente Docker, mas pode ser executado localmente via Visual Studio ou CLI do .NET.
- O banco de dados é inicializado automaticamente e as migrations podem ser aplicadas na inicialização da API.
- O sistema implementa separação de roles (Admin, Teacher, Student) tanto nos endpoints quanto no SignalR Hub.
- O log de ações permite auditoria detalhada das operações dos usuários.
- O Swagger está disponível apenas em ambiente de desenvolvimento.
- O projeto utiliza JWT para autenticação e cookies para integração com o frontend.
- O backend implementa tratamento global de erros e validação de modelos.
- O sistema possui endpoints para consulta paginada e filtrada de usuários, logs, exercícios e competições.

---
