using System;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Repositories;
using AuthSystem.Infrastructure.Persistence;
using AuthSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthSystem.Infrastructure
{
    /// <summary>
    /// Extensiones para el registro de servicios de infraestructura
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Agrega los servicios de infraestructura al contenedor de dependencias
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios con los servicios de infraestructura agregados</returns>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar el contexto de base de datos
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    });
            });

            // Registrar repositorios
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
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

            // Registrar unidad de trabajo
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
