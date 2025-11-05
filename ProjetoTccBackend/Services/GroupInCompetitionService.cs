using Microsoft.EntityFrameworkCore;
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

        public GroupInCompetitionService(IGroupInCompetitionRepository groupInCompetitionRepository)
        {
            _groupInCompetitionRepository = groupInCompetitionRepository;
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
    }
}
