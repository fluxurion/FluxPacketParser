using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V5_5_0_61735.Parsers
{
    public static class FluxHotfixHandler
    {
        // 1.15.9 (0x4A0003): wire layout matches retail SMSG_HOTFIX_CONNECT —
        // {u32 count, records, i32 dataSize, blob}. Each record is 21 bytes:
        // {u32 HotfixId, u32 UniqueId, u32 Type(DB2Hash), i32 RecordId,
        //  i32 HotfixDataSize, u8 flag byte (top 3 bits = HotfixStatus)}.
        [Parser(Opcode.SMSG_HOTFIX_CONNECT, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleHotfixConnect(Packet packet)
        {
            var hotfixCount = packet.ReadUInt32("HotfixCount");

            for (var i = 0u; i < hotfixCount; ++i)
            {
                packet.ReadUInt32("HotfixID", i, "HotfixRecord");
                packet.ReadUInt32("UniqueID", i, "HotfixRecord");
                packet.ReadUInt32E<DB2Hash>("Type", i, "HotfixRecord");
                packet.ReadInt32("RecordID", i, "HotfixRecord");
                packet.ReadInt32("HotfixDataSize", i, "HotfixRecord");
                packet.ResetBitReader();
                packet.ReadBitsE<HotfixStatus>("Status", 3, i, "HotfixRecord");
                packet.ResetBitReader();
            }

            var dataSize = packet.ReadInt32("HotfixDataSize");
            packet.ReadBytes(dataSize);
        }
    }
}
