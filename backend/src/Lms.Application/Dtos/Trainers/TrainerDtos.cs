namespace Lms.Application.Dtos.Trainers;

public record TrainerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Biography,
    string? Avatar,
    string? Expertise,
    string? Phone,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateTrainerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Biography,
    string? Avatar,
    string? Expertise,
    string? Phone);

public record UpdateTrainerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Biography,
    string? Avatar,
    string? Expertise,
    string? Phone);
