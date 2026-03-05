// Domain/Entities/Configurations/MessageReactionConfiguration.cs
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities.Configrations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("MessageReactions");

        // composite PK — one emoji per user per message
        builder.HasKey(r => new { r.MessageId, r.UserId, r.Emoji });

        builder.Property(r => r.Emoji)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(r => r.ReactedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // reaction → message
        builder.HasOne(r => r.Message)
            .WithMany(m => m.Reactions)
            .HasForeignKey(r => r.MessageId)
            .OnDelete(DeleteBehavior.Cascade);   // reactions die with the message

        // reaction → user
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}