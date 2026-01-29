using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using YpeSke.ViewModels;

namespace YpeSke.Views;

public class MessagesView : XtraUserControl
{
    private readonly GridControl _gridControl;
    private readonly GridView _gridView;
    private readonly Panel _headerPanel;
    private readonly Label _headerName;
    private readonly Label _headerStatus;
    private readonly Panel _avatarPanel;
    private readonly Panel _emptyStatePanel;
    private MessagesViewModel? _viewModel;
    private string _headerInitials = "";

    public MessagesView()
    {
        InitializeComponent();
        _gridControl = new GridControl { Dock = DockStyle.Fill };
        _gridView = new GridView();
        _headerPanel = new Panel();
        _headerName = new Label();
        _headerStatus = new Label();
        _avatarPanel = new Panel();
        _emptyStatePanel = new Panel();

        SetupHeader();
        SetupEmptyState();
        SetupGridControl();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.BackColor = Color.FromArgb(245, 245, 245);
    }

    private void SetupHeader()
    {
        _headerPanel.Dock = DockStyle.Top;
        _headerPanel.Height = 65;
        _headerPanel.BackColor = Color.White;
        _headerPanel.Visible = false;

        _avatarPanel.Size = new Size(40, 40);
        _avatarPanel.Location = new Point(16, 12);
        _avatarPanel.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(0, 120, 212));
            e.Graphics.FillEllipse(brush, 0, 0, 39, 39);

            using var font = new Font("Segoe UI", 12, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(_headerInitials, font, textBrush, new Rectangle(0, 0, 40, 40), sf);
        };

        _headerName.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        _headerName.ForeColor = Color.FromArgb(36, 36, 36);
        _headerName.Location = new Point(70, 12);
        _headerName.AutoSize = true;

        _headerStatus.Font = new Font("Segoe UI", 10);
        _headerStatus.ForeColor = Color.FromArgb(107, 183, 0); // Green for online
        _headerStatus.Location = new Point(70, 36);
        _headerStatus.AutoSize = true;

        // Call buttons on the right
        var videoBtn = CreateHeaderButton("\U0001F4F9", 36); // Video camera
        var audioBtn = CreateHeaderButton("\U0001F4DE", 36); // Phone
        var moreBtn = CreateHeaderButton("\u22EE", 36);      // More (vertical dots)

        _headerPanel.SizeChanged += (s, e) =>
        {
            moreBtn.Location = new Point(_headerPanel.Width - 50, 15);
            audioBtn.Location = new Point(_headerPanel.Width - 90, 15);
            videoBtn.Location = new Point(_headerPanel.Width - 130, 15);
        };

        _headerPanel.Controls.Add(_avatarPanel);
        _headerPanel.Controls.Add(_headerName);
        _headerPanel.Controls.Add(_headerStatus);
        _headerPanel.Controls.Add(videoBtn);
        _headerPanel.Controls.Add(audioBtn);
        _headerPanel.Controls.Add(moreBtn);

