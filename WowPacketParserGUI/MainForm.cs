using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Drawing;

namespace WowPacketParserGUI;

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
    private Button prevPageButton = null!;
    private Button nextPageButton = null!;
    private TextBox highlightTextBox = null!;
    private ComboBox packetComboBox = null!;
    private TextBox searchTextBox = null!;
    private RichTextBox outputTextBox = null!;
    private ProgressBar progressBar = null!;
    private Label progressLabel = null!;
    private Label occurrenceLabel = null!;
    private Label pageLabel = null!;
    private List<string> allPackets = new();
    private Dictionary<string, List<List<string>>> packetLines = new();
    private string? currentFilePath;
    private string? parsedContent;
    private Process? currentProcess;
    private int lastReportedProgress = -1;
    private bool isReparsing = false;
    private string? selectedPacketBeforeReparse;
    private int currentPage = 0;
    private int totalPages = 0;
    private int pageBeforeReparse = 0;

    public MainForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "WowPacketParser GUI";
        Size = new Size(950, 600);
        MinimumSize = new Size(950, 500);
        StartPosition = FormStartPosition.CenterScreen;

        // File selection
        var fileLabel = new Label 
        { 
            Text = "PKT File:", 
            Location = new Point(10, 15), 
            Size = new Size(60, 23),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        
        filePathTextBox = new TextBox 
        { 
            Location = new Point(75, 12), 
            Size = new Size(500, 23), 
            ReadOnly = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        
        browseButton = new Button 
        { 
            Text = "Browse", 
            Location = new Point(585, 11), 
            Size = new Size(75, 25),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        browseButton.Click += BrowseButton_Click;

        // Parse button
        parseButton = new Button 
        { 
            Text = "Parse", 
            Location = new Point(670, 11), 
            Size = new Size(75, 25), 
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        parseButton.Click += ParseButton_Click;

        // Cancel button
        cancelButton = new Button 
        { 
            Text = "Cancel", 
            Location = new Point(670, 11), 
            Size = new Size(75, 25), 
            Enabled = false, 
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        cancelButton.Click += CancelButton_Click;

        // Open config button
        openConfigButton = new Button 
        { 
            Text = "Config", 
            Location = new Point(755, 11), 
            Size = new Size(70, 25),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        openConfigButton.Click += OpenConfigButton_Click;

        // Packet selection
        var packetLabel = new Label 
        { 
            Text = "Packet:", 
            Location = new Point(10, 50), 
            Size = new Size(50, 23),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        
        searchTextBox = new TextBox 
        { 
            Location = new Point(65, 47), 
            Size = new Size(200, 23), 
            PlaceholderText = "Search packets...",
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        searchTextBox.TextChanged += SearchTextBox_TextChanged;

        packetComboBox = new ComboBox
        {
            Location = new Point(275, 47),
            Size = new Size(300, 23),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        packetComboBox.SelectedIndexChanged += PacketComboBox_SelectedIndexChanged;

        // Re-parse button
        reparseButton = new Button 
        { 
            Text = "Re-parse", 
            Location = new Point(585, 46), 
            Size = new Size(75, 25), 
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        reparseButton.Click += ReparseButton_Click;

        // Copy button
        copyButton = new Button 
        { 
            Text = "Copy", 
            Location = new Point(670, 46), 
            Size = new Size(75, 25), 
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        copyButton.Click += CopyButton_Click;

        // Open in editor button
        openEditorButton = new Button 
        { 
            Text = "Open", 
            Location = new Point(755, 46), 
            Size = new Size(70, 25), 
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        openEditorButton.Click += OpenEditorButton_Click;

        // First Craft Treasures button
        firstCraftButton = new Button 
        { 
            Text = "First Craft", 
            Location = new Point(835, 46), 
            Size = new Size(80, 25), 
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        firstCraftButton.Click += FirstCraftButton_Click;

        // Occurrence label
        occurrenceLabel = new Label
        {
            Location = new Point(10, 80),
            Size = new Size(150, 23),
            Text = "",
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        // Previous page button
        prevPageButton = new Button
        {
            Text = "◀ Prev",
            Location = new Point(170, 79),
            Size = new Size(70, 25),
            Enabled = false,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        prevPageButton.Click += PrevPageButton_Click;

        // Page label
        pageLabel = new Label
        {
            Location = new Point(245, 80),
            Size = new Size(80, 23),
            Text = "",
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        // Next page button
        nextPageButton = new Button
        {
            Text = "Next ▶",
            Location = new Point(330, 79),
            Size = new Size(70, 25),
            Enabled = false,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        nextPageButton.Click += NextPageButton_Click;

        // Highlight text label and input
        var highlightLabel = new Label
        {
            Text = "Highlight:",
            Location = new Point(410, 80),
            Size = new Size(60, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleRight,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        highlightTextBox = new TextBox
        {
            Location = new Point(475, 79),
            Size = new Size(150, 23),
            PlaceholderText = "Text to highlight...",
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        highlightTextBox.TextChanged += HighlightTextBox_TextChanged;

        // Progress bar
        progressBar = new ProgressBar 
        { 
            Location = new Point(410, 80), 
            Size = new Size(260, 23), 
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        // Progress label
        progressLabel = new Label
        {
            Location = new Point(680, 80),
            Size = new Size(80, 23),
            Text = "0%",
            TextAlign = System.Drawing.ContentAlignment.MiddleRight,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        // Output - This is the most important for resizing
        outputTextBox = new RichTextBox
        {
            Location = new Point(10, 110),
            Size = new Size(890, 440),
            ReadOnly = true,
            Font = new Font("Consolas", 9),
            ScrollBars = RichTextBoxScrollBars.Both,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.AddRange(new Control[] {
            fileLabel, filePathTextBox, browseButton, parseButton, cancelButton, openConfigButton,
            packetLabel, searchTextBox, packetComboBox, reparseButton, copyButton, openEditorButton, firstCraftButton,
            occurrenceLabel, prevPageButton, pageLabel, nextPageButton,
            highlightLabel, highlightTextBox,
            progressBar, progressLabel, outputTextBox
        });

        // Handle form resize to update button positions dynamically
        this.Resize += MainForm_Resize;
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        // Update positions of buttons on the right side based on form width
        int rightMargin = this.ClientSize.Width - 10;
        
        // Row 1 buttons
        openConfigButton.Left = rightMargin - openConfigButton.Width;
        parseButton.Left = openConfigButton.Left - parseButton.Width - 10;
        cancelButton.Left = parseButton.Left;
        browseButton.Left = parseButton.Left - browseButton.Width - 10;
        
        // Update file path textbox width
        filePathTextBox.Width = browseButton.Left - filePathTextBox.Left - 10;
        
        // Row 2 buttons
        firstCraftButton.Left = rightMargin - firstCraftButton.Width;
        openEditorButton.Left = firstCraftButton.Left - openEditorButton.Width - 10;
        copyButton.Left = openEditorButton.Left - copyButton.Width - 10;
        reparseButton.Left = copyButton.Left - reparseButton.Width - 10;
        
        // Update packet combo box width
        packetComboBox.Width = reparseButton.Left - packetComboBox.Left - 10;
        
        // Update output textbox size
        outputTextBox.Width = rightMargin - outputTextBox.Left;
        outputTextBox.Height = this.ClientSize.Height - outputTextBox.Top - 10;
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
            outputTextBox.Clear();
            allPackets.Clear();
            packetComboBox.Items.Clear();
            packetComboBox.Enabled = false;
            occurrenceLabel.Visible = false;
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
            outputTextBox.SelectionColor = Color.Black;
            outputTextBox.SelectionBackColor = Color.White;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                outputTextBox.ResumeLayout();
                return;
            }

            // Find and highlight all occurrences (case-insensitive)
            int startIndex = 0;
            while (true)
            {
                int index = outputTextBox.Text.IndexOf(searchText, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index < 0) break;

                outputTextBox.Select(index, searchText.Length);
                outputTextBox.SelectionBackColor = Color.Yellow;
                outputTextBox.SelectionColor = Color.Black;

                startIndex = index + searchText.Length;
            }

            outputTextBox.DeselectAll();
        }
        finally
        {
            outputTextBox.ResumeLayout();
        }
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
            highlightTextBox.Visible = true;
            return;
        }

        prevPageButton.Visible = true;
        nextPageButton.Visible = true;
        pageLabel.Visible = true;
        highlightTextBox.Visible = true;

        prevPageButton.Enabled = currentPage > 0;
        nextPageButton.Enabled = currentPage < totalPages - 1;
        pageLabel.Text = $"{currentPage + 1} / {totalPages}";
    }

    private void DisplayCurrentPage()
    {
        var selectedPacket = packetComboBox.SelectedItem?.ToString();
        if (selectedPacket == null || !packetLines.ContainsKey(selectedPacket))
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

                                    var occurrences = packetLines[selectedPacketBeforeReparse];
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
        var lines = output.Split('\n');
        var packetRegex = new Regex(@"(ServerToClient|ClientToServer):\s+(\w+)\s+\(0x[0-9A-F]+\)");

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
                    allPackets.Add(currentPacket);

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

        allPackets.Sort();
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

            packetComboBox.Items.AddRange(filteredPackets.ToArray());
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

        var selectedPacket = packetComboBox.SelectedItem.ToString();
        if (selectedPacket != null && packetLines.ContainsKey(selectedPacket))
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
                highlightTextBox.Visible = true;
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
            progressForm.Close();
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