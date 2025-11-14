using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Database.Requests.Competition
{
    /// <summary>
    /// Request model for blocking a group's submissions in a competition.
    /// </summary>
    public class BlockGroupSubmissionRequest
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
