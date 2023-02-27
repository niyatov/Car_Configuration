using Car_Configuration.Dtoes;
using Car_Configuration.Entities;
using Microsoft.AspNetCore.Identity;

public class Seed
{
    public static async Task InitializeRolesAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Seed>();

        var roles = config.GetSection("Identity:IdentityServer:Roles").Get<string[]>();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var newRole = new UserRole();
                newRole.Name = role;

                var result = await roleManager.CreateAsync(newRole);

                if (!result.Succeeded)
                {
                    logger.LogWarning($"Seed role {role} failed. Error: {result.Errors.First().Description}");
                }
                else
                {
                    logger.LogInformation("Seed role {0} succeeded.", role);
                }
            }
            else
            {
                logger.LogInformation($"Role {role} already exists.");
            }
        }

        logger.LogInformation("Role seed is finished.");
    }

    public static async Task InitializeUserAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Seed>();

        var users = config.GetSection("Identity:IdentityServer:TestUsers").Get<TestUser[]>();

        foreach (var user in users)
        {
            var newUser = new User();
            newUser.UserName = user.UserName;

            var result = await userManager.CreateAsync(newUser, user.Password);

            if (result.Succeeded)
            {
                var roleResult = await userManager.AddToRolesAsync(newUser, user.Roles);

                if (roleResult.Succeeded)
                {
                    logger.LogInformation($"{user.UserName}ga rollar qo'shildi.");
                }
                else
                {
                    logger.LogInformation($"{user.UserName}ga rollar qo'shilmadi.");
                }
            }
            else
            {
                logger.LogInformation($"{user.UserName} user yaratilmadi");
            }
        }
    }
}