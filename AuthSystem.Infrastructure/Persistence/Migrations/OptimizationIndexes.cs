using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthSystem.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migración para añadir índices de optimización a las tablas más consultadas
    /// </summary>
    public partial class OptimizationIndexes : Migration
    {
        /// <summary>
        /// Método que se ejecuta al aplicar la migración
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Índices para la tabla de usuarios
            migrationBuilder.CreateIndex(
                name: "IX_Users_LastLoginDate",
                table: "Users",
                column: "LastLoginDate");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            // Índices para la tabla de sesiones de usuario
            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_CreatedAt",
                table: "UserSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ExpiresAt",
                table: "UserSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_Status",
                table: "UserSessions",
                column: "Status");

            // Índices para la tabla de intentos de inicio de sesión
            migrationBuilder.CreateIndex(
                name: "IX_UserLoginAttempts_AttemptDate",
                table: "UserLoginAttempts",
                column: "AttemptDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserLoginAttempts_IpAddress",
                table: "UserLoginAttempts",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserLoginAttempts_Username",
                table: "UserLoginAttempts",
                column: "Username");

            // Índices para la tabla de logs de auditoría
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EventDate",
                table: "AuditLogs",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EventType",
                table: "AuditLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            // Índices para la tabla de tokens revocados
            migrationBuilder.CreateIndex(
                name: "IX_RevokedTokens_RevokedAt",
                table: "RevokedTokens",
                column: "RevokedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedTokens_ExpiresAt",
                table: "RevokedTokens",
                column: "ExpiresAt");

            // Índices para la tabla de permisos
            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ModuleId",
                table: "Permissions",
                column: "ModuleId");

            // Índices para la tabla de sucursales
            migrationBuilder.CreateIndex(
                name: "IX_Branches_OrganizationId",
                table: "Branches",
                column: "OrganizationId");

            // Índices para la tabla de relaciones usuario-rol
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            // Índices para la tabla de relaciones rol-permiso
            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            // Índices para la tabla de relaciones usuario-sucursal
            migrationBuilder.CreateIndex(
                name: "IX_UserBranches_UserId_BranchId",
                table: "UserBranches",
                columns: new[] { "UserId", "BranchId" },
                unique: true);
        }

        /// <summary>
        /// Método que se ejecuta al revertir la migración
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar índices de la tabla de usuarios
            migrationBuilder.DropIndex(
                name: "IX_Users_LastLoginDate",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Status",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            // Eliminar índices de la tabla de sesiones de usuario
            migrationBuilder.DropIndex(
                name: "IX_UserSessions_CreatedAt",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_ExpiresAt",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_Status",
                table: "UserSessions");

            // Eliminar índices de la tabla de intentos de inicio de sesión
            migrationBuilder.DropIndex(
                name: "IX_UserLoginAttempts_AttemptDate",
                table: "UserLoginAttempts");

            migrationBuilder.DropIndex(
                name: "IX_UserLoginAttempts_IpAddress",
                table: "UserLoginAttempts");

            migrationBuilder.DropIndex(
                name: "IX_UserLoginAttempts_Username",
                table: "UserLoginAttempts");

            // Eliminar índices de la tabla de logs de auditoría
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EventDate",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EventType",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs");

            // Eliminar índices de la tabla de tokens revocados
            migrationBuilder.DropIndex(
                name: "IX_RevokedTokens_RevokedAt",
                table: "RevokedTokens");

            migrationBuilder.DropIndex(
                name: "IX_RevokedTokens_ExpiresAt",
                table: "RevokedTokens");

            // Eliminar índices de la tabla de permisos
            migrationBuilder.DropIndex(
                name: "IX_Permissions_ModuleId",
                table: "Permissions");

            // Eliminar índices de la tabla de sucursales
            migrationBuilder.DropIndex(
                name: "IX_Branches_OrganizationId",
                table: "Branches");

            // Eliminar índices de la tabla de relaciones usuario-rol
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles");

            // Eliminar índices de la tabla de relaciones rol-permiso
            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions");

            // Eliminar índices de la tabla de relaciones usuario-sucursal
            migrationBuilder.DropIndex(
                name: "IX_UserBranches_UserId_BranchId",
                table: "UserBranches");
        }
    }
}
