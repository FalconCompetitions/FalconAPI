using System;

namespace ProjetoTccBackend.Exceptions.Group
{
    /// <summary>
    /// Exception used to indicate a successful group update due to concurrency (e.g., group was removed or changed by another user).
    /// </summary>
    public class GroupConcurrencySuccessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConcurrencySuccessException"/> class with a default message.
        /// </summary>
        public GroupConcurrencySuccessException() : base("O grupo foi removido ou alterado por outro usuário. Operação considerada bem-sucedida.") { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConcurrencySuccessException"/> class with a custom message.
        /// </summary>
        /// <param name="message">The custom error message.</param>
        public GroupConcurrencySuccessException(string message) : base(message) { }
    }
}