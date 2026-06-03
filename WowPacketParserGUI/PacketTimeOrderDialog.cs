using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WowPacketParserGUI;

public readonly struct PacketTimeEntry
{
    public string Number { get; init; }
    public string Time { get; init; }
    public string Direction { get; init; }
    public string Opcode { get; init; }
    public string Name { get; init; }
}

public class PacketTimeOrderDialog : Form
{
    private DataGridView dataGridView = null!;
    private Button copyButton = null!;
    private Button closeButton = null!;
    private Label summaryLabel = null!;
    private List<PacketTimeEntry> entries;

    public PacketTimeOrderDialog(List<PacketTimeEntry> entries)
    {
        this.entries = entries;
        InitializeComponent();
        ApplyDarkTheme();
        LoadData();
    }

    private void InitializeComponent()
    {
        Text = "Packets in Time Order";
        Size = new Size(900, 600);
        MinimumSize = new Size(650, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        Padding = new Padding(12, 10, 12, 10);

        // Summary label
        summaryLabel = new Label
        {
            Location = new Point(12, 12),
            Size = new Size(500, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // DataGridView
        dataGridView = new DataGridView
        {
            Location = new Point(12, 44),
            Size = new Size(860, 460),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
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

        // Copy button
        copyButton = new Button
        {
            Text = "Copy All",
            Location = new Point(12, 518),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        copyButton.Click += CopyButton_Click;

        // Close button
        closeButton = new Button
        {
            Text = "Close",
            Location = new Point(772, 518),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        closeButton.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            summaryLabel,
            dataGridView,
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

        dataGridView.BackgroundColor = bgPanel;
        dataGridView.DefaultCellStyle.BackColor = bgPanel;
        dataGridView.DefaultCellStyle.ForeColor = fgText;
        dataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0x3A, 0x7F, 0xD4);
        dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
        dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
        dataGridView.GridColor = Color.FromArgb(0x30, 0x30, 0x35);
        dataGridView.ColumnHeadersDefaultCellStyle.BackColor = bgControl;
        dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = fgText;
        dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dataGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = bgControl;
        dataGridView.EnableHeadersVisualStyles = false;
        dataGridView.BorderStyle = BorderStyle.None;

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
        summaryLabel.Text = entries.Count == 1
            ? "1 packet found"
            : $"{entries.Count} packets found";

        dataGridView.Rows.Clear();
        foreach (var entry in entries)
        {
            dataGridView.Rows.Add(entry.Number, entry.Time, entry.Direction, entry.Name, entry.Opcode);
        }
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
            var cells = row.Cells;
            lines.Add(string.Join("\t",
                cells[0].Value,
                cells[1].Value,
                cells[2].Value,
                cells[3].Value,
                cells[4].Value));
        }

        if (lines.Count > 0)
        {
            Clipboard.SetText(string.Join("\n", lines));
            System.Media.SystemSounds.Beep.Play();
        }
    }
}
