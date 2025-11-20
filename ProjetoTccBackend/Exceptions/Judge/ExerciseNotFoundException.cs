namespace ProjetoTccBackend.Exceptions.Judge
{
    /// <summary>
    /// Exception thrown when an exercise is not found during judge operations.
    /// </summary>
    public class ExerciseNotFoundException : Exception
    {
        /// <summary>
        /// Information about the exercise that was not found.
        /// </summary>
        private object ExerciseInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseNotFoundException"/> class.
        /// </summary>
        public ExerciseNotFoundException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseNotFoundException"/> class with exercise information.
        /// </summary>
        /// <param name="obj">Information about the exercise that was not found.</param>
        public ExerciseNotFoundException(object obj)
        {
            ExerciseInfo = obj;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseNotFoundException"/> class with a message and exercise information.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="obj">Information about the exercise that was not found.</param>
        public ExerciseNotFoundException(string message, object obj) : base(message)
        {
            ExerciseInfo = obj;
        }
    }
}
