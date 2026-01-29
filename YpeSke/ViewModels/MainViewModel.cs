using YpeSke.Models;
using YpeSke.Services;

namespace YpeSke.ViewModels;

public enum NavigationTab
{
    Chats,
    Calls,
    Contacts,
    Notifications
}

public class MainViewModel : BaseViewModel
{
    private readonly IMessageService _messageService;
    private NavigationTab _currentTab = NavigationTab.Chats;
    private Contact? _currentUser;

    public ContactsViewModel ContactsViewModel { get; }
    public MessagesViewModel MessagesViewModel { get; }

    public MainViewModel(IMessageService messageService)
    {
        _messageService = messageService;

        ContactsViewModel = new ContactsViewModel(messageService);
        MessagesViewModel = new MessagesViewModel(messageService);

        // Wire up events
        ContactsViewModel.ConversationSelected += OnConversationSelected;
        MessagesViewModel.MessageSent += OnMessageSent;

        // Load current user
        _currentUser = _messageService.GetCurrentUser();
    }

    public NavigationTab CurrentTab
    {
        get => _currentTab;
        set
        {
            if (SetProperty(ref _currentTab, value))
            {
                OnPropertyChanged(nameof(ChatsActive));
                OnPropertyChanged(nameof(CallsActive));
                OnPropertyChanged(nameof(ContactsActive));
                OnPropertyChanged(nameof(NotificationsActive));
            }
        }
    }

    public Contact? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public string ChatsActive => CurrentTab == NavigationTab.Chats ? "active" : "";
    public string CallsActive => CurrentTab == NavigationTab.Calls ? "active" : "";
    public string ContactsActive => CurrentTab == NavigationTab.Contacts ? "active" : "";
    public string NotificationsActive => CurrentTab == NavigationTab.Notifications ? "active" : "";

    public async Task InitializeAsync()
    {
        await ContactsViewModel.LoadConversationsAsync();
    }

    private void OnConversationSelected(object? sender, Conversation? conversation)
    {
        MessagesViewModel.CurrentConversation = conversation;
    }

    private void OnMessageSent(object? sender, EventArgs e)
    {
        ContactsViewModel.RefreshConversations();
    }

    public void NavigateTo(NavigationTab tab)
    {
        CurrentTab = tab;
    }
}
