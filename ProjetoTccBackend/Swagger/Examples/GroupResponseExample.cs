using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="GroupResponse"/> for Swagger documentation.
    /// </summary>
    public class GroupResponseExample : ISwaggerExampleProvider<GroupResponse>
    {
        /// <summary>
        /// Gets an example instance of <see cref="GroupResponse"/>.
        /// </summary>
        /// <returns>An example group response.</returns>
        public GroupResponse GetExample() => new GroupResponse()
        {
            Id = 1,
            Name = "Example Group",
            Users = new List<Database.Responses.User.GenericUserInfoResponse>
            {
                new Database.Responses.User.GenericUserInfoResponse
                {
                    Id = "UUID",
                    Name = "Example User",
                    Email = "example@email.com",
                    JoinYear = 2024,
                    CreatedAt = DateTime.UtcNow,
                }
            }
        };
    }
}
