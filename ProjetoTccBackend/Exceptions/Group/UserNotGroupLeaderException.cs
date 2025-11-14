namespace ProjetoTccBackend.Exceptions.Group
{
    public class UserNotGroupLeaderException : Exception
    {
        public UserNotGroupLeaderException() : base("O usuário não é o líder do grupo!")
        {
            
        }
    }
}
