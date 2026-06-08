using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MMoneyWeb.Web.Components;
using MMoneyWeb.Web.Components.Account;
using MMoneyWeb.Web.Data;
using MMoneyWeb.Web.Services;

Console.WriteLine($"[startup] MMoneyWeb PID={Environment.ProcessId}");

// Coolify pode injetar PORT=3000 por defeito; ASPNETCORE_URLS do Dockerfile (8080) tem prioridade.
var aspnetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (string.IsNullOrWhiteSpace(aspnetUrls))
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{port}");
}

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
}

try
{
    Directory.CreateDirectory(dataProtectionKeysPath);
}
catch (Exception ex)
{
    Console.WriteLine($"[startup] Aviso: nao foi possivel usar '{dataProtectionKeysPath}': {ex.Message}");
    dataProtectionKeysPath = Path.Combine(Path.GetTempPath(), "mmoneyweb-keys");
    Directory.CreateDirectory(dataProtectionKeysPath);
    Console.WriteLine($"[startup] Usando fallback: {dataProtectionKeysPath}");
}
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("MMoneyWeb");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var mmoneyConnectionString = builder.Configuration.GetConnectionString("MMoneyConnection") ?? throw new InvalidOperationException("Connection string 'MMoneyConnection' not found.");
builder.Services.AddDbContextFactory<MMoneyDbContext>(options =>
    options.UseSqlServer(mmoneyConnectionString));

builder.Services.AddScoped<LancamentosViewService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Logger.LogInformation(
    "Iniciando Kestrel em {Urls}",
    Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://+:8080");

if (app.Configuration.GetValue("Database:RunMigrationsOnStartup", false))
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        _ = Task.Run(async () =>
        {
            try
            {
                app.Logger.LogInformation("Aplicando migrations do Identity (background)...");
                await using var scope = app.Services.CreateAsyncScope();
                var identityDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await identityDb.Database.MigrateAsync();
                app.Logger.LogInformation("Migrations do Identity concluídas.");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Falha ao aplicar migrations do Identity. Verifique connection string e acesso ao SQL Server.");
            }
        });
    });
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

var requireHttps = builder.Configuration.GetValue("App:RequireHttps", false);
if (requireHttps)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
