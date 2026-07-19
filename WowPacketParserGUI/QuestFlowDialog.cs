using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WowPacketParserGUI;

public readonly struct QuestFlowEntry
{
    public string Number { get; init; }
    public string Time { get; init; }
    public string Direction { get; init; }
    public string Opcode { get; init; }
    public string Name { get; init; }
    public string FullContent { get; init; }
}

public class QuestFlowDialog : Form
{
    private DataGridView dataGridView = null!;
    private RichTextBox detailTextBox = null!;
    private Button copyButton = null!;
    private Button closeButton = null!;
    private Label summaryLabel = null!;
    private SplitContainer splitContainer = null!;
    private TextBox filterTextBox = null!;
    private Label legendLabel = null!;
    private List<QuestFlowEntry> entries;
    private uint questId;
    private HashSet<string> questGiverGuids = new();
    private HashSet<string> playerGuids = new();

    private static readonly Color RowQuestGiver = Color.FromArgb(0x1E, 0x33, 0x1E);
    private static readonly Color RowPlayer = Color.FromArgb(0x1E, 0x28, 0x3A);
    private static readonly Color RowBoth = Color.FromArgb(0x2E, 0x1E, 0x33);
    private static readonly Color RowQuestGiverSel = Color.FromArgb(0x2A, 0x5A, 0x2A);
    private static readonly Color RowPlayerSel = Color.FromArgb(0x2A, 0x44, 0x5E);
    private static readonly Color RowBothSel = Color.FromArgb(0x44, 0x2A, 0x4E);

    public QuestFlowDialog(List<QuestFlowEntry> entries, uint questId)
    {
        this.entries = entries;
        this.questId = questId;
        InitializeComponent();
        ApplyDarkTheme();
        LoadData();
    }

