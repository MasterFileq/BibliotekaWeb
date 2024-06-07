using BibliotekaWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()  // Add roles
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed roles and admin users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        await SeedData.Initialize(roleManager, userManager, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();

public static class SeedData
{
    public static async Task Initialize(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ILogger logger)
    {
        string[] roleNames = { "Administrator", "Bibliotekarz", "Czytelnik" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    logger.LogError($"Error creating role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        var users = new (string Email, string Role)[]
        {
            ("a@b.c", "Administrator"),
            ("b@c.d", "Bibliotekarz"),
            ("c@d.e", "Czytelnik")
        };

        var password = "Aa123456!";

        foreach (var userInfo in users)
        {
            var user = await userManager.FindByEmailAsync(userInfo.Email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (createResult.Succeeded)
                {
                    var addToRoleResult = await userManager.AddToRoleAsync(user, userInfo.Role);
                    if (!addToRoleResult.Succeeded)
                    {
                        logger.LogError($"Error adding user {userInfo.Email} to role {userInfo.Role}: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                    }
                    else
                    {
                        logger.LogInformation($"Successfully added user {userInfo.Email} to role {userInfo.Role}");
                    }
                }
                else
                {
                    logger.LogError($"Error creating user {userInfo.Email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"User {userInfo.Email} already exists.");
            }
        }
    }
}
