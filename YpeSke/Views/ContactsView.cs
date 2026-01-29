using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using YpeSke.ViewModels;

namespace YpeSke.Views;

public class ContactsView : XtraUserControl
{
    private readonly GridControl _gridControl;
    private readonly GridView _gridView;
    private readonly TextEdit _searchBox;
    private ContactsViewModel? _viewModel;

    public ContactsView()
    {
        InitializeComponent();
        _gridControl = new GridControl { Dock = DockStyle.Fill };
        _gridView = new GridView();
        _searchBox = new TextEdit();

        SetupSearchBox();
        SetupGridControl();
        SetupLayout();
    }

    private void InitializeComponent()
    {
        this.BackColor = Color.White;
    }

    private void SetupSearchBox()
    {
        _searchBox.Dock = DockStyle.Top;
        _searchBox.Properties.NullValuePrompt = "Search contacts...";
        _searchBox.Properties.NullValuePromptShowForEmptyValue = true;
        _searchBox.Height = 40;
        _searchBox.EditValueChanged += SearchBox_EditValueChanged;
    }

    private void SetupGridControl()
    {
        _gridControl.MainView = _gridView;

        // Hide standard grid UI
        _gridView.OptionsView.ShowColumnHeaders = false;
        _gridView.OptionsView.ShowGroupPanel = false;
        _gridView.OptionsView.ShowIndicator = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _gridView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

        // Single column for custom drawing
        var col = _gridView.Columns.AddVisible("Name");
        col.OptionsColumn.ReadOnly = true;

        // Custom drawing events
        _gridView.CalcRowHeight += GridView_CalcRowHeight;
        _gridView.CustomDrawCell += GridView_CustomDrawCell;
        _gridView.RowCellStyle += GridView_RowCellStyle;

        // Handle selection
        _gridView.FocusedRowObjectChanged += GridView_FocusedRowObjectChanged;
    }

    private void GridView_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
    {
        e.RowHeight = 70;
    }

    private void GridView_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
    {
        if (e.RowHandle < 0) return;

        var item = _gridView.GetRow(e.RowHandle) as ConversationDisplayItem;
        if (item == null) return;

        if (item.IsSelected)
        {
            e.Appearance.BackColor = Color.FromArgb(229, 246, 253);
        }
        else
        {
            e.Appearance.BackColor = Color.White;
        }
    }

    private void GridView_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
    {
        if (e.RowHandle < 0) return;

        var item = _gridView.GetRow(e.RowHandle) as ConversationDisplayItem;
        if (item == null) return;

        e.Handled = true;
        var g = e.Graphics;
        var bounds = e.Bounds;

        // Background
        var bgColor = item.IsSelected ? Color.FromArgb(229, 246, 253) : Color.White;
        using (var brush = new SolidBrush(bgColor))
        {
            g.FillRectangle(brush, bounds);
        }

        // Avatar circle (left side)
        var avatarSize = 48;
        var avatarX = bounds.X + 12;
        var avatarY = bounds.Y + (bounds.Height - avatarSize) / 2;
        var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using (var avatarBrush = new SolidBrush(Color.FromArgb(0, 175, 240)))
        {
            g.FillEllipse(avatarBrush, avatarRect);
        }

        // Initials in avatar
        using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
        using (var textBrush = new SolidBrush(Color.White))
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(item.Initials, font, textBrush, avatarRect, sf);
        }

        // Status indicator
        var statusSize = 14;
        var statusX = avatarX + avatarSize - statusSize + 2;
        var statusY = avatarY + avatarSize - statusSize + 2;
        var statusRect = new Rectangle(statusX, statusY, statusSize, statusSize);

        var statusColor = item.StatusClass switch
        {
            "online" => Color.FromArgb(107, 183, 0),
            "away" => Color.FromArgb(255, 170, 68),
            "busy" => Color.FromArgb(197, 15, 31),
            _ => Color.FromArgb(138, 138, 138)
        };

        using (var statusBrush = new SolidBrush(statusColor))
        using (var borderPen = new Pen(Color.White, 2))
        {
            g.FillEllipse(statusBrush, statusRect);
            g.DrawEllipse(borderPen, statusRect);
        }

        // Text area starts after avatar
        var textX = avatarX + avatarSize + 12;
        var textWidth = bounds.Width - textX - 60;

        // Name
        using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
        using (var textBrush = new SolidBrush(Color.FromArgb(36, 36, 36)))
        {
            var nameRect = new Rectangle(textX, bounds.Y + 12, textWidth, 22);
            g.DrawString(item.Name, font, textBrush, nameRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
        }

        // Message preview
        using (var font = new Font("Segoe UI", 9))
        using (var textBrush = new SolidBrush(Color.FromArgb(97, 97, 97)))
        {
            var previewRect = new Rectangle(textX, bounds.Y + 38, textWidth, 20);
            g.DrawString(item.LastMessagePreview, font, textBrush, previewRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
        }

        // Time (right side)
        using (var font = new Font("Segoe UI", 9))
        using (var textBrush = new SolidBrush(Color.FromArgb(138, 138, 138)))
        {
            var timeRect = new Rectangle(bounds.Right - 70, bounds.Y + 12, 60, 20);
            var sf = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(item.LastMessageTime, font, textBrush, timeRect, sf);
        }

        // Unread badge
        if (item.HasUnread && item.UnreadCount > 0)
        {
            var badgeText = item.UnreadCount.ToString();
            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                var badgeSize = g.MeasureString(badgeText, font);
                var badgeWidth = Math.Max(20, (int)badgeSize.Width + 10);
                var badgeX = bounds.Right - 20 - badgeWidth / 2;
                var badgeY = bounds.Y + 40;
                var badgeRect = new Rectangle(badgeX, badgeY, badgeWidth, 18);

                using (var badgeBrush = new SolidBrush(Color.FromArgb(0, 175, 240)))
                {
                    g.FillRoundedRectangle(badgeBrush, badgeRect, 9);
                }

                using (var textBrush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(badgeText, font, textBrush, badgeRect, sf);
                }
            }
        }

        // Bottom border
        using (var pen = new Pen(Color.FromArgb(230, 230, 230)))
        {
            g.DrawLine(pen, bounds.X + 72, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
        }
    }

    private void SetupLayout()
    {
        var searchPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(8),
            BackColor = Color.White
        };
        searchPanel.Controls.Add(_searchBox);
        _searchBox.Dock = DockStyle.Fill;

        this.Controls.Add(_gridControl);
        this.Controls.Add(searchPanel);
    }

    public void SetViewModel(ContactsViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        RefreshData();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ContactsViewModel.Conversations))
        {
            RefreshData();
        }
    }

    public void RefreshData()
    {
        if (_viewModel == null) return;

        var items = _viewModel.GetDisplayItems();
        _gridControl.DataSource = items;
        _gridControl.RefreshDataSource();
    }

    private void SearchBox_EditValueChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SearchText = _searchBox.Text ?? string.Empty;
            RefreshData();
        }
    }

    private void GridView_FocusedRowObjectChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowObjectChangedEventArgs e)
    {
        if (_viewModel == null || e.Row is not ConversationDisplayItem item)
            return;

        _viewModel.SelectedConversation = item.Conversation;
        RefreshData();
    }
}

// Extension method for rounded rectangles
public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        var arc = new Rectangle(rect.Location, new Size(radius * 2, radius * 2));

        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - radius * 2;
        path.AddArc(arc, 270, 90);
        arc.Y = rect.Bottom - radius * 2;
        path.AddArc(arc, 0, 90);
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();

        g.FillPath(brush, path);
    }
}
