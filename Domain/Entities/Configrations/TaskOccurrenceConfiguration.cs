// Domain/EntitiesConfigrations/TaskOccurrenceConfiguration.cs
using Domain.Entities;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities.Configrations;

public class TaskOccurrenceConfiguration : IEntityTypeConfiguration<TaskOccurrence>
{
    public void Configure(EntityTypeBuilder<TaskOccurrence> builder)
    {
        builder.ToTable("TaskOccurrences");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.Progress)
            .HasDefaultValue(0);

        builder.Property(o => o.Notes)
            .HasMaxLength(2000);

        // occurrence → parent task
        builder.HasOne(o => o.Task)
            .WithMany(t => t.Occurrences)
            .HasForeignKey(o => o.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}