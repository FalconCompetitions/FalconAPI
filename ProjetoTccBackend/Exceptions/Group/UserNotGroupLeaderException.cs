namespace ProjetoTccBackend.Exceptions.Group
{
    public class UserNotGroupLeaderException : FormException
    {
        public UserNotGroupLeaderException() : base(
            new Dictionary<string, string> { { "form", "Você não é o líder do grupo" } },
            "Você não é o líder do grupo"
        )
        {
        }

        public UserNotGroupLeaderException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}
