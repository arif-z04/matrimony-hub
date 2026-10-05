# Matrimony Hub — Production-Ready Matrimonial Platform

[![.NET Core](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0%20%2F%20MariaDB-blue.svg)](https://www.mysql.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg)]()

**Matrimony Hub** is a full-stack, production-ready matrimonial web application designed for dignified, trustworthy, and culturally resonant marriage partner matching in Bangladesh. Built with **ASP.NET Core**, **C#**, **Entity Framework Core**, **MySQL / MariaDB**, and **Bootstrap 5 Material Design**, it incorporates rigorous identity verification workflows (Bangladeshi National ID / NID), strict privacy gates on personal contact details, intelligent compatibility scoring, and server-verified payment gateways.

---

## 1. Key Architectural Features

- **Strict Contact Access Gate**: Personal phone numbers and email addresses are never exposed to unpaid viewers. The backend verifies payment status and logs contact unlocks in an authoritative `ContactAccess` table.
- **National ID (NID) Verification Workflow**: Supports smart card and classic NID document uploads. Compliance moderation allows administrators to approve or reject with mandatory audit reasons. Security rules prevent administrators from approving their own NID.
- **Configurable Payment Gateways**: Built around the `IPaymentGateway` strategy pattern, supporting Bangladeshi gateways (**bKash**, **SSLCommerz**, **Nagad**) and an interactive **Sandbox Gateway** for end-to-end local testing.
- **Real Database-Backed Analytics**: Strictly **no fake statistics** or dummy counters. All statistics displayed on the Hero page, User Dashboard, and Admin Dashboard reflect actual database rows.
- **Intelligent Compatibility Scoring Engine**: Evaluates age preferences, religion, education, profession, location proximity (Bangladeshi divisions), lifestyle compatibility, and verified status to produce match scores (0–100%).
- **Administrative Governance Portal**: Complete operational dashboard featuring user account suspension/restoration, NID compliance verification, transaction auditing, success stories moderation, and chronological `AdminLogs`.
- **Clean Architecture & SOLID Design**: Separation into `Domain`, `Application`, `Infrastructure`, `Web`, and `Tests` projects.

---

## 2. Technology Stack

- **Backend**: ASP.NET Core (.NET 10 / 9), C#, ASP.NET Core Identity
- **ORM / Persistence**: Entity Framework Core, Pomelo.EntityFrameworkCore.MySql
- **Database**: MySQL 8.0+ / MariaDB 10.5+ (InnoDB, UTF-8 MB4)
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla ES6), Bootstrap 5.3, Bootstrap Icons, Google Fonts (Playfair Display & Inter)
- **Testing**: xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing

---

## 3. Project Structure

```
MatrimonyHub/
│
├── src/
│   ├── MatrimonyHub.Domain/            # Entities, Value Objects, Domain Enums
│   ├── MatrimonyHub.Application/       # DTOs, Service Interfaces, Common Results
│   ├── MatrimonyHub.Infrastructure/    # DbContext, Identity, Payment Gateways, Services, Migrations
│   └── MatrimonyHub.Web/               # MVC Controllers, Razor Views, REST APIs, wwwroot
│
├── sql/
│   ├── 01_database.sql                 # Database creation (utf8mb4_unicode_ci)
│   ├── 02_tables.sql                   # Tables in dependency-safe order
│   ├── 03_constraints.sql              # Foreign keys, unique constraints, check constraints
│   ├── 04_indexes.sql                  # Performance & search indexes
│   ├── 05_queries.sql                  # Production queries & analytics
│   ├── 06_seed_data.sql                # Seed data for development & testing
│   ├── 07_views.sql                    # Production analytical views
│   └── 08_stored_procedures.sql        # Transactional stored procedures
│
├── tests/
│   └── MatrimonyHub.Tests/             # Unit and integration test suite
│
├── docs/                               # System documentation (API, DB, Architecture, Testing)
├── demo.md                             # Seeded accounts and test credentials
├── .env.example                        # Environment variables template
├── README.md                           # Master setup & operations documentation
└── MatrimonyHub.sln                    # Solution configuration
```

