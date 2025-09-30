namespace ProjetoTccBackend.Exceptions.Group
{
    public class UserHasGroupException : Exception
    {
        public UserHasGroupException() : base("O usuário já está em outro grupo!")
        {
            
        }
    }
}
