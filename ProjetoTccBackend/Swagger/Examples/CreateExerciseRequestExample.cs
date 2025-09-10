using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Swagger.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class CreateExerciseRequestExample : ISwaggerExampleProvider<CreateExerciseRequest>
    {
        public CreateExerciseRequest GetExample() => new CreateExerciseRequest()
        {
            ExerciseTypeId = 1,
            Title = "Soma de Números",
            Description = "Some dois números inteiros.",
            EstimatedTime = TimeSpan.FromMinutes(30),
            Inputs = new List<CreateExerciseInputRequest>(),
            Outputs = new List<CreateExerciseOutputRequest>()
        };
    }
}
