# Guía de Desarrollo para AuthSystem

## Introducción

Esta guía proporciona información para desarrolladores que trabajan en el proyecto AuthSystem. Incluye convenciones de código, flujos de trabajo recomendados y mejores prácticas para mantener la calidad del código y la coherencia en todo el proyecto.

## Entorno de Desarrollo

### Requisitos

- **.NET SDK 8.0** o superior
- **Visual Studio 2022** o **Visual Studio Code** con extensiones para C#
- **SQL Server** (local o en contenedor Docker)
- **Git** para control de versiones

### Configuración Inicial

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

## Convenciones de Código

### Nomenclatura

- **Clases y Métodos**: PascalCase (ej. `UserService`, `GetById`)
- **Variables y Parámetros**: camelCase (ej. `userId`, `roleList`)
- **Interfaces**: Prefijo "I" + PascalCase (ej. `IUserRepository`)
- **Constantes**: UPPER_SNAKE_CASE (ej. `MAX_LOGIN_ATTEMPTS`)
- **Archivos**: Mismo nombre que la clase principal que contienen

### Estructura de Código

- Organizar miembros de clase en el siguiente orden:
  1. Campos privados
  2. Propiedades
  3. Constructores
  4. Métodos públicos
  5. Métodos privados

- Limitar la longitud de los métodos a 30 líneas cuando sea posible
- Evitar comentarios obvios, preferir código autoexplicativo
- Usar regiones solo cuando sea absolutamente necesario

### Documentación

- Documentar todas las interfaces y clases públicas con comentarios XML
- Incluir ejemplos de uso cuando sea apropiado
- Mantener actualizada la documentación cuando cambie el código

## Flujo de Trabajo de Desarrollo

### Gestión de Ramas

Seguimos un flujo de trabajo basado en GitFlow:

- `main`: Código de producción estable
- `develop`: Rama de integración para características completadas
- `feature/nombre-caracteristica`: Ramas para nuevas características
- `bugfix/nombre-bug`: Ramas para correcciones de errores
- `release/x.y.z`: Ramas de preparación para lanzamientos

### Proceso de Desarrollo

1. Crear una rama de característica desde `develop`:
   ```bash
   git checkout develop
   git pull
   git checkout -b feature/nueva-caracteristica
   ```

2. Desarrollar y hacer commits frecuentes:
   ```bash
   git add .
   git commit -m "Descripción clara del cambio"
   ```

3. Mantener la rama actualizada con `develop`:
   ```bash
   git fetch
   git rebase origin/develop
   ```

4. Enviar la rama para revisión:
   ```bash
   git push -u origin feature/nueva-caracteristica
   ```

5. Crear un Pull Request a `develop`

### Revisión de Código

- Todas las contribuciones deben pasar por revisión de código
- Los revisores deben verificar:
  - Funcionalidad correcta
  - Adherencia a la arquitectura
  - Calidad del código
  - Cobertura de pruebas

## Pruebas

### Tipos de Pruebas

- **Pruebas Unitarias**: Para componentes individuales
- **Pruebas de Integración**: Para interacciones entre componentes
- **Pruebas de API**: Para endpoints de la API

### Directrices de Pruebas

- Mantener una cobertura de pruebas de al menos 80%
- Seguir el patrón AAA (Arrange-Act-Assert)
- Usar mocks para aislar dependencias externas
- Nombrar las pruebas con formato: `Method_Scenario_ExpectedBehavior`

### Ejecutar Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar pruebas específicas
dotnet test --filter "Category=UnitTest"
```

## Manejo de Errores y Logging

### Principios de Manejo de Errores

- Usar excepciones personalizadas para errores de dominio
- Capturar y registrar excepciones en los límites de la aplicación
- Devolver respuestas de error consistentes desde la API

### Logging

- Usar Serilog para logging estructurado
- Incluir contexto relevante en los logs
- Seguir niveles de logging apropiados:
  - `Verbose`: Información detallada para depuración
  - `Debug`: Información útil para desarrollo
  - `Information`: Eventos normales de la aplicación
  - `Warning`: Situaciones potencialmente problemáticas
  - `Error`: Errores que no impiden la operación
  - `Fatal`: Errores críticos que detienen la aplicación

## Despliegue

### Entornos

- **Desarrollo**: Para pruebas durante el desarrollo
- **Pruebas**: Para pruebas de QA
- **Producción**: Entorno de usuario final

### Proceso de Despliegue

1. Crear una rama de release:
   ```bash
   git checkout develop
   git checkout -b release/x.y.z
   ```

2. Realizar ajustes finales y pruebas

3. Fusionar en `main` y etiquetar:
   ```bash
   git checkout main
   git merge release/x.y.z
   git tag -a vx.y.z -m "Versión x.y.z"
   git push origin main --tags
   ```

4. Fusionar cambios de vuelta a `develop`:
   ```bash
   git checkout develop
   git merge release/x.y.z
   git push origin develop
   ```

## Recursos Adicionales

- [Documentación de .NET](https://docs.microsoft.com/en-us/dotnet/)
- [Documentación de Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Guía de Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Documentación de MediatR](https://github.com/jbogard/MediatR/wiki)
- [Documentación de AutoMapper](https://docs.automapper.org/en/stable/)
