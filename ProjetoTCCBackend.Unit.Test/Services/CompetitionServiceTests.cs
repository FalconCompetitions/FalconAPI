using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Enums.Competition;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services;

/// <summary>
/// Unit tests for CompetitionService.
/// </summary>
public class CompetitionServiceTests : IDisposable
{
    private TccDbContext _dbContext;
    private CompetitionService _competitionService;
    private Mock<IUserService> _userServiceMock;
    private Mock<ICompetitionStateService> _competitionStateServiceMock;
    private Mock<ICompetitionCacheService> _competitionCacheServiceMock;
    private Mock<ILogger<CompetitionService>> _loggerMock;

    public CompetitionServiceTests()
    {
        _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");
        _userServiceMock = new Mock<IUserService>();
        _competitionStateServiceMock = new Mock<ICompetitionStateService>();
        _competitionCacheServiceMock = new Mock<ICompetitionCacheService>();
        _loggerMock = new Mock<ILogger<CompetitionService>>();
        
        var competitionRepository = new CompetitionRepository(_dbContext);
        var groupInCompetitionRepository = new GroupInCompetitionRepository(_dbContext);
        var competitionRankingRepository = new CompetitionRankingRepository(_dbContext);
        var questionRepository = new QuestionRepository(_dbContext);
        var answerRepository = new AnswerRepository(_dbContext);
        var exerciseInCompetitionRepository = new ExerciseInCompetitionRepository(_dbContext);
        
        _competitionService = new CompetitionService(
            _userServiceMock.Object,
            competitionRepository,
            groupInCompetitionRepository,
            competitionRankingRepository,
            questionRepository,
            answerRepository,
            exerciseInCompetitionRepository,
            _competitionStateServiceMock.Object,
            _competitionCacheServiceMock.Object,
            _dbContext,
            _loggerMock.Object
        );
    }

    public void Dispose()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
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
            ExerciseIds = new List<int>(),
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

        // Act
        var result = await _competitionService.CreateCompetition(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Competition", result.Name);
        Assert.Equal(3, result.MaxMembers);
        var savedCompetition = await _dbContext.Competitions.FindAsync(result.Id);
        Assert.NotNull(savedCompetition);
    }

    [Fact]
    public async Task CreateCompetition_ThrowsException_WhenCompetitionExistsOnSameDate()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(1);
        
        var existingCompetition = new Competition
        {
            Name = "Existing Competition",
            Description = "Test Description",
            StartTime = startTime,
            Duration = TimeSpan.FromHours(2),
            MaxMembers = 3,
            Status = CompetitionStatus.Pending
        };
        _dbContext.Competitions.Add(existingCompetition);
        await _dbContext.SaveChangesAsync();
        
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

