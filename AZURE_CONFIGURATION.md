# Configuração de Variáveis de Ambiente no Azure

## Problemas Identificados

### 1. JudgeApi:Url Ausente
O erro `System.ArgumentNullException: Value cannot be null. (Parameter 'uriString')` ocorre quando a configuração `JudgeApi:Url` não está definida no ambiente de produção do Azure.

### 2. CORS Bloqueado
O erro `Access to fetch at 'https://falconapi.azurewebsites.net/...' from origin 'https://falconcompetitions.azurewebsites.net' has been blocked by CORS policy` ocorre quando a configuração `Cors:FrontendURL` não está definida corretamente.

## Configurações Obrigatórias

Para que a aplicação funcione corretamente no Azure, você deve configurar as seguintes variáveis de ambiente no Azure App Service:

### 1. JudgeApi__Url
- **Descrição**: URL base da API do Judge
- **Formato**: `https://seu-judge-api-url/v0`
- **Exemplo**: `https://judge-api.azurewebsites.net/v0`
- **Nota**: No Azure, use `__` (duplo underscore) ao invés de `:` para níveis aninhados

### 2. JudgeApi__SecurityKey
- **Descrição**: Chave de segurança para autenticação com o Judge API
- **Formato**: GUID
- **Exemplo**: `57fba00c-aa3d-4009-87d6-700f58a4032b`

### 3. Cors__FrontendURL ⚠️ IMPORTANTE
- **Descrição**: URL do frontend para configuração de CORS
- **Formato**: `https://sua-aplicacao-frontend.azurewebsites.net`
- **Exemplo**: `https://falconcompetitions.azurewebsites.net`
- **Nota**: Esta configuração é essencial para permitir WebSocket (SignalR) e requisições HTTP do frontend

## Como Configurar no Azure Portal

1. Acesse o [Azure Portal](https://portal.azure.com)
2. Navegue até o seu App Service
3. No menu lateral, clique em **Configuration**
4. Na seção **Application settings**, clique em **+ New application setting**
5. Adicione as seguintes configurações:

   | Nome | Valor |
   |------|-------|
   | `JudgeApi__Url` | URL da sua API Judge |
   | `JudgeApi__SecurityKey` | Chave de segurança do Judge |
   | `Cors__FrontendURL` | `https://falconcompetitions.azurewebsites.net` |

6. Clique em **Save** para salvar as configurações
7. Clique em **Continue** para confirmar o restart da aplicação

## Como Configurar via Azure CLI

```bash
# Definir variáveis
RESOURCE_GROUP="seu-resource-group"
APP_NAME="falconapi-akakfufefhhxgchj"
JUDGE_URL="https://sua-judge-api-url/v0"
JUDGE_KEY="57fba00c-aa3d-4009-87d6-700f58a4032b"
FRONTEND_URL="https://falconcompetitions.azurewebsites.net"

# Configurar as variáveis
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --settings \
    JudgeApi__Url=$JUDGE_URL \
    JudgeApi__SecurityKey=$JUDGE_KEY \
    Cors__FrontendURL=$FRONTEND_URL
```

## Verificar Configuração

Após configurar, você pode verificar se as variáveis foram aplicadas:

```bash
az webapp config appsettings list \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --query "[?name=='JudgeApi__Url' || name=='JudgeApi__SecurityKey' || name=='Cors__FrontendURL']"
```

## Outras Configurações Importantes

Certifique-se de que as seguintes configurações também estejam definidas:

- `ConnectionStrings__DefaultConnection`: String de conexão do banco de dados
- `Jwt__Key`: Chave JWT para autenticação
- `Jwt__Issuer`: Emissor do token JWT
- `Jwt__Audience`: Audiência do token JWT
- `SymmetricSecurityKey`: Chave de segurança simétrica
- `Cors__FrontendURL`: URL do frontend (essencial para CORS e WebSocket)

## Tratamento de Erro e Logs

### Validação de Configuração JudgeApi

O código agora inclui validação na inicialização:

```csharp
var judgeApiUrl = builder.Configuration["JudgeApi:Url"];
if (string.IsNullOrEmpty(judgeApiUrl))
{
    throw new InvalidOperationException(
        "Configuration 'JudgeApi:Url' is missing. Please ensure it is set in appsettings.json or environment variables."
    );
}
```

### Logs de CORS

O código agora inclui logs de debug para CORS:

```csharp
Console.WriteLine($"[CORS] Added FrontendURL from config: {frontendUrl}");
Console.WriteLine($"[CORS] Configured origins: {string.Join(", ", allowedOrigins)}");
```

Você verá essas mensagens nos logs do Azure durante o startup da aplicação.

## Logs de Diagnóstico

Para visualizar logs e diagnosticar problemas:

1. No Azure Portal, vá até o App Service
2. Clique em **Log stream** no menu lateral
3. Ou use o Azure CLI:

```bash
az webapp log tail \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME
```

## Resolução de Problemas Comuns

### Problema: CORS bloqueado no WebSocket (SignalR)

**Sintomas**: 
```
Access to fetch at 'https://falconapi.azurewebsites.net/hub/competition/negotiate' 
has been blocked by CORS policy
```

**Solução**:
1. Verifique se `Cors__FrontendURL` está configurado corretamente no Azure
2. Reinicie a aplicação após alterar a configuração
3. Verifique os logs de startup para confirmar: `[CORS] Configured origins: ...`

### Problema: JudgeApi:Url ausente

**Sintomas**:
```
ArgumentNullException: Value cannot be null. (Parameter 'uriString')
```

**Solução**:
1. Configure `JudgeApi__Url` nas variáveis de ambiente do Azure
2. A aplicação falhará no startup com mensagem clara se a configuração estiver ausente

## Notas Importantes

- **Separadores**: No Azure App Service, use `__` (duplo underscore) para representar `:` em configurações aninhadas
- **Restart**: A aplicação será reiniciada automaticamente após alterar as configurações
- **Segurança**: Nunca commite chaves de segurança no repositório Git. Use sempre variáveis de ambiente ou Azure Key Vault
- **Fallback**: Se `Cors__FrontendURL` não estiver configurado, o sistema usará `https://falconcompetitions.azurewebsites.net` como fallback
