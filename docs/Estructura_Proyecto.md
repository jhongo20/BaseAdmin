# Estructura del Proyecto AuthSystem

## Organización de Carpetas y Archivos

Este documento detalla la estructura de carpetas y archivos del proyecto AuthSystem, explicando el propósito de cada componente y cómo se relacionan entre sí.

## Estructura General

```
AuthSystem/
├── AuthSystem.API/               # Capa de presentación (API REST)
├── AuthSystem.Application/       # Capa de aplicación (lógica de negocio)
├── AuthSystem.Domain/            # Capa de dominio (entidades y reglas de negocio)
├── AuthSystem.Infrastructure/    # Capa de infraestructura (implementaciones técnicas)
├── AuthSystem.UnitTests/         # Pruebas unitarias
├── AuthSystem.IntegrationTests/  # Pruebas de integración
├── docs/                         # Documentación del proyecto
└── Directory.Build.props         # Propiedades comunes para todos los proyectos
```

## AuthSystem.Domain

Contiene las entidades del negocio, interfaces y reglas de dominio. Es independiente de cualquier tecnología externa.

```
AuthSystem.Domain/
├── Common/
│   ├── Enums/           # Enumeraciones del dominio
│   ├── Exceptions/      # Excepciones específicas del dominio
│   └── ValueObjects/    # Objetos de valor inmutables
├── Constants/           # Constantes del dominio
├── Entities/            # Entidades principales del dominio
│   ├── BaseEntity.cs    # Clase base para todas las entidades
│   ├── User.cs
│   ├── Role.cs
│   ├── Permission.cs
│   ├── Module.cs
│   ├── Organization.cs
│   ├── Branch.cs
│   ├── AuditLog.cs
│   └── UserSession.cs
├── Events/
│   └── DomainEvents/    # Eventos de dominio
└── Interfaces/          # Contratos e interfaces
    ├── IRepository.cs
    ├── IUnitOfWork.cs
    └── IServices/       # Interfaces para servicios de dominio
```

## AuthSystem.Application

Implementa los casos de uso de la aplicación, orquestando el flujo entre las entidades del dominio y los servicios externos.

```
AuthSystem.Application/
├── Common/
│   ├── Behaviors/       # Comportamientos de pipeline (validación, logging, etc.)
│   ├── Exceptions/      # Excepciones específicas de la aplicación
│   ├── Interfaces/      # Interfaces específicas de la aplicación
│   └── Models/          # Modelos comunes de la aplicación
├── DependencyInjection/ # Configuración de inyección de dependencias
├── DTOs/                # Objetos de transferencia de datos
│   ├── Authentication/
│   ├── Organizations/
│   ├── Permissions/
│   ├── Roles/
│   └── Users/
├── Features/            # Características organizadas por funcionalidad (CQRS)
│   ├── Authentication/
│   │   ├── Commands/    # Comandos (modifican estado)
│   │   └── Queries/     # Consultas (solo lectura)
│   ├── Organizations/
│   ├── Permissions/
│   ├── Roles/
│   └── Users/
└── Mappings/            # Perfiles de AutoMapper
```

## AuthSystem.Infrastructure

Proporciona implementaciones concretas de las interfaces definidas en las capas de dominio y aplicación.

```
AuthSystem.Infrastructure/
├── Authentication/      # Implementación de servicios de autenticación
│   ├── AuthService.cs   # Servicio de autenticación (local y LDAP)
│   └── TokenService.cs  # Servicio de generación y validación de tokens
├── Caching/             # Implementación de servicios de caché
├── DependencyInjection/ # Configuración de inyección de dependencias
├── Email/               # Implementación de servicios de correo electrónico
├── Logging/             # Implementación de servicios de logging
├── Persistence/         # Implementación de persistencia de datos
│   ├── Configurations/  # Configuraciones de Entity Framework
│   ├── Migrations/      # Migraciones de base de datos
│   ├── Repositories/    # Implementaciones de repositorios
│   │   ├── BaseRepository.cs
│   │   ├── UserRepository.cs
│   │   ├── RoleRepository.cs
│   │   ├── PermissionRepository.cs
│   │   ├── UserRoleRepository.cs
│   │   ├── UserBranchRepository.cs
│   │   └── RolePermissionRepository.cs
│   ├── ApplicationDbContext.cs
│   └── UnitOfWork.cs
└── Services/            # Implementación de otros servicios
```

## AuthSystem.API

Expone la funcionalidad de la aplicación como una API REST.

```
AuthSystem.API/
├── Controllers/         # Controladores de API
│   ├── v1/              # Versión 1 de la API
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── RolesController.cs
│   │   ├── PermissionsController.cs
│   │   └── OrganizationsController.cs
│   └── v2/              # Versión 2 de la API (si existe)
├── Filters/             # Filtros de acción y autorización
├── Middleware/          # Middleware personalizado
│   ├── ErrorHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Models/              # Modelos específicos de la API
├── Program.cs           # Punto de entrada de la aplicación
└── appsettings.json     # Configuración de la aplicación
```

## AuthSystem.UnitTests y AuthSystem.IntegrationTests

Contienen pruebas automatizadas para verificar el correcto funcionamiento del sistema.

```
AuthSystem.UnitTests/
├── Application/         # Pruebas para la capa de aplicación
│   ├── Features/
│   │   ├── Authentication/
│   │   ├── Users/
│   │   └── ...
│   └── Mappings/
├── Domain/              # Pruebas para la capa de dominio
└── Mocks/               # Objetos mock para pruebas

AuthSystem.IntegrationTests/
├── API/                 # Pruebas para endpoints de API
├── Infrastructure/      # Pruebas para la capa de infraestructura
└── TestBase/            # Clases base para pruebas de integración
```

## Dependencias entre Proyectos

Las dependencias entre proyectos siguen el principio de dependencia de Clean Architecture, donde las capas externas dependen de las internas, pero no al revés:

1. **AuthSystem.Domain**: No tiene dependencias externas.
2. **AuthSystem.Application**: Depende de AuthSystem.Domain.
3. **AuthSystem.Infrastructure**: Depende de AuthSystem.Application y AuthSystem.Domain.
4. **AuthSystem.API**: Depende de AuthSystem.Infrastructure, AuthSystem.Application y AuthSystem.Domain.
5. **AuthSystem.UnitTests**: Depende de todos los proyectos para probarlos.
6. **AuthSystem.IntegrationTests**: Depende de todos los proyectos para probarlos.

## Versiones de Paquetes

El proyecto utiliza las siguientes versiones de paquetes principales, todas compatibles con .NET 8.0:

- **Microsoft.Extensions.***: 8.0.0/8.0.2
- **Microsoft.EntityFrameworkCore**: 8.0.0
- **AutoMapper**: 12.0.1
- **AutoMapper.Extensions.Microsoft.DependencyInjection**: 12.0.1
- **MediatR**: 11.1.0
- **MediatR.Extensions.Microsoft.DependencyInjection**: 11.1.0
- **FluentValidation**: 11.8.0
- **Serilog**: 3.1.1
- **xUnit**: 2.6.2 (para pruebas)
- **Moq**: 4.20.70 (para pruebas)
