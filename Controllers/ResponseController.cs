using SurveyApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repositories.Interfaces;

namespace SurveyApp.Controllers
{
    public class ResponseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public ResponseController(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _environment = environment;
        }

        // GET: Response/TakeSurvey/5 - Anketi göster
        public async Task<IActionResult> TakeSurvey(int id)
        {
            var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.Id == id && s.IsActive,
                s => s.Questions
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

            // Anket süresi dolmuş mu kontrol et
            if (surveyEntity.EndDate.HasValue && surveyEntity.EndDate.Value < DateTime.Now)
            {
                ViewBag.Expired = true;
            }

            var viewModel = _mapper.Map<SurveyTakeViewModel>(surveyEntity);
            return View(viewModel);
        }

        // POST: Response/SubmitResponse - Cevapları kaydet
        [HttpPost]
        public async Task<IActionResult> SubmitResponse([FromBody] SubmitResponseViewModel model)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                    s => s.Id == model.SurveyId,
                    s => s.Questions
                );

                var surveyEntity = survey.FirstOrDefault();
                if (surveyEntity == null || !surveyEntity.IsActive)
                {
                    return Json(new { success = false, message = "Anket bulunamadı veya aktif değil" });
                }

                if (surveyEntity.EndDate.HasValue && surveyEntity.EndDate.Value.Date < DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "Anket süresi dolmuş" });
                }

                // IP adresini al
                var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                // Response oluştur
                var response = new Response
                {
                    SurveyId = model.SurveyId,
                    UserIp = userIp,
                    CompletedDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Responses.AddAsync(response);
                await _unitOfWork.SaveChangesAsync(); // Response'u kaydet ki ID alabilelim

                // Answers'ı ekle
                foreach (var answerModel in model.Answers)
                {
                    var question = surveyEntity.Questions.FirstOrDefault(q => q.Id == answerModel.QuestionId);
                    if (question == null) continue;

                    await ProcessAnswer(response.Id, answerModel, question.Type);
                }

                await _unitOfWork.CommitTransactionAsync();

                return Json(new { success = true, message = "Cevaplarınız kaydedildi. Teşekkürler!" });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // Helper method - Cevap işleme
        private async Task ProcessAnswer(int responseId, AnswerSubmitViewModel answerModel, QuestionType questionType)
        {
            switch (questionType)
            {
                case QuestionType.SingleChoice:
                    if (answerModel.OptionId.HasValue)
                    {
                        var answer = new Answer
                        {
                            ResponseId = responseId,
                            QuestionId = answerModel.QuestionId,
                            OptionId = answerModel.OptionId,
                            AnswerDate = DateTime.Now
                        };
                        await _unitOfWork.Answers.AddAsync(answer);
                    }
                    break;

                case QuestionType.MultipleChoice:
                    if (answerModel.OptionIds != null && answerModel.OptionIds.Any())
                    {
                        foreach (var optionId in answerModel.OptionIds)
                        {
                            var answer = new Answer
                            {
                                ResponseId = responseId,
                                QuestionId = answerModel.QuestionId,
                                OptionId = optionId,
                                AnswerDate = DateTime.Now
                            };
                            await _unitOfWork.Answers.AddAsync(answer);
                        }
                    }
                    break;

                case QuestionType.OpenEnded:
                case QuestionType.ShortText:
                    if (!string.IsNullOrEmpty(answerModel.AnswerText))
                    {
                        var answer = new Answer
                        {
                            ResponseId = responseId,
                            QuestionId = answerModel.QuestionId,
                            AnswerText = answerModel.AnswerText,
                            AnswerDate = DateTime.Now
                        };
                        await _unitOfWork.Answers.AddAsync(answer);
                    }
                    break;

                case QuestionType.Number:
                    if (answerModel.NumberValue.HasValue)
                    {
                        var answer = new Answer
                        {
                            ResponseId = responseId,
                            QuestionId = answerModel.QuestionId,
                            AnswerText = answerModel.NumberValue.ToString(),
                            AnswerDate = DateTime.Now
                        };
                        await _unitOfWork.Answers.AddAsync(answer);
                    }
                    break;

                case QuestionType.DatePicker:
                    if (answerModel.DateValue.HasValue)
                    {
                        var answer = new Answer
                        {
                            ResponseId = responseId,
                            QuestionId = answerModel.QuestionId,
                            AnswerText = answerModel.DateValue.Value.ToString("yyyy-MM-dd"),
                            AnswerDate = DateTime.Now
                        };
                        await _unitOfWork.Answers.AddAsync(answer);
                    }
                    break;

                case QuestionType.Rating5:
                case QuestionType.Rating10:
                    if (answerModel.RatingValue.HasValue)
                    {
                        var answer = new Answer
                        {
                            ResponseId = responseId,
                            QuestionId = answerModel.QuestionId,
                            RatingValue = answerModel.RatingValue,
                            AnswerDate = DateTime.Now
                        };
                        await _unitOfWork.Answers.AddAsync(answer);
                    }
                    break;

                case QuestionType.ImageUpload:
                case QuestionType.FileUpload:
                    if (!string.IsNullOrEmpty(answerModel.FileName))
                    {
                        var answer = new Answer
                        {
                            ResponseId = responseId,
                            QuestionId = answerModel.QuestionId,
                            FilePath = answerModel.FileName,
                            AnswerDate = DateTime.Now
                        };
                        await _unitOfWork.Answers.AddAsync(answer);
                    }
                    break;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // POST: Response/UploadFile - Dosya yükleme
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int questionId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Dosya seçilmedi" });

                // Dosya boyutu kontrolü (5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return Json(new { success = false, message = "Dosya boyutu 5MB'dan küçük olmalıdır" });

                // Güvenli dosya adı oluştur
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "responses");

                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Json(new { success = true, fileName = fileName, filePath = $"/uploads/responses/{fileName}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Dosya yükleme hatası: " + ex.Message });
            }
        }

        // GET: Response/ThankYou - Teşekkür sayfası
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}