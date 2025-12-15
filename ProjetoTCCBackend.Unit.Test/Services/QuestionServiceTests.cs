using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Enums.Question;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories;
using ProjetoTccBackend.Services;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services;

/// <summary>
/// Unit tests for QuestionService.
/// </summary>
public class QuestionServiceTests : IDisposable
{
    private TccDbContext _dbContext;
    private QuestionService _questionService;

    public QuestionServiceTests()
    {
        _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");
        var questionRepository = new QuestionRepository(_dbContext);
        _questionService = new QuestionService(questionRepository);
    }

    public void Dispose()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
    }

    /// <summary>
    /// Tests that GetQuestionsAsync returns an empty list when no questions exist.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_ReturnsEmptyList_WhenNoQuestionsExist()
    {
        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(0, result.TotalPages);
    }

    /// <summary>
    /// Tests that GetQuestionsAsync returns correctly paginated questions from first page.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_ReturnsPagedQuestions_WhenQuestionsExist()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var question1 = new Question
        {
            CompetitionId = 1,
            ExerciseId = 1,
            UserId = user.Id,
            Content = "Question 1",
            QuestionType = QuestionType.Exercise
        };
        var question2 = new Question
        {
            CompetitionId = 1,
            ExerciseId = null,
            UserId = user.Id,
            Content = "Question 2",
            QuestionType = QuestionType.General
        };
        var question3 = new Question
        {
            CompetitionId = 1,
            ExerciseId = 2,
            UserId = user.Id,
            Content = "Question 3",
            QuestionType = QuestionType.Exercise
        };

        _dbContext.Questions.AddRange(question1, question2, question3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);

        var firstQuestion = result.Items.First();
        Assert.Equal("Question 1", firstQuestion.Content);
        Assert.Equal(user.Id, firstQuestion.UserId);
        Assert.Equal((int)QuestionType.Exercise, firstQuestion.QuestionType);
    }

    /// <summary>
    /// Tests that GetQuestionsAsync returns correctly paginated questions from second page.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_ReturnsSecondPage_WhenPageTwoRequested()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var question1 = new Question { CompetitionId = 1, ExerciseId = null, UserId = user.Id, Content = "Question 1", QuestionType = QuestionType.General };
        var question2 = new Question { CompetitionId = 1, ExerciseId = null, UserId = user.Id, Content = "Question 2", QuestionType = QuestionType.General };
        var question3 = new Question { CompetitionId = 1, ExerciseId = null, UserId = user.Id, Content = "Question 3", QuestionType = QuestionType.General };

        _dbContext.Questions.AddRange(question1, question2, question3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 2, pageSize: 2);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Page);

        var thirdQuestion = result.Items.First();
        Assert.Equal("Question 3", thirdQuestion.Content);
    }

    /// <summary>
    /// Tests that GetQuestionsAsync returns question with answer when answer exists.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_ReturnsQuestionWithAnswer_WhenAnswerExists()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var answer = new Answer
        {
            Content = "Test Answer",
            UserId = user.Id
        };
        _dbContext.Answers.Add(answer);
        await _dbContext.SaveChangesAsync();

        var question = new Question
        {
            CompetitionId = 1,
            ExerciseId = null,
            UserId = user.Id,
            Content = "Question with answer",
            QuestionType = QuestionType.General,
            AnswerId = answer.Id
        };
        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);

        var questionWithAnswer = result.Items.First();
        Assert.NotNull(questionWithAnswer.Answer);
        Assert.Equal(answer.Id, questionWithAnswer.Answer!.Id);
        Assert.Equal("Test Answer", questionWithAnswer.Answer.Content);
        Assert.Equal(user.Id, questionWithAnswer.Answer.UserId);
    }

    /// <summary>
    /// Tests that GetQuestionsAsync returns question without answer when answer is null.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_ReturnsQuestionWithoutAnswer_WhenAnswerIsNull()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var question = new Question
        {
            CompetitionId = 1,
            ExerciseId = null,
            UserId = user.Id,
            Content = "Question without answer",
            QuestionType = QuestionType.General,
            AnswerId = null
        };
        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);

        var questionWithoutAnswer = result.Items.First();
        Assert.Null(questionWithoutAnswer.Answer);
        Assert.Null(questionWithoutAnswer.AnswerId);
    }

    /// <summary>
    /// Tests that GetQuestionsAsync returns correct user and group information.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_ReturnsCorrectUserAndGroupInfo()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "johndoe",
            Name = "John Doe",
            Email = "john@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var question = new Question
        {
            CompetitionId = 1,
            ExerciseId = null,
            UserId = user.Id,
            Content = "Test question",
            QuestionType = QuestionType.General
        };
        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);

        var questionResponse = result.Items.First();
        Assert.NotNull(questionResponse.User);
        Assert.Equal(user.Id, questionResponse.User.Id);
        Assert.Equal("John Doe", questionResponse.User.Name);

        // Verify group information is included
        Assert.NotNull(questionResponse.User.Group);
        Assert.Equal(group.Id, questionResponse.User.Group.Id);
        Assert.Equal("Test Group", questionResponse.User.Group.Name);
    }

    /// <summary>
    /// Tests that GetQuestionsAsync orders questions by ID.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_OrdersQuestionsById()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Add questions in non-sequential order
        var question3 = new Question { CompetitionId = 1, ExerciseId = null, UserId = user.Id, Content = "Question 3", QuestionType = QuestionType.General };
        var question1 = new Question { CompetitionId = 1, ExerciseId = null, UserId = user.Id, Content = "Question 1", QuestionType = QuestionType.General };
        var question2 = new Question { CompetitionId = 1, ExerciseId = null, UserId = user.Id, Content = "Question 2", QuestionType = QuestionType.General };

        _dbContext.Questions.AddRange(question3, question1, question2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count());

        // Verify questions are ordered by ID
        var questionsList = result.Items.ToList();
        Assert.True(questionsList[0].Id < questionsList[1].Id);
        Assert.True(questionsList[1].Id < questionsList[2].Id);
    }

    /// <summary>
    /// Tests that GetQuestionsAsync calculates total pages correctly when total count divides evenly by page size.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_CalculatesTotalPagesCorrectly_WhenExactDivision()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create exactly 10 questions (2 pages of 5)
        for (int i = 1; i <= 10; i++)
        {
            var question = new Question
            {
                CompetitionId = 1,
                ExerciseId = null,
                UserId = user.Id,
                Content = $"Question {i}",
                QuestionType = QuestionType.General
            };
            _dbContext.Questions.Add(question);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(5, result.Items.Count());
    }

    /// <summary>
    /// Tests that GetQuestionsAsync calculates total pages correctly when total count does not divide evenly.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_CalculatesTotalPagesCorrectly_WhenNotExactDivision()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create 7 questions (should be 2 pages with page size 5: 5 + 2)
        for (int i = 1; i <= 7; i++)
        {
            var question = new Question
            {
                CompetitionId = 1,
                ExerciseId = null,
                UserId = user.Id,
                Content = $"Question {i}",
                QuestionType = QuestionType.General
            };
            _dbContext.Questions.Add(question);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(5, result.Items.Count());
    }

    /// <summary>
    /// Tests that GetQuestionsAsync handles General question type correctly.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_HandlesGeneralQuestionType()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var question = new Question
        {
            CompetitionId = 1,
            ExerciseId = null,
            UserId = user.Id,
            Content = "General question",
            QuestionType = QuestionType.General
        };
        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);

        var generalQuestion = result.Items.First();
        Assert.Equal((int)QuestionType.General, generalQuestion.QuestionType);
        Assert.Null(generalQuestion.ExerciseId);
    }

    /// <summary>
    /// Tests that GetQuestionsAsync handles Exercise question type correctly.
    /// </summary>
    [Fact]
    public async Task GetQuestionsAsync_HandlesExerciseQuestionType()
    {
        // Arrange
        var group = new Group { Name = "Test Group", LeaderId = "user1" };
        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        var user = new User
        {
            Id = "user1",
            RA = "123456",
            UserName = "testuser",
            Name = "Test User",
            Email = "test@test.com",
            GroupId = group.Id
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var question = new Question
        {
            CompetitionId = 1,
            ExerciseId = 5,
            UserId = user.Id,
            Content = "Exercise-related question",
            QuestionType = QuestionType.Exercise
        };
        _dbContext.Questions.Add(question);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _questionService.GetQuestionsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);

        var exerciseQuestion = result.Items.First();
        Assert.Equal((int)QuestionType.Exercise, exerciseQuestion.QuestionType);
        Assert.Equal(5, exerciseQuestion.ExerciseId);
    }
}
