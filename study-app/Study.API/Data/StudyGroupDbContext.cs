using Microsoft.EntityFrameworkCore;
using Study.API.Data.Models;

public class StudyGroupDbContext : DbContext
    {
        public StudyGroupDbContext(DbContextOptions<StudyGroupDbContext> options)
            : base(options)
        {
        }

        public DbSet<StudyGroup> StudyGroups { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship between StudyGroup and User
            modelBuilder.Entity<StudyGroup>()
                .HasMany(sg => sg.Users)
                .WithMany(u => u.StudyGroups);

            // Configure StudyGroup entity
            modelBuilder.Entity<StudyGroup>()
                .HasKey(sg => sg.StudyGroupId);

            modelBuilder.Entity<StudyGroup>()
                .Property(sg => sg.Name)
                .IsRequired()
                .HasMaxLength(30);

            modelBuilder.Entity<StudyGroup>()
                .Property(sg => sg.Subject)
                .IsRequired();

            // Unique constraint: Only one study group per subject
            modelBuilder.Entity<StudyGroup>()
                .HasIndex(sg => sg.Subject)
                .IsUnique();

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
