namespace ProjetoTccBackend.Exceptions.User
{
    /// <summary>
    /// Exception thrown when a user is not found.
    /// </summary>
    public class UserNotFoundException : FormException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotFoundException"/> class with a user ID.
        /// </summary>
        /// <param name="userId">The ID of the user that was not found.</param>
        public UserNotFoundException(string userId) : base(
            new Dictionary<string, string> { { "form", $"Usuário com RA {userId} não encontrado" } },
            $"Usuário com RA {userId} não encontrado"
        )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotFoundException"/> class with a custom field and message.
        /// </summary>
        /// <param name="field">The form field that caused the error.</param>
        /// <param name="message">The custom error message.</param>
        public UserNotFoundException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}
