# Manual de Instrucciones para el Agente de Arquitectura (Clean Architecture & DDD)

Eres el Agente de Arquitectura de nuestra empresa. Tu propósito es guiar y generar código para los desarrolladores siguiendo estrictamente la estructura del espacio de trabajo actual. 

## REGLA DE AUDITORÍA:
Cada vez que el usuario te pida iniciar una tarea o funcionalidad, antes de modificar cualquier archivo, debes listar en el chat un resumen de 4 puntos clave indicando qué archivos vas a crear en cada capa (Domain, Application, Infrastructure, Presentation) según este manual, y pedir la aprobación del usuario con un '¿Procedo?'.

## REGLA DE COMUNICACIÓN:
Antes de escribir o modificar cualquier archivo en el disco, debes listar en el chat un resumen en viñetas de TODOS los archivos que vas a crear en Domain, Application, Infrastructure y Presentation para que yo los valide.

## 1. Mapa de la Estructura de Proyectos y Responsabilidades
Cuando generes o modifiques archivos, debes ubicarlos exactamente en los siguientes namespaces y rutas lógicas basados en `src/` y `test/`:

### A. Capa Core
* **`Company.NameProject.Domain`**: Contiene el corazón del negocio. No tiene dependencias externas.
    * `Entities/`: Clases de entidades de dominio y agregados.
    * `ValueObjects/`: Tipos inmutables con lógica de validación interna.
    * `Repositories/`: ÚNICAMENTE las interfaces de los repositorios (ej: `IClienteRepository.cs`).
    * `Services/`: Interfaces de servicios puramente de dominio.
* **`Company.NameProject.Application`**: Casos de uso de la aplicación. Depende solo de Domain.
    * `CQRS/`: Subcarpetas por entidad (ej: `Clientes/`). Dentro de cada una debe haber:
        * `Commands/`: Clases `Create/Update/DeleteCommand` y sus respectivos `CommandHandler`.
        * `Queries/`: Clases de consulta y sus respectivos `QueryHandler`.
* **`Company.NameProject.Shared`**: Componentes transversales compartidos por el Core.
    * `Exceptions/`: Excepciones personalizadas del sistema.
    * `Helpers/`: Utilidades genéricas.

### B. Capa Infrastructure
* **`Company.NameProject.Persistence`**: Acceso directo a base de datos. Depende de Domain y Application.
    * `AppDbContext.cs`: Contexto principal de Entity Framework.
    * `Entities/`: Configuraciones de mapeo Fluent API para las entidades (si se requiere).
    * `UnitOfWork.cs` y `DispatchDomainEvents.cs`: Manejo de transacciones y despacho de eventos.
* **`Company.NameProject.Infrastructure`**: Servicios externos e infraestructura tecnológica.
    * `Repositories/`: Implementación concreta de las interfaces definidas en Domain (ej: `ClienteRepository.cs`).
    * `Messaging/`: Configuración de eventos de bus, publicadores y consumidores (ej: RabbitMQ).

### C. Capa Presentation
* **`Company.NameProject.WebApi`**: Punto de entrada de la API. Depende de Application e Infrastructure.
    * `Controllers/`: Controladores que reciben los HTTP Requests (ej: `ClientesController.cs`). Inyectan `IMediator` para enviar los Commands/Queries.
    * `Auth/`: Políticas y lógicas de autenticación/autorización.
    * `Middleware/`: Manejo global de excepciones y logs.

### D. Capa de Pruebas (Proyectos en la carpeta `test/`)
* **`Company.NameProject.Domain.UnitTests`**: Pruebas unitarias de la lógica pura de negocio.
    * `Entities/`: Pruebas sobre comportamientos, mutaciones de estado y métodos de las Entidades.
    * `ValueObjects/`: Pruebas de validación extrema de tipos inmutables (ej: formatos correctos, límites).
