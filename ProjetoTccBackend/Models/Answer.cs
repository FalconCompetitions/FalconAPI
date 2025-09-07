using System.ComponentModel.DataAnnotations;

namespace ProjetoTccBackend.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        /// <summary>  
        /// Textual content of the answer.  
        /// </summary>  
        [DataType(DataType.MultilineText)]
        public required string Content { get; set; }

        public required string UserId { get; set; }

        public User User { get; set; }

        public Question Question { get; set; }

    }
}
