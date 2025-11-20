namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to create a competition that already exists for a specific date.
    /// </summary>
    public class ExistentCompetitionException : FormException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExistentCompetitionException"/> class with a default message.
        /// </summary>
        public ExistentCompetitionException() : base(
            new Dictionary<string, string> { { "form", "Já existe uma competição cadastrada para esta data" } },
            "Já existe uma competição cadastrada para esta data"
        )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistentCompetitionException"/> class with a custom message.
        /// </summary>
        /// <param name="message">The custom error message.</param>
        public ExistentCompetitionException(string message) : base(
            new Dictionary<string, string> { { "form", message } },
            message
        )
        {
        }
    }
}
