namespace SurveyApp.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public string? AnswerText { get; set; }
        public string UserIp { get; set; }
        public DateTime AnswerDate { get; set; }
    }
}
