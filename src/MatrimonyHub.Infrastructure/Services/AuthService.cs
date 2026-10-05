using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MatrimonyHub.Application.Common;
using MatrimonyHub.Application.DTOs;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;
using MatrimonyHub.Infrastructure.Persistence;

namespace MatrimonyHub.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext db,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ServiceResult<UserSummaryDto>> RegisterAsync(RegisterRequestDto request, string? ipAddress = null)
    {
        // 1. Check uniqueness
        var existingEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            return ServiceResult<UserSummaryDto>.Failure("An account with this email address already exists.");
        }

        var existingPhone = await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (existingPhone)
        {
            return ServiceResult<UserSummaryDto>.Failure("An account with this phone number already exists.");
        }

        // Validate age >= 18
        var age = DateTime.UtcNow.Year - request.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < request.DateOfBirth.DayOfYear ? 1 : 0);
        if (age < 18)
        {
            return ServiceResult<UserSummaryDto>.Failure("You must be at least 18 years of age to register.");
        }

        // 2. Create User
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            EmailConfirmed = true,
            AccountStatus = UserAccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResult<UserSummaryDto>.Failure(errors, "Registration failed.");
        }

        // Add Role
        await _userManager.AddToRoleAsync(user, "User");

        // 3. Create initial Matrimonial Profile
        var profile = new UserProfile
        {
            UserId = user.Id,
            FullName = request.FullName,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Country = "Bangladesh",
            CreatedAt = DateTime.UtcNow,
            PartnerPreference = new PartnerPreference
            {
                PreferredGender = request.Gender == Gender.Male ? Gender.Female : (request.Gender == Gender.Female ? Gender.Male : null),
                MinAge = Math.Max(18, age - 5),
                MaxAge = age + 5,
                CreatedAt = DateTime.UtcNow
            }
        };

        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();

        // 4. Send Welcome notification & email
        _db.Notifications.Add(new Notification
        {
            UserId = user.Id,
            Title = "Welcome to Matrimony Hub!",
            Message = "Complete your profile and submit your National ID (NID) for verification to gain trust and discover your ideal life partner.",
            Type = NotificationType.SystemNotification,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName);
        _logger.LogInformation("New user registered successfully: {Email} (Id: {Id})", user.Email, user.Id);

        return ServiceResult<UserSummaryDto>.Success(new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? "",
            Role = "User",
            IsVerified = false,
            ProfileId = profile.Id
        }, "Registration successful.");
    }

    public async Task<ServiceResult<UserSummaryDto>> LoginAsync(LoginRequestDto request, string? ipAddress = null)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        var attempt = new LoginAttempt
        {
            Email = request.Email,
            IpAddress = ipAddress,
            AttemptedAt = DateTime.UtcNow
        };

        if (user == null || user.IsDeleted)
        {
            attempt.Successful = false;
            _db.LoginAttempts.Add(attempt);
            await _db.SaveChangesAsync();
            return ServiceResult<UserSummaryDto>.Failure("Invalid email or password.");
        }

        if (user.AccountStatus == UserAccountStatus.Suspended)
        {
            attempt.Successful = false;
            _db.LoginAttempts.Add(attempt);
            await _db.SaveChangesAsync();
            return ServiceResult<UserSummaryDto>.Failure("Your account has been suspended. Please contact support.");
        }

        if (user.AccountStatus == UserAccountStatus.Deactivated)
        {
            attempt.Successful = false;
            _db.LoginAttempts.Add(attempt);
            await _db.SaveChangesAsync();
            return ServiceResult<UserSummaryDto>.Failure("This account has been deactivated.");
        }

        var signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            attempt.Successful = false;
            _db.LoginAttempts.Add(attempt);
            await _db.SaveChangesAsync();

            if (signInResult.IsLockedOut)
                return ServiceResult<UserSummaryDto>.Failure("Account is temporarily locked due to multiple failed login attempts. Please try again later.");

            return ServiceResult<UserSummaryDto>.Failure("Invalid email or password.");
        }

        attempt.Successful = true;
        user.LastLoginAt = DateTime.UtcNow;
        _db.LoginAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "User";

        var profile = await _db.UserProfiles
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        var primaryPhoto = profile?.Photos.FirstOrDefault(p => p.IsPrimary)?.PhotoUrl
                           ?? profile?.Photos.FirstOrDefault()?.PhotoUrl;

        return ServiceResult<UserSummaryDto>.Success(new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? "",
            Role = primaryRole,
            IsVerified = profile?.IsVerified ?? false,
            ProfileId = profile?.Id,
            PrimaryPhotoUrl = primaryPhoto
        }, "Login successful.");
    }

    public async Task<ServiceResult> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return ServiceResult.Success("Logged out successfully.");
    }

    public async Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Do not disclose whether email exists
            return ServiceResult<string>.Success(string.Empty, "If your email is registered, you will receive password reset instructions.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendEmailAsync(user.Email!, "Reset Your Password - Matrimony Hub",
            $"<p>Hello {user.FullName},</p><p>Use the following token to reset your password:</p><p><strong>{token}</strong></p>");

        return ServiceResult<string>.Success(token, "Reset token sent.");
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return ServiceResult.Failure("Password reset failed.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResult.Failure(errors, "Password reset failed.");
        }

        return ServiceResult.Success("Password has been successfully reset. You may now log in.");
    }

    public async Task<UserSummaryDto?> GetCurrentUserSummaryAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var profile = await _db.UserProfiles
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        var primaryPhoto = profile?.Photos.FirstOrDefault(p => p.IsPrimary)?.PhotoUrl
                           ?? profile?.Photos.FirstOrDefault()?.PhotoUrl;

        return new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? "",
            Role = roles.FirstOrDefault() ?? "User",
            IsVerified = profile?.IsVerified ?? false,
            ProfileId = profile?.Id,
            PrimaryPhotoUrl = primaryPhoto
        };
    }
}
