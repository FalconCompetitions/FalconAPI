using ProjetoTccBackend.Models;
using ProjetoTccBackend.Swagger.Interfaces;
using System;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class ExerciseExample : ISwaggerExampleProvider<Exercise>
    {
        public Exercise GetExample() => new Exercise()
        {
            Id = 1,
            Title = "Soma de Números",
            Description = "Some dois números inteiros.",
            EstimatedTime = TimeSpan.FromMinutes(30),
            // Adicione outros campos conforme necessário
        };
    }
}
