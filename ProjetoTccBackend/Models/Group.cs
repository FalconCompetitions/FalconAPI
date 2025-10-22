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
        public virtual ICollection<User> Users { get; set; }

        /// <summary>  
        /// Lista de competições associadas ao grupo.  
        /// </summary>  
        public virtual ICollection<Competition> Competitions { get; set; }

        /// <summary>  
        /// Lista de associações entre grupos e competições.  
        /// </summary>  
        public virtual ICollection<GroupInCompetition> GroupInCompetitions { get; } = [];

        /// <summary>  
        /// Lista de rankings de competições associados ao grupo.  
        /// </summary>  
        public virtual ICollection<CompetitionRanking> CompetitionRankings { get; } = [];

        /// <summary>  
        /// Lista de tentativas de exercícios realizadas pelo grupo.  
        /// </summary>  
        public virtual ICollection<GroupExerciseAttempt> GroupExerciseAttempts { get; set; } = [];

        public virtual ICollection<Log> Logs { get; } = [];

        /// <summary>
        /// Gets the collection of group invites associated with the current user.
        /// </summary>
        public virtual ICollection<GroupInvite> GroupInvites { get; set; }
    }
}
