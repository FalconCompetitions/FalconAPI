using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Swagger.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="CreateExerciseRequest"/> for Swagger documentation.
    /// </summary>
    public class CreateExerciseRequestExample : ISwaggerExampleProvider<CreateExerciseRequest>
    {
        /// <summary>
        /// Gets an example instance of <see cref="CreateExerciseRequest"/>.
        /// </summary>
        /// <returns>An example create exercise request.</returns>
        public CreateExerciseRequest GetExample() => new CreateExerciseRequest()
        {
            ExerciseTypeId = 1,
            Title = "Sum of Numbers",
            Description = "Sum two integer numbers.",
            Inputs = new List<CreateExerciseInputRequest>(),
            Outputs = new List<CreateExerciseOutputRequest>()
        };
    }
}
