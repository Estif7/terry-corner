# Terry Corner — MADE FOR THE CRAVE

Full-stack burger ordering platform: Angular customer app + ASP.NET Core Clean
Architecture API + EF Core + off-system payment/receipt workflow + admin dashboard.

## Repo layout

```
terry-corner/
├── terry-corner-client/   Angular 18 standalone app
└── server/                .NET 10 Clean Architecture solution
    ├── TerryCorner.sln
    ├── src/
    │   ├── TerryCorner.Domain          entities, enums — no dependencies
    │   ├── TerryCorner.Application     MediatR commands/handlers, validators, interfaces
    │   ├── TerryCorner.Infrastructure  EF Core, Identity, JWT, DbContext
    │   └── TerryCorner.Api             controllers, Program.cs, startup project
    └── tests/
        ├── TerryCorner.UnitTests
        └── TerryCorner.IntegrationTests
```

## Status

**Phase 1 (Foundation):** solution/project scaffolding, theme system, base layout,
Domain entities, EF Core configurations, ASP.NET Identity + JWT auth
(register/login/refresh/logout with refresh-token rotation and reuse detection),
global exception handling (RFC 7807 ProblemDetails), Swagger, CORS, Serilog.

**Phase 2 (Customer Experience — frontend):** home page (hero, today's special,
featured burgers, why-us, CTA), menu page (category filter/search/sort), product
customization modal (quantity + toppings + real-time pricing), cart page.

**Phase 2 (backend catalog read endpoints, done in parallel so the frontend above
has real data):** `GET /api/products` (category/search/featured filters),
`GET /api/products/{id}`, `GET /api/categories`, `GET /api/promotions/active`,
plus a dev-only `DbSeeder` that seeds roles, an admin user
(`admin@terrycorner.local` / `ChangeMe123!` — **rotate this in any shared
environment**), 5 categories, 7 toppings, 7 products, 2 payment methods, and one
active promotion.

**Phase 3 (Ordering — backend):** `POST /api/orders` (server-side price
recalculation — client-supplied prices are never trusted; validates product/
topping availability), `GET /api/orders/{orderNumber}` (tracking timeline),
`POST /api/orders/{orderNumber}/receipts` (multipart file upload with
extension/MIME/size validation, safe server-generated filenames, duplicate-
submission prevention), `GET /api/payment-methods/active`. Order status model
matches the spec's tracking timeline exactly (`PaymentPending` →
`PaymentReceiptSubmitted` → `PaymentVerification` on receipt upload).

Not yet built: staff receipt approval → auto-transition to `PreparingFood`,
SignalR live updates, and admin dashboard. Those are Phase 4 / 5.

**Phase 3 (Ordering — frontend):** checkout form (contact info, pickup/delivery,
notes, client + server validation), payment-instructions page (pulls active
payment methods, shows amount due), receipt-upload page (file type/size
validation client-side, mirrors server rules, submission confirmation), and
the order-tracking page now shows a real timeline (✓/●/○ markers — never
color alone) wired to `GET /api/orders/{orderNumber}`.

**Phase 4/5 (staff + admin):** SignalR (`/hubs/orders`) for live tracking updates;
staff payment-receipt review/approve/reject with the critical auto-transition to
`PreparingFood`; admin dashboard (Overview stats, Orders list/detail/manual
status, Payment Receipts queue) and full catalog CRUD (Products, Categories,
Toppings, Payment Settings, Promotions), all role-restricted.

**Account management:** register/login/logout, profile view/edit, order history
with reorder, and — this round — **admin user management**: list all users with
role/lockout/join-date, edit any user's profile (including email), restrict/
unrestrict (locks out sign-in immediately and revokes active sessions), and a
user-stats panel (totals by role, restricted count, new signups in the last 7
days). Also added: optional profile pictures (upload/change/remove, JPG/PNG,
2 MB limit, same safe-filename/no-public-URL pattern as payment receipts), and
staff-level users now land on `/admin/overview` after login with a **Dashboard**
link in the nav.

**This round requires a second EF Core migration** — `ApplicationUser` gained
two new columns (`ProfilePictureFileName`, `CreatedAtUtc`). See the new step 4a
below.

