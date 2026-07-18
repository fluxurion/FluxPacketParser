using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    private List<QuestFlowEntry> entries;
    private uint questId;

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

        splitContainer = new SplitContainer
        {
            Location = new Point(12, 44),
            Size = new Size(960, 580),
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

        dataGridView.Rows.Clear();
        foreach (var entry in entries)
        {
            dataGridView.Rows.Add(entry.Number, entry.Time, entry.Direction, entry.Name, entry.Opcode);
        }

        if (dataGridView.Rows.Count > 0)
        {
            dataGridView.Rows[0].Selected = true;
            ShowDetail(0);
        }
    }

    private void DataGridView_SelectionChanged(object? sender, EventArgs e)
    {
        if (dataGridView.SelectedRows.Count == 0)
            return;
        var idx = dataGridView.SelectedRows[0].Index;
        ShowDetail(idx);
    }

    private void ShowDetail(int index)
    {
        if (index < 0 || index >= entries.Count)
        {
            detailTextBox.Clear();
            return;
        }
        detailTextBox.Text = entries[index].FullContent;
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
            var idx = row.Index;
            if (idx >= 0 && idx < entries.Count)
                lines.Add(entries[idx].FullContent);
        }

        if (lines.Count > 0)
        {
            Clipboard.SetText(string.Join("\n\n", lines));
            System.Media.SystemSounds.Beep.Play();
        }
    }
}
