using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{
    public static class LfgHandler
    {        
        public static void ReadLfgListJoinRequest(Packet packet, params object[] idx)
        {
            packet.ReadInt32("GroupFinderActivityId", idx);
            packet.ReadSingle("RequiredItemLevel", idx);
            packet.ReadUInt32("RequiredHonorLevel", idx);

            packet.ResetBitReader();
            var lenName = packet.ReadBits(8);
            var lenComment = packet.ReadBits(11);
            var lenVoiceChat = packet.ReadBits(8);
            var hasQuest = false;
            packet.ReadBit("AutoAccept", idx);
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V7_1_5_23360))
            {
                packet.ReadBit("IsPrivate", idx);
                hasQuest = packet.ReadBit("HasQuest", idx);
            }

            packet.ReadWoWString("Name", lenName, idx);
            packet.ReadWoWString("Comment", lenComment, idx);
            packet.ReadWoWString("VoiceChat", lenVoiceChat, idx);

            if (hasQuest)
                packet.ReadUInt32("QuestID", idx);
        }

        [Parser(Opcode.SMSG_LFG_LIST_UPDATE_STATUS)]
        public static void HandleLfgListUpdateStatus(Packet packet)
        {
            V6_0_2_19033.Parsers.LfgHandler.ReadCliRideTicket(packet, "RideTicket");
            packet.ReadTime("RemainingTime");
            packet.ReadByte("ResultId");
            ReadLfgListJoinRequest(packet, "LFGListJoinRequest");
            packet.ResetBitReader();
            packet.ReadBit("Listed");
            var lenTitle = packet.ReadBits(6);
            packet.ReadWoWString("Title", lenTitle);
            var lenComment = packet.ReadBits(11);
            packet.ReadWoWString("Comment", lenComment);
            var lenMinRating = packet.ReadBits(8);
            var lenMinIlvl = packet.ReadBits(8);
            var lenVoiceChat = packet.ReadBits(6);
            packet.ReadUInt32("Goal");
            var hasMinRating = packet.ReadBit("hasMinRating");
            if (hasMinRating)
            {
                packet.ReadWoWString("MinRating", lenMinRating);
            }
            var hasMinIlvl = packet.ReadBit("hasMinIlvl");
            if (hasMinIlvl)
            {
                packet.ReadWoWString("MinIlvl", lenMinIlvl);
            }
            var hasVoiceChat = packet.ReadBit("hasVoiceChat");
            if (hasVoiceChat)
            {
                packet.ReadWoWString("VoiceChat", lenVoiceChat);
            }
            packet.ReadBit("LimitToFaction");
            packet.ReadBit("Private");
        }
    }
}
