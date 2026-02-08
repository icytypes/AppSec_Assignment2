using Microsoft.EntityFrameworkCore;
using _231046Y_Assignment2.Data;
using _231046Y_Assignment2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add Entity Framework - Using SQLite (no installation required, works immediately)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=FreshFarmMarketDb.db"));

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name = "FreshFarmMarket.Session";
});

// Add HttpContextAccessor for services
builder.Services.AddHttpContextAccessor();

// Register custom services
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<CaptchaService>();
builder.Services.AddScoped<ReCaptchaService>();
builder.Services.AddScoped<AccountLockoutService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<PasswordPolicyService>();
builder.Services.AddScoped<InputSanitizationService>();
builder.Services.AddScoped<TwoFactorService>();
builder.Services.AddHttpClient<ReCaptchaService>();

// Add Antiforgery for CSRF protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Add status code pages for custom error handling (must be before UseHttpsRedirection)
app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        response.Redirect("/404");
    }
    else if (response.StatusCode == 403)
    {
        response.Redirect("/403");
    }
    else if (response.StatusCode >= 500)
    {
        response.Redirect("/500");
    }
    return Task.CompletedTask;
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware must be before UseAuthorization
app.UseSession();

// Custom middleware to check session timeout
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    
    // Skip session check for static files, login, register, error pages, logout, forgot password, and reset password
    if (path.StartsWith("/lib") || path.StartsWith("/css") || path.StartsWith("/js") || 
        path.StartsWith("/uploads") || path.StartsWith("/_") || 
        path.Contains("/login") || path.Contains("/register") || 
        path.Contains("/error") || path.Contains("/logout") ||
        path.Contains("/forgotpassword") || path.Contains("/resetpassword") ||
        path.Contains("/404") || path.Contains("/403") || path.Contains("/500"))
    {
        await next();
        return;
    }
    
    var sessionService = context.RequestServices.GetRequiredService<SessionService>();
    if (!sessionService.IsSessionValid())
    {
        context.Response.Redirect("/Login?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
        return;
    }
    
    await next();
});

app.UseAuthorization();

app.MapRazorPages();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
