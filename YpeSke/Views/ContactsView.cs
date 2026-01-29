using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Tile;
using DevExpress.XtraGrid.Views.Tile.ViewInfo;
using YpeSke.ViewModels;

namespace YpeSke.Views;

public class ContactsView : XtraUserControl
{
    private readonly GridControl _gridControl;
    private readonly TileView _tileView;
    private readonly TextEdit _searchBox;
    private ContactsViewModel? _viewModel;

    public ContactsView()
    {
        InitializeComponent();
        _gridControl = new GridControl { Dock = DockStyle.Fill };
        _tileView = new TileView();
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
        _gridControl.MainView = _tileView;

        // Configure TileView for contact list display
        _tileView.OptionsTiles.RowCount = 0;
        _tileView.OptionsTiles.Orientation = System.Windows.Forms.Orientation.Vertical;
        _tileView.OptionsTiles.ItemSize = new Size(280, 70);
        _tileView.Appearance.ItemNormal.BackColor = Color.White;
        _tileView.Appearance.ItemHovered.BackColor = Color.FromArgb(232, 232, 232);
        _tileView.Appearance.ItemFocused.BackColor = Color.FromArgb(229, 246, 253);
        _tileView.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

        // Add columns for data binding
        _tileView.Columns.AddVisible("Name");
        _tileView.Columns.AddVisible("Initials");
        _tileView.Columns.AddVisible("LastMessagePreview");
        _tileView.Columns.AddVisible("LastMessageTime");
        _tileView.Columns.AddVisible("StatusClass");

        // Configure tile template
        var group = new TileViewItemElement
        {
            Column = _tileView.Columns["Initials"],
            TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter,
            ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter,
            Width = 48,
            Height = 48,
            AnchorElement = null,
            AnchorIndent = 10,
            Appearance = { Normal = { BackColor = Color.FromArgb(0, 175, 240), ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold) } }
        };

        var nameElement = new TileViewItemElement
        {
            Column = _tileView.Columns["Name"],
            TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.TopLeft,
            Appearance = { Normal = { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(36, 36, 36) } }
        };

        var previewElement = new TileViewItemElement
        {
            Column = _tileView.Columns["LastMessagePreview"],
            TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.BottomLeft,
            Appearance = { Normal = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(97, 97, 97) } }
        };

        var timeElement = new TileViewItemElement
        {
            Column = _tileView.Columns["LastMessageTime"],
            TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.TopRight,
            Appearance = { Normal = { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(138, 138, 138) } }
        };

        _tileView.TileTemplate.Add(group);
        _tileView.TileTemplate.Add(nameElement);
        _tileView.TileTemplate.Add(previewElement);
        _tileView.TileTemplate.Add(timeElement);

        // Handle item click
        _tileView.FocusedRowObjectChanged += TileView_FocusedRowObjectChanged;
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

    private void TileView_FocusedRowObjectChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowObjectChangedEventArgs e)
    {
        if (_viewModel == null || e.Row is not ConversationDisplayItem item)
            return;

        _viewModel.SelectedConversation = item.Conversation;
        RefreshData();
    }
}
