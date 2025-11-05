# SignalR CompetitionHub - Documentação Completa

## Informações Gerais

**Endpoint**: `/competitionHub`  
**Autenticação**: Obrigatória (JWT Bearer Token)  
**Protocolo**: SignalR WebSocket

---

## Conexão

### OnConnectedAsync (Automático)

**Descrição**: Executado automaticamente ao conectar ao hub. Retorna os dados da competição atual.

**Autorização**: Requer autenticação (qualquer role: Admin, Teacher, Student)

**Grupos SignalR atribuídos automaticamente**:
- `Admins` - Para usuários com role Admin
- `Teachers` - Para usuários com role Teacher  
- `Students` - Para usuários com role Student
- `{userId}` - Para estudantes, também são adicionados a um grupo com seu ID

**Evento de Retorno**: `OnConnectionResponse`

**Payload de Resposta**:
```typescript
interface CompetitionResponse {
  id: number;
  name: string;
  description: string;
  startTime: string; // ISO 8601 DateTime
  endTime: string; // ISO 8601 DateTime
  startInscriptions: string; // ISO 8601 DateTime
  endInscriptions: string; // ISO 8601 DateTime
  blockSubmissions: boolean;
  stopRanking: boolean;
  submissionPenalty: number;
  maxExercises: number;
  maxMembers: number;
  maxSubmissionSize: number;
  status: number; // Enum: 0=NotStarted, 1=InProgress, 2=Finished
  duration: number; // Duração em minutos
  competitionRankings: CompetitionRankingResponse[];
  exercises: ExerciseResponse[];
}

interface CompetitionRankingResponse {
  id: number;
  group: GroupResponse;
  penalty: number;
  points: number;
  rankOrder: number;
}

interface GroupResponse {
  id: string; // UUID
  leaderId: string; // UUID
  name: string;
  users: GenericUserInfoResponse[];
}

interface GenericUserInfoResponse {
  id: string; // UUID
  email: string;
  department: null;
  createdAt: string; // ISO 8601 DateTime
  exercisesCreated: null;
  joinYear: number;
  lastLoggedAt: string; // ISO 8601 DateTime
  name: string;
  ra: string;
  group: null;
}

interface ExerciseResponse {
  id: number;
  title: string;
  attachedFileId: number;
  description: string;
  exerciseTypeId: number;
  inputs: ExerciseInputResponse[];
  outputs: ExerciseOutputResponse[];
}

interface ExerciseInputResponse {
  id: number;
  exerciseId: number;
  input: string;
}

interface ExerciseOutputResponse {
  id: number;
  output: string;
  exerciseId: number;
  exerciseInputId: number;
}
```

**Retorno quando não há competição ativa**: `null`

---

### OnDisconnectedAsync (Automático)

**Descrição**: Executado automaticamente ao desconectar do hub. Registra log de logout.

**Sem eventos de retorno ao cliente**

---

## Métodos do Hub

### 1. GetConnectionId

**Descrição**: Retorna o ID da conexão SignalR atual.

**Autorização**: Requer autenticação

**Método de Invocação**: `GetConnectionId`

**Parâmetros**: Nenhum

**Evento de Retorno**: `ReceiveConnectionId`

**Payload de Resposta**:
```typescript
string // Connection ID
```

**Exemplo de uso (TypeScript)**:
```typescript
connection.invoke('GetConnectionId');

connection.on('ReceiveConnectionId', (connectionId: string) => {
  console.log('Connection ID:', connectionId);
});
```

---

### 2. Ping

**Descrição**: Verifica se a conexão está ativa (health check).

**Autorização**: Requer autenticação

**Método de Invocação**: `Ping`

**Parâmetros**: Nenhum

**Evento de Retorno**: `Pong`

**Payload de Resposta**:
```typescript
interface PongResponse {
  message: "Pong";
}
```

**Exemplo de uso (TypeScript)**:
```typescript
connection.invoke('Ping');

connection.on('Pong', (response: { message: string }) => {
  console.log(response.message); // "Pong"
});
```

---

