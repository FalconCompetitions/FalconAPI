namespace ProjetoTccBackend.Enums.Judge
{
    /// <summary>
    /// Represents the possible responses from the judge system after evaluating a submission.
    /// </summary>
    public enum JudgeSubmissionResponse
    {
        /// <summary>
        /// The submission was accepted and produced the correct output.
        /// </summary>
        Accepted = 0,

        /// <summary>
        /// The submission produced an incorrect output.
        /// </summary>
        WrongAnswer = 1,

        /// <summary>
        /// The submission caused a runtime error during execution.
        /// </summary>
        RuntimeError = 2,

        /// <summary>
        /// The submission failed to compile.
        /// </summary>
        CompilationError = 3,

        /// <summary>
        /// The submission output format was incorrect (presentation error).
        /// </summary>
        PresentationError = 4,

        /// <summary>
        /// The submission exceeded the allowed time limit.
        /// </summary>
        TimeLimitExceeded = 5,

        /// <summary>
        /// The submission exceeded the allowed memory limit.
        /// </summary>
        MemoryLimitExceeded = 6,

        /// <summary>
        /// The submission violated security constraints.
        /// </summary>
        SecurityError = 7,
    }
}
