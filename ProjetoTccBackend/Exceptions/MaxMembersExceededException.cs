using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the group exceeds the maximum number of members allowed in the competition.
    /// </summary>
    public class MaxMembersExceededException : Exception
    {
        public MaxMembersExceededException() : base("O grupo excedeu o número máximo de membros permitido na competição.") { }
        public MaxMembersExceededException(string message) : base(message) { }
    }
}