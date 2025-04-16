# API Versionada

## Descripción

La API versionada permite mantener múltiples versiones de la API simultáneamente, facilitando la evolución de la API sin romper la compatibilidad con clientes existentes. Esto proporciona las siguientes ventajas:

- Permite realizar cambios importantes sin afectar a los clientes existentes
- Facilita la transición gradual de los clientes a nuevas versiones
- Proporciona un mecanismo claro para deprecar y retirar versiones antiguas
- Mejora la documentación y la experiencia del desarrollador

## Estrategia de versionado

El sistema utiliza una estrategia de versionado basada en la URL, donde la versión se especifica como parte de la ruta:

```
/api/v{version}/{recurso}
```

Por ejemplo:
- `/api/v1/users`
- `/api/v2/users`

Esta estrategia ofrece las siguientes ventajas:
- Es fácil de entender y utilizar
- Es compatible con todas las herramientas y frameworks
- Permite a los desarrolladores ver claramente qué versión están utilizando
- Facilita las pruebas y la depuración

## Configuración

La configuración del versionado de API se realiza utilizando el paquete `Microsoft.AspNetCore.Mvc.Versioning`. La configuración básica se encuentra en el método `ConfigureServices` de la clase `Startup`:

```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

## Convenciones de versionado

### Cuándo crear una nueva versión

Se debe crear una nueva versión de la API cuando se realicen cambios que rompan la compatibilidad con versiones anteriores, como:

- Eliminar o renombrar propiedades en los modelos de respuesta
- Cambiar el tipo de datos de una propiedad
- Modificar la estructura de la respuesta
- Cambiar el comportamiento de un endpoint de manera significativa
- Eliminar un endpoint existente

### Cuándo no crear una nueva versión

No es necesario crear una nueva versión cuando los cambios son compatibles con versiones anteriores, como:

- Añadir nuevas propiedades opcionales a los modelos de respuesta
- Añadir nuevos endpoints
- Corregir errores sin cambiar el comportamiento esperado
- Mejorar el rendimiento sin cambiar la interfaz

## Implementación

### Controladores versionados

Los controladores se decoran con el atributo `ApiVersion` para indicar a qué versión pertenecen:

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[ApiController]
public class UsersV1Controller : ControllerBase
{
    // Implementación de la versión 1
}

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/users")]
[ApiController]
public class UsersV2Controller : ControllerBase
{
    // Implementación de la versión 2
}
```

### Mapeo entre versiones

Para facilitar la implementación de múltiples versiones, se utilizan mappers para convertir entre diferentes versiones de los modelos:

```csharp
public class UserMapperV1ToV2 : Profile
{
    public UserMapperV1ToV2()
    {
        CreateMap<UserResponseV1, UserResponseV2>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress));
    }
}
```

### Deprecación de versiones

Las versiones deprecadas se marcan con el atributo `ApiVersionAttribute` y la propiedad `Deprecated`:

```csharp
[ApiVersion("1.0", Deprecated = true)]
[Route("api/v{version:apiVersion}/users")]
[ApiController]
public class UsersV1Controller : ControllerBase
{
    // Implementación de la versión 1 (deprecada)
}
```

## Documentación con Swagger

La API versionada se integra con Swagger para proporcionar documentación interactiva para cada versión:

```csharp
services.AddSwaggerGen(options =>
{
    // Obtener todas las versiones de API disponibles
    var apiVersionDescriptions = provider.ApiVersionDescriptions;
    
    // Crear un documento Swagger para cada versión
    foreach (var description in apiVersionDescriptions)
    {
        options.SwaggerDoc(
            description.GroupName,
            new OpenApiInfo
            {
                Title = $"Auth System API {description.GroupName}",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ? "Esta versión de la API está obsoleta." : "API de autenticación y autorización.",
                Contact = new OpenApiContact
                {
                    Name = "Equipo de Desarrollo",
                    Email = "dev@example.com"
                }
            });
    }
    
    // Configuración adicional de Swagger...
});
```

## Ciclo de vida de las versiones

El ciclo de vida de las versiones de la API sigue estas etapas:

1. **Activa**: La versión está completamente soportada y se recomienda su uso.
2. **Deprecada**: La versión sigue funcionando pero está marcada como obsoleta. Los clientes deben migrar a una versión más reciente.
3. **Retirada**: La versión ya no está disponible. Las solicitudes a esta versión recibirán un error 410 Gone.

El tiempo entre cada etapa se comunica claramente a los clientes:

- Las versiones permanecen activas durante al menos 12 meses.
- Las versiones deprecadas se mantienen durante al menos 6 meses antes de ser retiradas.
- Se envían notificaciones a los clientes registrados 3 meses antes de retirar una versión.

## Ejemplo de uso

### Cliente consumiendo diferentes versiones

```javascript
// Cliente JavaScript - Versión 1
async function getUsersV1() {
  const response = await fetch('/api/v1/users', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  return await response.json();
}

// Cliente JavaScript - Versión 2
async function getUsersV2() {
  const response = await fetch('/api/v2/users', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  return await response.json();
}
```

### Respuestas de diferentes versiones

```json
// Respuesta de /api/v1/users/123
{
  "id": "123",
  "firstName": "John",
  "lastName": "Doe",
  "emailAddress": "john.doe@example.com",
  "isActive": true
}

// Respuesta de /api/v2/users/123
{
  "id": "123",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "isActive": true,
  "createdAt": "2025-01-15T10:30:45Z",
  "lastLogin": "2025-04-16T15:20:10Z"
}
```
