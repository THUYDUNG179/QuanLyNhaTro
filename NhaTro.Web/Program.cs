using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NhaTro.Application.Interfaces;
using NhaTro.Application.Services;
using NhaTro.Domain.Interfaces;
using NhaTro.Infrastructure.Data;
using NhaTro.Infrastructure.Repositories; // Ensure this using directive is present to resolve 'Repository<>'
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Localization; // Thêm dòng này ?? s? d?ng HttpContext.Session

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// C?u hình DbContext
builder.Services.AddDbContext<QLyNhaTroDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLyNhaTro")));

// ??ng ký Repository và Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMotelService, MotelService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IUtilityReadingService, UtilityReadingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// C?u hình Authentication (Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// C?U HÌNH SESSION
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var supportedCultures = new[] { new CultureInfo("vi-VN") };

// Thêm localization service
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// MIDDLEWARE SESSION (tr??c UseAuthentication và UseAuthorization)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// CÁC C?U HÌNH ROUTING ?Ã S?A TH? T?
app.MapStaticAssets(); // V?n gi? nguyên n?u b?n có method m? r?ng này

// ??NH NGH?A ROUTE CHO AREAS TR??C ROUTE M?C ??NH
app.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// ROUTE M?C ??NH SAU CÙNG
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets(); // V?n gi? nguyên n?u b?n có method m? r?ng này

app.Run();
