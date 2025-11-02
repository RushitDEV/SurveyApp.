using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Models;

namespace SurveyApp.Controllers
{
    public class ResponseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ResponseController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Anketi göster
        public async Task<IActionResult> TakeSurvey(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (survey == null)
                return NotFound();

            // Anket süresi dolmuş mu kontrol et
            if (survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now)
            {
                ViewBag.Expired = true;
                return View(survey);
            }

            return View(survey);
        }

        // Cevapları kaydet - DÜZELTİLMİŞ
        [HttpPost]
        public async Task<IActionResult> SubmitResponse([FromBody] SubmitResponseModel model)
        {
            try
            {
                var survey = await _context.Surveys
                    .Include(s => s.Questions)
                    .FirstOrDefaultAsync(s => s.Id == model.SurveyId);

                if (survey == null || !survey.IsActive)
                    return Json(new { success = false, message = "Anket bulunamadı veya aktif değil" });

                if (survey.EndDate.HasValue && survey.EndDate.Value.Date < DateTime.Now.Date)
                    return Json(new { success = false, message = "Anket süresi dolmuş" });

                // IP adresini al
                var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                // Response oluştur - ÖNCE KAYDET
                var response = new Response
                {
                    SurveyId = model.SurveyId,
                    UserIp = userIp,
                    CompletedDate = DateTime.Now
                };

                _context.Responses.Add(response);
                await _context.SaveChangesAsync(); // ÖNCE Response'u kaydet ki ID alabilelim

                // Şimdi Answers'ı ekle
                foreach (var answerModel in model.Answers)
                {
                    var question = survey.Questions.FirstOrDefault(q => q.Id == answerModel.QuestionId);
                    if (question == null) continue;

                    // Soru tipine göre cevabı kaydet
                    switch (question.Type)
                    {
                        case QuestionType.SingleChoice:
                            if (answerModel.OptionId.HasValue)
                            {
                                var answer = new Answer
                                {
                                    ResponseId = response.Id, // Artık ID var
                                    QuestionId = answerModel.QuestionId,
                                    OptionId = answerModel.OptionId,
                                    AnswerDate = DateTime.Now
                                };
                                _context.Answers.Add(answer);
                            }
                            break;

                        case QuestionType.MultipleChoice:
                            // Çoklu seçim için her seçenek ayrı answer
                            if (answerModel.OptionIds != null && answerModel.OptionIds.Any())
                            {
                                foreach (var optionId in answerModel.OptionIds)
                                {
                                    var answer = new Answer
                                    {
                                        ResponseId = response.Id,
                                        QuestionId = answerModel.QuestionId,
                                        OptionId = optionId,
                                        AnswerDate = DateTime.Now
                                    };
                                    _context.Answers.Add(answer);
                                }
                            }
                            break;

                        case QuestionType.OpenEnded:
                        case QuestionType.ShortText:
                            if (!string.IsNullOrEmpty(answerModel.AnswerText))
                            {
                                var answer = new Answer
                                {
                                    ResponseId = response.Id,
                                    QuestionId = answerModel.QuestionId,
                                    AnswerText = answerModel.AnswerText,
                                    AnswerDate = DateTime.Now
                                };
                                _context.Answers.Add(answer);
                            }
                            break;

                        case QuestionType.Number:
                            if (answerModel.NumberValue.HasValue)
                            {
                                var answer = new Answer
                                {
                                    ResponseId = response.Id,
                                    QuestionId = answerModel.QuestionId,
                                    AnswerText = answerModel.NumberValue.ToString(),
                                    AnswerDate = DateTime.Now
                                };
                                _context.Answers.Add(answer);
                            }
                            break;

                        case QuestionType.DatePicker:
                            if (answerModel.DateValue.HasValue)
                            {
                                var answer = new Answer
                                {
                                    ResponseId = response.Id,
                                    QuestionId = answerModel.QuestionId,
                                    AnswerText = answerModel.DateValue.Value.ToString("yyyy-MM-dd"),
                                    AnswerDate = DateTime.Now
                                };
                                _context.Answers.Add(answer);
                            }
                            break;

                        case QuestionType.Rating5:
                        case QuestionType.Rating10:
                            if (answerModel.RatingValue.HasValue)
                            {
                                var answer = new Answer
                                {
                                    ResponseId = response.Id,
                                    QuestionId = answerModel.QuestionId,
                                    RatingValue = answerModel.RatingValue,
                                    AnswerDate = DateTime.Now
                                };
                                _context.Answers.Add(answer);
                            }
                            break;

                        case QuestionType.ImageUpload:
                        case QuestionType.FileUpload:
                            if (!string.IsNullOrEmpty(answerModel.FileName))
                            {
                                var answer = new Answer
                                {
                                    ResponseId = response.Id,
                                    QuestionId = answerModel.QuestionId,
                                    FilePath = answerModel.FileName,
                                    AnswerDate = DateTime.Now
                                };
                                _context.Answers.Add(answer);
                            }
                            break;
                    }
                }

                await _context.SaveChangesAsync(); // Tüm Answers'ı kaydet

                return Json(new { success = true, message = "Cevaplarınız kaydedildi. Teşekkürler!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message + " | " + ex.InnerException?.Message });
            }
        }

        // Dosya yükleme
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

        // Teşekkür sayfası
        public IActionResult ThankYou()
        {
            return View();
        }
    }

    // ViewModel'ler
    public class SubmitResponseModel
    {
        public int SurveyId { get; set; }
        public List<AnswerSubmitModel> Answers { get; set; }
    }

    public class AnswerSubmitModel
    {
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public List<int>? OptionIds { get; set; }
        public string? AnswerText { get; set; }
        public int? NumberValue { get; set; }
        public DateTime? DateValue { get; set; }
        public int? RatingValue { get; set; }
        public string? FileName { get; set; }
    }
}