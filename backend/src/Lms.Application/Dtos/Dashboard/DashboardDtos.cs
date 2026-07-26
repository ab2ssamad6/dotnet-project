namespace Lms.Application.Dtos.Dashboard;

public record DashboardCountsDto(
    int Students,
    int Trainers,
    int Courses,
    int Modules,
    int Enrollments,
    int PublishedCourses);

public record RecentActivityDto(string Type, string Description, DateTime Timestamp);

public record CategoryCountDto(string Category, int Trainings);

public record DashboardDto(
    DashboardCountsDto Counts,
    IReadOnlyList<CategoryCountDto> TrainingsByCategory,
    IReadOnlyList<RecentActivityDto> RecentActivity);
