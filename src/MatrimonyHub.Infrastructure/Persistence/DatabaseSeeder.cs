using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MatrimonyHub.Domain.Entities;
using MatrimonyHub.Domain.Enums;

namespace MatrimonyHub.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        logger.LogInformation("Starting database seeding...");

        // 1. Roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role) { Description = $"{role} Role" });
                logger.LogInformation("Created role: {Role}", role);
            }
        }

        // 2. Admin User
        var adminEmail = "admin@matrimonyhub.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                PhoneNumber = "01700000001",
                EmailConfirmed = true,
                AccountStatus = UserAccountStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var createRes = await userManager.CreateAsync(adminUser, "Admin@Pass123!");
            if (createRes.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user seeded: {Email}", adminEmail);
            }
        }

        // 3. Realistic Demo Users & Profiles
        var demoUsers = new List<(string Name, string Email, string Phone, Gender Gender, DateTime Dob, string Education, string Inst, string Subj, string Occ, string Comp, string Job, string Inc, string Div, string Dist, string City, Religion Rel, MaritalStatus Mar, string Hobbies, string About, string PrefDiv, int MinA, int MaxA, bool Verified)>
        {
            ("Tanvir Ahmed", "tanvir.ahmed@example.com", "01711223344", Gender.Male, new DateTime(1994, 5, 14), "B.Sc in Computer Science", "BUET", "CSE", "Software Engineer", "BrainStation-23", "Lead Architect", "1.5L - 2.5L BDT", "Dhaka", "Dhaka", "Gulshan", Religion.Islam, MaritalStatus.NeverMarried, "Reading, Coding, Chess, Photography", "Practicing Muslim, passionate software engineer living in Dhaka. Looking for a pious, family-oriented partner with similar values.", "Dhaka", 22, 28, true),
            ("Nusrat Jahan", "nusrat.jahan@example.com", "01811223344", Gender.Female, new DateTime(1997, 8, 22), "MBBS, FCPS Part-1", "Dhaka Medical College", "Medicine", "Doctor", "Square Hospitals", "Medical Officer", "80K - 1.2L BDT", "Dhaka", "Dhaka", "Dhanmondi", Religion.Islam, MaritalStatus.NeverMarried, "Cooking, Travelling, Gardening", "Dedicated medical doctor from a respected and educated family in Dhaka. Value mutual respect, sincerity, and warmth in marriage.", "Dhaka", 27, 34, true),
            ("Rahim Chowdhury", "rahim.chowdhury@example.com", "01911223344", Gender.Male, new DateTime(1992, 11, 3), "BBA & MBA in Finance", "IBA, University of Dhaka", "Finance", "Banker", "Eastern Bank Ltd", "Senior Manager", "1.2L - 1.8L BDT", "Chittagong", "Chittagong", "Agrabad", Religion.Islam, MaritalStatus.NeverMarried, "Swimming, Travelling, Badminton", "Grounded, ambitious corporate professional based in Chittagong. Seeking a caring and educated life companion.", "Chittagong", 23, 29, true),
            ("Sadia Islam", "sadia.islam@example.com", "01722334455", Gender.Female, new DateTime(1996, 3, 18), "M.Sc in Architecture", "BUET", "Architecture", "Architect", "Studio Morphogenesis", "Senior Architect", "1.0L - 1.5L BDT", "Dhaka", "Dhaka", "Uttara", Religion.Islam, MaritalStatus.NeverMarried, "Painting, Interior Design, Hiking", "Creative architect with an artistic flair and traditional family principles. Seeking a supportive and kind partner.", "Dhaka", 28, 35, true),
            ("Kazi Farhan", "kazi.farhan@example.com", "01822334455", Gender.Male, new DateTime(1995, 7, 9), "B.Sc in Civil Engineering", "CUET", "Civil Engineering", "Project Engineer", "Max Group", "Project Manager", "90K - 1.4L BDT", "Sylhet", "Sylhet", "Zindabazar", Religion.Islam, MaritalStatus.NeverMarried, "Football, Photography, Camping", "Friendly, active civil engineer stationed between Sylhet and Dhaka. Value family bonds, humility, and trust.", "Sylhet", 21, 27, false),
            ("Anika Tabassum", "anika.tabassum@example.com", "01922334455", Gender.Female, new DateTime(1998, 12, 5), "B.A & M.A in English Literature", "University of Dhaka", "English", "Lecturer", "BRAC University", "Lecturer", "70K - 1.0L BDT", "Dhaka", "Dhaka", "Banani", Religion.Islam, MaritalStatus.NeverMarried, "Literature, Violin, Exploring cafes", "Academic with a modern outlook and deep moral principles. Looking for a broad-minded and honest life partner.", "Dhaka", 26, 33, true),
            ("Sourav Roy", "sourav.roy@example.com", "01733445566", Gender.Male, new DateTime(1993, 9, 12), "B.Sc in EEE", "AUST", "Electrical & Electronic", "Telecom Engineer", "Grameenphone", "Principal Engineer", "1.3L - 2.0L BDT", "Dhaka", "Dhaka", "Mirpur", Religion.Hinduism, MaritalStatus.NeverMarried, "Music, Cricket, Sci-Fi", "Calm and pragmatic engineer with a love for traditional values. Seeking an educated, affectionate partner.", "Dhaka", 24, 30, true),
            ("Puja Sen", "puja.sen@example.com", "01833445566", Gender.Female, new DateTime(1996, 6, 25), "MBA in Marketing", "Jahangirnagar University", "Marketing", "Brand Manager", "Unilever Bangladesh", "Brand Manager", "1.1L - 1.6L BDT", "Chittagong", "Chittagong", "Nasirabad", Religion.Hinduism, MaritalStatus.NeverMarried, "Dancing, Yoga, Reading", "Dynamic marketing specialist from Chittagong. Seeking a caring partner who values family traditions and career growth.", "Chittagong", 27, 33, true)
        };

        var seededProfiles = new List<UserProfile>();

        foreach (var item in demoUsers)
        {
            var user = await userManager.FindByEmailAsync(item.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = item.Email,
                    Email = item.Email,
                    PhoneNumber = item.Phone,
                    FullName = item.Name,
                    EmailConfirmed = true,
                    AccountStatus = UserAccountStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(10, 60))
                };

                var res = await userManager.CreateAsync(user, "User@Pass123!");
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");

                    var avatarGender = item.Gender == Gender.Male ? "men" : "women";
                    var avatarId = Random.Shared.Next(1, 95);
                    var photoUrl = $"https://randomuser.me/api/portraits/{avatarGender}/{avatarId}.jpg";

                    var profile = new UserProfile
                    {
                        UserId = user.Id,
                        FullName = item.Name,
                        Gender = item.Gender,
                        DateOfBirth = item.Dob,
                        HeightCm = item.Gender == Gender.Male ? Random.Shared.Next(170, 185) : Random.Shared.Next(155, 168),
                        WeightKg = item.Gender == Gender.Male ? Random.Shared.Next(68, 85) : Random.Shared.Next(50, 65),
                        MaritalStatus = item.Mar,
                        Religion = item.Rel,
                        MotherTongue = "Bengali",
                        Nationality = "Bangladeshi",
                        HighestEducation = item.Education,
                        Institution = item.Inst,
                        Subject = item.Subj,
                        GraduationYear = item.Dob.Year + 24,
                        Occupation = item.Occ,
                        Company = item.Comp,
                        JobTitle = item.Job,
                        IncomeRange = item.Inc,
                        Division = item.Div,
                        District = item.Dist,
                        City = item.City,
                        Country = "Bangladesh",
                        Smoking = false,
                        Drinking = false,
                        DietaryPreference = item.Rel == Religion.Islam ? DietaryPreference.Halal : DietaryPreference.Vegetarian,
                        Hobbies = item.Hobbies,
                        Interests = "Family time, Community, Personal growth",
                        AboutMe = item.About,
                        FamilyInfo = "Well-established, respectable family with modern educational background and traditional cultural values.",
                        PartnerExpectations = "Looking for a kind, honest, and understanding partner with strong moral character.",
                        IsVerified = item.Verified,
                        CreatedAt = user.CreatedAt,
                        PartnerPreference = new PartnerPreference
                        {
                            PreferredGender = item.Gender == Gender.Male ? Gender.Female : Gender.Male,
                            PreferredReligion = item.Rel,
                            PreferredMaritalStatus = MaritalStatus.NeverMarried,
                            PreferredDivision = item.PrefDiv,
                            MinAge = item.MinA,
                            MaxAge = item.MaxA,
                            CreatedAt = user.CreatedAt
                        }
                    };

                    db.UserProfiles.Add(profile);
                    await db.SaveChangesAsync();

                    // Add primary photo
                    db.ProfilePhotos.Add(new ProfilePhoto
                    {
                        UserProfileId = profile.Id,
                        PhotoUrl = photoUrl,
                        IsPrimary = true,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Add NID record if verified
                    if (item.Verified)
                    {
                        db.NidVerifications.Add(new NidVerification
                        {
                            UserId = user.Id,
                            NidNumber = $"199{Random.Shared.Next(1000000, 9999999)}",
                            FrontDocumentUrl = "/images/sample-nid-front.png",
                            BackDocumentUrl = "/images/sample-nid-back.png",
                            Status = NidVerificationStatus.Approved,
                            SubmittedAt = user.CreatedAt.AddHours(2),
                            ReviewedAt = user.CreatedAt.AddHours(5),
                            ReviewedByAdminId = adminUser.Id
                        });
                    }

                    seededProfiles.Add(profile);
                    logger.LogInformation("Seeded demo profile for {Email}", item.Email);
                }
            }
        }

        await db.SaveChangesAsync();

        // 4. Sample Success Stories
        if (!await db.SuccessStories.AnyAsync())
        {
            db.SuccessStories.AddRange(
                new SuccessStory
                {
                    CoupleNames = "Tahsin & Farhana",
                    StoryTitle = "Found My Soulmate in Just 3 Months",
                    StoryDescription = "We both joined Matrimony Hub seeking genuine life partners with aligned Islamic values. After unlocking contacts and having families meet, everything progressed with grace and blessing.",
                    PhotoUrl = "https://images.unsplash.com/photo-1583939003579-730e3918a45a?auto=format&fit=crop&w=800&q=80",
                    MarriageDate = new DateTime(2025, 12, 10),
                    Location = "Dhaka, Bangladesh",
                    Status = SuccessStoryStatus.Approved,
                    ApprovedByAdminId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                },
                new SuccessStory
                {
                    CoupleNames = "Saimon & Nazia",
                    StoryTitle = "A Modern Match Rooted in Mutual Respect",
                    StoryDescription = "Finding someone who respects both your career aspirations and family heritage felt daunting until Matrimony Hub. The verified profiles gave our parents absolute peace of mind.",
                    PhotoUrl = "https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=800&q=80",
                    MarriageDate = new DateTime(2026, 1, 15),
                    Location = "Chittagong, Bangladesh",
                    Status = SuccessStoryStatus.Approved,
                    ApprovedByAdminId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2)
                },
                new SuccessStory
                {
                    CoupleNames = "Zubair & Tasnim",
                    StoryTitle = "From Match Score to Nikah",
                    StoryDescription = "Our 95% match compatibility score proved surprisingly accurate! From mutual interests in travel to shared spiritual values, we couldn't be happier.",
                    PhotoUrl = "https://images.unsplash.com/photo-1511285560929-80b456fea0bc?auto=format&fit=crop&w=800&q=80",
                    MarriageDate = new DateTime(2026, 2, 20),
                    Location = "Sylhet, Bangladesh",
                    Status = SuccessStoryStatus.Approved,
                    ApprovedByAdminId = adminUser.Id,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                }
            );

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded approved success stories.");
        }

        // 5. Sample Payments & Unlocked Contacts
        if (!await db.Payments.AnyAsync() && seededProfiles.Count >= 2)
        {
            var user1 = await userManager.FindByEmailAsync("tanvir.ahmed@example.com");
            var profile2 = seededProfiles.FirstOrDefault(p => p.FullName == "Nusrat Jahan");

            if (user1 != null && profile2 != null)
            {
                var txnId = $"TXN{DateTime.UtcNow:yyyyMMdd}8899";
                var payment = new Payment
                {
                    UserId = user1.Id,
                    TargetProfileId = profile2.Id,
                    Amount = 500.00m,
                    Currency = "BDT",
                    PaymentPurpose = PaymentPurpose.ContactUnlock,
                    TransactionId = txnId,
                    Gateway = PaymentGateway.bKash,
                    GatewayTransactionId = "BK-TRX-998822",
                    Status = PaymentStatus.Successful,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    CompletedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(1)
                };

                db.Payments.Add(payment);
                await db.SaveChangesAsync();

                db.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentId = payment.Id,
                    GatewayTransactionId = payment.GatewayTransactionId,
                    GatewayResponse = "trxStatus=Completed",
                    Amount = 500.00m,
                    Status = PaymentStatus.Successful,
                    CreatedAt = payment.CompletedAt.Value
                });

                db.ContactAccesses.Add(new ContactAccess
                {
                    UserId = user1.Id,
                    TargetProfileId = profile2.Id,
                    PaymentId = payment.Id,
                    UnlockedAt = payment.CompletedAt.Value
                });

                await db.SaveChangesAsync();
                logger.LogInformation("Seeded sample payment and contact access.");
            }
        }

        logger.LogInformation("Database seeding completed.");
    }
}
