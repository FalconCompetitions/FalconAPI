using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the user is not the leader of the group.
    /// </summary>
    public class UserIsNotLeaderException : Exception
    {
        public UserIsNotLeaderException() : base("O usuário não é o líder do grupo.") { }
        public UserIsNotLeaderException(string message) : base(message) { }
    }
}