using System;
using System.Collections.Generic;

namespace SurveyApp.ViewModels
{
    // Liste görünümü için
    public class ResponseListViewModel
    {
        public int Id { get; set; }
        public string UserIp { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public int AnswerCount { get; set; }  // ✅ Eksik olan alan eklendi
    }

    // Detay görünümü için
    public class ResponseDetailViewModel
    {
        public int Id { get; set; }
        public string? SurveyTitle { get; set; }   // ✅ kullanılıyor AutoMapperProfile’da
        public string UserIp { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public List<AnswerSubmitViewModel> Answers { get; set; } = new();
    }

    // Anket yanıtı gönderimi için
    public class SubmitResponseViewModel
    {
        public int SurveyId { get; set; }
        public List<AnswerSubmitViewModel> Answers { get; set; } = new();
    }

    // ✅ Eksik olan AnswerSubmitViewModel (ResponseController bunu kullanıyor)
    public class AnswerSubmitViewModel
    {
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public List<int>? OptionIds { get; set; }  // Çoktan seçmeli sorular için
        public string? AnswerText { get; set; }
        public int? RatingValue { get; set; }
        public double? NumberValue { get; set; }
        public DateTime? DateValue { get; set; }
        public string? FileName { get; set; }
    }
}
