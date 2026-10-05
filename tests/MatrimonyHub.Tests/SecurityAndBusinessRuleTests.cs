using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.PaymentGateways;
using MatrimonyHub.Infrastructure.Persistence;
using MatrimonyHub.Infrastructure.Services;
using Xunit;

namespace MatrimonyHub.Tests;

public class SecurityAndBusinessRuleTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void AdminPasswordHashIsValid()
    {
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
        var user = new ApplicationUser { UserName = "admin@matrimonyhub.com" };
        var adminHash = "AQAAAAIAAYagAAAAEGGkBkSYdUINgqFZdEF89tlIVedpIxd5FuG4/irKuKUvQeJlk8nRpaVfaSqB4TwNOQ==";
        var result = hasher.VerifyHashedPassword(user, adminHash, "Admin@Pass123!");
        result.Should().Be(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task UserCannotAccessContactInfoWithoutSuccessfulPayment()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var viewerUser = new ApplicationUser { Id = 10, FullName = "Viewer User", Email = "viewer@example.com" };
        var candidateUser = new ApplicationUser { Id = 20, FullName = "Candidate User", Email = "candidate@example.com", PhoneNumber = "01799998888" };
        db.Users.AddRange(viewerUser, candidateUser);

        var candidateProfile = new UserProfile
        {
            Id = 100,
            UserId = candidateUser.Id,
            User = candidateUser,
            FullName = "Candidate User",
            Gender = Gender.Female,
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            IsActive = true
        };
        db.UserProfiles.Add(candidateProfile);
        await db.SaveChangesAsync();

        var paymentOptions = Options.Create(new PaymentGatewayOptions { ContactUnlockFee = 500m });
        var contactAccessService = new ContactAccessService(db, paymentOptions);
        var fileStorageMock = new Mock<IFileStorageService>();
        var profileService = new ProfileService(db, fileStorageMock.Object, contactAccessService, NullLogger<ProfileService>.Instance);

        // Act
        var result = await profileService.GetProfileByIdAsync(candidateProfile.Id, requestingUserId: viewerUser.Id);

        // Assert: Critical Privacy Protection
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsContactUnlocked.Should().BeFalse();
        result.Data.ContactPhone.Should().BeNull("Phone number must not be exposed without payment");
        result.Data.ContactEmail.Should().BeNull("Email address must not be exposed without payment");
        result.Data.ContactAddress.Should().BeNull("Address must not be exposed without payment");
    }

    [Fact]
    public async Task UserCanAccessContactInfoAfterSuccessfulPayment()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var viewerUser = new ApplicationUser { Id = 10, FullName = "Viewer User", Email = "viewer@example.com" };
        var candidateUser = new ApplicationUser { Id = 20, FullName = "Candidate User", Email = "candidate@example.com", PhoneNumber = "01799998888" };
        db.Users.AddRange(viewerUser, candidateUser);

        var candidateProfile = new UserProfile
        {
            Id = 100,
            UserId = candidateUser.Id,
            User = candidateUser,
            FullName = "Candidate User",
            Gender = Gender.Female,
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            City = "Dhanmondi",
            District = "Dhaka",
            Division = "Dhaka",
            Country = "Bangladesh",
            IsActive = true
        };
        var viewerProfile = new UserProfile
        {
            Id = 99,
            UserId = viewerUser.Id,
            User = viewerUser,
            FullName = "Viewer User",
            Gender = Gender.Male,
            DateOfBirth = DateTime.UtcNow.AddYears(-27),
            City = "Gulshan",
            District = "Dhaka",
            Division = "Dhaka",
            Country = "Bangladesh",
            IsActive = true,
            IsVerified = true
        };
        db.UserProfiles.AddRange(candidateProfile, viewerProfile);

        // Simulate successful payment & unlock record
        var payment = new Payment
        {
            Id = 501,
            UserId = viewerUser.Id,
            TargetProfileId = candidateProfile.Id,
            Amount = 500m,
            TransactionId = "TXN-TEST-123",
            Status = PaymentStatus.Successful,
            CreatedAt = DateTime.UtcNow
        };
        db.Payments.Add(payment);

        var unlockAccess = new ContactAccess
        {
            Id = 1,
            UserId = viewerUser.Id,
            TargetProfileId = candidateProfile.Id,
            PaymentId = payment.Id,
            UnlockedAt = DateTime.UtcNow
        };
        db.ContactAccesses.Add(unlockAccess);
        await db.SaveChangesAsync();

        var paymentOptions = Options.Create(new PaymentGatewayOptions { ContactUnlockFee = 500m });
        var contactAccessService = new ContactAccessService(db, paymentOptions);
        var fileStorageMock = new Mock<IFileStorageService>();
        var profileService = new ProfileService(db, fileStorageMock.Object, contactAccessService, NullLogger<ProfileService>.Instance);

        // Act
        var result = await profileService.GetProfileByIdAsync(candidateProfile.Id, requestingUserId: viewerUser.Id);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.IsContactUnlocked.Should().BeTrue();
        result.Data.ContactPhone.Should().Be("01799998888");
        result.Data.ContactEmail.Should().Be("candidate@example.com");
    }

    [Fact]
    public async Task AdminCannotApproveOwnNidVerification()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var adminUser = new ApplicationUser { Id = 1, FullName = "Admin User", Email = "admin@matrimonyhub.com" };
        db.Users.Add(adminUser);

        var selfVerification = new NidVerification
        {
            Id = 77,
            UserId = adminUser.Id, // Same as admin!
            User = adminUser,
            NidNumber = "19942691234567890",
            FrontDocumentUrl = "/uploads/verifications/front.jpg",
            Status = NidVerificationStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };
        db.NidVerifications.Add(selfVerification);
        await db.SaveChangesAsync();

        var fileStorageMock = new Mock<IFileStorageService>();
        var notifMock = new Mock<INotificationService>();
        var emailMock = new Mock<IEmailService>();
        var adminService = new AdminService(db);

        var nidService = new NidVerificationService(
            db, fileStorageMock.Object, notifMock.Object, emailMock.Object, adminService);

        // Act: Admin attempts to approve their own verification
        var reviewDto = new NidReviewDto
        {
            VerificationId = selfVerification.Id,
            Status = NidVerificationStatus.Approved
        };
        var result = await nidService.ReviewVerificationAsync(adminUserId: adminUser.Id, reviewDto);

        // Assert: Security violation must be enforced
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("Security violation");
    }

    [Fact]
    public async Task PrivateNidInformationIsNotExposedInPublicProfileDto()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var candidate = new ApplicationUser { Id = 30, FullName = "Private User", Email = "private@example.com" };
        db.Users.Add(candidate);

        var profile = new UserProfile
        {
            Id = 300,
            UserId = candidate.Id,
            User = candidate,
            FullName = "Private User",
            Gender = Gender.Male,
            DateOfBirth = DateTime.UtcNow.AddYears(-28),
            IsActive = true
        };
        db.UserProfiles.Add(profile);

        var nid = new NidVerification
        {
            Id = 99,
            UserId = candidate.Id,
            NidNumber = "99887766554433221",
            FrontDocumentUrl = "/uploads/verifications/secret_nid_doc.jpg",
            Status = NidVerificationStatus.Approved,
            SubmittedAt = DateTime.UtcNow
        };
        db.NidVerifications.Add(nid);
        await db.SaveChangesAsync();

        var paymentOptions = Options.Create(new PaymentGatewayOptions { ContactUnlockFee = 500m });
        var contactAccessService = new ContactAccessService(db, paymentOptions);
        var fileStorageMock = new Mock<IFileStorageService>();
        var profileService = new ProfileService(db, fileStorageMock.Object, contactAccessService, NullLogger<ProfileService>.Instance);

        // Act
        var result = await profileService.GetProfileByIdAsync(profile.Id, requestingUserId: 999);

        // Assert: ProfileDetailDto has no properties containing NidNumber or secret Document scans
        result.Data.Should().NotBeNull();
        typeof(ProfileDetailDto).GetProperty("NidNumber").Should().BeNull("Public profile DTO must never have a NidNumber property");
        typeof(ProfileCardDto).GetProperty("NidNumber").Should().BeNull("Match card DTO must never have a NidNumber property");
    }

    [Fact]
    public async Task ToggleFavorite_PreventsSelfFavoriting()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var user = new ApplicationUser { Id = 5, FullName = "Self User", Email = "self@example.com" };
        db.Users.Add(user);

        var profile = new UserProfile
        {
            Id = 50,
            UserId = user.Id,
            FullName = "Self User",
            Gender = Gender.Male,
            DateOfBirth = DateTime.UtcNow.AddYears(-27)
        };
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();

        var notifMock = new Mock<INotificationService>();
        var favService = new FavoriteService(db, notifMock.Object);

        // Act
        var result = await favService.ToggleFavoriteAsync(user.Id, profile.Id);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("own profile");
    }

    [Fact]
    public void MatchScoring_AccuratelyWeightsMatchingAttributes()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var matchService = new MatchService(db);

        var viewer = new UserProfile
        {
            Gender = Gender.Male,
            DateOfBirth = DateTime.UtcNow.AddYears(-28),
            Religion = Religion.Islam,
            Division = "Dhaka",
            HighestEducation = "B.Sc in Computer Science",
            DietaryPreference = DietaryPreference.Halal,
            PartnerPreference = new PartnerPreference
            {
                MinAge = 22,
                MaxAge = 27,
                PreferredReligion = Religion.Islam,
                PreferredDivision = "Dhaka"
            }
        };

        var perfectCandidate = new UserProfile
        {
            Gender = Gender.Female,
            DateOfBirth = DateTime.UtcNow.AddYears(-25), // In [22, 27]
            Religion = Religion.Islam,
            Division = "Dhaka",
            HighestEducation = "B.Sc in Software Engineering",
            DietaryPreference = DietaryPreference.Halal,
            IsVerified = true
        };

        var mismatchedCandidate = new UserProfile
        {
            Gender = Gender.Female,
            DateOfBirth = DateTime.UtcNow.AddYears(-38), // Far outside [22, 27]
            Religion = Religion.Hinduism,
            Division = "Sylhet",
            HighestEducation = "None",
            DietaryPreference = DietaryPreference.Vegetarian,
            IsVerified = false
        };

        // Act
        var perfectScore = matchService.CalculateMatchScore(viewer, perfectCandidate);
        var mismatchScore = matchService.CalculateMatchScore(viewer, mismatchedCandidate);

        // Assert
        perfectScore.Should().BeGreaterThan(80, "A well-matched partner should have a high score");
        mismatchScore.Should().BeLessThan(40, "A mismatched partner should have a low score");
        perfectScore.Should().BeGreaterThan(mismatchScore);
    }

    [Fact]
    public async Task UnverifiedUserCannotInitiatePaymentOrUnlockContact()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var unverifiedUser = new ApplicationUser { Id = 31, FullName = "Unverified User", Email = "unverified@example.com" };
        var candidateUser = new ApplicationUser { Id = 32, FullName = "Candidate User", Email = "candidate2@example.com", PhoneNumber = "01811223344" };
        db.Users.AddRange(unverifiedUser, candidateUser);

        var unverifiedProfile = new UserProfile
        {
            Id = 201,
            UserId = unverifiedUser.Id,
            User = unverifiedUser,
            FullName = "Unverified User",
            Gender = Gender.Male,
            DateOfBirth = DateTime.UtcNow.AddYears(-28),
            IsActive = true,
            IsVerified = false // NOT verified!
        };

        var candidateProfile = new UserProfile
        {
            Id = 202,
            UserId = candidateUser.Id,
            User = candidateUser,
            FullName = "Candidate User",
            Gender = Gender.Female,
            DateOfBirth = DateTime.UtcNow.AddYears(-24),
            IsActive = true,
            IsVerified = true
        };
        db.UserProfiles.AddRange(unverifiedProfile, candidateProfile);
        await db.SaveChangesAsync();

        var paymentOptions = Options.Create(new PaymentGatewayOptions { ContactUnlockFee = 500m });
        var contactAccessService = new ContactAccessService(db, paymentOptions);
        var mockGateway = new Mock<IPaymentGateway>();
        mockGateway.Setup(g => g.GatewayType).Returns(PaymentGateway.Sandbox);
        mockGateway.Setup(g => g.InitiatePaymentAsync(It.IsAny<Payment>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new PaymentResultDto { IsSuccess = true, CheckoutUrl = "https://example.com/checkout" });

        var paymentService = new PaymentService(
            db,
            new[] { mockGateway.Object },
            contactAccessService,
            new Mock<INotificationService>().Object,
            new Mock<IEmailService>().Object,
            paymentOptions,
            NullLogger<PaymentService>.Instance);

        // Act 1: Attempt to initiate payment
        var initResult = await paymentService.InitiateContactUnlockPaymentAsync(
            unverifiedUser.Id, candidateProfile.Id, PaymentGateway.Sandbox, "https://return.test", "https://cancel.test");

        // Act 2: Attempt direct unlock
        var unlockResult = await contactAccessService.UnlockContactAsync(unverifiedUser.Id, candidateProfile.Id, 999);

        // Act 3: Check contact access
        var hasAccess = await contactAccessService.HasAccessAsync(unverifiedUser.Id, candidateProfile.Id);

        // Assert
        initResult.Succeeded.Should().BeFalse();
        initResult.Message.Should().Contain("Identity Verification Required");
        unlockResult.Succeeded.Should().BeFalse();
        unlockResult.Message.Should().Contain("Identity Verification Required");
        hasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task UnverifiedUserCannotViewContactInfoEvenWithPriorPaymentRecord()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var user = new ApplicationUser { Id = 41, FullName = "Revoked User", Email = "revoked@example.com" };
        var candidateUser = new ApplicationUser { Id = 42, FullName = "Candidate User 3", Email = "target3@example.com", PhoneNumber = "01999887766" };
        db.Users.AddRange(user, candidateUser);

        var profile = new UserProfile
        {
            Id = 301,
            UserId = user.Id,
            User = user,
            FullName = "Revoked User",
            Gender = Gender.Male,
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            IsActive = true,
            IsVerified = false // Unverified or verification revoked
        };

        var candidateProfile = new UserProfile
        {
            Id = 302,
            UserId = candidateUser.Id,
            User = candidateUser,
            FullName = "Candidate User 3",
            Gender = Gender.Female,
            DateOfBirth = DateTime.UtcNow.AddYears(-26),
            City = "Uttara",
            District = "Dhaka",
            Division = "Dhaka",
            Country = "Bangladesh",
            IsActive = true,
            IsVerified = true
        };
        db.UserProfiles.AddRange(profile, candidateProfile);

        // Existing ContactAccess record in DB
        db.ContactAccesses.Add(new ContactAccess
        {
            Id = 55,
            UserId = user.Id,
            TargetProfileId = candidateProfile.Id,
            PaymentId = 999,
            UnlockedAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var paymentOptions = Options.Create(new PaymentGatewayOptions { ContactUnlockFee = 500m });
        var contactAccessService = new ContactAccessService(db, paymentOptions);
        var fileStorageMock = new Mock<IFileStorageService>();
        var profileService = new ProfileService(db, fileStorageMock.Object, contactAccessService, NullLogger<ProfileService>.Instance);

        // Act
        var hasAccess = await contactAccessService.HasAccessAsync(user.Id, candidateProfile.Id);
        var profileResult = await profileService.GetProfileByIdAsync(candidateProfile.Id, requestingUserId: user.Id);

        // Assert
        hasAccess.Should().BeFalse();
        profileResult.Succeeded.Should().BeTrue();
        profileResult.Data!.IsContactUnlocked.Should().BeFalse();
        profileResult.Data.ContactPhone.Should().BeNull();
        profileResult.Data.ContactEmail.Should().BeNull();
        profileResult.Data.ContactAddress.Should().BeNull();
    }
}
