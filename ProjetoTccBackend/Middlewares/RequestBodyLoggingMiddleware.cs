using System.Text;

namespace ProjetoTccBackend.Middlewares
{
    /// <summary>
    /// Middleware that logs the HTTP request body and passes control to the next middleware in the pipeline.
    /// </summary>
    /// <remarks>This middleware enables buffering on the request body to allow multiple reads, formats the
    /// body as indented JSON  if it is a valid JSON object, and logs the formatted body. If the body is not valid JSON,
    /// it logs the raw body content.  After processing, the middleware invokes the next middleware in the
    /// pipeline.</remarks>
    public class RequestBodyLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestBodyLoggingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestBodyLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">Logger for registering information and errors.</param>
        public RequestBodyLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestBodyLoggingMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes an incoming HTTP request, enabling buffering for the request body,  logging the formatted request
        /// body, and passing control to the next middleware in the pipeline.
        /// </summary>
        /// <remarks>This method enables buffering on the request body to allow multiple reads, formats
        /// the body as indented JSON  if it is a valid JSON object, and logs the formatted body. If the body is not
        /// valid JSON, it logs the raw body content. After processing, the method invokes the next middleware in the
        /// pipeline.</remarks>
        /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request and response.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            context.Request.Body.Position = 0;
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                leaveOpen: true
            );
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            string formattedBody = body;
            if (!string.IsNullOrWhiteSpace(body) && body.Trim().StartsWith("{"))
            {
                try
                {
                    var jsonElement =
                        System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(
                            body
                        );
                    formattedBody = System.Text.Json.JsonSerializer.Serialize(
                        jsonElement,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                    );
                }
                catch
                {
                }
            }

            _logger.LogInformation("Request Body:\n{Body}", formattedBody);

            await _next(context);
        }
    }
}
