using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Application.Interfaces;
using MatrimonyHub.Domain.Entities;

namespace MatrimonyHub.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    int,
    IdentityUserClaim<int>,
    IdentityUserRole<int>,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>,
    IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ProfilePhoto> ProfilePhotos => Set<ProfilePhoto>();
    public DbSet<PartnerPreference> PartnerPreferences => Set<PartnerPreference>();
    public DbSet<NidVerification> NidVerifications => Set<NidVerification>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<ContactAccess> ContactAccesses => Set<ContactAccess>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SuccessStory> SuccessStories => Set<SuccessStory>();
    public DbSet<AdminLog> AdminLogs => Set<AdminLog>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
            b.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            b.HasIndex(u => u.Email).IsUnique();
            b.HasIndex(u => u.PhoneNumber);
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
            b.Property(r => r.Description).HasMaxLength(250);
        });

        builder.Entity<IdentityUserRole<int>>(b => b.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<int>>(b => b.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<int>>(b => b.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<int>>(b => b.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<int>>(b => b.ToTable("UserTokens"));

        // UserProfile
        builder.Entity<UserProfile>(b =>
        {
            b.ToTable("Profiles");
            b.HasKey(p => p.Id);

            b.HasOne(p => p.User)
             .WithOne(u => u.Profile)
             .HasForeignKey<UserProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(p => p.FullName).HasMaxLength(150).IsRequired();
            b.Property(p => p.MotherTongue).HasMaxLength(50).HasDefaultValue("Bengali");
            b.Property(p => p.Nationality).HasMaxLength(50).HasDefaultValue("Bangladeshi");
            b.Property(p => p.HighestEducation).HasMaxLength(100);
            b.Property(p => p.Institution).HasMaxLength(150);
            b.Property(p => p.Subject).HasMaxLength(100);
            b.Property(p => p.Occupation).HasMaxLength(100);
            b.Property(p => p.Company).HasMaxLength(150);
            b.Property(p => p.JobTitle).HasMaxLength(100);
            b.Property(p => p.IncomeRange).HasMaxLength(50);
            b.Property(p => p.Division).HasMaxLength(50);
            b.Property(p => p.District).HasMaxLength(50);
            b.Property(p => p.City).HasMaxLength(50);
            b.Property(p => p.Country).HasMaxLength(50).HasDefaultValue("Bangladesh");
            b.Property(p => p.Hobbies).HasMaxLength(255);
            b.Property(p => p.Interests).HasMaxLength(255);

            b.HasIndex(p => p.Gender);
            b.HasIndex(p => p.DateOfBirth);
            b.HasIndex(p => p.Religion);
            b.HasIndex(p => p.MaritalStatus);
            b.HasIndex(p => p.Division);
            b.HasIndex(p => p.IsVerified);
            b.HasIndex(p => p.IsActive);
        });

        // ProfilePhoto
        builder.Entity<ProfilePhoto>(b =>
        {
            b.ToTable("ProfilePhotos");
            b.HasKey(p => p.Id);

            b.HasOne(p => p.UserProfile)
             .WithMany(u => u.Photos)
             .HasForeignKey(p => p.UserProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(p => p.PhotoUrl).HasMaxLength(255).IsRequired();
        });

        // PartnerPreference
        builder.Entity<PartnerPreference>(b =>
        {
            b.ToTable("PartnerPreferences");
            b.HasKey(p => p.Id);

            b.HasOne(p => p.UserProfile)
             .WithOne(u => u.PartnerPreference)
             .HasForeignKey<PartnerPreference>(p => p.UserProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(p => p.PreferredEducation).HasMaxLength(100);
            b.Property(p => p.PreferredOccupation).HasMaxLength(100);
            b.Property(p => p.PreferredDivision).HasMaxLength(50);
            b.Property(p => p.Notes).HasMaxLength(500);
        });

        // NidVerification
        builder.Entity<NidVerification>(b =>
        {
            b.ToTable("NidVerifications");
            b.HasKey(n => n.Id);

            b.HasOne(n => n.User)
             .WithMany(u => u.NidVerifications)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(n => n.ReviewedByAdmin)
             .WithMany()
             .HasForeignKey(n => n.ReviewedByAdminId)
             .OnDelete(DeleteBehavior.SetNull);

            b.Property(n => n.NidNumber).HasMaxLength(50).IsRequired();
            b.Property(n => n.FrontDocumentUrl).HasMaxLength(255).IsRequired();
            b.Property(n => n.BackDocumentUrl).HasMaxLength(255);
            b.Property(n => n.RejectionReason).HasMaxLength(500);

            b.HasIndex(n => n.Status);
            b.HasIndex(n => n.SubmittedAt);
        });

        // Favorite
        builder.Entity<Favorite>(b =>
        {
            b.ToTable("Favorites");
            b.HasKey(f => f.Id);

            b.HasOne(f => f.User)
             .WithMany(u => u.FavoritesGiven)
             .HasForeignKey(f => f.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(f => f.FavoriteProfile)
             .WithMany(p => p.FavoritedBy)
             .HasForeignKey(f => f.FavoriteProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            // DB Constraint: prevent duplicate favorites
            b.HasIndex(f => new { f.UserId, f.FavoriteProfileId }).IsUnique();
        });

        // ContactAccess
        builder.Entity<ContactAccess>(b =>
        {
            b.ToTable("ContactAccess");
            b.HasKey(c => c.Id);

            b.HasOne(c => c.User)
             .WithMany(u => u.ContactAccessesGiven)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(c => c.TargetProfile)
             .WithMany(p => p.ContactUnlockedBy)
             .HasForeignKey(c => c.TargetProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(c => c.Payment)
             .WithOne(p => p.ContactAccess)
             .HasForeignKey<ContactAccess>(c => c.PaymentId)
             .OnDelete(DeleteBehavior.Restrict);

            // DB Constraint: prevent duplicate contact access for same target
            b.HasIndex(c => new { c.UserId, c.TargetProfileId }).IsUnique();
        });

        // Payment
        builder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");
            b.HasKey(p => p.Id);

            b.HasOne(p => p.User)
             .WithMany(u => u.Payments)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.TargetProfile)
             .WithMany()
             .HasForeignKey(p => p.TargetProfileId)
             .OnDelete(DeleteBehavior.SetNull);

            b.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
            b.Property(p => p.Currency).HasMaxLength(10).HasDefaultValue("BDT");
            b.Property(p => p.TransactionId).HasMaxLength(100).IsRequired();
            b.Property(p => p.GatewayTransactionId).HasMaxLength(100);
            b.Property(p => p.FailureReason).HasMaxLength(255);

            b.HasIndex(p => p.TransactionId).IsUnique();
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.CreatedAt);
        });

        // PaymentTransaction
        builder.Entity<PaymentTransaction>(b =>
        {
            b.ToTable("Transactions");
            b.HasKey(t => t.Id);

            b.HasOne(t => t.Payment)
             .WithMany(p => p.Transactions)
             .HasForeignKey(t => t.PaymentId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(t => t.Amount).HasPrecision(18, 2);
            b.Property(t => t.GatewayTransactionId).HasMaxLength(100);
        });

        // Notification
        builder.Entity<Notification>(b =>
        {
            b.ToTable("Notifications");
            b.HasKey(n => n.Id);

            b.HasOne(n => n.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(n => n.Title).HasMaxLength(150).IsRequired();
            b.Property(n => n.Message).HasMaxLength(500).IsRequired();
            b.Property(n => n.RelatedEntityId).HasMaxLength(50);

            b.HasIndex(n => new { n.UserId, n.IsRead });
            b.HasIndex(n => n.CreatedAt);
        });

        // SuccessStory
        builder.Entity<SuccessStory>(b =>
        {
            b.ToTable("SuccessStories");
            b.HasKey(s => s.Id);

            b.HasOne(s => s.SubmittedByUser)
             .WithMany()
             .HasForeignKey(s => s.SubmittedByUserId)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(s => s.ApprovedByAdmin)
             .WithMany()
             .HasForeignKey(s => s.ApprovedByAdminId)
             .OnDelete(DeleteBehavior.SetNull);

            b.Property(s => s.CoupleNames).HasMaxLength(150).IsRequired();
            b.Property(s => s.StoryTitle).HasMaxLength(200).IsRequired();
            b.Property(s => s.PhotoUrl).HasMaxLength(255);
            b.Property(s => s.Location).HasMaxLength(100).IsRequired();

            b.HasIndex(s => s.Status);
            b.HasIndex(s => s.CreatedAt);
        });

        // AdminLog
        builder.Entity<AdminLog>(b =>
        {
            b.ToTable("AdminLogs");
            b.HasKey(l => l.Id);

            b.HasOne(l => l.AdminUser)
             .WithMany(u => u.AdminLogs)
             .HasForeignKey(l => l.AdminUserId)
             .OnDelete(DeleteBehavior.SetNull);

            b.Property(l => l.Action).HasMaxLength(100).IsRequired();
            b.Property(l => l.EntityType).HasMaxLength(100).IsRequired();
            b.Property(l => l.EntityId).HasMaxLength(50);
            b.Property(l => l.Description).HasMaxLength(1000).IsRequired();
            b.Property(l => l.IpAddress).HasMaxLength(50);
            b.Property(l => l.UserAgent).HasMaxLength(255);

            b.HasIndex(l => l.CreatedAt);
            b.HasIndex(l => l.Action);
        });

        // LoginAttempt
        builder.Entity<LoginAttempt>(b =>
        {
            b.ToTable("LoginAttempts");
            b.HasKey(a => a.Id);
            b.Property(a => a.Email).HasMaxLength(100).IsRequired();
            b.Property(a => a.IpAddress).HasMaxLength(50);
            b.HasIndex(a => new { a.Email, a.AttemptedAt });
        });
    }
}
