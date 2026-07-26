using Lms.Domain.Enums;

namespace Lms.Application.Dtos.Trainings;

public record TrainingDto(
    Guid Id,
    string Title,
    string Description,
    DifficultyLevel Difficulty,
    int Duration,
    string? Thumbnail,
    TrainingStatus Status,
    bool Published,
    Guid CategoryId,
    string? CategoryName,
    Guid TrainerId,
    string? TrainerName,
    int ModuleCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateTrainingRequest(
    string Title,
    string Description,
    DifficultyLevel Difficulty,
    int Duration,
    string? Thumbnail,
    TrainingStatus Status,
    bool Published,
    Guid CategoryId,
    Guid TrainerId);

public record UpdateTrainingRequest(
    string Title,
    string Description,
    DifficultyLevel Difficulty,
    int Duration,
    string? Thumbnail,
    TrainingStatus Status,
    bool Published,
    Guid CategoryId,
    Guid TrainerId);
