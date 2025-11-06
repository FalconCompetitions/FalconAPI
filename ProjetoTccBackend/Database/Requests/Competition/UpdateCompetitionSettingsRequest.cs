namespace ProjetoTccBackend.Database.Requests.Competition
{
    /// <summary>
    /// Request to update competition settings.
    /// All time values are in seconds (not TimeSpan).
    /// </summary>
    public class UpdateCompetitionSettingsRequest
    {
        public required int CompetitionId { get; set; }
        
        /// <summary>
        /// Duration of the competition in seconds
        /// </summary>
        public required int Duration { get; set; }
        
        /// <summary>
        /// Time before end when submissions should stop, in seconds
        /// </summary>
        public required int StopSubmissionsBeforeEnd { get; set; }
        
        /// <summary>
        /// Time before end when ranking should stop updating, in seconds
        /// </summary>
        public required int StopRankingBeforeEnd { get; set; }
        
        /// <summary>
        /// Penalty per wrong submission in seconds
        /// </summary>
        public required int SubmissionPenalty { get; set; }
        
        /// <summary>
        /// Maximum submission file size in KB
        /// </summary>
        public required int MaxSubmissionSize { get; set; }
    }
}
