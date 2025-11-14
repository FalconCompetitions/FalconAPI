using System;

namespace ProjetoTccBackend.Exceptions.Group
{
    /// <summary>
    /// Exception used to indicate a successful group update due to concurrency (e.g., group was removed or changed by another user).
    /// </summary>
    public class GroupConcurrencySuccessException : Exception
    {
        public GroupConcurrencySuccessException() : base("O grupo foi removido ou alterado por outro usuário. Operação considerada bem-sucedida.") { }
        public GroupConcurrencySuccessException(string message) : base(message) { }
    }
}