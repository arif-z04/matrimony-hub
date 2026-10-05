-- =====================================================================
-- 07_views.sql: Production Database Views for Reporting & Analytics
-- =====================================================================

USE `matrimony_hub`;

-- 1. View: Active & Verified Users with Profile Summaries
CREATE OR REPLACE VIEW `vw_active_verified_users` AS
SELECT 
    u.Id AS UserId,
    u.FullName,
    u.Email,
    u.PhoneNumber,
    p.Id AS ProfileId,
    p.Gender,
    TIMESTAMPDIFF(YEAR, p.DateOfBirth, CURDATE()) AS Age,
    p.Religion,
    p.MaritalStatus,
    p.HighestEducation,
    p.Occupation,
    p.Division,
    p.District,
    p.City,
    ph.PhotoUrl AS PrimaryPhotoUrl,
    p.CreatedAt AS ProfileCreatedAt
FROM Users u
INNER JOIN Profiles p ON u.Id = p.UserId
LEFT JOIN ProfilePhotos ph ON p.Id = ph.UserProfileId AND ph.IsPrimary = 1
WHERE u.IsDeleted = 0 
  AND u.AccountStatus = 1 -- Active
  AND p.IsActive = 1
  AND p.IsVerified = 1;

-- 2. View: Payment & Revenue Financial Summary
CREATE OR REPLACE VIEW `vw_payment_summary` AS
SELECT 
    p.Id AS PaymentId,
    p.TransactionId,
    u.FullName AS PayerName,
    u.Email AS PayerEmail,
    target.FullName AS TargetPartnerName,
    p.Amount,
    p.Currency,
    CASE p.Gateway
        WHEN 1 THEN 'bKash'
        WHEN 2 THEN 'SSLCommerz'
        WHEN 3 THEN 'Nagad'
        ELSE 'Sandbox'
    END AS GatewayName,
    CASE p.Status
        WHEN 1 THEN 'Pending'
        WHEN 2 THEN 'Processing'
        WHEN 3 THEN 'Successful'
        WHEN 4 THEN 'Failed'
        WHEN 5 THEN 'Cancelled'
        WHEN 6 THEN 'Refunded'
    END AS StatusName,
    p.CreatedAt,
    p.CompletedAt
FROM Payments p
INNER JOIN Users u ON p.UserId = u.Id
LEFT JOIN Profiles target ON p.TargetProfileId = target.Id;

-- 3. View: Admin Dashboard Key Real-Time Statistics
CREATE OR REPLACE VIEW `vw_admin_dashboard_statistics` AS
SELECT 
    (SELECT COUNT(*) FROM Users WHERE IsDeleted = 0) AS TotalUsers,
    (SELECT COUNT(*) FROM Users WHERE IsDeleted = 0 AND AccountStatus = 1) AS ActiveUsers,
    (SELECT COUNT(*) FROM Profiles WHERE IsVerified = 1 AND IsActive = 1) AS VerifiedUsers,
    (SELECT COUNT(*) FROM NidVerifications WHERE Status = 1) AS PendingNidVerifications,
    (SELECT COUNT(*) FROM Payments WHERE Status = 3) AS SuccessfulPayments,
    (SELECT COUNT(*) FROM Payments WHERE Status = 1) AS PendingPayments,
    (SELECT COUNT(*) FROM Transactions) AS TotalTransactions,
    (SELECT COALESCE(SUM(Amount), 0) FROM Payments WHERE Status = 3) AS TotalRevenueBDT,
    (SELECT COUNT(*) FROM SuccessStories WHERE Status = 2) AS ApprovedSuccessStories;

-- 4. View: Pending NID Verifications Queue
CREATE OR REPLACE VIEW `vw_pending_verifications` AS
SELECT 
    nv.Id AS VerificationId,
    u.Id AS UserId,
    u.FullName,
    u.Email,
    u.PhoneNumber,
    nv.NidNumber,
    nv.FrontDocumentUrl,
    nv.BackDocumentUrl,
    nv.SubmittedAt
FROM NidVerifications nv
INNER JOIN Users u ON nv.UserId = u.Id
WHERE nv.Status = 1; -- Pending
