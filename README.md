# HexagonalAPI

API ASP.NET Core (.NET 11) con arquitectura hexagonal para administrar productos (CRUD + búsqueda + imagen), con SQLite y Swagger.

## Ejecutar

```bash
dotnet run --project HexagonalAPI.Api --launch-profile http
```

Swagger UI: <http://localhost:5183/swagger> — la base se crea sola en `Data/productos.db` (relativo a la carpeta desde donde se ejecuta).

## Arquitectura (solución con 4 proyectos)

```
HexagonalAPI.Api ──► HexagonalAPI.Application ──► HexagonalAPI.Domain
        │                                              ▲
        └──► HexagonalAPI.Infrastructure ──────────────┘
                     (también implementa los puertos de Application)
```

| Proyecto | Rol |
|---|---|
| `HexagonalAPI.Domain` | Entidad `Producto`, `ImagenProducto`, regla del Id (`GeneradorIdProducto`), excepciones del dominio. No depende de nada. |
| `HexagonalAPI.Application` | Puerto de entrada (`IProductoService`), puertos de salida (`IProductoRepository`, `IProductoImagenRepository`) y los casos de uso (`ProductoService`). Solo depende de Domain. |
| `HexagonalAPI.Infrastructure` | Adaptador de salida: conexión SQLite, creación de tablas y repositorios con `Microsoft.Data.Sqlite`. Implementa los puertos de Application. |
| `HexagonalAPI.Api` | Adaptador de entrada: controladores, contratos HTTP, manejo de errores, Swagger y la raíz de composición (`Program.cs`, donde se conectan puertos con adaptadores). |

Domain y Application no conocen ASP.NET ni SQLite; Infrastructure no conoce a Api.

## Regla de negocio del Id

`PROD-{1ª y 3ª letra del nombre}-{4 caracteres a-zA-Z0-9 aleatorios}-{últimos 2 dígitos del año}`, p. ej. `Laptop` -> `PROD-LP-aB3x-26`.
Solo cuentan las letras A-Z del nombre (los acentos se normalizan: `Árbol` -> `AB`); se necesitan al menos 3.
El Id no cambia si luego se edita el nombre.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/productos` | Listar |
| GET | `/api/productos/buscar?termino=` | Buscar por Id, nombre o descripción |
| GET | `/api/productos/{id}` | Obtener por Id |
| POST | `/api/productos` | Crear |
| PUT | `/api/productos/{id}` | Actualizar |
| DELETE | `/api/productos/{id}` | Eliminar (borra su imagen en cascada) |
| PUT | `/api/productos/{id}/imagen` | Subir/reemplazar imagen (multipart, campo `archivo`; PNG/JPEG/GIF/WebP, máx. 2 MB) |
| GET | `/api/productos/{id}/imagen` | Descargar imagen |
| DELETE | `/api/productos/{id}/imagen` | Quitar imagen |

Las imágenes se guardan como BLOB en la tabla `ProductoImagenes` de SQLite. El tipo se detecta por los bytes del archivo, no por el `Content-Type` que envíe el cliente.