### 3. SendExerciseAttempt

**Descrição**: Envia uma tentativa de resolução de exercício para processamento assíncrono. O exercício é enfileirado e processado por um worker em background.

**Autorização**: Apenas role `Student`

**Método de Invocação**: `SendExerciseAttempt`

**Parâmetros de Entrada**:
```typescript
interface GroupExerciseAttemptRequest {
  groupId: number;
  exerciseId: number;
  code: string; // Código fonte da solução
  languageId: number; // ID da linguagem de programação
  competitionId: number;
}
```

**Eventos de Retorno**:
- `ReceiveExerciseAttemptResponse` - Para o estudante que submeteu (após processamento)
- `ReceiveExerciseAttemptError` - Para o estudante, caso ocorra erro na validação (antes do processamento)
- `ReceiveExerciseAttempt` - Para grupos `Admins` e `Teachers` (após processamento)
- `ReceiveRankingUpdate` - Para **todos** os participantes (`Students`, `Teachers`, `Admins`) com o ranking atualizado

**Payload de Resposta (ReceiveExerciseAttemptResponse)**:
```typescript
interface ExerciseSubmissionResponse {
  id: number;
  groupId: number;
  exerciseId: number;
  code: string;
  languageId: number;
  submittedAt: string; // ISO 8601 DateTime
  judgeResponse: number; // Enum: 0=Pending, 1=Accepted, 2=WrongAnswer, 3=TimeLimitExceeded, 4=MemoryLimitExceeded, 5=RuntimeError, 6=CompilationError
  executionTime: number; // Em milissegundos
  memoryUsed: number; // Em KB
  score: number;
  points: number; // Pontos obtidos na submissão
  penalty: number; // Penalidade aplicada
} | null // null se não houver competição ativa
```

**Payload de Erro (ReceiveExerciseAttemptError)**:
```typescript
interface ExerciseAttemptError {
  message: string; // Mensagem descritiva do erro
}
```

**Possíveis mensagens de erro**:
- `"Usuário não pertence a nenhum grupo"` - O estudante não está associado a nenhum grupo
- `"Seu grupo está bloqueado de enviar submissões nesta competição"` - O grupo foi bloqueado por um administrador ou professor

**Exemplo de uso (TypeScript)**:
```typescript
const request: GroupExerciseAttemptRequest = {
  groupId: 1,
  exerciseId: 1,
  code: 'print("Hello World")',
  languageId: 71, // Python
  competitionId: 1
};

connection.invoke('SendExerciseAttempt', request);

// Para o estudante que submeteu - Tratamento de erros (imediato, antes do processamento)
connection.on('ReceiveExerciseAttemptError', (error: { message: string }) => {
  console.error('Erro ao enviar submissão:', error.message);
  // Exibir mensagem de erro para o usuário
  alert(error.message);
});

// Para o estudante que submeteu - Resultado da avaliação (após processamento assíncrono)
connection.on('ReceiveExerciseAttemptResponse', (response: ExerciseSubmissionResponse | null) => {
  if (response) {
    console.log('Resultado da tentativa:', response);
    // Exibir resultado, atualizar UI
  }
});

// Para professores e administradores (notificação de todas as submissões)
connection.on('ReceiveExerciseAttempt', (response: ExerciseSubmissionResponse) => {
  console.log('Nova submissão processada:', response);
  // Atualizar dashboard de monitoramento, ranking em tempo real
});
```

**⚠️ Notas Importantes sobre Processamento**:
- A submissão é **processada de forma assíncrona** por um worker em background (`ExerciseSubmissionWorker`)
- O tempo de resposta depende da fila de processamento e da complexidade do exercício
- A resposta não é imediata - pode levar de segundos a minutos dependendo da carga
- O worker processa até 8 submissões em paralelo
- O worker executa em loop contínuo, verificando a fila a cada 1 segundo quando não há itens
- Admins e Teachers recebem notificações de **todas** as submissões processadas, não apenas as suas
- O sistema utiliza Entity Framework Core com eager loading (`.Include()`) para garantir que todos os dados necessários sejam carregados, evitando erros de lazy loading

