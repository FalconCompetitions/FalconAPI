using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the group is already inscribed in the competition.
    /// </summary>
    public class AlreadyInCompetitionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlreadyInCompetitionException"/> class with a default message.
        /// </summary>
        public AlreadyInCompetitionException() : base("O grupo já está inscrito na competição.") { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AlreadyInCompetitionException"/> class with a custom message.
        /// </summary>
        /// <param name="message">The custom error message.</param>
        public AlreadyInCompetitionException(string message) : base(message) { }
    }
}