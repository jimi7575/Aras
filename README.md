# Aras Interview Task

یک Web API کوچک با .NET 8 که مشتریان را از API آماده ارس دریافت می‌کند، سفارش خرید/فروش ثبت می‌کند و با Hangfire کیف پول مشتریان را به شکل idempotent به‌روزرسانی می‌کند.

## اجرا

```bash
dotnet restore
dotnet ef database update --project Aras.Infrastructure/Aras.Infrastructure.csproj --startup-project Aras.Presentation/Aras.Presentation.csproj
dotnet run --project Aras.Presentation/Aras.Presentation.csproj
```

Swagger بعد از اجرا در این آدرس در دسترس است:

```text
http://localhost:5147/swagger
```

داشبورد Hangfire:

```text
http://localhost:5147/hangfire
```

## تنظیمات

تنظیمات پیش‌فرض در `Aras.Presentation/appsettings.json` قرار دارد:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=aras.db"
  },
  "ArasTrader": {
    "BaseUrl": "https://interview.arasetrader.ir/",
    "Username": "user2",
    "Password": "P@ssUs#r2"
  }
}
```

برای Docker یا محیط عملیاتی می‌توانید این مقادیر را با environment variable جایگزین کنید:

```bash
ArasTrader__Username=user2
ArasTrader__Password=P@ssUs#r2
ConnectionStrings__Default="Data Source=/data/aras.db"
```

## Docker

```bash
docker build -t aras-task .
docker run --rm -p 8080:8080 aras-task
```

Swagger داخل کانتینر:

```text
http://localhost:8080/swagger
```

## APIهای اصلی

نمونه درخواست‌ها در `Aras.Presentation/Aras.http` قرار دارد.

```http
POST /api/customers/sync
GET /api/customers
POST /api/orders/addorder
PUT /api/orders/editorder/{id}
GET /api/orders
GET /api/wallets/{customerId}
```

برای نوع سفارش از مقدارهای `Buy` و `Sell` استفاده کنید.

## طراحی

موجودیت‌ها:

- `Customer`: اطلاعات مشتری دریافت‌شده از API بیرونی، با `NationalCode` یکتا.
- `Wallet`: کیف پول یک‌به‌یک برای هر مشتری، شامل `Balance`.
- `Order`: سفارش مشتری با وضعیت‌های `Pending`، `Applied` و `Rejected`.
- `WalletTransaction`: اثر مالی سفارش روی کیف پول، با `OrderId` یکتا.

## توکن و API بیرونی

`ArasTraderClient` توکن را در حافظه cache می‌کند. اگر توکن معتبر موجود باشد از همان استفاده می‌شود؛ اگر توکن قبلی موجود باشد refresh انجام می‌شود؛ و اگر refresh به علت انقضا ناموفق شود، token جدید با username/password گرفته می‌شود. برای خطاهای موقت HTTP هم retry کوتاه در نظر گرفته شده است.

## همزمانی و Idempotency

ثبت و ویرایش سفارش فقط روی سفارش‌های `Pending` انجام می‌شود. Job کیف پول سفارش‌های `Pending` را داخل transaction اعمال می‌کند و برای هر سفارش دقیقا یک `WalletTransaction` با `OrderId` یکتا می‌سازد. بنابراین اگر Job چند بار اجرا شود یا دو worker همزمان به یک سفارش برسند، constraint یکتا و بررسی وضعیت سفارش مانع دوباره اعمال شدن مبلغ می‌شود.

در SQLite نوشتن‌ها در سطح دیتابیس serialize می‌شوند. برای مهاجرت به PostgreSQL همین مدل با transaction و unique constraint حفظ می‌شود و می‌توان روی row version یا lock سطح ردیف نیز تکیه کرد.

## معماری

ساختار پروژه بر اساس Clean Architecture چهارلایه است:

- `Aras.Domain`: موجودیت‌ها و enumهای دامنه، بدون وابستگی به لایه‌های دیگر.
- `Aras.Application`: DTOها، قراردادهای repository/unit-of-work/external client، Gateway و use caseها.
- `Aras.Infrastructure`: EF Core، migrationها، repositoryها، UnitOfWork، Hangfire storage و client API ارس.
- `Aras.Presentation`: Web API، controllerها، Swagger، Hangfire dashboard و composition root.
