using YpeSke.Models;

namespace YpeSke.ViewModels;

public class ConversationViewModel : BaseViewModel
{
    private Conversation? _conversation;
    private Contact? _otherParticipant;
    private Guid _currentUserId;

    public ConversationViewModel(Guid currentUserId)
    {
        _currentUserId = currentUserId;
    }

    public Conversation? Conversation
    {
        get => _conversation;
        set
        {
            if (SetProperty(ref _conversation, value))
            {
                _otherParticipant = value?.GetOtherParticipant(_currentUserId);
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(DisplayAvatar));
                OnPropertyChanged(nameof(DisplayInitials));
                OnPropertyChanged(nameof(DisplayStatus));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsGroup));
            }
        }
    }

    public string DisplayName => Conversation?.GetDisplayName(_currentUserId) ?? string.Empty;
    public string? DisplayAvatar => Conversation?.GetDisplayAvatar(_currentUserId);
    public string DisplayInitials => Conversation?.GetDisplayInitials(_currentUserId) ?? "?";
    public UserStatus DisplayStatus => Conversation?.GetDisplayStatus(_currentUserId) ?? UserStatus.Offline;
    public string StatusText => _otherParticipant?.StatusText ?? "Offline";
    public bool IsGroup => Conversation?.IsGroup ?? false;
}
