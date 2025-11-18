namespace ProjetoTccBackend.Exceptions.Group
{
    public class UserHasGroupException : FormException
    {
        public UserHasGroupException() : base(
            new Dictionary<string, string> { { "form", "O usuário já está em um grupo" } },
            "O usuário já está em um grupo"
        )
        {
        }

        public UserHasGroupException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}
