using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the competition is not valid for inscription.
    /// </summary>
    public class NotValidCompetitionException : Exception
    {
        public NotValidCompetitionException() : base("Competição não está válida para inscrição.") { }
        public NotValidCompetitionException(string message) : base(message) { }
    }
}