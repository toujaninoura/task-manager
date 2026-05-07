using AutoMapper;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Mappings;

public class SharingProfile : Profile
{
    public SharingProfile()
    {
        CreateMap<TaskCollaborator, CollaboratorResponse>()
            .ConstructUsing(src => new CollaboratorResponse(
                src.Id,
                src.UserId,
                src.User.Email,
                src.Role,
                src.Status,
                src.InvitedAt));

        CreateMap<TaskCollaborator, InvitationResponse>()
            .ConstructUsing(src => new InvitationResponse(
                src.Id,
                src.TaskId,
                src.Task.Title,
                src.InvitedByUser.Email,
                src.Role,
                src.InvitedAt));
    }
}
