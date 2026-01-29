using DevExpress.XtraEditors;
using YpeSke.Services;
using YpeSke.ViewModels;

namespace YpeSke.Views;

public class MainForm : XtraForm
{
    private readonly MainViewModel _viewModel;
    private readonly SidebarView _sidebarView;
    private readonly ContactsView _contactsView;
    private readonly MessagesView _messagesView;
    private readonly ChatInputView _chatInputView;
    private readonly SplitContainerControl _mainSplitter;
    private readonly SplitContainerControl _chatSplitter;

    public MainForm()
    {
        InitializeComponent();

        // Initialize services and viewmodel
        var messageService = new MockMessageService();
        _viewModel = new MainViewModel(messageService);

        // Initialize views
        _sidebarView = new SidebarView();
        _contactsView = new ContactsView();
        _messagesView = new MessagesView();
        _chatInputView = new ChatInputView();
        _mainSplitter = new SplitContainerControl();
        _chatSplitter = new SplitContainerControl();

        SetupLayout();
        SetupBindings();

        // Load initial data
        this.Load += MainForm_Load;
    }

    private void InitializeComponent()
    {
        this.Text = "YpeSke - Chat";
        this.Size = new Size(1200, 800);
        this.MinimumSize = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;
    }

    private void SetupLayout()
    {
        // Main layout: Sidebar | (Contacts | Chat)

        // Setup sidebar
        _sidebarView.Dock = DockStyle.Left;
        _sidebarView.Width = 80;

        // Setup contact list panel (left side of main splitter)
        var contactsPanel = new Panel { Dock = DockStyle.Fill };
        contactsPanel.Controls.Add(_contactsView);
        _contactsView.Dock = DockStyle.Fill;

        // Setup chat area (right side of main splitter)
        var chatPanel = new Panel { Dock = DockStyle.Fill };

        // Chat area has messages view and input
        _chatInputView.Dock = DockStyle.Bottom;
        _chatInputView.Height = 60;

        _messagesView.Dock = DockStyle.Fill;

        chatPanel.Controls.Add(_messagesView);
        chatPanel.Controls.Add(_chatInputView);

        // Configure main splitter (contacts | chat)
        _mainSplitter.Dock = DockStyle.Fill;
        _mainSplitter.Horizontal = true;
        _mainSplitter.Panel1.Controls.Add(contactsPanel);
        _mainSplitter.Panel2.Controls.Add(chatPanel);
        _mainSplitter.SplitterPosition = 320;
        _mainSplitter.Panel1.MinSize = 250;
        _mainSplitter.Panel2.MinSize = 400;

        // Add to form
        this.Controls.Add(_mainSplitter);
        this.Controls.Add(_sidebarView);
    }

    private void SetupBindings()
    {
        _sidebarView.SetViewModel(_viewModel);
        _contactsView.SetViewModel(_viewModel.ContactsViewModel);
        _messagesView.SetViewModel(_viewModel.MessagesViewModel);
        _chatInputView.SetViewModel(_viewModel.MessagesViewModel);

        // Wire up message sent event to refresh messages view
        _chatInputView.MessageSent += (s, e) =>
        {
            _messagesView.RefreshData();
            _messagesView.ScrollToBottom();
            _contactsView.RefreshData();
        };
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        await _viewModel.InitializeAsync();
        _contactsView.RefreshData();

        // Auto-select first conversation to show the chat view
        if (_viewModel.ContactsViewModel.Conversations.Count > 0)
        {
            _viewModel.ContactsViewModel.SelectedConversation = _viewModel.ContactsViewModel.Conversations[0];
            _contactsView.RefreshData();
            _messagesView.RefreshData();
        }
    }
}
