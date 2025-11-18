namespace ProjetoTccBackend.Exceptions
{
    public class ErrorException : Exception
    {
        protected readonly object? _errorData = null;

        public ErrorException(object? errorData) : base()
        {
            this._errorData = errorData;
        }

        public ErrorException(string message) : base(message)
        {

        }
    }
}
