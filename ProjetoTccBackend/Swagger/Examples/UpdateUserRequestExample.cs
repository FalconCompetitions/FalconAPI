using ProjetoTccBackend.Database.Requests.User;
using ProjetoTccBackend.Swagger.Interfaces;

namespace ProjetoTccBackend.Swagger.Examples
{
    public class UpdateUserRequestExample : ISwaggerExampleProvider<UpdateUserRequest>
    {
        public UpdateUserRequest GetExample() => new UpdateUserRequest()
        {
            Name = "Novo Nome",
            Email = "novo@email.com",
            PhoneNumber = "11999999999",
            JoinYear = 2024
        };
    }
}