        // Bottom border
        _headerPanel.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(230, 230, 230));
            e.Graphics.DrawLine(pen, 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);
        };
    }

    private Panel CreateHeaderButton(string icon, int size)
    {
        var btn = new Panel
        {
            Size = new Size(size, size),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        btn.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Circle background on hover
            if (btn.Tag as string == "hover")
            {
                using var hoverBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
                e.Graphics.FillEllipse(hoverBrush, 0, 0, size - 1, size - 1);
            }

            // Icon
            using var font = new Font("Segoe UI Emoji", 14);
            using var brush = new SolidBrush(Color.FromArgb(0, 120, 212));
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(icon, font, brush, new Rectangle(0, 0, size, size), sf);
        };

        btn.MouseEnter += (s, e) => { btn.Tag = "hover"; btn.Invalidate(); };
        btn.MouseLeave += (s, e) => { btn.Tag = null; btn.Invalidate(); };

        return btn;
    }

    private void SetupEmptyState()
    {
        _emptyStatePanel.Dock = DockStyle.Fill;
        _emptyStatePanel.BackColor = Color.FromArgb(245, 245, 245);

        var titleLabel = new Label
        {
            Text = "Select a conversation",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(36, 36, 36),
            AutoSize = true
        };

        var descLabel = new Label
        {
            Text = "Choose a contact from the list to start chatting",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.FromArgb(97, 97, 97),
            AutoSize = true
        };

        _emptyStatePanel.SizeChanged += (s, e) =>
        {
            titleLabel.Location = new Point(
                (_emptyStatePanel.Width - titleLabel.Width) / 2,
                (_emptyStatePanel.Height - 50) / 2
            );
            descLabel.Location = new Point(
                (_emptyStatePanel.Width - descLabel.Width) / 2,
                titleLabel.Bottom + 10
            );
        };

        _emptyStatePanel.Controls.Add(titleLabel);
        _emptyStatePanel.Controls.Add(descLabel);
    }

    private void SetupGridControl()
    {
        _gridControl.MainView = _gridView;

        // Hide standard grid UI
        _gridView.OptionsView.ShowColumnHeaders = false;
        _gridView.OptionsView.ShowGroupPanel = false;
        _gridView.OptionsView.ShowIndicator = false;
        _gridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;
        _gridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
        _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedRow = false;
        _gridView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        _gridView.Appearance.Row.BackColor = Color.FromArgb(245, 245, 245);

        // Single column for custom drawing
        var col = _gridView.Columns.AddVisible("Content");
        col.OptionsColumn.ReadOnly = true;

        // Custom drawing
        _gridView.CalcRowHeight += GridView_CalcRowHeight;
        _gridView.CustomDrawCell += GridView_CustomDrawCell;
    }

    private void GridView_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
    {
        e.RowHeight = 70;
    }

    private void GridView_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
    {
        if (e.RowHandle < 0) return;

        var item = _gridView.GetRow(e.RowHandle) as MessageDisplayItem;
        if (item == null) return;

        e.Handled = true;
        var g = e.Graphics;
        var bounds = e.Bounds;

        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Background (chat area background)
        using (var bgBrush = new SolidBrush(Color.FromArgb(245, 245, 245)))
        {
            g.FillRectangle(bgBrush, bounds);
        }

        var margin = 12;
        var avatarSize = 32;
        var maxBubbleWidth = (int)(bounds.Width * 0.65);

        if (item.IsSent)
        {
            // Sent message - right aligned, no avatar
            var bubbleColor = Color.FromArgb(0, 175, 240); // Skype blue for sent
            var textColor = Color.White;

            // Measure content
            using var font = new Font("Segoe UI", 10);
            var textSize = g.MeasureString(item.Content, font, maxBubbleWidth - 24);
            var bubbleWidth = (int)textSize.Width + 24;
            var bubbleHeight = Math.Max(40, (int)textSize.Height + 24);

            var bubbleX = bounds.Right - margin - bubbleWidth;
            var bubbleY = bounds.Y + (bounds.Height - bubbleHeight) / 2;
            var bubbleRect = new Rectangle(bubbleX, bubbleY, bubbleWidth, bubbleHeight);

            // Draw bubble
            using (var bubbleBrush = new SolidBrush(bubbleColor))
            {
                var path = CreateRoundedRectangle(bubbleRect, 16);
                g.FillPath(bubbleBrush, path);
            }

            // Draw content
            using (var textBrush = new SolidBrush(textColor))
            {
                var contentRect = new Rectangle(bubbleRect.X + 12, bubbleRect.Y + 8, bubbleRect.Width - 24, bubbleRect.Height - 24);
                g.DrawString(item.Content, font, textBrush, contentRect);
            }

            // Draw time below bubble
            using var timeFont = new Font("Segoe UI", 8);
            using var timeBrush = new SolidBrush(Color.FromArgb(138, 138, 138));
            var timeText = item.FormattedTime + " " + item.DeliveryStatusIcon;
            var timeSize = g.MeasureString(timeText, timeFont);
            var timeX = bounds.Right - margin - timeSize.Width;
            var timeY = bubbleY + bubbleHeight + 2;
            if (timeY + timeSize.Height > bounds.Bottom - 2)
            {
                // Draw inside bubble if no space below
                timeX = bubbleRect.Right - 8 - timeSize.Width;
                timeY = bubbleRect.Bottom - 16;
                using var innerTimeBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
                g.DrawString(timeText, timeFont, innerTimeBrush, timeX, timeY);
            }
            else
            {
                g.DrawString(timeText, timeFont, timeBrush, timeX, timeY);
            }
        }
        else
        {
            // Received message - left aligned with avatar
            var bubbleColor = Color.White;
            var textColor = Color.FromArgb(36, 36, 36);

            // Avatar
            var avatarX = bounds.X + margin;
            var avatarY = bounds.Y + (bounds.Height - avatarSize) / 2;
            var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

            using (var avatarBrush = new SolidBrush(Color.FromArgb(0, 175, 240)))
            {
                g.FillEllipse(avatarBrush, avatarRect);
            }

            using (var initialsFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var initialsBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(item.SenderInitials ?? "?", initialsFont, initialsBrush, avatarRect, sf);
            }

            // Measure content
            using var font = new Font("Segoe UI", 10);
            var textSize = g.MeasureString(item.Content, font, maxBubbleWidth - 24);
            var bubbleWidth = (int)textSize.Width + 24;
            var bubbleHeight = Math.Max(40, (int)textSize.Height + 24);

            var bubbleX = avatarX + avatarSize + 8;
            var bubbleY = bounds.Y + (bounds.Height - bubbleHeight) / 2;
            var bubbleRect = new Rectangle(bubbleX, bubbleY, bubbleWidth, bubbleHeight);

            // Draw bubble
            using (var bubbleBrush = new SolidBrush(bubbleColor))
            using (var borderPen = new Pen(Color.FromArgb(220, 220, 220)))
            {
                var path = CreateRoundedRectangle(bubbleRect, 16);
                g.FillPath(bubbleBrush, path);
                g.DrawPath(borderPen, path);
            }

            // Draw content
            using (var textBrush = new SolidBrush(textColor))
            {
                var contentRect = new Rectangle(bubbleRect.X + 12, bubbleRect.Y + 8, bubbleRect.Width - 24, bubbleRect.Height - 24);
                g.DrawString(item.Content, font, textBrush, contentRect);
            }

            // Draw time below bubble
            using var timeFont = new Font("Segoe UI", 8);
            using var timeBrush = new SolidBrush(Color.FromArgb(138, 138, 138));
            var timeY = bubbleY + bubbleHeight + 2;
            if (timeY + 12 > bounds.Bottom - 2)
            {
                // Draw inside bubble
                var timeX = bubbleRect.Right - 8 - g.MeasureString(item.FormattedTime, timeFont).Width;
                timeY = bubbleRect.Bottom - 16;
                using var innerTimeBrush = new SolidBrush(Color.FromArgb(160, 160, 160));
                g.DrawString(item.FormattedTime, timeFont, innerTimeBrush, timeX, timeY);
            }
            else
            {
                g.DrawString(item.FormattedTime, timeFont, timeBrush, bubbleX, timeY);
            }
        }
    }

    private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var arc = new Rectangle(rect.Location, new Size(radius * 2, radius * 2));

        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - radius * 2;
        path.AddArc(arc, 270, 90);
        arc.Y = rect.Bottom - radius * 2;
        path.AddArc(arc, 0, 90);
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();

        return path;
    }

    private void SetupLayout()
    {
        var contentPanel = new Panel { Dock = DockStyle.Fill };
        contentPanel.Controls.Add(_gridControl);
        contentPanel.Controls.Add(_emptyStatePanel);

        this.Controls.Add(contentPanel);
        this.Controls.Add(_headerPanel);
    }

    public void SetViewModel(MessagesViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        RefreshData();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MessagesViewModel.Messages) ||
            e.PropertyName == nameof(MessagesViewModel.CurrentConversation) ||
            e.PropertyName == nameof(MessagesViewModel.HasConversation))
        {
            RefreshData();
        }
    }

    public void RefreshData()
    {
        if (_viewModel == null) return;

        var hasConversation = _viewModel.HasConversation;
        _headerPanel.Visible = hasConversation;
        _emptyStatePanel.Visible = !hasConversation;
        _gridControl.Visible = hasConversation;

        if (hasConversation)
        {
            // Update header
            var headerData = _viewModel.GetHeaderDisplayItem();
            if (headerData != null)
            {
                _headerName.Text = headerData.Name;
                _headerStatus.Text = headerData.StatusText;
                _headerInitials = headerData.Initials;
                _avatarPanel.Invalidate();
            }

            // Update messages
            var items = _viewModel.GetDisplayItems();
            _gridControl.DataSource = items;
            _gridControl.RefreshDataSource();

            // Scroll to bottom
            ScrollToBottom();
        }
    }

    public void ScrollToBottom()
    {
        if (_gridView.RowCount > 0)
        {
            _gridView.TopRowIndex = _gridView.RowCount - 1;
            _gridView.FocusedRowHandle = _gridView.RowCount - 1;
        }
    }
}
