using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class ExerciseSubmissionQueueItemRepository
        : GenericRepository<ExerciseSubmissionQueueItem>,
            IExerciseSubmissionQueueItemRepository
    {
        public ExerciseSubmissionQueueItemRepository(TccDbContext dbContext)
            : base(dbContext) { }
    }
}
