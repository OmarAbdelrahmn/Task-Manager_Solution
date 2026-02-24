// Domain/EntitiesConfigrations/TaskAssigneeConfiguration.cs
using Domain.Entities;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities.Configrations;

public class TaskAssigneeConfiguration : IEntityTypeConfiguration<TaskAssignee>
{
    public void Configure(EntityTypeBuilder<TaskAssignee> builder)
    {
        builder.ToTable("TaskAssignees");

        // composite PK — one user assigned once per task
        builder.HasKey(ta => new { ta.TaskId, ta.UserId });

        builder.Property(ta => ta.AssignedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // task → assignees
        builder.HasOne(ta => ta.Task)
            .WithMany(t => t.Assignees)
            .HasForeignKey(ta => ta.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // the assigned user
        builder.HasOne(ta => ta.User)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(ta => ta.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // the person who did the assigning
        builder.HasOne(ta => ta.AssignedBy)
            .WithMany()
            .HasForeignKey(ta => ta.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}