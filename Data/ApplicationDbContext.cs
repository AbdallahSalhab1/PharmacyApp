using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewWebApplicationProject.Models;
using System.Reflection.Emit;


namespace PharmacyApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public DbSet<Inventory> Inventories { get; set; }

        public DbSet<Medicine> Medicines { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Pharmacy> Pharmacies { get; set; }

        public DbSet<Purchase> Purchases { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Inventory>()
        .HasOne(i => i.Pharmacy)
        .WithMany(p => p.Inventory)
        .HasForeignKey(i => i.PharmacyId);

            // Medicine and Inventory
            builder.Entity<Inventory>()
                .HasOne(i => i.Medicine)
                .WithMany(m => m.Inventory)
                .HasForeignKey(i => i.MedicineId);
                

            // Pharmacy and Purchases
            builder.Entity<Purchase>()
                .HasOne(p => p.Pharmacy)
                .WithMany(ph => ph.Purchases)
                .HasForeignKey(p => p.PharmacyId);

            // Medicine and Purchases
            builder.Entity<Purchase>()
                .HasOne(p => p.Medicine)
                .WithMany(m => m.Purchases)
                .HasForeignKey(p => p.MedicineId);

            // Patient and Purchases
            builder.Entity<Purchase>()
                .HasOne(p => p.Patient)
                .WithMany(pt => pt.Purchases)
                .HasForeignKey(p => p.PatientId);
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
