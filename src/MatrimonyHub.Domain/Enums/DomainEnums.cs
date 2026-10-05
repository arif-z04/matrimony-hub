namespace MatrimonyHub.Domain.Enums;

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum MaritalStatus
{
    NeverMarried = 1,
    Divorced = 2,
    Widowed = 3,
    AwaitingDivorce = 4
}

public enum Religion
{
    Islam = 1,
    Hinduism = 2,
    Christianity = 3,
    Buddhism = 4,
    Other = 5
}

public enum DietaryPreference
{
    Any = 1,
    Halal = 2,
    Vegetarian = 3,
    NonVegetarian = 4,
    Vegan = 5
}

public enum NidVerificationStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Successful = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}

public enum PaymentGateway
{
    bKash = 1,
    SSLCommerz = 2,
    Nagad = 3,
    Sandbox = 4
}

public enum PaymentPurpose
{
    ContactUnlock = 1,
    PremiumMembership = 2
}

public enum NotificationType
{
    ProfileViewed = 1,
    FavoriteAdded = 2,
    VerificationApproved = 3,
    VerificationRejected = 4,
    PaymentSuccessful = 5,
    ContactUnlocked = 6,
    SystemNotification = 7
}

public enum SuccessStoryStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum UserAccountStatus
{
    Active = 1,
    Suspended = 2,
    Deactivated = 3
}
