using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Swagger.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class UpdateExerciseRequestExample : ISwaggerExampleProvider<UpdateExerciseRequest>
    {
        public UpdateExerciseRequest GetExample() => new UpdateExerciseRequest()
        {
            Id = 1,
            ExerciseTypeId = 1,
            Title = "Nova Soma",
            Description = "Atualize a soma de dois números.",
            Inputs = new List<UpdateExerciseInputRequest>(),
            Outputs = new List<UpdateExerciseOutputRequest>()
        };
    }
}
