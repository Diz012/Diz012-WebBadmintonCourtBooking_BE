using CourtBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CourtBookingAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Facility> Facilities { get; set; } = null!;
        public DbSet<Court> Courts { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<TimeSlot> TimeSlots { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Facility - Owner (User) relationship
            modelBuilder.Entity<Facility>()
                .HasOne(f => f.Owner)
                .WithMany(u => u.OwnedFacilities)
                .HasForeignKey(f => f.OwnerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Court - Facility relationship
            modelBuilder.Entity<Court>()
                .HasOne(c => c.Facility)
                .WithMany(f => f.Courts)
                .HasForeignKey(c => c.FacilityID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Booking - User relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Booking - Court relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Court)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CourtID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TimeSlot - Court relationship
            modelBuilder.Entity<TimeSlot>()
                .HasOne(t => t.Court)
                .WithMany(c => c.TimeSlots)
                .HasForeignKey(t => t.CourtID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Booking)
                .WithOne(b => b.Invoice)
                .HasForeignKey<Invoice>(i => i.BookingID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TimeSlot - Booking relationship
            modelBuilder.Entity<TimeSlot>()
                .HasOne(t => t.Booking)
                .WithMany(b => b.TimeSlots)
                .HasForeignKey(t => t.BookingID)
                .OnDelete(DeleteBehavior.SetNull);

            // Additional configurations (Default values, Unique indexes)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("Booker");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Booking>()
                .Property(b => b.PaymentStatus)
                .HasDefaultValue("Unpaid");

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingStatus)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Court>()
                .Property(c => c.Status)
                .HasDefaultValue("Active");

            modelBuilder.Entity<TimeSlot>()
                .Property(t => t.IsAvailable)
                .HasDefaultValue(true);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.PaymentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
