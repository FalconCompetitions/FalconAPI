using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    
    /// <summary>  
    /// Representa um grupo no sistema.  
    /// </summary>  
    public class Group
    {
        /// <summary>  
        /// Identificador único do grupo.  
        /// </summary>  
        [Key]
        public int Id { get; set; }

        /// <summary>  
        /// Nome do grupo.  
        /// </summary>  
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the leader.
        /// </summary>
        [Required]
        public string LeaderId { get; set; }

        /// <summary>  
        /// Lista de usuários associados ao grupo.  
        /// </summary>  
        public ICollection<User> Users { get; } = [];

        /// <summary>  
        /// Lista de competições associadas ao grupo.  
        /// </summary>  
        public ICollection<Competition> Competitions { get; } = [];

        /// <summary>  
        /// Lista de associações entre grupos e competições.  
        /// </summary>  
        public ICollection<GroupInCompetition> GroupInCompetitions { get; } = [];

        /// <summary>  
        /// Lista de rankings de competições associados ao grupo.  
        /// </summary>  
        public ICollection<CompetitionRanking> CompetitionRankings { get; } = [];

        /// <summary>  
        /// Lista de tentativas de exercícios realizadas pelo grupo.  
        /// </summary>  
        public ICollection<GroupExerciseAttempt> GroupExerciseAttempts { get; set; } = [];

        public ICollection<Log> Logs { get; } = [];
    }
}
