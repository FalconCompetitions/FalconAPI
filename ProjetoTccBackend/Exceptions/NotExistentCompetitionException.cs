using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the competition does not exist.
    /// </summary>
    public class NotExistentCompetitionException : Exception
    {
        public NotExistentCompetitionException() : base("Competição não existente.") { }
        public NotExistentCompetitionException(string message) : base(message) { }
    }
}