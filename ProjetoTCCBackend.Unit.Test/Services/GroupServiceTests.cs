using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using MockQueryable.Moq;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Exceptions.Group;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class GroupServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGroupRepository> _groupRepositoryMock;
        private readonly Mock<IGroupInviteService> _groupInviteServiceMock;
        private readonly Mock<ILogger<GroupService>> _loggerMock;
        private TccDbContext? _dbContext;

        public GroupServiceTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _groupRepositoryMock = new Mock<IGroupRepository>();
            _groupInviteServiceMock = new Mock<IGroupInviteService>();
            _loggerMock = new Mock<ILogger<GroupService>>();
        }

        private GroupService CreateService()
        {
            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");

            // Novo mock obrigatório para o construtor
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            return new GroupService(
                _userServiceMock.Object,
                _userRepositoryMock.Object,
                _groupRepositoryMock.Object,
                _groupInviteServiceMock.Object,
                _dbContext,
                _loggerMock.Object,
                httpContextAccessorMock.Object // <- adicionado
            );
        }

        [Fact]
        public async Task CreateGroupAsync_CreatesGroup_WhenUserHasNoGroup()
        {
            // Arrange
            var loggedUser = new User
            {
                Id = "user1",
                UserName = "testuser",
                Email = "test@test.com",
                GroupId = null,
            };

            var request = new CreateGroupRequest { Name = "Test Group", UserRAs = null };

            // Simulates the group after being saved to the database with generated ID
            var savedGroup = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "user1",
                Users = new List<User> { loggedUser },
                GroupInvites = new List<GroupInvite>(),
            };

            // First call: check if user already has a group (returns empty)
            var emptyGroupsQuery = new List<Group>().AsQueryable().BuildMock();

            // Second call: fetch the newly created group
            var groupQueryWithNew = new List<Group> { savedGroup }
                .AsQueryable()
                .BuildMock();

            _groupRepositoryMock
                .SetupSequence(r => r.Query())
                .Returns(emptyGroupsQuery)
                .Returns(groupQueryWithNew);

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);

            // Mock the Add to set the group ID
            _groupRepositoryMock
                .Setup(r => r.Add(It.IsAny<Group>()))
                .Callback<Group>(g => g.Id = 1);

            var service = CreateService();

            // Act
            var result = await service.CreateGroupAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Group", result.Name);
            Assert.Equal(1, result.Id);
            _groupRepositoryMock.Verify(r => r.Add(It.IsAny<Group>()), Times.Once);
            _userRepositoryMock.Verify(r => r.Update(loggedUser), Times.Once);
        }

        [Fact]
        public async Task CreateGroupAsync_ThrowsException_WhenUserAlreadyHasGroup()
        {
            // Arrange
            var loggedUser = new User
            {
                Id = "user1",
                UserName = "testuser",
                Email = "test@test.com",
            };

            var existingGroup = new Group
            {
                Id = 1,
                Name = "Existing Group",
                LeaderId = "user1",
            };

            var request = new CreateGroupRequest { Name = "New Group" };

            var groups = new List<Group> { existingGroup }
                .AsQueryable()
                .BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(groups);
            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<UserHasGroupException>(() =>
                service.CreateGroupAsync(request)
            );
        }

        [Fact]
        public async Task CreateGroupAsync_SendsInvites_WhenUserRAsProvided()
        {
            // Arrange
            var loggedUser = new User
            {
                Id = "user1",
                UserName = "testuser",
                Email = "test@test.com",
                GroupId = null,
            };

            var invitedUser = new User
            {
                Id = "user2",
                RA = "12345",
                UserName = "inviteduser",
            };

            var request = new CreateGroupRequest
            {
                Name = "Test Group",
                UserRAs = new List<string> { "12345" },
            };

            var savedGroup = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "user1",
                Users = new List<User> { loggedUser },
                GroupInvites = new List<GroupInvite>(),
            };

            var emptyGroupsQuery = new List<Group>().AsQueryable().BuildMock();
            var groupQueryWithNew = new List<Group> { savedGroup }
                .AsQueryable()
                .BuildMock();

            _groupRepositoryMock
                .SetupSequence(r => r.Query())
                .Returns(emptyGroupsQuery)
                .Returns(groupQueryWithNew);

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);
            _userRepositoryMock.Setup(r => r.GetByRa("12345")).Returns(invitedUser);

            _groupRepositoryMock
                .Setup(r => r.Add(It.IsAny<Group>()))
                .Callback<Group>(g => g.Id = 1);

            var service = CreateService();

            // Act
            var result = await service.CreateGroupAsync(request);

            // Assert
            Assert.NotNull(result);
            _groupInviteServiceMock.Verify(
                s => s.SendGroupInviteToUser(It.IsAny<InviteUserToGroupRequest>()),
                Times.Once
            );
        }

        [Fact]
        public void ChangeGroupName_ChangesName_WhenUserIsInGroup()
        {
            // Arrange
            var loggedUser = new User { Id = "user1", GroupId = 1 };

            var group = new Group
            {
                Id = 1,
                Name = "Old Name",
                LeaderId = "user1",
            };

            var request = new ChangeGroupNameRequest { Id = 1, Name = "New Name" };

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);
            _groupRepositoryMock.Setup(r => r.GetById(1)).Returns(group);

            var service = CreateService();

            // Act
            var result = service.ChangeGroupName(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            _groupRepositoryMock.Verify(r => r.Update(It.IsAny<Group>()), Times.Once);
        }

        [Fact]
        public void ChangeGroupName_ReturnsNull_WhenGroupNotFound()
        {
            // Arrange
            var loggedUser = new User { Id = "user1", GroupId = 1 };

            var request = new ChangeGroupNameRequest { Id = 999, Name = "New Name" };

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);
            _groupRepositoryMock.Setup(r => r.GetById(999)).Returns((Group)null!);

            var service = CreateService();

            // Act
            var result = service.ChangeGroupName(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ChangeGroupName_ThrowsException_WhenUserNotInGroup()
        {
            // Arrange
            var loggedUser = new User { Id = "user1", GroupId = 2 };

            var group = new Group
            {
                Id = 1,
                Name = "Old Name",
                LeaderId = "user2",
            };

            var request = new ChangeGroupNameRequest { Id = 1, Name = "New Name" };

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);
            _groupRepositoryMock.Setup(r => r.GetById(1)).Returns(group);

            var service = CreateService();

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => service.ChangeGroupName(request));
        }

        [Fact]
        public void GetGroupById_ReturnsGroup_WhenUserHasAccess()
        {
            // Arrange
            var loggedUser = new User { Id = "user1", GroupId = 1 };

            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "user1",
                Users = new List<User> { loggedUser },
            };

            var groups = new List<Group> { group }
                .AsQueryable()
                .BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(groups);
            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);

            var service = CreateService();

            // Act
            var result = service.GetGroupById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Group", result.Name);
        }

        [Fact]
        public void GetGroupById_ThrowsException_WhenUserHasNoAccess()
        {
            // Arrange
            var loggedUser = new User { Id = "user1", GroupId = 2 };

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);

            var service = CreateService();

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => service.GetGroupById(1));
        }

        [Fact]
        public async Task GetGroupsAsync_ReturnsAllGroups_WithoutSearch()
        {
            // Arrange
            var user1 = new User
            {
                Id = "user1",
                UserName = "User 1",
                RA = "001",
            };
            var user2 = new User
            {
                Id = "user2",
                UserName = "User 2",
                RA = "002",
            };

            var groups = new List<Group>
            {
                new Group
                {
                    Id = 1,
                    Name = "Group 1",
                    LeaderId = "user1",
                    Users = new List<User> { user1 },
                },
                new Group
                {
                    Id = 2,
                    Name = "Group 2",
                    LeaderId = "user2",
                    Users = new List<User> { user2 },
                },
            };

            var mock = groups.AsQueryable().BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(mock);

            var service = CreateService();

            // Act
            var result = await service.GetGroupsAsync(1, 10, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetGroupsAsync_ReturnsFilteredGroups_WithSearch()
        {
            // Arrange
            var user1 = new User
            {
                Id = "user1",
                UserName = "User 1",
                RA = "001",
            };
            var user2 = new User
            {
                Id = "user2",
                UserName = "User 2",
                RA = "002",
            };

            var groups = new List<Group>
            {
                new Group
                {
                    Id = 1,
                    Name = "Python Group",
                    LeaderId = "user1",
                    Users = new List<User> { user1 },
                },
                new Group
                {
                    Id = 2,
                    Name = "Java Group",
                    LeaderId = "user2",
                    Users = new List<User> { user2 },
                },
            };

            var mock = groups.AsQueryable().BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(mock);

            var service = CreateService();

            // Act
            var result = await service.GetGroupsAsync(1, 10, "Python");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Python Group", result.Items.First().Name);
        }

        [Fact]
        public async Task GetGroupsAsync_ReturnsPaginatedResults()
        {
            // Arrange
            var groups = new List<Group>();
            for (int i = 1; i <= 5; i++)
            {
                groups.Add(
                    new Group
                    {
                        Id = i,
                        Name = $"Group {i}",
                        LeaderId = $"user{i}",
                        Users = new List<User>(),
                    }
                );
            }

            var mock = groups.AsQueryable().BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(mock);

            var service = CreateService();

            // Act
            var result = await service.GetGroupsAsync(1, 2, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task UpdateGroupAsync_UpdatesGroup_WhenUserIsLeader()
        {
            // Arrange
            var userId = "user1";
            var userRoles = new List<string> { "User" };

            var group = new Group
            {
                Id = 1,
                Name = "Old Name",
                LeaderId = userId,
                Users = new List<User>(),
            };

            var request = new UpdateGroupRequest
            {
                Name = "New Name",
                MembersToRemove = new List<string>(),
            };

            var groups = new List<Group> { group }
                .AsQueryable()
                .BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(groups);

            var service = CreateService();

            // Act
            var result = await service.UpdateGroupAsync(1, request, userId, userRoles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task UpdateGroupAsync_ReturnsNull_WhenGroupNotFound()
        {
            // Arrange
            var userId = "user1";
            var userRoles = new List<string> { "User" };

            var request = new UpdateGroupRequest
            {
                Name = "New Name",
                MembersToRemove = new List<string>(),
            };

            var emptyGroups = new List<Group>().AsQueryable().BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(emptyGroups);

            var service = CreateService();

            // Act
            var result = await service.UpdateGroupAsync(999, request, userId, userRoles);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateGroupAsync_ReturnsNull_WhenUserIsNotLeaderOrAdmin()
        {
            // Arrange
            var userId = "user1";
            var userRoles = new List<string> { "User" };

            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "user2", // Different user
                Users = new List<User>(),
            };

            var request = new UpdateGroupRequest
            {
                Name = "New Name",
                MembersToRemove = new List<string>(),
            };

            var groups = new List<Group> { group }
                .AsQueryable()
                .BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(groups);

            var service = CreateService();

            // Act
            var result = await service.UpdateGroupAsync(1, request, userId, userRoles);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateGroupAsync_AllowsUpdate_WhenUserIsAdmin()
        {
            // Arrange
            var userId = "user1";
            var userRoles = new List<string> { "Admin" };

            var group = new Group
            {
                Id = 1,
                Name = "Old Name",
                LeaderId = userId, // Admin is the leader to pass the first query
                Users = new List<User>(),
            };

            var request = new UpdateGroupRequest
            {
                Name = "New Name",
                MembersToRemove = new List<string>(),
            };

            var updatedGroup = new Group
            {
                Id = 1,
                Name = "New Name",
                LeaderId = userId,
                Users = new List<User>(),
            };

            var groupsQuery1 = new List<Group> { group }
                .AsQueryable()
                .BuildMock();
            var groupsQuery2 = new List<Group> { updatedGroup }
                .AsQueryable()
                .BuildMock();

            _groupRepositoryMock
                .SetupSequence(r => r.Query())
                .Returns(groupsQuery1)
                .Returns(groupsQuery2);

            var service = CreateService();

            // Act
            var result = await service.UpdateGroupAsync(1, request, userId, userRoles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
        }

        [Fact]
        public async Task UpdateGroupAsync_RemovesMembers_WhenSpecified()
        {
            // Arrange
            var userId = "user1";
            var userRoles = new List<string> { "User" };

            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = userId,
                Users = new List<User>(),
            };

            var request = new UpdateGroupRequest
            {
                Name = "Test Group",
                MembersToRemove = new List<string> { "user2", "user3" },
            };

            var groups = new List<Group> { group }
                .AsQueryable()
                .BuildMock();
            _groupRepositoryMock.Setup(r => r.Query()).Returns(groups);
            _groupInviteServiceMock
                .Setup(s => s.RemoveUserFromGroupAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var service = CreateService();

            // Act
            var result = await service.UpdateGroupAsync(1, request, userId, userRoles);

            // Assert
            Assert.NotNull(result);
            _groupInviteServiceMock.Verify(s => s.RemoveUserFromGroupAsync(1, "user2"), Times.Once);
            _groupInviteServiceMock.Verify(s => s.RemoveUserFromGroupAsync(1, "user3"), Times.Once);
        }
    }
}
