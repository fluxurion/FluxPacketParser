using System.Text;
using System.Text.RegularExpressions;

namespace WowPacketParserGUI;

/// <summary>
/// A single parsed packet inside a *_parsed.txt file: byte range plus the
/// metadata found on its header line. The packet text itself stays on disk
/// and is read lazily via <see cref="ParsedFileIndex.ReadOccurrenceText"/>.
/// </summary>
public sealed class PacketOccurrence
{
    public required long Offset { get; init; }
    public required int Length { get; init; }
    public required string Direction { get; init; }
    public required string Name { get; init; }
    public required string Opcode { get; init; }
    public string Time { get; init; } = "";
    public string Number { get; init; } = "";

    /// "ServerToClient: SMSG_X" style key used by the packet combobox.
    public string Key => Direction + ": " + Name;
}

/// <summary>
/// Index over a *_parsed.txt file. Holds only header metadata and byte ranges,
/// so even multi-GB parsed files use a small amount of memory.
/// </summary>
public sealed class ParsedFileIndex
{
    public required string FilePath { get; init; }
    public required List<PacketOccurrence> Ordered { get; init; }
    public required Dictionary<string, List<PacketOccurrence>> ByKey { get; init; }
    public required List<string> UniqueKeys { get; init; }
    public required Dictionary<string, string> FirstTimestamp { get; init; }

    public string ReadOccurrenceText(PacketOccurrence occurrence)
    {
        using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
            bufferSize: 1 << 16, FileOptions.RandomAccess);
        stream.Position = occurrence.Offset;
        var buffer = new byte[occurrence.Length];
        stream.ReadExactly(buffer);
        return Encoding.UTF8.GetString(buffer);
    }
}

public static class ParsedFileIndexer
{
    private const long ProgressStep = 8L << 20;

    private static ReadOnlySpan<byte> ServerMarker => "ServerToClient:"u8;
    private static ReadOnlySpan<byte> ClientMarker => "ClientToServer:"u8;

    private static readonly Regex HeaderRegex = new(
        @"(ServerToClient|ClientToServer):\s+(\w+)\s+\((0x[0-9A-F]+)\)",
        RegexOptions.Compiled);
    private static readonly Regex TimeRegex = new(
        @"Time:\s+(\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2}\.\d{3})",
        RegexOptions.Compiled);
    private static readonly Regex NumberRegex = new(
        @"Number:\s+(\d+)",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans the file once, sequentially. Only lines containing a packet header
    /// marker are decoded; everything else is skipped at the byte level.
    /// </summary>
    public static ParsedFileIndex Build(string path, Action<int>? reportProgress = null)
    {
        var fileLength = Math.Max(new FileInfo(path).Length, 1);
        var ordered = new List<PacketOccurrence>();
        var byKey = new Dictionary<string, List<PacketOccurrence>>(StringComparer.Ordinal);
        var uniqueKeys = new List<string>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var firstTimestamp = new Dictionary<string, string>(StringComparer.Ordinal);

        // Direction/Name/Opcode repeat for every occurrence of a packet type — share one instance
        var interned = new Dictionary<string, string>(StringComparer.Ordinal);
        string Intern(string s) => interned.TryGetValue(s, out var v) ? v : interned[s] = s;

        long pendingOffset = -1;
        string pendingDirection = "", pendingName = "", pendingOpcode = "", pendingTime = "", pendingNumber = "";

        void ClosePending(long endOffset)
        {
            if (pendingOffset < 0)
                return;

            var occ = new PacketOccurrence
            {
                Offset = pendingOffset,
                Length = (int)Math.Min(endOffset - pendingOffset, int.MaxValue),
                Direction = pendingDirection,
                Name = pendingName,
                Opcode = pendingOpcode,
                Time = pendingTime,
                Number = pendingNumber
            };
            ordered.Add(occ);
            if (!byKey.TryGetValue(occ.Key, out var list))
                byKey[occ.Key] = list = new List<PacketOccurrence>();
            list.Add(occ);
            pendingOffset = -1;
        }

        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                   bufferSize: 1 << 20, FileOptions.SequentialScan))
        {
            var reader = new ByteLineReader(stream);
            var nextProgressAt = ProgressStep;

            while (reader.ReadLine())
            {
                if (reportProgress != null && reader.Position >= nextProgressAt)
                {
                    nextProgressAt = reader.Position + ProgressStep;
                    reportProgress((int)Math.Min(100, reader.Position * 100 / fileLength));
                }

                var line = reader.Line;
                if (line.IndexOf(ServerMarker) < 0 && line.IndexOf(ClientMarker) < 0)
                    continue;

                var text = Encoding.UTF8.GetString(line);
                var match = HeaderRegex.Match(text);
                if (!match.Success)
                    continue;

                ClosePending(reader.LineStart);

                pendingOffset = reader.LineStart;
                pendingDirection = Intern(match.Groups[1].Value);
                pendingName = Intern(match.Groups[2].Value);
                pendingOpcode = Intern(match.Groups[3].Value);
                var timeMatch = TimeRegex.Match(text);
                pendingTime = timeMatch.Success ? timeMatch.Groups[1].Value : "";
                var numberMatch = NumberRegex.Match(text);
                pendingNumber = numberMatch.Success ? numberMatch.Groups[1].Value : "";

                var key = pendingDirection + ": " + pendingName;
                if (seenKeys.Add(key))
                {
                    uniqueKeys.Add(key);
                    firstTimestamp[key] = pendingTime;
                }
            }

            ClosePending(fileLength);
        }

        uniqueKeys.Sort((a, b) => string.Compare(
            firstTimestamp.TryGetValue(a, out var ta) ? ta : "",
            firstTimestamp.TryGetValue(b, out var tb) ? tb : "",
            StringComparison.Ordinal));

        return new ParsedFileIndex
        {
            FilePath = path,
            Ordered = ordered,
            ByKey = byKey,
            UniqueKeys = uniqueKeys,
            FirstTimestamp = firstTimestamp
        };
    }

    /// <summary>
    /// Line reader over a stream that tracks exact byte positions.
    /// Lines include their terminator so offsets always line up with the file.
    /// </summary>
    private sealed class ByteLineReader
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer = new byte[1 << 16];
        private int _bufferPos;
        private int _bufferLen;
        private byte[] _line = new byte[8192];
        private int _lineLen;

        public long Position { get; private set; }
        public long LineStart { get; private set; }
        public ReadOnlySpan<byte> Line => _line.AsSpan(0, _lineLen);

        public ByteLineReader(Stream stream) => _stream = stream;

        public bool ReadLine()
        {
            _lineLen = 0;
            LineStart = Position;
            while (true)
            {
                if (_bufferPos >= _bufferLen)
                {
                    _bufferLen = _stream.Read(_buffer, 0, _buffer.Length);
                    _bufferPos = 0;
                    if (_bufferLen == 0)
                        return _lineLen > 0;
                }

                var nl = Array.IndexOf(_buffer, (byte)'\n', _bufferPos, _bufferLen - _bufferPos);
                var take = nl >= 0 ? nl - _bufferPos + 1 : _bufferLen - _bufferPos;
                if (_lineLen + take > _line.Length)
                    Array.Resize(ref _line, Math.Max(_line.Length * 2, _lineLen + take));
                Array.Copy(_buffer, _bufferPos, _line, _lineLen, take);
                _lineLen += take;
                _bufferPos += take;
                Position += take;
                if (nl >= 0)
                    return true;
            }
        }
    }
}
