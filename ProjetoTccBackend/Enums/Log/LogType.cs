namespace ProjetoTccBackend.Enums.Log
{
    /// <summary>
    /// Types of logs that can be recorded in the system.
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Action performed by a user.
        /// </summary>
        UserAction = 0,
        /// <summary>
        /// Action performed by the system.
        /// </summary>
        SystemAction = 1,

        /// <summary>
        /// User login action.
        /// </summary>
        Login = 2,

        /// <summary>
        /// User logout action.
        /// </summary>
        Logout = 3,

        /// <summary>
        /// Exercise submission action.
        /// </summary>
        SubmittedExercise = 4,

        /// <summary>
        /// Group blocked in competition action.
        /// </summary>
        GroupBlockedInCompetition = 5,

        /// <summary>
        /// Group unblocked in competition action.
        /// </summary>
        GroupUnblockedInCompetition = 6,

        /// <summary>
        /// Question sent action.
        /// </summary>
        QuestionSent = 7,

        /// <summary>
        /// Answer given action.
        /// </summary>
        AnswerGiven = 8
    }
}
