using Microsoft.AspNetCore.Identity;

namespace SurveyApp.Models
{
    // ✅ IdentityUser'dan türetiyoruz
    public class User : IdentityUser<int>  // int = UserId tipi
    {
        // IdentityUser zaten şunları içeriyor:
        // - Id (int)
        // - UserName
        // - Email
        // - PasswordHash
        // - PhoneNumber
        // - EmailConfirmed
        // - TwoFactorEnabled vb.

        // Ek alanlar:
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<Survey> Surveys { get; set; } = new List<Survey>();
        public ICollection<Response> Responses { get; set; } = new List<Response>();
    }
}