using System;

namespace SurveyApp.ViewModels
{
    public class AnswerDetailViewModel
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }

        // Display fields
        public string QuestionText { get; set; } = string.Empty;
        public string? OptionText { get; set; }
        public string? AnswerText { get; set; }

        // Additional fields for different question types
        public double? NumberValue { get; set; }
        public int? RatingValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string? FileName { get; set; }
        public DateTime AnswerDate { get; set; } = DateTime.Now;
    }
}