using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;

namespace SurveyApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var surveys = await _context.Surveys
                .Include(s => s.Questions)
                .Include(s => s.Responses)
                .ToListAsync();

            return View(surveys);
        }

        public async Task<IActionResult> Responses(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .Include(s => s.Responses)
                .ThenInclude(r => r.Answers)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
                return NotFound();

            return View(survey);
        }
    }
}
