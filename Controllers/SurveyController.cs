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
            // Giriş yapmış kullanıcının ID'sini al
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // ✅ DEBUG: Console'a yazdır
            Console.WriteLine($"💡 Giriş yapan kullanıcı ID: {userId}");

            // Sadece bu kullanıcının anketlerini getir
            var surveys = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.CreatedByUserId == userId, // ✅ BURASI DEĞİŞTİ
                s => s.Questions,
                s => s.Responses
            );

            // ✅ DEBUG: Kaç anket bulundu
            Console.WriteLine($"💡 Bulunan anket sayısı: {surveys.Count()}");

            // ✅ DEBUG: Her anketin sahibini yazdır
            foreach (var survey in surveys)
            {
                Console.WriteLine($"   - Anket: {survey.Title}, Sahibi: {survey.CreatedByUserId}");
            }

            var viewModel = _mapper.Map<List<SurveyListViewModel>>(surveys);
            return View(viewModel);
        }

        // GET: Survey/Create - Yeni anket oluşturma sayfası
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Survey/Create - Yeni anket kaydetme
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SurveyCreateEditViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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

                return Json(new { success = true, surveyId = survey.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Survey/Details/5 - Anket detayları (herkes görebilir)
        public async Task<IActionResult> Details(int id)
        {
            var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.Id == id,
                s => s.Questions,
                s => s.Responses
            );

            var surveyEntity = survey.FirstOrDefault();
            if (surveyEntity == null)
                return NotFound();

            var responses = await _unitOfWork.Responses.GetWhereWithIncludesAsync(
                r => r.SurveyId == id,
                r => r.Answers
            );

            var viewModel = _mapper.Map<SurveyDetailViewModel>(surveyEntity);
            ViewBag.Responses = responses.ToList();
            ViewBag.ResponseCount = responses.Count();

            return View(viewModel);
        }

        // ✅ GET: Survey/Edit/5 - Sadece kendi anketini düzenleyebilir
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.Id == id && s.CreatedByUserId == userId, // ✅ YETKİ KONTROLÜ EKLENDİ
                s => s.Questions
            );

            var surveyEntity = survey.FirstOrDefault();
            if (surveyEntity == null)
                return NotFound(); // veya Forbid() kullanabilirsiniz

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

        // ✅ POST: Survey/Edit/5 - Sadece kendi anketini güncelleyebilir
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] SurveyCreateEditViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                    s => s.Id == id && s.CreatedByUserId == userId, // ✅ YETKİ KONTROLÜ EKLENDİ
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

        // ✅ POST: Survey/Delete/5 - Sadece kendi anketini silebilir
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                    s => s.Id == id && s.CreatedByUserId == userId, // ✅ YETKİ KONTROLÜ EKLENDİ
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