namespace SurveyApp.Models
{
    public class Response
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string UserIp { get; set; }
        public DateTime CompletedDate { get; set; }

        // Navigation properties
        public Survey Survey { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }
}