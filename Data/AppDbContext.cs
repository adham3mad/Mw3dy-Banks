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
                    Email = "adham@mw3dy.com",
                    Phone = "01012345678",
                    City = "New Cairo",
                    Address = "53 شارع الثورة، مصر الجديدة"
                }
            );

            // Seed Branches with int IDs
            modelBuilder.Entity<Branch>().HasData(
                new Branch 
                { 
                    Id = 1, 
                    NameEn = "National Bank of Egypt - Fifth Settlement", NameAr = "البنك الأهلي المصري - التجمع الخامس",
                    CityEn = "New Cairo", CityAr = "القاهرة الجديدة",
                    AddressEn = "90th Street, Beside Americana Plaza", AddressAr = "شارع التسعين، بجوار أمريكانا بلازا",
                    HoursEn = "Sun–Thu · 8:30–15:00", HoursAr = "الأحد–الخميس · 8:30–15:00",
                    DistanceKm = 1.2 
                },
                new Branch 
                { 
                    Id = 2, 
                    NameEn = "Banque Misr - Heliopolis", NameAr = "بنك مصر - مصر الجديدة",
                    CityEn = "Cairo", CityAr = "القاهرة",
                    AddressEn = "24 El-Ahram Street, Roxy", AddressAr = "24 شارع الأهرام، روكسي",
                    HoursEn = "Sun–Thu · 8:30–15:00", HoursAr = "الأحد–الخميس · 8:30–15:00",
                    DistanceKm = 3.4 
                },
                new Branch 
                { 
                    Id = 3, 
                    NameEn = "QNB Alahli - Maadi", NameAr = "بنك قطر الوطني الأهلي - المعادي",
                    CityEn = "Cairo", CityAr = "القاهرة",
                    AddressEn = "77 Road 9, Near Maadi Metro Station", AddressAr = "77 طريق 9، بالقرب من محطة مترو المعادي",
                    HoursEn = "Sun–Thu · 8:30–15:00", HoursAr = "الأحد–الخميس · 8:30–15:00",
                    DistanceKm = 6.1 
                },
                new Branch 
                { 
                    Id = 4, 
                    NameEn = "Bank of Alexandria - Dokki", NameAr = "بنك الإسكندرية - الدقي",
                    CityEn = "Giza", CityAr = "الجيزة",
                    AddressEn = "120 Tahrir Street", AddressAr = "120 شارع التحرير",
                    HoursEn = "Sun–Thu · 8:30–15:00", HoursAr = "الأحد–الخميس · 8:30–15:00",
                    DistanceKm = 2.8 
                },
                new Branch 
                { 
                    Id = 5, 
                    NameEn = "Nasser Social Bank - Sheikh Zayed", NameAr = "بنك ناصر الاجتماعي - الشيخ زايد",
                    CityEn = "Giza", CityAr = "الجيزة",
                    AddressEn = "Sheikh Zayed, Inside Capital Business Park", AddressAr = "الشيخ زايد، داخل كابيتال بيزنس بارك",
                    HoursEn = "Sun–Thu · 8:30–15:00", HoursAr = "الأحد–الخميس · 8:30–15:00",
                    DistanceKm = 5.5 
                },
                new Branch 
                { 
                    Id = 6, 
                    NameEn = "Commercial International Bank (CIB) - Smouha", NameAr = "البنك التجاري الدولي - سموحة",
                    CityEn = "Alexandria", CityAr = "الإسكندرية",
                    AddressEn = "Victor Emmanuel Square, Smouha", AddressAr = "ميدان فيكتور عمانويل، سموحة",
                    HoursEn = "Sun–Thu · 8:30–15:00", HoursAr = "الأحد–الخميس · 8:30–15:00",
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
