using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.Group;
using ProjetoTccBackend.Exceptions.User;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using System.Security.Claims;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    /// <summary>
    /// Unit tests for the <see cref="GroupInviteService"/> class.
    /// </summary>
    public class GroupInviteServiceTests : IDisposable
    {
        private readonly TccDbContext _dbContext;
        private readonly GroupInviteService _groupInviteService;
        private readonly Mock<ILogger<GroupInviteService>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private User? _mockLoggedUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupInviteServiceTests"/> class.
        /// Sets up the in-memory database and service dependencies.
        /// </summary>
        public GroupInviteServiceTests()
        {
            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");

            var userRepository = new UserRepository(_dbContext);
            var groupRepository = new GroupRepository(_dbContext);
            var groupInviteRepository = new GroupInviteRepository(_dbContext);

            _loggerMock = new Mock<ILogger<GroupInviteService>>();
            _userServiceMock = new Mock<IUserService>();
            
            // Setup user service to return the mock logged user
            _userServiceMock.Setup(x => x.GetHttpContextLoggedUser()).Returns(() => _mockLoggedUser!);

            _groupInviteService = new GroupInviteService(
                _userServiceMock.Object,
                userRepository,
                groupRepository,
                groupInviteRepository,
                _loggerMock.Object,
                _dbContext
            );
        }

        /// <summary>
        /// Cleans up resources used by the test class.
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }

        /// <summary>
        /// Sets up a mock logged user for the user service.
        /// </summary>
        /// <param name="user">The user to set as the logged-in user.</param>
        private void SetupHttpContextUser(User user)
        {
            _mockLoggedUser = user;
        }

        #region GetUserGroupInvites Tests

        /// <summary>
        /// Tests that GetUserGroupInvites returns an empty list when the user has no pending invitations.
        /// </summary>
        [Fact]
        public async Task GetUserGroupInvites_ReturnsEmptyList_WhenNoInvitesExist()
        {
            // Arrange
            var user = new User
            {
                Id = "user1",
                Name = "Test User",
                Email = "test@test.com",
                RA = "12345"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInviteService.GetUserGroupInvites(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetUserGroupInvites returns only pending invitations (not accepted).
        /// </summary>
        [Fact]
        public async Task GetUserGroupInvites_ReturnsPendingInvites_WhenInvitesExist()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, invitedUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            var pendingInvite = new GroupInvite
            {
                GroupId = group.Id,
                UserId = invitedUser.Id,
                Accepted = false
            };

            var acceptedInvite = new GroupInvite
            {
                GroupId = group.Id,
                UserId = invitedUser.Id,
                Accepted = true
            };

            _dbContext.GroupInvites.AddRange(pendingInvite, acceptedInvite);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInviteService.GetUserGroupInvites(invitedUser.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.False(result[0].Accepted);
            Assert.Equal(group.Id, result[0].GroupId);
        }

        /// <summary>
        /// Tests that GetUserGroupInvites includes group and users navigation properties.
        /// </summary>
        [Fact]
        public async Task GetUserGroupInvites_IncludesGroupAndUsers_WhenInvitesExist()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, invitedUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            // Set GroupId after group is saved
            leader.GroupId = group.Id;
            _dbContext.Users.Update(leader);
            await _dbContext.SaveChangesAsync();

            var invite = new GroupInvite
            {
                GroupId = group.Id,
                UserId = invitedUser.Id,
                Accepted = false
            };

            _dbContext.GroupInvites.Add(invite);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInviteService.GetUserGroupInvites(invitedUser.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotNull(result[0].Group);
            Assert.Equal("Test Group", result[0].Group.Name);
            Assert.NotNull(result[0].Group.Users);
            Assert.Single(result[0].Group.Users);
        }

        #endregion

        #region SendGroupInviteToUser Tests

        /// <summary>
        /// Tests that SendGroupInviteToUser throws UserNotFoundException when user with specified RA doesn't exist.
        /// </summary>
        [Fact]
        public async Task SendGroupInviteToUser_ThrowsUserNotFoundException_WhenUserNotFound()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            leader.GroupId = group.Id;

            _dbContext.Users.Add(leader);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(leader);

            var request = new InviteUserToGroupRequest
            {
                RA = "99999",
                GroupId = group.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(
                () => _groupInviteService.SendGroupInviteToUser(request)
            );
        }

        /// <summary>
        /// Tests that SendGroupInviteToUser returns null when group doesn't exist.
        /// </summary>
        [Fact]
        public async Task SendGroupInviteToUser_ReturnsNull_WhenGroupNotFound()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "22222"
            };

            _dbContext.Users.AddRange(leader, invitedUser);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(leader);

            var request = new InviteUserToGroupRequest
            {
                RA = invitedUser.RA,
                GroupId = 999
            };

            // Act
            var result = await _groupInviteService.SendGroupInviteToUser(request);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SendGroupInviteToUser throws UserHasGroupException when invited user already belongs to a group.
        /// </summary>
        [Fact]
        public async Task SendGroupInviteToUser_ThrowsUserHasGroupException_WhenUserAlreadyInGroup()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "22222"
            };

            var group1 = new Group
            {
                Name = "Group 1",
                LeaderId = leader.Id
            };

            var group2 = new Group
            {
                Name = "Group 2",
                LeaderId = invitedUser.Id
            };

            _dbContext.Users.AddRange(leader, invitedUser);
            _dbContext.Groups.AddRange(group1, group2);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group1.Id;
            invitedUser.GroupId = group2.Id;
            _dbContext.Users.UpdateRange(leader, invitedUser);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(leader);

            var request = new InviteUserToGroupRequest
            {
                RA = invitedUser.RA,
                GroupId = group1.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<UserHasGroupException>(
                () => _groupInviteService.SendGroupInviteToUser(request)
            );
        }

        /// <summary>
        /// Tests that SendGroupInviteToUser throws UserNotGroupLeaderException when logged user is not the group leader.
        /// </summary>
        [Fact]
        public async Task SendGroupInviteToUser_ThrowsUserNotGroupLeaderException_WhenUserNotLeader()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var member = new User
            {
                Id = "member1",
                Name = "Member User",
                Email = "member@test.com",
                RA = "33333"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            leader.GroupId = group.Id;
            member.GroupId = group.Id;

            _dbContext.Users.AddRange(leader, member, invitedUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(member); // Member, not leader

            var request = new InviteUserToGroupRequest
            {
                RA = invitedUser.RA,
                GroupId = group.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<UserNotGroupLeaderException>(
                () => _groupInviteService.SendGroupInviteToUser(request)
            );
        }

        /// <summary>
        /// Tests that SendGroupInviteToUser throws MaxMembersExceededException when group has 3 members.
        /// </summary>
        [Fact]
        public async Task SendGroupInviteToUser_ThrowsMaxMembersExceededException_WhenGroupFull()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var member1 = new User
            {
                Id = "member1",
                Name = "Member 1",
                Email = "member1@test.com",
                RA = "22222"
            };

            var member2 = new User
            {
                Id = "member2",
                Name = "Member 2",
                Email = "member2@test.com",
                RA = "33333"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "44444"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, member1, member2, invitedUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group.Id;
            member1.GroupId = group.Id;
            member2.GroupId = group.Id;
            _dbContext.Users.UpdateRange(leader, member1, member2);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(leader);

            var request = new InviteUserToGroupRequest
            {
                RA = invitedUser.RA,
                GroupId = group.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<MaxMembersExceededException>(
                () => _groupInviteService.SendGroupInviteToUser(request)
            );
        }

        /// <summary>
        /// Tests that SendGroupInviteToUser throws MaxMembersExceededException when group has 2 members and 1 pending invite.
        /// </summary>
        [Fact]
        public async Task SendGroupInviteToUser_ThrowsMaxMembersExceededException_WhenGroupHasPendingInvites()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var member1 = new User
            {
                Id = "member1",
                Name = "Member 1",
                Email = "member1@test.com",
                RA = "22222"
            };

            var pendingUser = new User
            {
                Id = "pending1",
                Name = "Pending User",
                Email = "pending@test.com",
                RA = "33333"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "44444"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, member1, pendingUser, invitedUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group.Id;
            member1.GroupId = group.Id;
            _dbContext.Users.UpdateRange(leader, member1);
            await _dbContext.SaveChangesAsync();

            var pendingInvite = new GroupInvite
            {
                GroupId = group.Id,
                UserId = pendingUser.Id,
                Accepted = false
            };

            _dbContext.GroupInvites.Add(pendingInvite);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(leader);

            var request = new InviteUserToGroupRequest
            {
                RA = invitedUser.RA,
                GroupId = group.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<MaxMembersExceededException>(
                () => _groupInviteService.SendGroupInviteToUser(request)
            );
        }

        /// <summary>
        /// Tests that SendGroupInviteToUser returns null when invitation already exists.
        /// </summary>
        [Fact]
        public async Task SendGroupInviteToUser_ReturnsNull_WhenInviteAlreadyExists()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            leader.GroupId = group.Id;

            _dbContext.Users.AddRange(leader, invitedUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            var existingInvite = new GroupInvite
            {
                GroupId = group.Id,
                UserId = invitedUser.Id,
                Accepted = false
            };

            _dbContext.GroupInvites.Add(existingInvite);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(leader);

            var request = new InviteUserToGroupRequest
            {
                RA = invitedUser.RA,
                GroupId = group.Id
            };

            // Act
            var result = await _groupInviteService.SendGroupInviteToUser(request);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SendGroupInviteToUser successfully creates an invitation when all conditions are met.
        /// </summary>
        [Fact]
        public async Task SendGroupInviteToUser_CreatesInvite_WhenAllConditionsMet()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            leader.GroupId = group.Id;

            _dbContext.Users.AddRange(leader, invitedUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(leader);

            var request = new InviteUserToGroupRequest
            {
                RA = invitedUser.RA,
                GroupId = group.Id
            };

            // Act
            var result = await _groupInviteService.SendGroupInviteToUser(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.GroupId);
            Assert.Equal(invitedUser.Id, result.UserId);
            Assert.False(result.Accepted);
            Assert.NotNull(result.Group);
            Assert.NotNull(result.User);
        }

        #endregion

        #region AcceptGroupInviteAsync Tests

        /// <summary>
        /// Tests that AcceptGroupInviteAsync throws GroupInvitationException when invitation doesn't exist.
        /// </summary>
        [Fact]
        public async Task AcceptGroupInviteAsync_ThrowsGroupInvitationException_WhenInviteNotFound()
        {
            // Arrange
            var user = new User
            {
                Id = "user1",
                Name = "Test User",
                Email = "test@test.com",
                RA = "12345"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(user);

            // Act & Assert
            await Assert.ThrowsAsync<GroupInvitationException>(
                () => _groupInviteService.AcceptGroupInviteAsync(999)
            );
        }

        /// <summary>
        /// Tests that AcceptGroupInviteAsync successfully accepts invitation and adds user to group.
        /// </summary>
        [Fact]
        public async Task AcceptGroupInviteAsync_AcceptsInvite_WhenInviteExists()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            leader.GroupId = group.Id;

            _dbContext.Users.AddRange(leader, invitedUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            var invite = new GroupInvite
            {
                GroupId = group.Id,
                UserId = invitedUser.Id,
                Accepted = false
            };

            _dbContext.GroupInvites.Add(invite);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(invitedUser);

            // Act
            var result = await _groupInviteService.AcceptGroupInviteAsync(group.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Accepted);
            Assert.Equal(group.Id, result.GroupId);

            // Verify user was added to group
            var updatedUser = await _dbContext.Users.FindAsync(invitedUser.Id);
            Assert.Equal(group.Id, updatedUser!.GroupId);
        }

        /// <summary>
        /// Tests that AcceptGroupInviteAsync removes other pending invites when accepting one.
        /// </summary>
        [Fact]
        public async Task AcceptGroupInviteAsync_RemovesOtherInvites_WhenAcceptingOne()
        {
            // Arrange
            var leader1 = new User
            {
                Id = "leader1",
                Name = "Leader 1",
                Email = "leader1@test.com",
                RA = "11111"
            };

            var leader2 = new User
            {
                Id = "leader2",
                Name = "Leader 2",
                Email = "leader2@test.com",
                RA = "22222"
            };

            var invitedUser = new User
            {
                Id = "user1",
                Name = "Invited User",
                Email = "invited@test.com",
                RA = "33333"
            };

            var group1 = new Group
            {
                Name = "Group 1",
                LeaderId = leader1.Id
            };

            var group2 = new Group
            {
                Name = "Group 2",
                LeaderId = leader2.Id
            };

            leader1.GroupId = group1.Id;
            leader2.GroupId = group2.Id;

            _dbContext.Users.AddRange(leader1, leader2, invitedUser);
            _dbContext.Groups.AddRange(group1, group2);
            await _dbContext.SaveChangesAsync();

            var invite1 = new GroupInvite
            {
                GroupId = group1.Id,
                UserId = invitedUser.Id,
                Accepted = false
            };

            var invite2 = new GroupInvite
            {
                GroupId = group2.Id,
                UserId = invitedUser.Id,
                Accepted = false
            };

            _dbContext.GroupInvites.AddRange(invite1, invite2);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(invitedUser);

            // Act
            await _groupInviteService.AcceptGroupInviteAsync(group1.Id);

            // Assert
            var remainingInvites = _dbContext.GroupInvites
                .Where(i => i.UserId == invitedUser.Id && i.GroupId != group1.Id)
                .ToList();

            Assert.Empty(remainingInvites);
        }

        #endregion

        #region RemoveUserFromGroupAsync Tests

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync returns null when logged user is not in the specified group.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_ReturnsNull_WhenLoggedUserNotInGroup()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var otherUser = new User
            {
                Id = "other1",
                Name = "Other User",
                Email = "other@test.com",
                RA = "22222"
            };

            var group1 = new Group
            {
                Name = "Group 1",
                LeaderId = leader.Id
            };

            var group2 = new Group
            {
                Name = "Group 2",
                LeaderId = otherUser.Id
            };

            leader.GroupId = group1.Id;
            otherUser.GroupId = group2.Id;

            _dbContext.Users.AddRange(leader, otherUser);
            _dbContext.Groups.AddRange(group1, group2);
            await _dbContext.SaveChangesAsync();

            SetupHttpContextUser(leader);

            // Act
            var result = await _groupInviteService.RemoveUserFromGroupAsync(group2.Id, otherUser.Id);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync returns false when specified user is not in the group.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_ReturnsFalse_WhenUserNotInGroup()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var otherUser = new User
            {
                Id = "other1",
                Name = "Other User",
                Email = "other@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, otherUser);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group.Id;
            _dbContext.Users.Update(leader);
            await _dbContext.SaveChangesAsync();
            
            // Reload leader with Group navigation property
            _dbContext.Entry(leader).Reference(u => u.Group).Load();

            SetupHttpContextUser(leader);

            // Act
            var result = await _groupInviteService.RemoveUserFromGroupAsync(group.Id, otherUser.Id);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync returns false when non-leader tries to remove another user.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_ReturnsFalse_WhenNonLeaderTriesToRemoveOther()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var member1 = new User
            {
                Id = "member1",
                Name = "Member 1",
                Email = "member1@test.com",
                RA = "22222"
            };

            var member2 = new User
            {
                Id = "member2",
                Name = "Member 2",
                Email = "member2@test.com",
                RA = "33333"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, member1, member2);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group.Id;
            member1.GroupId = group.Id;
            member2.GroupId = group.Id;
            _dbContext.Users.UpdateRange(leader, member1, member2);
            await _dbContext.SaveChangesAsync();
            
            // Reload member1 with Group navigation property
            _dbContext.Entry(member1).Reference(u => u.Group).Load();

            SetupHttpContextUser(member1); // Non-leader

            // Act
            var result = await _groupInviteService.RemoveUserFromGroupAsync(group.Id, member2.Id);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync successfully removes a member when leader removes them.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_RemovesMember_WhenLeaderRemovesThem()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var member = new User
            {
                Id = "member1",
                Name = "Member User",
                Email = "member@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, member);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group.Id;
            member.GroupId = group.Id;
            _dbContext.Users.UpdateRange(leader, member);
            await _dbContext.SaveChangesAsync();
            
            // Reload leader with Group navigation property
            _dbContext.Entry(leader).Reference(u => u.Group).Load();

            SetupHttpContextUser(leader);

            // Act
            var result = await _groupInviteService.RemoveUserFromGroupAsync(group.Id, member.Id);

            // Assert
            Assert.True(result);

            // Verify member was removed
            var updatedMember = await _dbContext.Users.FindAsync(member.Id);
            Assert.Null(updatedMember!.GroupId);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync allows a member to remove themselves.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_RemovesMember_WhenMemberRemovesThemselves()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var member = new User
            {
                Id = "member1",
                Name = "Member User",
                Email = "member@test.com",
                RA = "22222"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, member);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group.Id;
            member.GroupId = group.Id;
            _dbContext.Users.UpdateRange(leader, member);
            await _dbContext.SaveChangesAsync();
            
            // Reload member with Group navigation property
            _dbContext.Entry(member).Reference(u => u.Group).Load();

            SetupHttpContextUser(member); // Member removing themselves

            // Act
            var result = await _groupInviteService.RemoveUserFromGroupAsync(group.Id, member.Id);

            // Assert
            Assert.True(result);

            // Verify member was removed
            var updatedMember = await _dbContext.Users.FindAsync(member.Id);
            Assert.Null(updatedMember!.GroupId);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync deletes the group when the last leader leaves.
        /// </summary>
        [Fact(Skip = "ExecuteDeleteAsync not supported by InMemory provider")]
        public async Task RemoveUserFromGroupAsync_DeletesGroup_WhenLastMemberIsLeader()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.Add(leader);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group.Id;
            _dbContext.Users.Update(leader);
            await _dbContext.SaveChangesAsync();
            
            // Reload leader with Group navigation property
            _dbContext.Entry(leader).Reference(u => u.Group).Load();

            SetupHttpContextUser(leader);

            // Act
            var result = await _groupInviteService.RemoveUserFromGroupAsync(group.Id, leader.Id);

            // Assert
            Assert.True(result);

            // Verify group was deleted
            var deletedGroup = await _dbContext.Groups.FindAsync(group.Id);
            Assert.Null(deletedGroup);

            // Verify leader was removed from group
            var updatedLeader = await _dbContext.Users.FindAsync(leader.Id);
            Assert.Null(updatedLeader!.GroupId);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync transfers leadership when leader leaves with other members.
        /// </summary>
        [Fact(Skip = "ExecuteDeleteAsync not supported by InMemory provider")]
        public async Task RemoveUserFromGroupAsync_TransfersLeadership_WhenLeaderLeavesWithMembers()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader User",
                Email = "leader@test.com",
                RA = "11111"
            };

            var member1 = new User
            {
                Id = "member1",
                Name = "Alice Member",
                Email = "alice@test.com",
                RA = "22222"
            };

            var member2 = new User
            {
                Id = "member2",
                Name = "Bob Member",
                Email = "bob@test.com",
                RA = "33333"
            };

            var group = new Group
            {
                Name = "Test Group",
                LeaderId = leader.Id
            };

            _dbContext.Users.AddRange(leader, member1, member2);
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            
            leader.GroupId = group.Id;
            member1.GroupId = group.Id;
            member2.GroupId = group.Id;
            _dbContext.Users.UpdateRange(leader, member1, member2);
            await _dbContext.SaveChangesAsync();
            
            // Reload leader with Group navigation property
            _dbContext.Entry(leader).Reference(u => u.Group).Load();

            SetupHttpContextUser(leader);

            // Act
            var result = await _groupInviteService.RemoveUserFromGroupAsync(group.Id, leader.Id);

            // Assert
            Assert.True(result);

            // Verify leader was removed
            var updatedLeader = await _dbContext.Users.FindAsync(leader.Id);
            Assert.Null(updatedLeader!.GroupId);

            // Verify leadership was transferred (should be Alice, first alphabetically)
            var updatedGroup = await _dbContext.Groups.FindAsync(group.Id);
            Assert.NotNull(updatedGroup);
            Assert.Equal(member1.Id, updatedGroup.LeaderId);

            // Verify group still exists
            Assert.NotNull(updatedGroup);
        }

        #endregion
    }
}

