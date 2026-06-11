using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Drawing;

namespace WowPacketParserGUI;

internal sealed class DarkComboBox : ComboBox
{
    private const int WM_PAINT = 0xF;

    public DarkComboBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Draw custom border
        using var pen = new Pen(Color.FromArgb(0x42, 0x42, 0x48));
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_PAINT && DropDownStyle == ComboBoxStyle.DropDownList)
        {
            base.WndProc(ref m);

            // Redraw border after default paint
            using var g = Graphics.FromHwnd(Handle);
            using var pen = new Pen(Color.FromArgb(0x42, 0x42, 0x48));
            g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
        else
        {
            base.WndProc(ref m);
        }
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var bgControl = Color.FromArgb(0x2C, 0x2C, 0x30);
        var bgControlHover = Color.FromArgb(0x3A, 0x3A, 0x40);
        var fgText = Color.FromArgb(0xE0, 0xE0, 0xE2);

        var isSelected = (e.State & DrawItemState.Selected) != 0;
        var itemBg = isSelected ? bgControlHover : bgControl;

        using (var bgBrush = new SolidBrush(itemBg))
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

        using (var fgBrush = new SolidBrush(fgText))
        {
            var text = Items[e.Index]?.ToString() ?? string.Empty;
            e.Graphics.DrawString(text, e.Font ?? Font, fgBrush, e.Bounds.X + 4, e.Bounds.Y + 2);
        }

        e.DrawFocusRectangle();
    }
}

public partial class MainForm : Form
{
    private TextBox filePathTextBox = null!;
    private Button browseButton = null!;
    private Button parseButton = null!;
    private Button cancelButton = null!;
    private Button reparseButton = null!;
    private Button copyButton = null!;
    private Button openEditorButton = null!;
    private Button openConfigButton = null!;
    private Button firstCraftButton = null!;
    private Button timeOrderButton = null!;
    private Button prevPageButton = null!;
    private Button nextPageButton = null!;
    private Button prevHighlightButton = null!;
    private Button nextHighlightButton = null!;
    private TextBox highlightTextBox = null!;
    private DarkComboBox packetComboBox = null!;
    private TextBox searchTextBox = null!;
    private RichTextBox outputTextBox = null!;
    private ProgressBar progressBar = null!;
    private Label progressLabel = null!;
    private Label occurrenceLabel = null!;
    private Panel comboBorderPanel = null!;
    private Panel filePathBorderPanel = null!;
    private Panel progressBarBorderPanel = null!;
    private Panel highlightBorderPanel = null!;
    private Label pageLabel = null!;
    private List<string> allPackets = new();
    private Dictionary<string, List<List<string>>> packetLines = new();
    private Dictionary<string, string> packetTimestamps = new();
    private string? currentFilePath;
    private string? parsedContent;
    private Process? currentProcess;
    private int lastReportedProgress = -1;
    private bool isReparsing = false;
    private string? selectedPacketBeforeReparse;
    private int currentPage = 0;
    private int totalPages = 0;
    private int pageBeforeReparse = 0;
    private List<int> highlightMatchPositions = new();
    private int currentHighlightIndex = -1;

