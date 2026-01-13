using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Users;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.Persistence;

public static class ConfigurePersistenceServices
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(
            configuration.GetConnectionString("DefaultConnection"));
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(
                dataSource,
                builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());
        
        services.AddRepositories();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationalUnitRepository, OrganizationalUnitRepository>();
        services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
        services.AddScoped<IUserOrganizationRepository, UserOrganizationRepository>();
        
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISubjectEnrollmentRepository, SubjectEnrollmentRepository>();
        
        services.AddScoped<ITestRepository, TestRepository>();
        services.AddScoped<ITestSessionRepository, TestSessionRepository>();
    }
}