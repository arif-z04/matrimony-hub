-- =====================================================================
-- 04_indexes.sql: Performance Indexes for Search & Verification
-- =====================================================================

USE `matrimony_hub`;

-- Users indexes
CREATE INDEX `IX_Users_Email` ON `Users` (`Email`);
CREATE INDEX `IX_Users_PhoneNumber` ON `Users` (`PhoneNumber`);
CREATE INDEX `IX_Users_AccountStatus` ON `Users` (`AccountStatus`);
CREATE INDEX `IX_Users_CreatedAt` ON `Users` (`CreatedAt`);

-- Profiles indexes (critical for search query filtering)
CREATE INDEX `IX_Profiles_Gender` ON `Profiles` (`Gender`);
CREATE INDEX `IX_Profiles_DateOfBirth` ON `Profiles` (`DateOfBirth`);
CREATE INDEX `IX_Profiles_Religion` ON `Profiles` (`Religion`);
CREATE INDEX `IX_Profiles_MaritalStatus` ON `Profiles` (`MaritalStatus`);
CREATE INDEX `IX_Profiles_Division` ON `Profiles` (`Division`);
CREATE INDEX `IX_Profiles_Occupation` ON `Profiles` (`Occupation`);
CREATE INDEX `IX_Profiles_IsVerified` ON `Profiles` (`IsVerified`);
CREATE INDEX `IX_Profiles_IsActive` ON `Profiles` (`IsActive`);
CREATE INDEX `IX_Profiles_Composite_Search` ON `Profiles` (`Gender`, `Religion`, `Division`, `IsVerified`);

-- Photos indexes
CREATE INDEX `IX_ProfilePhotos_UserProfileId` ON `ProfilePhotos` (`UserProfileId`);
CREATE INDEX `IX_ProfilePhotos_IsPrimary` ON `ProfilePhotos` (`IsPrimary`);

-- NID Verification indexes
CREATE INDEX `IX_NidVerifications_Status` ON `NidVerifications` (`Status`);
CREATE INDEX `IX_NidVerifications_SubmittedAt` ON `NidVerifications` (`SubmittedAt`);
CREATE INDEX `IX_NidVerifications_UserId` ON `NidVerifications` (`UserId`);

-- Payments indexes
CREATE INDEX `IX_Payments_UserId` ON `Payments` (`UserId`);
CREATE INDEX `IX_Payments_Status` ON `Payments` (`Status`);
CREATE INDEX `IX_Payments_CreatedAt` ON `Payments` (`CreatedAt`);
CREATE INDEX `IX_Payments_Gateway` ON `Payments` (`Gateway`);

-- ContactAccess indexes
CREATE INDEX `IX_ContactAccess_UserId` ON `ContactAccess` (`UserId`);
CREATE INDEX `IX_ContactAccess_TargetProfileId` ON `ContactAccess` (`TargetProfileId`);

-- Notifications indexes
CREATE INDEX `IX_Notifications_UserId_IsRead` ON `Notifications` (`UserId`, `IsRead`);
CREATE INDEX `IX_Notifications_CreatedAt` ON `Notifications` (`CreatedAt`);

-- SuccessStories indexes
CREATE INDEX `IX_SuccessStories_Status` ON `SuccessStories` (`Status`);
CREATE INDEX `IX_SuccessStories_MarriageDate` ON `SuccessStories` (`MarriageDate`);

-- AdminLogs indexes
CREATE INDEX `IX_AdminLogs_Action` ON `AdminLogs` (`Action`);
CREATE INDEX `IX_AdminLogs_CreatedAt` ON `AdminLogs` (`CreatedAt`);

-- LoginAttempts indexes
CREATE INDEX `IX_LoginAttempts_Email_AttemptedAt` ON `LoginAttempts` (`Email`, `AttemptedAt`);
