using System.Collections.Generic;

namespace SurveyApp.ViewModels
{
    // Kullanıcının anketi yanıtlarken kullanacağı model
    public class SurveyTakeViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<QuestionTakeViewModel> Questions { get; set; } = new();
    }

    // Kullanıcıya gösterilecek soru modeli
    public class QuestionTakeViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<OptionViewModel> Options { get; set; } = new();
    }
}
