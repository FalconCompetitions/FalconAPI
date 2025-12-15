using System;
using System.Threading.Tasks;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Enums.Exercise;
using ProjetoTccBackend.Enums.Judge;
using ProjetoTccBackend.Exceptions.Judge;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class GroupAttemptServiceTests
    {
        private readonly Mock<IJudgeService> _judgeServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IGroupRepository> _groupRepositoryMock;
        private readonly Mock<ICompetitionRankingService> _competitionRankingServiceMock;
        private readonly Mock<IGroupExerciseAttemptRepository> _groupExerciseAttemptRepositoryMock;
        private readonly TccDbContext _dbContext;

        public GroupAttemptServiceTests()
        {
            _judgeServiceMock = new Mock<IJudgeService>();
            _userServiceMock = new Mock<IUserService>();
            _groupRepositoryMock = new Mock<IGroupRepository>();
            _competitionRankingServiceMock = new Mock<ICompetitionRankingService>();
            _groupExerciseAttemptRepositoryMock = new Mock<IGroupExerciseAttemptRepository>();
            _dbContext = DbContextTestFactory.Create();
        }

        private GroupAttemptService CreateService()
        {
            return new GroupAttemptService(
                _dbContext,
                _judgeServiceMock.Object,
                _userServiceMock.Object,
                _groupRepositoryMock.Object,
                _competitionRankingServiceMock.Object,
                _groupExerciseAttemptRepositoryMock.Object
            );
        }

        [Fact]
        public async Task SubmitExerciseAttempt_ReturnsSubmissionAndRanking_WhenSubmissionIsAccepted()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                StartTime = DateTime.Now.AddHours(-1),
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                GroupId = 1,
                ExerciseId = 1,
                Code = "console.log('Hello');",
                LanguageType = LanguageType.Javascript,
            };

            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "user1",
            };

            var rankingResponse = new CompetitionRankingResponse
            {
                Id = 1,
                Group = new GroupResponse
                {
                    Id = 1,
                    Name = "Test Group",
                    LeaderId = "user1",
                },
                Points = 100,
                Penalty = 0,
            };

            _judgeServiceMock
                .Setup(s => s.SendGroupExerciseAttempt(request))
                .ReturnsAsync(JudgeSubmissionResponse.Accepted);

            _groupExerciseAttemptRepositoryMock
                .Setup(r => r.GetLastGroupCompetitionAttempt(1, 1))
                .Returns((GroupExerciseAttempt?)null);

            _groupRepositoryMock.Setup(r => r.GetByIdWithUsers(1)).Returns(group);

            _competitionRankingServiceMock
                .Setup(s => s.UpdateRanking(competition, group, It.IsAny<GroupExerciseAttempt>()))
                .ReturnsAsync(rankingResponse);

            var service = CreateService();

            // Act
            var result = await service.SubmitExerciseAttempt(competition, request);

            // Assert
            Assert.NotNull(result.submission);
            Assert.NotNull(result.ranking);
            Assert.Equal(1, result.submission.ExerciseId);
            Assert.Equal(1, result.submission.GroupId);
            Assert.True(result.submission.Accepted);
            Assert.Equal(JudgeSubmissionResponse.Accepted, result.submission.JudgeResponse);
            Assert.Equal(1, result.ranking.Group.Id);
            Assert.Equal(100, result.ranking.Points);
            
            // Verify new fields are populated correctly
            Assert.Equal("console.log('Hello');", result.submission.Code);
            Assert.Equal(LanguageType.Javascript, result.submission.LanguageId);
            Assert.Equal(1, result.submission.Score); // Accepted = 1 point
            Assert.Equal(100, result.submission.Points);
            Assert.Equal(0, result.submission.Penalty);
            Assert.True(result.submission.SubmittedAt <= DateTime.UtcNow);
            
            _groupExerciseAttemptRepositoryMock.Verify(
                r => r.Add(It.IsAny<GroupExerciseAttempt>()),
                Times.Once
            );
        }

        [Fact]
        public async Task SubmitExerciseAttempt_ReturnsSubmissionAndRanking_WhenSubmissionIsRejected()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                StartTime = DateTime.Now.AddHours(-1),
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                GroupId = 1,
                ExerciseId = 1,
                Code = "console.log('Hello');",
                LanguageType = LanguageType.Javascript,
            };

            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "user1",
            };

            var rankingResponse = new CompetitionRankingResponse
            {
                Id = 1,
                Group = new GroupResponse
                {
                    Id = 1,
                    Name = "Test Group",
                    LeaderId = "user1",
                },
                Points = 0,
                Penalty = 20,
            };

            _judgeServiceMock
                .Setup(s => s.SendGroupExerciseAttempt(request))
                .ReturnsAsync(JudgeSubmissionResponse.WrongAnswer);

            _groupExerciseAttemptRepositoryMock
                .Setup(r => r.GetLastGroupCompetitionAttempt(1, 1))
                .Returns((GroupExerciseAttempt?)null);

            _groupRepositoryMock.Setup(r => r.GetByIdWithUsers(1)).Returns(group);

            _competitionRankingServiceMock
                .Setup(s => s.UpdateRanking(competition, group, It.IsAny<GroupExerciseAttempt>()))
                .ReturnsAsync(rankingResponse);

            var service = CreateService();

            // Act
            var result = await service.SubmitExerciseAttempt(competition, request);

            // Assert
            Assert.NotNull(result.submission);
            Assert.NotNull(result.ranking);
            Assert.Equal(1, result.submission.ExerciseId);
            Assert.Equal(1, result.submission.GroupId);
            Assert.False(result.submission.Accepted);
            Assert.Equal(JudgeSubmissionResponse.WrongAnswer, result.submission.JudgeResponse);
            Assert.Equal(20, result.ranking.Penalty);
            
            // Verify new fields for rejected submission
            Assert.Equal("console.log('Hello');", result.submission.Code);
            Assert.Equal(LanguageType.Javascript, result.submission.LanguageId);
            Assert.Equal(0, result.submission.Score); // Rejected = 0 points
            Assert.Equal(0, result.submission.Points);
            Assert.Equal(20, result.submission.Penalty);
        }

        [Fact]
        public async Task SubmitExerciseAttempt_CalculatesDurationFromLastAttempt_WhenPreviousAttemptExists()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                StartTime = DateTime.UtcNow.AddHours(-2),
            };

            var lastAttempt = new GroupExerciseAttempt
            {
                Id = 1,
                GroupId = 1,
                ExerciseId = 1,
                CompetitionId = 1,
                Code = "previous code",
                SubmissionTime = DateTime.UtcNow.AddMinutes(-30),
                Accepted = false,
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                GroupId = 1,
                ExerciseId = 2,
                Code = "console.log('Test');",
                LanguageType = LanguageType.Javascript,
            };

            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "user1",
            };

            var rankingResponse = new CompetitionRankingResponse
            {
                Id = 1,
                Group = new GroupResponse
                {
                    Id = 1,
                    Name = "Test Group",
                    LeaderId = "user1",
                },
                Points = 100,
                Penalty = 0,
            };

            _judgeServiceMock
                .Setup(s => s.SendGroupExerciseAttempt(request))
                .ReturnsAsync(JudgeSubmissionResponse.Accepted);

            _groupExerciseAttemptRepositoryMock
                .Setup(r => r.GetLastGroupCompetitionAttempt(1, 1))
                .Returns(lastAttempt);

            _groupRepositoryMock.Setup(r => r.GetByIdWithUsers(1)).Returns(group);

            _competitionRankingServiceMock
                .Setup(s => s.UpdateRanking(competition, group, It.IsAny<GroupExerciseAttempt>()))
                .ReturnsAsync(rankingResponse);

            var service = CreateService();

            // Act
            var result = await service.SubmitExerciseAttempt(competition, request);

            // Assert
            Assert.NotNull(result.submission);
            Assert.True(result.submission.Accepted);
            Assert.Equal("console.log('Test');", result.submission.Code);
            Assert.Equal(LanguageType.Javascript, result.submission.LanguageId);
            
            // Verify attempt was added - use a more flexible time check
            // The duration should be approximately 30 minutes (from lastAttempt.SubmissionTime)
            _groupExerciseAttemptRepositoryMock.Verify(
                r =>
                    r.Add(
                        It.Is<GroupExerciseAttempt>(a =>
                            a.Time.TotalMinutes >= 28 && a.Time.TotalMinutes <= 32
                        )
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task SubmitExerciseAttempt_ThrowsJudgeException_WhenGroupNotFound()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                StartTime = DateTime.Now.AddHours(-1),
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                GroupId = 999,
                ExerciseId = 1,
                Code = "console.log('Hello');",
                LanguageType = LanguageType.Javascript,
            };

            _judgeServiceMock
                .Setup(s => s.SendGroupExerciseAttempt(request))
                .ReturnsAsync(JudgeSubmissionResponse.Accepted);

            _groupExerciseAttemptRepositoryMock
                .Setup(r => r.GetLastGroupCompetitionAttempt(999, 1))
                .Returns((GroupExerciseAttempt?)null);

            _groupRepositoryMock.Setup(r => r.GetByIdWithUsers(999)).Returns((Group?)null);

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<JudgeException>(() =>
                service.SubmitExerciseAttempt(competition, request)
            );
            Assert.Contains("Group with ID 999 not found", exception.Message);
        }

        [Fact]
        public async Task SubmitExerciseAttempt_ThrowsJudgeException_WhenJudgeServiceFails()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                StartTime = DateTime.Now.AddHours(-1),
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                GroupId = 1,
                ExerciseId = 1,
                Code = "invalid code",
                LanguageType = LanguageType.Javascript,
            };

            _judgeServiceMock
                .Setup(s => s.SendGroupExerciseAttempt(request))
                .ThrowsAsync(new Exception("Judge service error"));

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<JudgeException>(() =>
                service.SubmitExerciseAttempt(competition, request)
            );
        }

        [Fact]
        public async Task ChangeGroupExerciseAttempt_UpdatesAttempt_WhenAttemptExists()
        {
            // Arrange
            var attempt = new GroupExerciseAttempt
            {
                Id = 1,
                GroupId = 1,
                ExerciseId = 1,
                CompetitionId = 1,
                JudgeResponse = JudgeSubmissionResponse.WrongAnswer,
                Accepted = false,
                Code = "test code",
                Language = LanguageType.Javascript,
                SubmissionTime = DateTime.Now,
                Time = TimeSpan.FromMinutes(10),
            };

            _groupExerciseAttemptRepositoryMock.Setup(r => r.GetById(1)).Returns(attempt);

            var service = CreateService();

            // Act
            var result = await service.ChangeGroupExerciseAttempt(
                1,
                JudgeSubmissionResponse.Accepted
            );

            // Assert
            Assert.True(result);
            Assert.Equal(JudgeSubmissionResponse.Accepted, attempt.JudgeResponse);
            Assert.True(attempt.Accepted);
            _groupExerciseAttemptRepositoryMock.Verify(
                r => r.Update(It.IsAny<GroupExerciseAttempt>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ChangeGroupExerciseAttempt_ReturnsFalse_WhenAttemptNotFound()
        {
            // Arrange
            _groupExerciseAttemptRepositoryMock
                .Setup(r => r.GetById(999))
                .Returns((GroupExerciseAttempt)null!);

            var service = CreateService();

            // Act
            var result = await service.ChangeGroupExerciseAttempt(
                999,
                JudgeSubmissionResponse.Accepted
            );

            // Assert
            Assert.False(result);
            _groupExerciseAttemptRepositoryMock.Verify(
                r => r.Update(It.IsAny<GroupExerciseAttempt>()),
                Times.Never
            );
        }

        [Fact]
        public async Task ChangeGroupExerciseAttempt_SetsAcceptedToFalse_WhenResponseIsNotAccepted()
        {
            // Arrange
            var attempt = new GroupExerciseAttempt
            {
                Id = 1,
                GroupId = 1,
                ExerciseId = 1,
                CompetitionId = 1,
                JudgeResponse = JudgeSubmissionResponse.Accepted,
                Accepted = true,
                Code = "test code",
                Language = LanguageType.Javascript,
                SubmissionTime = DateTime.Now,
                Time = TimeSpan.FromMinutes(10),
            };

            _groupExerciseAttemptRepositoryMock.Setup(r => r.GetById(1)).Returns(attempt);

            var service = CreateService();

            // Act
            var result = await service.ChangeGroupExerciseAttempt(
                1,
                JudgeSubmissionResponse.TimeLimitExceeded
            );

            // Assert
            Assert.True(result);
            Assert.Equal(JudgeSubmissionResponse.TimeLimitExceeded, attempt.JudgeResponse);
            Assert.False(attempt.Accepted);
            _groupExerciseAttemptRepositoryMock.Verify(
                r => r.Update(It.IsAny<GroupExerciseAttempt>()),
                Times.Once
            );
        }

        [Fact]
        public async Task SubmitExerciseAttempt_StoresCorrectAttemptDetails_WhenCalled()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 5,
                Name = "Advanced Competition",
                StartTime = DateTime.Now.AddHours(-3),
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                GroupId = 10,
                ExerciseId = 20,
                Code = "def solution(): return True",
                LanguageType = LanguageType.Python,
            };

            var group = new Group
            {
                Id = 10,
                Name = "Python Masters",
                LeaderId = "user123",
            };

            var rankingResponse = new CompetitionRankingResponse
            {
                Id = 1,
                Group = new GroupResponse
                {
                    Id = 10,
                    Name = "Python Masters",
                    LeaderId = "user123",
                },
                Points = 250,
                Penalty = 5,
            };

            GroupExerciseAttempt? capturedAttempt = null;

            _judgeServiceMock
                .Setup(s => s.SendGroupExerciseAttempt(request))
                .ReturnsAsync(JudgeSubmissionResponse.Accepted);

            _groupExerciseAttemptRepositoryMock
                .Setup(r => r.GetLastGroupCompetitionAttempt(10, 5))
                .Returns((GroupExerciseAttempt?)null);

            _groupRepositoryMock.Setup(r => r.GetByIdWithUsers(10)).Returns(group);

            _groupExerciseAttemptRepositoryMock
                .Setup(r => r.Add(It.IsAny<GroupExerciseAttempt>()))
                .Callback<GroupExerciseAttempt>(a => capturedAttempt = a);

            _competitionRankingServiceMock
                .Setup(s => s.UpdateRanking(competition, group, It.IsAny<GroupExerciseAttempt>()))
                .ReturnsAsync(rankingResponse);

            var service = CreateService();

            // Act
            await service.SubmitExerciseAttempt(competition, request);

            // Assert
            Assert.NotNull(capturedAttempt);
            Assert.Equal(20, capturedAttempt.ExerciseId);
            Assert.Equal(5, capturedAttempt.CompetitionId);
            Assert.Equal(10, capturedAttempt.GroupId);
            Assert.Equal("def solution(): return True", capturedAttempt.Code);
            Assert.Equal(LanguageType.Python, capturedAttempt.Language);
            Assert.Equal(JudgeSubmissionResponse.Accepted, capturedAttempt.JudgeResponse);
            Assert.True(capturedAttempt.Accepted);
        }

        [Fact]
        public async Task SubmitExerciseAttempt_ReturnsAllRequiredFieldsForFrontend()
        {
            // Arrange
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                StartTime = DateTime.UtcNow.AddHours(-1),
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                GroupId = 5,
                ExerciseId = 10,
                Code = "print('test')",
                LanguageType = LanguageType.Python,
            };

            var group = new Group
            {
                Id = 5,
                Name = "Test Group",
                LeaderId = "user1",
            };

            var rankingResponse = new CompetitionRankingResponse
            {
                Id = 1,
                Group = new GroupResponse
                {
                    Id = 5,
                    Name = "Test Group",
                    LeaderId = "user1",
                },
                Points = 50,
                Penalty = 10,
            };

            _judgeServiceMock
                .Setup(s => s.SendGroupExerciseAttempt(request))
                .ReturnsAsync(JudgeSubmissionResponse.Accepted);

            _groupExerciseAttemptRepositoryMock
                .Setup(r => r.GetLastGroupCompetitionAttempt(5, 1))
                .Returns((GroupExerciseAttempt?)null);

            _groupRepositoryMock.Setup(r => r.GetByIdWithUsers(5)).Returns(group);

            _competitionRankingServiceMock
                .Setup(s => s.UpdateRanking(competition, group, It.IsAny<GroupExerciseAttempt>()))
                .ReturnsAsync(rankingResponse);

            var service = CreateService();

            // Act
            var result = await service.SubmitExerciseAttempt(competition, request);

            // Assert - Verify all fields required by frontend are present
            var submission = result.submission;
            
            // Core fields
            Assert.True(submission.Id >= 0);
            Assert.Equal(10, submission.ExerciseId);
            Assert.Equal(5, submission.GroupId);
            Assert.True(submission.Accepted);
            Assert.Equal(JudgeSubmissionResponse.Accepted, submission.JudgeResponse);
            
            // New fields for frontend compatibility
            Assert.Equal("print('test')", submission.Code);
            Assert.Equal(LanguageType.Python, submission.LanguageId);
            Assert.True(submission.SubmittedAt <= DateTime.UtcNow);
            Assert.True(submission.SubmittedAt >= DateTime.UtcNow.AddMinutes(-1));
            Assert.Equal(0, submission.ExecutionTime); // Not yet implemented
            Assert.Equal(0, submission.MemoryUsed); // Not yet implemented
            Assert.Equal(1, submission.Score); // Accepted = 1
            Assert.Equal(50, submission.Points); // From ranking
            Assert.Equal(10, submission.Penalty); // From ranking
        }
    }
}
