using System.Drawing;
using System.IO.Ports;
using System.Text;

namespace WindowsDnc;

public sealed class MainForm : Form
{
    private static readonly Color Navy = Color.FromArgb(23, 39, 61);
    private static readonly Color Blue = Color.FromArgb(32, 111, 219);
    private static readonly Color Surface = Color.White;
    private static readonly Color Background = Color.FromArgb(242, 245, 249);
    private static readonly Color Muted = Color.FromArgb(91, 105, 122);

    private readonly ComboBox profileBox = new();
    private readonly ComboBox portBox = new();
    private readonly Label profileSummary = new();
    private readonly RichTextBox programText = new();
    private readonly FlickerFreeTabControl documentTabs = new();
    private readonly Button newDocumentButton = new();
    private readonly TextBox filePathBox = new();
    private readonly ComboBox recentFilesBox = new();
    private readonly Button browseButton = new();
    private readonly Button sendButton = new();
    private readonly Button partialSendButton = new();
    private readonly Button receiveButton = new();
    private readonly Button stopButton = new();
    private readonly Button saveButton = new();
    private readonly Button refreshPortsButton = new();
    private readonly Button editProfileButton = new();
    private readonly Button addProfileButton = new();
    private readonly Button deleteProfileButton = new();
    private readonly TransferProgressPanel transferPanel = new();
    private readonly Label statusLabel = new();
    private readonly Label characterCountLabel = new();
    private readonly Label estimatedSendLabel = new();
    private readonly Label transferDetailsLabel = new();
    private readonly FlowLayoutPanel transferStatsPanel = new();
    private readonly Label percentStatLabel = new();
    private readonly Label linesStatLabel = new();
    private readonly Label bytesStatLabel = new();
    private readonly Label rateStatLabel = new();
    private readonly Label timeStatLabel = new();
    private readonly Label errorsStatLabel = new();
    private readonly Label activeProfileLabel = new();
    private readonly Button debugButton = new();
    private readonly Button logButton = new();
    private readonly Button soundButton = new();
    private readonly Label transferStateLabel = new();
    private readonly PictureBox logoBox = new();
    private readonly TextBox findBox = new();
    private readonly TextBox replaceBox = new();
    private readonly CheckBox matchCaseBox = new();
    private readonly CheckBox wholeWordBox = new();
    private readonly FlowLayoutPanel searchPanel = new();
    private readonly RowStyle searchRowStyle = new(SizeType.Absolute, 0);
    private readonly ComboBox quickNavigationBox = new();
    private readonly ListBox outlineList = new();
    private readonly ColumnStyle outlineColumnStyle = new(SizeType.Absolute, 300);
    private readonly CheckBox darkModeToggle = new();
    private readonly Button profileVisibilityButton = new();
    private readonly Button shareProfileButton = new();
    private readonly Button communityProfilesButton = new();
    private readonly Button aboutButton = new();
    private readonly Label editionStatusLabel = new();
    private readonly Panel tabStripPanel = new();
    private readonly FlowLayoutPanel serialSignalsPanel = new();
    private readonly Dictionary<string, Label> serialSignalLabels = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> serialSignalStates = new(StringComparer.Ordinal);
    private readonly System.Windows.Forms.Timer transferAnimationTimer = new() { Interval = 320 };
    private readonly System.Windows.Forms.Timer highlightTimer = new() { Interval = 180 };
    private readonly System.Windows.Forms.Timer outlineTimer = new() { Interval = 500 };
    private readonly System.Windows.Forms.Timer sendEstimateTimer = new() { Interval = 350 };
    private readonly System.Windows.Forms.Timer externalFileTimer = new() { Interval = 1500 };
    private readonly SerialDncService dncService = new();
    private readonly ProfileRepository profileRepository = new();
    private readonly AppSettingsRepository appSettingsRepository = new();
    private readonly TransferLogRepository transferLogRepository = new();

    private CancellationTokenSource? operationCancellation;
    private bool receivedContent;
    private bool darkMode;
    private bool applyingSyntaxHighlighting;
    private List<CncProfile> profiles = new();
    private HashSet<string> hiddenProfileIds = new(StringComparer.Ordinal);
    private List<string> recentFiles = new();
    private NcDocumentIndex documentIndex = NcDocumentIndex.Build(string.Empty);
    private SerialDebugWindow? debugWindow;
    private bool suppressRecentSelection;
    private bool replaceMode;
    private string activeTransferDirection = string.Empty;
    private string activeTransferProfile = string.Empty;
    private NcDocument? activeDocument;
    private bool switchingDocument;
    private bool alertsEnabled;
    private string completionSound = "Laundry Celebration";
    private string failureSound = "Laundry Waltz";
    private int alertVolume = 70;
    private int outlineWidth = 300;
    private int draggedTabIndex = -1;
    private int lastTransferLines;
    private long lastTransferBytes;
    private int lastTransferErrors;
    private double lastTransferRate;
    private TimeSpan lastTransferElapsed;
    private bool signalFlashOn = true;
    private bool updatingTransferLayout;
    private bool checkingExternalFiles;

