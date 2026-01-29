namespace YpeSke.Models;

public class Contact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Offline;
    public DateTime? LastSeen { get; set; }
    public string? Email { get; set; }
    public string Initials => GetInitials();

    private string GetInitials()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return "?";

        var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();

        return Name.Length >= 2 ? Name[..2].ToUpper() : Name.ToUpper();
    }

    public string StatusClass => Status switch
    {
        UserStatus.Online => "online",
        UserStatus.Away => "away",
        UserStatus.Busy => "busy",
        _ => "offline"
    };

    public string StatusText => Status switch
    {
        UserStatus.Online => "Online",
        UserStatus.Away => "Away",
        UserStatus.Busy => "Busy",
        UserStatus.Offline when LastSeen.HasValue => $"Last seen {FormatLastSeen(LastSeen.Value)}",
        _ => "Offline"
    };

    private static string FormatLastSeen(DateTime lastSeen)
    {
        var diff = DateTime.Now - lastSeen;
        if (diff.TotalMinutes < 1) return "just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
        return lastSeen.ToString("MMM d");
    }
}
