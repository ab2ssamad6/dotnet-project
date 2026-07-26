using Lms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lms.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasMany(c => c.Trainings)
            .WithOne(t => t.Category!)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.ToTable("Trainers");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LastName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Email).HasMaxLength(256).IsRequired();
        builder.Property(t => t.Biography).HasMaxLength(2000);
        builder.Property(t => t.Avatar).HasMaxLength(500);
        builder.Property(t => t.Expertise).HasMaxLength(500);
        builder.Property(t => t.Phone).HasMaxLength(40);
        builder.HasIndex(t => t.Email).IsUnique();

        builder.HasMany(t => t.Trainings)
            .WithOne(tr => tr.Trainer!)
            .HasForeignKey(tr => tr.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrainingConfiguration : IEntityTypeConfiguration<Training>
{
    public void Configure(EntityTypeBuilder<Training> builder)
    {
        builder.ToTable("Trainings");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(4000).IsRequired();
        builder.Property(t => t.Thumbnail).HasMaxLength(500);
        builder.Property(t => t.Difficulty).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(t => t.Title);
        builder.HasIndex(t => t.Published);

        builder.HasMany(t => t.Modules)
            .WithOne(m => m.Training!)
            .HasForeignKey(m => m.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Enrollments)
            .WithOne(e => e.Training!)
            .HasForeignKey(e => e.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Modules");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.VideoUrl).HasMaxLength(500);
        builder.Property(m => m.Attachment).HasMaxLength(500);
        builder.HasIndex(m => new { m.TrainingId, m.Order });

        builder.HasMany(m => m.Activities)
            .WithOne(a => a.Module!)
            .HasForeignKey(a => a.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
