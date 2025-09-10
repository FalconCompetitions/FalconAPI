using System.Linq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Database.Responses.Global;

namespace ProjetoTccBackend.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseInputRepository _exerciseInputRepository;
        private readonly IExerciseOutputRepository _exerciseOutputRepository;
        private readonly IJudgeService _judgeService;
        private readonly TccDbContext _dbContext;
        private readonly ILogger<ExerciseService> _logger;

        public ExerciseService(
            IExerciseRepository exerciseRepository,
            IExerciseInputRepository exerciseInputRepository,
            IExerciseOutputRepository exerciseOutputRepository,
            IJudgeService judgeService,
            TccDbContext dbContext,
            ILogger<ExerciseService> logger
        )
        {
            this._exerciseRepository = exerciseRepository;
            this._exerciseInputRepository = exerciseInputRepository;
            this._exerciseOutputRepository = exerciseOutputRepository;
            this._judgeService = judgeService;
            this._dbContext = dbContext;
            this._logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Exercise> CreateExerciseAsync(CreateExerciseRequest request)
        {
            string? judgeUuid = await this._judgeService.CreateJudgeExerciseAsync(request);

            if (judgeUuid == null)
            {
                throw new ErrorException(new { Message = "Não foi possível criar o exercício" });
            }

            var inputsRequest = request.Inputs.ToList();
            var outputsRequest = request.Outputs.ToList();

            Exercise exercise = new Exercise()
            {
                JudgeUuid = judgeUuid,
                Title = request.Title,
                Description = request.Description,
                EstimatedTime = request.EstimatedTime,
                ExerciseTypeId = request.ExerciseTypeId,
            };

            this._exerciseRepository.Add(exercise);

            List<ExerciseInput> inputs = new List<ExerciseInput>();
            List<ExerciseOutput> outputs = new List<ExerciseOutput>();

            foreach (var input in inputsRequest)
            {
                inputs.Add(
                    new ExerciseInput()
                    {
                        ExerciseId = exercise.Id,
                        JudgeUuid = judgeUuid,
                        Input = input.Input,
                    }
                );
            }

            this._exerciseInputRepository.AddRange(inputs);

            for (int i = 0; i < outputsRequest.Count; i++)
            {
                outputs.Add(
                    new ExerciseOutput()
                    {
                        ExerciseId = exercise.Id,
                        ExerciseInputId = inputs[i].Id,
                        JudgeUuid = judgeUuid,
                        Output = outputsRequest[i].Output,
                    }
                );
            }

            this._exerciseOutputRepository.AddRange(outputs);

            this._dbContext.SaveChanges();

            return exercise;
        }

        /// <inheritdoc/>
        public async Task<Exercise?> GetExerciseByIdAsync(int id)
        {
            Exercise? exercise = this._exerciseRepository.GetById(id);
            return exercise;
        }

        /// <inheritdoc/>
        public async Task<List<Exercise>> GetExercisesAsync()
        {
            List<Exercise> exercises = this._exerciseRepository.GetAll().ToList();

            return exercises;
        }

        public async Task<PagedResult<Exercise>> GetExercisesAsync(int page, int pageSize, string? search = null)
        {
            var query = this._exerciseRepository.GetAll().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search));
            }
            int totalCount = query.Count();
            var items = query.OrderBy(e => e.Id).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return await Task.FromResult(new PagedResult<Exercise>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        /// <inheritdoc/>
        public async Task UpdateExerciseAsync(int id, UpdateExerciseRequest request)
        {
            var exercise = this._exerciseRepository.GetById(id);

            if (exercise == null)
                throw new ErrorException($"Exercício com id {id} não encontrado.");

            exercise.Title = request.Title;
            exercise.Description = request.Description;
            exercise.EstimatedTime = request.EstimatedTime;
            exercise.ExerciseTypeId = request.ExerciseTypeId;

            // Current inputs and outputs in the database

            var currentInputs = this
                ._exerciseInputRepository.Find(x => x.ExerciseId.Equals(id))
                .ToList();
            var currentOutputs = this
                ._exerciseOutputRepository.Find(x => x.ExerciseId.Equals(id))
                .ToList();

            currentInputs.Sort((x, y) => x.Id.CompareTo(y.Id));
            currentOutputs.Sort((x, y) => x.Id.CompareTo(y.Id));

            List<CreateExerciseInputRequest> inputsToCreate = request
                .Inputs.Where(x => x.Id is null)
                .Select(
                    (x, idx) =>
                        new CreateExerciseInputRequest()
                        {
                            ExerciseId = id,
                            Input = x.Input,
                            OrderId = idx,
                        }
                )
                .ToList();

            List<CreateExerciseOutputRequest> outputsToCreate = request
                .Outputs.Where(x => x.Id is null)
                .Select(
                    (x, idx) =>
                        new CreateExerciseOutputRequest()
                        {
                            ExerciseId = id,
                            Output = x.Output,
                            OrderId = idx,
                        }
                )
                .ToList();

            inputsToCreate.Sort((x, y) => x.OrderId.CompareTo(y.OrderId));
            outputsToCreate.Sort((x, y) => x.OrderId.CompareTo(y.OrderId));

            // Remove inputs and outputs that are going to be created

            request.Inputs = request.Inputs.Where(x => x.Id is not null).ToList();
            request.Outputs = request.Outputs.Where(x => x.Id is not null).ToList();

            List<ExerciseInput> createdInputs = new List<ExerciseInput>();
            List<ExerciseOutput> createdOutputs = new List<ExerciseOutput>();

            foreach (var input in inputsToCreate)
            {
                createdInputs.Add(
                    new ExerciseInput()
                    {
                        ExerciseId = (int)input.ExerciseId!,
                        Input = input.Input,
                        JudgeUuid = exercise.JudgeUuid!,
                    }
                );
            }

            this._exerciseInputRepository.AddRange(createdInputs);

            for (int i = 0; i < outputsToCreate.Count; i++)
            {
                createdOutputs.Add(
                    new ExerciseOutput()
                    {
                        ExerciseId = exercise.Id,
                        JudgeUuid = exercise.JudgeUuid,
                        ExerciseInputId = createdInputs[i].Id,
                        Output = outputsToCreate[i].Output,
                    }
                );
            }

            this._exerciseOutputRepository.AddRange(createdOutputs);

            List<ExerciseInput> inputsToDelete = new List<ExerciseInput>();
            List<ExerciseOutput> outputsToDelete = new List<ExerciseOutput>();

            currentInputs.Sort((x, y) => x.Id.CompareTo(y.Id));
            currentOutputs.Sort((x, y) => x.Id.CompareTo(y.Id));

            foreach (var input in currentInputs)
            {
                if (request.Inputs.Any(x => x.Id.Equals(input.Id)))
                {
                    continue;
                }

                inputsToDelete.Add(input);
            }

            foreach (var output in currentOutputs)
            {
                if (request.Outputs.Any(x => x.Id.Equals(output.Id)))
                {
                    continue;
                }
                outputsToDelete.Add(output);
            }

            this._exerciseOutputRepository.RemoveRange(outputsToDelete);
            this._exerciseInputRepository.RemoveRange(inputsToDelete);

            this._exerciseRepository.Update(exercise);

            await this._dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteExerciseAsync(int id)
        {
            var exercise = this._exerciseRepository.GetById(id);
            if (exercise == null)
                throw new ErrorException($"Exercício com id {id} não encontrado.");

            var outputs = this._exerciseOutputRepository.Find(x => x.ExerciseId == id).ToList();
            var inputs = this._exerciseInputRepository.Find(x => x.ExerciseId == id).ToList();

            this._exerciseOutputRepository.RemoveRange(outputs);
            this._exerciseInputRepository.RemoveRange(inputs);

            this._exerciseRepository.Remove(exercise);
            await this._dbContext.SaveChangesAsync();
        }
    }
}
