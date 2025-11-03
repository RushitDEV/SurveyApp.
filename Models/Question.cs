using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        // 🔹 Soru tipi (enum)
        public QuestionType Type { get; set; }

        // 🔹 Sıra numarası
        public int Order { get; set; }

        // ✅ Eksik olan bu alan
        public bool IsRequired { get; set; } = false;

        // 🔹 İlişkiler
        public int SurveyId { get; set; }
        public Survey Survey { get; set; } = null!;

        public ICollection<Option> Options { get; set; } = new List<Option>();
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
