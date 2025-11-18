namespace ProjetoTccBackend.Exceptions.User
{
    public class UserNotFoundException : FormException
    {
        public UserNotFoundException(string userId) : base(
            new Dictionary<string, string> { { "form", $"Usuário com RA {userId} não encontrado" } },
            $"Usuário com RA {userId} não encontrado"
        )
        {
        }

        public UserNotFoundException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}
