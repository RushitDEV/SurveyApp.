using System;
using System.Collections.Generic;

namespace SurveyApp.ViewModels
{
    // 🔹 Soru detay görünümü (SurveyDetailViewModel içinde kullanılıyor)
    public class QuestionDetailViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;

        // ❗ Enum yerine string (örneğin: "SingleChoice", "MultipleChoice")
        public string Type { get; set; } = "SingleChoice";

        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public List<OptionViewModel> Options { get; set; } = new();
    }

    // 🔹 Soru oluşturma ve düzenleme için (SurveyCreateEditViewModel içinde)
    public class QuestionCreateEditViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;

        // ❗ Enum yerine string olarak tanımlı olmalı
        public string Type { get; set; } = "SingleChoice";

        public int Order { get; set; }
        public bool IsRequired { get; set; }

        // ❗ Options burada ViewModel olarak kalmalı, string değil
        public List<OptionViewModel> Options { get; set; } = new();
    }

    // 🔹 Seçenek görünümü (OptionViewModel)
    public class OptionViewModel
    {
        public int Id { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }

    // 🔹 İstatistik görünümü (Admin tarafında kullanılıyor)
    public class QuestionStatisticsViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Type { get; set; } = "SingleChoice";
        public int TotalAnswers { get; set; }
        public double? AverageRating { get; set; }    // Puan ortalaması sorular için
        public List<OptionStatisticsViewModel> OptionStatistics { get; set; } = new();
        public List<string>? TextAnswers { get; set; } = new();
    }

    // 🔹 Seçenek istatistikleri
    public class OptionStatisticsViewModel
    {
        public int Id { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
