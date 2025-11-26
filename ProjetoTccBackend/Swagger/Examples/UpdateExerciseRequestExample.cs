using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Swagger.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="UpdateExerciseRequest"/> for Swagger documentation.
    /// </summary>
    public class UpdateExerciseRequestExample : ISwaggerExampleProvider<UpdateExerciseRequest>
    {
        /// <summary>
        /// Gets an example instance of <see cref="UpdateExerciseRequest"/>.
        /// </summary>
        /// <returns>An example update exercise request.</returns>
        public UpdateExerciseRequest GetExample() => new UpdateExerciseRequest()
        {
            Id = 1,
            ExerciseTypeId = 1,
            Title = "New Sum",
            Description = "Update the sum of two numbers.",
            Inputs = new List<UpdateExerciseInputRequest>(),
            Outputs = new List<UpdateExerciseOutputRequest>()
        };
    }
}
