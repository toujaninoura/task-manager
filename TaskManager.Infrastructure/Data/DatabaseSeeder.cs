using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Tasks.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var demoUser = new User
        {
            Email = "demo@taskmanager.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo1234"),
            CreatedAt = now
        };

        await context.Users.AddAsync(demoUser);
        await context.SaveChangesAsync();

        var tasks = new List<TaskItem>
        {
            new()
            {
                UserId = demoUser.Id,
                Title = "Setup project repository",
                Description = "Initialize Git repo and configure CI/CD pipeline.",
                Status = TaskItemStatus.Todo,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(7),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Write technical specifications",
                Description = "Document API contracts and data models for the sprint.",
                Status = TaskItemStatus.Todo,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(3),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Security audit on authentication endpoints",
                Description = "Review JWT expiry, refresh token rotation and OWASP compliance.",
                Status = TaskItemStatus.Todo,
                Priority = TaskPriority.High,
                DueDate = null,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Implement task filtering API",
                Description = "Add query parameters for status, priority and due date filtering.",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(5),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Build Angular task list component",
                Description = "Standalone component with pagination and search support.",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Medium,
                DueDate = now.AddDays(10),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Integrate JWT interceptor in Angular",
                Description = "Attach Bearer token to all outgoing HTTP requests automatically.",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Medium,
                DueDate = null,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Optimize EF Core queries with AsNoTracking",
                Description = "Audit all read-only repository methods and apply AsNoTracking.",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(-1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Configure SQL Server connection string",
                Description = "Set up local and staging connection strings with TrustServerCertificate.",
                Status = TaskItemStatus.Done,
                Priority = TaskPriority.Low,
                DueDate = now.AddDays(-10),
                CreatedAt = now.AddDays(-15),
                UpdatedAt = now.AddDays(-10)
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Add FluentValidation on task creation",
                Description = "Validate Title length, Status enum and DueDate not in the past.",
                Status = TaskItemStatus.Done,
                Priority = TaskPriority.Medium,
                DueDate = null,
                CreatedAt = now.AddDays(-12),
                UpdatedAt = now.AddDays(-8)
            },
            new()
            {
                UserId = demoUser.Id,
                Title = "Deploy initial N-Tier solution structure",
                Description = "Domain, Application, Infrastructure, API and Tests projects scaffolded.",
                Status = TaskItemStatus.Done,
                Priority = TaskPriority.High,
                DueDate = now.AddDays(-5),
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now.AddDays(-5)
            }
        };

        await context.Tasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();
    }
}
