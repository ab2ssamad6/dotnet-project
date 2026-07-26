using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Dashboard;
using Lms.Infrastructure.Identity;
using Lms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lms.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly LmsDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(LmsDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<DashboardDto>> GetAsync(CancellationToken ct = default)
    {
        var students = (await _userManager.GetUsersInRoleAsync(Roles.Student)).Count;

        var counts = new DashboardCountsDto(
            Students: students,
            Trainers: await _context.Trainers.CountAsync(ct),
            Courses: await _context.Trainings.CountAsync(ct),
            Modules: await _context.Modules.CountAsync(ct),
            Enrollments: await _context.Enrollments.CountAsync(ct),
            PublishedCourses: await _context.Trainings.CountAsync(t => t.Published, ct));

        var byCategory = await _context.Categories.AsNoTracking()
            .OrderByDescending(c => c.Trainings.Count())
            .Take(10)
            .Select(c => new CategoryCountDto(c.Name, c.Trainings.Count()))
            .ToListAsync(ct);

        var recentEnrollments = await _context.Enrollments.AsNoTracking()
            .OrderByDescending(e => e.EnrolledAt)
            .Take(5)
            .Select(e => new RecentActivityDto("Enrollment",
                $"New enrollment in \"{e.Training!.Title}\"", e.EnrolledAt))
            .ToListAsync(ct);

        var recentTrainings = await _context.Trainings.AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new RecentActivityDto("Training", $"Training \"{t.Title}\" created", t.CreatedAt))
            .ToListAsync(ct);

        var recent = recentEnrollments.Concat(recentTrainings)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();

        return Result<DashboardDto>.Success(new DashboardDto(counts, byCategory, recent));
    }
}
