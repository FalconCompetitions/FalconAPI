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
    /// <summary>
    /// Service responsible for managing exercise operations.
    /// </summary>
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseInputRepository _exerciseInputRepository;
        private readonly IExerciseOutputRepository _exerciseOutputRepository;
        private readonly IJudgeService _judgeService;
        private readonly IAttachedFileService _attachedFileService;
        private readonly TccDbContext _dbContext;
        private readonly ILogger<ExerciseService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseService"/> class.
        /// </summary>
        /// <param name="exerciseRepository">The repository for exercise data access.</param>
        /// <param name="exerciseInputRepository">The repository for exercise input data access.</param>
        /// <param name="exerciseOutputRepository">The repository for exercise output data access.</param>
        /// <param name="judgeService">The service for judge operations.</param>
        /// <param name="attachedFileService">The service for attached file operations.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">Logger for registering information and errors.</param>
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
                throw new InvalidAttachedFileException("Invalid file format!");
            }

            AttachedFile attachedFile = await this._attachedFileService.ProcessAndSaveFile(file);

            string? judgeUuid = "ed2e8459-c43a-42d5-9a1e-87835a769ea1"; //await this._judgeService.CreateJudgeExerciseAsync(request);

            /*
            if (judgeUuid == null)
            {
                throw new FormException(
                    new Dictionary<string, string> { { "form", "Não foi possível criar o exercício no sistema de avaliação" } }
                );
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
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var items = await query
                .AsSplitQuery()
                .OrderBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(x => x.ExerciseInputs)
                .Include(x => x.ExerciseOutputs)
                .Include(x => x.AttachedFile)
                .ToListAsync();

            return new PagedResult<Exercise>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Page = page,
                PageSize = pageSize,
            };
        }

        /// <inheritdoc/>
        public async Task<Exercise> UpdateExerciseAsync(
            int id,
            IFormFile? file,
            UpdateExerciseRequest request
        )
        {
            var exercise = this._exerciseRepository.GetById(id);

            if (exercise == null)
                throw new FormException(
                    new Dictionary<string, string> { { "form", $"Exercício com id {id} não encontrado" } }
                );

            // Only update the file if a new one is provided
            if (file != null && file.Length > 0)
            {
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

                exercise.AttachedFileId = newAttachedFile.Id;
            }

            exercise.Title = request.Title;
            exercise.Description = request.Description;
            exercise.EstimatedTime = TimeSpan.FromMinutes(20);
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

            // Update existing inputs and outputs
            foreach (var inputRequest in request.Inputs.Where(x => x.Id is not null))
            {
                var existingInput = currentInputs.FirstOrDefault(x => x.Id == inputRequest.Id);
                if (existingInput != null)
                {
                    existingInput.Input = inputRequest.Input;
                    this._exerciseInputRepository.Update(existingInput);
                }
            }

            foreach (var outputRequest in request.Outputs.Where(x => x.Id is not null))
            {
                var existingOutput = currentOutputs.FirstOrDefault(x => x.Id == outputRequest.Id);
                if (existingOutput != null)
                {
                    existingOutput.Output = outputRequest.Output;
                    this._exerciseOutputRepository.Update(existingOutput);
                }
            }

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
                .Where(x => x.Id.Equals(id))
                .Include(x => x.AttachedFile)
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
                throw new FormException(
                    new Dictionary<string, string> { { "form", $"Exercício com id {id} não encontrado" } }
                );

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
