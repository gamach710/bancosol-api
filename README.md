# BancoSol API — Sistema de Billetera y Transferencias Multimoneda

API RESTful desarrollada en **.NET 10** para gestionar billeteras (cuentas) en Bolivianos (BOB) y Dólares (USD), con soporte de depósitos, retiros, transferencias multimoneda, historial paginado y reporte consolidado de saldos.

---

## 🌐 URLs Públicas

| Recurso | URL |
|---|---|
| API Base | `https://bancosol-api-production.up.railway.app` |
| Swagger UI | `https://bancosol-api-production.up.railway.app/swagger` |

---

## 🔑 Credenciales de prueba

Para acceder a los endpoints protegidos, primero obtener un token JWT:

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "bancosol123"
}
```

Usar el token en Swagger haciendo clic en **Authorize** 🔒 e ingresando:

```
Bearer <token>
```

---

## ✅ Requisitos previos (ejecución local)

| Herramienta | Versión mínima | Enlace |
|---|---|---|
| .NET SDK | 10.0 | https://dotnet.microsoft.com/download/dotnet/10.0 |
| PostgreSQL | 15+ | https://www.postgresql.org/download/ |
| EF Core CLI | (incluido con .NET SDK) | `dotnet tool install --global dotnet-ef` |

---

## ⚙️ Configuración local

**1. Clonar el repositorio**

```bash
git clone https://github.com/gamach710/bancosol-api.git
cd bancosol-api
```

**2. Configurar la cadena de conexión**

Editar `bancoSol/appsettings.Development.json` con tus datos de PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bancosol_core_db;Username=postgres;Password=tu_password"
  },
  "Jwt": {
    "Key": "BancoSol$SuperSecretKey#2026!MustBe32CharsMin",
    "Issuer": "BancoSolAPI",
    "Audience": "BancoSolClients",
    "ExpirationMinutes": 60
  },
  "AdminCredentials": {
    "Username": "admin",
    "Password": "bancosol123"
  }
}
```

**3. Aplicar migraciones de base de datos**

```bash
cd bancoSol
dotnet ef database update
```

Las migraciones crean automáticamente las tablas `accounts`, `customers`, `transactions`, `transfers` y `parameters`. Al arrancar, el sistema inicializa el parámetro de numeración de cuentas si no existe.

**4. Ejecutar el proyecto**

```bash
dotnet run
```

La API estará disponible en `https://localhost:7001` y Swagger en `https://localhost:7001/swagger`.

---

## 🧪 Ejecutar pruebas

```bash
cd BancoSol.Tests
dotnet test
```

Resultado esperado: **29 pruebas, 0 errores**.

Para ver detalle de cada prueba:

```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 🐳 Docker (opcional)

```bash
# Desde la raíz del repositorio
docker build -t bancosol-api .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=bancosol_core_db;Username=postgres;Password=tu_password" \
  -e Jwt__Key="BancoSol$SuperSecretKey#2026!MustBe32CharsMin" \
  -e AdminCredentials__Username="admin" \
  -e AdminCredentials__Password="bancosol123" \
  bancosol-api
