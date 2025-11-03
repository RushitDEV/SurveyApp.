using System;

namespace SurveyApp.ViewModels
{
    public class AnswerDetailViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? OptionText { get; set; }
        public string? AnswerText { get; set; }
        public double? NumberValue { get; set; }
        public int? RatingValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string? FileName { get; set; }
    }
}
