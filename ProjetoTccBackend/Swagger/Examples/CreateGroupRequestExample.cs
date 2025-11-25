using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="CreateGroupRequest"/> for Swagger documentation.
    /// </summary>
    public class CreateGroupRequestExample : ISwaggerExampleProvider<CreateGroupRequest>
    {
        /// <summary>
        /// Gets an example instance of <see cref="CreateGroupRequest"/>.
        /// </summary>
        /// <returns>An example create group request.</returns>
        public CreateGroupRequest GetExample() => new CreateGroupRequest()
        {
            Name = "Example Group"
        };
    }
}
