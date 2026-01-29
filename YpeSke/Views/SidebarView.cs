using DevExpress.XtraEditors;
using YpeSke.ViewModels;

namespace YpeSke.Views;

public class SidebarView : XtraUserControl
{
    private readonly Panel _profilePanel;
    private readonly Panel _navPanel;
    private MainViewModel? _viewModel;

    private NavigationTab _activeTab = NavigationTab.Chats;

    // Skype colors
    private readonly Color _sidebarBg = Color.FromArgb(0, 120, 212); // Skype blue
    private readonly Color _hoverBg = Color.FromArgb(0, 100, 180);
    private readonly Color _activeBg = Color.FromArgb(0, 90, 160);
    private readonly Color _iconColor = Color.White;

    public SidebarView()
    {
        InitializeComponent();
        _profilePanel = new Panel();
        _navPanel = new Panel();

        SetupProfileSection();
        SetupNavigation();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.Width = 68;
        this.BackColor = _sidebarBg;
    }

    private void SetupProfileSection()
    {
        _profilePanel.Dock = DockStyle.Top;
        _profilePanel.Height = 80;
        _profilePanel.BackColor = Color.Transparent;

        // User avatar
        var avatarPanel = new Panel
        {
            Size = new Size(44, 44),
            Location = new Point(12, 20),
            BackColor = Color.Transparent
        };
        avatarPanel.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // Avatar circle
            using var brush = new SolidBrush(Color.White);
            e.Graphics.FillEllipse(brush, 0, 0, 43, 43);

            // Initials
            using var font = new Font("Segoe UI", 14, FontStyle.Bold);
            using var textBrush = new SolidBrush(_sidebarBg);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("JD", font, textBrush, new Rectangle(0, 0, 44, 44), sf);

            // Online status dot
            using var statusBrush = new SolidBrush(Color.FromArgb(107, 183, 0));
            using var statusBorder = new Pen(Color.White, 2);
            e.Graphics.FillEllipse(statusBrush, 30, 30, 12, 12);
            e.Graphics.DrawEllipse(statusBorder, 30, 30, 12, 12);
        };

        _profilePanel.Controls.Add(avatarPanel);
    }

    private void SetupNavigation()
    {
        _navPanel.Dock = DockStyle.Fill;
        _navPanel.BackColor = Color.Transparent;

        var buttons = new[]
        {
            (NavigationTab.Chats, "\U0001F4AC", "Chats"),
            (NavigationTab.Calls, "\U0001F4DE", "Calls"),
            (NavigationTab.Contacts, "\U0001F465", "Contacts"),
            (NavigationTab.Notifications, "\U0001F514", "Notif")
        };

        var y = 0;
        foreach (var (tab, icon, label) in buttons)
        {
            var btn = CreateNavButton(tab, icon, label);
            btn.Location = new Point(0, y);
            _navPanel.Controls.Add(btn);
            y += 68;
        }
    }

    private Panel CreateNavButton(NavigationTab tab, string icon, string label)
    {
        var panel = new Panel
        {
            Size = new Size(68, 64),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Tag = tab
        };

        panel.Paint += (s, e) =>
        {
            var isActive = _activeTab == tab;
            var bg = isActive ? _activeBg : Color.Transparent;

            if (bg != Color.Transparent)
            {
                using var brush = new SolidBrush(bg);
                e.Graphics.FillRectangle(brush, panel.ClientRectangle);

                // Left indicator bar for active
                using var indicatorBrush = new SolidBrush(Color.White);
                e.Graphics.FillRectangle(indicatorBrush, 0, 8, 3, panel.Height - 16);
            }

            // Icon
            using var iconFont = new Font("Segoe UI Emoji", 20);
            using var iconBrush = new SolidBrush(_iconColor);
            var iconSf = new StringFormat { Alignment = StringAlignment.Center };
            e.Graphics.DrawString(icon, iconFont, iconBrush, new RectangleF(0, 10, panel.Width, 30), iconSf);

            // Label
            using var labelFont = new Font("Segoe UI", 8);
            using var labelBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            var labelSf = new StringFormat { Alignment = StringAlignment.Center };
            e.Graphics.DrawString(label, labelFont, labelBrush, new RectangleF(0, 42, panel.Width, 20), labelSf);
        };

        panel.MouseEnter += (s, e) =>
        {
            if (_activeTab != tab)
            {
                panel.BackColor = _hoverBg;
            }
        };

        panel.MouseLeave += (s, e) =>
        {
            panel.BackColor = Color.Transparent;
            panel.Invalidate();
        };

        panel.Click += (s, e) =>
        {
            _viewModel?.NavigateTo(tab);
        };

        return panel;
    }

    private void SetupLayout()
    {
        this.Controls.Add(_navPanel);
        this.Controls.Add(_profilePanel);
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
        _activeTab = _viewModel.CurrentTab;

        foreach (Control control in _navPanel.Controls)
        {
            control.Invalidate();
        }
    }
}
