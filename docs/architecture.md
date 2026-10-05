# Matrimony Hub - System Architecture & Design Specification

## 1. Architectural Philosophy
Matrimony Hub is engineered following **Clean Architecture** principles and **Domain-Driven Design (DDD)** patterns. The solution separates concerns into decoupled layers with unidirectional dependency flowing toward the Domain:

```
[ MatrimonyHub.Web ] (Presentation: MVC Controllers, Razor Views, REST APIs)
         │
         ▼
[ MatrimonyHub.Infrastructure ] (Persistence, Identity, Gateways, File System, Email)
         │
         ▼
[ MatrimonyHub.Application ] (Contracts, DTOs, Business Rules, Result Types)
         │
         ▼
[ MatrimonyHub.Domain ] (Core Entities, Value Objects, Domain Enums)
```

## 2. Core Subsystems

### A. Authentication & Role-Based Authorization
- Backed by **ASP.NET Core Identity** utilizing strongly-typed integer keys (`IdentityUser<int>`, `IdentityRole<int>`).
- Passwords hashed using PBKDF2 with HMAC-SHA256 and cryptographic salting.
- Dual-role model: `Admin` and `User`.
- Admin endpoints strictly guarded by policy `[Authorize(Roles = "Admin")]`.
- Account status lifecycle: `Active`, `Suspended`, `Deactivated`.

### B. Privacy & Contact Gate Subsystem
- Personal contact details (Phone number, Email address, Physical street residence) are strictly isolated.
- The `ProfileService` enforces a strict server-side policy:
  ```csharp
  if (isOwner || isContactUnlocked)
  {
      dto.ContactPhone = profile.User?.PhoneNumber;
      dto.ContactEmail = profile.User?.Email;
      dto.ContactAddress = $"{profile.City}, {profile.District}, {profile.Division}, {profile.Country}";
  }
  ```
- Any unauthorized caller receives `null` for sensitive contact fields.

### C. NID Identity Verification Workflow
- Members submit their government National ID number and high-resolution document scans.
- Requests enter a `Pending` queue.
- Self-review prevention rule:
  ```csharp
  if (verification.UserId == adminUserId)
  {
      return ServiceResult.Failure("Security violation: Administrators cannot review or approve their own identity verification.");
  }
  ```
- Approvals grant the verified badge; rejections mandate an explicit reason and notify the user via DB and email.

### D. Payment Architecture (`IPaymentService`)
- Gateways implemented through the `IPaymentGateway` strategy pattern:
  - `BkashPaymentGateway` (Tokenized API ready)
  - `SslCommerzPaymentGateway` (Session API ready)
  - `NagadPaymentGateway` (Merchant API ready)
  - `SandboxPaymentGateway` (Interactive test gateway with end-to-end authorization callback)
- Strict server-side fee validation: Fee is fetched exclusively from backend configuration (`ContactUnlockFee = 500.00 BDT`) - client input is never trusted.
- Idempotency & DB transaction wrapping ensure no duplicate unlocks or double charging.
