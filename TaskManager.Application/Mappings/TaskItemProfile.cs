using AutoMapper;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Mappings;

public class TaskItemProfile : Profile
{
    public TaskItemProfile()
    {
        CreateMap<TaskItem, TaskItemResponse>()
            .ConstructUsing(src => new TaskItemResponse(
                src.Id,
                src.Title,
                src.Description,
                src.Status,
                src.Status.ToString(),
                src.Priority,
                src.Priority.ToString(),
                src.DueDate,
                src.CreatedAt,
                src.UpdatedAt
            ));

        CreateMap<CreateTaskItemRequest, TaskItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<UpdateTaskItemRequest, TaskItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
