// Domain/EntitiesConfigrations/ConversationConfiguration.cs
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities.Configrations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .HasMaxLength(200);

        // TaskId is nullable — only set when Type = TaskThread
        builder.Property(c => c.TaskId)
            .IsRequired(false);

        // created by
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // unique constraint — one task can only have one task thread conversation
        builder.HasIndex(c => c.TaskId)
            .IsUnique()
            .HasFilter("[TaskId] IS NOT NULL");
    }
}