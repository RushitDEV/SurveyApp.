using SurveyApp.Models;

public class Answer
{
    public int Id { get; set; }

    public int ResponseId { get; set; }
    public int QuestionId { get; set; }
    public int? OptionId { get; set; }

    public string? AnswerText { get; set; }
    public string? FilePath { get; set; }
    public int? RatingValue { get; set; }
    public DateTime AnswerDate { get; set; }

    // Navigation properties
    public Response Response { get; set; }
    public Question Question { get; set; }
    public Option? Option { get; set; }
}
