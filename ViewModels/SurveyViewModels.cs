using System;
using System.Collections.Generic;

namespace SurveyApp.ViewModels
{
    // 🔹 Anket listesi görünümü
    public class SurveyListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int QuestionCount { get; set; }
        public int ResponseCount { get; set; }
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.Now;
    }

    // 🔹 Anket detay görünümü (Details.cshtml bunu kullanıyor)
    public class SurveyDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.Now;
        public List<QuestionDetailViewModel> Questions { get; set; } = new();
        public int ResponseCount { get; set; }
    }

    // 🔹 Anket oluşturma ve düzenleme
    public class SurveyCreateEditViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<QuestionCreateEditViewModel> Questions { get; set; } = new();
    }

    // 🔹 Anket istatistikleri (AdminController bunu istiyor)
    public class SurveyStatisticsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalResponses { get; set; }
        public List<QuestionStatisticsViewModel> Questions { get; set; } = new();
        public List<ResponseListViewModel> Responses { get; set; } = new();
    }
}
