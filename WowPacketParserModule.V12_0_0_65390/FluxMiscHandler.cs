using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V12_0_0_65390.Parsers
{
    public static class FluxMiscHandler
    {
        [Parser(Opcode.SMSG_SOCIAL_CONTRACT_REQUEST_RESPONSE)]
        public static void HandleSocialContractRequestResponse(Packet packet)
        {
            var value = packet.ReadByte("Byte");
            packet.AddValue("ShowContract", (value & 0x80) != 0);
        }

        [Parser(Opcode.SMSG_SYNC_WOW_ENTITLEMENTS)]
        public static void HandleSyncWowEntitlements(Packet packet)
        {
            var entitlementCount = packet.ReadUInt32("EntitlementCount");
            var displayCardCount = packet.ReadUInt32("DisplayCardCount");

            for (uint i = 0; i < entitlementCount; i++)
            {
                packet.ReadUInt32("Field0", i);
                packet.ReadUInt64("Field8", i);
                packet.ReadUInt64("Field16", i);
                packet.ReadUInt32("Field24", i);
                var flag = packet.ReadByte("Flag", i);
                packet.AddValue("HasFlag_bit7", (flag & 0x80) != 0, i);
            }

            for (uint i = 0; i < displayCardCount; i++)
                ReadDisplayCard(packet, i);
        }

        // sub_7FF659252720 — DisplayCard
        private static void ReadDisplayCard(Packet packet, params object[] index)
        {
            packet.ReadUInt32("ProductID", index);
            packet.ReadUInt32("Field260", index);
            packet.ReadUInt32("Field264", index);
            packet.ReadUInt32("Field268", index);
            packet.ReadUInt32("Field272", index);
            packet.ReadUInt32("Field276", index);
            packet.ReadUInt32("Field280", index);
            packet.ReadUInt32("Field296", index);
            packet.ReadUInt32("Field21840", index);
            packet.ReadUInt32("Field21844", index);
            packet.ReadUInt32("Field21848", index);
            packet.ReadUInt32("Field21852", index);
            packet.ReadUInt32("Field21856", index);

            var nameLen = packet.ReadByte("NameLength", index);
            var flags2 = packet.ReadByte("Flags2", index);
            packet.AddValue("Flags2_Bit7", (flags2 & 0x80) != 0, index);
            var flags3 = packet.ReadByte("Flags3", index);

            // Item loop: JamBattlePayDeliverableChoice (sub_7FF659253620)
            // count = (flags3 >> 7) | (2 * flags2)
            var itemCount = (uint)((flags3 >> 7) | (2 * (flags2 & 0x3F)));
            for (uint j = 0; j < itemCount; j++)
            {
                packet.ReadUInt32("DeliverableField0", index, j);
                packet.ReadUInt32("DeliverableField4", index, j);
                packet.ReadUInt32("DeliverableField8", index, j);
                packet.ReadUInt32("DeliverableField12", index, j);
                packet.ReadUInt32("DeliverableField16", index, j);
                packet.ReadUInt32("DeliverableField20", index, j);
                var itemFlags = packet.ReadByte("DeliverableFlags", index, j);
                packet.AddValue("DeliverableFlags_Bit7", (itemFlags & 0x80) != 0, index, j);
                // + sub_7FF659251BF0 if flag at item+21544 — needs decompilation
            }

            packet.ReadWoWString("Name", (int)nameLen, index);

            // JamBattlepayDisplayCard (sub_7FF659251BF0) if Flags3 bit6 set
            if ((flags3 & 0x40) != 0)
                _ = FluxBattlePayHandler.ReadVisualMetadata(packet, 0, 0, index);
        }
    }
}
