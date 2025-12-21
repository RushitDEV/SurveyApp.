using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repositories.Interfaces;
using SurveyApp.ViewModels;

namespace SurveyApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public AdminController(
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            UserManager<User> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var allSurveys = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => true,
                s => s.Questions,
                s => s.Responses,
                s => s.CreatedBy
            );

            var allUsers = await _userRepository.GetAllUsersAsync();
            var totalResponses = await _unitOfWork.Responses.CountAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = allUsers.Count(),
                TotalSurveys = allSurveys.Count(),
                TotalResponses = totalResponses,
                ActiveSurveys = allSurveys.Count(s => s.IsActive),
                RecentSurveys = _mapper.Map<List<SurveyListViewModel>>(
                    allSurveys.OrderByDescending(s => s.CreatedDate).Take(5)
                ),
                RecentUsers = allUsers.OrderByDescending(u => u.CreatedAt).Take(5).ToList()
            };

            return View(viewModel);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return View(users);
        }

        // GET: Admin/Surveys
        public async Task<IActionResult> Surveys()
        {
            var surveys = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => true,
                s => s.Questions,
                s => s.Responses,
                s => s.CreatedBy
            );

            var viewModel = _mapper.Map<List<SurveyListViewModel>>(surveys.OrderByDescending(s => s.CreatedDate));
            return View(viewModel);
        }

        // GET: Admin/SurveyDetails/5
        public async Task<IActionResult> SurveyDetails(int id)
        {
            var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => s.Id == id,
                s => s.Questions,
                s => s.Responses,
                s => s.CreatedBy
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
                        if (answer.OptionId.HasValue)
                        {
                            answer.Option = question.Options?.FirstOrDefault(o => o.Id == answer.OptionId.Value);
                        }
                    }
                }
            }

            var viewModel = _mapper.Map<SurveyDetailViewModel>(surveyEntity);
            ViewBag.Responses = _mapper.Map<List<ResponseDetailViewModel>>(responses.ToList());
            ViewBag.ResponseCount = responses.Count();

            return View(viewModel);
        }

        // POST: Admin/DeleteSurvey/5
        [HttpPost]
        public async Task<IActionResult> DeleteSurvey(int id)
        {
            try
            {
                var survey = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                    s => s.Id == id,
                    s => s.Questions,
                    s => s.Responses
                );

                var surveyEntity = survey.FirstOrDefault();
                if (surveyEntity == null)
                    return Json(new { success = false, message = "Anket bulunamadı" });

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

                return Json(new { success = true, message = "Anket silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // POST: Admin/ToggleUserStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı bulunamadı" });

                user.IsActive = !user.IsActive;

                // ✅ UserManager kullanarak güncelle
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Json(new { success = false, message = "Güncelleme başarısız" });
                }

                return Json(new { success = true, isActive = user.IsActive });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            var surveys = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => true,
                s => s.Responses
            );

            var users = await _userRepository.GetAllUsersAsync();

            var stats = new AdminStatisticsViewModel
            {
                TotalSurveys = surveys.Count(),
                ActiveSurveys = surveys.Count(s => s.IsActive),
                TotalResponses = surveys.Sum(s => s.Responses.Count),
                TotalUsers = users.Count(),
                ActiveUsers = users.Count(u => u.IsActive),
                SurveysCreatedThisMonth = surveys.Count(s => s.CreatedDate.Month == DateTime.Now.Month && s.CreatedDate.Year == DateTime.Now.Year),
                ResponsesThisMonth = surveys.SelectMany(s => s.Responses).Count(r => r.CreatedAt.Month == DateTime.Now.Month && r.CreatedAt.Year == DateTime.Now.Year)
            };

            return View(stats);
        }
    }
}