using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Enums.Competition;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class CompetitionServiceTests
    {
        private Mock<IUserService> _userServiceMock;
        private Mock<ICompetitionRepository> _competitionRepositoryMock;
        private Mock<IGroupInCompetitionRepository> _groupInCompetitionRepositoryMock;
        private Mock<ICompetitionRankingRepository> _competitionRankingRepositoryMock;
        private Mock<IQuestionRepository> _questionRepositoryMock;
        private Mock<IAnswerRepository> _answerRepositoryMock;
        private Mock<IExerciseInCompetitionRepository> _exerciseInCompetitionRepositoryMock;
        private Mock<ICompetitionStateService> _competitionStateServiceMock;
        private Mock<ILogger<CompetitionService>> _loggerMock;
        private TccDbContext? _dbContext;

        public CompetitionServiceTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _competitionRepositoryMock = new Mock<ICompetitionRepository>();
            _groupInCompetitionRepositoryMock = new Mock<IGroupInCompetitionRepository>();
            _competitionRankingRepositoryMock = new Mock<ICompetitionRankingRepository>();
            _questionRepositoryMock = new Mock<IQuestionRepository>();
            _answerRepositoryMock = new Mock<IAnswerRepository>();
            _exerciseInCompetitionRepositoryMock = new Mock<IExerciseInCompetitionRepository>();
            _competitionStateServiceMock = new Mock<ICompetitionStateService>();
            _loggerMock = new Mock<ILogger<CompetitionService>>();
        }

        private CompetitionService CreateService()
        {
            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");
            return new CompetitionService(
                _userServiceMock.Object,
                _competitionRepositoryMock.Object,
                _groupInCompetitionRepositoryMock.Object,
                _competitionRankingRepositoryMock.Object,
                _questionRepositoryMock.Object,
                _answerRepositoryMock.Object,
                _exerciseInCompetitionRepositoryMock.Object,
                _competitionStateServiceMock.Object,
                _dbContext,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateCompetition_CreatesSuccessfully_WhenNoConflict()
        {
            // Arrange
            var request = new CompetitionRequest
            {
                Name = "Test Competition",
                StartTime = DateTime.UtcNow.AddDays(1),
                Duration = TimeSpan.FromHours(3),
                ExerciseIds = new List<int> { 1, 2, 3 },
                MaxExercises = 10,
                MaxSubmissionSize = 1000,
                SubmissionPenalty = TimeSpan.FromMinutes(5),
                BlockSubmissions = TimeSpan.FromHours(2),
                StopRanking = TimeSpan.FromHours(2),
                Description = "Test description",
                MaxMembers = 3,
                StartInscriptions = DateTime.UtcNow,
                EndInscriptions = DateTime.UtcNow.AddHours(23),
            };

            var emptyCompetitions = new List<Competition>().AsQueryable();
            _competitionRepositoryMock
                .Setup(r =>
                    r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Competition, bool>>>())
                )
                .Returns(emptyCompetitions);

            var service = CreateService();

            // Act
            var result = await service.CreateCompetition(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Competition", result.Name);
            _competitionRepositoryMock.Verify(r => r.Add(It.IsAny<Competition>()), Times.Once);
        }

        [Fact]
        public async Task CreateCompetition_ThrowsException_WhenCompetitionExistsOnSameDate()
        {
            // Arrange
            var startTime = DateTime.UtcNow.AddDays(1);
            var request = new CompetitionRequest
            {
                Name = "Test Competition",
                StartTime = startTime,
                Duration = TimeSpan.FromHours(3),
                ExerciseIds = new List<int>(),
                MaxExercises = 10,
                MaxSubmissionSize = 1000,
                SubmissionPenalty = TimeSpan.FromMinutes(5),
                BlockSubmissions = TimeSpan.FromHours(2),
                StopRanking = TimeSpan.FromHours(2),
                Description = "Test",
                MaxMembers = 3,
                StartInscriptions = DateTime.UtcNow,
                EndInscriptions = DateTime.UtcNow.AddHours(23),
            };

            var existingCompetition = new Competition
            {
                Id = 1,
                Name = "Existing Competition",
                StartTime = startTime,
            };

            var competitions = new List<Competition> { existingCompetition }.AsQueryable();
            _competitionRepositoryMock
                .Setup(r =>
                    r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Competition, bool>>>())
                )
                .Returns(competitions);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ExistentCompetitionException>(() =>
                service.CreateCompetition(request)
            );
        }

        [Fact]
        public async Task GetCurrentCompetition_ReturnsCompetition_WhenOngoing()
        {
            // Arrange
            var currentTime = DateTime.UtcNow;
            var competition = new Competition
            {
                Id = 1,
                Name = "Current Competition",
                StartTime = currentTime.AddHours(-1),
                EndTime = currentTime.AddHours(2),
                StartInscriptions = currentTime.AddHours(-2),
                Status = CompetitionStatus.Ongoing,
                Exercices = new List<Exercise>(),
                Groups = new List<Group>(),
                CompetitionRankings = new List<CompetitionRanking>(),
                GroupInCompetitions = new List<GroupInCompetition>(),
            };

            var competitions = new List<Competition> { competition }
                .AsQueryable()
                .BuildMock();
            _competitionRepositoryMock.Setup(r => r.Query()).Returns(competitions);

            var service = CreateService();

            // Act
            var result = await service.GetCurrentCompetition();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(CompetitionStatus.Ongoing, result.Status);
        }

        [Fact]
        public async Task GetCurrentCompetition_ReturnsNull_WhenNoOngoingCompetition()
        {
            // Arrange
            var emptyCompetitions = new List<Competition>().AsQueryable().BuildMock();
            _competitionRepositoryMock.Setup(r => r.Query()).Returns(emptyCompetitions);

            var service = CreateService();

            // Act
            var result = await service.GetCurrentCompetition();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task OpenCompetitionInscriptionsAsync_ChangesStatus_ToOpenInscriptions()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                Status = CompetitionStatus.Pending,
            };

            var service = CreateService();

            // Act
            await service.OpenCompetitionInscriptionsAsync(competition);

            // Assert
            Assert.Equal(CompetitionStatus.OpenInscriptions, competition.Status);
            _competitionRepositoryMock.Verify(r => r.Update(competition), Times.Once);
        }

        [Fact]
        public async Task StartCompetitionAsync_ChangesStatus_ToOngoing()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                Status = CompetitionStatus.ClosedInscriptions,
            };

            var service = CreateService();

            // Act
            await service.StartCompetitionAsync(competition);

            // Assert
            Assert.Equal(CompetitionStatus.Ongoing, competition.Status);
            _competitionRepositoryMock.Verify(r => r.Update(competition), Times.Once);
        }

        [Fact]
        public async Task EndCompetitionAsync_ChangesStatus_ToFinished()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                Status = CompetitionStatus.Ongoing,
            };

            var service = CreateService();

            // Act
            await service.EndCompetitionAsync(competition);

            // Assert
            Assert.Equal(CompetitionStatus.Finished, competition.Status);
            _competitionRepositoryMock.Verify(r => r.Update(competition), Times.Once);
            _competitionStateServiceMock.Verify(s => s.SignalNoActiveCompetitions(), Times.Once);
        }

        [Fact]
        public async Task GetOpenCompetitionsAsync_ReturnsOnlyNonFinished()
        {
            // Arrange
            var competitions = new List<Competition>
            {
                new Competition
                {
                    Id = 1,
                    Name = "Ongoing",
                    Status = CompetitionStatus.Ongoing,
                },
                new Competition
                {
                    Id = 2,
                    Name = "Pending",
                    Status = CompetitionStatus.Pending,
                },
                new Competition
                {
                    Id = 3,
                    Name = "Finished",
                    Status = CompetitionStatus.Finished,
                },
            }
                .AsQueryable()
                .BuildMock();

            _competitionRepositoryMock.Setup(r => r.Query()).Returns(competitions);

            var service = CreateService();

            // Act
            var result = await service.GetOpenCompetitionsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, c => c.Status == CompetitionStatus.Finished);
        }

        [Fact]
        public async Task InscribeGroupInCompetition_ThrowsException_WhenUserNotLeader()
        {
            // Arrange
            var loggedUser = new User
            {
                Id = "user1",
                GroupId = 1,
                Group = new Group
                {
                    Id = 1,
                    LeaderId = "user2", // Different user
                    Users = new List<User>(),
                },
            };

            var request = new InscribeGroupToCompetitionRequest { CompetitionId = 1, GroupId = 1 };

            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<UserIsNotLeaderException>(() =>
                service.InscribeGroupInCompetition(request)
            );
        }

        [Fact]
        public async Task InscribeGroupInCompetition_ThrowsException_WhenCompetitionNotFound()
        {
            // Arrange
            var loggedUser = new User
            {
                Id = "user1",
                GroupId = 1,
                Group = new Group
                {
                    Id = 1,
                    LeaderId = "user1",
                    Users = new List<User> { new User { Id = "user1" } },
                },
            };

            var request = new InscribeGroupToCompetitionRequest
            {
                CompetitionId = 999,
                GroupId = 1,
            };

            var emptyCompetitions = new List<Competition>().AsQueryable().BuildMock();
            _competitionRepositoryMock.Setup(r => r.Query()).Returns(emptyCompetitions);
            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotExistentCompetitionException>(() =>
                service.InscribeGroupInCompetition(request)
            );
        }

        [Fact]
        public async Task InscribeGroupInCompetition_ThrowsException_WhenAlreadyInscribed()
        {
            // Arrange
            var loggedUser = new User
            {
                Id = "user1",
                GroupId = 1,
                Group = new Group
                {
                    Id = 1,
                    LeaderId = "user1",
                    Users = new List<User> { new User { Id = "user1" } },
                },
            };

            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                StartInscriptions = DateTime.UtcNow.AddHours(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(1),
                MaxMembers = 3,
                GroupInCompetitions = new List<GroupInCompetition>
                {
                    new GroupInCompetition { CompetitionId = 1, GroupId = 1 },
                },
            };

            var competitions = new List<Competition> { competition }
                .AsQueryable()
                .BuildMock();
            _competitionRepositoryMock.Setup(r => r.Query()).Returns(competitions);
            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);

            var request = new InscribeGroupToCompetitionRequest { CompetitionId = 1, GroupId = 1 };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<AlreadyInCompetitionException>(() =>
                service.InscribeGroupInCompetition(request)
            );
        }

        [Fact]
        public async Task InscribeGroupInCompetition_ThrowsException_WhenTooManyMembers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = "user1" },
                new User { Id = "user2" },
                new User { Id = "user3" },
                new User { Id = "user4" },
            };

            var loggedUser = new User
            {
                Id = "user1",
                GroupId = 1,
                Group = new Group
                {
                    Id = 1,
                    LeaderId = "user1",
                    Users = users,
                },
            };

            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                StartInscriptions = DateTime.UtcNow.AddHours(-1),
                EndInscriptions = DateTime.UtcNow.AddHours(1),
                MaxMembers = 3, // Only 3 members allowed
                GroupInCompetitions = new List<GroupInCompetition>(),
            };

            var competitions = new List<Competition> { competition }
                .AsQueryable()
                .BuildMock();
            _competitionRepositoryMock.Setup(r => r.Query()).Returns(competitions);
            _userServiceMock.Setup(s => s.GetHttpContextLoggedUser()).Returns(loggedUser);

            var request = new InscribeGroupToCompetitionRequest { CompetitionId = 1, GroupId = 1 };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<MaxMembersExceededException>(() =>
                service.InscribeGroupInCompetition(request)
            );
        }

        [Fact]
        public async Task BlockGroupInCompetition_ReturnsTrue_WhenGroupFound()
        {
            // Arrange
            var groupInCompetition = new GroupInCompetition
            {
                GroupId = 1,
                CompetitionId = 1,
                Blocked = false,
            };

            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");
            _dbContext.GroupsInCompetitions.Add(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            var service = new CompetitionService(
                _userServiceMock.Object,
                _competitionRepositoryMock.Object,
                _groupInCompetitionRepositoryMock.Object,
                _competitionRankingRepositoryMock.Object,
                _questionRepositoryMock.Object,
                _answerRepositoryMock.Object,
                _exerciseInCompetitionRepositoryMock.Object,
                _competitionStateServiceMock.Object,
                _dbContext,
                _loggerMock.Object
            );

            var request = new BlockGroupSubmissionRequest { GroupId = 1, CompetitionId = 1 };

            // Act
            var result = await service.BlockGroupInCompetition(request);

            // Assert
            Assert.True(result);
            Assert.True(groupInCompetition.Blocked);
        }

        [Fact]
        public async Task BlockGroupInCompetition_ReturnsFalse_WhenGroupNotFound()
        {
            // Arrange
            var service = CreateService();
            var request = new BlockGroupSubmissionRequest { GroupId = 999, CompetitionId = 999 };

            // Act
            var result = await service.BlockGroupInCompetition(request);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task StopCompetitionAsync_ReturnsTrue_AndStopsCompetition()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                Status = CompetitionStatus.Ongoing,
                EndTime = DateTime.UtcNow.AddHours(1),
            };

            var competitions = new List<Competition> { competition }
                .AsQueryable()
                .BuildMock();
            _competitionRepositoryMock.Setup(r => r.Query()).Returns(competitions);

            var service = CreateService();

            // Act
            var result = await service.StopCompetitionAsync(1);

            // Assert
            Assert.True(result);
            Assert.Equal(CompetitionStatus.Finished, competition.Status);
            Assert.True(competition.EndTime <= DateTime.UtcNow);
            _competitionRepositoryMock.Verify(r => r.Update(competition), Times.Once);
        }

        [Fact]
        public async Task StopCompetitionAsync_ReturnsFalse_WhenCompetitionNotFound()
        {
            // Arrange
            var emptyCompetitions = new List<Competition>().AsQueryable().BuildMock();
            _competitionRepositoryMock.Setup(r => r.Query()).Returns(emptyCompetitions);

            var service = CreateService();

            // Act
            var result = await service.StopCompetitionAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task StopCompetitionAsync_ReturnsFalse_WhenAlreadyFinished()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                Status = CompetitionStatus.Finished,
            };

            var competitions = new List<Competition> { competition }
                .AsQueryable()
                .BuildMock();
            _competitionRepositoryMock.Setup(r => r.Query()).Returns(competitions);

            var service = CreateService();

            // Act
            var result = await service.StopCompetitionAsync(1);

            // Assert
            Assert.False(result);
        }
    }
}
