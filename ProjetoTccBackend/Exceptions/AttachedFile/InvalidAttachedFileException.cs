namespace ProjetoTccBackend.Exceptions.AttachedFile
{
    public class InvalidAttachedFileException : Exception
    {
        public InvalidAttachedFileException(string message, Exception innerException) : base(message, innerException)
        {
            
        }

        public InvalidAttachedFileException(string message) : base(message)
        {
            
        }
    }
}
