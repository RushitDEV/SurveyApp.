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
        public int AnswerCount { get; set; }
    }

    // Detay görünümü için
    public class ResponseDetailViewModel
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string? SurveyTitle { get; set; }
        public int? UserId { get; set; }
        public string UserIp { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedDate { get; set; }

        // ✅ AnswerDetailViewModel olmalı, AnswerSubmitViewModel değil!
        public List<AnswerDetailViewModel> Answers { get; set; } = new();
    }

    // Anket yanıtı gönderimi için
    public class SubmitResponseViewModel
    {
        public int SurveyId { get; set; }
        public List<AnswerSubmitViewModel> Answers { get; set; } = new();
    }

    // Anket cevabı gönderme (submit)
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