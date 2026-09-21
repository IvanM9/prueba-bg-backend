# Carrito de Compras — Backend (bg_backend)

API REST para la prueba técnica Full Stack: autenticación JWT, catálogo de productos,
carrito de compras y checkout transaccional con control de stock.

Stack: **.NET 10** · **EF Core 10 + Npgsql** · **PostgreSQL 17** · **JWT Bearer** · **Swashbuckle 10**.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version` → `10.x`)
- Docker Desktop (para PostgreSQL 17)
- Opcional: Rider o VS Code con extensión C#

## Cómo ejecutar

```bash
# 1. Base de datos
docker compose up -d postgres
# Alternativa si el contenedor ya existe:
docker start cartdb

# 2. API
dotnet run --launch-profile http
```

- La API escucha en **`http://localhost:5017`** (perfil `http` de
  `Properties/launchSettings.json`). Swagger: **`http://localhost:5017/swagger`**.
- Si arrancas la API de otra forma (p. ej. `ASPNETCORE_URLS=http://localhost:5000`),
  ajusta la URL del frontend en `src/environments/`.
- **Migraciones y seeds automáticos en Development**: al arrancar, `Program.cs`
  ejecuta `db.Database.MigrateAsync()` + `DbSeeder.SeedAsync(db)`. No necesitas
  `dotnet ef database update` para un arranque limpio; la DB se crea y se siembra sola.
- CORS ya permite `http://localhost:4200` con credenciales (cookies).

## Swagger

1. Abre `http://localhost:5017/swagger`.
2. Ejecuta `POST /api/auth/login` con un usuario semilla → copia el `token`.
3. Pulsa **Authorize**, pega el token (sin escribir `Bearer`, Swagger lo añade) → **Authorize**.
4. Ya puedes probar los endpoints protegidos desde la UI.

La API acepta el JWT de dos formas (ver `Program.cs` → `OnMessageReceived`):
`Authorization: Bearer <token>` **o** cookie `access_token` (el login la fija como
HttpOnly; el frontend Angular la usa vía `withCredentials`).

## Usuarios semilla

| Email | Password | Rol |
|---|---|---|
| `admin@tienda.com` | `Admin123!` | Admin |
| `cliente@tienda.com` | `Cliente123!` | Customer |



## Decisiones técnicas

- **PostgreSQL sobre SQL Server**: portabilidad y Docker nativo en cualquier OS
  (`postgres:17-alpine`, puerto host **5433**, db/usuario/clave `cartdb`/`postgres`/`postgres`).
- **Hash de passwords con PBKDF2-SHA256** (`Common/PasswordHasher.cs`, 100 000
  iteraciones, formato `iteraciones.salt.hash`): sin dependencias externas y con
  comparación en tiempo constante.
- **Descuento: 10 % solo si subtotal > 100.00 (estricto)**. `$100.00` exactos → `$0`;
  `$100.01` → `$10.00`. Calculado en backend en `CartService.BuildCartDto` y
  `OrderService.CheckoutAsync` (redondeo a 2 decimales). El frontend solo muestra.
- **Checkout con transacción `Serializable` + execution strategy de EF Core**
  (`OrderService.CheckoutAsync`): re-valida stock e inactividad **dentro** de la
  transacción; si hay colisión concurrente, Postgres aborta y EF reintenta la unidad completa.
- **Snapshots en `OrderItem`** (`ProductName`, `UnitPrice`, `LineSubtotal`): el
  historial no cambia aunque el producto cambie después.
- **Cálculos siempre en backend**: carrito y órdenes devuelven subtotal/descuento/total
  ya calculados; el cliente nunca recalcula (el `PUT` devuelve el `CartDto` completo).
- **JWT con claim de rol** (`ClaimTypes.Role`, `Admin`/`Customer`): preparado para
  autorización por roles (p. ej. un futuro `POST /api/products` con
  `[Authorize(Roles="Admin")]`). Expiración: 120 min. Además de `Bearer`, se acepta
  cookie HttpOnly `access_token` para el frontend.
- **Seguridad extra**: `GET /api/orders/{id}` filtra por `userId` (sin IDOR: la orden
  de otro usuario devuelve `404`); middleware global (`ExceptionHandlingMiddleware`)
  que convierte excepciones no controladas en `500 problem+json` sin filtrar detalles.

## Configuración

Desarrollo (`appsettings.Development.json`, solo local):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=cartdb;Username=postgres;Password=postgres"
  },
  "Jwt": { "Key": "DEV_ONLY_..." }
}
```
