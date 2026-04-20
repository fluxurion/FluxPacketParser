using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace WowPacketParserGUI;

public class FirstCraftTreasureDialog : Form
{
    private DataGridView dataGridView = null!;
    private Button closeButton = null!;
    private Button exportSqlButton = null!;
    private Label summaryLabel = null!;
    private WebView2 tooltipWebView = null!;
    private Form tooltipForm = null!;
    private List<FirstCraftTreasure> treasures = new();

    public FirstCraftTreasureDialog(List<FirstCraftTreasure> treasures)
    {
        this.treasures = treasures;
        InitializeComponent();
        InitializeTooltipWebView();
        LoadData();
    }

    private void InitializeComponent()
    {
        Text = "First Craft Treasures by Spell";
        Size = new Size(750, 550);
        MinimumSize = new Size(550, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;

        // Summary label
        summaryLabel = new Label
        {
            Location = new Point(12, 12),
            Size = new Size(500, 23),
            Text = "No First Craft treasures found.",
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };

        // DataGridView
        dataGridView = new DataGridView
        {
            Location = new Point(12, 40),
            Size = new Size(710, 400),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Define columns - grouped by SpellID showing both rewards
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SpellID",
            HeaderText = "Spell ID",
            DataPropertyName = "SpellID",
            Width = 80,
            FillWeight = 12
        });
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CurrencyInfo",
            HeaderText = "Currency Reward",
            DataPropertyName = "CurrencyInfo",
            Width = 220,
            FillWeight = 38
        });
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ItemInfo",
            HeaderText = "Item Reward",
            DataPropertyName = "ItemInfo",
            Width = 220,
            FillWeight = 38
        });
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Time",
            HeaderText = "Time",
            DataPropertyName = "Time",
            Width = 100,
            FillWeight = 12
        });
        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "OperationID",
            HeaderText = "Operation ID",
            DataPropertyName = "OperationID",
            Width = 100,
            FillWeight = 12
        });

        // Add mouse events for tooltips
        dataGridView.CellMouseEnter += DataGridView_CellMouseEnter;
        dataGridView.CellMouseLeave += DataGridView_CellMouseLeave;
        dataGridView.CellClick += DataGridView_CellClick;

        // Export SQL button
        exportSqlButton = new Button
        {
            Text = "Export SQL",
            Location = new Point(12, 455),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Enabled = false
        };
        exportSqlButton.Click += ExportSqlButton_Click;

        // Close button
        closeButton = new Button
        {
            Text = "Close",
            Location = new Point(622, 455),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        closeButton.Click += (s, e) => Close();

        Controls.AddRange(new Control[]
        {
            summaryLabel,
            dataGridView,
            exportSqlButton,
            closeButton
        });
    }

    private void InitializeTooltipWebView()
    {
        // Create tooltip popup form with fixed size
        tooltipForm = new Form
        {
            Size = new Size(320, 100),
            StartPosition = FormStartPosition.Manual,
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            TopMost = true,
            Visible = false,
            BackColor = Color.FromArgb(30, 30, 30)
        };

        // Add a panel with border
        var borderPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(50, 50, 50),
            Padding = new Padding(1)
        };

        tooltipWebView = new WebView2
        {
            Dock = DockStyle.Fill
        };

        borderPanel.Controls.Add(tooltipWebView);
        tooltipForm.Controls.Add(borderPanel);

        // Initialize WebView2
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await tooltipWebView.EnsureCoreWebView2Async(null);
    }

    private void DataGridView_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

        var row = dataGridView.Rows[e.RowIndex];
        string? tooltipType = null;
        string? id = null;

        // Determine what type of ID we're hovering over
        if (e.ColumnIndex == 0) // SpellID column
        {
            id = row.Cells["SpellID"].Value?.ToString();
            if (id != null && id != "0" && id != "None")
                tooltipType = "spell";
        }
        else if (e.ColumnIndex == 1) // CurrencyInfo column
        {
            var currencyText = row.Cells["CurrencyInfo"].Value?.ToString();
            id = ExtractIdFromInfo(currencyText);
            if (id != null)
                tooltipType = "currency";
        }
        else if (e.ColumnIndex == 2) // ItemInfo column
        {
            var itemText = row.Cells["ItemInfo"].Value?.ToString();
            id = ExtractIdFromInfo(itemText);
            if (id != null)
                tooltipType = "item";
        }

        if (tooltipType != null && id != null)
        {
            ShowTooltip(e.RowIndex, e.ColumnIndex, tooltipType, id);
        }
    }

    private void DataGridView_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
    {
        HideTooltip();
    }

    private void DataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        // Open Wowhead page in browser on click
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

        var row = dataGridView.Rows[e.RowIndex];
        string? urlType = null;
        string? id = null;

        if (e.ColumnIndex == 0) // SpellID
        {
            id = row.Cells["SpellID"].Value?.ToString();
            if (id != null && id != "0" && id != "None")
                urlType = "spell";
        }
        else if (e.ColumnIndex == 1) // Currency
        {
            var text = row.Cells["CurrencyInfo"].Value?.ToString();
            id = ExtractIdFromInfo(text);
            if (id != null)
                urlType = "currency";
        }
        else if (e.ColumnIndex == 2) // Item
        {
            var text = row.Cells["ItemInfo"].Value?.ToString();
            id = ExtractIdFromInfo(text);
            if (id != null)
                urlType = "item";
        }

        if (urlType != null && id != null)
        {
            var url = $"https://www.wowhead.com/{urlType}={id}";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
    }

    private string? ExtractIdFromInfo(string? info)
    {
        if (string.IsNullOrEmpty(info) || info == "None") return null;
        // Extract ID from "ID: 1234, Qty: 5" format
        var match = System.Text.RegularExpressions.Regex.Match(info, @"ID:\s*(\d+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private void ShowTooltip(int rowIndex, int colIndex, string type, string id)
    {
        if (tooltipWebView.CoreWebView2 == null) return;

        // Position tooltip near the cell
        var cellRect = dataGridView.GetCellDisplayRectangle(colIndex, rowIndex, true);
        var screenPos = dataGridView.PointToScreen(new Point(cellRect.X, cellRect.Y + cellRect.Height));

        // Keep tooltip on screen
        var tooltipX = Math.Min(screenPos.X, Screen.PrimaryScreen!.WorkingArea.Width - tooltipForm.Width);
        var tooltipY = screenPos.Y + 5;
        if (tooltipY + tooltipForm.Height > Screen.PrimaryScreen.WorkingArea.Height)
        {
            tooltipY = screenPos.Y - tooltipForm.Height - 5;
        }

        tooltipForm.Location = new Point(tooltipX, tooltipY);

        // Generate HTML with Wowhead tooltip - white text color, fixed size
        var html = GenerateTooltipHtml(type, id);
        tooltipWebView.NavigateToString(html);
        tooltipForm.Visible = true;
    }

    private async Task AutoSizeTooltipAsync()
    {
        try
        {
            // Wait for Wowhead script to render
            await Task.Delay(500);

            if (!tooltipForm.Visible) return;

            // Try to get the tooltip dimensions from the DOM
            var result = await tooltipWebView.ExecuteScriptAsync(@"
                (function() {
                    var tooltip = document.querySelector('.wowhead-tooltip, .q, .q1, .q2, .q3, .q4');
                    if (tooltip) {
                        return JSON.stringify({
                            width: tooltip.offsetWidth + 20,
                            height: tooltip.offsetHeight + 20
                        });
                    }
                    var body = document.body;
                    return JSON.stringify({
                        width: body.scrollWidth + 20,
                        height: body.scrollHeight + 20
                    });
                })()
            ");

            // Parse the result
            var cleanResult = result.Replace("\"", "").Replace("{", "").Replace("}", "");
            var parts = cleanResult.Split(',');
            if (parts.Length == 2)
            {
                var widthStr = parts[0].Split(':')[1].Trim();
                var heightStr = parts[1].Split(':')[1].Trim();

                if (int.TryParse(widthStr, out int width) && int.TryParse(heightStr, out int height))
                {
                    width = Math.Max(width, 200);
                    width = Math.Min(width, 400);
                    height = Math.Max(height, 60);
                    height = Math.Min(height, 300);

                    tooltipForm.Invoke(() =>
                    {
                        tooltipForm.Size = new Size(width, height);
                    });
                }
            }
        }
        catch { /* Ignore resize errors */ }
    }

    private void HideTooltip()
    {
        tooltipForm.Visible = false;
    }

    private string GenerateTooltipHtml(string type, string id)
    {
        var wowheadLink = $"https://www.wowhead.com/{type}={id}";

        // HTML with white text color and fixed size
        var html = "<!DOCTYPE html>\n" +
            "<html>\n" +
            "<head>\n" +
            "    <style>\n" +
            "        body { margin: 0; padding: 8px; background: #1a1a1a; color: #ffffff; font-family: Arial, sans-serif; font-size: 12px; overflow: hidden; }\n" +
            "        #tooltip-container { display: inline-block; color: #ffffff; }\n" +
            "        a { color: #ffffff; text-decoration: none; }\n" +
            "        .wowhead-tooltip { color: #ffffff; }\n" +
            "    </style>\n" +
            "    <script>\n" +
            "        const whTooltips = { colorLinks: true, iconizeLinks: true, renameLinks: true };\n" +
            "    </script>\n" +
            "    <script src=\"https://wow.zamimg.com/js/tooltips.js\"></script>\n" +
            "</head>\n" +
            "<body>\n" +
            "    <div id=\"tooltip-container\">\n" +
            "        <a href=\"" + wowheadLink + "\" data-wowhead=\"" + type + "=" + id + "\"></a>\n" +
            "    </div>\n" +
            "</body>\n" +
            "</html>";

        return html;
    }

    private void LoadData()
    {
        if (treasures.Count == 0)
            return;

        dataGridView.Rows.Clear();

        // Group by SpellID and OperationID
        var groupedBySpell = treasures
            .Where(t => t.SpellID != "0")
            .GroupBy(t => new { t.SpellID, t.OperationID })
            .OrderBy(g => g.Key.SpellID);

        foreach (var group in groupedBySpell)
        {
            var spellId = group.Key.SpellID;
            var operationId = group.Key.OperationID;

            // Find currency and item for this spell
            var currency = group.FirstOrDefault(t => t.Type == "Currency");
            var item = group.FirstOrDefault(t => t.Type == "Item");

            string currencyInfo = currency != null
                ? $"ID: {currency.ItemID}, Qty: {currency.Quantity}"
                : "None";

            string itemInfo = item != null
                ? $"ID: {item.ItemID}, Qty: {item.Quantity}"
                : "None";

            // Format time from timestamp (extract HH:MM)
            var timeStr = group.FirstOrDefault()?.Timestamp ?? "";
            if (timeStr.Length >= 19)
            {
                // Format: 03/29/2026 14:10:21.741 -> extract 14:10
                var timeMatch = Regex.Match(timeStr, @"(\d{2}):(\d{2}):");
                if (timeMatch.Success)
                    timeStr = $"{timeMatch.Groups[1].Value}:{timeMatch.Groups[2].Value}";
                else
                    timeStr = "";
            }
            else
                timeStr = "";

            dataGridView.Rows.Add(
                spellId,
                currencyInfo,
                itemInfo,
                timeStr,
                operationId
            );
        }

        var uniqueSpells = groupedBySpell.Count();
        summaryLabel.Text = $"Found {uniqueSpells} First Craft spell(s) with rewards.";
        exportSqlButton.Enabled = true;
    }

    private void ExportSqlButton_Click(object? sender, EventArgs e)
    {
        using var saveDialog = new SaveFileDialog
        {
            Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*",
            Title = "Export First Craft Treasures to SQL",
            FileName = "treasure_loot_template.sql"
        };

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var sql = GenerateSql();
                File.WriteAllText(saveDialog.FileName, sql);
                var count = treasures.Where(t => t.SpellID != "0").Select(t => t.SpellID).Distinct().Count();
                MessageBox.Show($"Exported {count} entries to {saveDialog.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private string GenerateSql()
    {
        var sql = "-- First Craft Treasures - treasure_loot_template export\n";
        sql += "-- Generated by WowPacketParserGUI\n\n";
        sql += "DROP TABLE IF EXISTS `treasure_loot_template`;\n";
        sql += "CREATE TABLE `treasure_loot_template` (\n";
        sql += "  `TreasureID` INT UNSIGNED NOT NULL COMMENT 'FirstCraftTreasureID from CraftingData.db2',\n";
        sql += "  `Item` INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Item ID (for item rewards)',\n";
        sql += "  `Currency` INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Currency ID (for currency rewards)',\n";
        sql += "  `Chance` FLOAT NOT NULL DEFAULT 100 COMMENT 'Drop chance (100 = 100%)',\n";
        sql += "  `GroupID` INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Loot group identifier',\n";
        sql += "  `MinCount` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Minimum quantity',\n";
        sql += "  `MaxCount` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Maximum quantity',\n";
        sql += "  `Comment` VARCHAR(255) NULL DEFAULT '' COMMENT 'Optional comment',\n";
        sql += "  PRIMARY KEY (`TreasureID`, `Item`, `Currency`)\n";
        sql += ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='First Craft Treasure Loot Template';\n\n";

        // Group by SpellID
        var groupedBySpell = treasures
            .Where(t => t.SpellID != "0")
            .GroupBy(t => new { t.SpellID, t.OperationID })
            .OrderBy(g => g.Key.SpellID);

        foreach (var group in groupedBySpell)
        {
            var spellId = group.Key.SpellID;

            sql += $"-- Spell ID: {spellId}\n";
            sql += "SET @TreasureID = 0;\n";
            sql += "SET @Comment = '';\n";

            // Collect all valid treasures for this spell
            var values = new List<string>();
            foreach (var treasure in group.OrderBy(t => t.Type))
            {
                if (treasure.Type == "Item" && treasure.ItemID != "0")
                {
                    var minCount = treasure.Quantity;
                    var maxCount = treasure.Quantity;
                    values.Add($"  (@TreasureID, {treasure.ItemID}, 0, 100, 0, {minCount}, {maxCount}, @Comment)");
                }
                else if (treasure.Type == "Currency" && treasure.ItemID != "0")
                {
                    var minCount = treasure.Quantity;
                    var maxCount = treasure.Quantity;
                    values.Add($"  (@TreasureID, 0, {treasure.ItemID}, 100, 0, {minCount}, {maxCount}, @Comment)");
                }
            }

            // Generate single INSERT with all values
            if (values.Count > 0)
            {
                sql += "INSERT INTO `treasure_loot_template`\n";
                sql += "  (`TreasureID`, `Item`, `Currency`, `Chance`, `GroupID`, `MinCount`, `MaxCount`, `Comment`)\n";
                sql += "VALUES\n";
                sql += string.Join(",\n", values);
                sql += "; -- Update TreasureID from CraftingData.db2\n";
            }
            sql += "\n";
        }

        return sql;
    }
}

public class FirstCraftTreasure
{
    public string SpellID { get; set; } = "0";
    public string OperationID { get; set; } = "";
    public string Type { get; set; } = "";
    public string ItemID { get; set; } = "";
    public string Quantity { get; set; } = "";
    public string SourcePacket { get; set; } = "";
    public string Timestamp { get; set; } = "";
}