**⚠️ Validações Realizadas (antes do enfileiramento)**:
- Verifica se existe uma competição ativa
- Verifica se o usuário pertence a um grupo
- Verifica se o grupo está bloqueado na competição atual
- Se qualquer validação falhar, o evento `ReceiveExerciseAttemptError` é disparado imediatamente

---

### 4. SendCompetitionQuestion

**Descrição**: Envia uma pergunta/dúvida durante a competição. A pergunta é enviada para todos os professores e administradores conectados.

**Autorização**: Apenas role `Student`

**Método de Invocação**: `SendCompetitionQuestion`

**Parâmetros de Entrada**:
```typescript
interface CreateGroupQuestionRequest {
  groupId: number;
  competitionId: number;
  exerciseId?: number; // Opcional - se a pergunta é sobre um exercício específico
  questionText: string;
}
```

**Eventos de Retorno**:
- `ReceiveQuestionCreationResponse` - Para o autor da pergunta
- `ReceiveQuestionCreation` - Para grupos `Teachers` e `Admins`

**Payload de Resposta**:
```typescript
interface QuestionResponse {
  id: number;
  competitionId: number;
  exerciseId?: number | null;
  userId: string; // UUID do usuário que fez a pergunta
  user: GenericUserInfoResponse; // Informações completas do usuário
  content: string; // Conteúdo da pergunta
  answerId?: number | null;
  answer?: AnswerResponse | null;
  questionType: number; // Tipo da pergunta (enum)
} | null // null se não houver competição ativa

interface AnswerResponse {
  id: number;
  content: string; // Conteúdo da resposta
  userId: string; // UUID do usuário que respondeu
  user: GenericUserInfoResponse; // Informações completas do usuário que respondeu
}

interface GenericUserInfoResponse {
  id: string; // UUID
  name: string;
  email: string;
  createdAt: string; // ISO 8601 DateTime
  lastLoggedAt: string; // ISO 8601 DateTime
  ra: string;
  joinYear: number;
  department: string | null;
  exercisesCreated: null;
}
```

**Exemplo de uso (TypeScript)**:
```typescript
const request: CreateGroupQuestionRequest = {
  groupId: 1,
  competitionId: 1,
  exerciseId: 3,
  questionText: 'Não entendi o enunciado do exercício 3'
};

connection.invoke('SendCompetitionQuestion', request);

// Para o estudante que perguntou
connection.on('ReceiveQuestionCreationResponse', (question: QuestionResponse | null) => {
  if (question) {
    console.log('Pergunta criada:', question);
    console.log('Autor:', question.user.name);
    console.log('Conteúdo:', question.content);
  }
});

// Para professores e administradores
connection.on('ReceiveQuestionCreation', (question: QuestionResponse) => {
  console.log('Nova pergunta recebida:', question);
  console.log('De:', question.user.name, '(' + question.user.email + ')');
  console.log('Exercício:', question.exerciseId || 'Pergunta geral');
  // Exibir notificação ou adicionar à lista de perguntas
});
```

---

### 5. AnswerQuestion

**Descrição**: Responde uma pergunta feita por um grupo. A resposta é enviada para todos os professores e administradores.

**Autorização**: Apenas roles `Admin` ou `Teacher`

**Método de Invocação**: `AnswerQuestion`

**Parâmetros de Entrada**:
```typescript
interface AnswerGroupQuestionRequest {
  questionId: number;
  answerText: string;
  isPrivate: boolean; // Se true, apenas o grupo que perguntou verá a resposta
}
```

**Eventos de Retorno**:
- `ReceiveQuestionAnswerResponse` - Para quem respondeu
- `ReceiveQuestionAnswer` - Para grupos `Teachers` e `Admins`

**Payload de Resposta**:
```typescript
interface Answer {
  id: number;
  content: string; // Conteúdo da resposta
  userId: string; // UUID do professor/admin que respondeu
  user: User; // Informações completas do usuário que respondeu (incluindo navegação)
  question: Question; // Referência completa à pergunta (incluindo navegação)
} | null // null se não houver competição ativa
```