    public MainForm()
    {
        AppSettings savedSettings = appSettingsRepository.Load();
        darkMode = savedSettings.DarkMode;
        hiddenProfileIds = savedSettings.HiddenProfileIds?.ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        recentFiles = savedSettings.RecentFiles?
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList() ?? new List<string>();
        alertsEnabled = savedSettings.AlertsEnabled;
        // Move installations that still have the former factory choices to the
        // new defaults while preserving any other sounds the user selected.
        completionSound = savedSettings.CompletionSound == "Shop Bell"
            ? "Laundry Celebration"
            : savedSettings.CompletionSound;
        failureSound = savedSettings.FailureSound == "Robot"
            ? "Laundry Waltz"
            : savedSettings.FailureSound;
        alertVolume = Math.Clamp(savedSettings.AlertVolume, 0, 100);
        outlineWidth = Math.Clamp(savedSettings.OutlineWidth, 220, 520);
        outlineColumnStyle.Width = outlineWidth;
        Text = "C2C DNC Open Preview";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 720);
        Size = new Size(1120, 790);
        BackColor = Background;
        ForeColor = Navy;
        Font = new Font("Segoe UI", 9F);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        BuildInterface();
        UpdateEditionDisplay();
        Shown += (_, _) =>
        {
            UpdateTransferPanelLayout();
            PositionTabButtons();
        };
        RefreshRecentFiles();
        LoadProfiles();
        RefreshPorts();
        SetBusy(false);
        highlightTimer.Tick += (_, _) =>
        {
            highlightTimer.Stop();
            ApplySyntaxHighlighting();
        };
        outlineTimer.Tick += (_, _) =>
        {
            outlineTimer.Stop();
            RebuildDocumentIndex();
        };
        sendEstimateTimer.Tick += (_, _) =>
        {
            sendEstimateTimer.Stop();
            UpdatePreSendEstimate();
        };
        externalFileTimer.Tick += (_, _) => CheckForExternalFileChanges();
        externalFileTimer.Start();
        transferAnimationTimer.Tick += (_, _) =>
        {
            signalFlashOn = !signalFlashOn;
            transferPanel.AdvanceMarquee();
            RefreshSerialSignalColors();
        };
        darkModeToggle.Checked = darkMode;
        ApplyTheme();
        WarmUpSyntaxEditor();
        CreateNewDocument(false);
        SetTransferState("READY", Color.FromArgb(42, 91, 132));
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        SaveActiveDocumentState();
        if (!ConfirmCloseDocuments(documentTabs.TabPages.Cast<TabPage>()
                .Select(page => page.Tag)
                .OfType<NcDocument>()))
        {
            e.Cancel = true;
            return;
        }
        operationCancellation?.Cancel();
        highlightTimer.Stop();
        outlineTimer.Stop();
        sendEstimateTimer.Stop();
        externalFileTimer.Stop();
        debugWindow?.ClosePermanently();
        base.OnFormClosing(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.N))
        {
            CreateNewDocument();
            return true;
        }

        if (keyData == (Keys.Control | Keys.W))
        {
            CloseActiveDocument();
            return true;
        }

        if (keyData == (Keys.Control | Keys.Home))
        {
            GoToProgramTop();
            return true;
        }

        if (keyData == (Keys.Control | Keys.O) && browseButton.Enabled)
        {
            OpenProgramFile();
            return true;
        }

        if (keyData == (Keys.Control | Keys.S) && sendButton.Enabled)
        {
            _ = SendProgramAsync();
            return true;
        }

        if (keyData == (Keys.Control | Keys.F))
        {
            ShowSearch(false);
            return true;
        }

        if (keyData == (Keys.Control | Keys.H))
        {
            ShowSearch(true);
            return true;
        }

        if (keyData == Keys.F3)
        {
            FindNext(true);
            return true;
        }

        if (keyData == (Keys.Shift | Keys.F3))
        {
            FindNext(false);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(22),
            BackColor = Background
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 178));
        Controls.Add(root);

        var headingPanel = new Panel { Dock = DockStyle.Fill };
        logoBox.Location = new Point(0, 2);
        logoBox.Size = new Size(48, 48);
        logoBox.SizeMode = PictureBoxSizeMode.Zoom;
        headingPanel.Controls.Add(logoBox);
        headingPanel.Controls.Add(new Label
        {
            Text = "C2C DNC",
            Font = new Font("Segoe UI Semibold", 20F),
            ForeColor = Navy,
            AutoSize = true,
            Location = new Point(58, 2)
        });
        headingPanel.Controls.Add(new Label
        {
            Text = "RS-232 program sender / receiver",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Muted,
            Tag = "muted",
            AutoSize = true,
            Location = new Point(60, 38)
        });
        editionStatusLabel.Location = new Point(60, 55);
        editionStatusLabel.Size = new Size(650, 18);
        editionStatusLabel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
        editionStatusLabel.AutoEllipsis = true;
        editionStatusLabel.Font = new Font("Segoe UI Semibold", 8.5F);
        editionStatusLabel.Tag = "muted";
        headingPanel.Controls.Add(editionStatusLabel);
        var themePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 365,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4, 0, 0, 0)
        };
        ConfigureGroupedButton(profileVisibilityButton, "Shown profiles…", "profile");
        profileVisibilityButton.AutoSize = true;
        profileVisibilityButton.Height = 30;
        profileVisibilityButton.Click += (_, _) => ManageProfileVisibility();
        ConfigureGroupedButton(aboutButton, "About…", "utility");
        aboutButton.AutoSize = true;
        aboutButton.Height = 30;
        aboutButton.Click += (_, _) => ShowAboutDialog();
        darkModeToggle.Text = "Dark mode";
        darkModeToggle.AutoSize = true;
        darkModeToggle.Margin = new Padding(10, 7, 0, 0);
        darkModeToggle.CheckedChanged += (_, _) => ToggleDarkMode(darkModeToggle.Checked);
        themePanel.Controls.Add(aboutButton);
        themePanel.Controls.Add(profileVisibilityButton);
        themePanel.Controls.Add(darkModeToggle);
        ConfigureGroupedButton(shareProfileButton, "Share", "profile");
        shareProfileButton.AutoSize = true;
        shareProfileButton.Height = 28;
        shareProfileButton.Click += (_, _) => ShareSelectedProfile();
        ConfigureGroupedButton(communityProfilesButton, "Community configurations…", "profile");
        communityProfilesButton.AutoSize = true;
        communityProfilesButton.Height = 28;
        communityProfilesButton.Click += (_, _) => BrowseCommunityProfiles();
        themePanel.SetFlowBreak(darkModeToggle, true);
        themePanel.Controls.Add(shareProfileButton);
        themePanel.Controls.Add(communityProfilesButton);
        headingPanel.Controls.Add(themePanel);
        root.Controls.Add(headingPanel, 0, 0);

        var profileCard = CreateCard();
        root.Controls.Add(profileCard, 0, 1);

        profileBox.DropDownStyle = ComboBoxStyle.DropDownList;
        profileBox.DropDownWidth = 440;
        profileBox.Font = new Font("Segoe UI Semibold", 10F);
        profileBox.Location = new Point(18, 31);
        profileBox.Width = 310;
        profileBox.SelectedIndexChanged += (_, _) => ProfileChanged();
        profileCard.Controls.Add(FieldLabel("CNC PROFILE", 18, 10));
        profileCard.Controls.Add(profileBox);

        portBox.DropDownStyle = ComboBoxStyle.DropDown;
        portBox.Font = new Font("Segoe UI Semibold", 10F);
        portBox.Location = new Point(346, 31);
        portBox.Width = 112;
        profileCard.Controls.Add(FieldLabel("COM PORT", 346, 10));
        profileCard.Controls.Add(portBox);

        ConfigureGroupedButton(refreshPortsButton, "Refresh", "profile");
        refreshPortsButton.Location = new Point(470, 30);
        refreshPortsButton.Size = new Size(88, 31);
        refreshPortsButton.Click += (_, _) => RefreshPorts();
        profileCard.Controls.Add(refreshPortsButton);

        ConfigureGroupedButton(editProfileButton, "Edit profile", "profile");
        editProfileButton.Location = new Point(576, 30);
        editProfileButton.Size = new Size(100, 31);
        editProfileButton.Click += (_, _) => EditSelectedProfile();
        profileCard.Controls.Add(editProfileButton);

        ConfigureGroupedButton(addProfileButton, "Add profile", "profile");
        addProfileButton.Location = new Point(686, 30);
        addProfileButton.Size = new Size(96, 31);
        addProfileButton.Click += (_, _) => AddProfile();
        profileCard.Controls.Add(addProfileButton);

        ConfigureGroupedButton(deleteProfileButton, "Delete", "danger");
        deleteProfileButton.Location = new Point(792, 30);
        deleteProfileButton.Size = new Size(78, 31);
        deleteProfileButton.Click += (_, _) => DeleteSelectedProfile();
        profileCard.Controls.Add(deleteProfileButton);

        profileSummary.Visible = false;

        var editorCard = CreateCard();
        editorCard.Padding = new Padding(18);
        root.Controls.Add(editorCard, 0, 2);

        var editorLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 39));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        editorLayout.RowStyles.Add(searchRowStyle);
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        editorCard.Controls.Add(editorLayout);

        tabStripPanel.Dock = DockStyle.Fill;
        tabStripPanel.Margin = new Padding(0);
        documentTabs.Location = new Point(0, 0);
        documentTabs.Height = 33;
        // Reserve enough header space for the filename and its per-tab close mark.
        documentTabs.Padding = new Point(28, 3);
        documentTabs.ShowToolTips = true;
        documentTabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        documentTabs.ItemSize = new Size(150, 27);
        documentTabs.DrawItem += DrawDocumentTab;
        documentTabs.AllowDrop = true;
        documentTabs.SelectedIndexChanged += (_, _) =>
        {
            SwitchToSelectedDocument();
            PositionTabButtons();
        };
        documentTabs.MouseDown += HandleTabMouseDown;
        documentTabs.MouseMove += (_, eventArgs) =>
        {
            if (eventArgs.Button == MouseButtons.Left && draggedTabIndex >= 0)
            {
                documentTabs.DoDragDrop(documentTabs.TabPages[draggedTabIndex], DragDropEffects.Move);
            }
        };
        documentTabs.MouseUp += (_, _) => draggedTabIndex = -1;
        documentTabs.DragOver += (_, eventArgs) => eventArgs.Effect = DragDropEffects.Move;
        documentTabs.DragDrop += (_, eventArgs) => ReorderDraggedTab(eventArgs);
        var tabMenu = new ContextMenuStrip();
        tabMenu.Items.Add("Close tab", null, (_, _) => CloseActiveDocument());
        tabMenu.Items.Add("Close all", null, (_, _) => CloseAllDocuments());
        documentTabs.ContextMenuStrip = tabMenu;
        ConfigureGroupedButton(newDocumentButton, "+", "file");
        newDocumentButton.Font = new Font("Segoe UI Semibold", 13F);
        newDocumentButton.AccessibleName = "New program tab";
        newDocumentButton.Size = new Size(34, 29);
        newDocumentButton.Click += (_, _) => CreateNewDocument();
        tabStripPanel.Controls.Add(documentTabs);
        tabStripPanel.Controls.Add(newDocumentButton);
        tabStripPanel.Resize += (_, _) => PositionTabButtons();
        editorLayout.Controls.Add(tabStripPanel, 0, 0);

        var fileRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0)
        };
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 102));
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        fileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filePathBox.Dock = DockStyle.Fill;
        filePathBox.PlaceholderText = "Load an NC program, or paste one below";
        filePathBox.ReadOnly = true;
        filePathBox.BackColor = Color.White;
        ConfigureGroupedButton(browseButton, "Open file", "file");
        browseButton.Dock = DockStyle.Left;
        browseButton.Width = 92;
        browseButton.Click += (_, _) => OpenProgramFile();
        recentFilesBox.Dock = DockStyle.Fill;
        recentFilesBox.DropDownStyle = ComboBoxStyle.DropDownList;
        recentFilesBox.DropDownWidth = 430;
        recentFilesBox.SelectedIndexChanged += (_, _) => OpenSelectedRecentFile();
        fileRow.Controls.Add(browseButton, 0, 0);
        fileRow.Controls.Add(recentFilesBox, 1, 0);
        fileRow.Controls.Add(filePathBox, 2, 0);
        editorLayout.Controls.Add(fileRow, 0, 1);

        var navigationRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 2, 0, 0)
        };
        Button findButton = SmallButton("Find");
        ApplyButtonGroup(findButton, "search");
        findButton.Click += (_, _) => ShowSearch(false);
        Button replaceButton = SmallButton("Replace");
        ApplyButtonGroup(replaceButton, "search");
        replaceButton.Click += (_, _) => ShowSearch(true);
        Button previousToolButton = SmallButton("◀ Tool");
        ApplyButtonGroup(previousToolButton, "navigation");
        previousToolButton.Click += (_, _) => Navigate(NcNavigationKind.ToolChange, false);
        Button nextToolButton = SmallButton("Tool ▶");
        ApplyButtonGroup(nextToolButton, "navigation");
        nextToolButton.Click += (_, _) => Navigate(NcNavigationKind.ToolChange, true);
        Button previousOperationButton = SmallButton("◀ Operation");
        ApplyButtonGroup(previousOperationButton, "navigation");
        previousOperationButton.Click += (_, _) => Navigate(NcNavigationKind.Operation, false);
        Button nextOperationButton = SmallButton("Operation ▶");
        ApplyButtonGroup(nextOperationButton, "navigation");
        nextOperationButton.Click += (_, _) => Navigate(NcNavigationKind.Operation, true);
        Button topButton = SmallButton("⇈ Top");
        ApplyButtonGroup(topButton, "navigation");
        topButton.Click += (_, _) => GoToProgramTop();
        quickNavigationBox.DropDownStyle = ComboBoxStyle.DropDownList;
        quickNavigationBox.Width = 135;
        quickNavigationBox.Items.AddRange(new object[]
        {
            "Next operation", "M00 / M01", "Program end", "Spindle", "Coolant"
        });
        quickNavigationBox.SelectedIndex = 0;
        Button quickNextButton = SmallButton("Next");
        ApplyButtonGroup(quickNextButton, "navigation");
        quickNextButton.Click += (_, _) => Navigate(SelectedQuickNavigationKind(), true);
        navigationRow.Controls.AddRange(new Control[]
        {
            findButton, replaceButton, previousToolButton, nextToolButton,
            previousOperationButton, nextOperationButton, topButton, quickNavigationBox, quickNextButton
        });
        editorLayout.Controls.Add(navigationRow, 0, 2);

        searchPanel.Dock = DockStyle.Fill;
        searchPanel.FlowDirection = FlowDirection.LeftToRight;
        searchPanel.WrapContents = false;
        searchPanel.AutoScroll = true;
        searchPanel.Padding = new Padding(0, 3, 0, 0);
        findBox.Width = 170;
        findBox.PlaceholderText = "Find";
        findBox.KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.KeyCode == Keys.Enter)
            {
                FindNext(!eventArgs.Shift);
                eventArgs.SuppressKeyPress = true;
            }
            else if (eventArgs.KeyCode == Keys.Escape)
            {
                HideSearch();
            }
        };
        replaceBox.Width = 170;
        replaceBox.PlaceholderText = "Replace with";
        Button previousFindButton = SmallButton("Previous");
        ApplyButtonGroup(previousFindButton, "search");
        previousFindButton.Click += (_, _) => FindNext(false);
        Button nextFindButton = SmallButton("Next");
        ApplyButtonGroup(nextFindButton, "search");
        nextFindButton.Click += (_, _) => FindNext(true);
        Button replaceOneButton = SmallButton("Replace");
        ApplyButtonGroup(replaceOneButton, "search");
        replaceOneButton.Name = "ReplaceOne";
        replaceOneButton.Click += (_, _) => ReplaceOne();
        Button replaceAllButton = SmallButton("Replace all");
        ApplyButtonGroup(replaceAllButton, "search");
        replaceAllButton.Name = "ReplaceAll";
        replaceAllButton.Click += (_, _) => ReplaceAll();
        matchCaseBox.Text = "Case";
        matchCaseBox.AutoSize = true;
        matchCaseBox.Margin = new Padding(8, 6, 0, 0);
        wholeWordBox.Text = "Whole word";
        wholeWordBox.AutoSize = true;
        wholeWordBox.Margin = new Padding(8, 6, 0, 0);
        Button closeSearchButton = SmallButton("×");
        ApplyButtonGroup(closeSearchButton, "search");
        closeSearchButton.Click += (_, _) => HideSearch();
        searchPanel.Controls.AddRange(new Control[]
        {
            findBox, replaceBox, previousFindButton, nextFindButton,
            replaceOneButton, replaceAllButton, matchCaseBox, wholeWordBox, closeSearchButton
        });
        editorLayout.Controls.Add(searchPanel, 0, 3);

        programText.Dock = DockStyle.Fill;
        programText.Font = new Font("Cascadia Mono", 10F);
        programText.BorderStyle = BorderStyle.FixedSingle;
        programText.AcceptsTab = true;
        programText.WordWrap = false;
        programText.DetectUrls = false;
        programText.TextChanged += (_, _) =>
        {
            UpdateDocumentSizeLabel();
            SchedulePreSendEstimate();
            ScheduleSyntaxHighlighting();
            ScheduleOutlineRebuild();
            if (!switchingDocument && !applyingSyntaxHighlighting && activeDocument is not null)
            {
                activeDocument.Text = programText.Text;
                activeDocument.IsDirty = true;
                activeDocument.IsReceived = receivedContent;
                UpdateActiveTabTitle();
            }
        };
        programText.VScroll += (_, _) => ScheduleSyntaxHighlighting();
        programText.Resize += (_, _) => ScheduleSyntaxHighlighting();

        var editorSplit = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        editorSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        editorSplit.ColumnStyles.Add(outlineColumnStyle);
        editorSplit.Controls.Add(programText, 0, 0);
        var outlinePanel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 0, 0, 0)
        };
        var outlineHeaderRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 34,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0, 0, 4, 0)
        };
        outlineHeaderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        outlineHeaderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        outlineHeaderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        outlineHeaderRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        outlineHeaderRow.Controls.Add(new Label
        {
            Text = "NC OUTLINE",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI Semibold", 8F),
            Tag = "muted"
        }, 0, 0);
        Button narrowerOutlineButton = SmallButton("− Width");
        ApplyButtonGroup(narrowerOutlineButton, "navigation");
        narrowerOutlineButton.AutoSize = false;
        narrowerOutlineButton.Dock = DockStyle.Fill;
        narrowerOutlineButton.Margin = new Padding(3, 2, 3, 2);
        narrowerOutlineButton.Click += (_, _) => ResizeOutline(-40);
        Button widerOutlineButton = SmallButton("Width +");
        ApplyButtonGroup(widerOutlineButton, "navigation");
        widerOutlineButton.AutoSize = false;
        widerOutlineButton.Dock = DockStyle.Fill;
        widerOutlineButton.Margin = new Padding(3, 2, 3, 2);
        widerOutlineButton.Click += (_, _) => ResizeOutline(40);
        outlineHeaderRow.Controls.Add(narrowerOutlineButton, 1, 0);
        outlineHeaderRow.Controls.Add(widerOutlineButton, 2, 0);
        outlineList.Dock = DockStyle.Fill;
        outlineList.Font = new Font("Cascadia Mono", 8.5F);
        outlineList.HorizontalScrollbar = true;
        outlineList.DoubleClick += (_, _) => JumpToOutlineSelection();
        outlinePanel.Controls.Add(outlineList);
        outlinePanel.Controls.Add(outlineHeaderRow);
        outlineList.BringToFront();
        editorSplit.Controls.Add(outlinePanel, 1, 0);
        editorLayout.Controls.Add(editorSplit, 0, 4);

        var editorFooter = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        editorFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        editorFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 235));
        editorFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));

        statusLabel.Text = "Ready";
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Margin = new Padding(0);
        statusLabel.AutoSize = false;
        statusLabel.AutoEllipsis = false;
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        statusLabel.Font = new Font("Segoe UI Semibold", 9.5F);
        statusLabel.ForeColor = Muted;
        statusLabel.Tag = "muted";
        editorFooter.Controls.Add(statusLabel, 0, 0);

        estimatedSendLabel.Text = "Estimated send: —";
        estimatedSendLabel.Dock = DockStyle.Fill;
        estimatedSendLabel.AutoSize = false;
        estimatedSendLabel.TextAlign = ContentAlignment.MiddleRight;
        estimatedSendLabel.Font = new Font("Segoe UI Semibold", 9F);
        estimatedSendLabel.ForeColor = Muted;
        estimatedSendLabel.Tag = "muted";
        editorFooter.Controls.Add(estimatedSendLabel, 1, 0);

        characterCountLabel.Text = "0 characters  •  0 bytes";
        characterCountLabel.Dock = DockStyle.Fill;
        characterCountLabel.AutoSize = false;
        characterCountLabel.TextAlign = ContentAlignment.MiddleRight;
        characterCountLabel.ForeColor = Muted;
        characterCountLabel.Tag = "muted";
        editorFooter.Controls.Add(characterCountLabel, 2, 0);
        editorLayout.Controls.Add(editorFooter, 0, 5);

        transferPanel.Dock = DockStyle.Fill;
        transferPanel.Margin = new Padding(0, 4, 0, 8);
        transferPanel.BackColor = Surface;
        transferPanel.BorderStyle = BorderStyle.FixedSingle;
        transferPanel.Tag = "card";
        transferPanel.ProgressAreaHeight = 64;
        root.Controls.Add(transferPanel, 0, 3);

        ConfigureGroupedButton(sendButton, "Send program", "send");
        sendButton.Location = new Point(18, 17);
        sendButton.Size = new Size(132, 36);
        sendButton.Click += async (_, _) => await SendProgramAsync();
        transferPanel.Controls.Add(sendButton);

        ConfigureGroupedButton(partialSendButton, "▼", "send");
        partialSendButton.Location = new Point(150, 17);
        partialSendButton.Size = new Size(28, 36);
        partialSendButton.AccessibleName = "Partial send options";
        var partialMenu = new ContextMenuStrip();
        partialMenu.Items.Add("Send from current line…", null, async (_, _) => await SendProgramAsync(SendStartMode.CursorLine));
        partialMenu.Items.Add("Send from preceding tool change…", null, async (_, _) => await SendProgramAsync(SendStartMode.PreviousToolChange));
        partialSendButton.Click += (_, _) => partialMenu.Show(partialSendButton, new Point(0, partialSendButton.Height));
        transferPanel.Controls.Add(partialSendButton);

        ConfigureGroupedButton(receiveButton, "Receive", "receive");
        receiveButton.Location = new Point(188, 17);
        receiveButton.Size = new Size(112, 36);
        receiveButton.Click += async (_, _) => await ReceiveProgramAsync();
        transferPanel.Controls.Add(receiveButton);

        ConfigureGroupedButton(stopButton, "Stop", "danger");
        stopButton.Location = new Point(310, 17);
        stopButton.Size = new Size(82, 36);
        stopButton.Click += (_, _) => operationCancellation?.Cancel();
        transferPanel.Controls.Add(stopButton);

        ConfigureGroupedButton(saveButton, "Save as…", "file");
        saveButton.Location = new Point(402, 17);
        saveButton.Size = new Size(96, 36);
        saveButton.Click += (_, _) => SaveProgramFile();
        transferPanel.Controls.Add(saveButton);

        ConfigureGroupedButton(debugButton, "Debug", "utility");
        debugButton.Location = new Point(508, 17);
        debugButton.Size = new Size(78, 36);
        debugButton.Click += (_, _) => ShowDebugWindow();
        transferPanel.Controls.Add(debugButton);

        ConfigureGroupedButton(logButton, "Log", "utility");
        logButton.Location = new Point(596, 17);
        logButton.Size = new Size(70, 36);
        logButton.Click += (_, _) => ShowTransferLog();
        transferPanel.Controls.Add(logButton);

        ConfigureGroupedButton(soundButton, "Sounds…", "utility");
        soundButton.Location = new Point(676, 17);
        soundButton.Size = new Size(86, 36);
        soundButton.Click += (_, _) => ShowSoundSettings();
        transferPanel.Controls.Add(soundButton);

        activeProfileLabel.Location = new Point(18, 68);
        activeProfileLabel.AutoSize = true;
        activeProfileLabel.Font = new Font("Segoe UI Semibold", 9F);
        activeProfileLabel.Tag = "transparent";
        transferPanel.Controls.Add(activeProfileLabel);

        transferDetailsLabel.Location = new Point(330, 69);
        transferDetailsLabel.AutoSize = false;
        transferDetailsLabel.Size = new Size(390, 20);
        transferDetailsLabel.AutoEllipsis = true;
        transferDetailsLabel.Tag = "muted-transparent";
        transferDetailsLabel.ForeColor = Muted;
        transferDetailsLabel.BackColor = Color.Transparent;
        transferDetailsLabel.Visible = false;

        transferStatsPanel.Location = new Point(315, 68);
        transferStatsPanel.Size = new Size(650, 34);
        transferStatsPanel.WrapContents = false;
        transferStatsPanel.BackColor = Color.Transparent;
        transferStatsPanel.Tag = "transparent";
        ConfigureTransferStat(percentStatLabel, "0%", 70);
        ConfigureTransferStat(linesStatLabel, "Lines 0/0", 125);
        ConfigureTransferStat(bytesStatLabel, "Bytes 0/0", 155);
        ConfigureTransferStat(rateStatLabel, "0.0 chars/s", 120);
        ConfigureTransferStat(timeStatLabel, "Remaining --:--", 145);
        ConfigureTransferStat(errorsStatLabel, "Errors 0", 88);
        transferStatsPanel.Controls.AddRange(new Control[]
        {
            percentStatLabel, linesStatLabel, bytesStatLabel,
            rateStatLabel, timeStatLabel, errorsStatLabel
        });
        transferPanel.Controls.Add(transferStatsPanel);

        var transferDivider = new Panel
        {
            Location = new Point(0, 63),
            Height = 1,
            Width = transferPanel.ClientSize.Width,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            BackColor = Color.FromArgb(95, 115, 138),
            Tag = "transfer-divider"
        };
        transferPanel.Controls.Add(transferDivider);

        serialSignalsPanel.AutoSize = true;
        serialSignalsPanel.WrapContents = false;
        serialSignalsPanel.Location = new Point(92, 105);
        serialSignalsPanel.Padding = new Padding(0);
        serialSignalsPanel.BackColor = Color.Transparent;
        serialSignalsPanel.Tag = "transparent";
        foreach (string signal in new[] { "RX", "TX", "RTS", "CTS", "DTR", "DSR", "ERR" })
        {
            var label = new Label
            {
                Text = $"● {signal}",
                AutoSize = true,
                Margin = new Padding(5, 2, 8, 2),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 9F),
                Tag = "transparent"
            };
            serialSignalLabels[signal] = label;
            serialSignalStates[signal] = false;
            serialSignalsPanel.Controls.Add(label);
        }
        transferPanel.Controls.Add(serialSignalsPanel);
        var serialStatusLabel = new Label
        {
            Text = "SERIAL STATUS",
            Location = new Point(18, 107),
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 7.5F),
            Tag = "muted-transparent",
            BackColor = Color.Transparent
        };
        transferPanel.Controls.Add(serialStatusLabel);

        transferStateLabel.Text = "READY";
        transferStateLabel.TextAlign = ContentAlignment.MiddleCenter;
        transferStateLabel.Font = new Font("Segoe UI Black", 22F);
        transferStateLabel.ForeColor = Color.White;
        transferStateLabel.Size = new Size(280, 66);
        transferStateLabel.Location = new Point(750, 82);
        transferStateLabel.BorderStyle = BorderStyle.FixedSingle;
        transferStateLabel.Tag = "transfer-state";
        transferPanel.Controls.Add(transferStateLabel);
        transferPanel.Resize += (_, _) => UpdateTransferPanelLayout();
        transferPanel.Layout += (_, _) => UpdateTransferPanelLayout();
    }

    private void UpdateTransferPanelLayout()
    {
        if (updatingTransferLayout || transferPanel.ClientSize.Width <= 0)
        {
            return;
        }

        updatingTransferLayout = true;
        try
        {
            transferStateLabel.Left = transferPanel.ClientSize.Width - transferStateLabel.Width - 18;
            int availableStatsWidth = transferStateLabel.Left - transferStatsPanel.Left - 18;
            transferStatsPanel.Width = Math.Max(100, availableStatsWidth);
        }
        finally
        {
            updatingTransferLayout = false;
        }
    }

    private void LoadProfiles()
    {
        profiles = profileRepository.Load().ToList();
        RebindProfiles(profiles.FirstOrDefault()?.Id);
    }

    private void ProfileChanged()
    {
        shareProfileButton.Enabled = operationCancellation is null && SelectedProfile is not null;
        if (profileBox.SelectedItem is not CncProfile profile)
        {
            return;
        }

        portBox.Text = profile.PortName;
        profileSummary.Text = profile.Summary;
        statusLabel.Text = $"Ready for {profile.Name}.";
        activeProfileLabel.Text = $"Machine: {profile.Name}";
        deleteProfileButton.Enabled = !profile.IsBuiltIn && operationCancellation is null;
        SchedulePreSendEstimate();
    }

    private void RefreshPorts()
    {
        string selected = portBox.Text;
        string[] ports = SerialPort.GetPortNames()
            .OrderBy(PortSortKey)
            .ToArray();

        portBox.BeginUpdate();
        portBox.Items.Clear();
        portBox.Items.AddRange(ports);
        portBox.EndUpdate();

        portBox.Text = string.IsNullOrWhiteSpace(selected)
            ? (SelectedProfile?.PortName ?? string.Empty)
            : selected;
    }

    private async Task SendProgramAsync(SendStartMode startMode = SendStartMode.FullProgram)
    {
        if (operationCancellation is not null) return;
        if (string.IsNullOrEmpty(programText.Text))
        {
            ShowWarning("Load or paste an NC program before sending.");
            return;
        }

        CncProfile? profile = GetProfileWithSelectedPort();
        if (profile is null)
        {
            return;
        }

        string sourceToSend = programText.Text;
        if (startMode != SendStartMode.FullProgram)
        {
            int startIndex;
            if (startMode == SendStartMode.CursorLine)
            {
                startIndex = FindLineStart(sourceToSend, programText.SelectionStart);
            }
            else
            {
                documentIndex = NcDocumentIndex.Build(sourceToSend);
                NcNavigationItem? tool = documentIndex.Items
                    .Where(item => item.Kind == NcNavigationKind.ToolChange && item.CharacterIndex <= programText.SelectionStart)
                    .LastOrDefault();
                if (tool is null)
                {
                    ShowWarning("No tool change was found before the current cursor position.");
                    return;
                }
                startIndex = FindLineStart(sourceToSend, tool.CharacterIndex);
            }
            sourceToSend = sourceToSend[startIndex..];
        }

        if (startMode != SendStartMode.FullProgram)
        {
            DialogResult confirmation = MessageBox.Show(
                this,
                "This transfer starts in the middle of the NC program. Required safety lines, work offsets, spindle state, or the opening % marker may be missing.\r\n\r\nSend this partial program anyway?",
                "Confirm partial-program transfer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirmation != DialogResult.Yes)
            {
                return;
            }
        }

        await RunOperationAsync(
            token => dncService.SendAsync(profile, sourceToSend, CreateProgress(), token),
            "Send cancelled.",
            "SEND",
            profile,
            $"{Path.GetFileName(filePathBox.Text)}; {startMode}; {sourceToSend.Length:N0} characters");
    }

    private async Task ReceiveProgramAsync()
    {
        if (operationCancellation is not null) return;
        CncProfile? profile = GetProfileWithSelectedPort();
        if (profile is null)
        {
            return;
        }

        CreateNewDocument(false, true);
        receivedContent = true;
        if (activeDocument is not null)
        {
            activeDocument.IsReceived = true;
            activeDocument.IsDirty = true;
            UpdateActiveTabTitle();
        }

        await RunOperationAsync(
            async token =>
            {
                string result = await dncService.ReceiveAsync(profile, CreateProgress(), token);
                programText.Text = result;
                receivedContent = true;
                if (activeDocument is not null)
                {
                    activeDocument.Text = result;
                    activeDocument.IsReceived = true;
                    activeDocument.IsDirty = true;
                    UpdateActiveTabTitle();
                }
            },
            "Receive stopped.",
            "RECEIVE",
            profile,
            "Incoming NC program");
    }

    private async Task RunOperationAsync(
        Func<CancellationToken, Task> operation,
        string cancelledMessage,
        string direction,
        CncProfile profile,
        string details)
    {
        operationCancellation = new CancellationTokenSource();
        activeTransferDirection = direction;
        activeTransferProfile = profile.Name;
        lastTransferLines = 0;
        lastTransferBytes = 0;
        lastTransferErrors = 0;
        lastTransferRate = 0;
        lastTransferElapsed = TimeSpan.Zero;
        percentStatLabel.Text = "0%";
        linesStatLabel.Text = "Lines 0/0";
        bytesStatLabel.Text = "Bytes 0/0";
        rateStatLabel.Text = "0.0 chars/s";
        timeStatLabel.Text = direction == "SEND" ? "Remaining --:--" : "Elapsed 00:00";
        errorsStatLabel.Text = "Errors 0";
        ResetSerialSignals();
        SetBusy(true);
        transferPanel.ProgressPercent = 0;
        transferPanel.Marquee = direction == "RECEIVE";
        SetTransferState(direction == "SEND" ? "SENDING" : "RECEIVING", Color.FromArgb(0, 120, 215));
        AppendTransferLog(direction, profile.Name, "STARTED", details);

        try
        {
            await operation(operationCancellation.Token);
            AppendTransferLog(direction, profile.Name, "COMPLETED", BuildFinalTransferDetails(details));
            SetTransferState("COMPLETED", Color.FromArgb(0, 166, 81));
            PlayAlert(completionSound);
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = cancelledMessage;
            AppendTransferLog(direction, profile.Name, "CANCELLED", BuildFinalTransferDetails(details));
            SetTransferState("STOPPED", Color.FromArgb(255, 193, 7));
            PlayAlert(failureSound);
        }
        catch (Exception exception)
        {
            statusLabel.Text = "Operation failed.";
            AppendTransferLog(direction, profile.Name, "FAILED", BuildFinalTransferDetails($"{details}; {exception.Message}"));
            SetTransferState("FAILED", Color.FromArgb(211, 47, 47));
            PlayAlert(failureSound);
            MessageBox.Show(
                this,
                exception.Message,
                "DNC operation failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            operationCancellation.Dispose();
            operationCancellation = null;
            activeTransferDirection = string.Empty;
            SetBusy(false);
        }
    }

    private Progress<DncProgress> CreateProgress() => new(update =>
    {
        statusLabel.Text = update.EstimatedRemaining is TimeSpan remaining
            ? $"{update.Message}  •  about {FormatDuration(remaining)} remaining"
            : update.Elapsed is TimeSpan elapsed
                ? $"{update.Message}  •  elapsed {FormatDuration(elapsed)}"
                : update.Message;
        if (update.Percent is int percent)
        {
            transferPanel.Marquee = false;
            transferPanel.ProgressPercent = Math.Clamp(percent, 0, 100);
        }
        else
        {
            transferPanel.Marquee = true;
        }

        if (update.ReceivedText is not null)
        {
            programText.Text = update.ReceivedText;
            programText.SelectionStart = programText.TextLength;
            programText.ScrollToCaret();
            receivedContent = true;
        }

        if (update.Percent is int detailPercent)
        {
            percentStatLabel.Text = $"{Math.Clamp(detailPercent, 0, 100)}%";
        }
        if (update.LinesProcessed is int lines)
        {
            linesStatLabel.Text = update.TotalLines is int totalLines
                ? $"Lines {lines:N0}/{totalLines:N0}"
                : $"Lines {lines:N0}";
        }
        if (update.BytesProcessed is long bytes)
        {
            bytesStatLabel.Text = update.TotalBytes is long totalBytes
                ? $"Bytes {bytes:N0}/{totalBytes:N0}"
                : $"Bytes {bytes:N0}";
        }
        if (update.CharactersPerSecond is double rate)
        {
            rateStatLabel.Text = $"{rate:N1} chars/s";
            lastTransferRate = rate;
        }
        if (update.EstimatedRemaining is TimeSpan statRemaining)
        {
            timeStatLabel.Text = $"Remaining {FormatClock(statRemaining)}";
        }
        else if (update.Elapsed is TimeSpan statElapsed)
        {
            timeStatLabel.Text = $"Elapsed {FormatClock(statElapsed)}";
        }
        if (update.Elapsed is TimeSpan measuredElapsed)
        {
            lastTransferElapsed = measuredElapsed;
        }
        if (update.LinesProcessed is int finalLines) lastTransferLines = finalLines;
        if (update.BytesProcessed is long finalBytes) lastTransferBytes = finalBytes;
        if (update.SerialErrors is int errors) lastTransferErrors = errors;
        errorsStatLabel.Text = $"Errors {lastTransferErrors:N0}";
        UpdateSerialSignals(update);

        if (update.RawChunk is not null)
        {
            EnsureDebugWindow().Append(activeTransferDirection, update.RawChunk);
        }
    });

    private void SetBusy(bool busy)
    {
        profileBox.Enabled = !busy;
        portBox.Enabled = !busy;
        refreshPortsButton.Enabled = !busy;
        editProfileButton.Enabled = !busy;
        addProfileButton.Enabled = !busy;
        deleteProfileButton.Enabled = !busy && SelectedProfile is { IsBuiltIn: false };
        profileVisibilityButton.Enabled = !busy;
        shareProfileButton.Enabled = !busy && SelectedProfile is not null;
        communityProfilesButton.Enabled = !busy;
        browseButton.Enabled = !busy;
        recentFilesBox.Enabled = !busy;
        documentTabs.Enabled = !busy;
        newDocumentButton.Enabled = !busy;
        bool transfersAvailable = !busy;
        sendButton.Enabled = transfersAvailable;
        partialSendButton.Enabled = transfersAvailable;
        receiveButton.Enabled = transfersAvailable;
        saveButton.Enabled = !busy;
        soundButton.Enabled = !busy;
        programText.ReadOnly = busy;
        stopButton.Enabled = busy;
        transferAnimationTimer.Enabled = busy;
        if (!busy)
        {
            signalFlashOn = true;
            RefreshSerialSignalColors();
            UpdateEditionDisplay();
        }
    }

    private void UpdateEditionDisplay()
    {
        editionStatusLabel.Text = "OPEN PREVIEW  |  Unlimited transfers";
        editionStatusLabel.ForeColor = ThemeManager.Muted(darkMode);
        editionStatusLabel.AccessibleDescription = "Open Preview with no registration, activation, or transfer limit";
    }

    private void ShowAboutDialog()
    {
        using var dialog = new AboutDialog(darkMode);
        dialog.ShowDialog(this);
    }

    private void OpenProgramFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Open NC program",
            Filter = "NC programs (*.nc;*.tap;*.txt;*.mpf)|*.nc;*.tap;*.txt;*.mpf|All files (*.*)|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        int opened = dialog.FileNames.Count(OpenProgramPath);
        if (dialog.FileNames.Length > 1)
        {
            statusLabel.Text = opened == dialog.FileNames.Length
                ? $"Loaded {opened:N0} programs into separate tabs."
                : $"Loaded {opened:N0} of {dialog.FileNames.Length:N0} selected programs.";
        }
    }

    internal void OpenPostedPrograms(IEnumerable<string> arguments)
    {
        List<string> paths = arguments
            .Where(argument => !string.IsNullOrWhiteSpace(argument))
            .Select(argument => argument.Trim().Trim('"'))
            .Select(TryResolvePostedPath)
            .Where(path => path is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        int opened = paths.Count(OpenProgramPath);
        if (paths.Count > 1)
        {
            statusLabel.Text = opened == paths.Count
                ? $"Opened {opened:N0} posted programs in separate tabs."
                : $"Opened {opened:N0} of {paths.Count:N0} posted programs.";
        }

        RestoreAndActivate();
    }

    private static string? TryResolvePostedPath(string argument)
    {
        try
        {
            string fullPath = Path.GetFullPath(argument);
            return File.Exists(fullPath) ? fullPath : null;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }

    private void RestoreAndActivate()
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }

        Show();
        Activate();
        BringToFront();
    }

    private void CreateNewDocument(bool focusEditor = true, bool received = false)
    {
        SaveActiveDocumentState();
        var document = new NcDocument { IsReceived = received };
        var page = new TabPage(document.TabTitle)
        {
            Tag = document,
            BackColor = ThemeManager.Surface(darkMode),
            ToolTipText = document.DisplayName
        };
        switchingDocument = true;
        documentTabs.TabPages.Add(page);
        documentTabs.SelectedTab = page;
        PositionTabButtons();
        switchingDocument = false;
        LoadDocument(document);
        if (focusEditor)
        {
            programText.Focus();
        }
    }

    private void SwitchToSelectedDocument()
    {
        if (switchingDocument || documentTabs.SelectedTab?.Tag is not NcDocument selected)
        {
            return;
        }
        SaveActiveDocumentState();
        LoadDocument(selected);
    }

    private int TabIndexAt(Point location)
    {
        for (int index = 0; index < documentTabs.TabCount; index++)
        {
            if (documentTabs.GetTabRect(index).Contains(location))
            {
                return index;
            }
        }
        return -1;
    }

    private Rectangle GetTabCloseRectangle(int index)
    {
        Rectangle tabRectangle = documentTabs.GetTabRect(index);
        return new Rectangle(tabRectangle.Right - 24, tabRectangle.Top + 4, 19,
            Math.Max(16, tabRectangle.Height - 8));
    }

    private void HandleTabMouseDown(object? sender, MouseEventArgs eventArgs)
    {
        int tabIndex = TabIndexAt(eventArgs.Location);
        if (tabIndex < 0)
        {
            draggedTabIndex = -1;
            return;
        }

        if (eventArgs.Button == MouseButtons.Left &&
            GetTabCloseRectangle(tabIndex).Contains(eventArgs.Location))
        {
            draggedTabIndex = -1;
            CloseDocumentTab(documentTabs.TabPages[tabIndex]);
            return;
        }

        draggedTabIndex = tabIndex;
    }

    private void PositionTabButtons()
    {
        if (!tabStripPanel.IsHandleCreated) return;
        documentTabs.Size = new Size(tabStripPanel.ClientSize.Width, tabStripPanel.ClientSize.Height);
        int desiredLeft = documentTabs.TabCount == 0
            ? 4
            : documentTabs.GetTabRect(documentTabs.TabCount - 1).Right + 6;
        int controlsWidth = newDocumentButton.Width + 2;
        int availableRight = Math.Max(4, tabStripPanel.ClientSize.Width - controlsWidth - 2);
        int left;
        if (desiredLeft <= availableRight)
        {
            left = desiredLeft;
        }
        else
        {
            // Reserve a permanent right-side lane for the controls. The tab
            // control supplies its own scroll arrows when headers overflow.
            left = availableRight;
        }
        // Keep the native tab control from painting over the adjacent + button.
        // The overlap was most noticeable on a fresh launch with one Untitled tab.
        documentTabs.Width = Math.Max(80, left - 4);
        newDocumentButton.Location = new Point(left, 1);
        newDocumentButton.BringToFront();
    }

    private void DrawDocumentTab(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= documentTabs.TabCount) return;
        bool selected = e.Index == documentTabs.SelectedIndex;
        Color background = selected ? Blue : ThemeManager.Surface(darkMode);
        Color foreground = selected ? Color.White : ThemeManager.Foreground(darkMode);
        using var brush = new SolidBrush(background);
        e.Graphics.FillRectangle(brush, e.Bounds);
        Rectangle textRectangle = new(e.Bounds.Left + 8, e.Bounds.Top + 2,
            Math.Max(10, e.Bounds.Width - 36), e.Bounds.Height - 4);
        TextRenderer.DrawText(e.Graphics, documentTabs.TabPages[e.Index].Text,
            documentTabs.Font, textRectangle, foreground,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        using var closeFont = new Font("Segoe UI Semibold", 11F);
        TextRenderer.DrawText(e.Graphics, "×", closeFont,
            GetTabCloseRectangle(e.Index), foreground,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    private void ReorderDraggedTab(DragEventArgs eventArgs)
    {
        Point clientPoint = documentTabs.PointToClient(new Point(eventArgs.X, eventArgs.Y));
        int targetIndex = TabIndexAt(clientPoint);
        if (draggedTabIndex < 0 || targetIndex < 0 || targetIndex == draggedTabIndex)
        {
            draggedTabIndex = -1;
            return;
        }

        TabPage page = documentTabs.TabPages[draggedTabIndex];
        switchingDocument = true;
        documentTabs.TabPages.Remove(page);
        documentTabs.TabPages.Insert(targetIndex, page);
        documentTabs.SelectedTab = page;
        switchingDocument = false;
        draggedTabIndex = -1;
        PositionTabButtons();
    }

    private void LoadDocument(NcDocument document)
    {
        switchingDocument = true;
        try
        {
            activeDocument = document;
            receivedContent = document.IsReceived;
            programText.Text = document.Text;
            int scrollCharacter = programText.GetFirstCharIndexFromLine(document.FirstVisibleLine);
            if (scrollCharacter >= 0)
            {
                programText.Select(scrollCharacter, 0);
                programText.ScrollToCaret();
            }
            programText.Select(
                Math.Clamp(document.SelectionStart, 0, programText.TextLength),
                Math.Clamp(document.SelectionLength, 0, Math.Max(0, programText.TextLength - document.SelectionStart)));
            filePathBox.Text = document.FilePath;
            findBox.Text = document.FindText;
            replaceBox.Text = document.ReplaceText;
            matchCaseBox.Checked = document.MatchCase;
            wholeWordBox.Checked = document.WholeWord;
            replaceMode = document.ReplaceMode;
            UpdateDocumentSizeLabel();
            UpdateActiveTabTitle();
            RebuildDocumentIndex();
            ApplySyntaxHighlighting();
            statusLabel.Text = $"Active program: {document.DisplayName}.";
        }
        finally
        {
            switchingDocument = false;
        }
    }

    private void SaveActiveDocumentState()
    {
        if (activeDocument is null || switchingDocument)
        {
            return;
        }
        activeDocument.Text = programText.Text;
        activeDocument.FilePath = filePathBox.Text;
        activeDocument.IsReceived = receivedContent;
        activeDocument.SelectionStart = programText.SelectionStart;
        activeDocument.SelectionLength = programText.SelectionLength;
        int firstCharacter = programText.GetCharIndexFromPosition(new Point(1, 1));
        activeDocument.FirstVisibleLine = programText.GetLineFromCharIndex(firstCharacter);
        activeDocument.FindText = findBox.Text;
        activeDocument.ReplaceText = replaceBox.Text;
        activeDocument.MatchCase = matchCaseBox.Checked;
        activeDocument.WholeWord = wholeWordBox.Checked;
        activeDocument.ReplaceMode = replaceMode;
    }

    private void UpdateDocumentSizeLabel()
    {
        int characters = programText.TextLength;
        int bytes = Encoding.ASCII.GetByteCount(programText.Text);
        characterCountLabel.Text = $"{characters:N0} characters  •  {bytes:N0} bytes";
    }

    private static void CaptureFileStamp(NcDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.FilePath) || !File.Exists(document.FilePath))
        {
            document.HasKnownFileStamp = false;
            return;
        }

        var file = new FileInfo(document.FilePath);
        file.Refresh();
        document.LastKnownWriteTimeUtc = file.LastWriteTimeUtc;
        document.LastKnownFileLength = file.Length;
        document.HasKnownFileStamp = true;
    }

    private void CheckForExternalFileChanges()
    {
        if (checkingExternalFiles || operationCancellation is not null || IsDisposed)
        {
            return;
        }

        checkingExternalFiles = true;
        try
        {
            List<NcDocument> documents = documentTabs.TabPages.Cast<TabPage>()
                .Select(page => page.Tag)
                .OfType<NcDocument>()
                .Where(document => !string.IsNullOrWhiteSpace(document.FilePath))
                .ToList();

            foreach (NcDocument document in documents)
            {
                CheckDocumentForExternalChange(document);
            }
        }
        finally
        {
            checkingExternalFiles = false;
        }
    }

    private void CheckDocumentForExternalChange(NcDocument document)
    {
        if (document.ExternalChangePromptOpen || !File.Exists(document.FilePath))
        {
            return;
        }

        try
        {
            var file = new FileInfo(document.FilePath);
            file.Refresh();
            if (!document.HasKnownFileStamp)
            {
                CaptureFileStamp(document);
                return;
            }

            if (file.LastWriteTimeUtc == document.LastKnownWriteTimeUtc &&
                file.Length == document.LastKnownFileLength)
            {
                return;
            }

            // Remember this revision before prompting so declining does not
            // produce the same question every time the monitor runs.
            document.LastKnownWriteTimeUtc = file.LastWriteTimeUtc;
            document.LastKnownFileLength = file.Length;
            document.ExternalChangePromptOpen = true;
            DialogResult result;
            try
            {
                string unsavedWarning = document.IsDirty
                    ? "\n\nReloading will replace the unsaved edits in this tab."
                    : string.Empty;
                result = MessageBox.Show(
                    this,
                    $"“{document.DisplayName}” was changed outside C2C DNC. Reload it now?{unsavedWarning}",
                    "Program changed on disk",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
            }
            finally
            {
                document.ExternalChangePromptOpen = false;
            }

            if (result == DialogResult.Yes)
            {
                document.Text = File.ReadAllText(document.FilePath);
                document.IsDirty = false;
                document.IsReceived = false;
                document.SelectionStart = Math.Min(document.SelectionStart, document.Text.Length);
                document.SelectionLength = 0;
                CaptureFileStamp(document);
                if (ReferenceEquals(document, activeDocument))
                {
                    LoadDocument(document);
                }
                else
                {
                    UpdateDocumentTabTitle(document);
                }
                statusLabel.Text = $"Reloaded {document.DisplayName} after an external change.";
            }
            else
            {
                // The editor now differs from the file on disk, even if it had
                // no local edits before the external update.
                document.IsDirty = true;
                UpdateDocumentTabTitle(document);
                statusLabel.Text = $"Kept the current C2C DNC copy of {document.DisplayName}.";
            }
        }
        catch (IOException)
        {
            // Editors sometimes replace or briefly lock a file while saving.
            // Leave the old stamp intact and try again on the next monitor pass.
        }
        catch (UnauthorizedAccessException)
        {
            // A transient access problem should not interrupt the operator.
        }
    }

    private void SchedulePreSendEstimate()
    {
        sendEstimateTimer.Stop();
        sendEstimateTimer.Start();
    }

    private void UpdatePreSendEstimate()
    {
        if (SelectedProfile is not CncProfile profile || string.IsNullOrEmpty(programText.Text))
        {
            estimatedSendLabel.Text = "Estimated send: —";
            estimatedSendLabel.AccessibleDescription = "Open or paste an NC program to estimate its send time.";
            return;
        }

        DncSendEstimate estimate = SerialDncService.CalculateSendEstimate(profile, programText.Text);
        if (estimate.Lines == 0)
        {
            estimatedSendLabel.Text = "Estimated send: no transferable lines";
            estimatedSendLabel.AccessibleDescription =
                "The selected profile filters out every line in this program.";
            return;
        }

        estimatedSendLabel.Text = $"Estimated send: {FormatClock(estimate.Duration)}";
        estimatedSendLabel.AccessibleDescription =
            $"Estimated using {estimate.Lines:N0} filtered lines and {estimate.Bytes:N0} transmitted bytes.";
    }

    private void UpdateActiveTabTitle()
    {
        if (activeDocument is null)
        {
            return;
        }
        UpdateDocumentTabTitle(activeDocument);
    }

    private void UpdateDocumentTabTitle(NcDocument document)
    {
        TabPage? page = documentTabs.TabPages.Cast<TabPage>()
            .FirstOrDefault(item => ReferenceEquals(item.Tag, document));
        if (page is not null)
        {
            string title = document.TabTitle;
            string toolTip = string.IsNullOrWhiteSpace(document.FilePath)
                ? document.DisplayName
                : document.FilePath;
            page.ToolTipText = toolTip;
            if (!string.Equals(page.Text, title, StringComparison.Ordinal))
            {
                page.Text = title;
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(new Action(PositionTabButtons));
                }
            }
        }
    }

    private void CloseActiveDocument()
    {
        if (documentTabs.SelectedTab is TabPage page)
        {
            CloseDocumentTab(page);
        }
    }

    private void CloseDocumentTab(TabPage page)
    {
        if (page.Tag is not NcDocument document)
        {
            return;
        }

        bool wasSelected = ReferenceEquals(documentTabs.SelectedTab, page);
        int closingIndex = documentTabs.TabPages.IndexOf(page);
        if (ReferenceEquals(activeDocument, document))
        {
            SaveActiveDocumentState();
        }
        if (!ConfirmCloseDocuments([document]))
        {
            return;
        }

        switchingDocument = true;
        documentTabs.TabPages.Remove(page);
        page.Dispose();
        switchingDocument = false;
        PositionTabButtons();
        if (documentTabs.TabPages.Count == 0)
        {
            activeDocument = null;
            CreateNewDocument();
        }
        else if (wasSelected)
        {
            int nextIndex = Math.Clamp(closingIndex, 0, documentTabs.TabPages.Count - 1);
            documentTabs.SelectedIndex = nextIndex;
            activeDocument = null;
            if (documentTabs.SelectedTab?.Tag is NcDocument next)
            {
                LoadDocument(next);
            }
        }
    }

    private void CloseAllDocuments()
    {
        SaveActiveDocumentState();
        List<NcDocument> documents = documentTabs.TabPages.Cast<TabPage>()
            .Select(page => page.Tag)
            .OfType<NcDocument>()
            .ToList();
        if (!ConfirmCloseDocuments(documents))
        {
            return;
        }
        switchingDocument = true;
        documentTabs.TabPages.Clear();
        switchingDocument = false;
        PositionTabButtons();
        activeDocument = null;
        CreateNewDocument();
    }

    private bool ConfirmCloseDocuments(IEnumerable<NcDocument> documents)
    {
        foreach (NcDocument document in documents.Where(item => item.IsDirty))
        {
            DialogResult result = MessageBox.Show(
                this,
                $"“{document.DisplayName}” has unsaved changes. Discard those changes?",
                "Close modified program",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return false;
            }
        }
        return true;
    }

    private bool OpenProgramPath(string path)
    {
        try
        {
            string fullPath = Path.GetFullPath(path);
            TabPage? existingPage = documentTabs.TabPages.Cast<TabPage>()
                .FirstOrDefault(page => page.Tag is NcDocument document &&
                    string.Equals(document.FilePath, fullPath, StringComparison.OrdinalIgnoreCase));
            if (existingPage is not null)
            {
                documentTabs.SelectedTab = existingPage;
                return true;
            }

            NcDocument document;
            if (activeDocument is { IsDirty: false, Text.Length: 0 } &&
                string.IsNullOrWhiteSpace(activeDocument.FilePath) && documentTabs.TabPages.Count == 1)
            {
                document = activeDocument;
            }
            else
            {
                CreateNewDocument(false);
                document = activeDocument!;
            }
            document.Text = File.ReadAllText(fullPath);
            document.FilePath = fullPath;
            document.IsDirty = false;
            document.IsReceived = false;
            document.SelectionStart = 0;
            document.SelectionLength = 0;
            CaptureFileStamp(document);
            LoadDocument(document);
            statusLabel.Text = $"Loaded {Path.GetFileName(path)}.";
            AddRecentFile(fullPath);
            return true;
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Could not open file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void SaveProgramFile()
    {
        if (programText.TextLength == 0)
        {
            ShowWarning("There is no program text to save.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Save NC program",
            Filter = "NC program (*.nc)|*.nc|Text file (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = "nc",
            AddExtension = true,
            FileName = receivedContent ? $"received-{DateTime.Now:yyyyMMdd-HHmmss}.nc" : "program.nc"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            File.WriteAllText(dialog.FileName, programText.Text, System.Text.Encoding.ASCII);
            filePathBox.Text = dialog.FileName;
            if (activeDocument is not null)
            {
                activeDocument.FilePath = dialog.FileName;
                activeDocument.Text = programText.Text;
                activeDocument.IsDirty = false;
                activeDocument.IsReceived = false;
                CaptureFileStamp(activeDocument);
                UpdateActiveTabTitle();
            }
            statusLabel.Text = $"Saved {Path.GetFileName(dialog.FileName)}.";
            AddRecentFile(dialog.FileName);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Could not save file", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private CncProfile? GetProfileWithSelectedPort()
    {
        CncProfile? selected = SelectedProfile;
        string port = portBox.Text.Trim().ToUpperInvariant();

        if (selected is null || string.IsNullOrWhiteSpace(port))
        {
            ShowWarning("Select a CNC profile and enter a COM port.");
            return null;
        }

        return selected with { PortName = port };
    }

    private CncProfile? SelectedProfile => profileBox.SelectedItem as CncProfile;

    private void EditSelectedProfile()
    {
        if (SelectedProfile is not CncProfile selected)
        {
            return;
        }

        using var dialog = new ProfileEditorDialog(selected, false, darkMode);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        int index = profiles.FindIndex(item => item.Id == selected.Id);
        if (index >= 0)
        {
            profiles[index] = dialog.Result;
            SaveAndReloadProfiles(dialog.Result.Id);
        }
    }

    private void ShareSelectedProfile()
    {
        if (operationCancellation is not null || SelectedProfile is not CncProfile selected) return;
        using var dialog = new ShareProfileDialog(selected, darkMode);
        dialog.ShowDialog(this);
    }

    private void BrowseCommunityProfiles()
    {
        if (operationCancellation is not null) return;
        using var dialog = new CommunityProfilesDialog(profiles, darkMode);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is not CncProfile imported) return;
        try
        {
            // Persist a separate candidate list first. Failed writes leave all existing objects,
            // in-memory membership and hidden-profile preferences untouched.
            List<CncProfile> candidate = CommunityConfiguration.SaveNewProfile(profiles, imported, profileRepository.Save);
            profiles = candidate;
            RebindProfiles(imported.Id);
            statusLabel.Text = "Community configuration added as a new local copy. Verify the setup before use.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
        {
            MessageBox.Show(this, "No profile was added. " + ex.Message, "Could not save imported profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddProfile()
    {
        CncProfile template = SelectedProfile ?? CncProfile.BuiltIn[0];
        CncProfile newProfile = template with
        {
            Id = Guid.NewGuid().ToString("N"),
            IsBuiltIn = false,
            Name = $"Copy of {template.Name}"
        };

        using var dialog = new ProfileEditorDialog(newProfile, true, darkMode);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        profiles.Add(dialog.Result);
        hiddenProfileIds.Remove(dialog.Result.Id);
        SaveAndReloadProfiles(dialog.Result.Id);
    }

    private void DeleteSelectedProfile()
    {
        if (SelectedProfile is not CncProfile selected || selected.IsBuiltIn)
        {
            return;
        }

        DialogResult confirmation = MessageBox.Show(
            this,
            $"Delete the profile “{selected.Name}”?",
            "Delete CNC profile",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        profiles.RemoveAll(item => item.Id == selected.Id);
        hiddenProfileIds.Remove(selected.Id);
        SaveAppSettings();
        SaveAndReloadProfiles(CncProfile.BuiltIn[0].Id);
    }

    private void SaveAndReloadProfiles(string selectedId)
    {
        try
        {
            profileRepository.Save(profiles);
            RebindProfiles(selectedId);
            statusLabel.Text = "Profile settings saved.";
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Could not save profiles",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ShowWarning(string message) =>
        MessageBox.Show(this, message, "C2C DNC", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static int PortSortKey(string portName) =>
        int.TryParse(portName.AsSpan(3), out int number) ? number : int.MaxValue;

    private static string FormatDuration(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        return value.TotalHours >= 1
            ? $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}"
            : $"{(int)value.TotalMinutes}:{value.Seconds:00}";
    }

    private static string FormatClock(TimeSpan value)
    {
        if (value < TimeSpan.Zero) value = TimeSpan.Zero;
        return value.TotalHours >= 1
            ? $"{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}"
            : $"{(int)value.TotalMinutes:00}:{value.Seconds:00}";
    }

    private void ToggleDarkMode(bool enabled)
    {
        darkMode = enabled;
        ApplyTheme();
        try
        {
            SaveAppSettings();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            statusLabel.Text = "Theme changed, but the preference could not be saved.";
        }
    }

    private void ManageProfileVisibility()
    {
        using var dialog = new ProfileVisibilityDialog(profiles, hiddenProfileIds, darkMode);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        string? selectedId = SelectedProfile?.Id;
        hiddenProfileIds = dialog.HiddenProfileIds;
        try
        {
            SaveAppSettings();
            RebindProfiles(selectedId);
            statusLabel.Text = "Visible profiles updated.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Could not save profile visibility",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void RebindProfiles(string? selectedId)
    {
        List<CncProfile> visibleProfiles = profiles
            .Where(profile => !hiddenProfileIds.Contains(profile.Id))
            .ToList();
        if (visibleProfiles.Count == 0 && profiles.Count > 0)
        {
            hiddenProfileIds.Remove(profiles[0].Id);
            visibleProfiles.Add(profiles[0]);
        }

        profileBox.DataSource = null;
        profileBox.DataSource = visibleProfiles;
        profileBox.DisplayMember = nameof(CncProfile.Name);
        int index = visibleProfiles.FindIndex(profile => profile.Id == selectedId);
        profileBox.SelectedIndex = Math.Max(0, index);
    }

    private void ShowSearch(bool showReplace)
    {
        replaceMode = showReplace;
        searchRowStyle.Height = 38;
        replaceBox.Visible = showReplace;
        foreach (Control control in searchPanel.Controls)
        {
            if (control.Name is "ReplaceOne" or "ReplaceAll")
            {
                control.Visible = showReplace;
            }
        }

        if (programText.SelectionLength > 0 && !programText.SelectedText.Contains('\n'))
        {
            findBox.Text = programText.SelectedText;
        }
        findBox.Focus();
        findBox.SelectAll();
    }

    private void HideSearch()
    {
        searchRowStyle.Height = 0;
        programText.Focus();
    }

    private void FindNext(bool forward)
    {
        if (findBox.TextLength == 0)
        {
            ShowSearch(replaceMode);
            return;
        }

        if (!EditorSearch.Find(programText, findBox.Text, forward, matchCaseBox.Checked, wholeWordBox.Checked))
        {
            statusLabel.Text = $"“{findBox.Text}” was not found.";
        }
    }

    private void ReplaceOne()
    {
        if (EditorSearch.SelectionMatches(programText, findBox.Text, matchCaseBox.Checked))
        {
            programText.SelectedText = replaceBox.Text;
        }
        FindNext(true);
    }

    private void ReplaceAll()
    {
        int count = EditorSearch.ReplaceAll(
            programText,
            findBox.Text,
            replaceBox.Text,
            matchCaseBox.Checked,
            wholeWordBox.Checked);
        statusLabel.Text = count == 1 ? "Replaced 1 match." : $"Replaced {count:N0} matches.";
    }

    private void ScheduleOutlineRebuild()
    {
        outlineTimer.Stop();
        outlineTimer.Start();
    }

    private void RebuildDocumentIndex()
    {
        documentIndex = NcDocumentIndex.Build(programText.Text);
        int topIndex = outlineList.TopIndex;
        outlineList.BeginUpdate();
        outlineList.Items.Clear();
        outlineList.Items.AddRange(documentIndex.Items.Cast<object>().ToArray());
        outlineList.EndUpdate();
        outlineList.HorizontalExtent = documentIndex.Items.Count == 0
            ? 0
            : documentIndex.Items.Max(item =>
                TextRenderer.MeasureText(item.ToString(), outlineList.Font).Width) + 24;
        if (outlineList.Items.Count > 0)
        {
            outlineList.TopIndex = Math.Clamp(topIndex, 0, outlineList.Items.Count - 1);
        }
    }

    private void Navigate(NcNavigationKind kind, bool forward)
    {
        RebuildDocumentIndex();
        NcNavigationItem? item = documentIndex.Find(kind, programText.SelectionStart, forward);
        if (item is null)
        {
            statusLabel.Text = $"No {NavigationName(kind)} found in this program.";
            return;
        }
        JumpTo(item);
    }

    private void JumpToOutlineSelection()
    {
        if (outlineList.SelectedItem is NcNavigationItem item)
        {
            JumpTo(item);
        }
    }

    private void JumpTo(NcNavigationItem item)
    {
        programText.SelectionStart = Math.Clamp(item.CharacterIndex, 0, programText.TextLength);
        programText.SelectionLength = 0;
        programText.ScrollToCaret();
        programText.Focus();
        statusLabel.Text = $"Line {item.LineNumber}: {item.Label}";
    }

    private void GoToProgramTop()
    {
        programText.Select(0, 0);
        programText.ScrollToCaret();
        programText.Focus();
        statusLabel.Text = "Top of program.";
    }

    private void ResizeOutline(int change)
    {
        outlineWidth = Math.Clamp(outlineWidth + change, 220, 520);
        outlineColumnStyle.Width = outlineWidth;
        SaveAppSettings();
        statusLabel.Text = $"NC Outline width: {outlineWidth} px.";
    }

    private void SetTransferState(string state, Color color)
    {
        transferStateLabel.Text = state;
        transferStateLabel.BackColor = color;
        transferStateLabel.ForeColor = Color.White;
        transferPanel.FillColor = color;
        double luminance = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
        transferStateLabel.ForeColor = luminance > 155 ? Navy : Color.White;
        if (state == "READY")
        {
            transferPanel.ProgressPercent = 0;
            transferPanel.Marquee = false;
        }
        else if (state is "COMPLETED" or "FAILED" or "STOPPED")
        {
            transferPanel.Marquee = false;
            transferPanel.ProgressPercent = 100;
            timeStatLabel.Text = $"Total {FormatClock(lastTransferElapsed)}";
        }
    }

    private void PlayAlert(string soundName)
    {
        if (!alertsEnabled || alertVolume == 0)
        {
            return;
        }
        try
        {
            AlertSoundService.Play(soundName, alertVolume);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            statusLabel.Text = $"Alert sound could not be played: {exception.Message}";
        }
    }

    private void ShowSoundSettings()
    {
        using var dialog = new AlertSettingsDialog(
            alertsEnabled,
            completionSound,
            failureSound,
            alertVolume,
            darkMode);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        alertsEnabled = dialog.AlertsEnabled;
        completionSound = dialog.CompletionSound;
        failureSound = dialog.FailureSound;
        alertVolume = dialog.Volume;
        SaveAppSettings();
        statusLabel.Text = alertsEnabled
            ? $"Transfer sounds enabled: {completionSound}."
            : "Transfer sounds muted.";
    }

    private NcNavigationKind SelectedQuickNavigationKind() => quickNavigationBox.SelectedIndex switch
    {
        1 => NcNavigationKind.OptionalStop,
        2 => NcNavigationKind.ProgramEnd,
        3 => NcNavigationKind.Spindle,
        4 => NcNavigationKind.Coolant,
        _ => NcNavigationKind.Operation
    };

    private static string NavigationName(NcNavigationKind kind) => kind switch
    {
        NcNavigationKind.ToolChange => "tool changes",
        NcNavigationKind.Operation => "operation comments",
        NcNavigationKind.ProgramNumber => "program numbers",
        NcNavigationKind.OptionalStop => "M00/M01 stops",
        NcNavigationKind.ProgramEnd => "program end",
        NcNavigationKind.Spindle => "spindle commands",
        NcNavigationKind.Coolant => "coolant commands",
        _ => "navigation targets"
    };

    private void AddRecentFile(string path)
    {
        string fullPath = Path.GetFullPath(path);
        recentFiles.RemoveAll(item => string.Equals(item, fullPath, StringComparison.OrdinalIgnoreCase));
        recentFiles.Insert(0, fullPath);
        if (recentFiles.Count > 12)
        {
            recentFiles.RemoveRange(12, recentFiles.Count - 12);
        }
        RefreshRecentFiles();
        SaveAppSettings();
    }

    private void RefreshRecentFiles()
    {
        suppressRecentSelection = true;
        try
        {
            recentFilesBox.Items.Clear();
            recentFilesBox.Items.Add("Recent files…");
            recentFilesBox.Items.AddRange(recentFiles.Cast<object>().ToArray());
            recentFilesBox.SelectedIndex = 0;
        }
        finally
        {
            suppressRecentSelection = false;
        }
    }

    private void OpenSelectedRecentFile()
    {
        if (suppressRecentSelection || recentFilesBox.SelectedIndex <= 0)
        {
            return;
        }

        string? path = recentFilesBox.SelectedItem as string;
        suppressRecentSelection = true;
        recentFilesBox.SelectedIndex = 0;
        suppressRecentSelection = false;
        if (path is null)
        {
            return;
        }
        if (!File.Exists(path))
        {
            recentFiles.RemoveAll(item => string.Equals(item, path, StringComparison.OrdinalIgnoreCase));
            RefreshRecentFiles();
            SaveAppSettings();
            ShowWarning("That recent file no longer exists and was removed from the list.");
            return;
        }
        OpenProgramPath(path);
    }

    private SerialDebugWindow EnsureDebugWindow()
    {
        if (debugWindow is null || debugWindow.IsDisposed)
        {
            debugWindow = new SerialDebugWindow(darkMode);
        }
        return debugWindow;
    }

    private void ShowDebugWindow()
    {
        SerialDebugWindow window = EnsureDebugWindow();
        if (!window.Visible)
        {
            window.Show(this);
        }
        window.BringToFront();
    }

    private void ShowTransferLog()
    {
        using var dialog = new TransferLogDialog(transferLogRepository.ReadAll(), darkMode);
        dialog.ShowDialog(this);
    }

    private void AppendTransferLog(string direction, string profile, string result, string details)
    {
        try
        {
            transferLogRepository.Append(direction, profile, result, details);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            statusLabel.Text = $"Transfer log could not be written: {exception.Message}";
        }
    }

    private string BuildFinalTransferDetails(string details) =>
        $"{details}; lines {lastTransferLines:N0}; bytes {lastTransferBytes:N0}; " +
        $"{lastTransferRate:N1} chars/s; total {FormatClock(lastTransferElapsed)}; errors {lastTransferErrors:N0}";

    private void ResetSerialSignals()
    {
        foreach (string name in serialSignalStates.Keys.ToList())
        {
            serialSignalStates[name] = false;
        }
        RefreshSerialSignalColors();
    }

    private void UpdateSerialSignals(DncProgress update)
    {
        SetSerialSignal("RX", update.RxActive);
        SetSerialSignal("TX", update.TxActive);
        SetSerialSignal("RTS", update.RtsActive);
        SetSerialSignal("CTS", update.CtsActive);
        SetSerialSignal("DTR", update.DtrActive);
        SetSerialSignal("DSR", update.DsrActive);
        if (update.SerialErrors is int errors)
        {
            serialSignalStates["ERR"] = errors > 0;
            serialSignalLabels["ERR"].Text = errors == 0 ? "● ERR" : $"● ERR {errors}";
        }
        RefreshSerialSignalColors();
    }

    private void SetSerialSignal(string name, bool? active)
    {
        if (active is null) return;
        serialSignalStates[name] = active.Value;
    }

    private void RefreshSerialSignalColors()
    {
        foreach ((string name, Label label) in serialSignalLabels)
        {
            bool active = serialSignalStates.GetValueOrDefault(name);
            bool illuminated = active && (!transferAnimationTimer.Enabled || signalFlashOn);
            label.ForeColor = name == "ERR" && active
                ? (illuminated ? Color.FromArgb(235, 62, 62) : Color.FromArgb(125, 55, 55))
                : illuminated
                    ? Color.FromArgb(32, 210, 112)
                    : ThemeManager.Muted(darkMode);
        }
    }

    private static int FindLineStart(string text, int position)
    {
        int safePosition = Math.Clamp(position, 0, text.Length);
        int previousNewline = safePosition > 0 ? text.LastIndexOf('\n', safePosition - 1) : -1;
        return previousNewline + 1;
    }

    private void SaveAppSettings() => appSettingsRepository.Save(
        new AppSettings(
            darkMode,
            hiddenProfileIds.ToHashSet(StringComparer.Ordinal),
            recentFiles.ToList(),
            alertsEnabled,
            completionSound,
            failureSound,
            alertVolume,
            outlineWidth));

    private void ApplyTheme()
    {
        ThemeManager.Apply(this, darkMode);
        Image? previousLogo = logoBox.Image;
        logoBox.Image = BrandAssets.LoadLogo(darkMode);
        previousLogo?.Dispose();
        debugWindow?.ApplyTheme(darkMode);
        foreach (TabPage page in documentTabs.TabPages)
        {
            page.BackColor = ThemeManager.Surface(darkMode);
        }
        documentTabs.Invalidate();
        ResetSerialSignals();
        UpdateEditionDisplay();
        ApplySyntaxHighlighting();
    }

    private void ScheduleSyntaxHighlighting()
    {
        if (applyingSyntaxHighlighting || IsDisposed)
        {
            return;
        }

        highlightTimer.Stop();
        highlightTimer.Start();
    }

    private void ApplySyntaxHighlighting()
    {
        if (applyingSyntaxHighlighting || programText.IsDisposed)
        {
            return;
        }

        applyingSyntaxHighlighting = true;
        try
        {
            GCodeSyntaxHighlighter.Apply(programText, darkMode);
        }
        finally
        {
            applyingSyntaxHighlighting = false;
        }
    }

    private void WarmUpSyntaxEditor()
    {
        GCodeSyntaxHighlighter.WarmUp();
        programText.Text = "N1 G01 X1 Y1 Z1 F10 (STARTUP)";
        GCodeSyntaxHighlighter.Apply(programText, darkMode);
        programText.Clear();
        highlightTimer.Stop();
    }

    private static Panel CreateCard() => new()
    {
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 4, 0, 8),
        BackColor = Surface,
        BorderStyle = BorderStyle.FixedSingle,
        Tag = "card"
    };

    private static Button SmallButton(string text)
    {
        var button = new Button
        {
            AutoSize = true,
            Height = 29,
            Margin = new Padding(2, 0, 2, 0)
        };
        ConfigureSecondaryButton(button, text);
        return button;
    }

    private static void ConfigureTransferStat(Label label, string text, int width)
    {
        label.Text = text;
        label.Size = new Size(width, 28);
        label.Margin = new Padding(2, 0, 2, 0);
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Font = new Font("Cascadia Mono", 9.5F, FontStyle.Bold);
        label.AutoEllipsis = true;
        label.BackColor = Color.Transparent;
        label.Tag = "transparent";
    }

    private static Label FieldLabel(string text, int x, int y) => new()
    {
        Text = text,
        Location = new Point(x, y),
        AutoSize = true,
        ForeColor = Muted,
        Font = new Font("Segoe UI Semibold", 7.5F),
        Tag = "muted"
    };

    private static void ConfigurePrimaryButton(Button button, string text)
    {
        button.Text = text;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = ThemeManager.Action;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 9F);
        button.Cursor = Cursors.Hand;
        button.Tag = "primary";
    }

    private static void ConfigureSecondaryButton(Button button, string text)
    {
        button.Text = text;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(190, 200, 213);
        button.BackColor = Color.White;
        button.ForeColor = Navy;
        button.Font = new Font("Segoe UI Semibold", 9F);
        button.Cursor = Cursors.Hand;
    }

    private static void ConfigureGroupedButton(Button button, string text, string group)
    {
        ConfigureSecondaryButton(button, text);
        ApplyButtonGroup(button, group);
    }

    private static void ApplyButtonGroup(Button button, string group)
    {
        Color color = ThemeManager.ButtonGroupColor(group);
        button.Tag = $"button-{group}";
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.FlatAppearance.BorderColor = color;
        button.FlatAppearance.BorderSize = 1;
    }
}
