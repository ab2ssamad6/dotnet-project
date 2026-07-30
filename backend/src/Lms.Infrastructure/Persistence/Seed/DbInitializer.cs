using Lms.Application.Common;
using Lms.Infrastructure.Identity;
using Lms.Infrastructure.Options;
using Lms.Infrastructure.Persistence.Seed.DemoData;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lms.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var context = sp.GetRequiredService<LmsDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var options = sp.GetRequiredService<IOptions<SeedOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<LmsDbContext>>();

        await SeedRolesAsync(roleManager);
        await SeedUserAsync(userManager, options.AdminEmail, options.AdminPassword, "System", "Administrator", Roles.Administrator);
        var trainerUser = await SeedUserAsync(userManager, options.TrainerEmail, options.TrainerPassword, "Tina", "Trainer", Roles.Trainer);
        var studentUser = await SeedUserAsync(userManager, options.StudentEmail, options.StudentPassword, "Sam", "Student", Roles.Student);

        if (!options.SeedSampleData)
        {
            logger.LogInformation("Seeding complete (sample data disabled).");
            return;
        }

        await DemoDataSeeder.SeedAsync(context, userManager, trainerUser, studentUser, logger, ct);
        logger.LogInformation("Seeding complete (with demo data).");
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    internal static async Task<ApplicationUser> SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string firstName, string lastName, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
            return user;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            throw new InvalidOperationException(Describe($"create user {email}", created));

        var assigned = await userManager.AddToRoleAsync(user, role);
        if (!assigned.Succeeded)
            throw new InvalidOperationException(Describe($"add user {email} to role {role}", assigned));

        return user;
    }

    private static string Describe(string action, IdentityResult result) =>
        $"Failed to {action}: {string.Join("; ", result.Errors.Select(e => e.Description))}";
}
