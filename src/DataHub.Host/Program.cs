using RPK_BlazorApp.Data; // Předpokládáme, že ApplicationDbContext je zde
using RPK_BlazorApp.Data.Interceptors; // Předpokládáme, že ApplicationDbContext je zde
using RPK_BlazorApp.Services; // Předpokládáme, že IAllarmService a AllarmService jsou zde
using RPK_BlazorApp.Repositories; // Přidáno pro IAlarmRepository a AlarmRepository
using RPK_BlazorApp.Shared; // Potřebné pro AppPermissions
using RPK_BlazorApp.Models; // Potřebné pro Alarm
using RPK_BlazorApp; // Přidáno pro MappingProfile
using RPK_BlazorApp.Models.Interfaces; // Added for IDataService
using RPK_BlazorApp.Repositories.Generic; // Add this for IRepository and Repository

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore; // Potřebné pro AddDbContext
using Microsoft.FluentUI.AspNetCore.Components; // Potřebné pro AddFluentUIComponents




// ... existing usings

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add services for API controllers
builder.Services.AddControllers();

// Add FluentUI services
builder.Services.AddFluentUIComponents();

builder.Services.AddTransient<ComprehensiveAuditInterceptor>();
// ...existing code...
builder.Services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RestTestConnection"))
           .AddInterceptors(sp.GetRequiredService<ComprehensiveAuditInterceptor>());
});

// Register EisodDbContext for VIEW_EISOD_SD
builder.Services.AddDbContextFactory<EisodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EisodConnection"),
        sqlOptions => sqlOptions.CommandTimeout(120)));

builder.Services.AddHttpContextAccessor();

// Použijeme typeof(MappingProfile).Assembly pro explicitní určení sestavení, kde se profily nacházejí.
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);


builder.Services.AddScoped<IRepository<Alarm, ApplicationDbContext>, Repository<Alarm, ApplicationDbContext>>();

// Register ViewEisodSd services
// Register your repository and service
builder.Services.AddScoped<IAlarmRepository, AlarmRepository>();
builder.Services.AddScoped<IAlarmService, AlarmService>();
builder.Services.AddScoped<IDataService<Alarm>, AlarmService>();
builder.Services.AddScoped<IAlarmSpecificService, AlarmService>();
builder.Services.AddScoped<IViewEisodSdRepository, ViewEisodSdRepository>();
builder.Services.AddScoped<IViewEisodSdService, ViewEisodSdService>(); 
 
builder.Services.AddScoped<ICsvExportService, CsvExportService>();

builder.Services.AddScoped<IExcelExportService, ExcelExportService>(sp =>
    new ExcelExportService(sp.GetRequiredService<ILogger<ExcelExportService>>()));

builder.Services.AddScoped<IDummyItemService, DummyItemService>(sp =>
    new DummyItemService(
        sp.GetRequiredService<ILogger<DummyItemService>>(),
        sp.GetRequiredService<QueryParserService>(),
        sp.GetRequiredService<AutoMapper.IMapper>()));
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>(); // Přidáno
// Přidání registrace pro IGrindingService a IOperationalService
builder.Services.AddScoped<IGrindingService, GrindingService>();
builder.Services.AddScoped<IOperationalService, OperationalService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// ...


// Add Authentication services
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // The LoginPath must be a page that allows anonymous access, where users can log in.
        // Setting it to a protected page like the root ("/") will cause a redirect loop.
        options.LoginPath = "/login";
    });


#if DEBUG
builder.Services.AddScoped<AuthenticationStateProvider, DevelopmentAuthenticationStateProvider>();
#else
// Production AuthenticationStateProvider configuration would go here
// For example, if using ASP.NET Core Identity:
// builder.Services.AddAuthentication(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme)
// .AddIdentityCookies();
// builder.Services.AddCascadingAuthenticationState();
// Or for Windows Authentication, etc.

// For now, to ensure the app runs in Release without a full setup, 
// we can provide a default anonymous provider.
// Remove this if you have a proper production setup.
builder.Services.AddScoped<AuthenticationStateProvider, RPK_BlazorApp.Services.DefaultAnonymousAuthenticationStateProvider>();
#endif

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy(AppPermissions.AlarmsOverview.Read, policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("permission", AppPermissions.AlarmsOverview.Read));

    options.AddPolicy(AppPermissions.AlarmsOverview.Edit, policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("permission", AppPermissions.AlarmsOverview.Edit));

    options.AddPolicy(AppPermissions.AlarmsOverview.Delete, policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("permission", AppPermissions.AlarmsOverview.Delete));
});


builder.Services.AddScoped<UserPreferenceService>();
builder.Services.AddScoped<QueryParserService>();
builder.Services.AddSingleton<ChangeSetService>();
builder.Services.AddSingleton<NavigationContextService>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up...");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    logger.LogInformation("Request Path: {Path}, Method: {Method}", context.Request.Path, context.Request.Method);
    await next.Invoke();
    logger.LogInformation("Response Status Code: {StatusCode} for Path: {Path}", context.Response.StatusCode, context.Request.Path);
});

app.UseHttpsRedirection();

app.UseStaticFiles(); // Přidáno pro servírování statických souborů
// Antiforgery is important for Blazor Server.
app.UseRouting(); // Explicitní přidání UseRouting pro lepší kontrolu nad pořadím middleware

app.UseAuthentication(); // Přidání UseAuthentication
app.UseAuthorization(); // Přidání UseAuthorization

app.UseAntiforgery();
// Předpokládáme, že App.razor je ve složce Components a má namespace RPK_BlazorApp.Components
// Pokud je App.razor v kořenovém adresáři a má namespace RPK_BlazorApp, použijte MapRazorComponents<RPK_BlazorApp.App>()
app.MapRazorComponents<RPK_BlazorApp.Components.App>()
    .AddInteractiveServerRenderMode();

// Map API controller routes
app.MapControllers();

try
{
    app.Run();
}
finally
{
    logger.LogInformation("Application shutting down...");
}