---

## 4. Prerequisites

1. **.NET SDK**: 10.0 or 9.0 (`dotnet --version`)
2. **Database Engine**: MySQL 8.0+ or MariaDB 10.5+ running locally or in Docker
3. **Database Client**: `mysql` or `mariadb` CLI, or GUI like DBeaver / phpMyAdmin

---

## 5. Database Setup (Two Supported Methods)

Choose either **Method 1** (direct SQL scripts) or **Method 2** (Entity Framework Core migrations).

### Method 1: Using Raw SQL Scripts
Run the scripts inside `/sql` in sequence:
```bash
# 1. Create database
mysql -u root -p < sql/01_database.sql

# 2. Create tables
mysql -u root -p matrimony_hub < sql/02_tables.sql

# 3. Apply constraints
mysql -u root -p matrimony_hub < sql/03_constraints.sql

# 4. Create performance indexes
mysql -u root -p matrimony_hub < sql/04_indexes.sql

# 5. Populate seed data
mysql -u root -p matrimony_hub < sql/06_seed_data.sql

# 6. Create analytical views
mysql -u root -p matrimony_hub < sql/07_views.sql

# 7. Create stored procedures
mysql -u root -p matrimony_hub < sql/08_stored_procedures.sql
```

### Method 2: Using Entity Framework Core Migrations
Ensure your connection string in `src/MatrimonyHub.Web/appsettings.json` points to your MySQL database, then run:
```bash
dotnet ef database update --project src/MatrimonyHub.Infrastructure --startup-project src/MatrimonyHub.Web
```
The application will automatically seed the initial admin account, demo members, success stories, and verifications on first startup.

---

## 6. Configuration & Environment Variables

Copy `.env.example` to `.env` or customize `src/MatrimonyHub.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3307;Database=matrimony_hub;User=matrimony_user;Password=matrimony_pass;"
  },
  "PaymentGateways": {
    "ContactUnlockFee": 500.00,
    "ActiveGateway": "Sandbox",
    "Bkash": {
      "AppKey": "your_key",
      "AppSecret": "your_secret",
      "Username": "your_user",
      "Password": "your_password"
    },
    "SSLCommerz": {
      "StoreId": "your_store_id",
      "StorePassword": "your_store_password",
      "IsSandbox": true
    }
  }
}
```

---

## 7. Running the Application

To restore, build, and run the web application:
```bash
# Restore & build solution
dotnet restore
dotnet build

# Launch the web application
dotnet run --project src/MatrimonyHub.Web
```
The application will listen on `https://localhost:5001` or `http://localhost:5000`.

---

## 8. Development & Demo Credentials

Seeded accounts are provided for development testing. Complete details, login emails, and passwords are documented exclusively in:
👉 **[`demo.md`](file:///home/noir/Work/Matrimony-hub/demo.md)**

- **Admin Account**: `admin@matrimonyhub.com` / `Admin@Pass123!` (Access to `/Admin`)
- **Member Accounts**: Multiple male and female profiles across Dhaka, Chittagong, Sylhet, with verified NID and pending NID test cases.

---

## 9. Running Tests

To run the full suite of unit and integration tests:
```bash
dotnet test
```
The test suite validates:
- Contact information is inaccessible without verified payment.
- Unlocked contacts reveal phone and email only to legitimate payers.
- Admins cannot approve their own NID verifications.
- Non-admin users cannot access admin endpoints.
- Match compatibility scoring accurately weights preferences.

---

## 10. Production Deployment Guidelines

1. **Enforce HTTPS**: Set `app.UseHsts()` and enforce TLS 1.3 reverse proxy via NGINX or Cloudflare.
2. **Secrets Management**: Store connection strings and payment merchant credentials using environment variables or Azure Key Vault / HashiCorp Vault. Never commit secrets to source control.
3. **Database Performance**: Ensure MySQL `innodb_buffer_pool_size` is sized appropriately (e.g. 70–80% of dedicated RAM).
4. **File Storage**: In scalable multi-server deployments, swap `FileStorageService` with an S3 / Azure Blob Storage provider.
