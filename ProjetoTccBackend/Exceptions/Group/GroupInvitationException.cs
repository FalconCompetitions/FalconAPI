namespace ProjetoTccBackend.Exceptions.Group
{
    /// <summary>
    /// Exception thrown when a group invitation is invalid or cannot be processed.
    /// </summary>
    public class GroupInvitationException : FormException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupInvitationException"/> class with a default message.
        /// </summary>
        public GroupInvitationException() : base(
            new Dictionary<string, string> { { "form", "Convite de grupo inválido" } },
            "Convite de grupo inválido"
        )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupInvitationException"/> class with a custom field and message.
        /// </summary>
        /// <param name="field">The form field that caused the error.</param>
        /// <param name="message">The custom error message.</param>
        public GroupInvitationException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}
