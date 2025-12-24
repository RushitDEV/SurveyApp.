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

        // ---------------- DASHBOARD ----------------
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

        // ---------------- USERS ----------------
        public async Task<IActionResult> Users()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return View(users);
        }

        // 🔁 ROL DEĞİŞTİRME (JSON UYUMLU)
        [HttpPost]
        public async Task<IActionResult> ToggleUserRole([FromBody] ToggleUserRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı" });

            // ❗ Ana admin korunur
            if (user.UserName == "admin")
                return Json(new { success = false, message = "Ana adminin rolü değiştirilemez" });

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return Json(new
            {
                success = true,
                newRole = isAdmin ? "User" : "Admin"
            });
        }

        // ---------------- SURVEYS ----------------
        public async Task<IActionResult> Surveys()
        {
            var surveys = await _unitOfWork.Surveys.GetWhereWithIncludesAsync(
                s => true,
                s => s.Questions,
                s => s.Responses,
                s => s.CreatedBy
            );

            var viewModel = _mapper.Map<List<SurveyListViewModel>>(
                surveys.OrderByDescending(s => s.CreatedDate)
            );

            return View(viewModel);
        }

        // ---------------- STATISTICS ----------------
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
                SurveysCreatedThisMonth = surveys.Count(s =>
                    s.CreatedDate.Month == DateTime.Now.Month &&
                    s.CreatedDate.Year == DateTime.Now.Year),
                ResponsesThisMonth = surveys
                    .SelectMany(s => s.Responses)
                    .Count(r =>
                        r.CreatedAt.Month == DateTime.Now.Month &&
                        r.CreatedAt.Year == DateTime.Now.Year)
            };

            return View(stats);
        }
    }

    // 🔹 JSON Request DTO
    public class ToggleUserRoleRequest
    {
        public int UserId { get; set; }
    }
}
