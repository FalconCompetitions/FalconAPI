using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Text.Json.Nodes;

/// <summary>
/// Swagger operation filter that adds example responses to API documentation.
/// </summary>
public class SwaggerResponseExampleFilter : IOperationFilter
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerResponseExampleFilter"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving example providers.</param>
    public SwaggerResponseExampleFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Applies example responses to the Swagger operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation.</param>
    /// <param name="context">The operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation?.Responses == null)
            return;

        foreach (var response in operation.Responses)
        {
            // Verificar se response.Value e Content existem
            if (
                response.Value?.Content == null
                || !response.Value.Content.ContainsKey("application/json")
            )
                continue;

            var returnType = context
                .MethodInfo.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
                .Cast<ProducesResponseTypeAttribute>()
                .FirstOrDefault(r => r.StatusCode.ToString() == response.Key)
                ?.Type;

            if (
                returnType == null
                || returnType == typeof(void)
                || returnType == typeof(Task)
                || returnType == typeof(IActionResult)
            )
                continue;

            var exampleType = typeof(ISwaggerExampleProvider<>).MakeGenericType(returnType);
            var exampleProvider = _serviceProvider.GetService(exampleType);

            if (exampleProvider != null)
            {
                var method = exampleType.GetMethod("GetExample");
                var exampleInstance = method?.Invoke(exampleProvider, null);

                if (exampleInstance != null)
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(exampleInstance);
                        var parsedExample = JsonNode.Parse(json);

                        // Verificar se o content existe antes de atribuir
                        if (
                            response.Value.Content.TryGetValue(
                                "application/json",
                                out var mediaType
                            )
                        )
                        {
                            mediaType.Example = parsedExample;
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Log do erro mas não quebrar a geração do Swagger
                        Console.WriteLine(
                            $"Erro ao serializar exemplo para {returnType.Name}: {ex.Message}"
                        );
                    }
                }
            }
        }
    }
}
