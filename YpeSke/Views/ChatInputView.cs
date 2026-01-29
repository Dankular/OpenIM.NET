using DevExpress.XtraEditors;
using YpeSke.ViewModels;

namespace YpeSke.Views;

public class ChatInputView : XtraUserControl
{
    private readonly TextEdit _messageInput;
    private readonly Panel _sendButton;
    private readonly Panel _emojiButton;
    private readonly Panel _attachButton;
    private readonly Panel _gifButton;
    private MessagesViewModel? _viewModel;

    private readonly Color _skypeBlue = Color.FromArgb(0, 120, 212);

    public ChatInputView()
    {
        InitializeComponent();
        _messageInput = new TextEdit();
        _sendButton = new Panel();
        _emojiButton = new Panel();
        _attachButton = new Panel();
        _gifButton = new Panel();

        SetupControls();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.Height = 60;
        this.BackColor = Color.White;
    }

    private void SetupControls()
    {
        // Emoji button
        SetupIconButton(_emojiButton, "\U0001F600", "Emoji");

        // GIF button
        SetupIconButton(_gifButton, "GIF", "Send GIF");

        // Attach button
        SetupIconButton(_attachButton, "\U0001F4CE", "Attach file");

        // Message input
        _messageInput.Properties.NullValuePrompt = "Type a message";
        _messageInput.Properties.NullValuePromptShowForEmptyValue = true;
        _messageInput.Properties.Appearance.BorderColor = Color.FromArgb(220, 220, 220);
        _messageInput.Properties.AppearanceFocused.BorderColor = _skypeBlue;
        _messageInput.Height = 36;
        _messageInput.KeyDown += MessageInput_KeyDown;
        _messageInput.EditValueChanged += MessageInput_EditValueChanged;

        // Send button
        _sendButton.Size = new Size(40, 40);
        _sendButton.Cursor = Cursors.Hand;
        _sendButton.Paint += (s, e) =>
        {
            var canSend = !string.IsNullOrWhiteSpace(_messageInput.Text) && (_viewModel?.HasConversation ?? false);
            var bgColor = canSend ? _skypeBlue : Color.FromArgb(200, 200, 200);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Circle background
            using var brush = new SolidBrush(bgColor);
            e.Graphics.FillEllipse(brush, 2, 2, 36, 36);

            // Send arrow icon
            using var iconBrush = new SolidBrush(Color.White);
            using var font = new Font("Segoe UI Symbol", 14);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("\u27A4", font, iconBrush, new Rectangle(2, 2, 36, 36), sf);
        };
        _sendButton.Click += SendButton_Click;
    }

    private void SetupIconButton(Panel btn, string icon, string tooltip)
    {
        btn.Size = new Size(36, 36);
        btn.Cursor = Cursors.Hand;
        btn.Tag = tooltip;

        btn.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Hover effect
            if (btn.BackColor == Color.FromArgb(240, 240, 240))
            {
                using var hoverBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
                e.Graphics.FillEllipse(hoverBrush, 2, 2, 32, 32);
            }

            // Icon
            var isGif = icon == "GIF";
            using var font = isGif ? new Font("Segoe UI", 10, FontStyle.Bold) : new Font("Segoe UI Emoji", 16);
            using var brush = new SolidBrush(Color.FromArgb(100, 100, 100));
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(icon, font, brush, new Rectangle(0, 0, 36, 36), sf);
        };

        btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(240, 240, 240); btn.Invalidate(); };
        btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; btn.Invalidate(); };
    }

    private void SetupLayout()
    {
        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Padding = new Padding(8, 10, 8, 10),
            Margin = new Padding(0),
            BackColor = Color.White
        };

        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40)); // Emoji
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40)); // GIF
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40)); // Attach
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Input
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8));  // Spacer
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44)); // Send

        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _emojiButton.Anchor = AnchorStyles.None;
        _gifButton.Anchor = AnchorStyles.None;
        _attachButton.Anchor = AnchorStyles.None;
        _messageInput.Dock = DockStyle.Fill;
        _sendButton.Anchor = AnchorStyles.None;

        tableLayout.Controls.Add(_emojiButton, 0, 0);
        tableLayout.Controls.Add(_gifButton, 1, 0);
        tableLayout.Controls.Add(_attachButton, 2, 0);
        tableLayout.Controls.Add(_messageInput, 3, 0);
        tableLayout.Controls.Add(new Panel(), 4, 0); // Spacer
        tableLayout.Controls.Add(_sendButton, 5, 0);

        // Top border
        var borderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(230, 230, 230)
        };

        this.Controls.Add(tableLayout);
        this.Controls.Add(borderPanel);
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
            _sendButton.Invalidate();
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
        _sendButton.Invalidate();
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
