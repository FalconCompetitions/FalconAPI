namespace ProjetoTccBackend.Exceptions.AttachedFile
{
    /// <summary>
    /// Exception thrown when an attached file is invalid or cannot be processed.
    /// </summary>
    public class InvalidAttachedFileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAttachedFileException"/> class with a message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception that caused this exception.</param>
        public InvalidAttachedFileException(string message, Exception innerException) : base(message, innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAttachedFileException"/> class with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public InvalidAttachedFileException(string message) : base(message)
        {

        }
    }
}
