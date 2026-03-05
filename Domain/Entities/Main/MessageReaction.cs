// Domain/Entities/Main/MessageReaction.cs
using Domain.Entities.Identity;

namespace Domain.Entities.Main;

public class MessageReaction
{
    public int MessageId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;      // e.g. "👍", "❤️", "😂"
    public DateTime ReactedAt { get; set; } = DateTime.Now;

    // Nav
    public Message Message { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
}