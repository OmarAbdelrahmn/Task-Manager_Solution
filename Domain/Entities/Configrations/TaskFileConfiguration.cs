// Domain/EntitiesConfigrations/TaskFileConfiguration.cs
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities.Configrations;

public class TaskFileConfiguration : IEntityTypeConfiguration<TaskFile>
{
    public void Configure(EntityTypeBuilder<TaskFile> builder)
    {
        builder.ToTable("TaskFiles");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.StoredFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.FileUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        // file → task
        builder.HasOne(f => f.Task)
            .WithMany(t => t.Files)
            .HasForeignKey(f => f.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // file → occurrence (nullable)
        builder.HasOne(f => f.Occurrence)
            .WithMany(o => o.Files)
            .HasForeignKey(f => f.OccurrenceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // uploaded by
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(f => f.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}