        // Act & Assert
        await Assert.ThrowsAsync<ExistentCompetitionException>(() =>
            _competitionService.CreateCompetition(request)
        );
    }

    [Fact]
    public async Task GetCurrentCompetition_ReturnsCompetition_WhenOngoing()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        var competition = new Competition
        {
            Name = "Current Competition",
            Description = "Test Description",
            StartTime = currentTime.AddHours(-1),
            EndTime = currentTime.AddHours(2),
            StartInscriptions = currentTime.AddHours(-2),
            Duration = TimeSpan.FromHours(3),
            MaxMembers = 3,
            Status = CompetitionStatus.Ongoing
        };
        _dbContext.Competitions.Add(competition);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _competitionService.GetCurrentCompetition();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(competition.Id, result.Id);
        Assert.Equal(CompetitionStatus.Ongoing, result.Status);
    }

    [Fact]
    public async Task GetCurrentCompetition_ReturnsNull_WhenNoOngoingCompetition()
    {
        // Arrange
        // No competitions in database

        // Act
        var result = await _competitionService.GetCurrentCompetition();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task OpenCompetitionInscriptionsAsync_ChangesStatus_ToOpenInscriptions()
    {
        // Arrange
        var competition = new Competition
        {
            Name = "Test Competition",
            Description = "Test Description",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = TimeSpan.FromHours(3),
            MaxMembers = 3,
            Status = CompetitionStatus.Pending
        };
        _dbContext.Competitions.Add(competition);
        await _dbContext.SaveChangesAsync();

        // Act
        await _competitionService.OpenCompetitionInscriptionsAsync(competition);

        // Assert
        Assert.Equal(CompetitionStatus.OpenInscriptions, competition.Status);
        var updated = await _dbContext.Competitions.FindAsync(competition.Id);
        Assert.Equal(CompetitionStatus.OpenInscriptions, updated!.Status);
    }

    [Fact]
    public async Task StartCompetitionAsync_ChangesStatus_ToOngoing()
    {
        // Arrange
        var competition = new Competition
        {
            Name = "Test Competition",
            Description = "Test Description",
            StartTime = DateTime.UtcNow,
            Duration = TimeSpan.FromHours(3),
            MaxMembers = 3,
            Status = CompetitionStatus.ClosedInscriptions
        };
        _dbContext.Competitions.Add(competition);
        await _dbContext.SaveChangesAsync();

        // Act
        await _competitionService.StartCompetitionAsync(competition);

        // Assert
        Assert.Equal(CompetitionStatus.Ongoing, competition.Status);
        var updated = await _dbContext.Competitions.FindAsync(competition.Id);
        Assert.Equal(CompetitionStatus.Ongoing, updated!.Status);
    }

    [Fact]
    public async Task EndCompetitionAsync_ChangesStatus_ToFinished()
    {
        // Arrange
        var competition = new Competition
        {
            Name = "Test Competition",
            Description = "Test Description",
            StartTime = DateTime.UtcNow.AddHours(-2),
            Duration = TimeSpan.FromHours(2),
            MaxMembers = 3,
            Status = CompetitionStatus.Ongoing
        };
        _dbContext.Competitions.Add(competition);
        await _dbContext.SaveChangesAsync();

        // Act
        await _competitionService.EndCompetitionAsync(competition);

        // Assert
        Assert.Equal(CompetitionStatus.Finished, competition.Status);
        var updated = await _dbContext.Competitions.FindAsync(competition.Id);
        Assert.Equal(CompetitionStatus.Finished, updated!.Status);
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
                Name = "Ongoing",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromHours(3),
                MaxMembers = 3,
                Status = CompetitionStatus.Ongoing
            },
            new Competition
            {
                Name = "Pending",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                Duration = TimeSpan.FromHours(3),
                MaxMembers = 3,
                Status = CompetitionStatus.Pending
            },
            new Competition
            {
                Name = "Finished",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(-2),
                Duration = TimeSpan.FromHours(3),
                MaxMembers = 3,
                Status = CompetitionStatus.Finished
            }
        };
        _dbContext.Competitions.AddRange(competitions);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _competitionService.GetOpenCompetitionsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, c => c.Status == CompetitionStatus.Finished);
    }

    [Fact]
    public async Task GetFinishedCompetitionsAsync_ReturnsFinishedCompetitions()
    {
        // Arrange
        var competitions = new List<Competition>
        {
            new Competition
            {
                Name = "Finished 1",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(-10),
                Duration = TimeSpan.FromHours(5),
                MaxMembers = 3,
                Status = CompetitionStatus.Finished
            },
            new Competition
            {
                Name = "Finished 2",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(-3),
                Duration = TimeSpan.FromHours(3),
                MaxMembers = 4,
                Status = CompetitionStatus.Finished
            },
            new Competition
            {
                Name = "Ongoing",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddHours(-1),
                Duration = TimeSpan.FromHours(3),
                MaxMembers = 3,
                Status = CompetitionStatus.Ongoing
            }
        };
        _dbContext.Competitions.AddRange(competitions);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _competitionService.GetFinishedCompetitionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(CompetitionStatus.Finished, c.Status));
    }

    [Fact]
    public async Task BlockGroupInCompetition_ReturnsTrue_WhenGroupFound()
    {
        // Arrange
        var groupInCompetition = new GroupInCompetition
        {
            GroupId = 1,
            CompetitionId = 1,
            Blocked = false
        };
        _dbContext.GroupsInCompetitions.Add(groupInCompetition);
        await _dbContext.SaveChangesAsync();

        var request = new BlockGroupSubmissionRequest { GroupId = 1, CompetitionId = 1 };

        // Act
        var result = await _competitionService.BlockGroupInCompetition(request);

        // Assert
        Assert.True(result);
        Assert.True(groupInCompetition.Blocked);
    }

    [Fact]
    public async Task BlockGroupInCompetition_ReturnsFalse_WhenGroupNotFound()
    {
        // Arrange
        var request = new BlockGroupSubmissionRequest { GroupId = 999, CompetitionId = 999 };

        // Act
        var result = await _competitionService.BlockGroupInCompetition(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task StopCompetitionAsync_ReturnsTrue_AndStopsCompetition()
    {
        // Arrange
        var competition = new Competition
        {
            Name = "Test Competition",
            Description = "Test Description",
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow.AddHours(1),
            Duration = TimeSpan.FromHours(2),
            MaxMembers = 3,
            Status = CompetitionStatus.Ongoing
        };
        _dbContext.Competitions.Add(competition);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _competitionService.StopCompetitionAsync(competition.Id);

        // Assert
        Assert.True(result);
        var updated = await _dbContext.Competitions.FindAsync(competition.Id);
        Assert.Equal(CompetitionStatus.Finished, updated!.Status);
        Assert.True(updated.EndTime <= DateTime.UtcNow);
    }

    [Fact]
    public async Task StopCompetitionAsync_ReturnsFalse_WhenCompetitionNotFound()
    {
        // Arrange
        // No competition in database

        // Act
        var result = await _competitionService.StopCompetitionAsync(999);

        // Assert
        Assert.False(result);
    }
}


