using DevExpress.XtraEditors;
using YpeSke.ViewModels;

namespace YpeSke.Views;

public class ChatInputView : XtraUserControl
{
    private readonly MemoEdit _messageInput;
    private readonly SimpleButton _sendButton;
    private readonly SimpleButton _emojiButton;
    private readonly SimpleButton _attachButton;
    private MessagesViewModel? _viewModel;

    public ChatInputView()
    {
        InitializeComponent();
        _messageInput = new MemoEdit();
        _sendButton = new SimpleButton();
        _emojiButton = new SimpleButton();
        _attachButton = new SimpleButton();

        SetupControls();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.Height = 60;
        this.BackColor = Color.White;
        this.Padding = new Padding(12, 8, 12, 8);
    }

    private void SetupControls()
    {
        // Emoji button
        _emojiButton.Text = "\U0001F600";
        _emojiButton.Width = 40;
        _emojiButton.Height = 40;
        _emojiButton.Appearance.BorderColor = Color.Transparent;
        _emojiButton.Appearance.BackColor = Color.Transparent;

        // Attach button
        _attachButton.Text = "\U0001F4CE";
        _attachButton.Width = 40;
        _attachButton.Height = 40;
        _attachButton.Appearance.BorderColor = Color.Transparent;
        _attachButton.Appearance.BackColor = Color.Transparent;

        // Message input
        _messageInput.Properties.NullValuePrompt = "Type a message...";
        _messageInput.Properties.NullValuePromptShowForEmptyValue = true;
        _messageInput.Properties.ScrollBars = ScrollBars.None;
        _messageInput.Properties.AcceptsReturn = false;
        _messageInput.Height = 40;
        _messageInput.KeyDown += MessageInput_KeyDown;
        _messageInput.EditValueChanged += MessageInput_EditValueChanged;

        // Send button
        _sendButton.Text = "\u27A4";
        _sendButton.Width = 40;
        _sendButton.Height = 40;
        _sendButton.Appearance.BackColor = Color.FromArgb(0, 175, 240);
        _sendButton.Appearance.ForeColor = Color.White;
        _sendButton.Enabled = false;
        _sendButton.Click += SendButton_Click;
    }

    private void SetupLayout()
    {
        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));

        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _emojiButton.Dock = DockStyle.Fill;
        _attachButton.Dock = DockStyle.Fill;
        _messageInput.Dock = DockStyle.Fill;
        _sendButton.Dock = DockStyle.Fill;

        tableLayout.Controls.Add(_emojiButton, 0, 0);
        tableLayout.Controls.Add(_attachButton, 1, 0);
        tableLayout.Controls.Add(_messageInput, 2, 0);
        tableLayout.Controls.Add(_sendButton, 3, 0);

        this.Controls.Add(tableLayout);
    }

    public void SetViewModel(MessagesViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        UpdateSendButtonState();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MessagesViewModel.HasConversation))
        {
            this.Enabled = _viewModel?.HasConversation ?? false;
        }
    }

    private void MessageInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift)
        {
            e.SuppressKeyPress = true;
            _ = SendMessageAsync();
        }
    }

    private void MessageInput_EditValueChanged(object? sender, EventArgs e)
    {
        UpdateSendButtonState();
    }

    private void UpdateSendButtonState()
    {
        var hasText = !string.IsNullOrWhiteSpace(_messageInput.Text);
        var hasConversation = _viewModel?.HasConversation ?? false;
        _sendButton.Enabled = hasText && hasConversation;
    }

    private async void SendButton_Click(object? sender, EventArgs e)
    {
        await SendMessageAsync();
    }

    private async Task SendMessageAsync()
    {
        if (_viewModel == null || string.IsNullOrWhiteSpace(_messageInput.Text))
            return;

        _viewModel.MessageText = _messageInput.Text;
        _messageInput.Text = string.Empty;
        await _viewModel.SendMessageAsync();

        MessageSent?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? MessageSent;

    public void FocusInput()
    {
        _messageInput.Focus();
    }

    public void ClearInput()
    {
        _messageInput.Text = string.Empty;
    }
}
