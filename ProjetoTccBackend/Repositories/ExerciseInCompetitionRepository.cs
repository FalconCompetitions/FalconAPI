using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class ExerciseInCompetitionRepository : GenericRepository<ExerciseInCompetition>, IExerciseInCompetitionRepository
    {
        public ExerciseInCompetitionRepository(TccDbContext dbContext) : base(dbContext)
        {

        }


    }
}
