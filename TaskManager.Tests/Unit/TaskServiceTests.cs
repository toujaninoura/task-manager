using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
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
    private IMapper _mapper;
    private TaskService _sut;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _loggerMock = new Mock<ILogger<TaskService>>();

        _unitOfWorkMock.Setup(u => u.Tasks).Returns(_taskRepositoryMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<TaskItemProfile>());
        _mapper = config.CreateMapper();

        _sut = new TaskService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_WhenTaskExists_ShouldReturnTaskItemResponse()
    {
        var taskItem = new TaskItem
        {
            Id = 1,
            Title = "Test Task",
            Status = TaskItemStatus.Todo,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(taskItem);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Test Task");
    }

    [Test]
    public async Task GetByIdAsync_WhenTaskNotFound_ShouldThrowNotFoundException()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task CreateAsync_WhenValidRequest_ShouldReturnCreatedTaskItemResponse()
    {
        var request = new CreateTaskItemRequest("New Task", "Description", TaskPriority.High, null);
        var taskItem = new TaskItem { Id = 1, Title = "New Task", Priority = TaskPriority.High, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

        _taskRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TaskItem>())).ReturnsAsync(taskItem);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        _taskRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<TaskItem>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenTaskNotFound_ShouldThrowNotFoundException()
    {
        var request = new UpdateTaskItemRequest("Updated", null, TaskItemStatus.InProgress, TaskPriority.High, null);
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.UpdateAsync(99, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task DeleteAsync_WhenTaskNotFound_ShouldThrowNotFoundException()
    {
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TaskItem?)null);

        Func<Task> act = async () => await _sut.DeleteAsync(99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task GetAllAsync_WhenCalled_ShouldReturnPagedResponse()
    {
        var queryParams = new TaskQueryParams { Page = 1, PageSize = 10 };
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Task 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        var pagedResponse = PagedResponse<TaskItem>.Create(tasks, 1, 10, 2);

        _taskRepositoryMock.Setup(r => r.GetAllAsync(queryParams)).ReturnsAsync(pagedResponse);

        var result = await _sut.GetAllAsync(queryParams);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Data.Should().HaveCount(2);
    }
}
