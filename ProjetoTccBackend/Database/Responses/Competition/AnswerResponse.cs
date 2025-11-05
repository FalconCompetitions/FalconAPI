using ProjetoTccBackend.Database.Responses.User;

namespace ProjetoTccBackend.Database.Responses.Competition
{
    public class AnswerResponse
    {
        public int Id { get; set; }

        /// <summary>  
        /// Textual content of the answer.  
        /// </summary>  
        public required string Content { get; set; }


        public GenericUserInfoResponse User { get; set; }
    }
}
