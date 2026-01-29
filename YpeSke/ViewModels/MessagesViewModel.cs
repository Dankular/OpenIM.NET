using System.Collections.ObjectModel;
using YpeSke.Models;
using YpeSke.Services;

namespace YpeSke.ViewModels;

public class MessagesViewModel : BaseViewModel
{
    private readonly IMessageService _messageService;
    private ObservableCollection<ChatMessage> _messages = new();
    private Conversation? _currentConversation;
    private string _messageText = string.Empty;
    private bool _isTyping;
    private Guid _currentUserId;

    public MessagesViewModel(IMessageService messageService)
    {
        _messageService = messageService;
        _currentUserId = _messageService.CurrentUserId;
    }

    public ObservableCollection<ChatMessage> Messages
    {
        get => _messages;
        set => SetProperty(ref _messages, value);
    }

    public Conversation? CurrentConversation
    {
        get => _currentConversation;
        set
        {
            if (SetProperty(ref _currentConversation, value))
            {
                _ = LoadMessagesAsync();
                OnPropertyChanged(nameof(HasConversation));
            }
        }
    }

    public string MessageText
    {
        get => _messageText;
        set
        {
            if (SetProperty(ref _messageText, value))
            {
                OnPropertyChanged(nameof(CanSend));
            }
        }
    }

    public bool IsTyping
    {
        get => _isTyping;
        set => SetProperty(ref _isTyping, value);
    }

    public bool HasConversation => CurrentConversation != null;
    public bool CanSend => !string.IsNullOrWhiteSpace(MessageText) && HasConversation;
    public Guid CurrentUserId => _currentUserId;

    public event EventHandler? MessageSent;

    public async Task LoadMessagesAsync()
    {
        if (CurrentConversation == null)
        {
            Messages.Clear();
            return;
        }

        var messages = await _messageService.GetMessagesAsync(CurrentConversation.Id);

        // Mark messages with sender info
        foreach (var msg in messages)
        {
            msg.IsSentByCurrentUser = msg.SenderId == _currentUserId;
            if (!msg.IsSentByCurrentUser)
            {
                var sender = CurrentConversation.Participants.FirstOrDefault(p => p.Id == msg.SenderId);
                msg.SenderName = sender?.Name;
                msg.SenderAvatarUrl = sender?.AvatarUrl;
                msg.SenderInitials = sender?.Initials;
            }
        }

        Messages = new ObservableCollection<ChatMessage>(messages.OrderBy(m => m.Timestamp));

        // Mark as read
        if (CurrentConversation.UnreadCount > 0)
        {
            CurrentConversation.UnreadCount = 0;
            await _messageService.MarkConversationAsReadAsync(CurrentConversation.Id);
        }
    }

    public async Task SendMessageAsync()
    {
        if (!CanSend || CurrentConversation == null)
            return;

        var content = MessageText.Trim();
        MessageText = string.Empty;

        var message = new ChatMessage
        {
            SenderId = _currentUserId,
            ConversationId = CurrentConversation.Id,
            Content = content,
            Timestamp = DateTime.Now,
            IsSentByCurrentUser = true,
            DeliveryStatus = DeliveryStatus.Sending
        };

        // Add to UI immediately
        Messages.Add(message);
        CurrentConversation.Messages.Add(message);

        // Send via service
        var sent = await _messageService.SendMessageAsync(message);
        message.DeliveryStatus = DeliveryStatus.Sent;

        MessageSent?.Invoke(this, EventArgs.Empty);
    }

    public List<MessageDisplayItem> GetDisplayItems()
    {
        return Messages.Select(m => new MessageDisplayItem
        {
            Message = m,
            Content = m.Content,
            FormattedTime = m.FormattedTime,
            SenderName = m.SenderName ?? "",
            SenderAvatarUrl = m.SenderAvatarUrl,
            SenderInitials = m.SenderInitials ?? "?",
            IsSent = m.IsSentByCurrentUser,
            DeliveryStatusIcon = m.DeliveryStatusIcon,
            DeliveryStatusClass = m.DeliveryStatusClass,
            HasAvatar = !string.IsNullOrEmpty(m.SenderAvatarUrl)
        }).ToList();
    }

    public ChatHeaderDisplayItem? GetHeaderDisplayItem()
    {
        if (CurrentConversation == null)
            return null;

        var other = CurrentConversation.GetOtherParticipant(_currentUserId);
        return new ChatHeaderDisplayItem
        {
            Name = CurrentConversation.GetDisplayName(_currentUserId),
            AvatarUrl = CurrentConversation.GetDisplayAvatar(_currentUserId),
            Initials = CurrentConversation.GetDisplayInitials(_currentUserId),
            StatusClass = CurrentConversation.GetDisplayStatus(_currentUserId).ToString().ToLower(),
            StatusText = other?.StatusText ?? "Offline",
            HasAvatar = !string.IsNullOrEmpty(CurrentConversation.GetDisplayAvatar(_currentUserId))
        };
    }
}

public class MessageDisplayItem
{
    public ChatMessage Message { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public string FormattedTime { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public string SenderInitials { get; set; } = "?";
    public bool IsSent { get; set; }
    public string DeliveryStatusIcon { get; set; } = string.Empty;
    public string DeliveryStatusClass { get; set; } = string.Empty;
    public bool HasAvatar { get; set; }
    public string NoAvatar => HasAvatar ? "" : SenderInitials;
}

public class ChatHeaderDisplayItem
{
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Initials { get; set; } = string.Empty;
    public string StatusClass { get; set; } = "offline";
    public string StatusText { get; set; } = "Offline";
    public bool HasAvatar { get; set; }
    public string NoAvatar => HasAvatar ? "" : Initials;
}
