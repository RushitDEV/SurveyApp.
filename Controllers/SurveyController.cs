using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Models;
using System.Text.Json;

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
                .Include(s => s.Questions)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
            return View(surveys);
        }

        // Yeni anket oluþturma sayfasý
        public IActionResult Create()
        {
            return View();
        }

        // Anket kaydetme (Sorularla birlikte)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SurveyCreateModel model)
        {
            var survey = new Survey
            {
                Title = model.Title,
                Description = model.Description,
                EndDate = model.EndDate,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                Questions = new List<Question>()
            };

            if (model.Questions != null && model.Questions.Any())
            {
                int order = 1;
                foreach (var q in model.Questions)
                {
                    var question = new Question
                    {
                        QuestionText = q.QuestionText,
                        Type = (QuestionType)q.Type,
                        Order = order++,
                        Options = new List<Option>()
                    };

                    if (q.Options != null && q.Options.Any())
                    {
                        foreach (var opt in q.Options)
                        {
                            question.Options.Add(new Option { OptionText = opt });
                        }
                    }

                    survey.Questions.Add(question);
                }
            }

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();

            return Json(new { success = true, surveyId = survey.Id });
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

        // Anket düzenleme sayfasý
        public async Task<IActionResult> Edit(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
                return NotFound();

            return View(survey);
        }

        // Anket güncelleme
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] SurveyCreateModel model)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null)
                return NotFound();

            survey.Title = model.Title;
            survey.Description = model.Description;
            survey.EndDate = model.EndDate;
            survey.IsActive = model.IsActive;

            // Eski sorularý sil
            _context.Questions.RemoveRange(survey.Questions);

            // Yeni sorularý ekle
            survey.Questions = new List<Question>();
            if (model.Questions != null && model.Questions.Any())
            {
                int order = 1;
                foreach (var q in model.Questions)
                {
                    var question = new Question
                    {
                        QuestionText = q.QuestionText,
                        Type = (QuestionType)q.Type,
                        Order = order++,
                        Options = new List<Option>()
                    };

                    if (q.Options != null && q.Options.Any())
                    {
                        foreach (var opt in q.Options)
                        {
                            question.Options.Add(new Option { OptionText = opt });
                        }
                    }

                    survey.Questions.Add(question);
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, surveyId = survey.Id });
        }

        // Anket silme
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            if (survey == null)
                return NotFound();

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }

    // ViewModel
    public class SurveyCreateModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<QuestionModel> Questions { get; set; }
    }

    public class QuestionModel
    {
        public string QuestionText { get; set; }
        public int Type { get; set; }
        public List<string> Options { get; set; }
    }
}