```

---

## 📡 Endpoints

| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| POST | `/api/auth/login` | Obtener token JWT | ❌ |
| POST | `/api/accounts` | Crear cuenta (BOB o USD) | ✅ |
| GET | `/api/accounts/{accountNumber}` | Obtener cuenta por número | ✅ |
| POST | `/api/accounts/{accountNumber}/deposits` | Realizar depósito | ✅ |
| POST | `/api/accounts/{accountNumber}/withdrawals` | Realizar retiro | ✅ |
| GET | `/api/accounts/{accountNumber}/transactions` | Historial paginado de movimientos | ✅ |
| POST | `/api/transfers` | Transferencia entre cuentas (con idempotencia) | ✅ |
| GET | `/api/exchange-rate` | Tipo de cambio vigente USD/BOB | ✅ |
| GET | `/api/reports/consolidated-balance` | Reporte consolidado de saldo por período | ✅ |

---

## 🏗️ Estructura del proyecto

```
bancosol-api/
├── bancoSol/                         # Proyecto principal ASP.NET Core
│   ├── Controllers/                  # Controladores HTTP
│   │   ├── AccountController.cs
│   │   ├── AuthController.cs
│   │   ├── ExchangeRateController.cs
│   │   ├── ReportController.cs
│   │   └── TransferController.cs
│   ├── Services/
│   │   ├── Interfaces/               # Contratos de servicio
│   │   └── Implementations/          # Lógica de negocio
│   │       ├── AccountService.cs
│   │       ├── TransferService.cs
│   │       ├── ExchangeRateService.cs
│   │       ├── ReportService.cs
│   │       └── TokenService.cs
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   └── Implementations/          # Acceso a datos (EF Core)
│   │       └── AccountRepository.cs  # Incluye SELECT FOR UPDATE
│   ├── UnitOfWork/                   # Patrón Unit of Work
│   ├── Models/                       # Entidades de dominio
│   ├── DTOs/                         # Contratos de entrada/salida
│   ├── Mappers/                      # Conversión entidad ↔ DTO
│   ├── Validators/                   # Validaciones FluentValidation
│   ├── Middleware/                   # ExceptionMiddleware centralizado
│   ├── Migrations/                   # Migraciones EF Core versionadas
│   ├── Constants/                    # Enums: Currency, AccountStatus, TransactionType
│   └── Data/                         # ApplicationDbContext
├── BancoSol.Tests/                   # Proyecto de pruebas xUnit + Moq
│   ├── TransferServiceTests.cs       # 12 pruebas
│   ├── AccountServiceTests.cs        # 10 pruebas
│   └── ReportServiceTests.cs         # 7 pruebas
├── Dockerfile
├── README.md
└── bancoSol.slnx
```

---

## 🔧 Tecnologías utilizadas

| Tecnología | Uso |
|---|---|
| .NET 10 / ASP.NET Core | Framework principal |
| PostgreSQL 15+ | Base de datos relacional |
| Entity Framework Core | ORM con migraciones versionadas |
| FluentValidation | Validaciones de entrada |
| Serilog | Logging estructurado (consola + archivo diario) |
| xUnit + Moq | Pruebas unitarias |
| Swagger / OpenAPI | Documentación interactiva |
| JWT Bearer | Autenticación y autorización |
| IMemoryCache | Caché en memoria |

---

## 🧠 Decisiones técnicas

### Arquitectura en capas

Se adoptó una arquitectura en capas con separación explícita de responsabilidades:

- **Controllers** — reciben y responden peticiones HTTP; no contienen lógica de negocio.
- **Services** — contienen toda la lógica de negocio; orquestan repositorios.
- **Repositories** — acceso a datos mediante EF Core; un repositorio por entidad.
- **Unit of Work** — coordina múltiples repositorios en una única transacción de base de datos.
- **DTOs / Mappers** — desacoplan el modelo de dominio del contrato HTTP.
- **Validators** — FluentValidation valida entradas antes de llegar al servicio.

### Manejo de dinero

Todos los montos se manejan con tipo `decimal` (exacto) y se redondean a **2 decimales** antes de persistir, evitando errores de punto flotante en operaciones financieras.

### Códigos HTTP

| Código | Cuándo se devuelve |
|---|---|
| 200 OK | Operación exitosa |
| 201 Created | Cuenta creada |
| 400 Bad Request | Validación fallida, saldo insuficiente, moneda no soportada |
| 401 Unauthorized | Token ausente o inválido |
| 404 Not Found | Cuenta o recurso no encontrado |
| 409 Conflict | Clave de idempotencia duplicada / conflicto de concurrencia |
| 500 Internal Server Error | Error inesperado del sistema |

---

## 🔒 Estrategia de concurrencia

Se implementó **bloqueo pesimista a nivel de base de datos** (`SELECT FOR UPDATE`) al obtener cuentas para operaciones de débito (retiros y transferencias).

```csharp
// AccountRepository.cs
return await _context.Accounts
    .FromSqlInterpolated($@"
        SELECT * FROM accounts
        WHERE account_number = {accountNumber}
        FOR UPDATE")
    .FirstOrDefaultAsync();
```

**Por qué bloqueo pesimista:**  
En un sistema financiero las colisiones de escritura son frecuentes (múltiples transferencias simultáneas sobre la misma cuenta). El bloqueo pesimista serializa el acceso fila a fila en PostgreSQL, garantizando que ninguna operación concurrente pueda dejar el saldo negativo ni corromper el historial. Se eligió sobre bloqueo optimista porque el costo de reintentos en caso de colisión es mayor que el costo del lock preventivo.

Además, todas las operaciones de transferencia se realizan dentro de una única transacción de EF Core (`SaveChangesAsync`), lo que garantiza atomicidad: si cualquier paso falla, se hace ROLLBACK completo.

---

## 🔁 Estrategia de idempotencia

Las transferencias aceptan un campo obligatorio `IdempotencyKey` en el body del request. Antes de procesar cualquier transferencia, el sistema consulta si ya existe una con esa clave:

```csharp
// TransferService.cs
var existing = await _unitOfWork.Transfers.GetByIdempotencyKeyAsync(request.IdempotencyKey);
if (existing != null)
    return existing.ToResponse(); // Devuelve el resultado original sin re-ejecutar
```

**Comportamiento:**
- Si la clave no existe → se procesa la transferencia normalmente.
- Si la clave ya existe → se devuelve el resultado de la operación original sin debitar ni acreditar nuevamente.
- La clave queda persistida en la tabla `transfers` junto a la operación.

Esto permite reintentos seguros desde el cliente (ej. timeout de red) sin duplicar efectos.

---

## 🛡️ Estrategia de resiliencia ante caída del proveedor externo

El tipo de cambio USD/BOB se obtiene de la API pública de HexaRate (`https://hexarate.paikama.co/api/rates/USD/BOB/latest`). Se implementaron tres capas de resiliencia:

**1. Timeout configurado**

```csharp
// Program.cs
builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
    client.BaseAddress = new Uri("https://hexarate.paikama.co");
});
```

**2. Caché en memoria (TTL 30 minutos)**

Si el tipo de cambio ya fue consultado, se devuelve desde caché sin llamar al proveedor externo:

```csharp
if (_cache.TryGetValue(CacheKey, out ExchangeRateResponse? cached) && cached != null)
    return cached;
// ... llamada HTTP ...
_cache.Set(CacheKey, rate, TimeSpan.FromMinutes(30));
```

**3. Fallback automático**

Si el proveedor externo no responde (timeout, error de red, 5xx), el sistema usa una tasa predeterminada (`6.94 BOB/USD`) y continúa operando. La respuesta incluye `"isFallback": true` para que el cliente sepa que se usó la tasa de respaldo. El fallback también se cachea por 5 minutos para no reintentar constantemente:

```csharp
catch (Exception ex)
{
    _logger.LogWarning(ex, "API de tipo de cambio no disponible, usando tasa de fallback: {Rate}", FallbackRate);
    var fallback = new ExchangeRateResponse { Rate = FallbackRate, IsFallback = true };
    _cache.Set(CacheKey, fallback, TimeSpan.FromMinutes(5));
    return fallback;
}
```

**La API nunca se cae porque el proveedor externo esté caído.**

---

## 🧪 Pruebas incluidas (29 en total)

### TransferServiceTests — 12 pruebas

| # | Prueba |
|---|---|
| 1 | Transferencia USD → BOB multiplica por la tasa |
| 2 | Transferencia BOB → USD divide por la tasa |
| 3 | Misma moneda no llama al servicio de tipo de cambio |
| 4 | Saldo origen se descuenta correctamente |
| 5 | Saldo destino se incrementa correctamente |
| 6 | Saldo insuficiente lanza `ArgumentException` |
| 7 | Idempotencia: reintento con misma clave no re-ejecuta ni guarda |
| 8 | Cuenta origen no encontrada lanza `KeyNotFoundException` |
| 9 | Cuenta destino no encontrada lanza `KeyNotFoundException` |
| 10 | Cuenta origen inactiva lanza `InvalidOperationException` |
| 11 | Cuenta destino bloqueada lanza `InvalidOperationException` |
| 12 | Transferencia exitosa guarda 1 Transfer + 2 Transactions |

### AccountServiceTests — 10 pruebas

Validan: moneda no soportada (EUR), retiro con saldo insuficiente, depósito en cuenta inactiva, cuenta no encontrada, validaciones de monto, entre otros.

### ReportServiceTests — 7 pruebas

Validan: reporte en BOB convierte correctamente, reporte en USD, moneda en minúsculas se normaliza, fechas fuera de rango devuelven 0, EndDate inclusivo, tasa fallback reflejada, tipo de cambio consultado exactamente una vez.

---

## 🔐 Seguridad

Los endpoints están protegidos con **JWT Bearer Authentication**. Para probarlos:

1. `POST /api/auth/login` con las credenciales de administrador
2. Copiar el token de la respuesta
3. En Swagger, clic en **Authorize** e ingresar `Bearer <token>`
4. Todos los endpoints quedan habilitados durante la sesión (60 minutos)

---

## 🚀 Despliegue

La API está desplegada en **Railway** con:

- CI/CD automático via GitHub (cada push a `main` despliega)
- Base de datos PostgreSQL gestionada en Railway
- Migraciones EF Core ejecutadas automáticamente al iniciar (`context.Database.Migrate()`)
- Variables de entorno configuradas en Railway (no hay secrets en el repositorio)
- Puerto configurable via variable de entorno `PORT` (por defecto 8080)

