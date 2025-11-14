using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    /// <summary>
    /// Service responsible for managing group-in-competition operations.
    /// </summary>
    public class GroupInCompetitionService : IGroupInCompetitionService
    {
        private readonly IGroupInCompetitionRepository _groupInCompetitionRepository;
        private readonly TccDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupInCompetitionService"/> class.
        /// </summary>
        /// <param name="groupInCompetitionRepository">The group in competition repository.</param>
        /// <param name="dbContext">The database context.</param>
        public GroupInCompetitionService(
            IGroupInCompetitionRepository groupInCompetitionRepository,
            TccDbContext dbContext)
        {
            _groupInCompetitionRepository = groupInCompetitionRepository;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves the current valid competition registration for a group.
        /// A registration is considered valid if the current date is within the competition's date range.
        /// </summary>
        /// <param name="groupId">The ID of the group to retrieve the registration for.</param>
        /// <returns>The valid GroupInCompetitionResponse if found, otherwise null.</returns>
        public async Task<GroupInCompetitionResponse?> GetCurrentValidCompetitionByGroupIdAsync(int groupId)
        {
            var groupInCompetition = await _groupInCompetitionRepository
                .GetCurrentValidCompetitionByGroupIdAsync(groupId);

            if (groupInCompetition == null)
            {
                return null;
            }

            var response = new GroupInCompetitionResponse
            {
                GroupId = groupInCompetition.GroupId,
                CompetitionId = groupInCompetition.CompetitionId,
                CreatedOn = groupInCompetition.CreatedOn,
                Blocked = groupInCompetition.Blocked,
                Group = groupInCompetition.Group != null ? new GroupResponse
                {
                    Id = groupInCompetition.Group.Id,
                    Name = groupInCompetition.Group.Name,
                    LeaderId = groupInCompetition.Group.LeaderId,
                    Users = groupInCompetition.Group.Users?.Select(u => new GenericUserInfoResponse
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Ra = u.RA,
                        Email = u.Email,
                        Department = u.Department,
                        JoinYear = u.JoinYear,
                        CreatedAt = u.CreatedAt,
                        LastLoggedAt = u.LastLoggedAt
                    }).ToList() ?? new List<GenericUserInfoResponse>()
                } : null,
                Competition = groupInCompetition.Competition != null ? new CompetitionResponse
                {
                    Id = groupInCompetition.Competition.Id,
                    Name = groupInCompetition.Competition.Name,
                    Description = groupInCompetition.Competition.Description,
                    MaxMembers = groupInCompetition.Competition.MaxMembers,
                    StartTime = groupInCompetition.Competition.StartTime,
                    EndTime = groupInCompetition.Competition.EndTime,
                    StartInscriptions = groupInCompetition.Competition.StartInscriptions,
                    EndInscriptions = groupInCompetition.Competition.EndInscriptions,
                    Duration = groupInCompetition.Competition.Duration,
                    StopRanking = groupInCompetition.Competition.StopRanking,
                    BlockSubmissions = groupInCompetition.Competition.BlockSubmissions,
                    SubmissionPenalty = groupInCompetition.Competition.SubmissionPenalty,
                    MaxExercises = groupInCompetition.Competition.MaxExercises,
                    Status = groupInCompetition.Competition.Status,
                    MaxSubmissionSize = groupInCompetition.Competition.MaxSubmissionSize,
                    ExerciseIds = groupInCompetition.Competition.ExercisesInCompetition?
                        .Select(eic => eic.ExerciseId).ToList() ?? new List<int>(),
                    CompetitionRankings = groupInCompetition.Competition.CompetitionRankings?
                        .Select(cr => new CompetitionRankingResponse
                        {
                            Id = cr.Id,
                            Points = cr.Points,
                            Penalty = cr.Penalty,
                            RankOrder = cr.RankOrder,
                            Group = new GroupResponse
                            {
                                Id = cr.GroupId,
                                Name = cr.Group?.Name ?? "",
                                LeaderId = cr.Group?.LeaderId ?? ""
                            },
                            ExerciseAttempts = new List<GroupExerciseAttemptResponse>()
                        }).ToList()
                } : null
            };

            return response;
        }

        /// <summary>
        /// Checks if a group is blocked from participating in a specific competition.
        /// </summary>
        /// <param name="groupId">The ID of the group to check.</param>
        /// <param name="competitionId">The ID of the competition to check.</param>
        /// <returns>True if the group is blocked, false otherwise.</returns>
        public async Task<bool> IsGroupBlockedInCompetitionAsync(int groupId, int competitionId)
        {
            var groupInCompetition = await _groupInCompetitionRepository.Query()
                .FirstOrDefaultAsync(gic => gic.GroupId == groupId && gic.CompetitionId == competitionId);

            if (groupInCompetition == null)
            {
                return false;
            }

            return groupInCompetition.Blocked;
        }

        /// <summary>
        /// Retrieves all groups registered in a specific competition.
        /// </summary>
        /// <param name="competitionId">The ID of the competition.</param>
        /// <returns>A collection of GroupInCompetitionResponse objects.</returns>
        public async Task<ICollection<GroupInCompetitionResponse>> GetGroupsByCompetitionAsync(int competitionId)
        {
            var groupsInCompetition = await _groupInCompetitionRepository.Query()
                .Include(gic => gic.Group)
                    .ThenInclude(g => g!.Users)
                .Include(gic => gic.Competition)
                .Where(gic => gic.CompetitionId == competitionId)
                .ToListAsync();

            var response = groupsInCompetition.Select(gic => new GroupInCompetitionResponse
            {
                GroupId = gic.GroupId,
                CompetitionId = gic.CompetitionId,
                CreatedOn = gic.CreatedOn,
                Blocked = gic.Blocked,
                Group = gic.Group != null ? new GroupResponse
                {
                    Id = gic.Group.Id,
                    Name = gic.Group.Name,
                    LeaderId = gic.Group.LeaderId,
                    Users = gic.Group.Users?.Select(u => new GenericUserInfoResponse
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Ra = u.RA,
                        Email = u.Email,
                        Department = u.Department,
                        JoinYear = u.JoinYear,
                        CreatedAt = u.CreatedAt,
                        LastLoggedAt = u.LastLoggedAt
                    }).ToList() ?? new List<GenericUserInfoResponse>()
                } : null,
                Competition = gic.Competition != null ? new CompetitionResponse
                {
                    Id = gic.Competition.Id,
                    Name = gic.Competition.Name,
                    Description = gic.Competition.Description,
                    MaxMembers = gic.Competition.MaxMembers,
                    StartTime = gic.Competition.StartTime,
                    EndTime = gic.Competition.EndTime,
                    StartInscriptions = gic.Competition.StartInscriptions,
                    EndInscriptions = gic.Competition.EndInscriptions,
                    Duration = gic.Competition.Duration,
                    StopRanking = gic.Competition.StopRanking,
                    BlockSubmissions = gic.Competition.BlockSubmissions,
                    SubmissionPenalty = gic.Competition.SubmissionPenalty,
                    MaxExercises = gic.Competition.MaxExercises,
                    Status = gic.Competition.Status,
                    MaxSubmissionSize = gic.Competition.MaxSubmissionSize,
                    ExerciseIds = new List<int>(),
                    CompetitionRankings = new List<CompetitionRankingResponse>()
                } : null
            }).ToList();

            return response;
        }

        /// <summary>
        /// Unblocks a group in a specific competition, allowing them to submit exercises again.
        /// </summary>
        /// <param name="groupId">The ID of the group to unblock.</param>
        /// <param name="competitionId">The ID of the competition.</param>
        /// <returns>True if the group was successfully unblocked, false otherwise.</returns>
        public async Task<bool> UnblockGroupInCompetitionAsync(int groupId, int competitionId)
        {
            var groupInCompetition = await _groupInCompetitionRepository.Query()
                .FirstOrDefaultAsync(gic => gic.GroupId == groupId && gic.CompetitionId == competitionId);

            if (groupInCompetition == null)
            {
                return false;
            }

            groupInCompetition.Blocked = false;
            _groupInCompetitionRepository.Update(groupInCompetition);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
