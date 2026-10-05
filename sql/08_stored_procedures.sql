-- =====================================================================
-- 08_stored_procedures.sql: Stored Procedures for Optimized Operations
-- =====================================================================

USE `matrimony_hub`;

DROP PROCEDURE IF EXISTS `sp_SearchCompatiblePartners`;
DROP PROCEDURE IF EXISTS `sp_UnlockContactAccess`;
DROP PROCEDURE IF EXISTS `sp_GetAdminDashboardStats`;

DELIMITER $$

-- 1. Search Compatible Partners Procedure
CREATE PROCEDURE `sp_SearchCompatiblePartners`(
    IN in_ViewerUserId INT,
    IN in_TargetGender INT,
    IN in_MinAge INT,
    IN in_MaxAge INT,
    IN in_Religion INT,
    IN in_Division VARCHAR(50),
    IN in_VerifiedOnly TINYINT(1),
    IN in_Offset INT,
    IN in_Limit INT
)
BEGIN
    SELECT 
        p.Id AS ProfileId,
        p.UserId,
        p.FullName,
        p.Gender,
        TIMESTAMPDIFF(YEAR, p.DateOfBirth, CURDATE()) AS Age,
        p.HeightCm,
        p.MaritalStatus,
        p.Religion,
        p.Division,
        p.District,
        p.City,
        p.HighestEducation,
        p.Occupation,
        ph.PhotoUrl AS PrimaryPhotoUrl,
        p.IsVerified,
        LEFT(p.AboutMe, 120) AS AboutMeExcerpt,
        EXISTS(SELECT 1 FROM Favorites f WHERE f.UserId = in_ViewerUserId AND f.FavoriteProfileId = p.Id) AS IsFavorited,
        EXISTS(SELECT 1 FROM ContactAccess ca WHERE ca.UserId = in_ViewerUserId AND ca.TargetProfileId = p.Id) AS IsContactUnlocked
    FROM Profiles p
    INNER JOIN Users u ON p.UserId = u.Id
    LEFT JOIN ProfilePhotos ph ON p.Id = ph.UserProfileId AND ph.IsPrimary = 1
    WHERE p.IsActive = 1
      AND u.IsDeleted = 0
      AND u.AccountStatus = 1
      AND (in_ViewerUserId IS NULL OR p.UserId <> in_ViewerUserId)
      AND (in_TargetGender IS NULL OR p.Gender = in_TargetGender)
      AND (in_MinAge IS NULL OR TIMESTAMPDIFF(YEAR, p.DateOfBirth, CURDATE()) >= in_MinAge)
      AND (in_MaxAge IS NULL OR TIMESTAMPDIFF(YEAR, p.DateOfBirth, CURDATE()) <= in_MaxAge)
      AND (in_Religion IS NULL OR p.Religion = in_Religion)
      AND (in_Division IS NULL OR in_Division = '' OR p.Division = in_Division)
      AND (in_VerifiedOnly = 0 OR in_VerifiedOnly IS NULL OR p.IsVerified = 1)
    ORDER BY p.IsVerified DESC, p.CreatedAt DESC
    LIMIT in_Limit OFFSET in_Offset;
END$$

-- 2. Unlock Contact Access Transactional Procedure
CREATE PROCEDURE `sp_UnlockContactAccess`(
    IN in_UserId INT,
    IN in_TargetProfileId INT,
    IN in_PaymentId INT,
    OUT out_Success TINYINT(1),
    OUT out_Message VARCHAR(255)
)
BEGIN
    DECLARE v_PaymentStatus INT;
    DECLARE v_ExistingAccess INT;
    
    -- Check payment status
    SELECT Status INTO v_PaymentStatus FROM Payments WHERE Id = in_PaymentId AND UserId = in_UserId;
    
    IF v_PaymentStatus IS NULL THEN
        SET out_Success = 0;
        SET out_Message = 'Payment record does not exist or user mismatch.';
    ELSEIF v_PaymentStatus <> 3 THEN -- 3: Successful
        SET out_Success = 0;
        SET out_Message = 'Payment has not been confirmed as successful.';
    ELSE
        -- Check if already unlocked
        SELECT COUNT(*) INTO v_ExistingAccess FROM ContactAccess 
        WHERE UserId = in_UserId AND TargetProfileId = in_TargetProfileId;
        
        IF v_ExistingAccess > 0 THEN
            SET out_Success = 1;
            SET out_Message = 'Contact information was already unlocked.';
        ELSE
            INSERT INTO ContactAccess (UserId, TargetProfileId, PaymentId, UnlockedAt)
            VALUES (in_UserId, in_TargetProfileId, in_PaymentId, NOW(6));
            
            SET out_Success = 1;
            SET out_Message = 'Contact information unlocked successfully.';
        END IF;
    END IF;
END$$

-- 3. Stored Procedure for Admin Dashboard Overview
CREATE PROCEDURE `sp_GetAdminDashboardStats`()
BEGIN
    SELECT * FROM vw_admin_dashboard_statistics;
END$$

DELIMITER ;
