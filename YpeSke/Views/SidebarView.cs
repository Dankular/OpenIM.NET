using DevExpress.XtraEditors;
using YpeSke.ViewModels;

namespace YpeSke.Views;

public class SidebarView : XtraUserControl
{
    private readonly SimpleButton _chatsButton;
    private readonly SimpleButton _callsButton;
    private readonly SimpleButton _contactsButton;
    private readonly SimpleButton _notificationsButton;
    private MainViewModel? _viewModel;

    private readonly Color _activeColor = Color.FromArgb(229, 246, 253);
    private readonly Color _activeForeColor = Color.FromArgb(0, 175, 240);
    private readonly Color _normalColor = Color.FromArgb(245, 245, 245);
    private readonly Color _normalForeColor = Color.FromArgb(97, 97, 97);

    public SidebarView()
    {
        InitializeComponent();
        _chatsButton = CreateNavButton("\U0001F4AC", "Chats");
        _callsButton = CreateNavButton("\U0001F4DE", "Calls");
        _contactsButton = CreateNavButton("\U0001F465", "Contacts");
        _notificationsButton = CreateNavButton("\U0001F514", "Notifications");

        SetupLayout();
        SetupEvents();
    }

    private void InitializeComponent()
    {
        this.Width = 80;
        this.BackColor = Color.FromArgb(245, 245, 245);
    }

    private SimpleButton CreateNavButton(string icon, string label)
    {
        var button = new SimpleButton
        {
            Text = $"{icon}\n{label}",
            Height = 70,
            Dock = DockStyle.Top,
            Appearance =
            {
                BackColor = _normalColor,
                ForeColor = _normalForeColor,
                BorderColor = Color.Transparent,
                TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center }
            },
            AppearanceHovered =
            {
                BackColor = Color.FromArgb(232, 232, 232),
                ForeColor = _normalForeColor,
                BorderColor = Color.Transparent
            }
        };

        return button;
    }

    private void SetupLayout()
    {
        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        _chatsButton.Width = this.Width - 8;
        _callsButton.Width = this.Width - 8;
        _contactsButton.Width = this.Width - 8;
        _notificationsButton.Width = this.Width - 8;

        flowPanel.Controls.Add(_chatsButton);
        flowPanel.Controls.Add(_callsButton);
        flowPanel.Controls.Add(_contactsButton);
        flowPanel.Controls.Add(_notificationsButton);

        this.Controls.Add(flowPanel);
    }

    private void SetupEvents()
    {
        _chatsButton.Click += (s, e) => _viewModel?.NavigateTo(NavigationTab.Chats);
        _callsButton.Click += (s, e) => _viewModel?.NavigateTo(NavigationTab.Calls);
        _contactsButton.Click += (s, e) => _viewModel?.NavigateTo(NavigationTab.Contacts);
        _notificationsButton.Click += (s, e) => _viewModel?.NavigateTo(NavigationTab.Notifications);
    }

    public void SetViewModel(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        UpdateActiveState();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentTab))
        {
            UpdateActiveState();
        }
    }

    private void UpdateActiveState()
    {
        if (_viewModel == null) return;

        SetButtonActive(_chatsButton, _viewModel.CurrentTab == NavigationTab.Chats);
        SetButtonActive(_callsButton, _viewModel.CurrentTab == NavigationTab.Calls);
        SetButtonActive(_contactsButton, _viewModel.CurrentTab == NavigationTab.Contacts);
        SetButtonActive(_notificationsButton, _viewModel.CurrentTab == NavigationTab.Notifications);
    }

    private void SetButtonActive(SimpleButton button, bool isActive)
    {
        if (isActive)
        {
            button.Appearance.BackColor = _activeColor;
            button.Appearance.ForeColor = _activeForeColor;
        }
        else
        {
            button.Appearance.BackColor = _normalColor;
            button.Appearance.ForeColor = _normalForeColor;
        }
    }
}
