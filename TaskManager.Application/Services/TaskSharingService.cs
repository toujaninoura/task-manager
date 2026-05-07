using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using ValidationException = TaskManager.Domain.Exceptions.ValidationException;

namespace TaskManager.Application.Services;

public class TaskSharingService : ITaskSharingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<InviteCollaboratorRequest> _inviteValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskSharingService> _logger;

    public TaskSharingService(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IValidator<InviteCollaboratorRequest> inviteValidator,
        IMapper mapper,
        ILogger<TaskSharingService> logger)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _inviteValidator = inviteValidator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CollaboratorResponse> InviteCollaboratorAsync(
        int taskId, InviteCollaboratorRequest request, int ownerUserId, CancellationToken ct = default)
    {
        // Fail-fast: validate before any database calls
        var validation = await _inviteValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var task = await _unitOfWork.Tasks.GetByIdAndUserIdAsync(taskId, ownerUserId, ct)
            ?? throw new NotFoundException(nameof(TaskItem), taskId);

        var invitedUser = await _userRepository.GetByEmailAsync(request.Email)
            ?? throw new NotFoundException("User", request.Email);

        if (invitedUser.Id == ownerUserId)
            throw new BusinessException("Cannot invite yourself as a collaborator.");

        var existing = await _unitOfWork.Collaborators.GetByTaskAndUserAsync(taskId, invitedUser.Id, ct);
        if (existing is not null)
            throw new ConflictException("User is already a collaborator on this task.");

        var collaborator = new TaskCollaborator
        {
            TaskId = taskId,
            UserId = invitedUser.Id,
            InvitedByUserId = ownerUserId,
            Role = request.Role,
            Status = InvitationStatus.Pending,
            InvitedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.Collaborators.CreateAsync(collaborator, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("User {InvitedUserId} invited to task {TaskId} by owner {OwnerId}", invitedUser.Id, taskId, ownerUserId);

        created.User = invitedUser;
        return _mapper.Map<CollaboratorResponse>(created);
    }

    public async Task AcceptInvitationAsync(int taskId, int userId, CancellationToken ct = default)
    {
        var collab = await _unitOfWork.Collaborators.GetByTaskAndUserTrackingAsync(taskId, userId, ct)
            ?? throw new NotFoundException("Invitation", taskId);

        if (collab.Status != InvitationStatus.Pending)
            throw new BusinessException("Invitation has already been processed.");

        collab.Status = InvitationStatus.Accepted;
        collab.RespondedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} accepted invitation for task {TaskId}", userId, taskId);
    }

    public async Task RejectInvitationAsync(int taskId, int userId, CancellationToken ct = default)
    {
        var collab = await _unitOfWork.Collaborators.GetByTaskAndUserTrackingAsync(taskId, userId, ct)
            ?? throw new NotFoundException("Invitation", taskId);

        if (collab.Status != InvitationStatus.Pending)
            throw new BusinessException("Invitation has already been processed.");

        collab.Status = InvitationStatus.Rejected;
        collab.RespondedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} rejected invitation for task {TaskId}", userId, taskId);
    }

    public async Task RemoveCollaboratorAsync(int taskId, int targetUserId, int requestingUserId, CancellationToken ct = default)
    {
        var collab = await _unitOfWork.Collaborators.GetByTaskAndUserAsync(taskId, targetUserId, ct)
            ?? throw new NotFoundException("Collaborator", targetUserId);

        var isOwner = await _unitOfWork.Tasks.GetByIdAndUserIdAsync(taskId, requestingUserId, ct) is not null;
        var isSelf = requestingUserId == targetUserId;

        if (!isOwner && !isSelf)
            throw new UnauthorizedException("Only the task owner or the collaborator themselves can remove a collaborator.");

        await _unitOfWork.Collaborators.DeleteAsync(collab, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Collaborator {TargetUserId} removed from task {TaskId} by {RequestingUserId}", targetUserId, taskId, requestingUserId);
    }

    public async Task<IEnumerable<CollaboratorResponse>> GetCollaboratorsAsync(
        int taskId, int requestingUserId, CancellationToken ct = default)
    {
        var isOwner = await _unitOfWork.Tasks.GetByIdAndUserIdAsync(taskId, requestingUserId, ct) is not null;
        if (!isOwner)
        {
            var isCollaborator = await _unitOfWork.Collaborators.IsAcceptedCollaboratorAsync(taskId, requestingUserId, ct);
            if (!isCollaborator)
                throw new UnauthorizedException("Access denied.");
        }

        var collabs = await _unitOfWork.Collaborators.GetByTaskIdAsync(taskId, ct);
        return _mapper.Map<IEnumerable<CollaboratorResponse>>(collabs);
    }

    public async Task<IEnumerable<InvitationResponse>> GetSharedTasksAsync(int userId, CancellationToken ct = default)
    {
        var collabs = await _unitOfWork.Collaborators.GetSharedWithUserAsync(userId, ct);
        return _mapper.Map<IEnumerable<InvitationResponse>>(collabs);
    }

    public async Task<IEnumerable<InvitationResponse>> GetPendingInvitationsAsync(int userId, CancellationToken ct = default)
    {
        var collabs = await _unitOfWork.Collaborators.GetPendingForUserAsync(userId, ct);
        return _mapper.Map<IEnumerable<InvitationResponse>>(collabs);
    }
}
