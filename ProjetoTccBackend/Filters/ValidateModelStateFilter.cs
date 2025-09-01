using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Filters
{
    /// <summary>
    /// Action filter that validates the model state before the action executes.
    /// Returns a 400 error with validation error details if the model is invalid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateModelStateFilter : Attribute, IActionFilter
    {
        /// <summary>
        /// Lists the model validation errors present in the action context.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>A list of form errors.</returns>
        private static List<FormError> ListModelErrors(ActionContext context)
        {
            List<FormError> errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new FormError(x.Key, e.ErrorMessage)))
                .ToList<FormError>();

            return errors;
        }

        /// <inheritdoc />
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        /// <inheritdoc />
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                List<FormError> errors = ListModelErrors(context);

                var result = new
                {
                    errors = errors
                };

                context.Result = new BadRequestObjectResult(result);
                return;
            }
            return;
        }
    }
}
