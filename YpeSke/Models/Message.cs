namespace YpeSke.Models;

public enum MessageType
{
    Text,
    Image,
    File,
    System
}

public enum DeliveryStatus
{
    Sending,
    Sent,
    Delivered,
    Read,
    Failed
}

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Sent;

    // For display purposes
    public string? SenderName { get; set; }
    public string? SenderAvatarUrl { get; set; }
    public string? SenderInitials { get; set; }
    public bool IsSentByCurrentUser { get; set; }

    public string FormattedTime => FormatTimestamp(Timestamp);
    public string DeliveryStatusIcon => DeliveryStatus switch
    {
        DeliveryStatus.Sending => "...",
        DeliveryStatus.Sent => "✓",
        DeliveryStatus.Delivered => "✓✓",
        DeliveryStatus.Read => "✓✓",
        DeliveryStatus.Failed => "!",
        _ => ""
    };

    public string DeliveryStatusClass => DeliveryStatus switch
    {
        DeliveryStatus.Read => "read",
        DeliveryStatus.Failed => "failed",
        _ => ""
    };

    private static string FormatTimestamp(DateTime timestamp)
    {
        var today = DateTime.Today;
        if (timestamp.Date == today)
            return timestamp.ToString("h:mm tt");
        if (timestamp.Date == today.AddDays(-1))
            return $"Yesterday {timestamp:h:mm tt}";
        if (timestamp.Date > today.AddDays(-7))
            return timestamp.ToString("ddd h:mm tt");
        return timestamp.ToString("MMM d, h:mm tt");
    }
}
