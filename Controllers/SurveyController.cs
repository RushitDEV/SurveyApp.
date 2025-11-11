using SurveyApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SurveyApp.Controllers
{
    public class SurveyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SurveyController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ✅ GET: Survey/Index - Sadece kullanıcının kendi anketleri
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Console.WriteLine($"💡 Giriş yapan kullanıcı ID: {userId}");

            var surveys = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.CreatedByUserId == userId,
                s => s.Questions,
                s => s.Responses
            );

            Console.WriteLine($"💡 Bulunan anket sayısı: {surveys.Count()}");

            foreach (var survey in surveys)
            {
                Console.WriteLine($"   - Anket: {survey.Title}, Sahibi: {survey.CreatedByUserId}");
            }

            var viewModel = _mapper.Map<List<SurveyListViewModel>>(surveys);
            return View(viewModel);
        }

        // GET: Survey/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Survey/Create
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SurveyCreateEditViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                Console.WriteLine($"💡 Anket oluşturan kullanıcı ID: {userId}");

                var survey = new Survey
                {
                    Title = model.Title,
                    Description = model.Description,
                    EndDate = model.EndDate,
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.Now,
                    CreatedByUserId = userId,
                    Questions = new List<Question>()
                };

                if (model.Questions != null && model.Questions.Any())
                {
                    int order = 1;
                    foreach (var q in model.Questions)
                    {
                        Enum.TryParse<QuestionType>(q.Type, out var questionType);

                        var question = new Question
                        {
                            QuestionText = q.QuestionText,
                            Type = questionType,
                            Order = order++,
                            IsRequired = q.IsRequired,
                            Options = new List<Option>()
                        };

                        if (q.Options != null && q.Options.Any())
                        {
                            foreach (var opt in q.Options)
                            {
                                question.Options.Add(new Option
                                {
                                    OptionText = opt.OptionText
                                });
                            }
                        }

                        survey.Questions.Add(question);
                    }
                }

                await _unitOfWork.Surveys.AddAsync(survey);
                await _unitOfWork.SaveChangesAsync();

                Console.WriteLine($"💡 Anket kaydedildi - ID: {survey.Id}, Sahibi: {survey.CreatedByUserId}");

                return Json(new { success = true, surveyId = survey.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ✅ GET: Survey/Details/5 - Sadece kendi anketinin detaylarını görebilir
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.Id == id && s.CreatedByUserId == userId,
                s => s.Questions,
                s => s.Responses
            );

            var surveyEntity = survey.FirstOrDefault();
            if (surveyEntity == null)
                return NotFound();

            foreach (var question in surveyEntity.Questions)
            {
                var options = await _unitOfWork.Options.GetWhereAsync(o => o.QuestionId == question.Id);
                question.Options = options.ToList();
            }

            var responses = await _unitOfWork.Responses.GetWhereWithIncludesAsync(
                r => r.SurveyId == id,
                r => r.Answers,
                r => r.User
            );

            foreach (var response in responses)
            {
                foreach (var answer in response.Answers)
                {
                    var question = surveyEntity.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question != null)
                    {
                        answer.Question = question;
                    }

                    if (answer.OptionId.HasValue)
                    {
                        var option = question?.Options?.FirstOrDefault(o => o.Id == answer.OptionId.Value);
                        if (option != null)
                        {
                            answer.Option = option;
                        }
                    }
                }
            }

            Console.WriteLine($"💡 Anket ID: {id}, Sahibi: {userId}");
            Console.WriteLine($"💡 Toplam yanıt sayısı: {responses.Count()}");

            foreach (var response in responses)
            {
                Console.WriteLine($"   - Yanıt ID: {response.Id}, Kullanıcı: {response.User?.Username ?? "Anonim"}, Tarih: {response.CreatedAt}");
                Console.WriteLine($"   - Bu yanıttaki cevap sayısı: {response.Answers?.Count ?? 0}");

                foreach (var answer in response.Answers)
                {
                    Console.WriteLine($"     * Soru ID: {answer.QuestionId}, Soru: {answer.Question?.QuestionText ?? "Yüklenemedi"}");
                    Console.WriteLine($"     * Cevap: {answer.AnswerText ?? answer.Option?.OptionText ?? "Boş"}");
                }
            }

            var viewModel = _mapper.Map<SurveyDetailViewModel>(surveyEntity);
            var responseViewModels = _mapper.Map<List<ResponseDetailViewModel>>(responses.ToList());

            ViewBag.Responses = responseViewModels;
            ViewBag.ResponseCount = responses.Count();

            return View(viewModel);
        }

        // GET: Survey/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.Id == id && s.CreatedByUserId == userId,
                s => s.Questions
            );

            var surveyEntity = survey.FirstOrDefault();
            if (surveyEntity == null)
                return NotFound();

            foreach (var question in surveyEntity.Questions)
            {
                var options = await _unitOfWork.Options.GetWhereAsync(o => o.QuestionId == question.Id);
                question.Options = options.ToList();
            }

            var viewModel = new SurveyCreateEditViewModel
            {
                Id = surveyEntity.Id,
                Title = surveyEntity.Title,
                Description = surveyEntity.Description,
                EndDate = surveyEntity.EndDate,
                IsActive = surveyEntity.IsActive,
                Questions = surveyEntity.Questions.Select(q => new QuestionCreateEditViewModel
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Type = q.Type.ToString(),
                    Order = q.Order,
                    IsRequired = q.IsRequired,
                    Options = q.Options.Select(o => new OptionViewModel
                    {
                        Id = o.Id,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Survey/Edit/5
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] SurveyCreateEditViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                    s => s.Id == id && s.CreatedByUserId == userId,
                    s => s.Questions
                );

                var surveyEntity = survey.FirstOrDefault();
                if (surveyEntity == null)
                    return Json(new { success = false, message = "Anket bulunamadı veya yetkiniz yok" });

                foreach (var question in surveyEntity.Questions)
                {
                    var options = await _unitOfWork.Options.GetWhereAsync(o => o.QuestionId == question.Id);
                    question.Options = options.ToList();
                }

                surveyEntity.Title = model.Title;
                surveyEntity.Description = model.Description;
                surveyEntity.EndDate = model.EndDate;
                surveyEntity.IsActive = model.IsActive;

                _unitOfWork.Questions.DeleteRange(surveyEntity.Questions);

                surveyEntity.Questions = new List<Question>();
                if (model.Questions != null && model.Questions.Any())
                {
                    int order = 1;
                    foreach (var q in model.Questions)
                    {
                        Enum.TryParse<QuestionType>(q.Type, out var questionType);

                        var question = new Question
                        {
                            QuestionText = q.QuestionText,
                            Type = questionType,
                            Order = order++,
                            IsRequired = q.IsRequired,
                            Options = new List<Option>()
                        };

                        if (q.Options != null && q.Options.Any())
                        {
                            foreach (var opt in q.Options)
                            {
                                question.Options.Add(new Option
                                {
                                    OptionText = opt.OptionText
                                });
                            }
                        }

                        surveyEntity.Questions.Add(question);
                    }
                }

                _unitOfWork.Surveys.Update(surveyEntity);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, surveyId = surveyEntity.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Survey/Delete/5
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                    s => s.Id == id && s.CreatedByUserId == userId,
                    s => s.Questions,
                    s => s.Responses
                );

                var surveyEntity = survey.FirstOrDefault();
                if (surveyEntity == null)
                    return Json(new { success = false, message = "Anket bulunamadı veya yetkiniz yok" });

                foreach (var question in surveyEntity.Questions)
                {
                    var options = await _unitOfWork.Options.GetWhereAsync(o => o.QuestionId == question.Id);
                    question.Options = options.ToList();
                }

                foreach (var response in surveyEntity.Responses)
                {
                    var answers = await _unitOfWork.Answers.GetWhereAsync(a => a.ResponseId == response.Id);
                    response.Answers = answers.ToList();
                }

                _unitOfWork.Answers.DeleteRange(surveyEntity.Responses.SelectMany(r => r.Answers));
                _unitOfWork.Options.DeleteRange(surveyEntity.Questions.SelectMany(q => q.Options));
                _unitOfWork.Questions.DeleteRange(surveyEntity.Questions);
                _unitOfWork.Responses.DeleteRange(surveyEntity.Responses);

                _unitOfWork.Surveys.Delete(surveyEntity);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}