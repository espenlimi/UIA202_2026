using Heimevernet.Web.DataAccess;
using Heimevernet.Web.Middleware;
using Heimevernet.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var connectionString = builder.Configuration.GetConnectionString("heimevernetdb")
    ?? throw new InvalidOperationException(
        "The 'heimevernetdb' connection string was not configured. Run the web app through Aspire.");

builder.Services.AddDbContext<HeimevernetDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddSingleton<IResourceDbConnectionFactory, MySqlResourceDbConnectionFactory>();
builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();

var identityBuilder = builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 10;
    })
    .AddDefaultTokenProviders();

var dataAccessProvider = builder.Configuration["DataAccess:Provider"] ?? "EntityFramework";
switch (dataAccessProvider.ToUpperInvariant())
{
    case "ENTITYFRAMEWORK":
        identityBuilder.AddEntityFrameworkStores<HeimevernetDbContext>();
        builder.Services.AddScoped<IResourceRepository, EfResourceRepository>();
        break;
    case "DAPPER":
        builder.Services.AddScoped<IIdentityUserRepository, DapperIdentityUserRepository>();
        builder.Services.AddScoped<IUserStore<IdentityUser>, RepositoryIdentityUserStore>();
        builder.Services.AddScoped<IRoleStore<IdentityRole>, RepositoryIdentityRoleStore>();
        builder.Services.AddScoped<IResourceRepository, DapperResourceRepository>();
        break;
    case "ADONET":
        builder.Services.AddScoped<IIdentityUserRepository, AdoNetIdentityUserRepository>();
        builder.Services.AddScoped<IUserStore<IdentityUser>, RepositoryIdentityUserStore>();
        builder.Services.AddScoped<IRoleStore<IdentityRole>, RepositoryIdentityRoleStore>();
        builder.Services.AddScoped<IResourceRepository, AdoNetResourceRepository>();
        break;
    default:
        throw new InvalidOperationException(
            $"The DataAccess:Provider value '{dataAccessProvider}' is invalid. Use EntityFramework, Dapper, or AdoNet.");
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HeimevernetDbContext>();
    dbContext.Database.Migrate();
    ResourceDbSeeder.Seed(dbContext);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedRolesAsync(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    await IdentitySeeder.SeedDevelopmentAdminAsync(userManager);
    await IdentitySeeder.SeedDevelopmentUserAsync(userManager);

}

// Configure the HTTP request pipeline.
app.UseMiddleware<ContentSecurityPolicyMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHttpsRedirection();
    app.UseHsts();
}
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
