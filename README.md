# Aras Interview Task

A small .NET 8 Web API that imports customers from the Aras Trader interview API, stores them in SQLite, accepts buy/sell orders, and applies wallet updates through an idempotent Hangfire job.

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- EF Core with SQLite
- Hangfire with SQLite storage
- Redis for external API token cache
- Docker / Docker Compose
- Swagger and `.http` request samples

## Run Locally

Redis is required because the external API token is cached in Redis.

```bash
dotnet restore
dotnet ef database update --project Aras.Infrastructure/Aras.Infrastructure.csproj --startup-project Aras.Presentation/Aras.Presentation.csproj
dotnet run --project Aras.Presentation/Aras.Presentation.csproj
```

Swagger:

```text
http://localhost:5147/swagger
```

Hangfire dashboard:

```text
http://localhost:5147/hangfire
```

## Run With Docker Compose

```bash
docker compose up --build
```

Swagger:

```text
http://localhost:8080/swagger
```

Hangfire dashboard:

```text
http://localhost:8080/hangfire
```

## Configuration

Default settings are in `Aras.Presentation/appsettings.json`.

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=aras.db;",
    "Redis": "redis:6379"
  },
  "ArasTrader": {
    "BaseUrl": "https://interview.arasetrader.ir/",
    "Username": "user2",
    "Password": "P@ssUs#r2"
  }
}
```

Development uses `Aras.Presentation/appsettings.Development.json`, where Redis points to `localhost:6379`.

Environment variable examples:

```bash
ArasTrader__Username=user2
ArasTrader__Password=P@ssUs#r2
ConnectionStrings__Default="Data Source=/data/aras.db;"
ConnectionStrings__Redis="redis:6379"
```

## Main APIs

Sample requests are available in `Aras.Presentation/Aras.http`.

```http
POST /api/customers/sync
GET /api/customers
POST /api/orders/addorder
PUT /api/orders/editorder/{id}
GET /api/orders
GET /api/wallets/{customerId}
```

Order side values:

```text
Buy
Sell
```

## Database

The database schema is managed by EF Core migrations in `Aras.Infrastructure/Migrations`.

SQLite files are local runtime artifacts and are ignored by git:

```text
Aras.Presentation/aras-dev.db
Aras.Presentation/aras.db
```

## External API Token Strategy

`ArasTraderClient` manages the external API token flow:

- If there is no cached token, it calls `POST /api/auth/token`.
- If a cached token exists and is near expiry, it calls `POST /api/auth/refresh`.
- The token is cached in Redis until its real expiry time.
- The client refreshes before expiry to reduce auth calls and avoid rate limits.
- Short retry handling is included for temporary HTTP failures.

If another client, such as Postman, creates an active token for the same user and this service does not have that token in Redis, the external API may return `409 active_token_exists`. In that case the service cannot refresh because the refresh endpoint requires the previous JWT.

## Domain Design

- `Customer`: imported from the external API and uniquely identified by `NationalCode`.
- `Wallet`: one-to-one with a customer and stores the current `Balance`.
- `Order`: buy/sell request with `Pending`, `Applied`, or `Rejected` status.
- `WalletTransaction`: immutable financial effect of an applied order. `OrderId` is unique for idempotency.

Domain entities use private setters and behavior methods such as `Order.Apply`, `Order.Reject`, and `Wallet.HasSufficientBalance` so state changes stay explicit.

## Gateway

`OrderGateway` is the application entry point for order registration. It delegates validation and persistence to `OrderService`, then enqueues the Hangfire wallet job after a new order is accepted. This keeps HTTP controllers thin and leaves room for future orchestration such as audit logging or event publishing.

## Hangfire Job And Idempotency

`WalletJob` periodically picks pending orders and applies them to wallets.

Idempotency is handled by:

- Processing only `Pending` orders.
- Rejecting insufficient-balance buy orders.
- Creating exactly one `WalletTransaction` per order.
- Enforcing a unique index on `WalletTransaction.OrderId`.

Running the job multiple times does not apply the same order twice.

## Race Condition Strategy

SQLite does not support row-level `SELECT FOR UPDATE` locking like PostgreSQL. To keep wallet updates safe, wallet balance updates are performed atomically in the database:

```sql
UPDATE Wallets
SET Balance = Balance + @signedAmount,
    UpdatedAtUtc = @now
WHERE Id = @walletId
  AND Balance + @signedAmount >= 0
```

If the update affects zero rows, the order is rejected. This prevents concurrent buy orders from driving a wallet balance below zero.

For a future PostgreSQL version, the repository boundary is the right place to switch to row-level locks such as `FOR UPDATE SKIP LOCKED`.
