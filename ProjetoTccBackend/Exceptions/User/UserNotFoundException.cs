namespace ProjetoTccBackend.Exceptions.User
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userId) : base($"O usuário de id {userId} não existe!")
        {
            
        }
    }
}
