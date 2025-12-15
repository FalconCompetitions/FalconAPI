namespace ProjetoTccBackend.Exceptions.Group
{
    /// <summary>
    /// Exception thrown when a user is not the leader of a group but attempts an operation that requires leader privileges.
    /// </summary>
    public class UserNotGroupLeaderException : FormException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotGroupLeaderException"/> class with a default message.
        /// </summary>
        public UserNotGroupLeaderException() : base(
            new Dictionary<string, string> { { "form", "Você não é o líder do grupo" } },
            "Você não é o líder do grupo"
        )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotGroupLeaderException"/> class with a custom field and message.
        /// </summary>
        /// <param name="field">The form field that caused the error.</param>
        /// <param name="message">The custom error message.</param>
        public UserNotGroupLeaderException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}
