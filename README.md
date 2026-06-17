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
dotnet new install "ruta_aqui\BaseArchitecture-netcore"
```

## 3. Crear nuevo proyecto
Para generar un nuevo proyecto con la configuración inicial estándar, ejecuta:

```bash
dotnet new arquitectura-base -n "Acme.Pagos" --Company Cresa --ProjectName Pagos --DatabaseType SQLServer --IncludeRabbit false
```
* arquitectura-base, este nombre esta en el template del proyecto base
    
### Resultado de Namespaces
El motor de plantillas renombrará automáticamente todas las referencias:
* `Company.NameProject` $\rightarrow$ **Cresa.Pagos**
* `Company` $\rightarrow$ **Cresa**
* `NameProject` $\rightarrow$ **Pagos**

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

* 📁 **src/**
  * 📁 `Company.ProjectName.API`: Capa de presentación, controladores y endpoints de entrada.
  * 📁 `Company.ProjectName.Application`: Casos de uso, interfaces, DTOs y lógica de negocio.
  * 📁 `Company.ProjectName.Domain`: Entidades principales, excepciones de dominio y lógica pura.
  * 📁 `Company.ProjectName.Infrastructure`: Implementación de base de datos, repositorios y clientes externos.

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
4. **Verificar Swagger:** Abre tu navegador e ingresa a `http://localhost:5000/swagger` (o el puerto configurado) para probar los endpoints.
