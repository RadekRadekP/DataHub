using DataHub.Platform.Data; // For ApplicationDbContext
using DataHub.Platform.Data.Interceptors; // For ComprehensiveAuditInterceptor
using Grinding.Services;
using Grinding.Services.Interfaces;
using Grinding.Services.Data;
using Grinding.Services.Repositories;
using DataHub.Core.Models;
using DataHub.Host.Mappings;
using DataHub.Core.Models.Interfaces;
using DataHub.Core.Interfaces;
using DataHub.Core.Services;
using DataHub.Core.Permissions;
using Eisod.Services;
using Eisod.Services.Data;
using Eisod.Services.Interfaces;
using Eisod.Services.Repositories;
using DataHub.Host.Services;
using Eisod.Shared.Models;
using Grinding.Shared.Models;
using DataHub.Platform.Repositories; // For AuditLogRepository

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add services for API controllers
builder.Services.AddControllers();

// Add FluentUI services
builder.Services.AddFluentUIComponents();

builder.Services.AddTransient<ComprehensiveAuditInterceptor>();

builder.Services.AddDbContextFactory<DataHub.Platform.Data.ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RestTestConnection"))
           .AddInterceptors(sp.GetRequiredService<ComprehensiveAuditInterceptor>());
});

// Register DataHub.Core.Data.ApplicationDbContext for UserPreferenceService
builder.Services.AddDbContextFactory<DataHub.Core.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RestTestConnection")));

// Register EisodDbContext
builder.Services.AddDbContextFactory<EisodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EisodConnection"),
        sqlOptions => sqlOptions.CommandTimeout(120)));

// Register GrindingDbContext
builder.Services.AddDbContextFactory<GrindingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GrindingConnection") ?? builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(120)));

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);


builder.Services.AddScoped<IRepository<Alarm>, Grinding.Services.Repositories.AlarmRepository>();

// Register Repositories and Services
builder.Services.AddScoped<IAlarmRepository, Grinding.Services.Repositories.AlarmRepository>();
builder.Services.AddScoped<IAlarmService, AlarmService>();
builder.Services.AddScoped<IDataService<Alarm>, AlarmService>();
builder.Services.AddScoped<IAlarmSpecificService, AlarmService>();

builder.Services.AddScoped<IViewEisodSdRepository, Eisod.Services.Repositories.ViewEisodSdRepository>();
builder.Services.AddScoped<IViewEisodSdService, ViewEisodSdService>();
builder.Services.AddScoped<IDataService<ViewEisodSd>, ViewEisodSdService>();
 
builder.Services.AddScoped<ICsvExportService, CsvExportService>();

// Register DummyItemService
builder.Services.AddScoped<Grinding.Services.IDummyItemService, Grinding.Services.DummyItemService>();
builder.Services.AddScoped<IDataService<DummyItem>, Grinding.Services.DummyItemService>();

builder.Services.AddScoped<IExcelExportService, ExcelExportService>(sp =>
    new ExcelExportService(sp.GetRequiredService<ILogger<ExcelExportService>>()));

builder.Services.AddScoped<IDataService<DataHub.Core.Models.DummyItem>, DataHub.Core.Services.DummyItemService>();
builder.Services.AddScoped<DataHub.Core.Services.DummyItemService>();

builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>(); // From DataHub.Platform.Repositories
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

builder.Services.AddScoped<IGrindingService, GrindingService>();
builder.Services.AddScoped<IOperationalService, OperationalService>();


// Add Authentication services
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });

#if DEBUG
builder.Services.AddScoped<DevelopmentAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<DevelopmentAuthenticationStateProvider>());
#else
builder.Services.AddScoped<AuthenticationStateProvider, DefaultAnonymousAuthenticationStateProvider>();
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
builder.Services.AddSingleton<DataHub.Core.Services.ChangeSetService>();
builder.Services.AddSingleton<DataHub.Core.Services.NavigationContextService>();

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

app.UseStaticFiles(); 
app.UseRouting(); 

app.UseAuthentication(); 
app.UseAuthorization(); 

app.UseAntiforgery();

app.MapRazorComponents<DataHub.Host.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(Eisod.Pages._Imports).Assembly,
        typeof(Grinding.Pages._Imports).Assembly
    );

app.MapControllers();

try
{
    app.Run();
}
finally
{
    logger.LogInformation("Application shutting down...");
}