**Role model (simplified):** two operational roles — **Admin** (everything,
including user management) and **Manager** (all restaurant operations: orders,
receipts, catalog, promotions — no access to Users, enforced both in the
backend `[Authorize]` attributes and hidden/route-guarded in the frontend).
`Customer` still exists for self-registered accounts. The `Staff` role/enum
value is still defined in code but no longer used anywhere — harmless, just
inert. Only the seeded `admin@terrycorner.local` account can create new
**Manager** accounts (`POST /api/admin/users/managers` — name/email/password,
role is always `Manager`, never self-registrable) or change an existing user's
role (`PATCH /api/admin/users/{id}/role`, among `Customer`/`Manager`/`Admin`;
you can't change your own role, same as you can't restrict yourself).

**This was written in a sandbox without the .NET SDK or NuGet access, so the
backend has not been compiled. Follow the steps below on your machine — that's
the first real build.**

## Fixes applied since the last zip

**This zip is meant as a fresh baseline** — extract it into a brand-new empty
folder, not on top of an old one. Your working folder had accumulated enough
drift from stale/skipped file overwrites (a missing project reference, a lost
migration file) that patching further wasn't reliable anymore. Nothing needs
to be carried over from the old folder; regenerate `Migrations/` and
`dotnet user-secrets` fresh here (see Setup below — your JWT secret may
already be there since secrets are stored per-project outside the folder, but
verify with `dotnet user-secrets list`).

If you patched a previous zip by hand, this version already includes those
fixes — you can discard your local patches and start from this zip.

- **`IdentityResult` ambiguity (properly this time)** — my previous fix only
  aliased the framework type for one usage inside the method body; the return
  type and the `new IdentityResult(...)` calls were still bare, which stays
  ambiguous as long as both `Microsoft.AspNetCore.Identity` and
  `TerryCorner.Application.Common.Interfaces` are imported in the same file —
  an alias doesn't remove the original name from scope, it just adds another
  one. Fixed by aliasing *our* DTO instead (`AppIdentityResult`) and using that
  alias for every reference in `IdentityService.cs`, so there's no bare
  `IdentityResult` left in the file at all.
- **Target framework** — every project now targets `net10.0` (was `net8.0`),
  matching the SDK you actually have installed.
- **Database provider** — now **PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL`
  (started as SQL Server, briefly SQLite for zero-install local dev, now
  PostgreSQL per your choice — closer to what you'd actually deploy), pointed
  at your existing local Postgres install. (A `docker-compose.yml` is also
  included at `server/` in case you ever want a disposable Postgres instance
  instead — not required, and not part of the setup steps below.)
- **Package versions** — all EF Core / Identity / JwtBearer packages bumped to
  the `10.0.0` line to match the retargeted TFM, including
  `Microsoft.Extensions.Logging.Abstractions` in `TerryCorner.Application`
  (this one was missed in the previous zip and caused a `NU1605` package
  downgrade error — every project referencing an `Microsoft.Extensions.*` or
  `Microsoft.EntityFrameworkCore.*` package now stays on the same `10.0.0`
  line so this class of error can't recur); `System.IdentityModel.Tokens.Jwt`
  bumped to `8.19.2`.
- **Angular API URL** — `environment.ts` now points at
  `http://localhost:5000/api` to match the `http` launch profile (no HTTPS dev
  cert needed).

The `Products`/`Categories`/`Promotions` controllers were already in the zip
you had (`phase2b`) — your build just never got far enough to run them because
of the `IdentityResult` compile error above. Once you rebuild with this
version, `/api/categories` and `/api/products` should already return the
seeded data.

## Setup — run these one at a time

The database is **PostgreSQL**, using the instance you already have installed.

