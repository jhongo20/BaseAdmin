# Sistema de Logging Estructurado

## Descripción

El sistema de logging estructurado implementado con Serilog permite registrar eventos y errores de la aplicación de manera eficiente y organizada. Los logs estructurados son más fáciles de consultar, filtrar y analizar que los logs tradicionales basados en texto plano.

## Características

- **Logging estructurado**: Los logs se almacenan en formato JSON, lo que facilita su procesamiento y análisis.
- **Múltiples destinos**: Los logs se envían a la consola y a archivos (texto plano y JSON).
- **Rotación de archivos**: Los archivos de log se rotan diariamente para evitar que crezcan demasiado.
- **Enriquecimiento de logs**: Los logs se enriquecen con información contextual como el usuario, la solicitud HTTP, el hilo de ejecución, etc.
- **Niveles de log configurables**: Se pueden configurar diferentes niveles de log para diferentes partes de la aplicación.
- **Correlación de solicitudes**: Cada solicitud HTTP recibe un ID de correlación que se incluye en todos los logs relacionados.

## Arquitectura

El sistema de logging está compuesto por:

1. **Configuración**: Sección `Serilog` en el archivo `appsettings.json`.
2. **Middleware `LoggingMiddleware`**: Enriquece los logs con información contextual y registra el inicio y fin de cada solicitud HTTP.
3. **Extensiones `LoggingExtensions`**: Facilitan el uso de Serilog en los controladores y servicios.
4. **Integración con ASP.NET Core**: Serilog se integra con el sistema de logging de ASP.NET Core.

## Configuración

La configuración del sistema de logging se realiza en el archivo `appsettings.json`:

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Enrichers.Environment",
      "Serilog.Enrichers.Thread"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/structured-log-.json",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId"
    ],
    "Properties": {
      "Application": "AuthSystem"
    }
  }
}
```

### Explicación de la configuración

- **Using**: Lista de ensamblados que Serilog debe cargar.
- **MinimumLevel**: Nivel mínimo de log que se registrará.
  - **Default**: Nivel mínimo para todos los logs.
  - **Override**: Niveles mínimos para namespaces específicos.
- **WriteTo**: Lista de destinos donde se enviarán los logs.
  - **Console**: Logs en la consola.
  - **File**: Logs en archivos de texto plano.
  - **File (JSON)**: Logs en archivos JSON.
- **Enrich**: Lista de enriquecedores que añaden información adicional a los logs.
- **Properties**: Propiedades globales que se añaden a todos los logs.

## Niveles de Log

Serilog define los siguientes niveles de log, en orden de severidad:

1. **Verbose**: Información detallada para depuración.
2. **Debug**: Información útil para depuración.
3. **Information**: Información general sobre el funcionamiento de la aplicación.
4. **Warning**: Advertencias que no impiden el funcionamiento de la aplicación.
5. **Error**: Errores que afectan a una operación específica.
6. **Fatal**: Errores críticos que impiden el funcionamiento de la aplicación.

## Uso en el código

### Registrar un log simple

```csharp
// En un controlador o servicio
private readonly ILogger<MyController> _logger;

public MyController(ILogger<MyController> logger)
{
    _logger = logger;
}

public IActionResult MyAction()
{
    _logger.LogInformation("Ejecutando MyAction");
    // ...
}
```

### Registrar un log con propiedades estructuradas

```csharp
_logger.LogInformation("Usuario {Username} ha iniciado sesión desde {IpAddress}", username, ipAddress);
```

### Registrar una excepción

```csharp
try
{
    // ...
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error al procesar la solicitud");
    return StatusCode(500);
}
```

### Usar el contexto de log

```csharp
using (LogContext.PushProperty("CustomProperty", "Value"))
{
    _logger.LogInformation("Este log incluirá CustomProperty");
}
```

### Usar las extensiones de logging

```csharp
// Enriquecer logs con información del usuario
httpContext.EnrichLogsWithUserInfo();

// Enriquecer logs con información de la solicitud
httpContext.EnrichLogsWithRequestInfo();

