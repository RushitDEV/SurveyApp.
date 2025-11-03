using SurveyApp.Models;

public class Option
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;

    public Question Question { get; set; } = null!;
}
