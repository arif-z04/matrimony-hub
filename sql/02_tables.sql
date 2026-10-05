-- =====================================================================
-- 02_tables.sql: Create Tables in Dependency-Safe Order
-- =====================================================================

USE `matrimony_hub`;

-- 1. Roles Table
CREATE TABLE IF NOT EXISTS `Roles` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(256) NULL,
    `NormalizedName` VARCHAR(256) NULL,
    `ConcurrencyStamp` LONGTEXT NULL,
    `Description` VARCHAR(250) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Users Table
CREATE TABLE IF NOT EXISTS `Users` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserName` VARCHAR(256) NULL,
    `NormalizedUserName` VARCHAR(256) NULL,
    `Email` VARCHAR(256) NULL,
    `NormalizedEmail` VARCHAR(256) NULL,
    `EmailConfirmed` TINYINT(1) NOT NULL DEFAULT 0,
    `PasswordHash` LONGTEXT NULL,
    `SecurityStamp` LONGTEXT NULL,
    `ConcurrencyStamp` LONGTEXT NULL,
    `PhoneNumber` VARCHAR(50) NULL,
    `PhoneNumberConfirmed` TINYINT(1) NOT NULL DEFAULT 0,
    `TwoFactorEnabled` TINYINT(1) NOT NULL DEFAULT 0,
    `LockoutEnd` DATETIME(6) NULL,
    `LockoutEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `AccessFailedCount` INT NOT NULL DEFAULT 0,
    `FullName` VARCHAR(150) NOT NULL,
    `AccountStatus` INT NOT NULL DEFAULT 1, -- 1: Active, 2: Suspended, 3: Deactivated
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `LastLoginAt` DATETIME(6) NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. Identity User Roles Table
CREATE TABLE IF NOT EXISTS `UserRoles` (
    `UserId` INT NOT NULL,
    `RoleId` INT NOT NULL,
    PRIMARY KEY (`UserId`, `RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. Identity User Claims Table
CREATE TABLE IF NOT EXISTS `UserClaims` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `ClaimType` LONGTEXT NULL,
    `ClaimValue` LONGTEXT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. Identity User Logins Table
CREATE TABLE IF NOT EXISTS `UserLogins` (
    `LoginProvider` VARCHAR(128) NOT NULL,
    `ProviderKey` VARCHAR(128) NOT NULL,
    `ProviderDisplayName` LONGTEXT NULL,
    `UserId` INT NOT NULL,
    PRIMARY KEY (`LoginProvider`, `ProviderKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. Identity Role Claims Table
CREATE TABLE IF NOT EXISTS `RoleClaims` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `RoleId` INT NOT NULL,
    `ClaimType` LONGTEXT NULL,
    `ClaimValue` LONGTEXT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 7. Identity User Tokens Table
CREATE TABLE IF NOT EXISTS `UserTokens` (
    `UserId` INT NOT NULL,
    `LoginProvider` VARCHAR(128) NOT NULL,
    `Name` VARCHAR(128) NOT NULL,
    `Value` LONGTEXT NULL,
    PRIMARY KEY (`UserId`, `LoginProvider`, `Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 8. Profiles Table
CREATE TABLE IF NOT EXISTS `Profiles` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `FullName` VARCHAR(150) NOT NULL,
    `Gender` INT NOT NULL, -- 1: Male, 2: Female, 3: Other
    `DateOfBirth` DATETIME(6) NOT NULL,
    `HeightCm` INT NULL,
    `WeightKg` INT NULL,
    `MaritalStatus` INT NOT NULL DEFAULT 1, -- 1: NeverMarried, 2: Divorced, 3: Widowed
    `Religion` INT NOT NULL DEFAULT 1, -- 1: Islam, 2: Hinduism, 3: Christianity, 4: Buddhism
    `MotherTongue` VARCHAR(50) NOT NULL DEFAULT 'Bengali',
    `Nationality` VARCHAR(50) NOT NULL DEFAULT 'Bangladeshi',
    `HighestEducation` VARCHAR(100) NULL,
    `Institution` VARCHAR(150) NULL,
    `Subject` VARCHAR(100) NULL,
    `GraduationYear` INT NULL,
    `Occupation` VARCHAR(100) NULL,
    `Company` VARCHAR(150) NULL,
    `JobTitle` VARCHAR(100) NULL,
    `IncomeRange` VARCHAR(50) NULL,
    `Division` VARCHAR(50) NULL,
    `District` VARCHAR(50) NULL,
    `City` VARCHAR(50) NULL,
    `Country` VARCHAR(50) NOT NULL DEFAULT 'Bangladesh',
    `Smoking` TINYINT(1) NOT NULL DEFAULT 0,
    `Drinking` TINYINT(1) NOT NULL DEFAULT 0,
    `DietaryPreference` INT NOT NULL DEFAULT 2, -- 2: Halal, 3: Vegetarian
    `Hobbies` VARCHAR(255) NULL,
    `Interests` VARCHAR(255) NULL,
    `AboutMe` LONGTEXT NULL,
    `FamilyInfo` LONGTEXT NULL,
    `PartnerExpectations` LONGTEXT NULL,
    `IsVerified` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 9. Profile Photos Table
CREATE TABLE IF NOT EXISTS `ProfilePhotos` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserProfileId` INT NOT NULL,
    `PhotoUrl` VARCHAR(255) NOT NULL,
    `IsPrimary` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 10. Partner Preferences Table
CREATE TABLE IF NOT EXISTS `PartnerPreferences` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserProfileId` INT NOT NULL,
    `MinAge` INT NULL,
    `MaxAge` INT NULL,
    `PreferredGender` INT NULL,
    `PreferredReligion` INT NULL,
    `PreferredMaritalStatus` INT NULL,
    `PreferredEducation` VARCHAR(100) NULL,
    `PreferredOccupation` VARCHAR(100) NULL,
    `PreferredDivision` VARCHAR(50) NULL,
    `DietaryPreference` INT NULL,
    `Notes` VARCHAR(500) NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 11. NID Verifications Table
CREATE TABLE IF NOT EXISTS `NidVerifications` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `NidNumber` VARCHAR(50) NOT NULL,
    `FrontDocumentUrl` VARCHAR(255) NOT NULL,
    `BackDocumentUrl` VARCHAR(255) NULL,
    `Status` INT NOT NULL DEFAULT 1, -- 1: Pending, 2: Approved, 3: Rejected
    `SubmittedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `ReviewedAt` DATETIME(6) NULL,
    `ReviewedByAdminId` INT NULL,
    `RejectionReason` VARCHAR(500) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 12. Favorites Table
CREATE TABLE IF NOT EXISTS `Favorites` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `FavoriteProfileId` INT NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 13. Payments Table
CREATE TABLE IF NOT EXISTS `Payments` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `TargetProfileId` INT NULL,
    `Amount` DECIMAL(18, 2) NOT NULL,
    `Currency` VARCHAR(10) NOT NULL DEFAULT 'BDT',
    `PaymentPurpose` INT NOT NULL DEFAULT 1, -- 1: ContactUnlock
    `TransactionId` VARCHAR(100) NOT NULL,
    `Gateway` INT NOT NULL DEFAULT 4, -- 1: bKash, 2: SSLCommerz, 3: Nagad, 4: Sandbox
    `GatewayTransactionId` VARCHAR(100) NULL,
    `Status` INT NOT NULL DEFAULT 1, -- 1: Pending, 2: Processing, 3: Successful, 4: Failed, 5: Cancelled, 6: Refunded
    `FailureReason` VARCHAR(255) NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CompletedAt` DATETIME(6) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 14. Transactions Table (Gateway transaction audit log)
CREATE TABLE IF NOT EXISTS `Transactions` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `PaymentId` INT NOT NULL,
    `GatewayTransactionId` VARCHAR(100) NULL,
    `GatewayResponse` LONGTEXT NULL,
    `Amount` DECIMAL(18, 2) NOT NULL,
    `Status` INT NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 15. Contact Access Table
CREATE TABLE IF NOT EXISTS `ContactAccess` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `TargetProfileId` INT NOT NULL,
    `PaymentId` INT NOT NULL,
    `UnlockedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 16. Notifications Table
CREATE TABLE IF NOT EXISTS `Notifications` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `Title` VARCHAR(150) NOT NULL,
    `Message` VARCHAR(500) NOT NULL,
    `Type` INT NOT NULL,
    `IsRead` TINYINT(1) NOT NULL DEFAULT 0,
    `RelatedEntityId` VARCHAR(50) NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 17. Success Stories Table
CREATE TABLE IF NOT EXISTS `SuccessStories` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `CoupleNames` VARCHAR(150) NOT NULL,
    `StoryTitle` VARCHAR(200) NOT NULL,
    `StoryDescription` LONGTEXT NOT NULL,
    `PhotoUrl` VARCHAR(255) NULL,
    `MarriageDate` DATETIME(6) NOT NULL,
    `Location` VARCHAR(100) NOT NULL,
    `Status` INT NOT NULL DEFAULT 2, -- 1: Pending, 2: Approved, 3: Rejected
    `SubmittedByUserId` INT NULL,
    `ApprovedByAdminId` INT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 18. Admin Logs Table
CREATE TABLE IF NOT EXISTS `AdminLogs` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `AdminUserId` INT NULL,
    `Action` VARCHAR(100) NOT NULL,
    `EntityType` VARCHAR(100) NOT NULL,
    `EntityId` VARCHAR(50) NULL,
    `Description` VARCHAR(1000) NOT NULL,
    `IpAddress` VARCHAR(50) NULL,
    `UserAgent` VARCHAR(255) NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 19. Login Attempts Table
CREATE TABLE IF NOT EXISTS `LoginAttempts` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Email` VARCHAR(100) NOT NULL,
    `IpAddress` VARCHAR(50) NULL,
    `Successful` TINYINT(1) NOT NULL,
    `AttemptedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
