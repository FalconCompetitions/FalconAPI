using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Swagger.Interfaces;
using System;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class CompetitionRequestExample : ISwaggerExampleProvider<CompetitionRequest>
    {
        public CompetitionRequest GetExample() => new CompetitionRequest()
        {
            StartTime = DateTime.UtcNow,
        };
    }
}
