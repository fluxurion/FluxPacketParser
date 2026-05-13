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
            // CDataStore::WriteBits5
            var groupCount = packet.ReadBits("GroupCount", 5);
            packet.ResetBitReader(); // FlushBits

            for (var i = 0; i < groupCount; i++)
            {
                packet.ReadUInt64("GroupID", i);
                packet.ReadByte("OrderIndex", i); // v8 + 8
                packet.ReadUInt32("WarbandSceneID", i); // v8 + 268
                packet.ReadUInt32("Flags", i); // v8 + 272
                packet.ReadUInt32("Unk3", i); // v8 + 276 (Could be IconID?)
                
                var memberCount = packet.ReadUInt32("MemberCount", i); // v8 + 288

                for (var j = 0; j < memberCount; j++)
                {
                    packet.ReadUInt32("SlotIndex", i, j); // v11 + 0
                    var memberType = packet.ReadUInt32("MemberType", i, j); // v11 + 4
                    packet.ReadUInt32("Unk4", i, j); // v11 + 8

                    if (memberType == 0)
                        packet.ReadPackedGuid128("MemberGUID", i, j); // v11 + 16
                }

                // Logic for Name Length (v13 >> 1 and v13 & 1)
                // This is a 9-bit read in total.
                var nameLength = packet.ReadBits("NameLength", 9, i);
                packet.ResetBitReader(); // FlushBits

                if (nameLength > 0)
                {
                    // Note: The assembly uses WriteData with v14 (the raw length)
                    // ReadWoWString usually expects a length or reads until null.
                    packet.ReadWoWString("GroupName", nameLength, i);
                }
            }
        }
    }
}
