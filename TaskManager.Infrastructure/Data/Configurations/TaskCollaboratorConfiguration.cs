using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Data.Configurations;

public class TaskCollaboratorConfiguration : IEntityTypeConfiguration<TaskCollaborator>
{
    public void Configure(EntityTypeBuilder<TaskCollaborator> builder)
    {
        builder.ToTable("TaskCollaborators");

        builder.HasKey(tc => tc.Id);

        builder.Property(tc => tc.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(tc => tc.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(tc => tc.InvitedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(tc => tc.Task)
            .WithMany(t => t.Collaborators)
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tc => tc.User)
            .WithMany(u => u.CollaborationsReceived)
            .HasForeignKey(tc => tc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tc => tc.InvitedByUser)
            .WithMany(u => u.CollaborationsGiven)
            .HasForeignKey(tc => tc.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tc => new { tc.TaskId, tc.UserId }).IsUnique();
        builder.HasIndex(tc => tc.UserId);
        builder.HasIndex(tc => tc.Status);
    }
}
