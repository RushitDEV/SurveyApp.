using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Hubs;
using SurveyApp.Models;
using SurveyApp.Repositories;
using SurveyApp.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Password policy
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User policy
    options.User.RequireUniqueEmail = true;

    // Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // SignIn
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ✅ Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    // 🔥 Docker/localhost'ta Always bazen cookie’yi bozuyor (HTTPS zorunlu olduğu için)
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;

    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Repository Pattern - Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// ✅ SignalR
builder.Services.AddSignalR();

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// ✅ Seed Roles + Admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

    Console.WriteLine("🔧 DB migrate ediliyor...");
    await context.Database.MigrateAsync();
    Console.WriteLine("✅ DB migrate tamam.");

    // Roles
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var roleRes = await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        if (!roleRes.Succeeded)
            throw new Exception("Admin rolü oluşturulamadı: " + string.Join(", ", roleRes.Errors.Select(e => e.Description)));
    }

    if (!await roleManager.RoleExistsAsync("User"))
    {
        var roleRes = await roleManager.CreateAsync(new IdentityRole<int>("User"));
        if (!roleRes.Succeeded)
            throw new Exception("User rolü oluşturulamadı: " + string.Join(", ", roleRes.Errors.Select(e => e.Description)));
    }

    // Admin user
    var adminUserName = "admin";
    var adminEmail = "admin@surveyapp.com";
    var adminPassword = "Admin123!";

    var adminUser = await userManager.FindByNameAsync(adminUserName);
    if (adminUser == null)
    {
        Console.WriteLine("👤 Admin bulunamadı, oluşturuluyor...");

        adminUser = new User
        {
            UserName = adminUserName,
            Email = adminEmail,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createRes = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createRes.Succeeded)
            throw new Exception("Admin kullanıcı oluşturulamadı: " + string.Join(", ", createRes.Errors.Select(e => e.Description)));

        var addRoleRes = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!addRoleRes.Succeeded)
            throw new Exception("Admin role atanamadı: " + string.Join(", ", addRoleRes.Errors.Select(e => e.Description)));

        Console.WriteLine("✅ Admin oluşturuldu!");
        Console.WriteLine("   Kullanıcı adı: admin");
        Console.WriteLine("   Şifre: Admin123!");
    }
    else
    {
        Console.WriteLine("✅ Admin zaten var (username: admin).");
    }
}

// Pipeline
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

// SignalR
app.MapHub<SurveyHub>("/surveyHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
