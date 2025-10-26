namespace SurveyApp.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType Type { get; set; }
        public int Order { get; set; }
        public Survey Survey { get; set; }
        public List<Option> Options { get; set; } = new List<Option>();
    }
}
