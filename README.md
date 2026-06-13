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

La API estará disponible en `https://localhost:5111` y Swagger en `https://localhost:5111/swagger`.

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
🔒 Estrategia de concurrencia

Se usa bloqueo pesimista con SELECT FOR UPDATE en PostgreSQL al momento de debitar una cuenta. Esto hace que si dos operaciones llegan al mismo tiempo sobre la misma cuenta, una espera a que la otra termine — así el saldo nunca queda negativo ni inconsistente.
csharp

| SELECT * FROM accounts WHERE account_number = {accountNumber} FOR UPDATE | 

Se eligió este enfoque sobre el optimista porque en un sistema financiero es preferible prevenir la colisión que tener que reintentarla.

🔁 Estrategia de idempotencia

Cada transferencia lleva un IdempotencyKey. Si el cliente reintenta la misma operación (por ejemplo, por un timeout de red), el sistema detecta que ya fue procesada y devuelve el resultado original sin volver a ejecutarla.

|  var existing = await _unitOfWork.Transfers.GetByIdempotencyKeyAsync(request.IdempotencyKey);
if (existing != null)
    return existing.ToResponse(); | 


🛡️ Resiliencia ante caída del proveedor externo

El tipo de cambio se obtiene de HexaRate. Si el servicio no responde, el sistema no se cae — tiene tres capas de protección:

Timeout de 5 segundos para no esperar indefinidamente.
Caché de 30 minutos para no llamar al proveedor en cada operación.
Fallback automático a una tasa de 6.94 BOB/USD si el servicio falla, marcando la respuesta con isFallback: true.

---

## 🧪 Pruebas incluidas (29 en total)

### TransferServiceTests — 12 pruebas

| # | Prueba |

### TransferServiceTests — 10 pruebas
Transferencia USD → BOB multiplica por la tasa , Transferencia BOB → USD divide por la tasa , Misma moneda no llama al servicio de tipo de cambio , Saldo origen se descuenta correctamente ,Saldo destino se incrementa correctamente , Saldo insuficiente lanza ArgumentException , Idempotencia: reintento con misma clave no re-ejecuta ni guarda , Cuenta origen no encontrada lanza KeyNotFoundException9Cuenta destino no encontrada lanza KeyNotFoundException

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