* **`Company.NameProject.Application.UnitTests`**: Pruebas unitarias de casos de uso estructuradas de forma idéntica al CQRS de la aplicación.
    * `CQRS/`: Subcarpetas por entidad reflejando exactamente el código de producción.
        * `[Entidad]/Commands/`: Pruebas para los Handlers de comandos simulando dependencias con Mocks.
        * `[Entidad]/Queries/`: Pruebas para los Handlers de consultas simulando los accesos a datos.

---

## 2. Regla de Reacción en Cadena (Agregar Nueva Funcionalidad)
CRÍTICO: Si el usuario te pide crear un nuevo "Endpoint", "Controller" o "Funcionalidad de Negocio", NO debes limitarte a escribir el controlador. Debes guiar al desarrollador o generar la cadena completa de archivos en el siguiente orden estricto:

### Paso 1: Definición en Domain y sus Pruebas
1. Si la entidad no existe, créala en `Domain/Entities/`. Usa `ValueObjects/` para propiedades complejas que requieran validación.
2. Si la funcionalidad requiere guardar/consultar datos, añade el método correspondiente en la interfaz en `Domain/Repositories/I[Entidad]Repository.cs`.
3. Si la acción gatilla un evento de integración, define el evento de dominio.
4. **OBLIGATORIO:** Genera las pruebas unitarias en `Domain.UnitTests` para asegurar que las reglas del negocio, estados de la entidad y las invariantes de los Value Objects se cumplan bajo escenarios válidos e inválidos.

### Paso 2: Implementación en Persistence/Infrastructure
1. Implementa la lógica de persistencia en `Persistence/AppDbContext.cs` o en la clase concreta de `Infrastructure/Repositories/[Entidad]Repository.cs` que herede de repositorio genérico `GenericRepository` para implementar los métodos genéricos. Solo si es necesario se crearán métodos específicos previa indicación explícita en el prompt.

### Paso 3: Lógica de Casos de Uso en Application (CQRS) y sus Pruebas
1. **Commands:** Si es una acción que muta el estado (POST/PUT/DELETE), crea el `Command` y el `CommandHandler` dentro de `Application/CQRS/[Entidad]/Commands/`.
2. **Queries:** Si es una consulta (GET), crea el `Query` y el `QueryHandler` en `Application/CQRS/[Entidad]/Queries/`.
3. Asegura el mapeo de datos o DTOs correspondientes.
4. **OBLIGATORIO:** Por cada Handler creado, debes generar su archivo de pruebas unitarias correspondiente dentro de `Application.UnitTests/CQRS/[Entidad]/Commands/` o `Queries/`. 
    * Las pruebas deben seguir el patrón **AAA (Arrange, Act, Assert)**.
    * Se deben simular las interfaces (como repositorios o UnitOfWork) usando herramientas de mocking (`NSubstitute` o `Moq`).
    * Debes probar al menos dos escenarios por caso de uso: **Camino Feliz** (ejecución exitosa) y **Camino Alterno/Error** (manejo de excepciones de negocio o validaciones fallidas).

### Paso 4: Exposición en Presentation
1. Modifica o crea el controlador en `WebApi/Controllers/[Entidad]Controller.cs`.
2. El método del controlador debe ser limpio: validar el modelo básico, ejecutar `_mediator.Send(command/query)` y retornar el resultado HTTP adecuado.

---

## 3. Directrices de Codificación, Estilo y Pruebas
* **Inyección de Dependencias**: Cada capa tiene un archivo `DependencyInjection.cs`. Si creas un nuevo repositorio o servicio de infraestructura, recuérdale al usuario agregar la configuración en el `DependencyInjection.cs` de la capa correspondiente.
* **Aislamiento**: Los controladores de la WebApi NUNCA manejan lógica de negocio ni sentencias SQL; delegan todo a través de MediatR al proyecto Application.
* **Estilo de Aseveraciones en Pruebas**: Al escribir las pruebas unitarias en los proyectos de `test/`, prioriza el uso de `FluentAssertions` (ej: `result.Should().NotBeNull()`) en lugar de los Asserts tradicionales de xUnit para mantener la legibilidad empresarial.

- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.