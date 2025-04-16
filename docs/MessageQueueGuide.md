# Guía Completa del Sistema de Colas para Tareas Pesadas

![Banner Sistema de Colas](https://via.placeholder.com/800x200/4a86e8/ffffff?text=Sistema+de+Colas+para+Tareas+Pesadas)

## Contenido

1. [Introducción](#introducción)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Instalación y Configuración](#instalación-y-configuración)
4. [Casos de Uso](#casos-de-uso)
5. [Guía de Implementación](#guía-de-implementación)
6. [Monitoreo y Mantenimiento](#monitoreo-y-mantenimiento)
7. [Solución de Problemas](#solución-de-problemas)
8. [Preguntas Frecuentes](#preguntas-frecuentes)

---

## Introducción

El sistema de colas para tareas pesadas es una solución diseñada para mejorar el rendimiento y la escalabilidad de la aplicación, permitiendo que operaciones que consumen muchos recursos como el envío de correos electrónicos y la generación de reportes se ejecuten de forma asíncrona.

### Beneficios Clave

- **Mejora de Rendimiento**: Las operaciones pesadas no bloquean los hilos de procesamiento principales
- **Mayor Responsividad**: Los usuarios reciben respuestas inmediatas mientras las tareas se procesan en segundo plano
- **Escalabilidad Horizontal**: Permite distribuir la carga de trabajo entre múltiples servidores
- **Resistencia a Fallos**: Los mensajes persisten incluso si el servidor se reinicia
- **Programación de Tareas**: Capacidad para programar tareas para ejecución futura

---

## Arquitectura del Sistema

El sistema de colas implementa una arquitectura de mensajería basada en el patrón productor/consumidor, utilizando RabbitMQ como broker de mensajes y MassTransit como biblioteca de abstracción.

### Diagrama de Componentes

```
┌─────────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│             │      │             │      │             │      │             │      │             │
│   Cliente   │─────▶│     API     │─────▶│  Servicio   │─────▶│   RabbitMQ  │─────▶│ Consumidor  │
│             │      │             │      │   de Cola   │      │             │      │             │
└─────────────┘      └─────────────┘      └─────────────┘      └─────────────┘      └─────────────┘
                                                                                           │
                                                                                           ▼
                                                                                    ┌─────────────┐
                                                                                    │             │
                                                                                    │Procesamiento│
                                                                                    │             │
                                                                                    └─────────────┘
```

### Flujo de Datos

1. **Cliente**: Realiza una solicitud para enviar un correo o generar un reporte
2. **API**: Recibe la solicitud y la valida
3. **Servicio de Cola**: Prepara el mensaje y lo publica en la cola correspondiente
4. **RabbitMQ**: Almacena el mensaje hasta que un consumidor lo procese
5. **Consumidor**: Recibe el mensaje y ejecuta la tarea correspondiente
6. **Procesamiento**: Se realiza la tarea (envío de correo o generación de reporte)

### Tipos de Colas Implementadas

#### Cola de Correos Electrónicos

Diseñada para manejar el envío asíncrono de correos electrónicos, con soporte para:

- Envío individual y masivo
- Correos con adjuntos
- Programación de envíos
- Priorización de mensajes

#### Cola de Reportes

Especializada en la generación de reportes que pueden consumir muchos recursos:

- Múltiples formatos (PDF, Excel, CSV)
- Notificación automática al completarse
- Almacenamiento configurable
- Parámetros personalizables

---

## Instalación y Configuración

### Requisitos Previos

- .NET 8.0 o superior
- RabbitMQ Server 3.8 o superior
- Acceso a un servidor SMTP para el envío de correos

### Instalación de RabbitMQ

#### Windows

1. Descargar el instalador desde [rabbitmq.com](https://www.rabbitmq.com/download.html)
2. Ejecutar el instalador y seguir las instrucciones
3. Habilitar el plugin de administración:
   ```
   rabbitmq-plugins enable rabbitmq_management
   ```

#### Docker

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### Configuración del Sistema

La configuración se realiza a través del archivo `appsettings.json`:

```json
"MessageQueueSettings": {
  "Host": "localhost",
  "VirtualHost": "/",
  "Username": "guest",
  "Password": "guest",
  "Port": 5672,
  "UseSsl": false,
  "RetryCount": 3,
  "RetryInterval": 1000,
  "PrefetchCount": 16,
  "ConcurrentMessageLimit": 8,
  "QueueNames": {
    "EmailQueue": "auth-system-email-queue",
    "ReportQueue": "auth-system-report-queue"
  }
}
```

#### Parámetros Principales

| Parámetro | Descripción | Valor Recomendado |
|-----------|-------------|-------------------|
| Host | Dirección del servidor RabbitMQ | localhost (desarrollo), IP o dominio (producción) |
| VirtualHost | Host virtual en RabbitMQ | / (por defecto) |
| Username | Usuario para autenticación | guest (desarrollo), usuario personalizado (producción) |
| Password | Contraseña para autenticación | guest (desarrollo), contraseña segura (producción) |
| RetryCount | Número de reintentos para mensajes fallidos | 3-5 |
| PrefetchCount | Mensajes que un consumidor puede procesar a la vez | 16-32 (ajustar según capacidad del servidor) |

---

## Casos de Uso

### Envío de Correos Electrónicos

#### Escenario 1: Notificación a Múltiples Usuarios

Cuando se necesita enviar una notificación a un gran número de usuarios (por ejemplo, cambios en los términos de servicio), el sistema de colas permite:

1. Encolar todos los correos de una vez
2. Procesar los envíos en segundo plano
3. No bloquear la interfaz de usuario durante el proceso

```csharp
// Ejemplo de código para enviar notificaciones masivas
var users = await _userService.GetAllActiveUsersAsync();
var emailMessages = new List<EmailMessage>();

foreach (var user in users)
{
    emailMessages.Add(new EmailMessage
    {
        To = user.Email,
        Subject = "Actualización de Términos de Servicio",
        Body = $"<p>Estimado/a {user.Name},</p><p>Hemos actualizado nuestros términos de servicio...</p>",
        IsHtml = true
    });
}

await _emailQueueService.QueueBulkEmailAsync(emailMessages);
```

#### Escenario 2: Correos Programados

Para campañas de marketing o recordatorios programados:

```csharp
var reminderEmail = new EmailMessage
{
    To = "cliente@ejemplo.com",
    Subject = "Recordatorio de cita",
    Body = "<p>Le recordamos su cita programada para mañana...</p>",
    IsHtml = true,
    ScheduledFor = DateTime.UtcNow.AddDays(1) // Programado para enviarse en 24 horas
};

await _emailQueueService.QueueEmailAsync(reminderEmail);
```

### Generación de Reportes

#### Escenario 1: Reporte Mensual de Actividad

Para generar reportes periódicos que requieren procesamiento intensivo:

```csharp
var parameters = new Dictionary<string, object>
{
    { "Month", DateTime.Now.Month },
    { "Year", DateTime.Now.Year },
    { "DepartmentId", departmentId }
};

var reportMessage = new ReportMessage
{
    ReportType = "MonthlyActivityReport",
    Parameters = parameters,
    OutputFormat = "PDF",
    NotificationEmail = "manager@ejemplo.com",
    Priority = 2 // Prioridad media
};

await _reportQueueService.QueueReportAsync(reportMessage);
```

#### Escenario 2: Exportación de Datos Masivos

Para exportar grandes volúmenes de datos sin bloquear la aplicación:

```csharp
var parameters = new Dictionary<string, object>
{
    { "StartDate", startDate },
    { "EndDate", endDate },
    { "IncludeDetails", true },
    { "FilterCriteria", filterCriteria }
};

await _reportQueueService.QueueReportAsync(
    "DataExport",
    parameters,
    "Excel",
    userEmail);
```

---

## Guía de Implementación

### Integración con Servicios Existentes

Para integrar el sistema de colas con servicios existentes:

1. Inyectar el servicio correspondiente (`IEmailQueueService` o `IReportQueueService`)
2. Reemplazar las llamadas síncronas por llamadas a los métodos de cola
3. Implementar la lógica de notificación cuando sea necesario

### Ejemplo: Migración de Envío de Correos

**Antes:**

```csharp
// Método síncrono que bloquea el hilo
public async Task SendWelcomeEmailAsync(User user)
{
    await _emailService.SendEmailAsync(
        user.Email,
        "Bienvenido a nuestra plataforma",
        $"<p>Hola {user.Name},</p><p>Gracias por registrarte...</p>",
        true);
}
```

**Después:**

```csharp
// Método que encola el correo para envío asíncrono
public async Task SendWelcomeEmailAsync(User user)
{
    await _emailQueueService.QueueEmailAsync(
        user.Email,
        "Bienvenido a nuestra plataforma",
        $"<p>Hola {user.Name},</p><p>Gracias por registrarte...</p>",
        true);
}
```

### Creación de Nuevos Tipos de Mensajes

Para extender el sistema con nuevos tipos de mensajes:

1. Crear una nueva clase de mensaje en `AuthSystem.Domain.Models.MessageQueue.Messages`
2. Definir una interfaz de servicio en `AuthSystem.Domain.Interfaces.Services`
3. Implementar el servicio en `AuthSystem.Infrastructure.Services`
4. Crear un consumidor en `AuthSystem.Infrastructure.MessageQueue.Consumers`
5. Registrar el consumidor en `MessageQueueExtensions.cs`

---

## Monitoreo y Mantenimiento

### Interfaz de Administración de RabbitMQ

La interfaz web de RabbitMQ proporciona herramientas para:

- Monitorear el estado de las colas
- Ver estadísticas de mensajes (publicados, entregados, etc.)
- Gestionar conexiones y canales
- Inspeccionar mensajes individuales

Acceso: `http://localhost:15672` (usuario: guest, contraseña: guest)

### Métricas Clave a Monitorear

| Métrica | Descripción | Umbral de Alerta |
|---------|-------------|------------------|
| Longitud de Cola | Número de mensajes en espera | >1000 mensajes |
| Tasa de Consumo | Mensajes procesados por segundo | <10 mensajes/segundo |
| Mensajes sin Confirmar | Mensajes entregados pero no confirmados | >100 mensajes |
| Uso de Memoria | Memoria utilizada por RabbitMQ | >70% de la memoria asignada |

### Mantenimiento Preventivo

1. **Limpieza de Colas**: Revisar y purgar mensajes antiguos no procesados
2. **Actualización de RabbitMQ**: Mantener el servidor actualizado a la última versión estable
3. **Respaldo de Configuración**: Realizar copias de seguridad de la configuración de RabbitMQ
4. **Revisión de Logs**: Analizar los logs para detectar problemas potenciales

---

## Solución de Problemas

### Problemas Comunes y Soluciones

| Problema | Posibles Causas | Solución |
|----------|----------------|----------|
| Mensajes no procesados | Consumidores inactivos, errores en el procesamiento | Verificar estado de los consumidores, revisar logs de errores |
| Colas creciendo sin control | Tasa de publicación > tasa de consumo, consumidores lentos | Aumentar número de consumidores, optimizar procesamiento |
| Conexión rechazada | Credenciales incorrectas, servidor no disponible | Verificar configuración, comprobar estado del servidor |
| Mensajes duplicados | Problemas de confirmación, reintentos excesivos | Implementar idempotencia en los consumidores |

### Logs y Diagnóstico

Los logs del sistema de colas se encuentran en:

- **Logs de RabbitMQ**: `/var/log/rabbitmq` (Linux) o `%APPDATA%\RabbitMQ\log` (Windows)
- **Logs de la Aplicación**: Configurados en `appsettings.json` bajo la sección `Logging`

---

## Preguntas Frecuentes

### Generales

**P: ¿Qué pasa si RabbitMQ no está disponible?**  
R: La aplicación intentará reconectarse automáticamente. Si no es posible, se registrará un error y la operación fallará.

**P: ¿Cómo puedo aumentar el rendimiento del sistema?**  
R: Ajustar `PrefetchCount` y `ConcurrentMessageLimit`, aumentar el número de consumidores, y optimizar el procesamiento de mensajes.

**P: ¿Los mensajes se pierden si el servidor se reinicia?**  
R: No, RabbitMQ persiste los mensajes en disco si está configurado correctamente.

### Correos Electrónicos

**P: ¿Puedo cancelar un correo programado?**  
R: Actualmente no se soporta la cancelación de mensajes programados.

**P: ¿Cómo sé si un correo se envió correctamente?**  
R: Los logs del sistema registran el resultado del envío. Se podría implementar un sistema de notificación de estado.

### Reportes

**P: ¿Cuánto tiempo se guardan los reportes generados?**  
R: Los reportes se guardan en la ruta especificada indefinidamente. Se recomienda implementar una política de retención.

**P: ¿Puedo generar reportes muy grandes?**  
R: Sí, el sistema está diseñado para manejar reportes de cualquier tamaño, pero se recomienda establecer límites razonables.

---

## Recursos Adicionales

- [Documentación Oficial de RabbitMQ](https://www.rabbitmq.com/documentation.html)
- [Documentación de MassTransit](https://masstransit-project.com/documentation/concepts/)
- [Patrones de Mensajería Empresarial](https://www.enterpriseintegrationpatterns.com/)
- [Mejores Prácticas para Sistemas de Colas](https://www.cloudamqp.com/blog/part1-rabbitmq-best-practice.html)
