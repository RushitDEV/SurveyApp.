using SurveyApp.Models;

namespace SurveyApp.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSurveys { get; set; }
        public int TotalResponses { get; set; }
        public int ActiveSurveys { get; set; }
        public List<SurveyListViewModel> RecentSurveys { get; set; } = new();
        public List<User> RecentUsers { get; set; } = new();
    }

    public class AdminStatisticsViewModel
    {
        public int TotalSurveys { get; set; }
        public int ActiveSurveys { get; set; }
        public int TotalResponses { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int SurveysCreatedThisMonth { get; set; }
        public int ResponsesThisMonth { get; set; }
    }
}