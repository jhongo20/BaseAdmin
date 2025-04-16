# Script para aplicar la migración de índices de optimización a la base de datos
param (
    [string]$environment = "Development"
)

Write-Host "Aplicando migración de índices de optimización a la base de datos ($environment)..." -ForegroundColor Cyan

# Establecer la variable de entorno ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT = $environment

# Navegar al directorio del proyecto API
$apiProjectPath = Join-Path $PSScriptRoot "..\AuthSystem.API"
Set-Location $apiProjectPath

# Aplicar la migración
try {
    dotnet ef database update --context ApplicationDbContext
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migración aplicada exitosamente." -ForegroundColor Green
    } else {
        Write-Host "Error al aplicar la migración. Código de salida: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host "Error al aplicar la migración: $_" -ForegroundColor Red
    exit 1
}

# Mostrar información sobre los índices creados
Write-Host "`nÍndices de optimización aplicados:" -ForegroundColor Cyan
Write-Host "- Índices para la tabla Users (LastLoginDate, Status, CreatedAt)" -ForegroundColor White
Write-Host "- Índices para la tabla UserSessions (CreatedAt, ExpiresAt, Status)" -ForegroundColor White
Write-Host "- Índices para la tabla UserLoginAttempts (AttemptDate, IpAddress, Username)" -ForegroundColor White
Write-Host "- Índices para la tabla AuditLogs (EventDate, EventType, EntityName_EntityId)" -ForegroundColor White
Write-Host "- Índices para la tabla RevokedTokens (RevokedAt, ExpiresAt)" -ForegroundColor White
Write-Host "- Índices para relaciones (UserRoles, RolePermissions, UserBranches)" -ForegroundColor White
Write-Host "- Otros índices para mejorar el rendimiento de consultas frecuentes" -ForegroundColor White

Write-Host "`nRecuerde ejecutar consultas de prueba para verificar la mejora en el rendimiento." -ForegroundColor Yellow
