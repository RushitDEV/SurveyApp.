namespace SurveyApp.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }  // Hangi cevap setine ait
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }  // Tek/Çok seçmeli için
        public string? AnswerText { get; set; }  // Metin, sayý, tarih için
        public string? FilePath { get; set; }  // Resim/Dosya yükleme için
        public int? RatingValue { get; set; }  // Derecelendirme için
        public DateTime AnswerDate { get; set; }

        // Navigation properties
        public Response Response { get; set; }
        public Question Question { get; set; }
        public Option Option { get; set; }
    }
}