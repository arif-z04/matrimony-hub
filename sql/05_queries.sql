-- =====================================================================
-- 05_queries.sql: Production Queries & Operational Analytics
-- =====================================================================

USE `matrimony_hub`;

-- 1. Find all active and verified female profiles living in Dhaka division
SELECT 
    p.Id AS ProfileId,
    p.FullName,
    TIMESTAMPDIFF(YEAR, p.DateOfBirth, CURDATE()) AS Age,
    p.Occupation,
    p.HighestEducation,
    p.City,
    p.Division,
    ph.PhotoUrl AS PrimaryPhoto
FROM Profiles p
LEFT JOIN ProfilePhotos ph ON p.Id = ph.UserProfileId AND ph.IsPrimary = 1
WHERE p.IsActive = 1 
  AND p.IsVerified = 1
  AND p.Gender = 2 -- Female
  AND p.Division = 'Dhaka'
ORDER BY p.CreatedAt DESC;

-- 2. Search compatible partners matching specific age and criteria
SELECT 
    p.Id,
    p.FullName,
    TIMESTAMPDIFF(YEAR, p.DateOfBirth, CURDATE()) AS Age,
    p.Religion,
    p.MaritalStatus,
    p.Occupation,
    p.Division,
    p.IsVerified
FROM Profiles p
WHERE p.IsActive = 1
  AND p.Gender = 1 -- Male
  AND TIMESTAMPDIFF(YEAR, p.DateOfBirth, CURDATE()) BETWEEN 25 AND 35
  AND p.Religion = 1 -- Islam
  AND p.MaritalStatus = 1 -- NeverMarried
ORDER BY p.IsVerified DESC, p.CreatedAt DESC
LIMIT 20;

-- 3. Platform user demographic counts by Gender
SELECT 
    CASE p.Gender 
        WHEN 1 THEN 'Male'
        WHEN 2 THEN 'Female'
        ELSE 'Other'
    END AS GenderName,
    COUNT(*) AS TotalProfiles,
    SUM(CASE WHEN p.IsVerified = 1 THEN 1 ELSE 0 END) AS VerifiedProfiles
FROM Profiles p
WHERE p.IsActive = 1
GROUP BY p.Gender;

-- 4. Geographic distribution of users across Bangladesh divisions
SELECT 
    COALESCE(p.Division, 'Unspecified') AS Division,
    COUNT(*) AS MemberCount,
    SUM(CASE WHEN p.IsVerified = 1 THEN 1 ELSE 0 END) AS VerifiedMembers
FROM Profiles p
WHERE p.IsActive = 1
GROUP BY p.Division
ORDER BY MemberCount DESC;

-- 5. Revenue statistics & payment performance summary
SELECT 
    COUNT(*) AS TotalTransactions,
    SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) AS SuccessfulPayments,
    SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS PendingPayments,
    SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) AS FailedPayments,
    COALESCE(SUM(CASE WHEN Status = 3 THEN Amount ELSE 0 END), 0) AS TotalRevenueBDT,
    AVG(CASE WHEN Status = 3 THEN Amount ELSE NULL END) AS AvgTransactionValue
FROM Payments;

-- 6. Monthly revenue trend for financial reporting
SELECT 
    DATE_FORMAT(CreatedAt, '%Y-%m') AS RevenueMonth,
    COUNT(*) AS TotalTransactions,
    SUM(Amount) AS TotalRevenueBDT
FROM Payments
WHERE Status = 3 -- Successful
GROUP BY DATE_FORMAT(CreatedAt, '%Y-%m')
ORDER BY RevenueMonth DESC;

-- 7. Pending NID verifications queue for admin compliance review
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
WHERE nv.Status = 1 -- Pending
ORDER BY nv.SubmittedAt ASC;

-- 8. Recent registrations monitoring (last 7 days)
SELECT 
    u.Id,
    u.FullName,
    u.Email,
    u.PhoneNumber,
    u.AccountStatus,
    p.IsVerified,
    u.CreatedAt
FROM Users u
LEFT JOIN Profiles p ON u.Id = p.UserId
WHERE u.IsDeleted = 0
ORDER BY u.CreatedAt DESC
LIMIT 15;

-- 9. Check if a user has unlocked contact details for a target profile
SELECT 
    ca.Id AS AccessId,
    buyer.FullName AS BuyerName,
    target.FullName AS TargetName,
    ca.UnlockedAt,
    target_user.PhoneNumber AS ContactPhone,
    target_user.Email AS ContactEmail
FROM ContactAccess ca
INNER JOIN Users buyer ON ca.UserId = buyer.Id
INNER JOIN Profiles target ON ca.TargetProfileId = target.Id
INNER JOIN Users target_user ON target.UserId = target_user.Id
WHERE ca.UserId = 2 AND ca.TargetProfileId = 1;