0. Create the database and confirm you can reach it. Using `psql` (or pgAdmin,
   if you'd rather click through it):
   ```
   psql -U postgres -h localhost -c "CREATE DATABASE terrycorner;"
   ```
   Expected: `CREATE DATABASE`. If `psql` prompts for a password, that's your
   existing Postgres superuser password — not a new one to invent.

   Then set the connection string as a user secret (step 3 below covers the
   command) using **your actual Postgres username/password/port** — the
   `appsettings.Development.json` in this zip has placeholder credentials
   (`postgres`/`postgres` on port `5432`) that almost certainly don't match
   your local install, so don't rely on them silently working.

1. Confirm your .NET SDK version:
   ```
   dotnet --version
   ```
   Expected: a version starting with `10.` (this project targets `net10.0` to
   match).

2. Restore and build the backend:
   ```
   cd server
   dotnet restore
   dotnet build
   ```
   Expected: `Build succeeded.` with `0 Error(s)`.

3. Set the JWT signing key as a user secret (never commit a real key to
   `appsettings.Development.json`):
   ```
   cd src/TerryCorner.Api
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:SigningKey" "a-long-random-string-at-least-32-chars"
   ```
   Expected: `Successfully saved ... to the secret store.` for each command.
   (If your Postgres password isn't `postgres`, also set
   `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=terrycorner;Username=postgres;Password=YOUR_PASSWORD"`.)

4. **First-time setup only** — generate the first migration (skip this if you
   already have a `Migrations/` folder from a previous zip):
   ```
   dotnet tool install --global dotnet-ef
   cd ../../
   dotnet ef migrations add InitialCreate -p src/TerryCorner.Infrastructure -s src/TerryCorner.Api
   ```
   Expected: `Done.` and a new `Migrations/` folder under
   `src/TerryCorner.Infrastructure`.

4a. **Everyone, this round** — `ApplicationUser` gained two new columns
   (`ProfilePictureFileName`, `CreatedAtUtc`), so generate a follow-up migration
   for that schema change (safe to run whether or not you did step 4 first —
   `dotnet ef` only picks up what actually changed):
   ```
   dotnet ef migrations add AddUserProfileFields -p src/TerryCorner.Infrastructure -s src/TerryCorner.Api
   ```
   Expected: `Done.` and a new migration file mentioning
   `ProfilePictureFileName` and `CreatedAtUtc` in `Migrations/`.

5. Run the API (Program.cs applies the migration and seeds sample data
   automatically in Development — Postgres must already be running from step 0):
   ```
   dotnet run --project src/TerryCorner.Api --launch-profile http
   ```
   Expected: log lines ending in `Now listening on: http://localhost:5000`, and
   `http://localhost:5000/swagger` loads in a browser showing Auth/Products/
   Categories/Promotions endpoints.

   The seeded admin account is `admin@terrycorner.local` / `ChangeMe123!` —
   rotate this password before this ever runs anywhere but your machine.

6. Smoke-test registration (new terminal, API still running). **On Windows
   PowerShell**, `curl` is aliased to `Invoke-WebRequest` with different syntax —
   either use the Swagger UI at `http://localhost:5000/swagger` and try the
   endpoint there instead, or call the real curl explicitly as `curl.exe`:
   ```
   curl.exe http://localhost:5000/api/auth/register -X POST -H "Content-Type: application/json" -d "{\"fullName\":\"Test User\",\"email\":\"test@example.com\",\"phoneNumber\":\"0911000000\",\"password\":\"SuperSecret123!\"}"
   ```
   Expected: a `200 OK` JSON body with `accessToken`, `refreshToken`, and a
   `user` object.

7. Confirm the seeded catalog is reachable (Swagger UI works here too):
   ```
   curl.exe http://localhost:5000/api/categories
   curl.exe "http://localhost:5000/api/products?featuredOnly=true"
   ```
   Expected: 5 categories; 3 featured products with `toppings` arrays.

8. Place a test order (grab a real `id` from step 7's product list output and
   substitute it below — the price and toppings you send don't matter, the
   server recalculates everything from that product's current price):
   ```
   curl.exe http://localhost:5000/api/orders -X POST -H "Content-Type: application/json" -d '{"contactFullName":"Test User","contactPhoneNumber":"0911000000","orderType":"Pickup","items":[{"productId":"PASTE_A_PRODUCT_ID_HERE","quantity":2,"toppingIds":[]}]}'
   ```
   Expected: `200 OK` with an `orderNumber` like `"TC-1000"`, `"status":"PaymentPending"`,
   and a `total` matching `2 ×` that product's price. Paste back what you get.

9. Track that order (use the `orderNumber` from step 8):
   ```
   curl.exe http://localhost:5000/api/orders/TC-1000
   ```
   Expected: a `timeline` array where `OrderReceived` and `PaymentPending` are
   `isComplete: true`/`isActive` as appropriate, matching the tracking timeline
   in the spec.


## Running the Angular client

1. Install dependencies (already done if you're using the files as generated):
   ```
   cd terry-corner-client
   npm install
   ```
   Expected: exits 0, no `npm error` lines.

2. Start the dev server:
   ```
   npm start
   ```
   Expected: `Local: http://localhost:4200/` and the Terry Corner header/footer
   render with placeholder pages when you open it.

## Next

Once you've run steps 1–7 and confirmed they match — especially step 2
(`dotnet build`, since I can't compile this myself and am fixing errors from
what you report) — tell me the results and I'll move on to Phase 3: checkout,
server-side price recalculation, order creation, and receipt upload.
