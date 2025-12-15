using ProjetoTccBackend.Database;
using ProjetoTccBackend.Enums.Competition;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories;
using ProjetoTccBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    /// <summary>
    /// Unit tests for the <see cref="GroupInCompetitionService"/> class.
    /// </summary>
    public class GroupInCompetitionServiceTests : IDisposable
    {
        private readonly TccDbContext _dbContext;
        private readonly GroupInCompetitionService _groupInCompetitionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupInCompetitionServiceTests"/> class.
        /// Sets up the in-memory database and service dependencies.
        /// </summary>
        public GroupInCompetitionServiceTests()
        {
            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");

            var groupInCompetitionRepository = new GroupInCompetitionRepository(_dbContext);

            _groupInCompetitionService = new GroupInCompetitionService(
                groupInCompetitionRepository,
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

        #region GetCurrentValidCompetitionByGroupIdAsync Tests

        /// <summary>
        /// Tests that GetCurrentValidCompetitionByGroupIdAsync returns null when no competition registration exists.
        /// </summary>
        [Fact]
        public async Task GetCurrentValidCompetitionByGroupIdAsync_ReturnsNull_WhenNoRegistrationExists()
        {
            // Arrange
            var group = new Group
            {
                Name = "Test Group",
                LeaderId = "leader1"
            };

            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.GetCurrentValidCompetitionByGroupIdAsync(group.Id);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetCurrentValidCompetitionByGroupIdAsync returns null when competition is not in valid date range.
        /// </summary>
        [Fact]
        public async Task GetCurrentValidCompetitionByGroupIdAsync_ReturnsNull_WhenCompetitionNotValid()
        {
            // Arrange
            var group = new Group
            {
                Name = "Test Group",
                LeaderId = "leader1"
            };

            var competition = new Competition
            {
                Name = "Past Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow.AddDays(-10),
                EndTime = DateTime.UtcNow.AddDays(-5),
                StartInscriptions = DateTime.UtcNow.AddDays(-20),
                EndInscriptions = DateTime.UtcNow.AddDays(-11),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Finished
            };

            _dbContext.Groups.Add(group);
            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInCompetition = new GroupInCompetition
            {
                GroupId = group.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow.AddDays(-12),
                Blocked = false
            };

            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.GetCurrentValidCompetitionByGroupIdAsync(group.Id);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetCurrentValidCompetitionByGroupIdAsync returns registration when competition is valid.
        /// </summary>
        [Fact]
        public async Task GetCurrentValidCompetitionByGroupIdAsync_ReturnsRegistration_WhenCompetitionValid()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader",
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

            var competition = new Competition
            {
                Name = "Current Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(5),
                StartInscriptions = DateTime.UtcNow.AddDays(-10),
                EndInscriptions = DateTime.UtcNow.AddDays(-2),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInCompetition = new GroupInCompetition
            {
                GroupId = group.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow.AddDays(-5),
                Blocked = false
            };

            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.GetCurrentValidCompetitionByGroupIdAsync(group.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.GroupId);
            Assert.Equal(competition.Id, result.CompetitionId);
            Assert.False(result.Blocked);
            Assert.NotNull(result.Group);
            Assert.Equal("Test Group", result.Group.Name);
            Assert.NotNull(result.Competition);
            Assert.Equal("Current Competition", result.Competition.Name);
        }

        /// <summary>
        /// Tests that GetCurrentValidCompetitionByGroupIdAsync includes group users in response.
        /// </summary>
        [Fact]
        public async Task GetCurrentValidCompetitionByGroupIdAsync_IncludesGroupUsers_WhenUsersExist()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader",
                Email = "leader@test.com",
                RA = "11111",
                Department = "CS",
                JoinYear = 2020
            };

            var member = new User
            {
                Id = "member1",
                Name = "Member",
                Email = "member@test.com",
                RA = "22222",
                Department = "CS",
                JoinYear = 2021
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

            var competition = new Competition
            {
                Name = "Current Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(5),
                StartInscriptions = DateTime.UtcNow.AddDays(-10),
                EndInscriptions = DateTime.UtcNow.AddDays(-2),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInCompetition = new GroupInCompetition
            {
                GroupId = group.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow.AddDays(-5),
                Blocked = false
            };

            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.GetCurrentValidCompetitionByGroupIdAsync(group.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Group);
            Assert.NotNull(result.Group.Users);
            Assert.Equal(2, result.Group.Users.Count);
            Assert.Contains(result.Group.Users, u => u.Name == "Leader");
            Assert.Contains(result.Group.Users, u => u.Name == "Member");
        }

        #endregion

        #region IsGroupBlockedInCompetitionAsync Tests

        /// <summary>
        /// Tests that IsGroupBlockedInCompetitionAsync returns false when registration doesn't exist.
        /// </summary>
        [Fact]
        public async Task IsGroupBlockedInCompetitionAsync_ReturnsFalse_WhenRegistrationNotFound()
        {
            // Arrange
            var groupId = 999;
            var competitionId = 888;

            // Act
            var result = await _groupInCompetitionService.IsGroupBlockedInCompetitionAsync(groupId, competitionId);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsGroupBlockedInCompetitionAsync returns false when group is not blocked.
        /// </summary>
        [Fact]
        public async Task IsGroupBlockedInCompetitionAsync_ReturnsFalse_WhenGroupNotBlocked()
        {
            // Arrange
            var group = new Group
            {
                Name = "Test Group",
                LeaderId = "leader1"
            };

            var competition = new Competition
            {
                Name = "Test Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                StartInscriptions = DateTime.UtcNow.AddDays(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Groups.Add(group);
            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInCompetition = new GroupInCompetition
            {
                GroupId = group.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow,
                Blocked = false
            };

            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.IsGroupBlockedInCompetitionAsync(group.Id, competition.Id);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsGroupBlockedInCompetitionAsync returns true when group is blocked.
        /// </summary>
        [Fact]
        public async Task IsGroupBlockedInCompetitionAsync_ReturnsTrue_WhenGroupIsBlocked()
        {
            // Arrange
            var group = new Group
            {
                Name = "Test Group",
                LeaderId = "leader1"
            };

            var competition = new Competition
            {
                Name = "Test Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                StartInscriptions = DateTime.UtcNow.AddDays(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Groups.Add(group);
            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInCompetition = new GroupInCompetition
            {
                GroupId = group.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow,
                Blocked = true
            };

            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.IsGroupBlockedInCompetitionAsync(group.Id, competition.Id);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetGroupsByCompetitionAsync Tests

        /// <summary>
        /// Tests that GetGroupsByCompetitionAsync returns empty list when no groups are registered.
        /// </summary>
        [Fact]
        public async Task GetGroupsByCompetitionAsync_ReturnsEmptyList_WhenNoGroupsRegistered()
        {
            // Arrange
            var competition = new Competition
            {
                Name = "Test Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                StartInscriptions = DateTime.UtcNow.AddDays(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.GetGroupsByCompetitionAsync(competition.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetGroupsByCompetitionAsync returns all registered groups.
        /// </summary>
        [Fact]
        public async Task GetGroupsByCompetitionAsync_ReturnsAllGroups_WhenGroupsRegistered()
        {
            // Arrange
            var group1 = new Group
            {
                Name = "Group 1",
                LeaderId = "leader1"
            };

            var group2 = new Group
            {
                Name = "Group 2",
                LeaderId = "leader2"
            };

            var competition = new Competition
            {
                Name = "Test Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                StartInscriptions = DateTime.UtcNow.AddDays(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Groups.AddRange(group1, group2);
            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInComp1 = new GroupInCompetition
            {
                GroupId = group1.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow,
                Blocked = false
            };

            var groupInComp2 = new GroupInCompetition
            {
                GroupId = group2.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow,
                Blocked = true
            };

            _dbContext.GroupsInCompetitions.AddRange(groupInComp1, groupInComp2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.GetGroupsByCompetitionAsync(competition.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Group!.Name == "Group 1" && !r.Blocked);
            Assert.Contains(result, r => r.Group!.Name == "Group 2" && r.Blocked);
        }

        /// <summary>
        /// Tests that GetGroupsByCompetitionAsync includes group users.
        /// </summary>
        [Fact]
        public async Task GetGroupsByCompetitionAsync_IncludesGroupUsers_WhenUsersExist()
        {
            // Arrange
            var leader = new User
            {
                Id = "leader1",
                Name = "Leader",
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

            var competition = new Competition
            {
                Name = "Test Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                StartInscriptions = DateTime.UtcNow.AddDays(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInCompetition = new GroupInCompetition
            {
                GroupId = group.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow,
                Blocked = false
            };

            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.GetGroupsByCompetitionAsync(competition.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotNull(result.First().Group);
            Assert.NotNull(result.First().Group!.Users);
            Assert.Single(result.First().Group.Users);
            Assert.Equal("Leader", result.First().Group.Users.First().Name);
        }

        #endregion

        #region UnblockGroupInCompetitionAsync Tests

        /// <summary>
        /// Tests that UnblockGroupInCompetitionAsync returns false when registration doesn't exist.
        /// </summary>
        [Fact]
        public async Task UnblockGroupInCompetitionAsync_ReturnsFalse_WhenRegistrationNotFound()
        {
            // Arrange
            var groupId = 999;
            var competitionId = 888;

            // Act
            var result = await _groupInCompetitionService.UnblockGroupInCompetitionAsync(groupId, competitionId);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that UnblockGroupInCompetitionAsync unblocks the group successfully.
        /// </summary>
        [Fact]
        public async Task UnblockGroupInCompetitionAsync_UnblocksGroup_WhenRegistrationExists()
        {
            // Arrange
            var group = new Group
            {
                Name = "Test Group",
                LeaderId = "leader1"
            };

            var competition = new Competition
            {
                Name = "Test Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                StartInscriptions = DateTime.UtcNow.AddDays(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Groups.Add(group);
            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInCompetition = new GroupInCompetition
            {
                GroupId = group.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow,
                Blocked = true
            };

            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.UnblockGroupInCompetitionAsync(group.Id, competition.Id);

            // Assert
            Assert.True(result);

            // Verify group was unblocked
            var updated = await _dbContext.GroupsInCompetitions
                .FirstOrDefaultAsync(g => g.GroupId == group.Id && g.CompetitionId == competition.Id);
            Assert.NotNull(updated);
            Assert.False(updated.Blocked);
        }

        /// <summary>
        /// Tests that UnblockGroupInCompetitionAsync returns true when group is already unblocked.
        /// </summary>
        [Fact]
        public async Task UnblockGroupInCompetitionAsync_ReturnsTrue_WhenGroupAlreadyUnblocked()
        {
            // Arrange
            var group = new Group
            {
                Name = "Test Group",
                LeaderId = "leader1"
            };

            var competition = new Competition
            {
                Name = "Test Competition",
                Description = "Test",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                StartInscriptions = DateTime.UtcNow.AddDays(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromMinutes(60),
                MaxMembers = 3,
                MaxExercises = 5,
                Status = CompetitionStatus.Ongoing
            };

            _dbContext.Groups.Add(group);
            _dbContext.Competitions.Add(competition);
            await _dbContext.SaveChangesAsync();

            var groupInCompetition = new GroupInCompetition
            {
                GroupId = group.Id,
                CompetitionId = competition.Id,
                CreatedOn = DateTime.UtcNow,
                Blocked = false
            };

            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupInCompetitionService.UnblockGroupInCompetitionAsync(group.Id, competition.Id);

            // Assert
            Assert.True(result);

            // Verify group remains unblocked
            var updated = await _dbContext.GroupsInCompetitions
                .FirstOrDefaultAsync(g => g.GroupId == group.Id && g.CompetitionId == competition.Id);
            Assert.NotNull(updated);
            Assert.False(updated.Blocked);
        }

        #endregion
    }
}



