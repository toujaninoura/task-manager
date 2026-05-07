using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Tests.Unit;

[TestFixture]
public class TaskSharingServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<ITaskRepository> _taskRepositoryMock;
    private Mock<ITaskCollaboratorRepository> _collaboratorRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IValidator<InviteCollaboratorRequest>> _validatorMock;
    private Mock<ILogger<TaskSharingService>> _loggerMock;
    private IMapper _mapper;
    private TaskSharingService _sut;

    private const int OwnerId = 1;
    private const int GuestId = 2;
    private const int TaskId = 10;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _collaboratorRepositoryMock = new Mock<ITaskCollaboratorRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _validatorMock = new Mock<IValidator<InviteCollaboratorRequest>>();
        _loggerMock = new Mock<ILogger<TaskSharingService>>();

        _unitOfWorkMock.Setup(u => u.Tasks).Returns(_taskRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Collaborators).Returns(_collaboratorRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Default: validation succeeds
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<InviteCollaboratorRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<SharingProfile>();
            cfg.AddProfile<TaskItemProfile>();
        });
        _mapper = config.CreateMapper();

        _sut = new TaskSharingService(
            _unitOfWorkMock.Object,
            _userRepositoryMock.Object,
            _validatorMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    private static TaskItem BuildTask(int id = TaskId, int userId = OwnerId) => new()
    {
        Id = id,
        Title = "Test Task",
        UserId = userId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static User BuildUser(int id, string email) => new()
    {
        Id = id,
        Email = email,
        PasswordHash = "hash",
        CreatedAt = DateTime.UtcNow
    };

    private static TaskCollaborator BuildCollaborator(
        int id = 1,
        int taskId = TaskId,
        int userId = GuestId,
        int invitedByUserId = OwnerId,
        TaskShareRole role = TaskShareRole.Viewer,
        InvitationStatus status = InvitationStatus.Pending) => new()
    {
        Id = id,
        TaskId = taskId,
        UserId = userId,
        InvitedByUserId = invitedByUserId,
        Role = role,
        Status = status,
        InvitedAt = DateTime.UtcNow,
        User = BuildUser(userId, "guest@test.com"),
        InvitedByUser = BuildUser(invitedByUserId, "owner@test.com"),
        Task = BuildTask(taskId)
    };

    // ================== InviteCollaboratorAsync ==================

    [Test]
    public async Task InviteCollaboratorAsync_WhenOwnerInvitesExistingUser_ShouldReturnCollaboratorResponse()
    {
        var request = new InviteCollaboratorRequest("guest@test.com", TaskShareRole.Viewer);
        var ownerTask = BuildTask();
        var guestUser = BuildUser(GuestId, "guest@test.com");
        var created = BuildCollaborator();

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerTask);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("guest@test.com"))
            .ReturnsAsync(guestUser);
        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskCollaborator?)null);
        _collaboratorRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TaskCollaborator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _sut.InviteCollaboratorAsync(TaskId, request, OwnerId);

        result.Should().NotBeNull();
        result.UserId.Should().Be(GuestId);
        result.Email.Should().Be("guest@test.com");
        _collaboratorRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<TaskCollaborator>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task InviteCollaboratorAsync_WhenNonOwnerInvites_ShouldThrowNotFoundException()
    {
        var request = new InviteCollaboratorRequest("guest@test.com", TaskShareRole.Viewer);

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, 999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.InviteCollaboratorAsync(TaskId, request, 999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task InviteCollaboratorAsync_WhenEmailNotFound_ShouldThrowNotFoundException()
    {
        var request = new InviteCollaboratorRequest("unknown@test.com", TaskShareRole.Viewer);
        var ownerTask = BuildTask();

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerTask);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("unknown@test.com"))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () => await _sut.InviteCollaboratorAsync(TaskId, request, OwnerId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task InviteCollaboratorAsync_WhenAlreadyCollaborator_ShouldThrowConflictException()
    {
        var request = new InviteCollaboratorRequest("guest@test.com", TaskShareRole.Viewer);
        var ownerTask = BuildTask();
        var guestUser = BuildUser(GuestId, "guest@test.com");
        var existing = BuildCollaborator();

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerTask);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("guest@test.com"))
            .ReturnsAsync(guestUser);
        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        Func<Task> act = async () => await _sut.InviteCollaboratorAsync(TaskId, request, OwnerId);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Test]
    public async Task InviteCollaboratorAsync_WhenInvitingSelf_ShouldThrowBusinessException()
    {
        var request = new InviteCollaboratorRequest("owner@test.com", TaskShareRole.Viewer);
        var ownerTask = BuildTask();
        var ownerUser = BuildUser(OwnerId, "owner@test.com");

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerTask);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("owner@test.com"))
            .ReturnsAsync(ownerUser);

        Func<Task> act = async () => await _sut.InviteCollaboratorAsync(TaskId, request, OwnerId);

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Test]
    public async Task InviteCollaboratorAsync_WhenRoleIsOwner_ShouldThrowValidationException()
    {
        var request = new InviteCollaboratorRequest("guest@test.com", TaskShareRole.Owner);
        var failures = new List<ValidationFailure>
        {
            new("Role", "Role must be Editor or Viewer. Owner cannot be assigned.")
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        Func<Task> act = async () => await _sut.InviteCollaboratorAsync(TaskId, request, OwnerId);

        await act.Should().ThrowAsync<TaskManager.Domain.Exceptions.ValidationException>();
    }

    // ================== AcceptInvitationAsync ==================

    [Test]
    public async Task AcceptInvitationAsync_WhenValidInvitation_ShouldSetStatusAccepted()
    {
        var collab = BuildCollaborator(status: InvitationStatus.Pending);

        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserTrackingAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collab);

        await _sut.AcceptInvitationAsync(TaskId, GuestId);

        collab.Status.Should().Be(InvitationStatus.Accepted);
        collab.RespondedAt.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AcceptInvitationAsync_WhenInvitationNotFound_ShouldThrowNotFoundException()
    {
        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserTrackingAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskCollaborator?)null);

        Func<Task> act = async () => await _sut.AcceptInvitationAsync(TaskId, GuestId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task AcceptInvitationAsync_WhenWrongUser_ShouldThrowNotFoundException()
    {
        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserTrackingAsync(TaskId, 999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskCollaborator?)null);

        Func<Task> act = async () => await _sut.AcceptInvitationAsync(TaskId, 999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task AcceptInvitationAsync_WhenAlreadyAccepted_ShouldThrowBusinessException()
    {
        var collab = BuildCollaborator(status: InvitationStatus.Accepted);

        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserTrackingAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collab);

        Func<Task> act = async () => await _sut.AcceptInvitationAsync(TaskId, GuestId);

        await act.Should().ThrowAsync<BusinessException>();
    }

    // ================== RejectInvitationAsync ==================

    [Test]
    public async Task RejectInvitationAsync_WhenValidInvitation_ShouldSetStatusRejected()
    {
        var collab = BuildCollaborator(status: InvitationStatus.Pending);

        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserTrackingAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collab);

        await _sut.RejectInvitationAsync(TaskId, GuestId);

        collab.Status.Should().Be(InvitationStatus.Rejected);
        collab.RespondedAt.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RejectInvitationAsync_WhenInvitationNotFound_ShouldThrowNotFoundException()
    {
        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserTrackingAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskCollaborator?)null);

        Func<Task> act = async () => await _sut.RejectInvitationAsync(TaskId, GuestId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task RejectInvitationAsync_WhenWrongUser_ShouldThrowNotFoundException()
    {
        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserTrackingAsync(TaskId, 999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskCollaborator?)null);

        Func<Task> act = async () => await _sut.RejectInvitationAsync(TaskId, 999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ================== RemoveCollaboratorAsync ==================

    [Test]
    public async Task RemoveCollaboratorAsync_WhenOwnerRemoves_ShouldCallDelete()
    {
        var collab = BuildCollaborator(userId: GuestId, invitedByUserId: OwnerId);
        var ownerTask = BuildTask(userId: OwnerId);

        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collab);
        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerTask);
        _collaboratorRepositoryMock.Setup(r => r.DeleteAsync(collab, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.RemoveCollaboratorAsync(TaskId, GuestId, OwnerId);

        _collaboratorRepositoryMock.Verify(r => r.DeleteAsync(collab, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RemoveCollaboratorAsync_WhenCollaboratorRemovesSelf_ShouldCallDelete()
    {
        var collab = BuildCollaborator(userId: GuestId, invitedByUserId: OwnerId);

        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collab);
        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        _collaboratorRepositoryMock.Setup(r => r.DeleteAsync(collab, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.RemoveCollaboratorAsync(TaskId, GuestId, GuestId);

        _collaboratorRepositoryMock.Verify(r => r.DeleteAsync(collab, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RemoveCollaboratorAsync_WhenUnauthorizedUser_ShouldThrowUnauthorizedException()
    {
        var collab = BuildCollaborator(userId: GuestId, invitedByUserId: OwnerId);
        const int intruder = 999;

        _collaboratorRepositoryMock.Setup(r => r.GetByTaskAndUserAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collab);
        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, intruder, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.RemoveCollaboratorAsync(TaskId, GuestId, intruder);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    // ================== GetCollaboratorsAsync ==================

    [Test]
    public async Task GetCollaboratorsAsync_WhenOwner_ShouldReturnList()
    {
        var ownerTask = BuildTask(userId: OwnerId);
        var collabs = new List<TaskCollaborator> { BuildCollaborator() };

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerTask);
        _collaboratorRepositoryMock.Setup(r => r.GetByTaskIdAsync(TaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collabs);

        var result = await _sut.GetCollaboratorsAsync(TaskId, OwnerId);

        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(GuestId);
    }

    [Test]
    public async Task GetCollaboratorsAsync_WhenAcceptedCollaborator_ShouldReturnList()
    {
        var collabs = new List<TaskCollaborator> { BuildCollaborator(status: InvitationStatus.Accepted) };

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        _collaboratorRepositoryMock.Setup(r => r.IsAcceptedCollaboratorAsync(TaskId, GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _collaboratorRepositoryMock.Setup(r => r.GetByTaskIdAsync(TaskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collabs);

        var result = await _sut.GetCollaboratorsAsync(TaskId, GuestId);

        result.Should().HaveCount(1);
    }

    [Test]
    public async Task GetCollaboratorsAsync_WhenNonCollaborator_ShouldThrowUnauthorizedException()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(TaskId, 999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        _collaboratorRepositoryMock.Setup(r => r.IsAcceptedCollaboratorAsync(TaskId, 999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Func<Task> act = async () => await _sut.GetCollaboratorsAsync(TaskId, 999);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    // ================== GetSharedTasksAsync ==================

    [Test]
    public async Task GetSharedTasksAsync_ShouldReturnAcceptedSharedTasks()
    {
        var collabs = new List<TaskCollaborator>
        {
            BuildCollaborator(status: InvitationStatus.Accepted)
        };

        _collaboratorRepositoryMock.Setup(r => r.GetSharedWithUserAsync(GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collabs);

        var result = await _sut.GetSharedTasksAsync(GuestId);

        result.Should().HaveCount(1);
        result.First().TaskId.Should().Be(TaskId);
    }

    // ================== GetPendingInvitationsAsync ==================

    [Test]
    public async Task GetPendingInvitationsAsync_ShouldReturnPendingInvitations()
    {
        var collabs = new List<TaskCollaborator>
        {
            BuildCollaborator(status: InvitationStatus.Pending)
        };

        _collaboratorRepositoryMock.Setup(r => r.GetPendingForUserAsync(GuestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(collabs);

        var result = await _sut.GetPendingInvitationsAsync(GuestId);

        result.Should().HaveCount(1);
        result.First().TaskId.Should().Be(TaskId);
    }
}
