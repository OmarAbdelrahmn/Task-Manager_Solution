// Domain/EntitiesConfigrations/AppTaskConfiguration.cs
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities.Configrations;

public class AppTaskConfiguration : IEntityTypeConfiguration<AppTask>
{
    public void Configure(EntityTypeBuilder<AppTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.RecurrenceType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Progress)
            .HasDefaultValue(0);

        // CreatedById → the user who created the task
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // one task → one conversation (task thread)
        builder.HasOne(t => t.Conversation)
            .WithOne(c => c.Task)
            .HasForeignKey<Conversation>(c => c.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}