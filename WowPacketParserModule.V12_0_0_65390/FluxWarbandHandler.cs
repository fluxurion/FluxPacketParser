using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V12_0_0_65390.Parsers
{
    public static class FluxWarbandHandler
    {
        private static void ReadWarbandGroupMember(Packet packet, params object[] index)
        {
            packet.ReadUInt32("SlotIndex", index);
            var memberType = packet.ReadUInt32("MemberType", index);
            packet.ReadUInt32("Unk", index);

            if (memberType == 0)
                packet.ReadPackedGuid128("MemberGUID", index);
        }

        [Parser(Opcode.CMSG_SETUP_WARBAND_GROUPS)]
        public static void HandleSetupWarbandGroups(Packet packet)
        {
            packet.ResetBitReader();
            var groupCount = packet.ReadBits("GroupCount", 5);
            packet.ResetBitReader();

            for (var i = 0; i < groupCount; i++)
            {
                packet.ReadUInt64("GroupID", i);
                packet.ReadByte("OrderIndex", i);
                packet.ReadUInt32("WarbandSceneID", i);
                packet.ReadUInt32("Flags", i);
                packet.ReadUInt32("Unk", i);

                var memberCount = packet.ReadUInt32("MemberCount", i);

                for (var j = 0; j < memberCount; j++)
                    ReadWarbandGroupMember(packet, i, j);

                var nameLengthHigh = packet.ReadByte("NameLengthHigh", i);

                packet.ResetBitReader();
                var nameLengthLowBit = packet.ReadBit("NameLengthLowBit", i);
                packet.ReadBit("NameUnkBit", i);
                packet.ResetBitReader();

                var nameLength = (nameLengthHigh << 1) | (nameLengthLowBit ? 1 : 0);
                if (nameLength > 0)
                    packet.ReadWoWString("GroupName", nameLength, i);
            }
        }
    }
}
