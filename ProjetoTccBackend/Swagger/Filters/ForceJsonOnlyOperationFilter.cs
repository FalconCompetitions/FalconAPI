using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjetoTccBackend.Swagger.Filters
{
    /// <summary>
    /// Swagger operation filter that forces API endpoints to only accept and return JSON content.
    /// </summary>
    public class ForceJsonOnlyOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the JSON-only constraint to the Swagger operation by removing non-JSON media types.
        /// </summary>
        /// <param name="operation">The OpenAPI operation.</param>
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null)
                return;

            // Remove todos os tipos de mídia de entrada que não sejam application/json
            if (operation.RequestBody?.Content != null)
            {
                var keysToRemove = operation.RequestBody.Content.Keys
                    .Where(k => !k.Equals("application/json", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in keysToRemove)
                    operation.RequestBody.Content.Remove(key);
            }

            // Remove todos os tipos de mídia de saída que não sejam application/json
            if (operation.Responses != null)
            {
                foreach (var response in operation.Responses)
                {
                    // Verificar se response.Value e Content existem antes de acessar
                    if (response.Value?.Content == null)
                        continue;

                    var keysToRemove = response.Value.Content.Keys
                        .Where(
                            k => !k.Equals("application/json", StringComparison.OrdinalIgnoreCase)
                        )
                        .ToList();

                    foreach (var key in keysToRemove)
                        response.Value.Content.Remove(key);
                }
            }
        }
    }
}
