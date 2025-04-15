# Arquitectura del Sistema de Autenticación (AuthSystem)

## Introducción

AuthSystem es una aplicación de gestión de autenticación y autorización desarrollada con .NET 8.0 siguiendo los principios de Clean Architecture. Este documento describe la arquitectura del sistema, sus componentes principales y las decisiones de diseño que se han tomado.

## Visión General de la Arquitectura

El proyecto sigue una arquitectura en capas basada en los principios de Clean Architecture (también conocida como Arquitectura Hexagonal o Arquitectura de Cebolla), lo que permite una clara separación de responsabilidades y facilita el mantenimiento, las pruebas y la evolución del sistema.

![Diagrama de Clean Architecture](https://miro.medium.com/v2/resize:fit:720/format:webp/1*0R0r00uF1RyRFxkxo3HVDg.png)

## Estructura del Proyecto

El sistema está organizado en los siguientes proyectos:

### 1. AuthSystem.Domain

Contiene las entidades del negocio, interfaces y contratos. Esta capa es independiente de cualquier tecnología o framework externo.

**Componentes principales:**
- **Entidades**: User, Role, Permission, Module, Organization, Branch, AuditLog, UserSession
- **Interfaces de Repositorios**: IRepository, IUserRepository, IRoleRepository, etc.
- **Interfaces de Servicios**: IAuthService, IEmailService, etc.
- **Excepciones de Dominio**: ValidationException, NotFoundException, etc.
- **Enumeraciones y Constantes**: UserStatus, PermissionType, etc.

### 2. AuthSystem.Application

Contiene la lógica de aplicación, DTOs, servicios y características. Implementa los casos de uso del sistema.

**Componentes principales:**
- **Features (Características)**: Organizadas por funcionalidad (Authentication, Organizations, Permissions, Roles, Users)
- **Patrón CQRS**: Commands y Queries separados
- **DTOs**: Objetos de transferencia de datos
- **Validadores**: Implementados con FluentValidation
- **Mapeos**: Configuraciones de AutoMapper
- **Comportamientos**: Validación, Logging, Performance, etc.

### 3. AuthSystem.Infrastructure

Implementa las interfaces del dominio y proporciona servicios técnicos.

**Componentes principales:**
- **Persistencia**: 
  - ApplicationDbContext
  - Repositorios
  - UnitOfWork
  - Configuraciones de Entidades
- **Autenticación**: 
  - AuthService (LDAP, JWT)
  - TokenService
- **Servicios Externos**: 
  - EmailService
  - CacheService
  - LoggingService

### 4. AuthSystem.API

Expone los servicios como API REST.

**Componentes principales:**
- **Controladores**: Organizados por versiones (v1, v2)
- **Middleware**: ErrorHandling, Authentication, etc.
- **Configuración**: Swagger, CORS, etc.
- **Filters**: ActionFilters, AuthorizationFilters, etc.

### 5. AuthSystem.UnitTests y AuthSystem.IntegrationTests

Contienen pruebas unitarias e integración para asegurar la calidad del código.

## Patrones y Principios de Diseño

El sistema implementa varios patrones y principios de diseño:

1. **Patrón Repositorio**: Abstrae el acceso a datos
2. **Patrón Mediator**: Implementado con MediatR para desacoplar los componentes
3. **Patrón CQRS**: Separación de comandos y consultas
4. **Inyección de Dependencias**: Para desacoplar componentes
5. **Principio de Responsabilidad Única (SRP)**: Cada clase tiene una única responsabilidad
6. **Principio de Inversión de Dependencias (DIP)**: Dependencias hacia abstracciones, no implementaciones
7. **Principio de Segregación de Interfaces (ISP)**: Interfaces específicas y cohesivas

## Flujo de Autenticación

El sistema soporta múltiples mecanismos de autenticación:

1. **Autenticación Local**:
   - Almacenamiento seguro de contraseñas con BCrypt
   - Generación de tokens JWT para sesiones

2. **Autenticación LDAP/Active Directory**:
   - Integración con directorios corporativos
   - Mapeo de grupos a roles internos

3. **Gestión de Sesiones**:
   - Seguimiento de sesiones activas
   - Capacidad de invalidar sesiones

## Gestión de Permisos

El sistema implementa un modelo de control de acceso basado en roles (RBAC) con las siguientes características:

1. **Roles**: Agrupaciones de permisos
2. **Permisos**: Acciones específicas que pueden realizarse
3. **Módulos**: Áreas funcionales del sistema
4. **Organizaciones y Sucursales**: Estructura jerárquica para multi-tenancy

## Tecnologías Utilizadas

- **.NET 8.0**: Framework base
- **Entity Framework Core 8.0**: ORM para acceso a datos
- **MediatR 11.1.0**: Implementación del patrón Mediator
- **AutoMapper 12.0.1**: Mapeo de objetos
- **FluentValidation 11.8.0**: Validación de datos
- **JWT Authentication**: Para la gestión de tokens
- **Serilog**: Para logging estructurado
- **xUnit**: Para pruebas automatizadas
- **Moq**: Para mocking en pruebas
- **Swagger/OpenAPI**: Documentación de API

## Consideraciones de Seguridad

- Almacenamiento seguro de contraseñas con hashing
- Protección contra ataques CSRF
- Implementación de rate limiting
- Validación de entrada en todos los endpoints
- Logging de actividades sensibles
- Políticas de contraseñas configurables
- Bloqueo de cuentas tras intentos fallidos

## Escalabilidad y Rendimiento

- Implementación de caché distribuida con Redis
- Optimización de consultas con proyecciones
- Paginación en endpoints que devuelven grandes conjuntos de datos
- Políticas de retry con Polly para operaciones transitorias

## Conclusión

La arquitectura de AuthSystem está diseñada para ser robusta, mantenible y extensible. La clara separación de responsabilidades permite que el sistema evolucione con el tiempo sin acumular deuda técnica significativa.

Los principios de Clean Architecture garantizan que las reglas de negocio estén aisladas de los detalles de implementación, lo que facilita la adaptación a cambios en los requisitos o en las tecnologías subyacentes.