**Exemplo de uso (TypeScript)**:
```typescript
const request: AnswerGroupQuestionRequest = {
  questionId: 42,
  answerText: 'O exercício pede para você...',
  isPrivate: false
};

connection.invoke('AnswerQuestion', request);

// Para quem respondeu
connection.on('ReceiveQuestionAnswerResponse', (answer: Answer | null) => {
  if (answer) {
    console.log('Resposta enviada:', answer);
    console.log('Conteúdo:', answer.content);
    console.log('Para pergunta ID:', answer.question.id);
  }
});

// Para professores e administradores
connection.on('ReceiveQuestionAnswer', (answer: Answer) => {
  console.log('Nova resposta:', answer);
  console.log('Respondida por:', answer.user.name);
  console.log('Conteúdo:', answer.content);
  // Atualizar a pergunta com a resposta na UI
});
```

---

### 6. ChangeJudgeSubmissionResponse

**Descrição**: Altera manualmente o resultado de uma submissão (override do judge automático).

**Autorização**: Apenas roles `Admin` ou `Teacher`

**Método de Invocação**: `ChangeJudgeSubmissionResponse`

**Parâmetros de Entrada**:
```typescript
interface RevokeGroupSubmissionRequest {
  submissionId: number;
  newJudgeResponse: number; // Enum: 0=Pending, 1=Accepted, 2=WrongAnswer, 3=TimeLimitExceeded, 4=MemoryLimitExceeded, 5=RuntimeError, 6=CompilationError
}
```

**Evento de Retorno**: `ReceiveChangeJudgeSubmissionResponse`

**Payload de Resposta**:
```typescript
boolean // true se a alteração foi bem-sucedida, false caso contrário
```

**Exemplo de uso (TypeScript)**:
```typescript
const request: RevokeGroupSubmissionRequest = {
  submissionId: 123,
  newJudgeResponse: 1 // Accepted
};

connection.invoke('ChangeJudgeSubmissionResponse', request);

connection.on('ReceiveChangeJudgeSubmissionResponse', (succeeded: boolean) => {
  if (succeeded) {
    console.log('Resultado alterado com sucesso');
  } else {
    console.log('Falha ao alterar resultado');
  }
});
```

---

### 7. BlockGroupSubmission

**Descrição**: Bloqueia a capacidade de um grupo enviar submissões na competição. Quando um grupo é bloqueado, todas as tentativas de envio de submissões retornarão um erro via `ReceiveExerciseAttemptError`.

**Autorização**: Apenas roles `Admin` ou `Teacher`

**Método de Invocação**: `BlockGroupSubmission`

**Parâmetros de Entrada**:
```typescript
interface BlockGroupSubmissionRequest {
  groupId: number;
  competitionId: number;
  reason?: string; // Opcional - motivo do bloqueio
}
```

**Evento de Retorno**: `ReceiveBlockGroupSubmissionResponse`

**Payload de Resposta**:
```typescript
boolean // true se o bloqueio foi bem-sucedido, false caso contrário
```

**Exemplo de uso (TypeScript)**:
```typescript
const request: BlockGroupSubmissionRequest = {
  groupId: 1,
  competitionId: 1,
  reason: 'Violação das regras da competição'
};

connection.invoke('BlockGroupSubmission', request);

connection.on('ReceiveBlockGroupSubmissionResponse', (succeeded: boolean) => {
  if (succeeded) {
    console.log('Grupo bloqueado com sucesso');
  } else {
    console.log('Falha ao bloquear grupo');
  }
});
```

---

## Eventos Broadcast (Enviados pelo Servidor)

### ReceiveRankingUpdate

**Descrição**: Evento enviado automaticamente para todos os participantes quando o ranking de um grupo é atualizado após uma submissão de exercício.

**Destinatários**: Todos os usuários conectados (`Students`, `Teachers`, `Admins`)

**Quando é disparado**: Após cada submissão de exercício processada (aceita ou não)

