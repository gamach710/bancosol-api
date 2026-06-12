\# BancoSol API



API RESTful para gestión de billeteras (cuentas) en múltiples monedas con transferencias, conversión de tipo de cambio y reporte consolidado de saldos.



\---



\## Tecnologías



\- \*\*.NET 10\*\* — ASP.NET Core Web API

\- \*\*PostgreSQL\*\* — Base de datos relacional

\- \*\*Entity Framework Core\*\* — ORM con migraciones versionadas

\- \*\*Serilog\*\* — Logging estructurado

\- \*\*FluentValidation\*\* — Validaciones de entrada

\- \*\*xUnit + Moq\*\* — Pruebas unitarias

\- \*\*Swagger / OpenAPI\*\* — Documentación de la API

\- \*\*JWT Bearer\*\* — Autenticación y autorización



\---



\## URLs Públicas



| Recurso | URL |

|---|---|

| API Base | `https://bancosol-api-production.up.railway.app` |

| Swagger UI | `https://bancosol-api-production.up.railway.app/swagger` |



\---



\## Credenciales de prueba



Para acceder a los endpoints protegidos, primero obtener un token JWT:



```

POST /api/auth/login

```



```json

{

&#x20; "username": "admin",

&#x20; "password": "Admin123!"

}

```



Usar el token en Swagger haciendo clic en \*\*Authorize\*\* 🔒 e ingresando:



```

Bearer <token>

```



\---



\## Requisitos previos (ejecución local)



