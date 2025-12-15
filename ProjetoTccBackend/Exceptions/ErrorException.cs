namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Represents an error exception with optional error data.
    /// </summary>
    public class ErrorException : Exception
    {
        /// <summary>
        /// The error data associated with this exception.
        /// </summary>
        protected readonly object? _errorData = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorException"/> class with error data.
        /// </summary>
        /// <param name="errorData">The error data associated with this exception.</param>
        public ErrorException(object? errorData) : base()
        {
            this._errorData = errorData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorException"/> class with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ErrorException(string message) : base(message)
        {

        }
    }
}