**Payload**:

```typescript
interface CompetitionRankingResponse {
  id: number;
  points: number; // Total de pontos do grupo
  penalty: number; // Penalidade total em minutos
  rankOrder: number; // Posição no ranking (1 = primeiro lugar)
  group: GroupResponse;
  exerciseAttempts: ExerciseAttemptSummary[];
}

interface ExerciseAttemptSummary {
  groupId: number;
  exerciseId: number;
  attempts: number; // Número total de tentativas para este exercício
}

interface GroupResponse {
  id: string; // UUID
  leaderId: string; // UUID
  name: string;
  users: GenericUserInfoResponse[];
}
```

**Exemplo de handler (TypeScript)**:

```typescript
connection.on('ReceiveRankingUpdate', (ranking: CompetitionRankingResponse) => {
  console.log(`Ranking atualizado para grupo ${ranking.group.name}`);
  console.log(`Posição: ${ranking.rankOrder}`);
  console.log(`Pontos: ${ranking.points}`);
  console.log(`Penalidade: ${ranking.penalty} minutos`);
  
  // Atualizar tabela de ranking na UI
  updateRankingTable(ranking);
  
  // Exibir notificação se for meu grupo
  if (ranking.group.id === myGroupId) {
    showNotification(`Seu grupo está em ${ranking.rankOrder}º lugar!`);
  }
});
```

**Observações importantes**:
- Este evento é enviado para **todos** os participantes, não apenas para o grupo que submeteu
- Permite atualização em tempo real do ranking sem necessidade de polling
- Cada submissão (aceita ou não) gera uma atualização de ranking
- O campo `exerciseAttempts` mostra quantas tentativas cada grupo fez em cada exercício

---

## Enums de Referência

### JudgeResponse (Respostas do Judge)
```typescript
enum JudgeResponse {
  Pending = 0,
  Accepted = 1,
  WrongAnswer = 2,
  TimeLimitExceeded = 3,
  MemoryLimitExceeded = 4,
  RuntimeError = 5,
  CompilationError = 6
}
```

### CompetitionStatus
```typescript
enum CompetitionStatus {
  NotStarted = 0,
  InProgress = 1,
  Finished = 2
}
```

### LogType (para referência de ações registradas)
```typescript
enum LogType {
  Login = 0,
  Logout = 1,
  GroupBlockedInCompetition = 2,
  // Outros tipos conforme necessário
}
```

---

## Fluxo de Conexão Completo (Exemplo TypeScript)

```typescript
import * as signalR from '@microsoft/signalr';

// Configurar conexão
const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://seu-backend.com/competitionHub', {
    accessTokenFactory: () => getAuthToken() // Seu token JWT
  })
  .withAutomaticReconnect()
  .build();

// Registrar handlers de eventos ANTES de conectar
connection.on('OnConnectionResponse', (competition: CompetitionResponse | null) => {
  if (competition) {
    console.log('Competição atual:', competition);
    // Inicializar estado da aplicação com dados da competição
  } else {
    console.log('Nenhuma competição ativa');
  }
});

connection.on('ReceiveQuestionCreation', (question: QuestionResponse) => {
  console.log('Nova pergunta:', question);
  console.log('De:', question.user.name);
  // Exibir notificação para professores/admins
});

connection.on('ReceiveQuestionAnswer', (answer: Answer) => {
  console.log('Nova resposta:', answer);
  console.log('Respondida por:', answer.user.name);
  // Atualizar interface com a resposta
});

connection.on('ReceiveExerciseAttemptResponse', (response: ExerciseAttemptResponse | null) => {
  if (response) {
    console.log('Resultado da tentativa:', response);
    // Atualizar ranking, exibir resultado
  }
});

connection.on('ReceiveExerciseAttemptError', (error: { message: string }) => {
  console.error('Erro na submissão:', error.message);
  // Exibir mensagem de erro ao usuário
});

// Conectar
await connection.start();
console.log('Conectado ao CompetitionHub');

// Usar métodos
await connection.invoke('Ping');
await connection.invoke('GetConnectionId');

// Desconectar quando necessário
await connection.stop();
```

