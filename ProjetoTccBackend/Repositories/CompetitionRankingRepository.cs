using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class CompetitionRankingRepository : GenericRepository<CompetitionRanking>, ICompetitionRankingRepository
    {
        public CompetitionRankingRepository(TccDbContext tccDbContext) : base(tccDbContext)
        {

        }


    }
}
