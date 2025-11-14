using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the group is already inscribed in the competition.
    /// </summary>
    public class AlreadyInCompetitionException : Exception
    {
        public AlreadyInCompetitionException() : base("O grupo já está inscrito na competição.") { }
        public AlreadyInCompetitionException(string message) : base(message) { }
    }
}