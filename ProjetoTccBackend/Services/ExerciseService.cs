using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.AttachedFile;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseInputRepository _exerciseInputRepository;
        private readonly IExerciseOutputRepository _exerciseOutputRepository;
        private readonly IJudgeService _judgeService;
        private readonly IAttachedFileService _attachedFileService;
        private readonly TccDbContext _dbContext;
        private readonly ILogger<ExerciseService> _logger;

        public ExerciseService(
            IExerciseRepository exerciseRepository,
            IExerciseInputRepository exerciseInputRepository,
            IExerciseOutputRepository exerciseOutputRepository,
            IJudgeService judgeService,
            IAttachedFileService attachedFileService,
            TccDbContext dbContext,
            ILogger<ExerciseService> logger
        )
        {
            this._exerciseRepository = exerciseRepository;
            this._exerciseInputRepository = exerciseInputRepository;
            this._exerciseOutputRepository = exerciseOutputRepository;
            this._judgeService = judgeService;
            this._attachedFileService = attachedFileService;
            this._dbContext = dbContext;
            this._logger = logger;
        }

        /// <inheritdoc />
        public async Task<Exercise> CreateExerciseAsync(
            CreateExerciseRequest request,
            IFormFile file
        )
        {
            bool isFileFormatValid = this._attachedFileService.IsSubmittedFileValid(file);

            if (isFileFormatValid is false)
            {
                throw new InvalidAttachedFileException("Formato de arquivo inválido!");
            }

            AttachedFile attachedFile = await this._attachedFileService.ProcessAndSaveFile(file);

            string? judgeUuid = "ed2e8459-c43a-42d5-9a1e-87835a769ea1"; //await this._judgeService.CreateJudgeExerciseAsync(request);

            /*
            if (judgeUuid == null)
            {
                throw new ErrorException(new { Message = "Não foi possível criar o exercício" });
            }
            */

            var inputsRequest = request.Inputs.ToList();
            var outputsRequest = request.Outputs.ToList();

            Exercise exercise = new Exercise()
            {
                JudgeUuid = judgeUuid,
                AttachedFileId = attachedFile.Id,
                Title = request.Title,
                Description = request.Description,
                EstimatedTime = TimeSpan.FromMinutes(20),
                ExerciseTypeId = request.ExerciseTypeId,
            };

            this._exerciseRepository.Add(exercise);
            await this._dbContext.SaveChangesAsync();

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
            await this._dbContext.SaveChangesAsync();

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

            await this._dbContext.SaveChangesAsync();

            Exercise createdExercise = await this
                ._exerciseRepository.Query()
                .Where(x => x.Id.Equals(exercise.Id))
                .Include(x => x.ExerciseInputs)
                .Include(x => x.ExerciseOutputs)
                .Include(x => x.AttachedFile)
                .FirstAsync();

            return createdExercise;
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

        /// <inheritdoc />
        public async Task<PagedResult<Exercise>> GetExercisesAsync(
            int page,
            int pageSize,
            string? search = null,
            int? exerciseTypeId = null
        )
        {
            var query = this._exerciseRepository.Query();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.Title.Contains(search) || e.Description.Contains(search)
                );
            }

            this._logger.LogInformation($"ExerciseTypeId: {exerciseTypeId}");

            if (exerciseTypeId != null)
            {
                query = query.Where(e => e.ExerciseTypeId == exerciseTypeId);
            }

            int totalCount = query.Count();
            var items = await query
                .AsSplitQuery()
                .OrderBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(x => x.ExerciseInputs)
                .Include(x => x.ExerciseOutputs)
                .ToListAsync();

            return new PagedResult<Exercise>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        /// <inheritdoc/>
        public async Task<Exercise> UpdateExerciseAsync(
            int id,
            IFormFile file,
            UpdateExerciseRequest request
        )
        {
            var exercise = this._exerciseRepository.GetById(id);

            if (exercise == null)
                throw new ErrorException($"Exercício com id {id} não encontrado.");

            bool isFileValid = this._attachedFileService.IsSubmittedFileValid(file);

            if (isFileValid is false)
            {
                throw new InvalidAttachedFileException("Formato de arquivo inválido!");
            }

            AttachedFile newAttachedFile =
                await this._attachedFileService.DeleteAndReplaceExistentFile(
                    (int)exercise.AttachedFileId!,
                    file
                );

            exercise.Title = request.Title;
            exercise.Description = request.Description;
            exercise.EstimatedTime = request.EstimatedTime;
            exercise.ExerciseTypeId = request.ExerciseTypeId;
            exercise.AttachedFileId = newAttachedFile.Id;

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
            await this._dbContext.SaveChangesAsync();

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
            await this._dbContext.SaveChangesAsync();

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

            var updatedExercise = await this
                ._exerciseRepository.Query()
                .Where(x => x.Id.Equals(request.Id))
                .Include(x => x.ExerciseInputs)
                .Include(x => x.ExerciseOutputs)
                .FirstAsync();

            return updatedExercise;
        }

        /// <inheritdoc/>
        public async Task DeleteExerciseAsync(int id)
        {
            var exercise = this
                ._exerciseRepository.Query()
                .Include(e => e.AttachedFile)
                .Where(e => e.Id == id)
                .FirstOrDefault();
            
            if (exercise == null)
                throw new ErrorException($"Exercício com id {id} não encontrado.");

            var outputs = this._exerciseOutputRepository.Find(x => x.ExerciseId == id).ToList();
            var inputs = this._exerciseInputRepository.Find(x => x.ExerciseId == id).ToList();

            this._exerciseOutputRepository.RemoveRange(outputs);
            this._exerciseInputRepository.RemoveRange(inputs);

            this._attachedFileService.DeleteAttachedFile(exercise.AttachedFile!);
            this._exerciseRepository.Remove(exercise);
            await this._dbContext.SaveChangesAsync();
        }
    }
}
