using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data.Configurations;

public class TaskCollaboratorConfiguration : IEntityTypeConfiguration<TaskCollaborator>
{
    public void Configure(EntityTypeBuilder<TaskCollaborator> builder)
    {
        builder.ToTable("TaskCollaborators");

        builder.HasKey(tc => tc.Id);

        builder.Property(tc => tc.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(tc => tc.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(tc => tc.InvitedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(tc => tc.Task)
            .WithMany(t => t.Collaborators)
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tc => tc.InvitedUser)
            .WithMany()
            .HasForeignKey(tc => tc.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tc => tc.InvitedByUser)
            .WithMany()
            .HasForeignKey(tc => tc.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tc => new { tc.TaskId, tc.InvitedUserId })
            .IsUnique();
    }
}
