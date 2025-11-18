using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the competition does not exist.
    /// </summary>
    public class NotExistentCompetitionException : FormException
    {
        public NotExistentCompetitionException() : base(
            new Dictionary<string, string> { { "form", "Competição não encontrada" } },
            "Competição não encontrada"
        )
        {
        }

        public NotExistentCompetitionException(string message) : base(
            new Dictionary<string, string> { { "form", message } },
            message
        )
        {
        }
    }
}