\- \[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

\- \[PostgreSQL 15+](https://www.postgresql.org/download/)



\---



\## Configuración local



\*\*1. Clonar el repositorio\*\*



```bash

git clone https://github.com/gamach710/bancosol-api.git

cd bancosol-api

```



\*\*2. Configurar la cadena de conexión\*\*



Editar `bancoSol/appsettings.Development.json`:



```json

{

&#x20; "ConnectionStrings": {

&#x20;   "DefaultConnection": "Host=localhost;Port=5432;Database=bancosol\_core\_db;Username=postgres;Password=tu\_password"

&#x20; },

&#x20; "Jwt": {

&#x20;   "Key": "tu\_clave\_secreta\_muy\_larga",

&#x20;   "Issuer": "bancosol",

&#x20;   "Audience": "bancosol"

&#x20; }

}

```



\*\*3. Ejecutar migraciones\*\*



```bash

cd bancoSol

dotnet ef database update

```



\*\*4. Ejecutar el proyecto\*\*



```bash

dotnet run

```



La API estará disponible en `https://localhost:7001` y Swagger en `https://localhost:7001/swagger`.



\---



\## Ejecutar pruebas



```bash

cd BancoSol.Tests

dotnet test

```



Resultado esperado: \*\*29 pruebas, 0 errores\*\*.



\---



\## Endpoints



| Método | Ruta | Descripción |

|---|---|---|

| POST | `/api/auth/login` | Obtener token JWT |

| POST | `/api/accounts` | Crear cuenta |

| GET | `/api/accounts/{id}` | Obtener cuenta por ID |

| GET | `/api/accounts/number/{accountNumber}` | Obtener cuenta por número |

| GET | `/api/accounts/customer/{customerId}` | Cuentas por cliente |

| POST | `/api/accounts/{accountNumber}/deposit` | Realizar depósito |

| POST | `/api/accounts/{accountNumber}/withdraw` | Realizar retiro |

| GET | `/api/accounts/{accountNumber}/transactions` | Historial paginado |

| POST | `/api/transfers` | Transferencia entre cuentas |

| GET | `/api/exchange-rate` | Tipo de cambio vigente USD/BOB |

| GET | `/api/reports/consolidated-balance` | Reporte consolidado de saldo |



\---



\## Decisiones técnicas



\### Arquitectura



Se adoptó una arquitectura en capas con separación clara de responsabilidades:



\- \*\*Controllers\*\* — Reciben y responden peticiones HTTP

\- \*\*Services\*\* — Contienen la lógica de negocio

\- \*\*Repositories\*\* — Acceso a datos

\- \*\*Unit of Work\*\* — Coordina múltiples repositorios en una transacción

\- \*\*DTOs / Mappers\*\* — Separación entre modelos de dominio y contratos de API



\### Estrategia de concurrencia



Se implementó \*\*bloqueo pesimista a nivel de base de datos\*\* mediante `SELECT FOR UPDATE` al obtener cuentas para operaciones de débito. Esto garantiza que operaciones simultáneas sobre la misma cuenta no puedan dejar el saldo negativo ni corromper el historial, ya que PostgreSQL serializa el acceso fila por fila.



```csharp

// El método GetByAccountNumberForUpdateAsync aplica FOR UPDATE

context.Accounts

&#x20;   .FromSqlRaw("SELECT \* FROM accounts WHERE account\_number = {0} FOR UPDATE", accountNumber)

&#x20;   .FirstOrDefaultAsync();

```



\### Estrategia de idempotencia



Las transferencias aceptan un campo `IdempotencyKey`. Antes de procesar cualquier transferencia, el sistema consulta si ya existe una con esa clave. Si existe, devuelve el resultado original sin volver a ejecutar la operación. Esto permite reintentos seguros desde el cliente.



\### Estrategia de resiliencia (tipo de cambio externo)



El servicio de tipo de cambio integra la API de HexaRate (`https://hexarate.paikama.co`). Se implementaron tres capas de resiliencia:



1\. \*\*Timeout configurado\*\* de 5 segundos en el `HttpClient`

2\. \*\*Caché en memoria\*\* del tipo de cambio con TTL razonable para evitar llamadas innecesarias

3\. \*\*Fallback automático\*\* a una tasa predeterminada si el servicio externo no responde, marcando la respuesta con `IsFallbackRate: true`



La API nunca cae por un proveedor externo caído.



\### Política de redondeo



Todos los montos monetarios se manejan con tipo `decimal` y se redondean a \*\*2 decimales\*\* usando `Math.Round(amount, 2)` antes de persistir.



\### Atomicidad



Entity Framework Core con PostgreSQL garantiza atomicidad mediante transacciones implícitas en `SaveChangesAsync()`. Si cualquier paso de una transferencia o depósito falla, EF hace rollback automático y ningún cambio queda aplicado parcialmente.



\---



\## Estructura del proyecto



```

bancosol-api/

├── bancoSol/

│   ├── Controllers/

│   ├── Services/

│   │   ├── Interfaces/

│   │   └── Implementations/

│   ├── Repositories/

│   │   ├── Interfaces/

│   │   └── Implementations/

│   ├── Models/

│   ├── DTOs/

│   ├── Mappers/

│   ├── Validators/

│   ├── Middleware/

│   ├── UnitOfWork/

│   ├── Constants/

│   ├── Data/

│   └── Migrations/

├── BancoSol.Tests/

│   ├── TransferServiceTests.cs

│   ├── ReportServiceTests.cs

│   └── AccountServiceTests.cs

├── Dockerfile

├──  README.md

└── bancoSol.slnx

```



\---



\## Pruebas incluidas



\### TransferServiceTests (12 pruebas)

\- Conversión USD → BOB multiplica por la tasa

\- Conversión BOB → USD divide por la tasa

\- Misma moneda no llama al servicio de tipo de cambio

\- Saldo origen se descuenta correctamente

\- Saldo destino se incrementa correctamente

\- Saldo insuficiente lanza `ArgumentException`

\- Idempotencia no ejecuta dos veces ni guarda

\- Cuenta origen no encontrada lanza `KeyNotFoundException`

\- Cuenta destino no encontrada lanza `KeyNotFoundException`

\- Cuenta origen inactiva lanza `InvalidOperationException`

\- Cuenta destino bloqueada lanza `InvalidOperationException`

\- Transferencia exitosa guarda 1 Transfer + 2 Transactions



\### ReportServiceTests (7 pruebas)

\- Reporte en BOB convierte y suma todos los tipos de transacción

\- Reporte en USD convierte BOB correctamente

\- Moneda en minúsculas se normaliza a mayúsculas

\- Fechas fuera de rango devuelven balance 0

\- EndDate es inclusivo

\- Tasa fallback se refleja en la respuesta

\- Tipo de cambio se consulta exactamente una vez



\---



\## Seguridad



Los endpoints están protegidos con \*\*JWT Bearer Authentication\*\*. Para probarlos:



1\. Hacer `POST /api/auth/login` con las credenciales de administrador

2\. Copiar el token de la respuesta

3\. En Swagger, clic en \*\*Authorize\*\* e ingresar `Bearer <token>`

4\. Todos los endpoints quedan habilitados durante la sesión



\---



\## Despliegue



La API está desplegada en \*\*Railway\*\* con:



\- Detección automática de cambios vía GitHub (CI/CD automático)

\- Base de datos PostgreSQL en Railway

\- Migraciones ejecutadas automáticamente al iniciar

\- Variables de entorno configuradas en Railway (no hay secrets en el repositorio)

