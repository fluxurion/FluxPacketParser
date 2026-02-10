# WowPacketParser GUI

A Windows Forms GUI wrapper for WowPacketParser.exe that provides an easy-to-use interface for parsing WoW packet files.

## Features

- **File Selection**: Browse and select .pkt or .bin files
- **Parse Button**: Execute WowPacketParser.exe on the selected file
- **Packet Search**: Search through parsed packets with a real-time filter
- **Packet Navigation**: Select packets from dropdown to jump to them in the output
- **Re-parse**: Re-run parsing on the same file with updated settings

## Usage

1. **Build the project**:
   ```
   dotnet build WowPacketParserGUI
   ```

2. **Run the application**:
   ```
   dotnet run --project WowPacketParserGUI
   ```

3. **Using the GUI**:
   - Click "Browse" to select a .pkt or .bin file
   - Click "Parse" to run WowPacketParser.exe on the file
   - Use the search box to filter packets by name
   - Select a packet from the dropdown to navigate to it in the output
   - Click "Re-parse" to run the parser again

## Requirements

- .NET 9.0 SDK
- Windows OS (Windows Forms)
- WowPacketParser.exe must be available in one of these locations:
  - Same directory as the GUI executable
  - `WowPacketParser/bin/Debug/net9.0/`
  - `WowPacketParser/bin/Release/net9.0/`

## Notes

The GUI automatically extracts packet information from the parser output and populates the dropdown with ServerToClient and ClientToServer packets for easy navigation.