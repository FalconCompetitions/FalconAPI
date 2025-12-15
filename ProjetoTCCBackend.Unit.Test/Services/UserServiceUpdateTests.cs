using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Database.Requests.User;
using ProjetoTccBackend.Services.Interfaces;
using ApiEstoqueASP.Services;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    /// <summary>
    /// Unit tests for UpdateUserAsync method in UserService.
    /// Ensures that user updates, including department field, are correctly persisted.
    /// </summary>
    public class UserServiceUpdateTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly UserService _userService;

        public UserServiceUpdateTests()
        {
            // Create minimal mocks needed for UserService
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<User>>(),
                Array.Empty<IUserValidator<User>>(),
                Array.Empty<IPasswordValidator<User>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<User>>>());

            _mockUserRepository = new Mock<IUserRepository>();
            var mockGroupInviteRepository = new Mock<IGroupInviteRepository>();
            var mockCompetitionRankingRepository = new Mock<ICompetitionRankingRepository>();
            
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<User>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<User>>());

            var mockTokenService = new Mock<ITokenService>();
            var mockLogger = new Mock<ILogger<UserService>>();

            _userService = new UserService(
                _mockUserManager.Object,
                _mockUserRepository.Object,
                mockGroupInviteRepository.Object,
                mockCompetitionRankingRepository.Object,
                mockSignInManager.Object,
                contextAccessor.Object,
                mockTokenService.Object,
                mockLogger.Object
            );
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateDepartmentField_WhenDepartmentIsProvided()
        {
            // Arrange
            var userId = "test-user-id";
            var existingUser = new User
            {
                Id = userId,
                Name = "John Doe",
                Email = "john@example.com",
                UserName = "john@example.com",
                RA = "123456",
                JoinYear = 2020,
                Department = null // Initially null
            };

            var updateRequest = new UpdateUserRequest
            {
                Name = "John Doe",
                Email = "john@example.com",
                JoinYear = 2020,
                Department = "Computer Science" // New department
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(existingUser);
            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Computer Science", result.Department);
            _mockUserManager.Verify(m => m.UpdateAsync(It.Is<User>(u => 
                u.Department == "Computer Science")), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateDepartmentToNull_WhenDepartmentIsNull()
        {
            // Arrange
            var userId = "test-user-id";
            var existingUser = new User
            {
                Id = userId,
                Name = "Jane Doe",
                Email = "jane@example.com",
                UserName = "jane@example.com",
                RA = "789012",
                JoinYear = 2021,
                Department = "Mathematics" // Has existing department
            };

            var updateRequest = new UpdateUserRequest
            {
                Name = "Jane Doe",
                Email = "jane@example.com",
                JoinYear = 2021,
                Department = null // Clearing department
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(existingUser);
            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Department);
            _mockUserManager.Verify(m => m.UpdateAsync(It.Is<User>(u => 
                u.Department == null)), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateAllFields_IncludingDepartment()
        {
            // Arrange
            var userId = "test-user-id";
            var existingUser = new User
            {
                Id = userId,
                Name = "Old Name",
                Email = "old@example.com",
                UserName = "old@example.com",
                RA = "111111",
                JoinYear = 2019,
                Department = "Old Department",
                PhoneNumber = "1234567890"
            };

            var updateRequest = new UpdateUserRequest
            {
                Name = "New Name",
                Email = "new@example.com",
                JoinYear = 2022,
                Department = "New Department",
                PhoneNumber = "9876543210"
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(existingUser);
            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal("new@example.com", result.Email);
            Assert.Equal("new@example.com", result.UserName);
            Assert.Equal(2022, result.JoinYear);
            Assert.Equal("New Department", result.Department);
            Assert.Equal("9876543210", result.PhoneNumber);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var userId = "non-existent-user";
            var updateRequest = new UpdateUserRequest
            {
                Name = "Test",
                Email = "test@example.com"
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns((User?)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateRequest);

            // Assert
            Assert.Null(result);
            _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
