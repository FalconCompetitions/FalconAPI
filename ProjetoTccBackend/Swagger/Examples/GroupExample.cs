using ProjetoTccBackend.Models;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    /// <summary>
    /// Provides example instances of <see cref="Group"/> for Swagger documentation.
    /// </summary>
    public class GroupExample : ISwaggerExampleProvider<Group>
    {
        /// <summary>
        /// Gets an example instance of <see cref="Group"/>.
        /// </summary>
        /// <returns>An example group.</returns>
        public Group GetExample() => new Group()
        {
            Id = 1,
            Name = "Group name",
        };
    }
}
