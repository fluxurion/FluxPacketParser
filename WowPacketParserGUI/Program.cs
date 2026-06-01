namespace WowPacketParserGUI;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Force dark mode for the entire application (scrollbars and frames too)
        Application.SetColorMode(SystemColorMode.Dark);

        Application.Run(new MainForm());
    }
}