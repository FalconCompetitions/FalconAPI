using ProjetoTccBackend.Models;
using ProjetoTccBackend.Swagger.Interfaces;
using System;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="Exercise"/> for Swagger documentation.
    /// </summary>
    public class ExerciseExample : ISwaggerExampleProvider<Exercise>
    {
        /// <summary>
        /// Gets an example instance of <see cref="Exercise"/>.
        /// </summary>
        /// <returns>An example exercise.</returns>
        public Exercise GetExample() => new Exercise()
        {
            Id = 1,
            Title = "Sum of Numbers",
            Description = "Sum two integer numbers.",
            EstimatedTime = TimeSpan.FromMinutes(30),
            // Add other fields as needed
        };
    }
}
