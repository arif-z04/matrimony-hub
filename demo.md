# Matrimony Hub Demo Credentials

> **Notice**: These credentials are for local development and automated quality verification only. As mandated by security guidelines, these credentials are kept in this documentation file and are never rendered visibly in the website user interface.

---

## 1. System Administrator Account
Provides full access to the Admin Portal (`/Admin`), including NID verification compliance approvals/rejections, user account management (suspend/restore), payment monitoring, success stories moderation, and audit logs.

- **Role**: `Admin`
- **Email**: `admin@matrimonyhub.com`
- **Password**: `Admin@Pass123!`
- **Access Level**: Full Administrative Governance (`/Admin`)

---

## 2. Seeded Member Accounts (Grooms & Brides)

Each seeded account is pre-populated with realistic Bangladeshi profiles, education, profession, division, lifestyle, and preferences.

### Groom Account 1 (Dhaka - Software Engineer - NID Verified)
- **Role**: `User`
- **Email**: `tanvir.ahmed@example.com`
- **Password**: `User@Pass123!`
- **Profile**: 32 yrs, Lead Software Architect, BUET Graduate, Gulshan, Dhaka
- **State**: Verified NID, has completed 1 contact unlock transaction for Nusrat Jahan

### Bride Account 1 (Dhaka - Medical Doctor - NID Verified)
- **Role**: `User`
- **Email**: `nusrat.jahan@example.com`
- **Password**: `User@Pass123!`
- **Profile**: 29 yrs, Medical Officer, Dhaka Medical College, Dhanmondi, Dhaka
- **State**: Verified NID, profile contact unlocked by Tanvir Ahmed

### Groom Account 2 (Chittagong - Corporate Banker - NID Verified)
- **Role**: `User`
- **Email**: `rahim.chowdhury@example.com`
- **Password**: `User@Pass123!`
- **Profile**: 33 yrs, Senior Manager (EBL), IBA DU Graduate, Agrabad, Chittagong
- **State**: Verified NID, active matrimonial member

### Bride Account 2 (Dhaka - Senior Architect - NID Verified)
- **Role**: `User`
- **Email**: `sadia.islam@example.com`
- **Password**: `User@Pass123!`
- **Profile**: 30 yrs, Senior Architect, BUET, Uttara, Dhaka
- **State**: Verified NID, looking for groom in Dhaka

### Groom Account 3 (Sylhet - Civil Project Engineer - Pending NID)
- **Role**: `User`
- **Email**: `kazi.farhan@example.com`
- **Password**: `User@Pass123!`
- **Profile**: 31 yrs, Project Engineer, CUET, Zindabazar, Sylhet
- **State**: Has a **Pending NID Verification** in the admin queue ready for inspection and approval/rejection testing

### Bride Account 3 (Dhaka - University Lecturer - NID Verified)
- **Role**: `User`
- **Email**: `anika.tabassum@example.com`
- **Password**: `User@Pass123!`
- **Profile**: 27 yrs, Lecturer at BRAC University, Dhaka University MA, Banani, Dhaka
- **State**: Verified NID

---

## 3. Contact Unlock & Payment Testing

- When viewing another member's profile (e.g., while signed in as `rahim.chowdhury@example.com` viewing `sadia.islam@example.com`), private phone and email details are locked.
- Clicking **Unlock Contact Information** directs to the checkout page (`/Payment/Checkout?targetProfileId=...`) where the fee is securely verified server-side (**BDT 500.00**).
- The user can select the **Sandbox Gateway**, **bKash**, **SSLCommerz**, or **Nagad**.
- In local development mode, selecting any gateway launches the gateway portal where you can click **Authorize & Confirm Payment**, which simulates the real callback to `/api/payments/callback`, validates the transaction server-side in a database transaction, unlocks the contact access, and records audit logs.
