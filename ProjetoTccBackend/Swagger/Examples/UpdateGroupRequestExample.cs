using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class UpdateGroupRequestExample : ISwaggerExampleProvider<UpdateGroupRequest>
    {
        public UpdateGroupRequest GetExample() => new UpdateGroupRequest()
        {
            Name = "Novo Nome do Grupo"
        };
    }
}
