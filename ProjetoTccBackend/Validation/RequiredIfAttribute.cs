using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Validation
{
    /// <summary>
    /// Validation attribute that enforces a required field conditionally, based on the value of another property.
    /// The decorated property will be required only if the specified dependent property equals the target value.
    /// </summary>
    /// <example>
    /// [RequiredIf("Status", "Active", ErrorMessage = "Field is required when status is Active.")]
    /// public string? Description { get; set; }
    /// </example>
    /// <remarks>
    /// Useful for scenarios where a field should only be mandatory under certain conditions.
    /// </remarks>
    public class RequiredIfAttribute : ValidationAttribute
    {
        /// <summary>
        /// Gets the name of the dependent property.
        /// </summary>
        private string _dependentProperty { get; }

        /// <summary>
        /// Gets the target value that triggers the validation.
        /// </summary>
        private object _targetValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
        /// </summary>
        /// <param name="dependentProperty">The name of the dependent property.</param>
        /// <param name="targetValue">The target value that triggers the validation.</param>
        public RequiredIfAttribute(string dependentProperty, object targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
        }

        /// <summary>
        /// Validates the specified value against the dependent property's target value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating the result of the validation.</returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dependentPropertyInfo = validationContext.ObjectType.GetProperty(_dependentProperty);

            if (dependentPropertyInfo == null)
            {
                return new ValidationResult($"Propriedade desconhecida: {_dependentProperty}");
            }

            var dependentPropertyValue = dependentPropertyInfo.GetValue(validationContext.ObjectInstance);

            if (Equals(dependentPropertyValue, _targetValue))
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
