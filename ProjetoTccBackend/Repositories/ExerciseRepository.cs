using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class ExerciseRepository : GenericRepository<Exercise>, IExerciseRepository
    {
        public ExerciseRepository(TccDbContext dbContext)
            : base(dbContext) { }

        public override Exercise? GetById(int id)
        {
            return this
                ._dbContext.Exercises.Include(e => e.ExerciseInputs)
                .Include(e => e.ExerciseOutputs)
                .Where(e => e.Id.Equals(id))
                .FirstOrDefault();
        }
    }
}
