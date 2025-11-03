using SurveyApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Repositories.Interfaces;

namespace SurveyApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: Admin/Index - Ana dashboard
        public async Task<IActionResult> Index()
        {
            var surveys = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => true,
                s => s.Questions,
                s => s.Responses
            );

            var viewModel = _mapper.Map<List<SurveyListViewModel>>(surveys);
            return View(viewModel);
        }

        // GET: Admin/Responses/5 - Anket yanıtlarını görüntüle
        public async Task<IActionResult> Responses(int id)
        {
            var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.Id == id,
                s => s.Questions,
                s => s.Responses
            );

            var surveyEntity = survey.FirstOrDefault();
            if (surveyEntity == null)
                return NotFound();

            // Options'ları yükle
            foreach (var question in surveyEntity.Questions)
            {
                var options = await _unitOfWork.Options.GetWhereAsync(o => o.QuestionId == question.Id);
                question.Options = options.ToList();
            }

            // Responses'ların Answers'larını yükle
            foreach (var response in surveyEntity.Responses)
            {
                var answers = await _unitOfWork.Answers.GetWhereAsync(a => a.ResponseId == response.Id);
                response.Answers = answers.ToList();

                // Her answer için question ve option bilgisini yükle
                foreach (var answer in response.Answers)
                {
                    answer.Question = surveyEntity.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (answer.OptionId.HasValue && answer.Question != null)
                    {
                        answer.Option = answer.Question.Options.FirstOrDefault(o => o.Id == answer.OptionId);
                    }
                }
            }

            var viewModel = _mapper.Map<SurveyStatisticsViewModel>(surveyEntity);

            // İstatistikleri hesapla
            viewModel = CalculateStatistics(viewModel, surveyEntity);

            return View(viewModel);
        }

        // GET: Admin/Statistics/5 - Detaylı istatistikler
        public async Task<IActionResult> Statistics(int id)
        {
            var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.Id == id,
                s => s.Questions,
                s => s.Responses
            );

            var surveyEntity = survey.FirstOrDefault();
            if (surveyEntity == null)
                return NotFound();

            // Options'ları yükle
            foreach (var question in surveyEntity.Questions)
            {
                var options = await _unitOfWork.Options.GetWhereAsync(o => o.QuestionId == question.Id);
                question.Options = options.ToList();
            }

            // Tüm answers'ları yükle
            var allAnswers = await _unitOfWork.Answers.GetWhereWithIncludesAsync(
                a => surveyEntity.Questions.Select(q => q.Id).Contains(a.QuestionId),
                a => a.Question,
                a => a.Option
            );

            foreach (var question in surveyEntity.Questions)
            {
                question.Answers = allAnswers.Where(a => a.QuestionId == question.Id).ToList();
            }

            var viewModel = _mapper.Map<SurveyStatisticsViewModel>(surveyEntity);
            viewModel = CalculateStatistics(viewModel, surveyEntity);

            return View(viewModel);
        }

        // GET: Admin/ExportResults/5 - Sonuçları Excel olarak indir
        public async Task<IActionResult> ExportResults(int id)
        {
            // Bu metodu daha sonra implement edebilirsiniz
            // EPPlus veya ClosedXML gibi kütüphanelerle Excel export yapabilirsiniz

            return RedirectToAction(nameof(Responses), new { id });
        }

        // Helper method - İstatistik hesaplama
        private SurveyStatisticsViewModel CalculateStatistics(SurveyStatisticsViewModel viewModel, Models.Survey survey)
        {
            foreach (var questionViewModel in viewModel.Questions)
            {
                var question = survey.Questions.FirstOrDefault(q => q.Id == questionViewModel.Id);
                if (question == null) continue;

                questionViewModel.TotalAnswers = question.Answers.Count;

                // Seçenekli sorular için istatistik hesapla
                if (question.Type == Models.QuestionType.SingleChoice ||
                    question.Type == Models.QuestionType.MultipleChoice)
                {
                    questionViewModel.OptionStatistics = new List<OptionStatisticsViewModel>();

                    foreach (var option in question.Options)
                    {
                        var count = question.Answers.Count(a => a.OptionId == option.Id);
                        var percentage = questionViewModel.TotalAnswers > 0
                            ? (double)count / questionViewModel.TotalAnswers * 100
                            : 0;

                        questionViewModel.OptionStatistics.Add(new OptionStatisticsViewModel
                        {
                            Id = option.Id,
                            OptionText = option.OptionText,
                            Count = count,
                            Percentage = Math.Round(percentage, 2)
                        });
                    }
                }

                // Açık uçlu sorular için cevapları listele
                if (question.Type == Models.QuestionType.OpenEnded ||
                    question.Type == Models.QuestionType.ShortText)
                {
                    questionViewModel.TextAnswers = question.Answers
                        .Where(a => !string.IsNullOrEmpty(a.AnswerText))
                        .Select(a => a.AnswerText!)
                        .ToList();
                }

                // Rating soruları için ortalama hesapla
                if (question.Type == Models.QuestionType.Rating5 ||
                    question.Type == Models.QuestionType.Rating10)
                {
                    var ratings = question.Answers
                        .Where(a => a.RatingValue.HasValue)
                        .Select(a => a.RatingValue!.Value);

                    if (ratings.Any())
                    {
                        questionViewModel.AverageRating = Math.Round(ratings.Average(), 2);
                    }
                }
            }

            return viewModel;
        }

        // GET: Admin/DeleteResponse/5 - Yanıt silme
        [HttpPost]
        public async Task<IActionResult> DeleteResponse(int id, int surveyId)
        {
            try
            {
                var response = await _unitOfWork.Responses.GetByIdAsync(id);
                if (response == null)
                    return Json(new { success = false, message = "Yanıt bulunamadı" });

                _unitOfWork.Responses.Delete(response);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, message = "Yanıt silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
    }
}