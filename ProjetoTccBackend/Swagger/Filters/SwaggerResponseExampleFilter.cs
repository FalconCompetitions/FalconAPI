using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ProjetoTccBackend.Swagger.Interfaces;

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
        foreach (var response in operation.Responses)
        {
            if (response.Value.Content.ContainsKey("application/json"))
            {
                var returnType = context.MethodInfo
                    .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
                    .Cast<ProducesResponseTypeAttribute>()
                    .FirstOrDefault(r => r.StatusCode.ToString() == response.Key)?
                    .Type;

                if (returnType == null || returnType == typeof(void) || returnType == typeof(Task) || returnType == typeof(IActionResult))
                    continue;


                if (returnType != null)
                {
                    var exampleType = typeof(ISwaggerExampleProvider<>).MakeGenericType(returnType);
                    var exampleProvider = _serviceProvider.GetService(exampleType);
                    if (exampleProvider != null)
                    {
                        var method = exampleType.GetMethod("GetExample");
                        var exampleInstance = method?.Invoke(exampleProvider, null);
                        if (exampleInstance != null)
                        {
                            var json = JsonSerializer.Serialize(exampleInstance);
                            response.Value.Content["application/json"].Example = new OpenApiString(json);
                        }
                    }
                }
            }
        }
    }
}
