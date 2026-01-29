namespace YpeSke.Models;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<Contact> Participants { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();
    public ChatMessage? LastMessage => Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault();
    public int UnreadCount { get; set; }
    public bool IsGroup => Participants.Count > 2;
    public string? GroupName { get; set; }

    // Get the "other" contact in a 1:1 conversation
    public Contact? GetOtherParticipant(Guid currentUserId)
    {
        return Participants.FirstOrDefault(p => p.Id != currentUserId);
    }

    public string GetDisplayName(Guid currentUserId)
    {
        if (IsGroup && !string.IsNullOrEmpty(GroupName))
            return GroupName;

        var other = GetOtherParticipant(currentUserId);
        return other?.Name ?? "Unknown";
    }

    public string? GetDisplayAvatar(Guid currentUserId)
    {
        if (IsGroup)
            return null;

        var other = GetOtherParticipant(currentUserId);
        return other?.AvatarUrl;
    }

    public string GetDisplayInitials(Guid currentUserId)
    {
        if (IsGroup && !string.IsNullOrEmpty(GroupName))
        {
            var words = GroupName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 2)
                return $"{words[0][0]}{words[1][0]}".ToUpper();
            return GroupName.Length >= 2 ? GroupName[..2].ToUpper() : GroupName.ToUpper();
        }

        var other = GetOtherParticipant(currentUserId);
        return other?.Initials ?? "?";
    }

    public UserStatus GetDisplayStatus(Guid currentUserId)
    {
        if (IsGroup)
            return Participants.Any(p => p.Id != currentUserId && p.Status == UserStatus.Online)
                ? UserStatus.Online
                : UserStatus.Offline;

        var other = GetOtherParticipant(currentUserId);
        return other?.Status ?? UserStatus.Offline;
    }

    public string LastMessagePreview
    {
        get
        {
            if (LastMessage == null)
                return "No messages yet";

            var prefix = LastMessage.IsSentByCurrentUser ? "You: " : "";
            var content = LastMessage.Content;

            if (content.Length > 40)
                content = content[..40] + "...";

            return prefix + content;
        }
    }

    public string LastMessageTime
    {
        get
        {
            if (LastMessage == null)
                return "";

            var timestamp = LastMessage.Timestamp;
            var today = DateTime.Today;

            if (timestamp.Date == today)
                return timestamp.ToString("h:mm tt");
            if (timestamp.Date == today.AddDays(-1))
                return "Yesterday";
            if (timestamp.Date > today.AddDays(-7))
                return timestamp.ToString("ddd");
            return timestamp.ToString("MMM d");
        }
    }
}
