using Lms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lms.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        // A student can enrol in a given training only once.
        builder.HasIndex(e => new { e.StudentId, e.TrainingId }).IsUnique();

        builder.HasMany(e => e.ModuleCompletions)
            .WithOne(m => m.Enrollment!)
            .HasForeignKey(m => m.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.QuizAttempts)
            .WithOne(q => q.Enrollment!)
            .HasForeignKey(q => q.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ModuleCompletionConfiguration : IEntityTypeConfiguration<ModuleCompletion>
{
    public void Configure(EntityTypeBuilder<ModuleCompletion> builder)
    {
        builder.ToTable("ModuleCompletions");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.EnrollmentId, m.ModuleId }).IsUnique();

        // No FK navigation from Module side; reference by id only to avoid a cascade cycle.
        builder.HasOne(m => m.Module)
            .WithMany()
            .HasForeignKey(m => m.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("QuizAttempts");
        builder.HasKey(q => q.Id);
        builder.HasIndex(q => new { q.EnrollmentId, q.ActivityId });
    }
}
