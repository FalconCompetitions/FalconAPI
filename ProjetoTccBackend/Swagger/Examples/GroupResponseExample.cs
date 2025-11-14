using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class GroupResponseExample : ISwaggerExampleProvider<GroupResponse>
    {
        public GroupResponse GetExample() => new GroupResponse()
        {
            Id = 1,
            Name = "Grupo Exemplo",
            Users = new List<Database.Responses.User.GenericUserInfoResponse>
            {
                new Database.Responses.User.GenericUserInfoResponse
                {
                    Id = "UUID",
                    Name = "Usuário Exemplo",
                    Email = "exemplo@email.com",
                    JoinYear = 2024,
                    CreatedAt = DateTime.UtcNow,
                }
            }
        };
    }
}
