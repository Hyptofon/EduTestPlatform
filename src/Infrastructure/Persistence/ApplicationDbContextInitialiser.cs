using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager)
{
    public async Task InitialiseAsync()
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while initialising the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] 
        { 
            ApplicationRole.SuperAdmin,
            ApplicationRole.OrganizationAdmin, 
            ApplicationRole.Teacher, 
            ApplicationRole.Student 
        };

        foreach (var roleName in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        // Seed SuperAdmin
        var superAdminEmail = "superadmin@edutestplatform.com";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
        
        if (superAdminUser == null)
        {
            superAdminUser = ApplicationUser.Create(
                superAdminEmail, 
                "Super", 
                "Admin", 
                "superadmin");
            
            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, ApplicationRole.SuperAdmin);
                logger.LogInformation("SuperAdmin user created successfully");
            }
        }

        // Seed Organization Admin (for testing)
        var orgAdminEmail = "orgadmin@edutestplatform.com";
        var orgAdminUser = await userManager.FindByEmailAsync(orgAdminEmail);
        
        if (orgAdminUser == null)
        {
            orgAdminUser = ApplicationUser.Create(
                orgAdminEmail, 
                "Organization", 
                "Admin", 
                "orgadmin");
            
            var result = await userManager.CreateAsync(orgAdminUser, "OrgAdmin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(orgAdminUser, ApplicationRole.OrganizationAdmin);
                logger.LogInformation("Organization Admin user created successfully");
            }
        }

        // Seed Teacher (for testing)
        var teacherEmail = "teacher@edutestplatform.com";
        var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
        
        if (teacherUser == null)
        {
            teacherUser = ApplicationUser.Create(
                teacherEmail, 
                "Test", 
                "Teacher", 
                "teacher");
            
            var result = await userManager.CreateAsync(teacherUser, "Teacher@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(teacherUser, ApplicationRole.Teacher);
                logger.LogInformation("Teacher user created successfully");
            }
        }

        // Seed Student (for testing)
        var studentEmail = "student@edutestplatform.com";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);
        
        if (studentUser == null)
        {
            studentUser = ApplicationUser.Create(
                studentEmail, 
                "Test", 
                "Student", 
                "student");
            
            var result = await userManager.CreateAsync(studentUser, "Student@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, ApplicationRole.Student);
                logger.LogInformation("Student user created successfully");
            }
        }
    }
}