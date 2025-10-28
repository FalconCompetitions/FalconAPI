namespace ProjetoTccBackend.Database.Requests.Competition
{
    public class InscribeGroupToCompetitionRequest
    {
        public int CompetitionId { get; set; }
        public int GroupId { get; set; }
    }
}
