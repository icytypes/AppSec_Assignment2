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
    options.IdleTimeout = TimeSpan.FromMinutes(1); // 1 minute for demo/testing
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
builder.Services.AddScoped<EmailLoggerService>();
builder.Services.AddScoped<EmailService>();
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
    // Use custom error page for production
    app.UseExceptionHandler("/500");
    app.UseHsts();
}
else
{
    // In development, show detailed errors
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware must be before UseAuthorization
app.UseSession();

// Custom middleware to check session timeout
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    
    // Skip session check for static files, login, register, error pages, logout, forgot password, reset password, test pages, and email viewer
    if (path.StartsWith("/lib") || path.StartsWith("/css") || path.StartsWith("/js") || 
        path.StartsWith("/uploads") || path.StartsWith("/_") || 
        path.Contains("/login") || path.Contains("/register") || 
        path.Contains("/error") || path.Contains("/logout") ||
        path.Contains("/forgotpassword") || path.Contains("/resetpassword") ||
        path.Contains("/404") || path.Contains("/403") || path.Contains("/500") ||
        path.Contains("/testerrors") || path.Contains("/viewemails"))
    {
        await next();
        return;
    }
    
    // List of known valid pages that require authentication
    var knownPages = new[] { "/", "/index", "/changepassword", "/enable2fa" };
    var isKnownPage = knownPages.Contains(path);
    
    // Only check session for known pages
    // Unknown pages will be handled by MapFallback (404) without requiring authentication
    if (isKnownPage)
    {
        var sessionService = context.RequestServices.GetRequiredService<SessionService>();
        if (!sessionService.IsSessionValid())
        {
            context.Response.Redirect("/Login?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
            return;
        }
    }
    
    // Let unknown pages through to MapFallback (they'll become 404s)
    await next();
});

app.UseAuthorization();

app.MapRazorPages();

// Add status code pages to handle 404s and other errors
app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    var statusCode = response.StatusCode;
    var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";

    // Skip if already on an error page to avoid redirect loops
    if (path.Contains("/404") || path.Contains("/403") || path.Contains("/500"))
    {
        return Task.CompletedTask;
    }

    // Only redirect if response hasn't started
    if (!response.HasStarted)
    {
        if (statusCode == 404)
        {
            response.Redirect("/404");
        }
        else if (statusCode == 403)
        {
            response.Redirect("/403");
        }
        else if (statusCode >= 500)
        {
            response.Redirect("/500");
        }
    }
    return Task.CompletedTask;
});

// Catch-all route handler for unmatched routes (404s)
// This MUST be after MapRazorPages to catch routes that Razor Pages doesn't handle
app.MapFallback(context =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    
    // Skip static files and error pages - let them be handled normally
    if (path.StartsWith("/lib") || path.StartsWith("/css") || path.StartsWith("/js") ||
        path.StartsWith("/uploads") || path.StartsWith("/_") ||
        path == "/404" || path == "/403" || path == "/500")
    {
        // Don't handle these - let them through or return 404
        context.Response.StatusCode = 404;
        return Task.CompletedTask;
    }
    
    // This is an unmatched route - redirect to 404 page
    context.Response.Redirect("/404");
    return Task.CompletedTask;
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
