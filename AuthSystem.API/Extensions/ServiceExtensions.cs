using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Repositories;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Infrastructure.Persistence;
using AuthSystem.Infrastructure.Persistence.Repositories;
using AuthSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserBranchRepository, UserBranchRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            
            return services;
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar servicios de la aplicación
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ILdapService, LdapService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAccountLockoutService, AccountLockoutService>();
            services.AddScoped<ICaptchaService, CaptchaService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddSingleton<ISecurityMonitoringService, SecurityMonitoringService>();
            
            return services;
        }
    }
}
