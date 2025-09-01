using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class CreateGroupRequestExample : ISwaggerExampleProvider<CreateGroupRequest>
    {
        public CreateGroupRequest GetExample() => new CreateGroupRequest()
        {
            Name = "Grupo Exemplo"
        };
    }
}
