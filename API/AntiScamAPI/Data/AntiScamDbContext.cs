using Microsoft.EntityFrameworkCore;
using AntiScamAPI.Models;

namespace AntiScamAPI.Data;

public class AntiScamDbContext : DbContext
{
    public AntiScamDbContext(DbContextOptions<AntiScamDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<ModuleEntity> Modules { get; set; } = null!;
    public DbSet<RatingEntity> Ratings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users table
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserCluster).HasColumnName("user_cluster");
            entity.Property(e => e.DigitalLiteracy).HasColumnName("digital_literacy");
            entity.Property(e => e.AgeGroup).HasColumnName("age_group").HasMaxLength(20);
            entity.Property(e => e.RiskProfile).HasColumnName("risk_profile").HasMaxLength(20);
            entity.Property(e => e.PreferredTopic).HasColumnName("preferred_topic").HasMaxLength(50);
        });

        // Modules table
        modelBuilder.Entity<ModuleEntity>(entity =>
        {
            entity.ToTable("modules");
            entity.HasKey(e => e.ModuleId);
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.ScamType).HasColumnName("scam_type").HasMaxLength(50);
            entity.Property(e => e.Difficulty).HasColumnName("difficulty");
            entity.Property(e => e.TargetLiteracy).HasColumnName("target_literacy");
            entity.Property(e => e.DurationMin).HasColumnName("duration_min");
        });

        // Ratings table
        modelBuilder.Entity<RatingEntity>(entity =>
        {
            entity.ToTable("ratings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Composite index for fast lookups
            entity.HasIndex(e => new { e.UserId, e.ModuleId });
        });
    }
}

// Database entities (different from ML.NET input classes)
public class UserEntity
{
    public uint UserId { get; set; }
    public int UserCluster { get; set; }
    public float DigitalLiteracy { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
    public string RiskProfile { get; set; } = string.Empty;
    public string PreferredTopic { get; set; } = string.Empty;
}

public class ModuleEntity
{
    public uint ModuleId { get; set; }
    public string ScamType { get; set; } = string.Empty;
    public float Difficulty { get; set; }
    public float TargetLiteracy { get; set; }
    public float DurationMin { get; set; }
}

public class RatingEntity
{
    public int Id { get; set; }
    public uint UserId { get; set; }
    public uint ModuleId { get; set; }
    public float Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
