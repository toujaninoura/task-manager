using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
using TaskManager.Application.Validators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Tests.Unit;

[TestFixture]
public class TaskServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<ITaskRepository> _taskRepositoryMock;
    private Mock<ILogger<TaskService>> _loggerMock;
    private Mock<IValidator<CreateTaskItemRequest>> _createValidatorMock;
    private Mock<IValidator<UpdateTaskItemRequest>> _updateValidatorMock;
    private IMapper _mapper;
    private TaskService _sut;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _loggerMock = new Mock<ILogger<TaskService>>();
        _createValidatorMock = new Mock<IValidator<CreateTaskItemRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateTaskItemRequest>>();

        _unitOfWorkMock.Setup(u => u.Tasks).Returns(_taskRepositoryMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<TaskItemProfile>());
        _mapper = config.CreateMapper();

        _sut = new TaskService(
            _unitOfWorkMock.Object,
            _mapper,
            _loggerMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    // --- GetAllAsync ---

    [Test]
    public async Task GetAllAsync_ReturnsOnlyUserTasks()
    {
        const int userId = 42;
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Task 2", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _taskRepositoryMock.Setup(r => r.GetAllByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);
        _taskRepositoryMock.Setup(r => r.CountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await _sut.GetAllAsync(userId, 1, 10);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Data.Should().HaveCount(2);
        result.Data.All(t => true).Should().BeTrue();
    }

    // --- GetByIdAsync ---

    [Test]
    public async Task GetByIdAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(99, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.GetByIdAsync(99, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GetByIdAsync_WhenWrongUser_ThrowsNotFoundException()
    {
        const int taskId = 5;
        const int wrongUserId = 999;

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(taskId, wrongUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.GetByIdAsync(taskId, wrongUserId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GetByIdAsync_WhenTaskExists_ShouldReturnTaskItemResponse()
    {
        const int userId = 1;
        var taskItem = new TaskItem
        {
            Id = 1,
            Title = "Test Task",
            UserId = userId,
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(1, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);

        var result = await _sut.GetByIdAsync(1, userId);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Test Task");
    }

    // --- CreateAsync ---

    [Test]
    public async Task CreateAsync_WithValidRequest_ReturnsResponse()
    {
        const int userId = 1;
        var request = new CreateTaskItemRequest("New Task", "Description", TaskItemStatus.Todo, TaskPriority.High, null);
        var taskItem = new TaskItem
        {
            Id = 1,
            Title = "New Task",
            UserId = userId,
            Priority = TaskPriority.High,
            Status = TaskItemStatus.Todo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _createValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _taskRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(request, userId);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        _taskRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WithEmptyTitle_ThrowsValidationException()
    {
        const int userId = 1;
        var request = new CreateTaskItemRequest("", null, TaskItemStatus.Todo, TaskPriority.Medium, null);
        var failures = new List<ValidationFailure>
        {
            new("Title", "Title is required.")
        };

        _createValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        Func<Task> act = async () => await _sut.CreateAsync(request, userId);

        await act.Should().ThrowAsync<TaskManager.Domain.Exceptions.ValidationException>();
    }

    // --- UpdateAsync ---

    [Test]
    public async Task UpdateAsync_WhenValidRequest_ShouldReturnUpdatedResponse()
    {
        const int taskId = 1;
        const int userId = 1;
        var request = new UpdateTaskItemRequest("Updated Title", "Updated Desc", TaskItemStatus.InProgress, TaskPriority.High, null);
        var tracked = new TaskItem
        {
            Id = taskId,
            Title = "Updated Title",
            Description = "Updated Desc",
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.High,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _updateValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdTrackingAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracked);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracked);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(taskId, request, userId);

        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Status.Should().Be(TaskItemStatus.InProgress);
        _taskRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        const int taskId = 99;
        const int userId = 1;
        var request = new UpdateTaskItemRequest("Title", null, TaskItemStatus.Todo, TaskPriority.Medium, null);

        _updateValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdTrackingAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.UpdateAsync(taskId, request, userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task UpdateAsync_WhenInvalidRequest_ThrowsValidationException()
    {
        const int taskId = 1;
        const int userId = 1;
        var request = new UpdateTaskItemRequest("", null, TaskItemStatus.Todo, TaskPriority.Medium, null);
        var failures = new List<ValidationFailure> { new("Title", "Title is required.") };

        _updateValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        Func<Task> act = async () => await _sut.UpdateAsync(taskId, request, userId);

        await act.Should().ThrowAsync<TaskManager.Domain.Exceptions.ValidationException>();
    }

    // --- DeleteAsync ---

    [Test]
    public async Task DeleteAsync_CallsSoftDelete()
    {
        const int taskId = 1;
        const int userId = 1;
        var taskItem = new TaskItem
        {
            Id = taskId,
            Title = "Task to delete",
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);
        _taskRepositoryMock.Setup(r => r.SoftDeleteAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.DeleteAsync(taskId, userId);

        _taskRepositoryMock.Verify(r => r.SoftDeleteAsync(taskId, userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(99, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.DeleteAsync(99, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
