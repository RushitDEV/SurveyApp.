using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Repositories;
using SurveyApp.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using SurveyApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Pattern - Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Auth Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// ✅ Admin kullanıcısı oluştur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var authService = services.GetRequiredService<IAuthService>();

        // Database'i oluştur
        context.Database.Migrate();

        // Admin kullanıcısı yoksa oluştur
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@surveyapp.com",
                PasswordHash = authService.HashPassword("Admin123!"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Users.Add(adminUser);
            context.SaveChanges();

            Console.WriteLine("✅ Admin kullanıcısı oluşturuldu!");
            Console.WriteLine("   Kullanıcı adı: admin");
            Console.WriteLine("   Şifre: Admin123!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Admin oluşturma hatası: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();