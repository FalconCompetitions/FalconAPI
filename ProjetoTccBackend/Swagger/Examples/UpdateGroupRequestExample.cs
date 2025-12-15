using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Swagger.Interfaces;
using System.Collections.Generic;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="UpdateGroupRequest"/> for Swagger documentation.
    /// </summary>
    public class UpdateGroupRequestExample : ISwaggerExampleProvider<UpdateGroupRequest>
    {
        /// <summary>
        /// Gets an example instance of <see cref="UpdateGroupRequest"/>.
        /// </summary>
        /// <returns>An example update group request.</returns>
        public UpdateGroupRequest GetExample() => new UpdateGroupRequest()
        {
            Name = "New Group Name"
        };
    }
}
