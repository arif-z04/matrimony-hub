-- =====================================================================
-- 03_constraints.sql: Foreign Keys, Unique & Check Constraints
-- =====================================================================

USE `matrimony_hub`;

-- UserRoles Foreign Keys
ALTER TABLE `UserRoles`
    ADD CONSTRAINT `FK_UserRoles_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    ADD CONSTRAINT `FK_UserRoles_Roles` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE;

-- UserClaims Foreign Keys
ALTER TABLE `UserClaims`
    ADD CONSTRAINT `FK_UserClaims_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

-- UserLogins Foreign Keys
ALTER TABLE `UserLogins`
    ADD CONSTRAINT `FK_UserLogins_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

-- RoleClaims Foreign Keys
ALTER TABLE `RoleClaims`
    ADD CONSTRAINT `FK_RoleClaims_Roles` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE;

-- UserTokens Foreign Keys
ALTER TABLE `UserTokens`
    ADD CONSTRAINT `FK_UserTokens_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

-- Profiles 1-to-1 User Foreign Key & Unique Constraint
ALTER TABLE `Profiles`
    ADD CONSTRAINT `FK_Profiles_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    ADD CONSTRAINT `UQ_Profiles_UserId` UNIQUE (`UserId`);

-- ProfilePhotos Foreign Key
ALTER TABLE `ProfilePhotos`
    ADD CONSTRAINT `FK_ProfilePhotos_Profiles` FOREIGN KEY (`UserProfileId`) REFERENCES `Profiles` (`Id`) ON DELETE CASCADE;

-- PartnerPreferences 1-to-1 Profile Foreign Key & Unique Constraint
ALTER TABLE `PartnerPreferences`
    ADD CONSTRAINT `FK_PartnerPreferences_Profiles` FOREIGN KEY (`UserProfileId`) REFERENCES `Profiles` (`Id`) ON DELETE CASCADE,
    ADD CONSTRAINT `UQ_PartnerPreferences_UserProfileId` UNIQUE (`UserProfileId`);

-- NidVerifications Foreign Keys
ALTER TABLE `NidVerifications`
    ADD CONSTRAINT `FK_NidVerifications_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    ADD CONSTRAINT `FK_NidVerifications_Admin` FOREIGN KEY (`ReviewedByAdminId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL;

-- Favorites Foreign Keys & Unique Constraint (No Duplicate Favorites)
ALTER TABLE `Favorites`
    ADD CONSTRAINT `FK_Favorites_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    ADD CONSTRAINT `FK_Favorites_Profiles` FOREIGN KEY (`FavoriteProfileId`) REFERENCES `Profiles` (`Id`) ON DELETE CASCADE,
    ADD CONSTRAINT `UQ_Favorites_User_Profile` UNIQUE (`UserId`, `FavoriteProfileId`);

-- Payments Foreign Keys & Unique TransactionId
ALTER TABLE `Payments`
    ADD CONSTRAINT `FK_Payments_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    ADD CONSTRAINT `FK_Payments_TargetProfile` FOREIGN KEY (`TargetProfileId`) REFERENCES `Profiles` (`Id`) ON DELETE SET NULL,
    ADD CONSTRAINT `UQ_Payments_TransactionId` UNIQUE (`TransactionId`);

-- Transactions Foreign Key
ALTER TABLE `Transactions`
    ADD CONSTRAINT `FK_Transactions_Payments` FOREIGN KEY (`PaymentId`) REFERENCES `Payments` (`Id`) ON DELETE CASCADE;

-- ContactAccess Foreign Keys & Unique Constraint (No Duplicate Contact Unlocks)
ALTER TABLE `ContactAccess`
    ADD CONSTRAINT `FK_ContactAccess_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    ADD CONSTRAINT `FK_ContactAccess_Profiles` FOREIGN KEY (`TargetProfileId`) REFERENCES `Profiles` (`Id`) ON DELETE CASCADE,
    ADD CONSTRAINT `FK_ContactAccess_Payments` FOREIGN KEY (`PaymentId`) REFERENCES `Payments` (`Id`) ON DELETE RESTRICT,
    ADD CONSTRAINT `UQ_ContactAccess_User_Target` UNIQUE (`UserId`, `TargetProfileId`);

-- Notifications Foreign Key
ALTER TABLE `Notifications`
    ADD CONSTRAINT `FK_Notifications_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

-- SuccessStories Foreign Keys
ALTER TABLE `SuccessStories`
    ADD CONSTRAINT `FK_SuccessStories_SubmittedUser` FOREIGN KEY (`SubmittedByUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL,
    ADD CONSTRAINT `FK_SuccessStories_AdminUser` FOREIGN KEY (`ApprovedByAdminId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL;

-- AdminLogs Foreign Key
ALTER TABLE `AdminLogs`
    ADD CONSTRAINT `FK_AdminLogs_AdminUser` FOREIGN KEY (`AdminUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL;

-- Check Constraints (where supported by MariaDB / MySQL 8.0.16+)
ALTER TABLE `Profiles`
    ADD CONSTRAINT `CHK_Profiles_Gender` CHECK (`Gender` IN (1, 2, 3)),
    ADD CONSTRAINT `CHK_Profiles_Height` CHECK (`HeightCm` IS NULL OR (`HeightCm` >= 100 AND `HeightCm` <= 250));

ALTER TABLE `Payments`
    ADD CONSTRAINT `CHK_Payments_Amount` CHECK (`Amount` > 0);