---

## Notas Importantes

1. **Autenticação**: Todos os métodos requerem autenticação JWT via Bearer Token.

2. **Grupos SignalR**: Usuários são automaticamente adicionados a grupos baseados em suas roles ao conectar.

3. **Cache de Competição**: O hub usa cache em memória para a competição atual, reduzindo consultas ao banco.

4. **Processamento Assíncrono**: `SendExerciseAttempt` usa uma fila (`ExerciseSubmissionQueue`) para processamento assíncrono. A resposta pode não ser imediata.

5. **Worker de Background**: O `ExerciseSubmissionWorker` executa continuamente, processando itens da fila mesmo quando está vazia (aguarda 1 segundo entre verificações).

6. **Entity Framework Core**: O sistema utiliza eager loading (`.Include()`) para carregar propriedades de navegação (como `Group.Users`), evitando problemas de lazy loading e erros de null reference.

7. **Reconexão Automática**: Recomenda-se configurar `.withAutomaticReconnect()` no cliente SignalR.

8. **Logs**: Todas as ações importantes são registradas (login, logout, bloqueios, etc.).

9. **Timezone**: Todos os DateTime são retornados em UTC (ISO 8601).

10. **IDs**: IDs de usuários são strings (UUID), IDs de grupos são numéricos (`int`), assim como IDs de exercícios, competições, submissões, etc.

---

## Tratamento de Erros

- Se não houver competição ativa, a maioria dos métodos retorna `null` no evento de resposta.
- Erros de autorização resultam em desconexão automática.
- Exceções não tratadas são logadas e podem resultar em respostas `false` ou `null`.
- **Validações de submissão**: Erros de validação (usuário sem grupo, grupo bloqueado) são retornados via `ReceiveExerciseAttemptError` antes do enfileiramento.
- **Erros de processamento**: Erros durante o processamento assíncrono (grupo não encontrado, dados inválidos) são capturados pelo worker e podem resultar em `JudgeException`.
- **Navegação de entidades**: O sistema garante o carregamento de todas as propriedades de navegação necessárias (usando `.Include()`) para evitar erros de null reference durante o processamento.

---

## Versionamento

**Versão do documento**: 1.2  
**Data**: 5 de novembro de 2025  
**Backend Framework**: ASP.NET Core SignalR  
**Cliente Recomendado**: @microsoft/signalr (TypeScript/JavaScript)

### Changelog

#### Versão 1.2 (5 de novembro de 2025)
- **Correção crítica**: Implementado carregamento eager (`.Include()`) da propriedade de navegação `Group.Users` usando Entity Framework Core
- Adicionado método `GetByIdWithUsers()` no `GroupRepository` para evitar lazy loading e erros de null reference
- Corrigido bug no `ExerciseSubmissionWorker` que causava término prematuro quando a fila estava vazia
- Worker agora executa loop contínuo com delay de 1 segundo quando não há itens na fila
- Melhorado tratamento de erros em `GroupAttemptService` com validação de grupo nulo
- Sistema de processamento assíncrono mais robusto e confiável
- **Atualizado**: Documentação de `SendCompetitionQuestion` agora reflete o retorno completo de `QuestionResponse` com informações do usuário
- **Atualizado**: Documentação de `AnswerQuestion` agora mostra o retorno da entidade `Answer` completa com navegação de usuário e pergunta
- **Correção**: Interfaces TypeScript agora correspondem exatamente aos DTOs do backend

#### Versão 1.1 (4 de novembro de 2025)
- Adicionado evento `ReceiveExerciseAttemptError` para tratamento de erros de validação antes do enfileiramento
- Documentada validação de bloqueio de grupo em `SendExerciseAttempt`
- Adicionada seção sobre validações realizadas antes do processamento assíncrono
- Atualizado método `BlockGroupSubmission` com informação sobre impacto em submissões futuras

#### Versão 1.0 (3 de novembro de 2025)
- Versão inicial da documentação
