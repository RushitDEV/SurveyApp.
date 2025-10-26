using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Models;

namespace SurveyApp.Controllers
{
    public class SurveyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SurveyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Anket listesi
        public async Task<IActionResult> Index()
        {
            var surveys = await _context.Surveys
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
            return View(surveys);
        }

        // Yeni anket oluþturma sayfasý
        public IActionResult Create()
        {
            return View();
        }

        // Anket kaydetme
        [HttpPost]
        public async Task<IActionResult> Create(Survey survey)
        {
            survey.CreatedDate = DateTime.Now;
            survey.IsActive = true;
            
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // Anket detaylarý
        public async Task<IActionResult> Details(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
                return NotFound();

            return View(survey);
        }
    }
}
