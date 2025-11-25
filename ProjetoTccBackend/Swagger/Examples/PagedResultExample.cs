using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="PagedResult{GroupResponse}"/> for Swagger documentation.
    /// </summary>
    public class PagedResultGroupResponseExample : ISwaggerExampleProvider<PagedResult<GroupResponse>>
    {
        /// <summary>
        /// Gets an example instance of <see cref="PagedResult{GroupResponse}"/>.
        /// </summary>
        /// <returns>An example paged result of group responses.</returns>
        public PagedResult<GroupResponse> GetExample() => new PagedResult<GroupResponse>
        {
            Items = new List<GroupResponse>
            {
                new GroupResponse
                {
                    Id = 1,
                    Name = "Example Group",
                    LeaderId = "GUID",
                    Users = new List<GenericUserInfoResponse>
                    {
                        new GenericUserInfoResponse
                        {
                            Id = "user-uuid",
                            Name = "Example User",
                            Email = "example@email.com",
                            JoinYear = 2024,
                            CreatedAt = DateTime.UtcNow,
                            Department = "Department",
                            LastLoggedAt = DateTime.UtcNow,
                            Ra = "000000"
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

    /// <summary>
    /// Provides example instances of <see cref="PagedResult{GenericUserInfoResponse}"/> for Swagger documentation.
    /// </summary>
    public class PagedResultUserInfoResponseExample : ISwaggerExampleProvider<PagedResult<GenericUserInfoResponse>>
    {
        /// <summary>
        /// Gets an example instance of <see cref="PagedResult{GenericUserInfoResponse}"/>.
        /// </summary>
        /// <returns>An example paged result of user info responses.</returns>
        public PagedResult<GenericUserInfoResponse> GetExample() => new PagedResult<GenericUserInfoResponse>
        {
            Items = new List<GenericUserInfoResponse>
            {
                new GenericUserInfoResponse
                {
                    Id = "user-uuid",
                    Name = "Example User",
                    Email = "example@email.com",
                    JoinYear = 2024,
                    CreatedAt = DateTime.UtcNow,
                    Department = "Department",
                    Ra = "0000000",
                    LastLoggedAt = DateTime.UtcNow,
                }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };
    }
}
