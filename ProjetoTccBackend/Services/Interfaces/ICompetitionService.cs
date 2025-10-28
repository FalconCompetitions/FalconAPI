using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Enums.Competition;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing competitions.
    /// </summary>
    public interface ICompetitionService
    {
        /// <summary>
        /// Creates a new competition based on the provided request data.
        /// </summary>
        /// <param name="request">The competition creation request containing start and end times.</param>
        /// <exception cref="ExistentCompetitionException">An exception is thrown if a competition is already in progress.</exception>
        /// <returns>The created <see cref="Competition"/> object.</returns>
        Task<Competition> CreateCompetition(CompetitionRequest request);

        /// <summary>
        /// Retrieves the existing competition.
        /// </summary>
        /// <returns>The existing <see cref="Competition"/> object.</returns>
        Task<Competition> GetExistentCompetition();

        /// <summary>
        /// Retrieves a collection of competitions that are marked as template models.
        /// </summary>
        /// <remarks>This method queries the competition repository to retrieve competitions with a status
        /// of <see cref="CompetitionStatus.ModelTemplate"/>. The returned competitions are mapped to <see
        /// cref="CompetitionResponse"/> objects, which include key details such as competition metadata and associated
        /// exercise IDs.</remarks>
        /// <returns>A collection of <see cref="CompetitionResponse"/> objects representing competitions that are designated as
        /// templates. If no template competitions exist, an empty collection is returned.</returns>
        Task<ICollection<CompetitionResponse>> GetCreatedTemplateCompetitions();

        /// <summary>
        /// Retrieves the current competition based on the system's current date and time.
        ///
        /// This method queries the competition repository to find a competition where the current date and time falls within its start and end time.
        ///
        /// Returns the current competition if found, otherwise returns null.
        /// </summary>
        Task<Competition?> GetCurrentCompetition();

        /// <summary>
        /// Creates a new group question for a specific user and group.
        /// </summary>
        /// <param name="loggedUser">The user who is creating the question.</param>
        /// <param name="request">The request containing details of the question to be created.</param>
        /// <returns>The created <see cref="Question"/> object.</returns>
        Task<Question> CreateGroupQuestion(User loggedUser, CreateGroupQuestionRequest request);

        /// <summary>
        /// Answers a group question for a specific user.
        /// </summary>
        /// <param name="loggedUser">The user who is answering the question.</param>
        /// <param name="request">The request containing details of the answer to be submitted.</param>
        /// <returns>The <see cref="Answer"/> object representing the answer to the question.</returns>
        /// <remarks>Only Admin and Teacher users have access to this method.</remarks>
        Task<Answer> AnswerGroupQuestion(User loggedUser, AnswerGroupQuestionRequest request);

        /// <summary>
        /// Opens inscriptions for the specified competition, allowing participants to register.
        /// </summary>
        /// <remarks>This method enables the registration process for the given competition. Ensure that
        /// the competition is in a valid state to accept inscriptions before calling this method.</remarks>
        /// <param name="competition">The competition for which inscriptions will be opened. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task OpenCompetitionInscriptionsAsync(Competition competition);

        /// <summary>
        /// Closes the inscriptions period for the specified competition.
        /// </summary>
        /// <remarks>This method finalizes the inscription process for the given competition, preventing
        /// further modifications or additions to the inscriptions. Ensure that all necessary inscriptions are completed
        /// before calling this method.</remarks>
        /// <param name="competition">The competition for which inscriptions should be closed. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CloseCompetitionInscriptionsAsync(Competition competition);

        /// <summary>
        /// Starts the specified competition asynchronously.
        /// </summary>
        /// <remarks>This method initiates the competition and performs any necessary setup.  Ensure that
        /// the <paramref name="competition"/> object is fully initialized before calling this method.</remarks>
        /// <param name="competition">The <see cref="Competition"/> instance representing the competition to start. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task StartCompetitionAsync(Competition competition);

        /// <summary>
        /// Ends the specified competition and performs any necessary finalization tasks.
        /// </summary>
        /// <remarks>This method finalizes the competition, ensuring that all related processes are
        /// completed. Callers should ensure that the <paramref name="competition"/> is in a valid state before invoking
        /// this method.</remarks>
        /// <param name="competition">The competition to be ended. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task EndCompetitionAsync(Competition competition);

        /// <summary>
        /// Asynchronously retrieves a collection of competitions that are currently open for participation.
        /// </summary>
        /// <remarks>This method is intended to provide a list of competitions that are currently
        /// accepting participants.  The caller can use the returned collection to display or process the available
        /// competitions.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see
        /// cref="Competition"/> objects representing the open competitions. If no competitions are open,  the
        /// collection will be empty.</returns>
        Task<ICollection<Competition>> GetOpenCompetitionsAsync();

        /// <summary>
        /// Updates an existing competition with the specified data.
        /// </summary>
        /// <remarks>This method updates the competition's details and synchronizes its associated
        /// exercises based on the provided request. Any exercises not included in the request will be removed from the
        /// competition, and new exercises will be added.</remarks>
        /// <param name="id">The unique identifier of the competition to update.</param>
        /// <param name="request">An <see cref="UpdateCompetitionRequest"/> object containing the updated competition details,  such as name,
        /// start time, duration, exercise IDs, and other configuration settings.</param>
        /// <returns>The updated <see cref="Competition"/> object if the competition exists; otherwise, <see langword="null"/>.</returns>
        Task<Competition?> UpdateCompetitionAsync(int id, UpdateCompetitionRequest request);

        /// <summary>
        /// Retrieves a collection of competitions that are currently open for subscriptions.
        /// </summary>
        /// <remarks>This method queries the underlying competition repository to find competitions with a
        /// status  of <see cref="CompetitionStatus.OpenInscriptions"/>. The returned collection is read-only and
        /// reflects the current state of the repository at the time of the query.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see
        /// cref="Competition"/> objects where the subscription status is open. If no competitions  are open for
        /// subscriptions, the collection will be empty.</returns>
        Task<ICollection<Competition>> GetOpenSubscriptionCompetitionsAsync();


        /// <summary>
        /// Registers the logged-in user's group in a specified competition.
        /// </summary>
        /// <remarks>The logged-in user must be the leader of their group to perform this operation. The
        /// competition must exist, be open for inscriptions, and the group must not already be registered.
        /// Additionally, the group's member count must not exceed the competition's maximum allowed members.</remarks>
        /// <param name="request">The request containing the competition ID to register the group in.</param>
        /// <returns>A <see cref="GroupInCompetition"/> object representing the group's registration in the competition.</returns>
        /// <exception cref="UserIsNotLeaderException">Thrown if the logged-in user is not the leader of their group.</exception>
        /// <exception cref="NotExistentCompetitionException">Thrown if the specified competition does not exist.</exception>
        /// <exception cref="AlreadyInCompetitionException">Thrown if the group is already registered in the specified competition.</exception>
        /// <exception cref="NotValidCompetitionException">Thrown if the competition is not currently open for inscriptions.</exception>
        /// <exception cref="MaxMembersExceededException">Thrown if the group's member count exceeds the competition's maximum allowed members.</exception>
        Task<GroupInCompetition> InscribeGroupInCompetition(
            InscribeGroupToCompetitionRequest request
        );
    }
}
