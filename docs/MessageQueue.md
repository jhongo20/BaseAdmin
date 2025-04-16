# Sistema de Colas para Tareas Pesadas

## Introducción

El sistema de colas implementado permite manejar tareas pesadas de forma asíncrona, como el envío de correos electrónicos y la generación de reportes. Esto mejora el rendimiento de la aplicación al evitar que estas tareas bloqueen los hilos de procesamiento principales.

## Tecnologías Utilizadas

- **RabbitMQ**: Broker de mensajes utilizado para la gestión de colas.
- **MassTransit**: Biblioteca de abstracción para la mensajería que simplifica la interacción con RabbitMQ.
- **Background Services**: Servicios en segundo plano para procesar mensajes de las colas.

## Arquitectura

El sistema de colas sigue una arquitectura de publicación/suscripción con los siguientes componentes:

1. **Productores**: Servicios que generan mensajes y los publican en las colas.
2. **Colas**: Almacenan los mensajes hasta que son procesados.
3. **Consumidores**: Procesan los mensajes de las colas.

### Diagrama de Flujo

```
[Cliente] -> [API] -> [Servicio de Cola] -> [RabbitMQ] -> [Consumidor] -> [Procesamiento]
```

## Tipos de Colas Implementadas

### 1. Cola de Correos Electrónicos

Permite encolar correos electrónicos para su envío asíncrono, evitando que el envío de correos bloquee las solicitudes HTTP.

#### Características:
- Envío de correos individuales o en lote
- Programación de correos para envío futuro
- Priorización de correos
- Soporte para adjuntos

### 2. Cola de Reportes

Permite encolar la generación de reportes que pueden ser procesos intensivos en CPU y memoria.

#### Características:
- Generación de reportes en diferentes formatos (PDF, Excel, CSV)
- Notificación por correo cuando el reporte está listo
- Programación de generación de reportes
- Priorización de reportes

## Configuración

La configuración del sistema de colas se realiza en el archivo `appsettings.json`:

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

### Parámetros de Configuración

- **Host**: Dirección del servidor RabbitMQ.
- **VirtualHost**: Host virtual en RabbitMQ.
- **Username/Password**: Credenciales de autenticación.
- **Port**: Puerto de conexión a RabbitMQ.
- **UseSsl**: Indica si se debe usar SSL para la conexión.
- **RetryCount**: Número de reintentos para mensajes fallidos.
- **RetryInterval**: Intervalo entre reintentos (en milisegundos).
- **PrefetchCount**: Número de mensajes que un consumidor puede procesar a la vez.
- **ConcurrentMessageLimit**: Límite de mensajes concurrentes por consumidor.
- **QueueNames**: Nombres de las colas utilizadas.

## Uso

### Envío de Correos Electrónicos

```csharp
// Inyección del servicio
private readonly IEmailQueueService _emailQueueService;

// Encolar un correo electrónico simple
await _emailQueueService.QueueEmailAsync(
    "destinatario@ejemplo.com",
    "Asunto del correo",
    "<p>Cuerpo del correo en HTML</p>",
    true);

// Encolar un correo electrónico completo
var emailMessage = new EmailMessage
{
    To = "destinatario@ejemplo.com",
    Subject = "Asunto del correo",
    Body = "<p>Cuerpo del correo en HTML</p>",
    IsHtml = true,
    // Otras propiedades...
};
await _emailQueueService.QueueEmailAsync(emailMessage);
```

### Generación de Reportes

```csharp
// Inyección del servicio
private readonly IReportQueueService _reportQueueService;

// Encolar un reporte simple
var parameters = new Dictionary<string, object>
{
    { "UserId", "123" },
    { "StartDate", DateTime.Now.AddDays(-30) },
    { "EndDate", DateTime.Now }
};
await _reportQueueService.QueueReportAsync(
    "UserActivityReport",
    parameters,
    "PDF",
    "usuario@ejemplo.com");

// Encolar un reporte completo
var reportMessage = new ReportMessage
{
    ReportType = "UserActivityReport",
    Parameters = parameters,
    OutputFormat = "PDF",
    NotificationEmail = "usuario@ejemplo.com",
    // Otras propiedades...
};
await _reportQueueService.QueueReportAsync(reportMessage);
```

## Endpoints de API

### Correos Electrónicos

- **POST /api/EmailQueue/queue**: Encola un correo electrónico.
- **POST /api/EmailQueue/queue-bulk**: Encola múltiples correos electrónicos.

### Reportes

- **POST /api/ReportQueue/queue**: Encola un reporte.
- **POST /api/ReportQueue/queue-bulk**: Encola múltiples reportes.

## Monitoreo y Gestión

Para monitorear y gestionar las colas, se puede utilizar la interfaz de administración de RabbitMQ, accesible en:

```
http://localhost:15672
```

Con las credenciales predeterminadas:
- Usuario: guest
- Contraseña: guest

## Consideraciones para Producción

1. **Seguridad**:
   - Cambiar las credenciales predeterminadas de RabbitMQ.
   - Habilitar SSL para conexiones en producción.
   - Configurar permisos adecuados para las colas.

2. **Escalabilidad**:
   - Configurar clústeres de RabbitMQ para alta disponibilidad.
   - Ajustar los parámetros de PrefetchCount y ConcurrentMessageLimit según la carga.

3. **Monitoreo**:
   - Implementar alertas para colas con acumulación de mensajes.
   - Monitorear el consumo de recursos de los consumidores.

4. **Manejo de Errores**:
   - Implementar colas de mensajes muertos (dead-letter queues) para mensajes que no se pueden procesar.
   - Configurar políticas de reintento adecuadas.

## Extensión del Sistema

El sistema de colas está diseñado para ser extensible. Para añadir nuevos tipos de colas:

1. Crear un nuevo modelo de mensaje en `AuthSystem.Domain.Models.MessageQueue.Messages`.
2. Crear una interfaz de servicio en `AuthSystem.Domain.Interfaces.Services`.
3. Implementar el servicio en `AuthSystem.Infrastructure.Services`.
4. Crear un consumidor en `AuthSystem.Infrastructure.MessageQueue.Consumers`.
5. Registrar el consumidor en `MessageQueueExtensions.cs`.
6. Actualizar la configuración en `appsettings.json`.
