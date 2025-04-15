# AuthSystem

## Sistema de Autenticación y Autorización

AuthSystem es una solución completa para la gestión de autenticación y autorización de usuarios, desarrollada con .NET 8.0 y siguiendo los principios de Clean Architecture. El sistema proporciona un conjunto robusto de funcionalidades para la gestión de usuarios, roles, permisos y organizaciones.

## Características Principales

- **Autenticación Multi-método**: Soporte para autenticación local y LDAP/Active Directory
- **Gestión de Usuarios**: Registro, actualización, eliminación y consulta de usuarios
- **Control de Acceso Basado en Roles (RBAC)**: Asignación de roles y permisos granulares
- **Multi-tenancy**: Soporte para múltiples organizaciones y sucursales
- **Auditoría**: Registro detallado de actividades y cambios en el sistema
- **API RESTful**: Interfaz moderna para integración con otras aplicaciones
- **Documentación API**: Documentación interactiva con Swagger/OpenAPI

## Tecnologías Utilizadas

- **.NET 8.0**: Framework base
- **Entity Framework Core 8.0**: ORM para acceso a datos
- **MediatR 11.1.0**: Implementación del patrón Mediator
- **AutoMapper 12.0.1**: Mapeo de objetos
- **FluentValidation 11.8.0**: Validación de datos
- **JWT Authentication**: Para la gestión de tokens
- **Serilog**: Para logging estructurado
- **xUnit**: Para pruebas automatizadas

## Estructura del Proyecto

El proyecto sigue una arquitectura en capas:

- **AuthSystem.Domain**: Entidades y reglas de negocio
- **AuthSystem.Application**: Lógica de aplicación y casos de uso
- **AuthSystem.Infrastructure**: Implementaciones técnicas
- **AuthSystem.API**: Interfaz de API REST
- **AuthSystem.UnitTests**: Pruebas unitarias
- **AuthSystem.IntegrationTests**: Pruebas de integración

## Documentación

Para más información sobre el proyecto, consulte los siguientes documentos:

- [Arquitectura](./Arquitectura.md): Descripción detallada de la arquitectura del sistema
- [Estructura del Proyecto](./Estructura_Proyecto.md): Organización de carpetas y archivos
- [Guía de Desarrollo](./Guia_Desarrollo.md): Convenciones y flujos de trabajo para desarrolladores

## Requisitos

- .NET SDK 8.0 o superior
- SQL Server (o compatible con Entity Framework Core)
- Visual Studio 2022 o Visual Studio Code (recomendado)

## Instalación

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/jhongo20/BaseAdmin.git
   cd BaseAdmin
   ```

2. Restaurar paquetes NuGet:
   ```bash
   dotnet restore
   ```

3. Configurar la base de datos:
   - Actualizar la cadena de conexión en `appsettings.Development.json`
   - Aplicar migraciones:
     ```bash
     cd AuthSystem.API
     dotnet ef database update
     ```

4. Ejecutar la aplicación:
   ```bash
   dotnet run --project AuthSystem.API
   ```

5. Acceder a la documentación de la API:
   ```
   https://localhost:5197/swagger
   ```

## Contribución

Las contribuciones son bienvenidas. Por favor, lea la [Guía de Desarrollo](./Guia_Desarrollo.md) para conocer nuestras convenciones y flujos de trabajo.

## Licencia

Este proyecto está licenciado bajo [MIT License](LICENSE).
