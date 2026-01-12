using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V11_0_0_55666.Parsers
{
    public static class WarbandHandler
    {
        [Parser(Opcode.CMSG_SETUP_WARBAND_GROUPS)]
        public static void HandleSetupWarbandGroups(Packet packet)
        {
            var groupCountFlag = packet.ReadByte("GroupCountFlag");

            for (var i = 0; i < groupCountFlag / 8; ++i)
            {
                packet.ResetBitReader();

                packet.ReadUInt64("GroupID");
                packet.ReadByte("OrderIndex");
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V11_1_0_59347))
                    packet.ReadUInt32("WarbandSceneID");
                packet.ReadInt32("Flags");
                var memberCount = packet.ReadUInt32();
                for (var j = 0u; j < memberCount; ++j)
                    ReadWarbandGroupMember(packet, "Members", j);

                if (ClientVersion.AddedInVersion(ClientVersionBuild.V11_1_0_59347))
                {
                    var nameLength = packet.ReadBits(9);
                    packet.ReadWoWString("Name", nameLength);
                }
            }
        }

        public static void ReadWarbandGroupMember(Packet packet, params object[] idx)
        {
            packet.ReadInt32("WarbandScenePlacementID", idx);
            var type = packet.ReadInt32("Type", idx);
            if (type == 0)
                packet.ReadPackedGuid128("Guid", idx);
        }

        public static void ReadWarbandGroup(Packet packet, params object[] idx)
        {
            packet.ReadUInt64("GroupID", idx);
            packet.ReadByte("OrderIndex", idx);
            packet.ReadInt32("Flags", idx);
            var memberCount = packet.ReadUInt32();
            for (var i = 0u; i < memberCount; ++i)
                ReadWarbandGroupMember(packet, idx, "Members", i);
        }
    }
}
