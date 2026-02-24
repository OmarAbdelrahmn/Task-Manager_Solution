// Domain/EntitiesConfigrations/ConversationParticipantConfiguration.cs
using Domain.Entities;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities.Configrations;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("ConversationParticipants");

        // composite PK — one user per conversation
        builder.HasKey(cp => new { cp.ConversationId, cp.UserId });

        builder.Property(cp => cp.JoinedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // participant → conversation
        builder.HasOne(cp => cp.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        // participant → user
        builder.HasOne(cp => cp.User)
            .WithMany(u => u.Conversations)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}