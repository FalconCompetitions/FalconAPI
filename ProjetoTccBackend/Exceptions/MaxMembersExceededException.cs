using System;

namespace ProjetoTccBackend.Exceptions
{
    /// <summary>
    /// Exception thrown when the group exceeds the maximum number of members allowed in the competition.
    /// </summary>
    public class MaxMembersExceededException : FormException
    {
        public MaxMembersExceededException() : base(
            new Dictionary<string, string> { { "form", "O grupo excedeu o número máximo de membros permitido" } },
            "O grupo excedeu o número máximo de membros permitido"
        )
        {
        }

        public MaxMembersExceededException(string message) : base(
            new Dictionary<string, string> { { "form", message } },
            message
        )
        {
        }

        public MaxMembersExceededException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}