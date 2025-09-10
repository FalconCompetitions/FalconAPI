using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class CompetitionRepository : GenericRepository<Competition>, ICompetitionRepository
    {
        public CompetitionRepository(TccDbContext dbContext)
            : base(dbContext) { }

        /// <inheritdoc/>
        public async Task<ICollection<Question>> GetCompetitionQuestions(int competitionId)
        {
            List<Question> questionList = await this
                ._dbContext.Competitions.Where(c => c.Id.Equals(competitionId))
                .SelectMany(c => c.Questions)
                .ToListAsync();

            return questionList;
        }

        /// <inheritdoc />
        public async Task<ICollection<Competition>> GetOpenCompetitionsAsync()
        {
            return await this
                ._dbContext.Competitions.Where(x =>
                    x.Status != Enums.Competition.CompetitionStatus.Finished
                )
                .ToListAsync();
        }
    }
}
