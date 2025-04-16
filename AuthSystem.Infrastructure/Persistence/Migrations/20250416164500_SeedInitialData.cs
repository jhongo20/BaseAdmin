using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthSystem.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migración para agregar datos iniciales al sistema
    /// </summary>
    public partial class SeedInitialData : Migration
    {
        /// <summary>
        /// Método para aplicar la migración
        /// </summary>
        /// <param name="migrationBuilder">Constructor de migraciones</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Constantes para IDs
            var adminRoleId = Guid.NewGuid();
            var userRoleId = Guid.NewGuid();
            var adminUserId = Guid.NewGuid();
            
            // IDs de módulos
            var dashboardModuleId = Guid.NewGuid();
            var usersModuleId = Guid.NewGuid();
            var rolesModuleId = Guid.NewGuid();
            var permissionsModuleId = Guid.NewGuid();
            var organizationsModuleId = Guid.NewGuid();
            var branchesModuleId = Guid.NewGuid();
            var auditLogsModuleId = Guid.NewGuid();
            
            // IDs de permisos para Dashboard
            var viewDashboardPermissionId = Guid.NewGuid();
            
            // IDs de permisos para Usuarios
            var viewUsersPermissionId = Guid.NewGuid();
            var createUserPermissionId = Guid.NewGuid();
            var editUserPermissionId = Guid.NewGuid();
            var deleteUserPermissionId = Guid.NewGuid();
            
            // IDs de permisos para Roles
            var viewRolesPermissionId = Guid.NewGuid();
            var createRolePermissionId = Guid.NewGuid();
            var editRolePermissionId = Guid.NewGuid();
            var deleteRolePermissionId = Guid.NewGuid();
            
            // IDs de permisos para Permisos
            var viewPermissionsPermissionId = Guid.NewGuid();
            var createPermissionPermissionId = Guid.NewGuid();
            var editPermissionPermissionId = Guid.NewGuid();
            var deletePermissionPermissionId = Guid.NewGuid();
            
            // IDs de permisos para Organizaciones
            var viewOrganizationsPermissionId = Guid.NewGuid();
            var createOrganizationPermissionId = Guid.NewGuid();
            var editOrganizationPermissionId = Guid.NewGuid();
            var deleteOrganizationPermissionId = Guid.NewGuid();
            
            // IDs de permisos para Sucursales
            var viewBranchesPermissionId = Guid.NewGuid();
            var createBranchPermissionId = Guid.NewGuid();
            var editBranchPermissionId = Guid.NewGuid();
            var deleteBranchPermissionId = Guid.NewGuid();
            
            // IDs de permisos para Logs de Auditoría
            var viewAuditLogsPermissionId = Guid.NewGuid();

            // Fecha actual para los campos de creación
            var now = DateTime.UtcNow;
            
            // 1. Agregar Módulos
            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Name", "Description", "Route", "Icon", "DisplayOrder", "ParentId", "IsEnabled", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { dashboardModuleId, "Dashboard", "Panel principal del sistema", "/dashboard", "dashboard", 1, null, true, now, "System" },
                    { usersModuleId, "Usuarios", "Gestión de usuarios del sistema", "/users", "people", 2, null, true, now, "System" },
                    { rolesModuleId, "Roles", "Gestión de roles del sistema", "/roles", "assignment", 3, null, true, now, "System" },
                    { permissionsModuleId, "Permisos", "Gestión de permisos del sistema", "/permissions", "lock", 4, null, true, now, "System" },
                    { organizationsModuleId, "Organizaciones", "Gestión de organizaciones", "/organizations", "business", 5, null, true, now, "System" },
                    { branchesModuleId, "Sucursales", "Gestión de sucursales", "/branches", "store", 6, null, true, now, "System" },
                    { auditLogsModuleId, "Logs de Auditoría", "Visualización de logs de auditoría", "/audit-logs", "history", 7, null, true, now, "System" }
                });

            // 2. Agregar Permisos
            // 2.1 Permisos de Dashboard
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Type", "ModuleId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[]
                { viewDashboardPermissionId, "Ver Dashboard", "Permiso para ver el dashboard", "View", dashboardModuleId, true, now, "System" });

            // 2.2 Permisos de Usuarios
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Type", "ModuleId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { viewUsersPermissionId, "Ver Usuarios", "Permiso para ver usuarios", "View", usersModuleId, true, now, "System" },
                    { createUserPermissionId, "Crear Usuario", "Permiso para crear usuarios", "Create", usersModuleId, true, now, "System" },
                    { editUserPermissionId, "Editar Usuario", "Permiso para editar usuarios", "Edit", usersModuleId, true, now, "System" },
                    { deleteUserPermissionId, "Eliminar Usuario", "Permiso para eliminar usuarios", "Delete", usersModuleId, true, now, "System" }
                });

            // 2.3 Permisos de Roles
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Type", "ModuleId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { viewRolesPermissionId, "Ver Roles", "Permiso para ver roles", "View", rolesModuleId, true, now, "System" },
                    { createRolePermissionId, "Crear Rol", "Permiso para crear roles", "Create", rolesModuleId, true, now, "System" },
                    { editRolePermissionId, "Editar Rol", "Permiso para editar roles", "Edit", rolesModuleId, true, now, "System" },
                    { deleteRolePermissionId, "Eliminar Rol", "Permiso para eliminar roles", "Delete", rolesModuleId, true, now, "System" }
                });

            // 2.4 Permisos de Permisos
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Type", "ModuleId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { viewPermissionsPermissionId, "Ver Permisos", "Permiso para ver permisos", "View", permissionsModuleId, true, now, "System" },
                    { createPermissionPermissionId, "Crear Permiso", "Permiso para crear permisos", "Create", permissionsModuleId, true, now, "System" },
                    { editPermissionPermissionId, "Editar Permiso", "Permiso para editar permisos", "Edit", permissionsModuleId, true, now, "System" },
                    { deletePermissionPermissionId, "Eliminar Permiso", "Permiso para eliminar permisos", "Delete", permissionsModuleId, true, now, "System" }
                });

            // 2.5 Permisos de Organizaciones
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Type", "ModuleId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { viewOrganizationsPermissionId, "Ver Organizaciones", "Permiso para ver organizaciones", "View", organizationsModuleId, true, now, "System" },
                    { createOrganizationPermissionId, "Crear Organización", "Permiso para crear organizaciones", "Create", organizationsModuleId, true, now, "System" },
                    { editOrganizationPermissionId, "Editar Organización", "Permiso para editar organizaciones", "Edit", organizationsModuleId, true, now, "System" },
                    { deleteOrganizationPermissionId, "Eliminar Organización", "Permiso para eliminar organizaciones", "Delete", organizationsModuleId, true, now, "System" }
                });

            // 2.6 Permisos de Sucursales
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Type", "ModuleId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { viewBranchesPermissionId, "Ver Sucursales", "Permiso para ver sucursales", "View", branchesModuleId, true, now, "System" },
                    { createBranchPermissionId, "Crear Sucursal", "Permiso para crear sucursales", "Create", branchesModuleId, true, now, "System" },
                    { editBranchPermissionId, "Editar Sucursal", "Permiso para editar sucursales", "Edit", branchesModuleId, true, now, "System" },
                    { deleteBranchPermissionId, "Eliminar Sucursal", "Permiso para eliminar sucursales", "Delete", branchesModuleId, true, now, "System" }
                });

            // 2.7 Permisos de Logs de Auditoría
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Type", "ModuleId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[]
                { viewAuditLogsPermissionId, "Ver Logs de Auditoría", "Permiso para ver logs de auditoría", "View", auditLogsModuleId, true, now, "System" });

            // 3. Agregar Roles
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name", "Description", "OrganizationId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { adminRoleId, "Administrador", "Rol con acceso completo al sistema", null, true, now, "System" },
                    { userRoleId, "Usuario", "Rol con acceso limitado al sistema", null, true, now, "System" }
                });

            // 4. Asignar Permisos a Roles
            // 4.1 Asignar todos los permisos al rol Administrador
            var permissionIds = new Guid[]
            {
                viewDashboardPermissionId,
                viewUsersPermissionId, createUserPermissionId, editUserPermissionId, deleteUserPermissionId,
                viewRolesPermissionId, createRolePermissionId, editRolePermissionId, deleteRolePermissionId,
                viewPermissionsPermissionId, createPermissionPermissionId, editPermissionPermissionId, deletePermissionPermissionId,
                viewOrganizationsPermissionId, createOrganizationPermissionId, editOrganizationPermissionId, deleteOrganizationPermissionId,
                viewBranchesPermissionId, createBranchPermissionId, editBranchPermissionId, deleteBranchPermissionId,
                viewAuditLogsPermissionId
            };

            foreach (var permissionId in permissionIds)
            {
                migrationBuilder.InsertData(
                    table: "RolePermissions",
                    columns: new[] { "Id", "RoleId", "PermissionId", "IsActive", "CreatedAt", "CreatedBy" },
                    values: new object[] { Guid.NewGuid(), adminRoleId, permissionId, true, now, "System" });
            }

            // 4.2 Asignar permisos básicos al rol Usuario
            var userPermissionIds = new Guid[]
            {
                viewDashboardPermissionId,
                viewUsersPermissionId,
                viewRolesPermissionId,
                viewOrganizationsPermissionId,
                viewBranchesPermissionId
            };

            foreach (var permissionId in userPermissionIds)
            {
                migrationBuilder.InsertData(
                    table: "RolePermissions",
                    columns: new[] { "Id", "RoleId", "PermissionId", "IsActive", "CreatedAt", "CreatedBy" },
                    values: new object[] { Guid.NewGuid(), userRoleId, permissionId, true, now, "System" });
            }

            // 5. Crear Usuario Administrador
            // Nota: En un entorno real, este hash debería generarse con BCrypt o similar
            var adminPasswordHash = "$2a$11$Ht5gVNBXEwHXGsnQTlBMUe7KJkdG1TdvYAp/xM3GAJfzIFCf0tKLa"; // Hash para "Admin123!"
            
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Username", "Email", "PasswordHash", "FullName", "UserType", "IsActive", "IsEmailVerified", "CreatedAt", "CreatedBy" },
                values: new object[]
                { adminUserId, "admin", "admin@system.com", adminPasswordHash, "Administrador del Sistema", "Local", true, true, now, "System" });

            // 6. Asignar Rol Administrador al Usuario Administrador
            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "UserId", "RoleId", "IsActive", "CreatedAt", "CreatedBy" },
                values: new object[] { Guid.NewGuid(), adminUserId, adminRoleId, true, now, "System" });
        }

        /// <summary>
        /// Método para revertir la migración
        /// </summary>
        /// <param name="migrationBuilder">Constructor de migraciones</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar usuario administrador
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Username",
                keyValue: "admin");

            // Eliminar roles
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Name",
                keyValue: "Administrador");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Name",
                keyValue: "Usuario");

            // Eliminar permisos por módulo
            var moduleNames = new[] { "Dashboard", "Usuarios", "Roles", "Permisos", "Organizaciones", "Sucursales", "Logs de Auditoría" };
            foreach (var moduleName in moduleNames)
            {
                migrationBuilder.Sql($"DELETE FROM Permissions WHERE ModuleId IN (SELECT Id FROM Modules WHERE Name = '{moduleName}')");
            }

            // Eliminar módulos
            foreach (var moduleName in moduleNames)
            {
                migrationBuilder.DeleteData(
                    table: "Modules",
                    keyColumn: "Name",
                    keyValue: moduleName);
            }
        }
    }
}
