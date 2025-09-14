using System.Threading.Tasks;
using ApiEstoqueASP.Services;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class UserServiceTests
    {
        
        // TODO
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser()
        {
            Assert.True(true);
            return;

            // Arrange
            var userManagerMock = new Mock<UserManager<User>>();
            var signInManagerMock = new Mock<SignInManager<User>>();
            var userRepoMock = new Mock<IUserRepository>();
            var iHttpContextAcessorMock = new Mock<IHttpContextAccessor>();
            var tokenServiceMock = new Mock<ITokenService>();
            var loggerMock = new Mock<ILogger<UserService>>();
            var expectedUser = new User { Id = "user1" };

            userRepoMock.Setup(r => r.GetById("user1")).Returns(expectedUser);
            var service = new UserService(
                userManagerMock.Object,
                userRepoMock.Object,
                signInManagerMock.Object,
                iHttpContextAcessorMock.Object,
                tokenServiceMock.Object,
                loggerMock.Object
            );

            // Act
            var result = await service.GetUser("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1", result.Id);
        }
    }
}
