# Implementación de Características de Seguridad

Este documento detalla cómo se han implementado las características de seguridad en el sistema de autenticación, siguiendo la arquitectura de Clean Architecture.

## Tokens de Propósito Específico

### Implementación

Los tokens de propósito específico se implementan en el servicio `JwtService` mediante los siguientes métodos:

1. **Generación de tokens**:
   - `GeneratePurposeTokenAsync`: Método base para generar tokens con propósito específico
   - `GeneratePasswordResetTokenAsync`: Genera tokens para restablecimiento de contraseña
   - `GenerateAccountActivationTokenAsync`: Genera tokens para activación de cuentas

2. **Validación de tokens**:
   - `ValidatePurposeTokenAsync`: Método base para validar tokens con propósito específico
   - `ValidatePasswordResetTokenAsync`: Valida tokens de restablecimiento de contraseña
   - `ValidateAccountActivationTokenAsync`: Valida tokens de activación de cuenta

### Características de seguridad

- Los tokens incluyen un claim de propósito específico (`purpose`)
- Duración limitada (24 horas por defecto)
- Validación de firma, emisor y audiencia
- Validación del ID de usuario asociado
- Almacenamiento del token en la base de datos para validación adicional

## Plantillas de Correo Electrónico

Las plantillas de correo electrónico se implementan como archivos HTML en el directorio `Templates` de la API:

1. **PasswordReset.html**: Plantilla para correos de restablecimiento de contraseña
2. **AccountActivation.html**: Plantilla para correos de activación de cuenta
3. **SecurityAlert.html**: Plantilla para alertas de seguridad

### Características

- Diseño responsivo compatible con la mayoría de clientes de correo
- Variables de plantilla para personalización ({{FullName}}, {{ResetLink}}, etc.)
- Información clara sobre la expiración de los tokens
- Instrucciones detalladas para los usuarios
- Estilo consistente con la identidad visual del sistema

## Auditoría de Acciones

La auditoría de acciones se implementa mediante:

1. **Middleware de Auditoría**: Registra automáticamente las acciones HTTP
2. **Métodos LogAuditAsync**: Implementados en los controladores para registrar acciones específicas

### Información registrada

- Tipo de acción (Create, Read, Update, Delete)
- Entidad afectada y su ID
- Usuario que realizó la acción
- Dirección IP
- User-Agent (información del navegador/dispositivo)
- Fecha y hora
- Endpoint accedido
- Parámetros de consulta
- Severidad de la acción

## Políticas de Contraseñas

Las políticas de contraseñas se implementan mediante validaciones en los modelos:

```csharp
[Required(ErrorMessage = "La nueva contraseña es obligatoria")]
[MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
    ErrorMessage = "La contraseña debe contener al menos una letra minúscula, una letra mayúscula, un número y un carácter especial")]
public string NewPassword { get; set; }
```

### Requisitos

- Mínimo 8 caracteres
- Al menos una letra minúscula
- Al menos una letra mayúscula
- Al menos un número
- Al menos un carácter especial

## Integración con Clean Architecture

La implementación de seguridad sigue los principios de Clean Architecture:

1. **Domain**: Define las interfaces (`IJwtService`, `IEmailService`) y entidades (`AuditLog`, `User`)
2. **Infrastructure**: Implementa los servicios (`JwtService`, `EmailService`)
3. **API**: Utiliza los servicios a través de inyección de dependencias

Esta separación permite:
- Testabilidad de las características de seguridad
- Desacoplamiento entre la lógica de negocio y la implementación técnica
- Facilidad para cambiar implementaciones sin afectar la lógica de negocio

## Mejores Prácticas Implementadas

1. **No revelar información sensible**: Las respuestas de API no revelan si un correo existe o no
2. **Tokens de corta duración**: Los tokens JWT tienen una duración limitada
3. **Refresh tokens**: Permiten renovar la sesión sin requerir credenciales
4. **Auditoría completa**: Todas las acciones sensibles se registran
5. **Notificaciones de seguridad**: Los usuarios reciben alertas sobre cambios en su cuenta
6. **Validación en múltiples capas**: Tanto en el cliente como en el servidor
7. **Sanitización de datos**: Los datos sensibles se sanitizan antes de almacenarse
8. **Cabeceras de seguridad**: Implementación de cabeceras CSP, X-Frame-Options, etc.

## Mantenimiento y Mejoras Futuras

Para mantener y mejorar la seguridad del sistema:

1. **Revisiones periódicas**: Auditar regularmente el código de seguridad
2. **Actualizaciones de dependencias**: Mantener actualizadas las bibliotecas de seguridad
3. **Pruebas de penetración**: Realizar pruebas de seguridad periódicas
4. **Monitoreo de logs**: Implementar alertas para actividades sospechosas
5. **Mejoras potenciales**:
   - Autenticación de dos factores (2FA)
   - Integración con proveedores de identidad externos (OAuth)
   - Análisis de comportamiento para detectar actividades sospechosas
   - Cifrado de datos sensibles en la base de datos
