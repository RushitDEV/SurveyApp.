using System;
using System.Collections.Generic;

namespace SurveyApp.Models
{
    public class Response
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }

        public string? UserIp { get; set; }  // ✅ Eksikmiş
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }  // ✅ Eksikmiş

        // Navigation properties
        public Survey Survey { get; set; } = null!;
        public List<Answer> Answers { get; set; } = new();
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
