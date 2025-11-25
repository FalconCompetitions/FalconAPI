using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Swagger.Interfaces;
using System;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="CompetitionRequest"/> for Swagger documentation.
    /// </summary>
    public class CompetitionRequestExample : ISwaggerExampleProvider<CompetitionRequest>
    {
        /// <summary>
        /// Gets an example instance of <see cref="CompetitionRequest"/>.
        /// </summary>
        /// <returns>An example competition request.</returns>
        public CompetitionRequest GetExample() => new CompetitionRequest()
        {
            StartTime = DateTime.UtcNow,
        };
    }
}
