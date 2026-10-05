# Matrimony Hub - REST API Documentation

All API endpoints return standard JSON responses wrapped in `ServiceResult` or `PagedResult<T>`.

## 1. Authentication Endpoints (`/api/auth`)

### POST `/api/auth/register`
Creates a new matrimonial member account and initializes their profile.
- **Request Body**:
  ```json
  {
    "fullName": "Tanvir Ahmed",
    "email": "tanvir@example.com",
    "phoneNumber": "01711223344",
    "gender": 1,
    "dateOfBirth": "1994-05-14T00:00:00Z",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "acceptTerms": true
  }
  ```
- **Responses**: `200 OK`, `400 Bad Request`.

### POST `/api/auth/login`
Authenticates user credentials and establishes cookie session.
- **Request Body**:
  ```json
  {
    "email": "user@example.com",
    "password": "User@Pass123!",
    "rememberMe": true
  }
  ```
- **Responses**: `200 OK`, `401 Unauthorized`.

---

## 2. Matches & Search Endpoints (`/api/matches`)

### GET `/api/matches`
Searches candidates with filters, pagination, and calculated match compatibility scores.
- **Query Parameters**:
  - `Gender` (int, 1: Male, 2: Female)
  - `MinAge` (int), `MaxAge` (int)
  - `Religion` (int), `Division` (string)
  - `VerifiedOnly` (bool)
  - `Keyword` (string)
  - `Page` (int), `PageSize` (int)
- **Response**: `PagedResult<ProfileCardDto>`

---

## 3. Favorites Endpoints (`/api/favorites`)

### POST `/api/favorites/{profileId}`
Toggles bookmark status for candidate profile.
- **Authorization**: Required (`User`)
- **Responses**: `200 OK` (Message: Added or Removed), `400 Bad Request` (Self-favorite disallowed).

---

## 4. Payment Endpoints (`/api/payments`)

### POST `/api/payments/create`
Initiates server-verified checkout session to unlock candidate contact.
- **Request Body**:
  ```json
  {
    "targetProfileId": 2,
    "gateway": 1
  }
  ```
- **Response**:
  ```json
  {
    "succeeded": true,
    "data": {
      "paymentId": 12,
      "transactionId": "TXN20261005120015",
      "amount": 500.00,
      "currency": "BDT",
      "checkoutUrl": "/checkout/gateway?gateway=bKash&txn=..."
    }
  }
  ```

### POST `/api/payments/callback`
Server-to-server gateway callback verification endpoint.
- Validates signature and transaction ID.
- Automatically unlocks contact and logs audit history.
