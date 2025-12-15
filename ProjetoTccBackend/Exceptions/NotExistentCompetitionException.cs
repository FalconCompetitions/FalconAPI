using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the competition does not exist.
    /// </summary>
    public class NotExistentCompetitionException : FormException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotExistentCompetitionException"/> class with a default message.
        /// </summary>
        public NotExistentCompetitionException() : base(
            new Dictionary<string, string> { { "form", "Competição não encontrada" } },
            "Competição não encontrada"
        )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExistentCompetitionException"/> class with a custom message.
        /// </summary>
        /// <param name="message">The custom error message.</param>
        public NotExistentCompetitionException(string message) : base(
            new Dictionary<string, string> { { "form", message } },
            message
        )
        {
        }
    }
}