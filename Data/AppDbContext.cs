using Microsoft.EntityFrameworkCore;
using Mw3dy.Models;
using System;

namespace Mw3dy.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Branch)
                .WithMany(b => b.Appointments)
                .HasForeignKey(a => a.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default User Id
            var defaultUserId = 1;

            // Seed User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = defaultUserId,
                    Name = "Adham Emad",
                    Email = "adham@mw3dy.com"
                }
            );

            // Seed Branches with int IDs
            modelBuilder.Entity<Branch>().HasData(
                new Branch 
                { 
                    Id = 1, 
                    NameEn = "Downtown Flagship", NameAr = "الفرع الرئيسي بوسط المدينة",
                    CityEn = "Manhattan, NY", CityAr = "مانهاتن، نيويورك",
                    AddressEn = "200 Park Ave, Floor 12", AddressAr = "200 شارع بارك، الطابق 12",
                    HoursEn = "Sun–Thu · 9:00–17:00", HoursAr = "الأحد–الخميس · 9:00–17:00",
                    DistanceKm = 1.2 
                },
                new Branch 
                { 
                    Id = 2, 
                    NameEn = "Financial District", NameAr = "الفرع المالي",
                    CityEn = "New York, NY", CityAr = "نيويورك، نيويورك",
                    AddressEn = "55 Wall Street", AddressAr = "55 شارع وول ستريت",
                    HoursEn = "Sun–Thu · 8:30–17:30", HoursAr = "الأحد–الخميس · 8:30–17:30",
                    DistanceKm = 3.4 
                },
                new Branch 
                { 
                    Id = 3, 
                    NameEn = "Brooklyn Heights", NameAr = "بروكلين هايتس",
                    CityEn = "Brooklyn, NY", CityAr = "بروكلين، نيويورك",
                    AddressEn = "180 Montague St", AddressAr = "180 شارع مونتاغ",
                    HoursEn = "Sun–Thu · 9:00–17:00", HoursAr = "الأحد–الخميس · 9:00–17:00",
                    DistanceKm = 6.1 
                },
                new Branch 
                { 
                    Id = 4, 
                    NameEn = "Midtown Plaza", NameAr = "ميدتاون بلازا",
                    CityEn = "New York, NY", CityAr = "نيويورك، نيويورك",
                    AddressEn = "1290 Avenue of the Americas", AddressAr = "1290 جادة الأمريكتين",
                    HoursEn = "Sun–Thu · 9:00–18:00", HoursAr = "الأحد–الخميس · 9:00–18:00",
                    DistanceKm = 2.8 
                },
                new Branch 
                { 
                    Id = 5, 
                    NameEn = "Harbor Pointe", NameAr = "هاربور بوينت",
                    CityEn = "Jersey City, NJ", CityAr = "جيرسي سيتي، نيوجيرسي",
                    AddressEn = "10 Exchange Pl", AddressAr = "10 إكسشينج بليس",
                    HoursEn = "Sun–Thu · 9:00–17:00", HoursAr = "الأحد–الخميس · 9:00–17:00",
                    DistanceKm = 5.5 
                },
                new Branch 
                { 
                    Id = 6, 
                    NameEn = "Queens Center", NameAr = "كوينز سنتر",
                    CityEn = "Queens, NY", CityAr = "كوينز، نيويورك",
                    AddressEn = "90-15 Queens Blvd", AddressAr = "90-15 كوينز بوليفارد",
                    HoursEn = "Sun–Thu · 9:30–17:00", HoursAr = "الأحد–الخميس · 9:30–17:00",
                    DistanceKm = 11.4 
                }
            );

            // Seed Services
            modelBuilder.Entity<Service>().HasData(
                new Service { Id = "account", Icon = "wallet", Title = "Open an Account", Desc = "Personal, joint or business accounts in minutes." },
                new Service { Id = "loan", Icon = "hand-coins", Title = "Personal Loan", Desc = "Flexible financing tailored to your goals." },
                new Service { Id = "mortgage", Icon = "home", Title = "Mortgage", Desc = "Find your home with competitive rates." },
                new Service { Id = "wealth", Icon = "trending-up", Title = "Wealth Management", Desc = "Bespoke advisory for long-term growth." },
                new Service { Id = "cards", Icon = "credit-card", Title = "Premium Cards", Desc = "Black, Platinum and World Elite tiers." },
                new Service { Id = "business", Icon = "briefcase", Title = "Business Banking", Desc = "Solutions that scale with your company." }
            );
        }
    }
}
