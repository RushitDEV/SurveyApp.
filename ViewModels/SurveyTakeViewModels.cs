using System;
using System.Collections.Generic;

namespace SurveyApp.ViewModels
{
    // 🔹 Anket doldurma sayfası için ana ViewModel
    public class SurveyTakeViewModel
    {
        public int Id { get; set; }  // ✅ Eklendi
        public int SurveyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }  // ✅ Eklendi
        public List<QuestionDetailViewModel> Questions { get; set; } = new();
    }

    // 🔹 Anket cevaplarını submit etmek için kullanılan model
    public class SurveyResponseSubmitViewModel
    {
        public int SurveyId { get; set; }
        public string? RespondentName { get; set; }
        public string? RespondentEmail { get; set; }
        public List<QuestionAnswerViewModel> Answers { get; set; } = new();
    }

    // 🔹 Her bir soruya verilen cevap
    public class QuestionAnswerViewModel
    {
        public int QuestionId { get; set; }
        public string? TextAnswer { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
        public int? Rating { get; set; }
    }

    // 🔹 AutoMapper için (eğer kullanılıyorsa)
    public class QuestionTakeViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Type { get; set; } = "SingleChoice";
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public List<OptionViewModel> Options { get; set; } = new();
    }
}