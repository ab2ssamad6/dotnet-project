using AutoMapper;
using Lms.Application.Dtos.Categories;
using Lms.Application.Dtos.Modules;
using Lms.Application.Dtos.Trainers;
using Lms.Domain.Entities;

namespace Lms.Application.Mapping;

/// <summary>
/// AutoMapper configuration for the straightforward entity → DTO projections.
/// Aggregates that need joined names or polymorphic shaping (Training, activities)
/// are projected explicitly inside their services for clarity and query efficiency.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<Trainer, TrainerDto>();
        CreateMap<Module, ModuleDto>();
    }
}
