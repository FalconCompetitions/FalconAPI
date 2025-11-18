namespace ProjetoTccBackend.Exceptions.Group
{
    public class GroupInvitationException : FormException
    {
        public GroupInvitationException() : base(
            new Dictionary<string, string> { { "form", "Convite de grupo inválido" } },
            "Convite de grupo inválido"
        )
        {
        }

        public GroupInvitationException(string field, string message) : base(
            new Dictionary<string, string> { { field, message } },
            message
        )
        {
        }
    }
}
