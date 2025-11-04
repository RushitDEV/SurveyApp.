using System;
using SurveyApp.Models;

namespace SurveyApp.Models
{
    public class Answer
    {
        public int Id { get; set; }

        public int ResponseId { get; set; }
        public Response Response { get; set; } = null!; // ✅ null! ekledik

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!; // ✅ null! ekledik

        public int? OptionId { get; set; }
        public Option? Option { get; set; }

        public string? AnswerText { get; set; }
        public string? FilePath { get; set; }
        public int? RatingValue { get; set; }
        public DateTime AnswerDate { get; set; }
    }
}
