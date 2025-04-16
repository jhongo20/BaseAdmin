# Documentación de Rate Limiting en AuthSystem

## Introducción

El Rate Limiting es una técnica de seguridad que limita el número de solicitudes que un cliente puede hacer a la API en un período de tiempo determinado. Esta funcionalidad es crucial para:

- Proteger contra ataques de denegación de servicio (DoS)
- Prevenir ataques de fuerza bruta
- Garantizar un uso equitativo de los recursos
- Mantener la estabilidad y disponibilidad del sistema

## Implementación

AuthSystem implementa una solución de rate limiting robusta y configurable utilizando la biblioteca `AspNetCoreRateLimit`. La implementación incluye:

1. **Doble protección**: Limitación por dirección IP y por identificador de cliente
2. **Reglas específicas por endpoint**: Diferentes límites según la sensibilidad de cada operación
3. **Listas blancas**: Excepciones para IPs y clientes autorizados
4. **Respuestas personalizadas**: Mensajes informativos cuando se excede el límite
5. **Registro y monitoreo**: Middleware para detectar y registrar intentos de abuso

## Configuración

La configuración del rate limiting se encuentra en el archivo `appsettings.json` bajo la sección `RateLimiting`:

```json
"RateLimiting": {
  "EnableEndpointRateLimiting": true,
  "StackBlockedRequests": false,
  "HttpStatusCode": 429,
  "IpWhitelist": ["127.0.0.1", "::1"],
  "ClientIdHeader": "X-ClientId",
  "ClientWhitelist": ["admin-dashboard"],
  "GeneralRules": [
    {
      "Endpoint": "*:/api/auth/login",
      "Period": "5m",
      "Limit": 10
    },
    ...
  ],
  "QuotaExceededResponse": {
    "Content": "{ \"status\": 429, \"message\": \"Demasiadas solicitudes. Por favor, intente de nuevo más tarde.\", \"retryAfter\": {0} }",
    "ContentType": "application/json",
    "StatusCode": 429
  }
}
```

### Explicación de la configuración

| Parámetro | Descripción |
|-----------|-------------|
| `EnableEndpointRateLimiting` | Cuando es `true`, las reglas se aplican por endpoint. Si es `false`, se aplican globalmente. |
| `StackBlockedRequests` | Si es `true`, las solicitudes bloqueadas se cuentan para el siguiente período. |
| `HttpStatusCode` | Código de estado HTTP devuelto cuando se excede el límite (429 = Too Many Requests). |
| `IpWhitelist` | Lista de direcciones IP que están exentas de las limitaciones. |
| `ClientIdHeader` | Nombre del encabezado HTTP que contiene el identificador del cliente. |
| `ClientWhitelist` | Lista de identificadores de cliente que están exentos de las limitaciones. |
| `GeneralRules` | Conjunto de reglas que definen los límites para diferentes endpoints. |
| `QuotaExceededResponse` | Configuración de la respuesta cuando se excede el límite. |

### Reglas generales

Cada regla en `GeneralRules` tiene la siguiente estructura:

```json
{
  "Endpoint": "*:/api/auth/login",
  "Period": "5m",
  "Limit": 10
}
```

| Parámetro | Descripción | Ejemplos |
|-----------|-------------|----------|
| `Endpoint` | Patrón que define a qué endpoints se aplica la regla. Utiliza el formato `{HTTP_VERB}:{RUTA}`. El asterisco (`*`) es un comodín. | `*:/api/auth/login`, `get:/api/users*` |
| `Period` | Período de tiempo durante el cual se aplica el límite. | `1s`, `1m`, `1h`, `1d` |
| `Limit` | Número máximo de solicitudes permitidas durante el período. | `5`, `10`, `100` |

## Reglas actuales

| Endpoint | Límite | Período | Justificación |
|----------|--------|---------|---------------|
| `/api/auth/login` | 10 | 5 minutos | Prevenir ataques de fuerza bruta |
| `/api/auth/register` | 5 | 1 hora | Evitar creación masiva de cuentas |
| `/api/auth/reset-password` | 3 | 1 hora | Proteger contra abuso de restablecimiento de contraseñas |
| `/api/users*` | 30 | 1 minuto | Operaciones frecuentes pero sensibles |
| `/api/roles*` | 20 | 1 minuto | Operaciones de administración menos frecuentes |
| `/api/permissions*` | 20 | 1 minuto | Operaciones de administración menos frecuentes |
| `/api/organizations*` | 20 | 1 minuto | Operaciones de administración menos frecuentes |
| `/api/modules*` | 20 | 1 minuto | Operaciones de administración menos frecuentes |
| `/api/ratelimittest*` | 5 | 1 minuto | Endpoint de prueba |
| `/api/ratelimittest/client*` | 3 | 1 minuto | Endpoint de prueba con identificador de cliente |
| `*` (todos los demás) | 100 | 1 minuto | Límite general para cualquier endpoint no especificado |

## Cómo ampliar límites

Para modificar o ampliar los límites de rate limiting, puedes:

1. **Modificar reglas existentes**: Ajustar los valores de `Limit` o `Period` en las reglas existentes.
2. **Agregar nuevas reglas**: Añadir nuevas entradas en el array `GeneralRules`.
3. **Añadir excepciones**: Incluir IPs o identificadores de cliente en las listas blancas.

### Ejemplos de modificaciones

#### Aumentar el límite general
```json
{
  "Endpoint": "*",
  "Period": "1m",
  "Limit": 200  // Aumentado de 100 a 200
}
```

#### Añadir una regla para un nuevo endpoint
```json
{
  "Endpoint": "*:/api/reports*",
  "Period": "1h",
  "Limit": 50
}
```

#### Añadir una regla específica por método HTTP
```json
{
  "Endpoint": "post:/api/documents",
  "Period": "1m",
  "Limit": 15
}
```

## Registro y monitoreo

El sistema incluye un middleware personalizado (`RateLimitingLoggerMiddleware`) que registra los intentos de exceder los límites. Este middleware:

1. Captura información detallada sobre cada intento (IP, cliente, ruta, método)
2. Registra eventos en los logs del sistema
3. Permite implementar lógica adicional como alertas o bloqueos temporales

## Pruebas

Para probar el rate limiting, se ha implementado un controlador específico:

- **Endpoint**: `/api/ratelimittest`
- **Límite**: 5 solicitudes por minuto
- **Respuesta**: Información sobre los límites y solicitudes restantes

Para probar con un identificador de cliente:

1. Acceder a `/api/ratelimittest/client`
2. Incluir el encabezado `X-ClientId` con un valor
3. Para evitar limitaciones, usar un cliente en la lista blanca: `X-ClientId: admin-dashboard`

## Consideraciones para producción

En un entorno de producción, considere:

1. **Almacenamiento distribuido**: Reemplazar el almacenamiento en memoria por Redis para entornos con múltiples instancias
2. **Monitoreo avanzado**: Implementar alertas cuando se detecten patrones de abuso
3. **Ajuste fino**: Revisar y ajustar los límites basándose en patrones de uso reales
4. **Documentación para clientes**: Informar a los consumidores de la API sobre los límites existentes

## Referencias

- [Documentación de AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit)
- [OWASP - Protección contra ataques de fuerza bruta](https://owasp.org/www-community/controls/Blocking_Brute_Force_Attacks)
- [Buenas prácticas para implementar rate limiting en APIs](https://cloud.google.com/architecture/api-design-best-practices-rate-limiting)
