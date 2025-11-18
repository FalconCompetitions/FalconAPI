namespace ProjetoTccBackend.Exceptions.Judge
{
    public class ExerciseNotFoundException : Exception
    {
        private object ExerciseInfo { get; set; }

        public ExerciseNotFoundException()
        {

        }

        public ExerciseNotFoundException(object obj)
        {
            ExerciseInfo = obj;
        }

        public ExerciseNotFoundException(string message, object obj) : base(message)
        {
            ExerciseInfo = obj;
        }
    }
}
