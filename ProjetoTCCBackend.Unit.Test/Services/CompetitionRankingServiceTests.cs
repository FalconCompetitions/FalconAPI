using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class CompetitionRankingServiceTests
    {
        private Mock<ICompetitionRankingRepository> _competitionRankingRepositoryMock;
        private Mock<IGroupExerciseAttemptRepository> _groupExerciseAttemptRepositoryMock;
        private TccDbContext? _dbContext;

        public CompetitionRankingServiceTests()
        {
            _competitionRankingRepositoryMock = new Mock<ICompetitionRankingRepository>();
            _groupExerciseAttemptRepositoryMock = new Mock<IGroupExerciseAttemptRepository>();
        }

        private CompetitionRankingService CreateService()
        {
            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");
            return new CompetitionRankingService(
                _competitionRankingRepositoryMock.Object,
                _groupExerciseAttemptRepositoryMock.Object,
                _dbContext
            );
        }

        [Fact]
        public async Task UpdateRanking_CreatesNewRanking_WhenGroupNotInRanking()
        {
            var competition = new Competition
            {
                Id = 1,
                Name = "Test Competition",
                SubmissionPenalty = TimeSpan.FromMinutes(10),
            };

            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "user1",
                Users = new List<User>
                {
                    new User
                    {
                        Id = "user1",
                        Email = "user1@test.com",
                        Name = "User 1",
                        JoinYear = 2024,
                    },
                },
            };

            var exerciseAttempt = new GroupExerciseAttempt
            {
                Code = "",
                Id = 1,
                GroupId = 1,
                CompetitionId = 1,
                ExerciseId = 1,
                Accepted = true,
            };

            var attempts = new List<GroupExerciseAttempt> { exerciseAttempt }.AsQueryable();
            var emptyRankings = new List<CompetitionRanking>().AsQueryable();

            _groupExerciseAttemptRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<GroupExerciseAttempt, bool>
                        >>()
                    )
                )
                .Returns(attempts);
            _competitionRankingRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<CompetitionRanking, bool>
                        >>()
                    )
                )
                .Returns(emptyRankings);

            var service = CreateService();
            var result = await service.UpdateRanking(competition, group, exerciseAttempt);

            Assert.NotNull(result);
            Assert.Equal(1, result.Points);
            Assert.Equal(10, result.Penalty);
            Assert.Equal(1, result.RankOrder);
            _competitionRankingRepositoryMock.Verify(
                r => r.Add(It.IsAny<CompetitionRanking>()),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateRanking_UpdatesExistingRanking()
        {
            var competition = new Competition
            {
                Id = 1,
                SubmissionPenalty = TimeSpan.FromMinutes(5),
            };
            var group = new Group
            {
                Id = 1,
                Name = "Test",
                LeaderId = "u1",
                Users = new List<User>
                {
                    new User
                    {
                        Id = "u1",
                        Email = "t@t.com",
                        Name = "T",
                        JoinYear = 2024,
                    },
                },
            };
            var exerciseAttempt = new GroupExerciseAttempt
            {
                Code = "",
                Id = 2,
                GroupId = 1,
                CompetitionId = 1,
                ExerciseId = 1,
                Accepted = true,
            };
            var attempts = new List<GroupExerciseAttempt>
            {
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 1,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = false,
                },
                exerciseAttempt,
            }.AsQueryable();
            var existingRanking = new CompetitionRanking
            {
                Id = 1,
                CompetitionId = 1,
                GroupId = 1,
                Points = 0,
                Penalty = 5,
                RankOrder = 1,
            };
            var rankings = new List<CompetitionRanking> { existingRanking }.AsQueryable();

            _groupExerciseAttemptRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<GroupExerciseAttempt, bool>
                        >>()
                    )
                )
                .Returns(attempts);
            _competitionRankingRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<CompetitionRanking, bool>
                        >>()
                    )
                )
                .Returns(rankings);

            var service = CreateService();
            var result = await service.UpdateRanking(competition, group, exerciseAttempt);

            Assert.NotNull(result);
            Assert.Equal(1, result.Points);
            Assert.Equal(10, result.Penalty);
            _competitionRankingRepositoryMock.Verify(
                r => r.Update(It.IsAny<CompetitionRanking>()),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateRanking_CalculatesMultipleAttempts()
        {
            var competition = new Competition
            {
                Id = 1,
                SubmissionPenalty = TimeSpan.FromMinutes(20),
            };
            var group = new Group
            {
                Id = 1,
                Name = "Test",
                LeaderId = "u1",
                Users = new List<User>
                {
                    new User
                    {
                        Id = "u1",
                        Email = "t@t.com",
                        Name = "T",
                        JoinYear = 2024,
                    },
                },
            };
            var exerciseAttempt = new GroupExerciseAttempt
            {
                Code = "",
                Id = 5,
                GroupId = 1,
                CompetitionId = 1,
                ExerciseId = 2,
                Accepted = false,
            };
            var attempts = new List<GroupExerciseAttempt>
            {
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 1,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = false,
                },
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 2,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = false,
                },
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 3,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = true,
                },
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 4,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 2,
                    Accepted = false,
                },
                exerciseAttempt,
            }.AsQueryable();

            _groupExerciseAttemptRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<GroupExerciseAttempt, bool>
                        >>()
                    )
                )
                .Returns(attempts);
            _competitionRankingRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<CompetitionRanking, bool>
                        >>()
                    )
                )
                .Returns(new List<CompetitionRanking>().AsQueryable());

            var service = CreateService();
            var result = await service.UpdateRanking(competition, group, exerciseAttempt);

            Assert.NotNull(result);
            Assert.Equal(1, result.Points);
            Assert.Equal(100, result.Penalty);
        }

        [Fact]
        public async Task UpdateRanking_ReturnsZeroPoints_WhenNoAccepted()
        {
            var competition = new Competition
            {
                Id = 1,
                SubmissionPenalty = TimeSpan.FromMinutes(10),
            };
            var group = new Group
            {
                Id = 1,
                Name = "Test",
                LeaderId = "u1",
                Users = new List<User>
                {
                    new User
                    {
                        Id = "u1",
                        Email = "t@t.com",
                        Name = "T",
                        JoinYear = 2024,
                    },
                },
            };
            var exerciseAttempt = new GroupExerciseAttempt
            {
                Code = "",
                Id = 3,
                GroupId = 1,
                CompetitionId = 1,
                ExerciseId = 1,
                Accepted = false,
            };
            var attempts = new List<GroupExerciseAttempt>
            {
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 1,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = false,
                },
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 2,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = false,
                },
                exerciseAttempt,
            }.AsQueryable();

            _groupExerciseAttemptRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<GroupExerciseAttempt, bool>
                        >>()
                    )
                )
                .Returns(attempts);
            _competitionRankingRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<CompetitionRanking, bool>
                        >>()
                    )
                )
                .Returns(new List<CompetitionRanking>().AsQueryable());

            var service = CreateService();
            var result = await service.UpdateRanking(competition, group, exerciseAttempt);

            Assert.NotNull(result);
            Assert.Equal(0, result.Points);
            Assert.Equal(30, result.Penalty);
        }

        [Fact]
        public async Task UpdateRanking_IncludesGroupInfo()
        {
            var competition = new Competition
            {
                Id = 1,
                SubmissionPenalty = TimeSpan.FromMinutes(10),
            };
            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                LeaderId = "u1",
                Users = new List<User>
                {
                    new User
                    {
                        Id = "u1",
                        Email = "a@a.com",
                        Name = "A",
                        JoinYear = 2024,
                    },
                    new User
                    {
                        Id = "u2",
                        Email = "b@b.com",
                        Name = "B",
                        JoinYear = 2023,
                    },
                },
            };
            var exerciseAttempt = new GroupExerciseAttempt
            {
                Code = "",
                Id = 1,
                GroupId = 1,
                CompetitionId = 1,
                ExerciseId = 1,
                Accepted = true,
            };

            _groupExerciseAttemptRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<GroupExerciseAttempt, bool>
                        >>()
                    )
                )
                .Returns(new List<GroupExerciseAttempt> { exerciseAttempt }.AsQueryable());
            _competitionRankingRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<CompetitionRanking, bool>
                        >>()
                    )
                )
                .Returns(new List<CompetitionRanking>().AsQueryable());

            var service = CreateService();
            var result = await service.UpdateRanking(competition, group, exerciseAttempt);

            Assert.NotNull(result.Group);
            Assert.Equal(1, result.Group.Id);
            Assert.Equal("Test Group", result.Group.Name);
            Assert.Equal(2, result.Group.Users.Count);
        }

        [Fact]
        public async Task UpdateRanking_IncludesExerciseAttempts()
        {
            var competition = new Competition
            {
                Id = 1,
                SubmissionPenalty = TimeSpan.FromMinutes(10),
            };
            var group = new Group
            {
                Id = 1,
                Name = "Test",
                LeaderId = "u1",
                Users = new List<User>
                {
                    new User
                    {
                        Id = "u1",
                        Email = "t@t.com",
                        Name = "T",
                        JoinYear = 2024,
                    },
                },
            };
            var exerciseAttempt = new GroupExerciseAttempt
            {
                Code = "",
                Id = 4,
                GroupId = 1,
                CompetitionId = 1,
                ExerciseId = 2,
                Accepted = false,
            };
            var attempts = new List<GroupExerciseAttempt>
            {
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 1,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = true,
                },
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 2,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = false,
                },
                new GroupExerciseAttempt
                {
                    Code = "",
                    Id = 3,
                    GroupId = 1,
                    CompetitionId = 1,
                    ExerciseId = 1,
                    Accepted = false,
                },
                exerciseAttempt,
            }.AsQueryable();

            _groupExerciseAttemptRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<GroupExerciseAttempt, bool>
                        >>()
                    )
                )
                .Returns(attempts);
            _competitionRankingRepositoryMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<
                            Func<CompetitionRanking, bool>
                        >>()
                    )
                )
                .Returns(new List<CompetitionRanking>().AsQueryable());

            var service = CreateService();
            var result = await service.UpdateRanking(competition, group, exerciseAttempt);

            Assert.NotNull(result.ExerciseAttempts);
            Assert.Equal(2, result.ExerciseAttempts.Count);
            var ex1 = result.ExerciseAttempts.FirstOrDefault(e => e.ExerciseId == 1);
            Assert.NotNull(ex1);
            Assert.Equal(3, ex1.Attempts);
        }
    }
}
