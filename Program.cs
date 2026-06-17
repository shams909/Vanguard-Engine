using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Data;
using Vanguard_Engine.Repositories;
using Vanguard_Engine.Services;
using Vanguard_Engine.UnitOfWork;
using Vanguard_Engine.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IAppwriteService, AppwriteService>();
builder.Services.AddScoped<IUnitOfWork, AppwriteUnitOfWork>();
builder.Services.AddScoped<IRoleRepository, AppwriteRoleRepository>();
builder.Services.AddScoped<IUserRepository, AppwriteUserRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGuardApplicationService, GuardApplicationService>();
builder.Services.AddScoped<IHiringService, HiringService>();
builder.Services.AddScoped<IClientRequestService, ClientRequestService>();
builder.Services.AddScoped<IVIPRequestService, VIPRequestService>();
builder.Services.AddScoped<IGuardShiftService, GuardShiftService>();
builder.Services.AddScoped<IVipApplicationService, VipApplicationService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

var app = builder.Build();

// Seed data
// await DbInitializer.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<NotificationHub>("/notificationHub");
app.Run();
