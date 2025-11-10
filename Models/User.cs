namespace SurveyApp.Models  // ✅ Bu satır eksikti!
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "Admin" veya "User"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<Survey> Surveys { get; set; } = new List<Survey>();
        public ICollection<Response> Responses { get; set; } = new List<Response>();
    }
}