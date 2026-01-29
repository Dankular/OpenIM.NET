using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
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
    private readonly Panel _emptyStatePanel;
    private MessagesViewModel? _viewModel;

    public MessagesView()
    {
        InitializeComponent();
        _gridControl = new GridControl { Dock = DockStyle.Fill };
        _gridView = new GridView();
        _headerPanel = new Panel();
        _headerName = new Label();
        _headerStatus = new Label();
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
        _headerPanel.Padding = new Padding(16, 12, 16, 12);
        _headerPanel.Visible = false;

        _headerName.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        _headerName.ForeColor = Color.FromArgb(36, 36, 36);
        _headerName.Location = new Point(70, 12);
        _headerName.AutoSize = true;

        _headerStatus.Font = new Font("Segoe UI", 10);
        _headerStatus.ForeColor = Color.FromArgb(97, 97, 97);
        _headerStatus.Location = new Point(70, 34);
        _headerStatus.AutoSize = true;

        // Avatar circle
        var avatarPanel = new Panel
        {
            Size = new Size(40, 40),
            Location = new Point(16, 12),
            BackColor = Color.FromArgb(0, 175, 240)
        };
        avatarPanel.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(0, 175, 240));
            e.Graphics.FillEllipse(brush, 0, 0, 39, 39);
        };

        _headerPanel.Controls.Add(_headerName);
        _headerPanel.Controls.Add(_headerStatus);
        _headerPanel.Controls.Add(avatarPanel);
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
        _gridView.OptionsView.ShowColumnHeaders = false;
        _gridView.OptionsView.ShowGroupPanel = false;
        _gridView.OptionsView.ShowIndicator = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedRow = false;
        _gridView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

        // Add columns
        var contentCol = _gridView.Columns.AddVisible("Content");
        contentCol.OptionsColumn.ReadOnly = true;
        contentCol.Width = 400;

        var timeCol = _gridView.Columns.AddVisible("FormattedTime");
        timeCol.OptionsColumn.ReadOnly = true;
        timeCol.Width = 80;

        var senderCol = _gridView.Columns.AddVisible("SenderInitials");
        senderCol.OptionsColumn.ReadOnly = true;
        senderCol.Width = 50;

        var isSentCol = _gridView.Columns.AddVisible("IsSent");
        isSentCol.Visible = false;

        // Custom row appearance
        _gridView.RowCellStyle += GridView_RowCellStyle;
        _gridView.CalcRowHeight += GridView_CalcRowHeight;
        _gridView.CustomDrawCell += GridView_CustomDrawCell;
    }

    private void GridView_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
    {
        if (e.RowHandle < 0) return;

        var item = _gridView.GetRow(e.RowHandle) as MessageDisplayItem;
        if (item == null) return;

        if (item.IsSent)
        {
            e.Appearance.BackColor = Color.FromArgb(229, 246, 253);
        }
        else
        {
            e.Appearance.BackColor = Color.White;
        }
    }

    private void GridView_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
    {
        e.RowHeight = 60;
    }

    private void GridView_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
    {
        if (e.RowHandle < 0) return;

        var item = _gridView.GetRow(e.RowHandle) as MessageDisplayItem;
        if (item == null) return;

        e.Handled = true;
        var g = e.Graphics;
        var bounds = e.Bounds;

        // Background
        var bgColor = item.IsSent ? Color.FromArgb(229, 246, 253) : Color.White;
        using (var brush = new SolidBrush(bgColor))
        {
            g.FillRectangle(brush, bounds);
        }

        // Message bubble
        var bubbleMargin = item.IsSent ? bounds.Width / 3 : 50;
        var bubbleWidth = bounds.Width - bubbleMargin - 20;
        var bubbleX = item.IsSent ? bounds.X + bubbleMargin : bounds.X + 50;
        var bubbleRect = new Rectangle(bubbleX, bounds.Y + 5, bubbleWidth, bounds.Height - 10);

        using (var bubbleBrush = new SolidBrush(item.IsSent ? Color.FromArgb(229, 246, 253) : Color.White))
        using (var borderPen = new Pen(Color.FromArgb(224, 224, 224)))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var path = CreateRoundedRectangle(bubbleRect, 12);
            g.FillPath(bubbleBrush, path);
            if (!item.IsSent)
            {
                g.DrawPath(borderPen, path);
            }
        }

        // Avatar for received messages
        if (!item.IsSent)
        {
            var avatarRect = new Rectangle(bounds.X + 8, bounds.Y + 10, 32, 32);
            using (var avatarBrush = new SolidBrush(Color.FromArgb(0, 175, 240)))
            {
                g.FillEllipse(avatarBrush, avatarRect);
            }

            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                var initials = item.SenderInitials ?? "?";
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(initials, font, textBrush, avatarRect, sf);
            }
        }

        // Content
        var contentRect = new Rectangle(bubbleRect.X + 10, bubbleRect.Y + 8, bubbleRect.Width - 20, bubbleRect.Height - 30);
        using (var font = new Font("Segoe UI", 10))
        using (var textBrush = new SolidBrush(Color.FromArgb(36, 36, 36)))
        {
            g.DrawString(item.Content, font, textBrush, contentRect);
        }

        // Time
        var timeRect = new Rectangle(bubbleRect.Right - 60, bubbleRect.Bottom - 20, 50, 16);
        using (var font = new Font("Segoe UI", 8))
        using (var textBrush = new SolidBrush(Color.FromArgb(138, 138, 138)))
        {
            var sf = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(item.FormattedTime, font, textBrush, timeRect, sf);
        }

        // Delivery status for sent messages
        if (item.IsSent && !string.IsNullOrEmpty(item.DeliveryStatusIcon))
        {
            var statusRect = new Rectangle(bubbleRect.Right - 20, bubbleRect.Bottom - 20, 16, 16);
            using (var font = new Font("Segoe UI", 8))
            using (var textBrush = new SolidBrush(item.DeliveryStatusClass == "read" ? Color.FromArgb(0, 175, 240) : Color.FromArgb(138, 138, 138)))
            {
                g.DrawString(item.DeliveryStatusIcon, font, textBrush, statusRect);
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
