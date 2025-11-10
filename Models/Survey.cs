namespace SurveyApp.Models
{
    public class Survey
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }

        // İlişkiler
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<Response> Responses { get; set; } = new List<Response>(); // 🔥 Bu satır eklendi
    }
}
