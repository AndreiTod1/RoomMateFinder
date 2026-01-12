using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using Microsoft.EntityFrameworkCore;

namespace RoomMate_Finder.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Clear ALL data using raw SQL - check if table exists before truncating
        await context.Database.ExecuteSqlRawAsync(@"
            DO $$ 
            DECLARE
                tbl TEXT;
            BEGIN
                SET session_replication_role = 'replica';
                
                FOR tbl IN 
                    SELECT tablename FROM pg_tables 
                    WHERE schemaname = 'public' 
                    AND tablename IN ('messages', 'conversations', 'reviews', 'room_listings', 'user_actions', 'matches', 'profiles')
                LOOP
                    EXECUTE 'TRUNCATE TABLE public.' || quote_ident(tbl) || ' CASCADE';
                END LOOP;
                
                SET session_replication_role = 'origin';
            END $$;
        ");

        // 2. Create Admin
        var admin = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            PasswordHash = PasswordHasher.HashPassword("Admin1.."),
            FullName = "Super Admin",
            Role = "Admin",
            Age = 30,
            Gender = "Other",
            ProfilePicturePath = "",
            University = "Tech University",
            Bio = "System Administrator",
            Lifestyle = "Early Bird, Clean",
            Interests = "Coding, Management",
            CreatedAt = DateTime.UtcNow
        };

        // 3. Create Regular Users
        var users = new List<Profile>();
        for (int i = 1; i <= 5; i++)
        {
            users.Add(new Profile
            {
                Id = Guid.NewGuid(),
                Email = $"user{i}@test.com",
                PasswordHash = PasswordHasher.HashPassword("User123!"),
                FullName = $"User {i}",
                Role = "User",
                Age = 20 + i,
                Gender = i % 2 == 0 ? "Male" : "Female",
                ProfilePicturePath = "",
                University = "Uni " + (char)('A' + i),
                Bio = $"Regular user bio {i}",
                Lifestyle = "Night Owl, Casual",
                Interests = "Music, Reading",
                CreatedAt = DateTime.UtcNow
            });
        }

        context.Profiles.Add(admin);
        context.Profiles.AddRange(users);
        await context.SaveChangesAsync();

        // 4. Create Room Listings
        // Using Random for seeding test data - not security sensitive
#pragma warning disable S2245 // Using pseudorandom number generator for test data seeding
        var random = new Random();
        var owners = new[] { admin, users[0], users[1] };

        foreach (var owner in owners)
        {
            context.RoomListings.Add(new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                Title = $"Cozy Room by {owner.FullName}",
                Description = "A wonderful place to live with great views and amenities.",
                City = "Cluj-Napoca",
                Area = random.Next(0, 2) == 0 ? "Marasti" : "Manastur",
                Price = random.Next(200, 600),
                AvailableFrom = DateTime.UtcNow.AddDays(random.Next(1, 30)),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Amenities = "WiFi, AC, Balcony"
            });
        }

        await context.SaveChangesAsync();
    }
}
