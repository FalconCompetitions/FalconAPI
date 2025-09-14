using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class PagedResultGroupResponseExample : ISwaggerExampleProvider<PagedResult<GroupResponse>>
    {
        public PagedResult<GroupResponse> GetExample() => new PagedResult<GroupResponse>
        {
            Items = new List<GroupResponse>
            {
                new GroupResponse
                {
                    Id = 1,
                    Name = "Grupo Exemplo",
                    LeaderId = "GUID",
                    Users = new List<GenericUserInfoResponse>
                    {
                        new GenericUserInfoResponse
                        {
                            Id = "user-uuid",
                            Name = "Usuário Exemplo",
                            Email = "exemplo@email.com",
                            JoinYear = 2024,
                            CreatedAt = DateTime.UtcNow
                        }
                    }
                }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };
    }

    public class PagedResultUserInfoResponseExample : ISwaggerExampleProvider<PagedResult<GenericUserInfoResponse>>
    {
        public PagedResult<GenericUserInfoResponse> GetExample() => new PagedResult<GenericUserInfoResponse>
        {
            Items = new List<GenericUserInfoResponse>
            {
                new GenericUserInfoResponse
                {
                    Id = "user-uuid",
                    Name = "Usuário Exemplo",
                    Email = "exemplo@email.com",
                    JoinYear = 2024,
                    CreatedAt = DateTime.UtcNow,
                }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };
    }
}
