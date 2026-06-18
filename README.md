# Guía de Uso de la Plantilla de Arquitectura

Sigue estos pasos para configurar tu entorno, descargar la plantilla y generar nuevos microservicios.

## Requisitos Previos
Antes de comenzar, asegúrate de tener instalado lo siguiente en tu máquina:
* **.NET 8.0 SDK** (o versión superior)
* **Git** para el control de versiones
* **Docker Desktop** (opcional, recomendado para levantar SQLServer/PostgreSQL y RabbitMQ)

---

## 1. Descargar plantilla
Clona el repositorio base desde GitHub ejecutando el siguiente comando:

```bash
git clone https://github.com/EEEC1409/BaseArchitecture-netcore.git
```

## 2. Instalar plantilla
Instala la plantilla en tu CLI de .NET de forma local apuntando a la ruta de origen:

```bash
dotnet new install "ruta_descarga\BaseArchitecture-netcore"
```

## 3. Crear nuevo proyecto
Para generar un nuevo proyecto con la configuración inicial estándar, ejecuta:

```bash
dotnet new arquitectura-base -n "Cresa.XXXXXXX" --Company Cresa --ProjectName XXXXXXX --DatabaseType SQLServer --IncludeRabbit false
```
* arquitectura-base, este nombre esta en el template del proyecto base
* **-n** Es el nombre de la carpeta que contendrá todo el proyecto
* **--Company** nombre del empresa que va como prefijo
* **--ProjectName** nombre del proyecto/aplicación a crear
* **--DatabaseType** tipo de base de datos

### Resultado de Namespaces
El motor de plantillas renombrará automáticamente todas las referencias:
* `Company.NameProject` $\rightarrow$ **Cresa.XXXXXX**
* `Company` $\rightarrow$ **Cresa**
* `NameProject` $\rightarrow$ **XXXXXXX**

---

## Ejemplos de Configuración Avanzada

### Opción A: SQL Server sin RabbitMQ
```bash
dotnet new arquitectura-base -n "Acme.Pagos" --Company Cresa --ProjectName Pagos --DatabaseType SQLServer --IncludeRabbit false
```

### Opción B: PostgreSQL con RabbitMQ
```bash
dotnet new arquitectura-base -n "Acme.Inventario" --Company Acme --ProjectName Inventario --DatabaseType PostgreSQL --IncludeRabbit true
```

---

## Estructura de Carpetas Generada
La plantilla implementa **Arquitectura Limpia (Clean Architecture)** separada por capas:

```
📁 src/
├── 📁 Core/
│   ├── 📁 Company.NameProject.Domain         → Entidades, Value Objects, interfaces de repositorios y eventos de dominio
│   │   ├── 📁 Common/                         → Entity<TId>, AggregateRoot, DomainException
│   │   ├── 📁 Entities/Events/                → IHasDomainEvents
│   │   ├── 📁 Repositories/                   → IGenericRepository<T>
│   │   ├── 📁 Services/                       → (servicios puramente de dominio)
│   │   └── 📁 ValueObjects/                   → Email, Money, CodigoIso
│   │
│   ├── 📁 Company.NameProject.Application     → Casos de uso (CQRS), comportamientos del pipeline y contratos
│   │   ├── 📁 Common/
│   │   │   ├── 📁 Behaviors/                  → ValidationBehavior, TransactionBehavior
│   │   │   └── 📁 Interfaces/                 → IUnitOfWork, IDateTimeProvider, IRequiresTransaction
│   │   └── 📁 CQRS/
│   │       └── 📁 [NombreEntidad]/
│   │           ├── 📁 Commands/               → Crear/Actualizar/EliminarCommand + Handler
│   │           └── 📁 Queries/                → Obtener/ListarQuery + Handler
│   │
│   └── 📁 Company.NameProject.Shared          → Componentes transversales (sin dependencias de dominio)
│       ├── 📁 Common/                         → PagedResult<T>, PaginationRequest
│       ├── 📁 Exceptions/                     → ApiException, ApiResponse<T>
│       └── 📁 Helpers/                        → DateTimeProvider
│
├── 📁 Infrastructure/
│   ├── 📁 Company.NameProject.Infrastructure  → Servicios externos, repositorios concretos y mensajería
│   │   ├── 📁 Messaging/                      → IRabbitMqPublisher, RabbitMqPublisher (opcional)
│   │   ├── 📁 Repositories/                   → GenericRepository<T>
│   │   └── 📁 Services/                       → SystemDateTimeProvider, OutboxProcessorService
│   │
│   └── 📁 Company.NameProject.Persistence     → Acceso a base de datos con EF Core
│       ├── 📁 Entities/                       → OutboxMessage
│       ├── AppDbContext.cs                    → DbContext principal + conversión de eventos a Outbox
│       ├── UnitOfWork.cs                      → Gestión de transacciones
│       └── DispatchDomainEvents.cs            → Serialización de eventos de dominio al Outbox
│
└── 📁 Presentation/
    └── 📁 Company.NameProject.WebApi          → Punto de entrada HTTP (controllers, middlewares, auth)
        ├── 📁 Auth/                           → AuthController, JwtTokenService, LoginRequest/Response
        ├── 📁 Middleware/                     → ExceptionMiddleware (manejo global de errores)
        ├── 📁 Options/                        → JwtSettings
        └── Program.cs

📁 tests/
├── 📁 Company.NameProject.Domain.Tests        → Pruebas unitarias de la capa de Dominio
│   ├── 📁 Common/                             → AggregateRootTests
│   └── 📁 ValueObjects/                       → EmailTests, MoneyTests, CodigoIsoTests
│
└── 📁 Company.NameProject.Application.Tests   → Pruebas unitarias de la capa de Aplicación
    ├── 📁 Common/Behaviors/                   → ValidationBehaviorTests, TransactionBehaviorTests
    └── 📁 CQRS/[NombreEntidad]/
        ├── 📁 Commands/                       → Esqueletos: Crear, Actualizar, Eliminar
        └── 📁 Queries/                        → Esqueletos: Obtener, Listar
```

> 💡 Los esqueletos de `CQRS/[NombreEntidad]/` en los tests están marcados con `[Skip]`.
> Al implementar una nueva entidad, **duplica esa carpeta, renómbrala** e implementa cada caso de prueba.
---

## Ejecución Inicial

Una vez creado el proyecto, compílalo y ejecútalo siguiendo estos comandos:

1. **Navegar a la carpeta de la API:**
   ```bash
   cd src/Acme.Pagos.API
   ```
2. **Restaurar dependencias:**
   ```bash
   dotnet restore
   ```
3. **Correr la aplicación:**
   ```bash
   dotnet run
   ```
4. **Verificar Swagger:** 
Abre tu navegador e ingresa a `https://localhost:55831/swagger` para probar los endpoints y autenticarte con JWT.

4. **Observacion:**
Esto es una plantilla de proyecto, por lo cual deben revisar el appsetting para realizar los ajustes correspondiente como por ejemplo la cadena de conexión, la llave de JWT. 
