using Microsoft.EntityFrameworkCore;
using MatrimonyHub.Domain.Entities;

namespace MatrimonyHub.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<ProfilePhoto> ProfilePhotos { get; }
    DbSet<PartnerPreference> PartnerPreferences { get; }
    DbSet<NidVerification> NidVerifications { get; }
    DbSet<Favorite> Favorites { get; }
    DbSet<ContactAccess> ContactAccesses { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<SuccessStory> SuccessStories { get; }
    DbSet<AdminLog> AdminLogs { get; }
    DbSet<LoginAttempt> LoginAttempts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
