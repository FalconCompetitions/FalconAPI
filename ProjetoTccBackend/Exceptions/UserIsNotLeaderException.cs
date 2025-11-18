using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the user is not the leader of the group.
    /// </summary>
    public class UserIsNotLeaderException : FormException
    {
        public UserIsNotLeaderException() : base(
            new Dictionary<string, string> { { "form", "Você não é o líder do grupo" } },
            "Você não é o líder do grupo"
        )
        {
        }

        public UserIsNotLeaderException(string message) : base(
            new Dictionary<string, string> { { "form", message } },
            message
        )
        {
        }
    }
}