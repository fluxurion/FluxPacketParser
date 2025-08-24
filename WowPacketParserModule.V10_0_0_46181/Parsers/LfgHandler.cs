/// FLUXURION

using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{
    public static class LfgHandler
    {
        [Parser(Opcode.CMSG_DF_GET_SYSTEM_INFO, ClientVersionBuild.V10_1_7_51187)]
        public static void HandleLFGLockInfoRequest(Packet packet)
        {
            packet.ReadBit("Player");
            if (packet.ReadBit())
                packet.ReadByte("PartyIndex");
        }

        [Parser(Opcode.CMSG_DF_JOIN, ClientVersionBuild.V10_1_7_51187)]
        public static void HandleDFJoin(Packet packet)
        {
            packet.ReadBit("QueueAsGroup");
            var hasPartyIndex = packet.ReadBit();
            packet.ReadBit("Mercenary");

            packet.ResetBitReader();

            packet.ReadByteE<LfgRoleFlag>("Roles");
            var slotsCount = packet.ReadInt32();

            if (hasPartyIndex)
                packet.ReadByte("PartyIndex");

            for (var i = 0; i < slotsCount; ++i)
                packet.ReadUInt32("Slot", i);
        }

        [Parser(Opcode.CMSG_DF_SET_ROLES, ClientVersionBuild.V10_1_7_51187)]
        public static void HandleDFSetRoles(Packet packet)
        {
            var hasPartyIndex = packet.ReadBit();
            packet.ReadByteE<LfgRoleFlag>("RolesDesired");
            if (hasPartyIndex)
                packet.ReadByte("PartyIndex");
        }

        [Parser(Opcode.SMSG_LFG_ROLE_CHOSEN, ClientVersionBuild.V10_1_7_51187)]
        public static void HandleLfgRoleChosen(Packet packet)
        {
            packet.ReadGuid("Player");
            packet.ReadByteE<LfgRoleFlag>("RoleMask");
            packet.ReadBit("Accepted");
        }
    }

    // NOT TC:
    public static void ReadLfgListJoinRequest(Packet packet, params object[] idx)
        {
            packet.ReadInt32("GroupFinderActivityId", idx);
            packet.ReadSingle("RequiredItemLevel", idx);
            packet.ReadSingle("UnkInt8", idx);

            packet.ResetBitReader();

            var lenName = packet.ReadBits(10);
            var lenComment = packet.ReadBits(11);
            var lenVoiceChat = packet.ReadBits(8);

            var flag = packet.ReadUInt32("Flag", idx);
            packet.ReadUInt32("Rating", idx);
            packet.ReadBit("AutoAccept", idx);
            packet.ReadBit("PrivateGroup", idx);
            packet.ReadBit("CrossFaction", idx);

            packet.ReadBit("dsa", idx);
            packet.ReadSingle("asd", idx);

            packet.ReadWoWString("Title", lenName, idx);
            packet.ReadWoWString("Details", lenComment, idx);
            packet.ReadWoWString("VoiceChat", lenVoiceChat, idx);

            // Flag 8: has playstyle or goal
            // flag 128: limited to player's faction

            if (flag.HasAnyFlag(8))
                packet.ReadByte("PlayStyle", idx);
        }

        [Parser(Opcode.CMSG_LFG_LIST_JOIN)]
        public static void HandleLFGListJoin(Packet packet)
        {
            ReadLfgListJoinRequest(packet, "LFGListJoinRequest");
        }

        [Parser(Opcode.SMSG_LFG_LIST_UPDATE_STATUS)]
        public static void HandleLfgListUpdateStatus(Packet packet)
        {
            V6_0_2_19033.Parsers.LfgHandler.ReadCliRideTicket(packet, "RideTicket");
            packet.ReadTime("RemainingTime");
            packet.ReadByte("ResultId");
            packet.ReadUInt16("banán");
            packet.ReadUInt16("Ananász");
            ReadLfgListJoinRequest(packet, "LFGListJoinRequest");
            packet.ResetBitReader();
            packet.ReadBit("Listed");
        }
    }
}
