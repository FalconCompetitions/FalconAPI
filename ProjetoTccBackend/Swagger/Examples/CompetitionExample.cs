using ProjetoTccBackend.Models;
using ProjetoTccBackend.Swagger.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="Competition"/> for Swagger documentation.
    /// </summary>
    public class CompetitionExample : ISwaggerExampleProvider<Competition>
    {
        /// <summary>
        /// Gets an example instance of <see cref="Competition"/>.
        /// </summary>
        /// <returns>An example competition.</returns>
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
