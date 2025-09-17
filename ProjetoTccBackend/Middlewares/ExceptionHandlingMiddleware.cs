using ProjetoTccBackend.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace ProjetoTccBackend.Middlewares
{
    /// <summary>
    /// Middleware responsible for handling exceptions globally in the application.
    /// Catches and processes known and unknown exceptions, returning appropriate HTTP responses and error messages in JSON format.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance for logging errors.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this._next = next;
            this._logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to handle exceptions during the HTTP request pipeline.
        /// Catches specific and general exceptions, sets the appropriate status code, and returns a JSON error response.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            bool isFormException = false;
            bool isUnexpectedError = false;

            var response = new object();

            try
            {
                await _next(context);

                if(context.Response.HasStarted)
                {
                    return;
                }

                if (context.Items.ContainsKey("ModelStateErrors"))
                {
                    isFormException = true;
                    response = context.Items["ModelStateErrors"];
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";
                    
                }
            }
            catch (ErrorException ex) // Generic error thrown manually in the code
            {
                isUnexpectedError = true;
                this._logger.LogError("Error");
                response = new { message = ex.Message };
            }
            catch (FormException ex) // Validation error thrown manually
            {
                this._logger.LogError("Error");
                isFormException = true;
                response = new
                {
                    errors = ex.FormData.Select(x =>
                    {
                        return new { target = x.Key, error = x.Value };
                    })
                };
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Unexpected error");

                // If it is a ModelState invalid error
                if(context.Request.HasFormContentType || IsJsonRequest(context))
                {
                    isFormException = true;
                    response = HandleValidationException(ex);
                }
                else // If it is an unexpected error
                {
                    isUnexpectedError = true;
                    response = new
                    {
                        errors = new[]
                        {
                            new { target = "general", error = "An unexpected error occurred. Please try again later." }
                        }
                    };
                }
            }

            if(context.Response.HasStarted)
            {
                return;
            }

            context.Response.ContentType = "application/json";
            if(isFormException is true)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            } else if(isUnexpectedError is true)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        /// <summary>
        /// Checks if the current HTTP request is a JSON request.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>True if the request is JSON, otherwise false.</returns>
        private bool IsJsonRequest(HttpContext context)
        {
            if (context.Request.ContentType is null) return false;

            if (context.Response.ContentType is null) return false;

            return context.Response.ContentType!.Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles validation exceptions and formats them into a standard error response.
        /// </summary>
        /// <param name="ex">The exception to handle.</param>
        /// <returns>An object containing the formatted validation errors.</returns>
        private object HandleValidationException(Exception ex)
        {
            var errors = new List<object>();

            if(ex is ValidationException validationEx) // Manual validation was thrown
            {
                
                errors.AddRange(validationEx.ValidationResult.MemberNames.Select(target => new
                {
                    target,
                    error = validationEx.ValidationResult.ErrorMessage
                }));
            }
            else if(ex is ArgumentException argEx) // Specific error
            {
                errors.Add(new { target = "general", error = argEx.Message });
            }
            else // Unexpected error
            {
                this._logger.LogInformation(JsonSerializer.Serialize(errors));
                errors.Add(new { target = "general", error = "An unexpected error occurred. Please try again later." });
            }

            return new { errors };
        }
    }
}
