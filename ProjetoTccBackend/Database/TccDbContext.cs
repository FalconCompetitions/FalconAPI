using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Database
{
    public class TccDbContext : IdentityDbContext<User>
    {
        private readonly IConfiguration? _configuration;

        public DbSet<AttachedFile> AttachedFiles { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupInvite> GroupInvites { get; set; }
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<CompetitionRanking> CompetitionRankings { get; set; }
        public DbSet<ExerciseType> ExerciseTypes { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseInput> ExerciseInputs { get; set; }
        public DbSet<ExerciseOutput> ExerciseOutputs { get; set; }
        public DbSet<GroupInCompetition> GroupsInCompetitions { get; set; }
        public DbSet<ExerciseInCompetition> ExercisesInCompetitions { get; set; }
        public DbSet<GroupExerciseAttempt> GroupExerciseAttempts { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<ExerciseSubmissionQueueItem> ExerciseSubmissionQueueItems { get; set; }

        // Construtor para testes
        public TccDbContext(DbContextOptions<TccDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_configuration != null)
            {
                optionsBuilder.UseMySql(
                    _configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(
                        _configuration.GetConnectionString("DefaultConnection")
                    )
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Group - Users
            builder
                .Entity<Group>()
                .HasMany<User>(u => u.Users)
                .WithOne(g => g.Group)
                .HasForeignKey(u => u.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(required: false);

            // Competitions - Groups
            builder
                .Entity<Competition>()
                .HasMany<Group>(e => e.Groups)
                .WithMany(u => u.Competitions)
                .UsingEntity<GroupInCompetition>(e =>
                    e.Property(p => p.CreatedOn).HasDefaultValueSql("CURRENT_TIMESTAMP")
                );

            // CompetitionRanking - Competition
            builder
                .Entity<Competition>()
                .HasMany(c => c.CompetitionRankings)
                .WithOne(cr => cr.Competition)
                .HasForeignKey(cr => cr.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            // Group - CompetitionRanking
            builder
                .Entity<Group>()
                .HasMany<CompetitionRanking>(c => c.CompetitionRankings)
                .WithOne(g => g.Group)
                .HasForeignKey(c => c.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            // Competition - Exercises
            builder
                .Entity<Competition>()
                .HasMany<Exercise>(c => c.Exercices)
                .WithMany(e => e.Competitions)
                .UsingEntity<ExerciseInCompetition>();

            builder
                .Entity<ExerciseOutput>()
                .HasOne(e => e.ExerciseInput)
                .WithOne(e => e.ExerciseOutput)
                .HasForeignKey<ExerciseOutput>(e => e.ExerciseInputId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            // Exercise - ExericseInput[]
            builder
                .Entity<Exercise>()
                .HasMany(e => e.ExerciseInputs)
                .WithOne(e => e.Exercise)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            // Exercise - ExerciseOutputs[]
            builder
                .Entity<Exercise>()
                .HasMany(e => e.ExerciseOutputs)
                .WithOne(e => e.Exercise)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            builder
                .Entity<GroupExerciseAttempt>()
                .HasOne(g => g.Group)
                .WithMany(g => g.GroupExerciseAttempts)
                .HasForeignKey(g => g.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            builder
                .Entity<GroupExerciseAttempt>()
                .HasOne(g => g.Exercise)
                .WithMany(e => e.GroupExerciseAttempts)
                .HasForeignKey(g => g.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            builder
                .Entity<GroupExerciseAttempt>()
                .HasOne(g => g.Competition)
                .WithMany(g => g.GroupExerciseAttempts)
                .HasForeignKey(g => g.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            builder
                .Entity<Exercise>()
                .HasMany(e => e.Questions)
                .WithOne(q => q.Exercise)
                .HasForeignKey(q => q.ExerciseId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(required: false);

            builder
                .Entity<User>()
                .HasMany(u => u.Questions)
                .WithOne(q => q.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            builder
                .Entity<User>()
                .HasMany(u => u.Answers)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            builder
                .Entity<Competition>()
                .HasMany(c => c.Questions)
                .WithOne(q => q.Competition)
                .HasForeignKey(q => q.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            // Exercise - ExerciseTypeId
            builder
                .Entity<Exercise>()
                .HasOne(e => e.ExerciseType)
                .WithMany(e => e.Exercises)
                .HasForeignKey(e => e.ExerciseTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            // Log - User
            builder
                .Entity<Log>()
                .HasOne<User>(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(required: false);

            // Log[] - Competition
            builder
                .Entity<Log>()
                .HasOne(l => l.Competition)
                .WithMany(c => c.Logs)
                .HasForeignKey(l => l.CompetitionId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(required: false);

            // Log[] - Group
            builder
                .Entity<Log>()
                .HasOne(l => l.Group)
                .WithMany(g => g.Logs)
                .HasForeignKey(l => l.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(required: false);

            builder
                .Entity<Question>()
                .HasOne(q => q.Answer)
                .WithOne(a => a.Question)
                .HasForeignKey<Question>(q => q.AnswerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            var groupExerciseAttemptRequestConverter = new ValueConverter<
                GroupExerciseAttemptRequest,
                string
            >(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v =>
                    JsonSerializer.Deserialize<GroupExerciseAttemptRequest>(
                        v,
                        (JsonSerializerOptions)null
                    )
            );

            builder
                .Entity<ExerciseSubmissionQueueItem>()
                .Property(e => e.Request)
                .HasConversion(groupExerciseAttemptRequestConverter);

            builder
                .Entity<GroupInvite>()
                .HasOne(g => g.User)
                .WithMany(u => u.GroupInvites)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            builder
                .Entity<GroupInvite>()
                .HasOne(g => g.Group)
                .WithMany(g => g.GroupInvites)
                .HasForeignKey(g => g.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(required: true);

            builder
                .Entity<Exercise>()
                .HasOne(e => e.AttachedFile)
                .WithMany(a => a.Exercises)
                .HasForeignKey(e => e.AttachedFileId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(required: false);
        }
    }
}
