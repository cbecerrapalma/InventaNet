# InventaNet

Sistema de gestión de inventario y ventas en .NET 10: productos con variantes y presentaciones, stock por bodega, compras, ventas, precios y ofertas, clientes y proveedores, más una tienda en línea. La identidad (login, roles, permisos, tenants) la delega en [PassPlat](https://github.com/cbecerrapalma/PassPlat) y la base técnica es el framework [CBP](https://github.com/cbecerrapalma/CBP).

## Características

- **Catálogo**: productos, variantes y presentaciones, categorías, imágenes de producto y catálogos.
- **Inventario**: stock por bodega y movimientos.
- **Comercial**: compras, ventas, precios, ofertas y promociones.
- **Importación** de productos desde planillas Excel.
- **Backoffice** web (Blazor WebAssembly + MudBlazor) con menú según los permisos del usuario, selector de tenant y login con proveedores externos a través de PassPlat.
- **Tienda** pública (Blazor WebAssembly): catálogo, detalle de producto, login de clientes y contacto.
- **Multi-tenant**: toda consulta se filtra por el tenant del JWT emitido por PassPlat.
- **API REST** con ProblemDetails, correlación de peticiones, `/health` y Swagger.
- Formato chileno (es-CL) en fechas, números y moneda.

## Arquitectura

```text
Inventa.Web / Inventa.Store   (Blazor WASM)
        ↓ HTTP
Inventa.WebAPI                (ASP.NET Core, JWT de PassPlat)
        ↓
Inventa.Application.Async     (servicios, DTOs, validación, Result<T>)
        ↓
Inventa.Data.Async → Inventa.Data   (EF Core, repositorios, unidad de trabajo)
        ↓
Inventa.Domain                (entidades)
```

| Proyecto | Responsabilidad |
|---|---|
| `Inventa.Domain` | Entidades del negocio, sin dependencias de infraestructura |
| `Inventa.Data` | `DbContext`, configuraciones EF Core, repositorios |
| `Inventa.Data.Async` | Repositorios y acceso a datos asíncronos |
| `Inventa.Application` | Servicios síncronos (heredados) |
| `Inventa.Application.Async` | Servicios de aplicación, DTOs y validadores |
| `Inventa.WebAPI` | API HTTP, autenticación JWT, cliente de PassPlat, autorización |
| `Inventa.Web` | Backoffice: punto de venta, inventario, productos, precios, compras y ventas |
| `Inventa.Store` | Tienda en línea |
| `*.Tests` | Pruebas por capa: Application, Data, WebAPI, Web y Store (xUnit v3, Moq, bUnit) |

Los proyectos web nunca acceden a la base de datos: solo consumen `Inventa.WebAPI`.

## Tecnologías

C# · .NET 10 · ASP.NET Core · Blazor WebAssembly · MudBlazor 9.13.0 · Entity Framework Core 10.0.13 (SQL Server) · JWT Bearer · Swashbuckle 10.3 · EPPlus 8.7 · AutoMapper 17 · FluentValidation 12.2 · Fluid · xUnit v3 · Moq · bUnit

## Compilar

Requisito: SDK de .NET 10. Las tres soluciones van como carpetas hermanas, porque las referencias entre ellas son relativas:

```text
<carpeta>/
├── CBP/          https://github.com/cbecerrapalma/CBP
├── PassPlat/     https://github.com/cbecerrapalma/PassPlat
└── InventaNet/   este repositorio
```

```bash
dotnet build InventaNet.slnx
dotnet test --project Inventa.Application.Tests/Inventa.Application.Tests.csproj
```

InventaNet compila y sus pruebas corren sin PassPlat en ejecución. Para ejecutar la aplicación se necesitan SQL Server y la API de PassPlat. `Inventa.Data.Tests` usa una base SQL Server local.

## Configuración

Los valores locales (cadena de conexión con credenciales, claves JWT, proveedores de almacenamiento) van en `appsettings.Development.json` o en user-secrets, que no se publican. El `appsettings.json` del repositorio no trae secretos.

## Repositorios relacionados

- [CBP](https://github.com/cbecerrapalma/CBP): framework base
- [PassPlat](https://github.com/cbecerrapalma/PassPlat): identidad y acceso (IAM)

## Autor

**Claudio Becerra Palma** ([cbecerrapalma](https://github.com/cbecerrapalma)), desarrollador .NET / C#.

---

Este repositorio publica solo el código C# y los archivos de la solución .NET; documentación interna, scripts y datos quedan fuera.
