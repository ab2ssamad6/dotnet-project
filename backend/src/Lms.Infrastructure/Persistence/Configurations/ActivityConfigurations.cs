using Lms.Domain.Entities;
using Lms.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lms.Infrastructure.Persistence.Configurations;

public class LearningActivityConfiguration : IEntityTypeConfiguration<LearningActivity>
{
    public void Configure(EntityTypeBuilder<LearningActivity> builder)
    {
        builder.ToTable("Activities");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();

        // Computed, not persisted (its value is derived from the concrete type).
        builder.Ignore(a => a.ActivityType);

        // Table-per-hierarchy: one table for Lesson/Exercise/Quiz/Exam, distinguished by ActivityKind.
        builder.HasDiscriminator<ActivityType>("ActivityKind")
            .HasValue<Lesson>(ActivityType.Lesson)
            .HasValue<Exercise>(ActivityType.Exercise)
            .HasValue<Quiz>(ActivityType.Quiz)
            .HasValue<Exam>(ActivityType.Exam);

        builder.HasIndex(a => new { a.ModuleId, a.Order });
    }
}

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.Property(l => l.Content).HasColumnType("TEXT");
        builder.Property(l => l.VideoUrl).HasMaxLength(500);
    }
}

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.Property(e => e.Instructions).HasColumnType("TEXT");
        builder.Property(e => e.ExpectedOutcome).HasColumnType("TEXT");
    }
}

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasMany(a => a.Questions)
            .WithOne(q => q.Assessment!)
            .HasForeignKey(q => q.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Text).HasMaxLength(1000).IsRequired();
        builder.Property(q => q.Type).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question!)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("Answers");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Text).HasMaxLength(500).IsRequired();
    }
}
