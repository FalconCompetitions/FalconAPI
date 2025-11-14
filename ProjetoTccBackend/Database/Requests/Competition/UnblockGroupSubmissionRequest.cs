using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    /// <summary>
    /// Request model for unblocking a group's submissions in a competition.
    /// </summary>
    public class UnblockGroupSubmissionRequest
    {
        /// <summary>
        /// Identifier of the group participating in the competition.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Identifier of the competition in which the group is participating.
        /// </summary>
        public int CompetitionId { get; set; }
    }
}
