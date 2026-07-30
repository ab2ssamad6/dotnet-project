using AutoMapper;
using Lms.Application.Dtos.Categories;
using Lms.Application.Dtos.Modules;
using Lms.Application.Dtos.Trainers;
using Lms.Domain.Entities;

namespace Lms.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<Trainer, TrainerDto>();
        CreateMap<Module, ModuleDto>();
    }
}
