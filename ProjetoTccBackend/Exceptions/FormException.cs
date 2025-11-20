namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Represents an exception that contains form validation errors.
    /// </summary>
    public class FormException : Exception
    {
        /// <summary>
        /// Dictionary containing field names and their corresponding error messages.
        /// </summary>
        public readonly IDictionary<string, string> FormData;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormException"/> class with form data.
        /// </summary>
        /// <param name="formData">Dictionary containing field names and error messages.</param>
        public FormException(IDictionary<string, string> formData) : base()
        {
            this.FormData = formData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormException"/> class with form data and a message.
        /// </summary>
        /// <param name="formData">Dictionary containing field names and error messages.</param>
        /// <param name="message">The exception message.</param>
        public FormException(IDictionary<string, string> formData, string message) : base(message)
        {
            this.FormData = formData;
        }
    }
}
