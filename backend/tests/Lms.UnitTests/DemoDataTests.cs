using FluentAssertions;
using Lms.Domain.Entities;
using Lms.Domain.Enums;
using Lms.Infrastructure.Persistence.Seed.DemoData;

namespace Lms.Unittests;

public class DemoDataTests
{
    public static TheoryData<string> CourseTitles()
    {
        var data = new TheoryData<string>();
        foreach (var course in DemoCatalog.Courses)
            data.Add(course.Title);
        return data;
    }

    private static Training Build(string title)
    {
        var course = DemoCatalog.Courses.Single(c => c.Title == title);
        var category = new Category { Name = "Demo category" };
        var trainer = new Trainer
        {
            FirstName = "Demo",
            LastName = "Trainer",
            Email = "demo.trainer@lms.local"
        };
        return course.Create(category, trainer);
    }

    [Fact]
    public void Catalog_defines_the_four_demo_courses()
    {
        DemoCatalog.Courses.Select(c => c.Title).Should().BeEquivalentTo(
            "Prompt Engineering for Production LLM Apps",
            "Modern React: Interfaces That Scale",
            "Product Discovery and UX Research",
            "Application Security for Developers");
    }

    [Theory]
    [MemberData(nameof(CourseTitles))]
    public void Course_is_published_and_within_column_limits(string title)
    {
        var training = Build(title);

        training.Title.Should().Be(title);
        training.Title.Length.Should().BeLessThanOrEqualTo(200);
        training.Description.Should().NotBeNullOrWhiteSpace();
        training.Description.Length.Should().BeLessThanOrEqualTo(4000);
        training.Published.Should().BeTrue();
        training.Status.Should().Be(TrainingStatus.Published);
        training.Duration.Should().Be(training.Modules.Sum(m => m.Duration));
        training.Duration.Should().BeGreaterThan(0);
        training.Category.Should().NotBeNull();
        training.Trainer.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(CourseTitles))]
    public void Modules_are_numbered_contiguously_and_described(string title)
    {
        var modules = Build(title).Modules.ToList();

        modules.Should().HaveCount(6, "each demo course has five content modules plus a final assessment");
        modules.Select(m => m.Order).Should().Equal(Enumerable.Range(1, modules.Count));

        foreach (var module in modules)
        {
            module.Title.Length.Should().BeLessThanOrEqualTo(200);
            module.Description.Should().NotBeNullOrWhiteSpace(
                "the AI trainer prompt silently drops empty module descriptions");
            module.Description!.Length.Should().BeLessThanOrEqualTo(2000);
            module.Duration.Should().BeGreaterThan(0);
            module.Activities.Should().NotBeEmpty();
            module.Activities.Select(a => a.Order).Should().Equal(
                Enumerable.Range(1, module.Activities.Count));

            foreach (var activity in module.Activities)
                activity.Title.Length.Should().BeLessThanOrEqualTo(200);
        }

        modules.Take(2).Should().OnlyContain(
            m => m.AiAvatarEnabled, "the first modules are the ones demoed with the avatar");
    }

    [Theory]
    [MemberData(nameof(CourseTitles))]
    public void Lessons_and_exercises_carry_real_content(string title)
    {
        var activities = Build(title).Modules.SelectMany(m => m.Activities).ToList();

        var lessons = activities.OfType<Lesson>().ToList();
        lessons.Should().HaveCount(10);
        foreach (var lesson in lessons)
        {
            lesson.Content.Should().NotBeNullOrWhiteSpace();
            lesson.Content!.Length.Should().BeGreaterThan(200, "lesson prose should be substantial");
        }

        var exercises = activities.OfType<Exercise>().ToList();
        exercises.Should().HaveCount(5);
        foreach (var exercise in exercises)
        {
            exercise.Instructions.Should().NotBeNullOrWhiteSpace();
            exercise.ExpectedOutcome.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Theory]
    [MemberData(nameof(CourseTitles))]
    public void Assessments_have_a_coherent_answer_key(string title)
    {
        var activities = Build(title).Modules.SelectMany(m => m.Activities).ToList();

        var quizzes = activities.OfType<Quiz>().ToList();
        quizzes.Should().HaveCount(5);
        quizzes.Should().OnlyContain(q => q.Questions.Count >= 4 && q.Questions.Count <= 5);

        var exams = activities.OfType<Exam>().ToList();
        exams.Should().HaveCount(1);
        exams[0].Questions.Count.Should().BeGreaterThanOrEqualTo(8);
        exams[0].DurationMinutes.Should().NotBeNull();
        exams[0].PassingScore.Should().Be(70);

        foreach (var question in activities.OfType<Assessment>().SelectMany(a => a.Questions))
        {
            question.Text.Should().NotBeNullOrWhiteSpace();
            question.Text.Length.Should().BeLessThanOrEqualTo(1000);
            question.Points.Should().BeGreaterThan(0);
            question.Answers.Count.Should().BeGreaterThanOrEqualTo(2);
            question.Answers.Should().OnlyContain(a => a.Text.Length > 0 && a.Text.Length <= 500);

            var correct = question.Answers.Count(a => a.IsCorrect);
            switch (question.Type)
            {
                case QuestionType.TrueFalse:
                    question.Answers.Should().HaveCount(2);
                    question.Answers.Select(a => a.Text).Should().BeEquivalentTo("True", "False");
                    correct.Should().Be(1, "'{0}' must have exactly one correct answer", question.Text);
                    break;
                case QuestionType.MultipleChoice:
                    correct.Should().Be(1, "'{0}' must have exactly one correct answer", question.Text);
                    break;
                case QuestionType.MultipleAnswers:
                    correct.Should().BeGreaterThanOrEqualTo(2, "'{0}' is a multi-answer question", question.Text);
                    correct.Should().BeLessThan(
                        question.Answers.Count, "'{0}' needs at least one incorrect option", question.Text);
                    break;
            }
        }
    }

    [Fact]
    public void Enrollment_plan_references_known_courses_and_students()
    {
        var titles = DemoCatalog.Courses.Select(c => c.Title).ToHashSet();
        var students = DemoCatalog.Students.Select(s => s.Email).ToHashSet();
        students.Add("student@lms.local");

        var plan = DemoCatalog.EnrollmentsFor("student@lms.local");

        plan.Should().OnlyContain(e => titles.Contains(e.CourseTitle));
        plan.Should().OnlyContain(e => students.Contains(e.StudentEmail));
        plan.Should().OnlyContain(e => e.CompletedModules >= 0 && e.CompletedModules <= 6);
        plan.Select(e => (e.StudentEmail, e.CourseTitle)).Should().OnlyHaveUniqueItems(
            "Enrollment has a unique index on (StudentId, TrainingId)");
    }

    [Fact]
    public void Demo_accounts_and_categories_use_distinct_keys()
    {
        DemoCatalog.Trainers.Select(t => t.Person.Email)
            .Concat(DemoCatalog.Students.Select(s => s.Email))
            .Should().OnlyHaveUniqueItems("Trainer.Email and Identity emails are unique");

        DemoCatalog.Categories.Select(c => c.Name).Should().OnlyHaveUniqueItems(
            "Category.Name has a unique index");
    }
}