// Registrar una excepción con información detallada
_logger.LogDetailedException(ex, "Error al procesar la solicitud");
```

## Middleware de Logging

El middleware `LoggingMiddleware` se encarga de:

1. Generar un ID de correlación para cada solicitud HTTP.
2. Enriquecer los logs con información de la solicitud y del usuario.
3. Registrar el inicio y fin de cada solicitud HTTP.
4. Registrar las excepciones no controladas.

```csharp
// En Program.cs
app.UseLogging();
```

## Estructura de los archivos de log

### Logs de texto plano

Los logs de texto plano se almacenan en archivos con el formato `logs/log-YYYYMMDD.txt` y tienen el siguiente formato:

```
[2025-04-16 16:15:30.123 -05:00 INF] Solicitud iniciada: GET /api/v1/users
[2025-04-16 16:15:30.456 -05:00 INF] Solicitud completada: GET /api/v1/users - Estado: 200 - Duración: 123ms
```

### Logs estructurados (JSON)

Los logs estructurados se almacenan en archivos con el formato `logs/structured-log-YYYYMMDD.json` y tienen el siguiente formato:

```json
{"@t":"2025-04-16T16:15:30.1234567Z","@m":"Solicitud iniciada: GET /api/v1/users","@i":"42c54e84","CorrelationId":"6f1dfeb8-1234-5678-9abc-def012345678","RequestMethod":"GET","RequestPath":"/api/v1/users","RequestIP":"127.0.0.1","UserId":"123","Username":"admin","UserRole":"Admin","Application":"AuthSystem","MachineName":"SERVER01","ThreadId":5}
```

## Buenas prácticas

1. **Usar niveles de log adecuados**: Utilizar el nivel de log adecuado según la importancia del mensaje.
2. **Incluir información contextual**: Añadir información contextual a los logs para facilitar su análisis.
3. **Evitar información sensible**: No incluir información sensible como contraseñas o tokens en los logs.
4. **Usar propiedades estructuradas**: Utilizar propiedades estructuradas en lugar de concatenar strings.
5. **Gestionar excepciones**: Registrar las excepciones con su stack trace completo.
6. **Monitorear los logs**: Revisar regularmente los logs para detectar problemas.
7. **Rotar los archivos de log**: Configurar la rotación de archivos para evitar que crezcan demasiado.

## Herramientas para analizar logs

Los logs estructurados en formato JSON pueden ser analizados con herramientas como:

- **Seq**: Servidor de logs estructurados con interfaz web.
- **Elasticsearch + Kibana**: Stack ELK para almacenar y visualizar logs.
- **Grafana + Loki**: Stack para almacenar y visualizar logs.
- **Azure Application Insights**: Servicio de Azure para monitoreo y análisis de aplicaciones.

## Consideraciones de rendimiento

- **Filtrar logs innecesarios**: Configurar los niveles mínimos de log para evitar registrar información innecesaria.
- **Usar buffering**: Configurar el buffering de logs para mejorar el rendimiento.
- **Limitar el tamaño de los logs**: Configurar la rotación y retención de archivos de log.
- **Usar async logging**: Utilizar métodos asincrónicos para registrar logs en producción.

## Extensibilidad

Para añadir nuevos destinos de log:

1. Instalar el paquete NuGet correspondiente (por ejemplo, `Serilog.Sinks.Elasticsearch`).
2. Añadir el destino en la configuración de Serilog en `appsettings.json`.

```json
"WriteTo": [
  // ...
  {
    "Name": "Elasticsearch",
    "Args": {
      "nodeUris": "http://localhost:9200",
      "indexFormat": "authsystem-logs-{0:yyyy.MM}"
    }
  }
]
```

## Ejemplos de consultas

### Filtrar logs por nivel

```
logs/log-20250416.txt | Select-String -Pattern "ERR"
```

### Filtrar logs por usuario

```
Get-Content logs/structured-log-20250416.json | ConvertFrom-Json | Where-Object { $_.Username -eq "admin" }
```

### Filtrar logs por tiempo de respuesta

```
Get-Content logs/structured-log-20250416.json | ConvertFrom-Json | Where-Object { $_.ElapsedMilliseconds -gt 1000 }
```

### Filtrar logs por código de estado HTTP

```
Get-Content logs/structured-log-20250416.json | ConvertFrom-Json | Where-Object { $_.StatusCode -eq 500 }
```

## Conclusión

El sistema de logging estructurado implementado con Serilog proporciona una solución robusta y flexible para registrar eventos y errores en la aplicación. Los logs estructurados facilitan el análisis y la resolución de problemas, mejorando la observabilidad de la aplicación.