    private void InitializeComponent()
    {
        Text = $"Quest Flow - Quest ID {questId}";
        Size = new Size(1000, 700);
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        Padding = new Padding(12, 10, 12, 10);

        summaryLabel = new Label
        {
            Location = new Point(12, 12),
            Size = new Size(960, 23),
            TextAlign = ContentAlignment.MiddleLeft,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // Filter bar
        var filterLabel = new Label
        {
            Text = "Filter:",
            Location = new Point(12, 44),
            Size = new Size(50, 28),
            TextAlign = ContentAlignment.MiddleLeft,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        filterTextBox = new TextBox
        {
            Location = new Point(66, 44),
            Size = new Size(906, 28),
            PlaceholderText = "Filter by packet name, opcode, or content...",
            Font = new Font("Segoe UI", 9.5f),
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        filterTextBox.TextChanged += FilterTextBox_TextChanged;

        legendLabel = new Label
        {
            Location = new Point(12, 78),
            Size = new Size(960, 20),
            TextAlign = ContentAlignment.MiddleLeft,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        splitContainer = new SplitContainer
        {
            Location = new Point(12, 104),
            Size = new Size(960, 520),
            Orientation = Orientation.Horizontal,
            SplitterDistance = 280,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            SplitterWidth = 5,
            Panel1MinSize = 100,
            Panel2MinSize = 100
        };

        dataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false
        };

        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Number",
            HeaderText = "#",
            FillWeight = 8,
            MinimumWidth = 55
        });
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Time",
            HeaderText = "Time",
            FillWeight = 18,
            MinimumWidth = 130
        });
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Direction",
            HeaderText = "Dir",
            FillWeight = 8,
            MinimumWidth = 55
        });
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "Packet Name",
            FillWeight = 46,
            MinimumWidth = 280
        });
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Opcode",
            HeaderText = "Opcode",
            FillWeight = 14,
            MinimumWidth = 90
        });

        dataGridView.SelectionChanged += DataGridView_SelectionChanged;

        detailTextBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Font = new Font("Consolas", 9f),
            BorderStyle = BorderStyle.None
        };

        splitContainer.Panel1.Controls.Add(dataGridView);
        splitContainer.Panel2.Controls.Add(detailTextBox);

        copyButton = new Button
        {
            Text = "Copy All",
            Location = new Point(12, 638),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        copyButton.Click += CopyButton_Click;

        closeButton = new Button
        {
            Text = "Close",
            Location = new Point(872, 638),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        closeButton.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            summaryLabel,
            filterLabel,
            filterTextBox,
            legendLabel,
            splitContainer,
            copyButton,
            closeButton
        });

        KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.C)
                CopySelectedToClipboard();
            if (e.KeyCode == Keys.Escape)
                Close();
        };
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

        BackColor = bgDark;
        ForeColor = fgText;

        summaryLabel.ForeColor = fgText;
        summaryLabel.BackColor = Color.Transparent;
        summaryLabel.Font = new Font("Segoe UI", 9.5f);

        filterTextBox.BackColor = bgControl;
        filterTextBox.ForeColor = fgText;
        filterTextBox.BorderStyle = BorderStyle.FixedSingle;

        legendLabel.ForeColor = fgDim;
        legendLabel.BackColor = Color.Transparent;
        legendLabel.Font = new Font("Segoe UI", 8.5f);
        legendLabel.Text = "";

        splitContainer.BackColor = bgDark;
        splitContainer.Panel1.BackColor = bgDark;
        splitContainer.Panel2.BackColor = bgDark;

        dataGridView.BackgroundColor = bgPanel;
        dataGridView.DefaultCellStyle.BackColor = bgPanel;
        dataGridView.DefaultCellStyle.ForeColor = fgText;
        dataGridView.DefaultCellStyle.SelectionBackColor = borderAccent;
        dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
        dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
        dataGridView.GridColor = Color.FromArgb(0x30, 0x30, 0x35);
        dataGridView.ColumnHeadersDefaultCellStyle.BackColor = bgControl;
        dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = fgText;
        dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dataGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = bgControl;
        dataGridView.EnableHeadersVisualStyles = false;
        dataGridView.BorderStyle = BorderStyle.None;

        detailTextBox.BackColor = bgPanel;
        detailTextBox.ForeColor = fgText;

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
        }

        StyleButton(copyButton);
        StyleButton(closeButton);
        copyButton.FlatAppearance.BorderColor = borderAccent;
    }

    private void LoadData()
    {
        var acceptIdx = entries.FindIndex(e =>
            e.Name == "CMSG_QUEST_GIVER_ACCEPT_QUEST");
        var completeIdx = entries.FindLastIndex(e =>
            e.Name == "SMSG_QUEST_GIVER_QUEST_COMPLETE");

        string header;
        if (entries.Count == 0)
        {
            header = $"Quest ID {questId}: no matching packet sequence found.";
        }
        else
        {
            var range = (acceptIdx >= 0 && completeIdx >= 0)
                ? $"(ACCEPT at #{entries[acceptIdx].Number} → COMPLETE at #{entries[completeIdx].Number})"
                : "(partial — one or both endpoints missing)";
            header = $"Quest ID {questId} — {entries.Count} packets {range}";
        }

        summaryLabel.Text = header;

        ExtractGuids();

        UpdateLegend();

        dataGridView.Rows.Clear();
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var rowIdx = dataGridView.Rows.Add(entry.Number, entry.Time, entry.Direction, entry.Name, entry.Opcode);
            dataGridView.Rows[rowIdx].Tag = i;
            ApplyRowHighlight(dataGridView.Rows[rowIdx], entry.FullContent);
        }

        if (dataGridView.Rows.Count > 0)
        {
            dataGridView.Rows[0].Selected = true;
            ShowDetailForRow(0);
        }
    }

    private void DataGridView_SelectionChanged(object? sender, EventArgs e)
    {
        if (dataGridView.SelectedRows.Count == 0)
            return;
        ShowDetailForRow(dataGridView.SelectedRows[0].Index);
    }

    private void ShowDetailForRow(int rowIdx)
    {
        if (rowIdx < 0 || rowIdx >= dataGridView.Rows.Count)
        {
            detailTextBox.Clear();
            return;
        }
        var tag = dataGridView.Rows[rowIdx].Tag as int?;
        if (tag is int entryIdx && entryIdx >= 0 && entryIdx < entries.Count)
            detailTextBox.Text = entries[entryIdx].FullContent;
        else
            detailTextBox.Clear();
    }

    private void FilterTextBox_TextChanged(object? sender, EventArgs e)
    {
        var filter = filterTextBox.Text.Trim();
        if (string.IsNullOrEmpty(filter))
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
                row.Visible = true;
            return;
        }

        var filterLower = filter.ToLowerInvariant();
        foreach (DataGridViewRow row in dataGridView.Rows)
        {
            var tag = row.Tag as int?;
            if (tag is int entryIdx && entryIdx >= 0 && entryIdx < entries.Count)
            {
                var entry = entries[entryIdx];
                row.Visible = entry.Name.ToLowerInvariant().Contains(filterLower) ||
                              entry.Opcode.ToLowerInvariant().Contains(filterLower) ||
                              entry.Direction.ToLowerInvariant().Contains(filterLower) ||
                              entry.FullContent.ToLowerInvariant().Contains(filterLower);
            }
        }
    }

    private void ExtractGuids()
    {
        questGiverGuids.Clear();
        playerGuids.Clear();

        var guidRegex = new Regex(@"Full:\s*0x([0-9A-Fa-f]+)\s+(\w+)", RegexOptions.Multiline);

        foreach (var entry in entries)
        {
            foreach (Match m in guidRegex.Matches(entry.FullContent))
            {
                var hex = m.Groups[1].Value;
                var type = m.Groups[2].Value;
                if (type.Equals("Creature", StringComparison.OrdinalIgnoreCase))
                    questGiverGuids.Add(hex);
                else if (type.Equals("Player", StringComparison.OrdinalIgnoreCase))
                    playerGuids.Add(hex);
            }
        }
    }

    private void ApplyRowHighlight(DataGridViewRow row, string content)
    {
        bool hasQuestGiver = false;
        bool hasPlayer = false;

        foreach (var guid in questGiverGuids)
        {
            if (content.Contains(guid, StringComparison.OrdinalIgnoreCase))
            {
                hasQuestGiver = true;
                break;
            }
        }

        foreach (var guid in playerGuids)
        {
            if (content.Contains(guid, StringComparison.OrdinalIgnoreCase))
            {
                hasPlayer = true;
                break;
            }
        }

        var (bg, selBg) = (hasQuestGiver, hasPlayer) switch
        {
            (true, true)   => (RowBoth, RowBothSel),
            (true, false)  => (RowQuestGiver, RowQuestGiverSel),
            (false, true)  => (RowPlayer, RowPlayerSel),
            _              => (Color.Empty, Color.Empty)
        };

        if (bg != Color.Empty)
        {
            row.DefaultCellStyle.BackColor = bg;
            row.DefaultCellStyle.SelectionBackColor = selBg;
        }
    }

    private void UpdateLegend()
    {
        var parts = new List<string>();
        if (questGiverGuids.Count > 0)
            parts.Add($"\u25A0 Questgiver NPC ({questGiverGuids.Count} GUID)");
        if (playerGuids.Count > 0)
            parts.Add($"\u25A0 Player ({playerGuids.Count} GUID)");
        if (questGiverGuids.Count > 0 && playerGuids.Count > 0)
            parts.Add("\u25A0 Both");

        legendLabel.Text = parts.Count > 0 ? string.Join("    ", parts) : "";
    }

    private void CopyButton_Click(object? sender, EventArgs e)
    {
        CopySelectedToClipboard();
    }

    private void CopySelectedToClipboard()
    {
        if (dataGridView.Rows.Count == 0)
            return;

        var rows = dataGridView.SelectedRows.Count > 0
            ? dataGridView.SelectedRows.Cast<DataGridViewRow>()
            : dataGridView.Rows.Cast<DataGridViewRow>();

        var lines = new List<string>();
        foreach (DataGridViewRow row in rows)
        {
            var tag = row.Tag as int?;
            if (tag is int entryIdx && entryIdx >= 0 && entryIdx < entries.Count)
                lines.Add(entries[entryIdx].FullContent);
        }

        if (lines.Count > 0)
        {
            Clipboard.SetText(string.Join("\n\n", lines));
            System.Media.SystemSounds.Beep.Play();
        }
    }
}
