# System Hotel Booking API

## Local setup

The API requires PostgreSQL and a JWT signing key. Keep credentials out of
`appsettings.json`; for local development, use .NET user-secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<postgresql-connection-string>" --project .\HotelBookingAPI\HotelBookingAPI.csproj
dotnet user-secrets set "Jwt:Key" "<random-secret-at-least-32-bytes>" --project .\HotelBookingAPI\HotelBookingAPI.csproj
```

In deployed environments, provide the same settings via the platform's secret
manager or environment variables:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`

The connection string must be an Npgsql connection string, for example:
`Host=db.example.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<password>;SSL Mode=Require`.

`Jwt:Issuer` and `Jwt:Audience` are non-secret settings in `appsettings.json`.
For a frontend hosted on a different origin, configure its exact origin(s) with
`Cors:AllowedOrigins` (environment variables use `Cors__AllowedOrigins__0`,
`Cors__AllowedOrigins__1`, and so on). Development allows the local Vite
origins on ports 8443 and 5173 by default.
If the database has no Admin user, configure a one-time bootstrap account using
`BootstrapAdmin:Username` and `BootstrapAdmin:Password` through a secret
provider. The username must be 3-50 characters and the password 12-100
characters. The API does not create an account with a built-in password.

Use a new database password and a newly generated JWT key for deployment.
Rotating the JWT key invalidates existing access tokens.

## Deploy to Render (free web service)

The repository includes a Render Blueprint at `../render.yaml` and a Dockerfile
for the API. In Render, create a new Blueprint from this repository and select
the free plan. The free web service may spin down while idle, so its first
request after inactivity can take longer.

The API now uses PostgreSQL. Create a Supabase project and provide its
PostgreSQL connection string in the
Blueprint prompt or the service's environment settings as
`ConnectionStrings__DefaultConnection`. Apply the EF Core migrations to that
database before the first deployment; the app does not apply migrations
automatically. In Supabase, select **Connect**, choose the **.NET** connection
string, and use the **Session pooler** if the direct database host is not
reachable over IPv4. Do not use the Supabase API URL or anon key as the
database connection string. The old SQL Server database is not copied; the
PostgreSQL database starts with the seed data defined in the application.
For example, from this directory, set the connection string in your local
PowerShell session and run:

```powershell
$env:ConnectionStrings__DefaultConnection = "<supabase-postgresql-connection-string>"
dotnet ef database update --project .\HotelBookingAPI\HotelBookingAPI.csproj
```

Use EF Core CLI version 8.0.10. If needed, install it with
`dotnet tool install --global dotnet-ef --version 8.0.10`.
The existing SQL Server migrations are retained as source history but excluded
from the PostgreSQL build; the new Supabase database starts empty and does not
copy data from the local SQL Server database.

Set the prompted `Jwt__Key` to a new random secret of at least 32 UTF-8 bytes.
For a new database, also set `BootstrapAdmin__Username` (3-50 characters) and
`BootstrapAdmin__Password` (12-100 characters); these are only needed when the
database has no Admin user. Set `Cors__AllowedOrigins__0` to the exact HTTPS
origin of the deployed frontend. Keep all secrets in Render's environment
settings, never in `render.yaml` or source control.

For the GitHub Pages frontend deployment, add the repository Actions variable
`VITE_API_BASE_URL` with the deployed API origin (for example,
`https://hotel-booking-api.onrender.com`). Set it under **Settings → Secrets and
variables → Actions → Variables**. The deployment workflow fails if this
variable is missing so it cannot publish a frontend whose API requests point
back to GitHub Pages.

## Payment confirmation flow

1. Search for rooms and get nightly prices with
   `GET /api/Bookings/available-rooms?checkIn=YYYY-MM-DD&checkOut=YYYY-MM-DD`.
2. The authenticated customer creates a booking. The server snapshots the
   applicable room, services, and discount prices into the booking.
3. The customer submits a payment claim:
   `POST /api/Bookings/{bookingId}/payment-claims`.
4. Staff/Admin review pending claims at
   `GET /api/PaymentClaims/pending`.
5. After verifying receipt of funds, the cashier confirms with
   `POST /api/Bookings/{bookingId}/payment-claims/{claimId}/confirm`; otherwise
   they reject it with `/reject` and provide a reason.
6. The customer and staff can view the payment ledger at
   `GET /api/Bookings/{bookingId}/payments`.

Customer confirmation alone does not count as money received. The API does not
currently integrate an online payment gateway.

## Build and tests

```powershell
dotnet build .\HotelBookingAPI.sln
dotnet test .\HotelBookingAPI.sln
```
