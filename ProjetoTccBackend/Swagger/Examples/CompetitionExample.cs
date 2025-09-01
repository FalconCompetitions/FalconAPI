using ProjetoTccBackend.Models;
using ProjetoTccBackend.Swagger.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class CompetitionExample : ISwaggerExampleProvider<Competition>
    {
        public Competition GetExample() => new Competition()
        {
            Id = 1,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2),
            Duration = TimeSpan.FromHours(2),
            StopRanking = DateTime.UtcNow.AddHours(1.5),
            BlockSubmissions = DateTime.UtcNow.AddHours(2),
            SubmissionPenalty = TimeSpan.FromMinutes(10),
            // Collections left empty for brevity
        };
    }
}
