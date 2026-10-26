using LuxeSmilesDental.Models;
using Microsoft.EntityFrameworkCore;
namespace LuxeSmilesDental.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<DentalChart> DentalCharts { get; set; }
        public DbSet<TreatmentRecord> TreatmentRecords { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<PatientRecall> PatientRecalls { get; set; }
        public DbSet<InventoryItem> Inventory { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Appointment to Dentist relationship
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Dentist)
                .WithMany()
                .HasForeignKey(a => a.DentistId)
                .OnDelete(DeleteBehavior.Restrict);
            // Appointment to Patient relationship
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.HasKey(e => e.PatientId);
                entity.Property(e => e.PatientId).HasColumnName("PatientId");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            // DentalChart configuration
            modelBuilder.Entity<DentalChart>(entity =>
            {
                entity.ToTable("DentalCharts");
                entity.HasKey(e => e.ChartId);
                entity.Property(e => e.ChartId).HasColumnName("ChartId");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.DentalCharts)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TreatmentRecord configuration
            modelBuilder.Entity<TreatmentRecord>(entity =>
            {
                entity.ToTable("TreatmentRecords");
                entity.HasKey(e => e.TreatmentRecordId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(t => t.DentalChart)
                    .WithMany(d => d.TreatmentRecords)
                    .HasForeignKey(t => t.ChartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PatientRecall configuration
            modelBuilder.Entity<PatientRecall>(entity =>
            {
                entity.ToTable("PatientRecalls");
                entity.HasKey(e => e.RecallId);
                entity.Property(e => e.RecallId).HasColumnName("RecallId");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(p => p.Patient)
                    .WithMany(p => p.PatientRecalls)
                    .HasForeignKey(p => p.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}