using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.Group;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using System.Security.Claims;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    /// <summary>
    /// Unit tests for the <see cref="GroupService"/> class.
    /// </summary>
    public class GroupServiceTests : IDisposable
    {
        private readonly TccDbContext _dbContext;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IGroupInviteService> _groupInviteServiceMock;
        private readonly Mock<ILogger<GroupService>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly GroupService _groupService;

        public GroupServiceTests()
        {
            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");
            
            var userRepository = new UserRepository(_dbContext);
            var groupRepository = new GroupRepository(_dbContext);
            
            _userServiceMock = new Mock<IUserService>();
            _groupInviteServiceMock = new Mock<IGroupInviteService>();
            _loggerMock = new Mock<ILogger<GroupService>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _groupService = new GroupService(
                _userServiceMock.Object,
                userRepository,
                groupRepository,
                _groupInviteServiceMock.Object,
                _dbContext,
                _loggerMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        [Fact]
        public async Task CreateGroupAsync_CreatesGroup_Successfully()
        {
            // Arrange
            var user = new User { Id = "user1", Name = "Test User", UserName = "Test User", RA = "123456", Email = "test@test.com" };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(user);

            var request = new CreateGroupRequest
            {
                Name = "Test Group",
                UserRAs = null
            };

            // Act
            var result = await _groupService.CreateGroupAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Group", result.Name);
            Assert.Equal(user.Id, result.LeaderId);
            
            var updatedUser = await _dbContext.Users.FindAsync(user.Id);
            Assert.Equal(result.Id, updatedUser!.GroupId);
        }

        [Fact]
        public async Task CreateGroupAsync_ThrowsException_WhenUserAlreadyHasGroup()
        {
            // Arrange
            var user = new User { Id = "user1", Name = "Test User", UserName = "Test User", RA = "123456", Email = "test@test.com" };
            var existingGroup = new Group { Name = "Existing Group", LeaderId = user.Id };
            
            _dbContext.Users.Add(user);
            _dbContext.Groups.Add(existingGroup);
            await _dbContext.SaveChangesAsync();

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(user);

            var request = new CreateGroupRequest { Name = "New Group" };

            // Act & Assert
            await Assert.ThrowsAsync<UserHasGroupException>(() => _groupService.CreateGroupAsync(request));
        }

        [Fact]
        public async Task CreateGroupAsync_SendsInvites_WhenUserRAsProvided()
        {
            // Arrange
            var leader = new User { Id = "leader1", Name = "Leader", UserName = "Leader", RA = "111111", Email = "leader@test.com" };
            var invitee = new User { Id = "invitee1", Name = "Invitee", UserName = "Invitee", RA = "222222", Email = "invitee@test.com" };
            
            _dbContext.Users.AddRange(leader, invitee);
            await _dbContext.SaveChangesAsync();

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(leader);

            var request = new CreateGroupRequest
            {
                Name = "Test Group",
                UserRAs = new List<string> { "222222" }
            };

            // Act
            var result = await _groupService.CreateGroupAsync(request);

            // Assert
            Assert.NotNull(result);
            _groupInviteServiceMock.Verify(
                s => s.SendGroupInviteToUser(It.Is<InviteUserToGroupRequest>(r => r.RA == "222222" && r.GroupId == result.Id)),
                Times.Once
            );
        }

        [Fact]
        public void ChangeGroupName_ChangesName_WhenUserIsLeader()
        {
            // Arrange
            var user = new User { Id = "user1", Name = "Test User", UserName = "Test User", RA = "123456", Email = "test@test.com" };
            var group = new Group { Id = 1, Name = "Old Name", LeaderId = user.Id };
            
            _dbContext.Users.Add(user);
            _dbContext.Groups.Add(group);
            _dbContext.SaveChanges();

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(user);
            
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Student")
            }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            var request = new ChangeGroupNameRequest { Id = 1, Name = "New Name" };

            // Act
            var result = _groupService.ChangeGroupName(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            
            var updatedGroup = _dbContext.Groups.Find(1);
            Assert.Equal("New Name", updatedGroup!.Name);
        }

        [Fact]
        public void ChangeGroupName_ThrowsException_WhenUserIsNotLeaderOrAdmin()
        {
            // Arrange
            var user = new User { Id = "user1", Name = "Test User", UserName = "Test User", RA = "123456", Email = "test@test.com" };
            var otherUser = new User { Id = "user2", Name = "Other User", UserName = "Other User", RA = "654321", Email = "other@test.com" };
            var group = new Group { Id = 1, Name = "Test Group", LeaderId = otherUser.Id };
            
            _dbContext.Users.AddRange(user, otherUser);
            _dbContext.Groups.Add(group);
            _dbContext.SaveChanges();

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(user);
            
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Student")
            }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            var request = new ChangeGroupNameRequest { Id = 1, Name = "New Name" };

            // Act & Assert
            Assert.Throws<FormException>(() => _groupService.ChangeGroupName(request));
        }

        [Fact]
        public void ChangeGroupName_ReturnsNull_WhenGroupNotFound()
        {
            // Arrange
            var user = new User { Id = "user1", Name = "Test User", UserName = "Test User", RA = "123456", Email = "test@test.com" };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(user);

            var request = new ChangeGroupNameRequest { Id = 999, Name = "New Name" };

            // Act
            var result = _groupService.ChangeGroupName(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetGroupById_ReturnsGroup_WhenUserIsLeader()
        {
            // Arrange
            var user = new User { Id = "user1", Name = "Test User", UserName = "Test User", RA = "123456", Email = "test@test.com", GroupId = 1 };
            var group = new Group { Id = 1, Name = "Test Group", LeaderId = user.Id };
            
            _dbContext.Users.Add(user);
            _dbContext.Groups.Add(group);
            _dbContext.SaveChanges();

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(user);
            
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Student")
            }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = _groupService.GetGroupById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Group", result.Name);
        }

        [Fact]
        public void GetGroupById_ThrowsException_WhenStudentAccessesOtherGroup()
        {
            // Arrange
            var user = new User { Id = "user1", Name = "Test User", UserName = "Test User", RA = "123456", Email = "test@test.com", GroupId = 2 };
            var otherUser = new User { Id = "user2", Name = "Other User", UserName = "Other User", RA = "654321", Email = "other@test.com" };
            var group = new Group { Id = 1, Name = "Test Group", LeaderId = otherUser.Id };
            
            _dbContext.Users.AddRange(user, otherUser);
            _dbContext.Groups.Add(group);
            _dbContext.SaveChanges();

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(user);
            
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Student")
            }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act & Assert
            Assert.Throws<FormException>(() => _groupService.GetGroupById(1));
        }

        [Fact]
        public async Task GetGroupsAsync_ReturnsAllGroups_WithoutSearch()
        {
            // Arrange
            var groups = new List<Group>
            {
                new Group { Name = "Group A", LeaderId = "user1" },
                new Group { Name = "Group B", LeaderId = "user2" },
                new Group { Name = "Group C", LeaderId = "user3" }
            };
            _dbContext.Groups.AddRange(groups);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupService.GetGroupsAsync(1, 10, null);

            // Assert
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count());
        }

        [Fact]
        public async Task GetGroupsAsync_FiltersGroups_WithSearch()
        {
            // Arrange
            var groups = new List<Group>
            {
                new Group { Name = "Alpha Group", LeaderId = "user1" },
                new Group { Name = "Beta Group", LeaderId = "user2" },
                new Group { Name = "Alpha Team", LeaderId = "user3" }
            };
            _dbContext.Groups.AddRange(groups);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupService.GetGroupsAsync(1, 10, "Alpha");

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
            Assert.All(result.Items, item => Assert.Contains("Alpha", item.Name));
        }

        [Fact]
        public async Task GetGroupsAsync_ReturnsPaginatedResults()
        {
            // Arrange
            var groups = new List<Group>();
            for (int i = 1; i <= 15; i++)
            {
                groups.Add(new Group { Name = $"Group {i}", LeaderId = $"user{i}" });
            }
            _dbContext.Groups.AddRange(groups);
            await _dbContext.SaveChangesAsync();

            // Act
            var page1 = await _groupService.GetGroupsAsync(1, 10, null);
            var page2 = await _groupService.GetGroupsAsync(2, 10, null);

            // Assert
            Assert.Equal(15, page1.TotalCount);
            Assert.Equal(10, page1.Items.Count());
            Assert.Equal(15, page2.TotalCount);
            Assert.Equal(5, page2.Items.Count());
        }
    }
}