    public MainForm()
    {
        InitializeComponent();
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    private void InitializeComponent()
    {
        Text = "WowPacketParser GUI";
        Size = new Size(1120, 700);
        MinimumSize = new Size(960, 520);
        StartPosition = FormStartPosition.CenterScreen;
        Padding = new Padding(12, 10, 12, 10);

        // ── Row 1: File selection ──────────────────────────────────────────────
        // Y=14 gives ~14px top padding
        var fileLabel = new Label
        {
            Text = "PKT File:",
            Location = new Point(12, 17),
            Size = new Size(62, 26),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        filePathBorderPanel = new Panel
        {
            Location = new Point(78, 13),
            Size = new Size(500, 26),
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        filePathTextBox = new TextBox
        {
            Location = new Point(-1, -1),
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5f)
        };
        filePathBorderPanel.Controls.Add(filePathTextBox);

        browseButton = new Button
        {
            Text = "Browse",
            Location = new Point(730, 14),
            Size = new Size(82, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        browseButton.Click += BrowseButton_Click;

        parseButton = new Button
        {
            Text = "Parse",
            Location = new Point(822, 14),
            Size = new Size(82, 28),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        parseButton.Click += ParseButton_Click;

        cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(822, 14),
            Size = new Size(82, 28),
            Enabled = false,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        cancelButton.Click += CancelButton_Click;

        openConfigButton = new Button
        {
            Text = "Config",
            Location = new Point(914, 14),
            Size = new Size(78, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        openConfigButton.Click += OpenConfigButton_Click;

        // ── Thin separator line ───────────────────────────────────────────────
        var separator1 = new Panel
        {
            Location = new Point(12, 50),
            Size = new Size(980, 1),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // ── Row 2: Packet selection ───────────────────────────────────────────
        var packetLabel = new Label
        {
            Text = "Packet:",
            Location = new Point(12, 62),
            Size = new Size(56, 28),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        searchTextBox = new TextBox
        {
            Location = new Point(72, 62),
            Size = new Size(210, 28),
            PlaceholderText = "Search packets...",
            Font = new Font("Segoe UI", 9.5f),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        searchTextBox.TextChanged += SearchTextBox_TextChanged;

        comboBorderPanel = new Panel
        {
            Location = new Point(292, 61),
            Size = new Size(700, 24),
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        packetComboBox = new DarkComboBox
        {
            Location = new Point(-1, -1),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = false,
            Font = new Font("Segoe UI", 9.5f)
        };
        packetComboBox.SelectedIndexChanged += PacketComboBox_SelectedIndexChanged;
        comboBorderPanel.Controls.Add(packetComboBox);

        // ── Thin separator line ───────────────────────────────────────────────
        var separator2 = new Panel
        {
            Location = new Point(12, 100),
            Size = new Size(980, 1),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // ── Row 3: Action buttons ─────────────────────────────────────────────
        reparseButton = new Button
        {
            Text = "Re-parse",
            Location = new Point(12, 112),
            Size = new Size(88, 28),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        reparseButton.Click += ReparseButton_Click;

        copyButton = new Button
        {
            Text = "Copy",
            Location = new Point(108, 112),
            Size = new Size(80, 28),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        copyButton.Click += CopyButton_Click;

        openEditorButton = new Button
        {
            Text = "Open",
            Location = new Point(196, 112),
            Size = new Size(80, 28),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        openEditorButton.Click += OpenEditorButton_Click;

        firstCraftButton = new Button
        {
            Text = "First Craft",
            Location = new Point(284, 112),
            Size = new Size(96, 28),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        firstCraftButton.Click += FirstCraftButton_Click;

        timeOrderButton = new Button
        {
            Text = "Time Order",
            Location = new Point(388, 112),
            Size = new Size(100, 28),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        timeOrderButton.Click += TimeOrderButton_Click;

        // ── Row 3 extended: Highlight search (next to buttons) ─────────────────
        prevHighlightButton = new Button
        {
            Text = "▲",
            Location = new Point(496, 112),
            Size = new Size(30, 28),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        prevHighlightButton.Click += PrevHighlightButton_Click;

        // Highlight textbox with blue border panel
        highlightBorderPanel = new Panel
        {
            Location = new Point(530, 112),
            Size = new Size(176, 28),
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        highlightTextBox = new TextBox
        {
            Location = new Point(-1, -1),
            Size = new Size(178, 30),
            PlaceholderText = "Highlight text...",
            Font = new Font("Segoe UI", 9.5f),
            BorderStyle = BorderStyle.FixedSingle
        };
        highlightTextBox.TextChanged += HighlightTextBox_TextChanged;
        highlightBorderPanel.Controls.Add(highlightTextBox);

        nextHighlightButton = new Button
        {
            Text = "▼",
            Location = new Point(710, 112),
            Size = new Size(30, 28),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        nextHighlightButton.Click += NextHighlightButton_Click;

        // ── Row 3 extended: Pagination ─────────────────────────────────────────
        occurrenceLabel = new Label
        {
            Location = new Point(752, 114),
            Size = new Size(90, 24),
            Text = "",
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        prevPageButton = new Button
        {
            Text = "◀",
            Location = new Point(848, 112),
            Size = new Size(40, 28),
            Enabled = false,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        prevPageButton.Click += PrevPageButton_Click;

        pageLabel = new Label
        {
            Location = new Point(892, 114),
            Size = new Size(60, 24),
            Text = "",
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        nextPageButton = new Button
        {
            Text = "▶",
            Location = new Point(956, 112),
            Size = new Size(40, 28),
            Enabled = false,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        nextPageButton.Click += NextPageButton_Click;

        // Progress bar with border panel (shows outline even when empty)
        progressBarBorderPanel = new Panel
        {
            Location = new Point(12, 151),
            Size = new Size(980, 24),
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        progressBar = new ProgressBar
        {
            Location = new Point(-1, -1),
            Size = new Size(982, 26),
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        progressBarBorderPanel.Controls.Add(progressBar);

        progressLabel = new Label
        {
            Location = new Point(748, 151),
            Size = new Size(50, 24),
            Text = "0%",
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        // ── Thin separator line ───────────────────────────────────────────────
        var separator3 = new Panel
        {
            Location = new Point(12, 184),
            Size = new Size(980, 1),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // ── Output area ───────────────────────────────────────────────────────
        outputTextBox = new RichTextBox
        {
            Location = new Point(12, 192),
            Size = new Size(980, 440),
            ReadOnly = true,
            Font = new Font("Consolas", 9.5f),
            ScrollBars = RichTextBoxScrollBars.Both,
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.AddRange(new Control[] {
            fileLabel, filePathBorderPanel, browseButton, parseButton, cancelButton, openConfigButton,
            separator1,
            packetLabel, searchTextBox, comboBorderPanel,
            separator2,
            reparseButton, copyButton, openEditorButton, firstCraftButton, timeOrderButton,
            prevHighlightButton, highlightBorderPanel, nextHighlightButton,
            occurrenceLabel, prevPageButton, pageLabel, nextPageButton,
            progressBarBorderPanel, progressLabel,
            separator3,
            outputTextBox
        });

        this.Load += (s, e) =>
        {
            var useDark = 1;
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
            MainForm_Resize(null, EventArgs.Empty);
        };

        ApplyDarkTheme();

        this.Resize += MainForm_Resize;
    }

    private void ApplyDarkTheme()
    {
        var bgDark = Color.FromArgb(0x18, 0x18, 0x1A);
        var bgPanel = Color.FromArgb(0x22, 0x22, 0x25);
        var bgControl = Color.FromArgb(0x2C, 0x2C, 0x30);
        var bgButton = Color.FromArgb(0x35, 0x35, 0x3A);
        var bgButtonHover = Color.FromArgb(0x45, 0x45, 0x4C);
        var fgText = Color.FromArgb(0xE0, 0xE0, 0xE2);
        var fgDim = Color.FromArgb(0x90, 0x90, 0x98);
        var borderColor = Color.FromArgb(0x42, 0x42, 0x48);
        var borderAccent = Color.FromArgb(0x3A, 0x7F, 0xD4);
        var separatorColor = Color.FromArgb(0x30, 0x30, 0x35);

        BackColor = bgDark;
        ForeColor = fgText;
        Font = new Font("Segoe UI", 9.5f);

        void StyleLabel(Label l)
        {
            l.ForeColor = fgText;
            l.BackColor = Color.Transparent;
            l.Font = new Font("Segoe UI", 9.5f);
        }

        void StyleButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = bgButton;
            b.ForeColor = fgText;
            b.Font = new Font("Segoe UI", 9.5f);
            b.FlatAppearance.BorderColor = borderColor;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = bgButtonHover;
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(0x28, 0x28, 0x2E);
            b.Cursor = Cursors.Hand;

            // Handle enabled/disabled state colors
            b.EnabledChanged += (s, e) =>
            {
                if (b.Enabled)
                {
                    b.BackColor = bgButton;
                    b.ForeColor = fgText;
                }
                else
                {
                    b.BackColor = Color.FromArgb(0x28, 0x28, 0x2E);
                    b.ForeColor = fgDim;
                }
            };
        }

        foreach (var c in Controls)
        {
            if (c is Label l) StyleLabel(l);
            if (c is Panel p && p != filePathBorderPanel && p != comboBorderPanel)
                p.BackColor = separatorColor;
        }

        // Border panels act as 1-px coloured border around inputs
        filePathBorderPanel.BackColor = borderColor;
        comboBorderPanel.BackColor = borderColor;

        // TextBoxes with placeholder color support
        void StyleTextBox(TextBox tb)
        {
            tb.BackColor = bgControl;
            tb.ForeColor = fgText;
            tb.BorderStyle = BorderStyle.FixedSingle;
            // Handle placeholder text color via tag
            tb.Enter += (s, e) => tb.ForeColor = fgText;
            tb.TextChanged += (s, e) =>
            {
                if (string.IsNullOrEmpty(tb.Text))
                    tb.ForeColor = fgDim;
                else
                    tb.ForeColor = fgText;
            };
        }

        StyleTextBox(filePathTextBox);
        StyleTextBox(searchTextBox);
        StyleTextBox(highlightTextBox);

        // RichTextBox — slightly lighter background so it reads as a distinct surface
        outputTextBox.BackColor = bgPanel;
        outputTextBox.ForeColor = fgText;

        // Buttons
        StyleButton(browseButton);
        StyleButton(parseButton);
        StyleButton(cancelButton);
        StyleButton(openConfigButton);
        StyleButton(reparseButton);
        StyleButton(copyButton);
        StyleButton(openEditorButton);
        StyleButton(firstCraftButton);
        StyleButton(timeOrderButton);
        StyleButton(prevHighlightButton);
        StyleButton(nextHighlightButton);
        StyleButton(prevPageButton);
        StyleButton(nextPageButton);

        // Parse / Browse get a subtle blue accent border
        parseButton.FlatAppearance.BorderColor = borderAccent;
        browseButton.FlatAppearance.BorderColor = borderAccent;

        // Trigger EnabledChanged to set initial disabled button colors
        foreach (var btn in new[] { parseButton, reparseButton, copyButton, openEditorButton, firstCraftButton, timeOrderButton, cancelButton, prevHighlightButton, nextHighlightButton, prevPageButton, nextPageButton })
        {
            var savedEnabled = btn.Enabled;
            btn.Enabled = !savedEnabled;
            btn.Enabled = savedEnabled;
        }

        // ComboBox — dark theme styling
        packetComboBox.BackColor = bgControl;
        packetComboBox.ForeColor = fgText;

        // Progress bar border color
        progressBarBorderPanel.BackColor = borderColor;

        // Highlight textbox - blue accent border
        highlightBorderPanel.BackColor = borderAccent;

        // ProgressBar - darker background for better contrast
        progressBar.BackColor = bgDark;
        progressBar.ForeColor = borderAccent;

        // Occurrence / page labels
        occurrenceLabel.ForeColor = fgDim;
        pageLabel.ForeColor = fgDim;
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        int rightMargin = this.ClientSize.Width - 12;

        // Row 1 buttons (right-anchored)
        openConfigButton.Left = rightMargin - openConfigButton.Width;
        parseButton.Left = openConfigButton.Left - parseButton.Width - 8;
        cancelButton.Left = parseButton.Left;
        browseButton.Left = parseButton.Left - browseButton.Width - 8;

        filePathBorderPanel.Width = browseButton.Left - filePathBorderPanel.Left - 8;
        // TextBox overflows 1px on each side so its system border is hidden behind panel edges
        filePathTextBox.Width = filePathBorderPanel.Width + 2;

        // Separators span full width (exclude custom border panels)
        foreach (var c in Controls)
            if (c is Panel p && p != filePathBorderPanel && p != comboBorderPanel && p != progressBarBorderPanel && p != highlightBorderPanel)
                p.Width = rightMargin - p.Left;

        // Row 2: combo box spans to right margin; overflows 1px on each side to hide system border
        comboBorderPanel.Width = rightMargin - comboBorderPanel.Left;
        packetComboBox.Width = comboBorderPanel.Width + 2;

        // Progress bar spans full width with label on right
        progressBarBorderPanel.Width = rightMargin - progressBarBorderPanel.Left;
        progressBar.Width = progressBarBorderPanel.Width + 2;
        progressLabel.Left = rightMargin - progressLabel.Width;

        outputTextBox.Width = rightMargin - outputTextBox.Left;
        outputTextBox.Height = this.ClientSize.Height - outputTextBox.Top - 12;
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = "Packet files (*.pkt;*.bin)|*.pkt;*.bin|All files (*.*)|*.*",
            Title = "Select packet file"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            currentFilePath = openFileDialog.FileName;
            filePathTextBox.Text = currentFilePath;

            // Check if parsed file already exists
            var parsedFile = Path.ChangeExtension(currentFilePath, null) + "_parsed.txt";
            if (File.Exists(parsedFile))
            {
                var fileInfo = new FileInfo(parsedFile);
                var result = MessageBox.Show(
                    $"Found existing parsed file:\n{parsedFile}\n\n" +
                    $"Last modified: {fileInfo.LastWriteTime}\n\n" +
                    "Click 'Yes' to load the existing parsed file.\n" +
                    "Click 'No' to re-parse the PKT file.",
                    "Existing Parsed File Found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Load existing parsed file
                    LoadExistingParsedFile(parsedFile);
                    return;
                }
            }

            // Standard new file state
            parseButton.Enabled = true;
            reparseButton.Enabled = false;
            copyButton.Enabled = false;
            openEditorButton.Enabled = false;
            firstCraftButton.Enabled = false;
            timeOrderButton.Enabled = false;
            outputTextBox.Clear();
            allPackets.Clear();
            packetComboBox.Items.Clear();
            packetComboBox.Enabled = false;
            occurrenceLabel.Visible = false;
            highlightBorderPanel.Visible = false;
            prevHighlightButton.Visible = false;
            nextHighlightButton.Visible = false;
            highlightTextBox.Clear();
            HidePagination();
            isReparsing = false;
            currentPage = 0;
            pageBeforeReparse = 0;
        }
    }

    private void LoadExistingParsedFile(string parsedFile)
    {
        outputTextBox.Text = "Loading existing parsed file...\n";

        Task.Run(async () =>
        {
            try
            {
                var fileInfo = new FileInfo(parsedFile);
                var totalBytes = fileInfo.Length;
                var totalRead = 0L;

                using var fileStream = new FileStream(parsedFile, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(fileStream);

                var content = new System.Text.StringBuilder();
                var readBuffer = new char[4096];
                int bytesRead;

                while ((bytesRead = await reader.ReadAsync(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    content.Append(readBuffer, 0, bytesRead);
                    totalRead += bytesRead;

                    var progress = (int)((totalRead * 100) / totalBytes);
                    this.Invoke(() =>
                    {
                        outputTextBox.Text = $"Loading existing parsed file... {Math.Min(progress, 100)}%\n";
                    });
                }

                parsedContent = content.ToString();

                this.Invoke(() =>
                {
                    outputTextBox.Text = "Existing parsed file loaded. Select a packet to view.\n" +
                                        $"File date: {fileInfo.LastWriteTime}\n" +
                                        "Click 'Re-parse' to parse the PKT file again if needed.";

                    ExtractPackets(parsedContent);
                    UpdatePacketComboBox();

                    // Enable buttons - reparse is available since we have a PKT file selected
                    parseButton.Enabled = false; // Already loaded, use reparse instead
                    reparseButton.Enabled = true;
                    copyButton.Enabled = packetComboBox.Items.Count > 0;
                    openEditorButton.Enabled = true;
                    firstCraftButton.Enabled = packetComboBox.Items.Count > 0;
                    timeOrderButton.Enabled = packetComboBox.Items.Count > 0;
                    packetComboBox.Enabled = packetComboBox.Items.Count > 0;
                });
            }
            catch (Exception ex)
            {
                this.Invoke(() =>
                {
                    outputTextBox.Text = $"Error loading parsed file: {ex.Message}\n";
                    MessageBox.Show($"Error loading parsed file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        });
    }

    private void CopyButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(outputTextBox.Text))
        {
            Clipboard.SetText(outputTextBox.Text);
            var originalText = copyButton.Text;
            copyButton.Text = "Copied!";
            Task.Delay(1000).ContinueWith(_ =>
            {
                this.Invoke(() => copyButton.Text = originalText);
            });
        }
    }

    private void OpenEditorButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(currentFilePath))
            return;

        var parsedFile = Path.ChangeExtension(currentFilePath, null) + "_parsed.txt";

        if (!File.Exists(parsedFile))
        {
            MessageBox.Show($"Parsed file not found: {parsedFile}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = parsedFile,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenConfigButton_Click(object? sender, EventArgs e)
    {
        var possibleConfigPaths = new[]
        {
            Path.Combine(Application.StartupPath, "WowPacketParser.dll.config"),
            Path.Combine(Directory.GetCurrentDirectory(), "WowPacketParser.dll.config"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "WowPacketParser", "bin", "Release", "WowPacketParser.dll.config"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "WowPacketParser", "bin", "Debug", "net9.0", "WowPacketParser.dll.config"),
            @"C:\FluxPacketParser\WowPacketParser\bin\Release\WowPacketParser.dll.config"
        };

        string? configPath = null;
        foreach (var path in possibleConfigPaths)
        {
            if (File.Exists(path))
            {
                configPath = path;
                break;
            }
        }

        if (string.IsNullOrEmpty(configPath))
        {
            var result = MessageBox.Show(
                "WowPacketParser.dll.config not found in default locations.\n\nWould you like to locate it manually?",
                "Config File Not Found",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using var openFileDialog = new OpenFileDialog
                {
                    Filter = "Config files (*.config)|*.config|All files (*.*)|*.*",
                    Title = "Locate WowPacketParser.dll.config",
                    FileName = "WowPacketParser.dll.config"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    configPath = openFileDialog.FileName;
                }
            }
        }

        if (!string.IsNullOrEmpty(configPath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = configPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening config file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ReparseButton_Click(object? sender, EventArgs e)
    {
        selectedPacketBeforeReparse = packetComboBox.SelectedItem?.ToString();
        pageBeforeReparse = currentPage;
        isReparsing = true;
        ParseButton_Click(sender, e);
    }

    private void PrevPageButton_Click(object? sender, EventArgs e)
    {
        if (currentPage > 0)
        {
            currentPage--;
            DisplayCurrentPage();
        }
    }

    private void NextPageButton_Click(object? sender, EventArgs e)
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            DisplayCurrentPage();
        }
    }

    private void HighlightTextBox_TextChanged(object? sender, EventArgs e)
    {
        HighlightText(highlightTextBox.Text);
    }

    private void HighlightText(string searchText)
    {
        outputTextBox.SuspendLayout();

        try
        {
            // Clear previous highlights
            outputTextBox.SelectionStart = 0;
            outputTextBox.SelectionLength = outputTextBox.Text.Length;
            outputTextBox.SelectionColor = Color.FromArgb(0xE0, 0xE0, 0xE2);
            outputTextBox.SelectionBackColor = Color.FromArgb(0x22, 0x22, 0x25);

            highlightMatchPositions.Clear();
            currentHighlightIndex = -1;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                prevHighlightButton.Enabled = false;
                nextHighlightButton.Enabled = false;
                outputTextBox.ResumeLayout();
                return;
            }

            // Find and highlight all occurrences (case-insensitive)
            int startIndex = 0;
            while (true)
            {
                int index = outputTextBox.Text.IndexOf(searchText, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index < 0) break;

                highlightMatchPositions.Add(index);

                outputTextBox.Select(index, searchText.Length);
                outputTextBox.SelectionBackColor = Color.FromArgb(0xE5, 0xC0, 0x07);
                outputTextBox.SelectionColor = Color.FromArgb(0x18, 0x18, 0x1A);

                startIndex = index + searchText.Length;
            }

            outputTextBox.DeselectAll();

            prevHighlightButton.Enabled = highlightMatchPositions.Count > 0;
            nextHighlightButton.Enabled = highlightMatchPositions.Count > 0;
        }
        finally
        {
            outputTextBox.ResumeLayout();
        }
    }

    private void NavigateHighlight(int direction)
    {
        if (highlightMatchPositions.Count == 0 || string.IsNullOrWhiteSpace(highlightTextBox.Text))
            return;

        currentHighlightIndex += direction;
        if (currentHighlightIndex < 0)
            currentHighlightIndex = highlightMatchPositions.Count - 1;
        else if (currentHighlightIndex >= highlightMatchPositions.Count)
            currentHighlightIndex = 0;

        int pos = highlightMatchPositions[currentHighlightIndex];
        int len = highlightTextBox.Text.Length;

        outputTextBox.Select(pos, len);
        outputTextBox.ScrollToCaret();
    }

    private void PrevHighlightButton_Click(object? sender, EventArgs e)
    {
        NavigateHighlight(-1);
    }

    private void NextHighlightButton_Click(object? sender, EventArgs e)
    {
        NavigateHighlight(1);
    }

    private void HidePagination()
    {
        prevPageButton.Visible = false;
        prevPageButton.Enabled = false;
        nextPageButton.Visible = false;
        nextPageButton.Enabled = false;
        pageLabel.Visible = false;
    }

    private void UpdatePaginationButtons()
    {
        if (totalPages <= 1)
        {
            prevPageButton.Visible = false;
            prevPageButton.Enabled = false;
            nextPageButton.Visible = false;
            nextPageButton.Enabled = false;
            pageLabel.Visible = false;
            highlightBorderPanel.Visible = true;
            prevHighlightButton.Visible = true;
            nextHighlightButton.Visible = true;
            return;
        }

        prevPageButton.Visible = true;
        nextPageButton.Visible = true;
        pageLabel.Visible = true;
        highlightBorderPanel.Visible = true;
        prevHighlightButton.Visible = true;
        nextHighlightButton.Visible = true;

        prevPageButton.Enabled = currentPage > 0;
        nextPageButton.Enabled = currentPage < totalPages - 1;
        pageLabel.Text = $"{currentPage + 1} / {totalPages}";
    }

    private string ExtractPacketName(string displayText)
    {
        var match = Regex.Match(displayText, @"^\d{2}:\d{2}:\d{2}\.\d{3}\s+");
        return match.Success ? displayText.Substring(match.Length) : displayText;
    }

    private void DisplayCurrentPage()
    {
        var displayText = packetComboBox.SelectedItem?.ToString();
        if (displayText == null) return;
        var selectedPacket = ExtractPacketName(displayText);
        if (!packetLines.ContainsKey(selectedPacket))
            return;

        var occurrences = packetLines[selectedPacket];
        if (currentPage < 0 || currentPage >= occurrences.Count)
        {
            currentPage = 0;
            return;
        }

        var packetContent = string.Join("\n", occurrences[currentPage]);
        outputTextBox.Text = packetContent;

        outputTextBox.SelectionStart = 0;
        outputTextBox.ScrollToCaret();

        if (!string.IsNullOrWhiteSpace(highlightTextBox.Text))
        {
            HighlightText(highlightTextBox.Text);
        }

        UpdatePaginationButtons();
    }

    private async void ParseButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(currentFilePath)) return;

        parseButton.Enabled = false;
        parseButton.Visible = false;
        reparseButton.Enabled = false;
        copyButton.Enabled = false;
        openEditorButton.Enabled = false;
        cancelButton.Enabled = true;
        cancelButton.Visible = true;
        progressBar.Visible = true;
        progressBar.Value = 0;
        progressLabel.Visible = true;
        progressLabel.Text = "0%";
        occurrenceLabel.Visible = false;
        highlightBorderPanel.Visible = false;
        prevHighlightButton.Visible = false;
        nextHighlightButton.Visible = false;
        highlightTextBox.Clear();
        HidePagination();
        lastReportedProgress = -1;

        if (!isReparsing)
        {
            outputTextBox.Text = "Parsing...\n";
        }

        try
        {
            var wppPath = "";

            var possiblePaths = new[]
            {
                Path.Combine(Application.StartupPath, "WowPacketParser.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "WowPacketParser.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "WowPacketParser", "bin", "Release", "WowPacketParser.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "WowPacketParser", "bin", "Debug", "net9.0", "WowPacketParser.exe"),
                @"C:\FluxPacketParser\WowPacketParser\bin\Release\WowPacketParser.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    wppPath = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(wppPath))
            {
                using var openFileDialog = new OpenFileDialog
                {
                    Filter = "WowPacketParser.exe|WowPacketParser.exe|All files (*.*)|*.*",
                    Title = "Locate WowPacketParser.exe"
                };

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    outputTextBox.Text = "WowPacketParser.exe not found. Please locate it manually.";
                    return;
                }

                wppPath = openFileDialog.FileName;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = wppPath,
                Arguments = $"\"{currentFilePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            currentProcess = Process.Start(startInfo);
            if (currentProcess != null)
            {
                currentProcess.OutputDataReceived += OutputDataReceived;
                currentProcess.ErrorDataReceived += ErrorDataReceived;

                currentProcess.BeginOutputReadLine();
                currentProcess.BeginErrorReadLine();

                await currentProcess.WaitForExitAsync();

                if (!currentProcess.HasExited)
                {
                    await currentProcess.StandardInput.WriteLineAsync();
                    await currentProcess.WaitForExitAsync();
                }
                progressBar.Value = 95;
                progressLabel.Text = "95%";

                var parsedFile = Path.ChangeExtension(currentFilePath, null) + "_parsed.txt";
                if (File.Exists(parsedFile))
                {
                    if (!isReparsing)
                    {
                        outputTextBox.AppendText("\nLoading parsed data...\n");
                    }

                    await Task.Run(async () =>
                    {
                        var fileInfo = new FileInfo(parsedFile);
                        var totalBytes = fileInfo.Length;
                        var totalRead = 0L;

                        using var fileStream = new FileStream(parsedFile, FileMode.Open, FileAccess.Read);
                        using var reader = new StreamReader(fileStream);

                        var content = new System.Text.StringBuilder();
                        var readBuffer = new char[4096];
                        int bytesRead;

                        while ((bytesRead = await reader.ReadAsync(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            content.Append(readBuffer, 0, bytesRead);
                            totalRead += bytesRead * sizeof(char);

                            var fileProgress = (int)((totalRead * 5) / totalBytes);
                            var newProgress = Math.Min(95 + fileProgress, 100);
                            progressBar.Invoke(() =>
                            {
                                progressBar.Value = newProgress;
                                progressLabel.Text = $"{newProgress}%";
                            });
                        }

                        var parsedContent = content.ToString();
                        this.parsedContent = parsedContent;

                        if (!isReparsing)
                        {
                            outputTextBox.Invoke(() => outputTextBox.AppendText("Parsing complete. Select a packet to view.\n"));
                        }

                        ExtractPackets(parsedContent);
                        UpdatePacketComboBox();

                        progressBar.Invoke(() =>
                        {
                            progressBar.Value = 100;
                            progressLabel.Text = "100%";
                        });

                        if (isReparsing && !string.IsNullOrEmpty(selectedPacketBeforeReparse))
                        {
                            packetComboBox.Invoke(() =>
                            {
                                var index = packetComboBox.Items.IndexOf(selectedPacketBeforeReparse);
                                if (index >= 0)
                                {
                                    packetComboBox.SelectedIndex = index;

                                    var packetName = ExtractPacketName(selectedPacketBeforeReparse);
                                    var occurrences = packetLines[packetName];
                                    totalPages = occurrences.Count;

                                    currentPage = Math.Min(pageBeforeReparse, totalPages - 1);
                                    if (currentPage < 0) currentPage = 0;

                                    DisplayCurrentPage();
                                }
                            });
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            if (!isReparsing)
            {
                outputTextBox.AppendText($"\nError: {ex.Message}\n");
            }
        }
        finally
        {
            parseButton.Enabled = true;
            parseButton.Visible = true;
            reparseButton.Enabled = true;
            copyButton.Enabled = packetComboBox.Items.Count > 0;
            openEditorButton.Enabled = packetComboBox.Items.Count > 0;
            firstCraftButton.Enabled = packetComboBox.Items.Count > 0;
            timeOrderButton.Enabled = packetComboBox.Items.Count > 0;
            cancelButton.Enabled = false;
            cancelButton.Visible = false;
            progressBar.Visible = false;
            progressLabel.Visible = false;
            currentProcess = null;
            isReparsing = false;
            selectedPacketBeforeReparse = null;
            pageBeforeReparse = 0;
        }
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        if (currentProcess != null && !currentProcess.HasExited)
        {
            currentProcess.Kill();
            if (!isReparsing)
            {
                outputTextBox.AppendText("\nParsing cancelled.\n");
            }
        }
    }

    private void ExtractPackets(string output)
    {
        allPackets.Clear();
        packetLines.Clear();
        packetTimestamps.Clear();
        var lines = output.Split('\n');
        var packetRegex = new Regex(@"(ServerToClient|ClientToServer):\s+(\w+)\s+\(0x[0-9A-F]+\)");
        var timeRegex = new Regex(@"Time:\s+(\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2}\.\d{3})");

        string? currentPacket = null;
        var currentPacketLines = new List<string>();

        foreach (var line in lines)
        {
            var match = packetRegex.Match(line);
            if (match.Success)
            {
                if (currentPacket != null && currentPacketLines.Count > 0)
                {
                    if (!packetLines.ContainsKey(currentPacket))
                    {
                        packetLines[currentPacket] = new List<List<string>>();
                    }
                    packetLines[currentPacket].Add(new List<string>(currentPacketLines));
                }

                currentPacket = $"{match.Groups[1].Value}: {match.Groups[2].Value}";
                if (!allPackets.Contains(currentPacket))
                {
                    allPackets.Add(currentPacket);
                    var timeMatch = timeRegex.Match(line);
                    if (timeMatch.Success)
                        packetTimestamps[currentPacket] = timeMatch.Groups[1].Value;
                }

                currentPacketLines.Clear();
                currentPacketLines.Add(line);
            }
            else if (currentPacket != null)
            {
                currentPacketLines.Add(line);
            }
        }

        if (currentPacket != null && currentPacketLines.Count > 0)
        {
            if (!packetLines.ContainsKey(currentPacket))
            {
                packetLines[currentPacket] = new List<List<string>>();
            }
            packetLines[currentPacket].Add(new List<string>(currentPacketLines));
        }

        allPackets.Sort((a, b) =>
        {
            var ta = packetTimestamps.GetValueOrDefault(a, "");
            var tb = packetTimestamps.GetValueOrDefault(b, "");
            return string.Compare(ta, tb, StringComparison.Ordinal);
        });
    }

    private void UpdatePacketComboBox()
    {
        packetComboBox.Invoke(() =>
        {
            var previousSelection = packetComboBox.SelectedItem?.ToString();

            packetComboBox.Items.Clear();

            var searchTerm = searchTextBox.Text.ToLower();
            var filteredPackets = string.IsNullOrEmpty(searchTerm)
                ? allPackets
                : allPackets.Where(p => p.ToLower().Contains(searchTerm) ||
                                       p.Split(':')[1].Trim().ToLower().Contains(searchTerm)).ToList();

            foreach (var packet in filteredPackets)
            {
                string displayText;
                if (packetTimestamps.TryGetValue(packet, out var timestamp) && timestamp.Length >= 19)
                {
                    var timePart = timestamp.Substring(11, 12);
                    displayText = $"{timePart} {packet}";
                }
                else
                {
                    displayText = packet;
                }
                packetComboBox.Items.Add(displayText);
            }

            packetComboBox.Enabled = filteredPackets.Count > 0;

            if (!string.IsNullOrEmpty(previousSelection))
            {
                var index = packetComboBox.Items.IndexOf(previousSelection);
                if (index >= 0)
                {
                    packetComboBox.SelectedIndex = index;
                }
            }
        });
    }

    private void SearchTextBox_TextChanged(object? sender, EventArgs e)
    {
        UpdatePacketComboBox();
    }

    private void PacketComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (packetComboBox.SelectedItem == null) return;

        var displayText = packetComboBox.SelectedItem.ToString();
        if (displayText == null) return;
        var selectedPacket = ExtractPacketName(displayText);
        if (packetLines.ContainsKey(selectedPacket))
        {
            var occurrences = packetLines[selectedPacket];

            if (!isReparsing)
            {
                currentPage = 0;
            }
            totalPages = occurrences.Count;

            occurrenceLabel.Text = occurrences.Count == 1
                ? "1 occurrence"
                : $"{occurrences.Count} occurrences";
            occurrenceLabel.Visible = true;

            DisplayCurrentPage();

            if (totalPages > 0)
            {
                highlightBorderPanel.Visible = true;
                prevHighlightButton.Visible = true;
                nextHighlightButton.Visible = true;
            }

            copyButton.Enabled = true;
            openEditorButton.Enabled = true;
        }
    }

    private void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            this.Invoke((Action)(() =>
            {
                if (e.Data.StartsWith("Progress: ") && e.Data.EndsWith("%"))
                {
                    if (int.TryParse(e.Data.Substring(10, e.Data.Length - 11), out int percentage))
                    {
                        if (percentage != lastReportedProgress)
                        {
                            lastReportedProgress = percentage;
                            progressBar.Value = Math.Min(percentage, 100);
                            progressLabel.Text = $"{percentage}%";
                        }
                    }
                }
                else if (!isReparsing)
                {
                    outputTextBox.AppendText(e.Data + Environment.NewLine);
                    outputTextBox.SelectionStart = outputTextBox.Text.Length;
                    outputTextBox.ScrollToCaret();
                }
            }));
        }
    }

    private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data) && !isReparsing)
        {
            this.Invoke((Action)(() =>
            {
                outputTextBox.AppendText(e.Data + Environment.NewLine);
                outputTextBox.SelectionStart = outputTextBox.Text.Length;
                outputTextBox.ScrollToCaret();
            }));
        }
    }

    private void TimeOrderButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(parsedContent))
        {
            MessageBox.Show("No parsed data available. Please parse a file first.", "No Data",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var entries = new List<PacketTimeEntry>();
        var packetRegex = new Regex(
            @"^(ServerToClient|ClientToServer):\s+(\w+)\s+\((0x[0-9A-F]+)\).*Time:\s+" +
            @"(\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2}\.\d{3}).*Number:\s+(\d+)",
            RegexOptions.Multiline);

        foreach (Match m in packetRegex.Matches(parsedContent))
        {
            entries.Add(new PacketTimeEntry
            {
                Direction = m.Groups[1].Value,
                Name = m.Groups[2].Value,
                Opcode = m.Groups[3].Value,
                Time = m.Groups[4].Value,
                Number = m.Groups[5].Value
            });
        }

        if (entries.Count == 0)
        {
            MessageBox.Show("No packets found in the parsed data.", "No Data",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Already in time order since regex matches sequentially — but ensure ascending Number sort
        entries.Sort((a, b) =>
        {
            var cmp = string.Compare(a.Time, b.Time, StringComparison.Ordinal);
            if (cmp != 0) return cmp;
            return int.TryParse(a.Number, out var na) && int.TryParse(b.Number, out var nb)
                ? na.CompareTo(nb) : 0;
        });

        using var dialog = new PacketTimeOrderDialog(entries);
        dialog.ShowDialog(this);
    }

    private async void FirstCraftButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(parsedContent))
        {
            MessageBox.Show("No parsed data available. Please parse a file first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Show progress dialog for large files
        var progressForm = new Form
        {
            Text = "Extracting First Craft Treasures...",
            Size = new Size(400, 100),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ControlBox = false
        };
        var progressLabel = new Label
        {
            Text = "Scanning parsed content for First Craft treasures...",
            Location = new Point(20, 20),
            Size = new Size(360, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };
        progressForm.Controls.Add(progressLabel);

        // Show progress form non-blocking
        progressForm.Show(this);
        progressForm.Refresh();

        try
        {
            // Run extraction on background thread
            var treasures = await Task.Run(() => ExtractFirstCraftTreasures(parsedContent));

            progressForm.Close();

            using var dialog = new FirstCraftTreasureDialog(treasures);
            dialog.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error extracting treasures: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private List<FirstCraftTreasure> ExtractFirstCraftTreasures(string content)
    {
        var treasures = new List<FirstCraftTreasure>();
        var lines = content.Split('\n');

        string? currentPacket = null;
        string? currentPacketName = null;
        string? timestamp = null;
        var packetData = new Dictionary<string, string>();

        // Track spell casts to link to First Craft treasures
        string? lastPlayerSpell = null;
        int spellSearchStartIdx = 0;

        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            var line = lines[lineIdx];

            // Detect spell cast packets (SMSG_SPELL_GO indicates a spell was cast)
            if (line.Contains("SMSG_SPELL_GO") || line.Contains("SMSG_SPELL_START"))
            {
                spellSearchStartIdx = lineIdx + 1;
                bool isPlayerCaster = false;
                string? spellId = null;

                // Scan next 30 lines for spell info and caster
                for (int i = spellSearchStartIdx; i < Math.Min(spellSearchStartIdx + 30, lines.Length); i++)
                {
                    var spellLine = lines[i];

                    // Check for player caster
                    if (spellLine.Contains("CasterGUID:") || spellLine.Contains("CasterUnit:"))
                    {
                        if (spellLine.Contains("Player/"))
                            isPlayerCaster = true;
                    }

                    // Extract spell ID
                    if (spellLine.Contains("SpellID:"))
                    {
                        var spellMatch = Regex.Match(spellLine, @"SpellID:\s+(\d+)");
                        if (spellMatch.Success)
                        {
                            spellId = spellMatch.Groups[1].Value;
                        }
                        break;
                    }

                    // Stop if we hit another packet header
                    if (spellLine.Contains("ServerToClient:") || spellLine.Contains("ClientToServer:"))
                        break;
                }

                // Only update lastPlayerSpell if caster is a player
                if (isPlayerCaster && spellId != null)
                {
                    lastPlayerSpell = spellId;
                }
                continue;
            }

            // Detect SMSG_CRAFT_ENCHANT_RESULT for CraftingDataID
            if (line.Contains("SMSG_CRAFT_ENCHANT_RESULT"))
            {
                currentPacket = line;
                currentPacketName = "SMSG_CRAFT_ENCHANT_RESULT";

                // Extract timestamp
                var timeMatch = Regex.Match(line, @"Time:\s+(\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2}\.\d+)");
                if (timeMatch.Success)
                    timestamp = timeMatch.Groups[1].Value;
                continue;
            }

            // Detect packet header for First Craft related packets
            if (line.Contains("SMSG_ITEM_PUSH_RESULT") || line.Contains("SMSG_SET_CURRENCY"))
            {
                currentPacket = line;
                currentPacketName = line.Contains("SMSG_ITEM_PUSH_RESULT") ? "SMSG_ITEM_PUSH_RESULT" : "SMSG_SET_CURRENCY";

                // Extract timestamp
                var timeMatch = Regex.Match(line, @"Time:\s+(\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2}\.\d+)");
                if (timeMatch.Success)
                    timestamp = timeMatch.Groups[1].Value;

                packetData.Clear();
                packetData["Packet"] = currentPacketName;
                packetData["Timestamp"] = timestamp ?? "";
                packetData["SpellID"] = lastPlayerSpell ?? "0";
                continue;
            }

            if (currentPacket == null) continue;

            // Parse key-value pairs
            if (line.Contains(":"))
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    packetData[key] = value;
                }
            }

            // Check for FirstCraftOperationID - this is the trigger
            if (line.Contains("FirstCraftOperationID:"))
            {
                var opMatch = Regex.Match(line, @"FirstCraftOperationID:\s+(\d+)");
                if (opMatch.Success)
                {
                    var operationId = opMatch.Groups[1].Value;

                    // Determine type based on packet name, NOT field presence
                    string type;
                    string itemId;
                    string quantity;

                    if (currentPacketName == "SMSG_SET_CURRENCY")
                    {
                        // For SMSG_SET_CURRENCY - Type field contains CurrencyID
                        type = "Currency";
                        var typeValue = packetData.GetValueOrDefault("Type", "0");
                        var typeMatch = Regex.Match(typeValue, @"^(\d+)");
                        itemId = typeMatch.Success ? typeMatch.Groups[1].Value : "0";

                        // Check multiple possible quantity fields
                        var qtyValue = packetData.GetValueOrDefault("Quantity", "0");
                        if (qtyValue == "0" || qtyValue == "")
                            qtyValue = packetData.GetValueOrDefault("QuantityChange", "0");
                        var qtyMatch = Regex.Match(qtyValue, @"^(\d+)");
                        quantity = qtyMatch.Success ? qtyMatch.Groups[1].Value : "0";
                    }
                    else
                    {
                        // For SMSG_ITEM_PUSH_RESULT - try multiple field names for ItemID
                        type = "Item";
                        var itemValue = packetData.GetValueOrDefault("ItemID", "0");
                        if (itemValue == "0" || itemValue == "")
                            itemValue = packetData.GetValueOrDefault("Item ID", "0");
                        if (itemValue == "0" || itemValue == "")
                            itemValue = packetData.GetValueOrDefault("Item", "0");
                        var itemMatch = Regex.Match(itemValue, @"^(\d+)");
                        itemId = itemMatch.Success ? itemMatch.Groups[1].Value : "0";

                        var qtyValue = packetData.GetValueOrDefault("Quantity", "0");
                        var qtyMatch = Regex.Match(qtyValue, @"^(\d+)");
                        quantity = qtyMatch.Success ? qtyMatch.Groups[1].Value : "0";
                    }

                    treasures.Add(new FirstCraftTreasure
                    {
                        OperationID = operationId,
                        Type = type,
                        ItemID = itemId,
                        Quantity = quantity,
                        SourcePacket = packetData.GetValueOrDefault("Packet", ""),
                        Timestamp = packetData.GetValueOrDefault("Timestamp", ""),
                        SpellID = packetData.GetValueOrDefault("SpellID", "0"),
                    });
                }
            }
        }

        return treasures;
    }
}
