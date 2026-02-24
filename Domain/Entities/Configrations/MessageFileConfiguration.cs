// Domain/EntitiesConfigrations/MessageFileConfiguration.cs
using Domain.Entities;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities.Configrations;

public class MessageFileConfiguration : IEntityTypeConfiguration<MessageFile>
{
    public void Configure(EntityTypeBuilder<MessageFile> builder)
    {
        builder.ToTable("MessageFiles");

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

        // file → message
        builder.HasOne(f => f.Message)
            .WithMany(m => m.Files)
            .HasForeignKey(f => f.MessageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}