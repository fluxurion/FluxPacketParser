using DBFileReaderLib.Attributes;

namespace WowPacketParser.DBC.Structures.TheWarWithin
{
    [DBFile("CraftingData")]
    public sealed class CraftingDataEntry
    {
        [Index(true)]
        public uint ID;
        public int FirstCraftTreasureID;
    }
}
