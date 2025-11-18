namespace ProjetoTccBackend.Exceptions
{
    public class ExistentCompetitionException : FormException
    {
        public ExistentCompetitionException() : base(
            new Dictionary<string, string> { { "form", "Já existe uma competição cadastrada para esta data" } },
            "Já existe uma competição cadastrada para esta data"
        )
        {
        }

        public ExistentCompetitionException(string message) : base(
            new Dictionary<string, string> { { "form", message } },
            message
        )
        {
        }
    }
}
