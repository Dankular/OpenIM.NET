using YpeSke.Models;

namespace YpeSke.Services;

public interface IMessageService
{
    Guid CurrentUserId { get; }

    Contact GetCurrentUser();
    Task<List<Contact>> GetContactsAsync();
    Task<List<Conversation>> GetConversationsAsync();
    Task<List<ChatMessage>> GetMessagesAsync(Guid conversationId);
    Task<ChatMessage> SendMessageAsync(ChatMessage message);
    Task MarkConversationAsReadAsync(Guid conversationId);
    Task<Contact?> GetContactAsync(Guid contactId);
}
