using Flashcards.Authorization;
using Flashcards.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// For authentication/authorization

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Get connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("FlashcardsDbContextConnection")
                       ?? throw new InvalidOperationException(
                           "Failed to retrieve connection string 'FlashcardsDbContextConnection'");

// Add DB context using connection string
builder.Services.AddDbContext<FlashcardsDbContext>(options => { options.UseSqlite(connectionString); });

// NOTE: defaultIdentity. Exception if both AddDefaultIdentity and AddIdentity are run.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        // Password settings
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredUniqueChars = 6;

        // User settings
        options.User.RequireUniqueEmail = true;

        // Signin settings
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<FlashcardsDbContext>();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AdventureWorks.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(1800); // 30 minutes
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages();

builder.Services.AddSession();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Register DAL services for dependency injection
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IDeckRepository, DeckRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, SubjectAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DeckAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CardAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, SessionAuthorizationHandler>();

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information() // Sets information as minimum log level. 
    .WriteTo.File($"Logs/Flashcards_{DateTime.Now:yyyy-MM-dd_HHmmss}.log");

loggerConfig.Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out _) &&
                                     e.Level == LogEventLevel.Information &&
                                     e.MessageTemplate.Text.Contains("Executed DbCommand"));
var logger = loggerConfig.CreateLogger();
builder.Logging.AddSerilog(logger);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    DbInit.Seed(app);
}

// Allows using files in wwwroot folder
app.UseStaticFiles();

// For authentication/authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.MapRazorPages();

app.Run();