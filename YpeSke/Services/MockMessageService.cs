using YpeSke.Models;

namespace YpeSke.Services;

public class MockMessageService : IMessageService
{
    private readonly Contact _currentUser;
    private readonly List<Contact> _contacts;
    private readonly List<Conversation> _conversations;
    private readonly Dictionary<Guid, List<ChatMessage>> _messages;

    public Guid CurrentUserId => _currentUser.Id;

    public MockMessageService()
    {
        _currentUser = CreateCurrentUser();
        _contacts = CreateMockContacts();
        _messages = new Dictionary<Guid, List<ChatMessage>>();
        _conversations = CreateMockConversations();
    }

    private Contact CreateCurrentUser()
    {
        return new Contact
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john.doe@example.com",
            Status = UserStatus.Online
        };
    }

    private List<Contact> CreateMockContacts()
    {
        return new List<Contact>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Alice Johnson",
                Email = "alice.johnson@example.com",
                Status = UserStatus.Online,
                LastSeen = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Bob Smith",
                Email = "bob.smith@example.com",
                Status = UserStatus.Away,
                LastSeen = DateTime.Now.AddMinutes(-15)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Carol Williams",
                Email = "carol.williams@example.com",
                Status = UserStatus.Busy,
                LastSeen = DateTime.Now.AddMinutes(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "David Brown",
                Email = "david.brown@example.com",
                Status = UserStatus.Offline,
                LastSeen = DateTime.Now.AddHours(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Emma Davis",
                Email = "emma.davis@example.com",
                Status = UserStatus.Online,
                LastSeen = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Frank Miller",
                Email = "frank.miller@example.com",
                Status = UserStatus.Offline,
                LastSeen = DateTime.Now.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Grace Wilson",
                Email = "grace.wilson@example.com",
                Status = UserStatus.Online,
                LastSeen = DateTime.Now
            }
        };
    }

    private List<Conversation> CreateMockConversations()
    {
        var conversations = new List<Conversation>();

        // Create a conversation with each contact
        foreach (var contact in _contacts)
        {
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Participants = new List<Contact> { _currentUser, contact }
            };

            // Add some mock messages
            var messages = CreateMockMessagesForConversation(conversation, contact);
            _messages[conversation.Id] = messages;
            conversation.Messages = messages;
            conversation.UnreadCount = contact.Status == UserStatus.Online ? new Random().Next(0, 4) : 0;

            conversations.Add(conversation);
        }

        return conversations;
    }

    private List<ChatMessage> CreateMockMessagesForConversation(Conversation conversation, Contact otherContact)
    {
        var messages = new List<ChatMessage>();
        var random = new Random();
        var baseTime = DateTime.Now.AddHours(-random.Next(1, 48));

        var sampleMessages = new[]
        {
            ("Hey! How's it going?", false),
            ("Hi! I'm doing great, thanks for asking!", true),
            ("Did you see the latest project updates?", false),
            ("Yes, I just reviewed them. Looks good!", true),
            ("Should we schedule a call to discuss?", false),
            ("Sure, I'm free tomorrow afternoon.", true),
            ("Perfect! Let's do 3 PM.", false),
            ("Sounds good to me!", true),
            ("Looking forward to it!", false)
        };

        var messageCount = random.Next(3, sampleMessages.Length);

        for (int i = 0; i < messageCount; i++)
        {
            var (content, isSent) = sampleMessages[i];
            var sender = isSent ? _currentUser : otherContact;

            messages.Add(new ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderId = sender.Id,
                Content = content,
                Timestamp = baseTime.AddMinutes(i * random.Next(5, 30)),
                IsRead = true,
                MessageType = MessageType.Text,
                DeliveryStatus = isSent ? DeliveryStatus.Read : DeliveryStatus.Delivered,
                IsSentByCurrentUser = isSent,
                SenderName = sender.Name,
                SenderInitials = sender.Initials
            });
        }

        return messages;
    }

    public Contact GetCurrentUser() => _currentUser;

    public Task<List<Contact>> GetContactsAsync()
    {
        return Task.FromResult(_contacts);
    }

    public Task<List<Conversation>> GetConversationsAsync()
    {
        return Task.FromResult(_conversations);
    }

    public Task<List<ChatMessage>> GetMessagesAsync(Guid conversationId)
    {
        if (_messages.TryGetValue(conversationId, out var messages))
        {
            return Task.FromResult(messages);
        }

        return Task.FromResult(new List<ChatMessage>());
    }

    public Task<ChatMessage> SendMessageAsync(ChatMessage message)
    {
        message.Id = Guid.NewGuid();
        message.Timestamp = DateTime.Now;
        message.DeliveryStatus = DeliveryStatus.Sent;

        if (_messages.TryGetValue(message.ConversationId, out var messages))
        {
            messages.Add(message);
        }
        else
        {
            _messages[message.ConversationId] = new List<ChatMessage> { message };
        }

        // Update conversation
        var conversation = _conversations.FirstOrDefault(c => c.Id == message.ConversationId);
        if (conversation != null && !conversation.Messages.Contains(message))
        {
            conversation.Messages.Add(message);
        }

        // Simulate delivery after a short delay
        Task.Run(async () =>
        {
            await Task.Delay(500);
            message.DeliveryStatus = DeliveryStatus.Delivered;

            await Task.Delay(1000);
            message.DeliveryStatus = DeliveryStatus.Read;
        });

        return Task.FromResult(message);
    }

    public Task MarkConversationAsReadAsync(Guid conversationId)
    {
        var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
        if (conversation != null)
        {
            conversation.UnreadCount = 0;
            foreach (var message in conversation.Messages)
            {
                message.IsRead = true;
            }
        }

        return Task.CompletedTask;
    }

    public Task<Contact?> GetContactAsync(Guid contactId)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId);
        return Task.FromResult(contact);
    }
}
