using System.Collections.ObjectModel;
using YpeSke.Models;
using YpeSke.Services;

namespace YpeSke.ViewModels;

public class ContactsViewModel : BaseViewModel
{
    private readonly IMessageService _messageService;
    private ObservableCollection<Conversation> _conversations = new();
    private Conversation? _selectedConversation;
    private string _searchText = string.Empty;
    private Guid _currentUserId;

    public ContactsViewModel(IMessageService messageService)
    {
        _messageService = messageService;
        _currentUserId = _messageService.CurrentUserId;
    }

    public ObservableCollection<Conversation> Conversations
    {
        get => _conversations;
        set => SetProperty(ref _conversations, value);
    }

    public Conversation? SelectedConversation
    {
        get => _selectedConversation;
        set
        {
            if (SetProperty(ref _selectedConversation, value))
            {
                ConversationSelected?.Invoke(this, value);
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterConversations();
            }
        }
    }

    public Guid CurrentUserId => _currentUserId;

    public event EventHandler<Conversation?>? ConversationSelected;

    public async Task LoadConversationsAsync()
    {
        var conversations = await _messageService.GetConversationsAsync();
        Conversations = new ObservableCollection<Conversation>(
            conversations.OrderByDescending(c => c.LastMessage?.Timestamp ?? DateTime.MinValue));
    }

    public void RefreshConversations()
    {
        var sorted = Conversations
            .OrderByDescending(c => c.LastMessage?.Timestamp ?? DateTime.MinValue)
            .ToList();

        Conversations.Clear();
        foreach (var conv in sorted)
        {
            Conversations.Add(conv);
        }
    }

    private void FilterConversations()
    {
        // For now, just refresh - in a real app, you'd filter based on SearchText
        OnPropertyChanged(nameof(Conversations));
    }

    public List<ConversationDisplayItem> GetDisplayItems()
    {
        return Conversations
            .Where(c => string.IsNullOrEmpty(SearchText) ||
                        c.GetDisplayName(_currentUserId).Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .Select(c => new ConversationDisplayItem
            {
                Conversation = c,
                Name = c.GetDisplayName(_currentUserId),
                AvatarUrl = c.GetDisplayAvatar(_currentUserId),
                Initials = c.GetDisplayInitials(_currentUserId),
                StatusClass = c.GetDisplayStatus(_currentUserId).ToString().ToLower(),
                LastMessagePreview = c.LastMessagePreview,
                LastMessageTime = c.LastMessageTime,
                UnreadCount = c.UnreadCount,
                HasAvatar = !string.IsNullOrEmpty(c.GetDisplayAvatar(_currentUserId)),
                HasUnread = c.UnreadCount > 0,
                IsSelected = c == SelectedConversation
            })
            .ToList();
    }
}

public class ConversationDisplayItem
{
    public Conversation Conversation { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Initials { get; set; } = string.Empty;
    public string StatusClass { get; set; } = "offline";
    public string LastMessagePreview { get; set; } = string.Empty;
    public string LastMessageTime { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public bool HasAvatar { get; set; }
    public bool HasUnread { get; set; }
    public bool IsSelected { get; set; }

    // For template binding - conditional display
    public string NoAvatar => HasAvatar ? "" : Initials;
}
