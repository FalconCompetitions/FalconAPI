using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    public class GroupInvitationResponse
    {
        public int Id { get; set; }

        public GenericUserInfoResponse? User { get; set; }

        public GroupResponse? Group { get; set; }

        public bool Accepted { get; set; }
    }
}
