using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de auditoría
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Registra una acción de auditoría
        /// </summary>
        /// <param name="userId">ID del usuario que realiza la acción</param>
        /// <param name="actionType">Tipo de acción</param>
        /// <param name="entityName">Nombre de la entidad afectada</param>
        /// <param name="entityId">ID de la entidad afectada (opcional)</param>
        /// <param name="description">Descripción de la acción</param>
        /// <param name="oldValues">Valores antiguos (opcional, para acciones de actualización)</param>
        /// <param name="newValues">Valores nuevos (opcional, para acciones de creación/actualización)</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="moduleName">Nombre del módulo (opcional)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="branchId">ID de la sucursal (opcional)</param>
        /// <param name="severity">Nivel de severidad (opcional)</param>
        /// <param name="isSuccessful">Indica si la acción fue exitosa</param>
        /// <param name="errorMessage">Mensaje de error (si la acción falló)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Registro de auditoría creado</returns>
        Task<AuditLog> LogActionAsync(
            Guid userId,
            AuditActionType actionType,
            string entityName,
            string entityId = null,
            string description = null,
            string oldValues = null,
            string newValues = null,
            string ipAddress = null,
            string userAgent = null,
            string moduleName = null,
            Guid? organizationId = null,
            Guid? branchId = null,
            string severity = "Information",
            bool isSuccessful = true,
            string errorMessage = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra un intento de inicio de sesión
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="isSuccessful">Indica si el inicio de sesión fue exitoso</param>
        /// <param name="userId">ID del usuario (si el inicio de sesión fue exitoso)</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="errorMessage">Mensaje de error (si el inicio de sesión falló)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Registro de auditoría creado</returns>
        Task<AuditLog> LogLoginAttemptAsync(
            string username,
            bool isSuccessful,
            Guid? userId = null,
            string ipAddress = null,
            string userAgent = null,
            string errorMessage = null,
            Guid? organizationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra un cierre de sesión
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="sessionId">ID de la sesión</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Registro de auditoría creado</returns>
        Task<AuditLog> LogLogoutAsync(
            Guid userId,
            Guid sessionId,
            string ipAddress = null,
            string userAgent = null,
            Guid? organizationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra un cambio de contraseña
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="isSuccessful">Indica si el cambio de contraseña fue exitoso</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="errorMessage">Mensaje de error (si el cambio de contraseña falló)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Registro de auditoría creado</returns>
        Task<AuditLog> LogPasswordChangeAsync(
            Guid userId,
            bool isSuccessful,
            string ipAddress = null,
            string userAgent = null,
            string errorMessage = null,
            Guid? organizationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra un cambio de permisos
        /// </summary>
        /// <param name="userId">ID del usuario que realiza la acción</param>
        /// <param name="targetRoleId">ID del rol afectado</param>
        /// <param name="targetRoleName">Nombre del rol afectado</param>
        /// <param name="oldPermissions">Permisos antiguos</param>
        /// <param name="newPermissions">Permisos nuevos</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Registro de auditoría creado</returns>
        Task<AuditLog> LogPermissionChangeAsync(
            Guid userId,
            Guid targetRoleId,
            string targetRoleName,
            string oldPermissions,
            string newPermissions,
            string ipAddress = null,
            string userAgent = null,
            Guid? organizationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra un bloqueo de cuenta
        /// </summary>
        /// <param name="adminUserId">ID del usuario administrador que realiza la acción</param>
        /// <param name="targetUserId">ID del usuario afectado</param>
        /// <param name="targetUsername">Nombre de usuario afectado</param>
        /// <param name="reason">Razón del bloqueo</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Registro de auditoría creado</returns>
        Task<AuditLog> LogAccountLockAsync(
            Guid adminUserId,
            Guid targetUserId,
            string targetUsername,
            string reason,
            string ipAddress = null,
            string userAgent = null,
            Guid? organizationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra un desbloqueo de cuenta
        /// </summary>
        /// <param name="adminUserId">ID del usuario administrador que realiza la acción</param>
        /// <param name="targetUserId">ID del usuario afectado</param>
        /// <param name="targetUsername">Nombre de usuario afectado</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Registro de auditoría creado</returns>
        Task<AuditLog> LogAccountUnlockAsync(
            Guid adminUserId,
            Guid targetUserId,
            string targetUsername,
            string ipAddress = null,
            string userAgent = null,
            Guid? organizationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de logs de auditoría del usuario</returns>
        Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetAuditLogsByUserAsync(
            Guid userId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por tipo de acción
        /// </summary>
        /// <param name="actionType">Tipo de acción</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de logs de auditoría del tipo especificado</returns>
        Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetAuditLogsByActionTypeAsync(
            AuditActionType actionType,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca logs de auditoría con criterios avanzados
        /// </summary>
        /// <param name="userId">ID del usuario (opcional)</param>
        /// <param name="actionType">Tipo de acción (opcional)</param>
        /// <param name="entityName">Nombre de la entidad (opcional)</param>
        /// <param name="startDate">Fecha de inicio (opcional)</param>
        /// <param name="endDate">Fecha de fin (opcional)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="branchId">ID de la sucursal (opcional)</param>
        /// <param name="moduleName">Nombre del módulo (opcional)</param>
        /// <param name="severity">Nivel de severidad (opcional)</param>
        /// <param name="isSuccessful">Indica si la acción fue exitosa (opcional)</param>
        /// <param name="ipAddress">Dirección IP (opcional)</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de logs de auditoría que cumplen los criterios</returns>
        Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> SearchAuditLogsAsync(
            Guid? userId = null,
            AuditActionType? actionType = null,
            string entityName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Guid? organizationId = null,
            Guid? branchId = null,
            string moduleName = null,
            string severity = null,
            bool? isSuccessful = null,
            string ipAddress = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un informe de auditoría
        /// </summary>
        /// <param name="startDate">Fecha de inicio</param>
        /// <param name="endDate">Fecha de fin</param>
        /// <param name="userId">ID del usuario (opcional)</param>
        /// <param name="actionType">Tipo de acción (opcional)</param>
        /// <param name="entityName">Nombre de la entidad (opcional)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="branchId">ID de la sucursal (opcional)</param>
        /// <param name="moduleName">Nombre del módulo (opcional)</param>
        /// <param name="severity">Nivel de severidad (opcional)</param>
        /// <param name="isSuccessful">Indica si la acción fue exitosa (opcional)</param>
        /// <param name="reportFormat">Formato del informe (CSV, PDF, Excel)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Ruta del archivo de informe generado</returns>
        Task<string> GenerateAuditReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? userId = null,
            AuditActionType? actionType = null,
            string entityName = null,
            Guid? organizationId = null,
            Guid? branchId = null,
            string moduleName = null,
            string severity = null,
            bool? isSuccessful = null,
            string reportFormat = "CSV",
            CancellationToken cancellationToken = default);
    }
}
