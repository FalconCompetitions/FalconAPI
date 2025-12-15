namespace ProjetoTccBackend.Exceptions.Group
{
    /// <summary>
    /// Exception thrown when a user already belongs to a group.
    /// </summary>
    public class UserHasGroupException : FormException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserHasGroupException"/> class with a default message.
        /// </summary>
        public UserHasGroupException() : base(
            new Dictionary<string, string> { { "form", "O usuário já está em um grupo" } },
            "O usuário já está em um grupo"
        )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserHasGroupException"/> class with a custom field and message.
        /// </summary>
        /// <param name="field">The form field that caused the error.</param>
        /// <param name="message">The custom error message.</param>
        public UserHasGroupException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}
