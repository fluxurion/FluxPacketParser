using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V1_60_1_70009.Parsers
{
    // Classic 1.60.1.70009 Club Finder (guild recruitment). Wire layouts from
    // TrinityCoreLuaSol (E:\TrinityCoreLuaSol) ClubFinderPackets.cpp — the
    // 70058 client readers/writers, guilds-only variant.
    public static class ClubFinderHandler
    {
        // CMSG_CLUB_FINDER_POST
        // bits {nameLen:7, descLen:12, type:3, crossFaction:1};
        // u64 clubId, u64 specIds, i32 flags, i32 minItemLevel, u32 avatar;
        // name, description
        [Parser(Opcode.CMSG_CLUB_FINDER_POST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderPost(Packet packet)
        {
            var nameLen = packet.ReadBits("NameLength", 7);
            var descLen = packet.ReadBits("DescriptionLength", 12);
            packet.ReadBits("Type", 3);
            packet.ReadBit("CrossFaction");

            packet.ReadUInt64("ClubID");
            packet.ReadUInt64("SpecIDs");
            packet.ReadInt32("Flags");
            packet.ReadInt32("MinItemLevel");
            packet.ReadUInt32("Avatar");

            packet.ReadWoWString("Name", nameLen);
            packet.ReadWoWString("Description", descLen);
        }

        // 64-byte tagged-union filter record shared by REQUEST_CLUBS_LIST and
        // REQUEST_CLUBS_DATA (TC ReadFilter, client writer rva 0x8C8350):
        // kind:3; kind 5/6 -> blobSize:6 + bytes (NUL counted in size);
        // kind 1/2 -> int32, kind 3/4 -> int64
        private static void ReadClubFinderRequestFilter(Packet packet, params object[] idx)
        {
            var kind = packet.ReadBits("Kind", 3, idx);
            if (kind == 5 || kind == 6)
            {
                var blobSize = packet.ReadBits("BlobSize", 6, idx);
                packet.ResetBitReader();
                if (blobSize != 0)
                    packet.ReadWoWString("Text", blobSize, idx);
                return;
            }

            packet.ResetBitReader();
            switch (kind)
            {
                case 1:
                case 2:
                    packet.ReadInt32("IntValue", idx);
                    break;
                case 3:
                case 4:
                    packet.ReadInt64("WideValue", idx);
                    break;
            }
        }

        // CMSG_CLUB_FINDER_REQUEST_CLUBS_LIST
        // u8 searchLenHi; bits {searchLenLo:1, type:3, flag:1};
        // i32 filterCount, i32 unk; searchString; filterCount x filter record
        [Parser(Opcode.CMSG_CLUB_FINDER_REQUEST_CLUBS_LIST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderRequestClubsList(Packet packet)
        {
            var searchLenHi = packet.ReadByte("SearchStringLengthHi");
            var searchLenLo = packet.ReadBits("SearchStringLengthLo", 1);
            packet.ReadBits("Type", 3);
            packet.ReadBit("UnkFlag");

            var filterCount = packet.ReadInt32("FilterCount");
            packet.ReadInt32("UnkInt");

            packet.ReadWoWString("SearchString", (searchLenHi << 1) | (int)searchLenLo);

            for (var i = 0; i < filterCount; ++i)
                ReadClubFinderRequestFilter(packet, i);
        }

        // CMSG_CLUB_FINDER_REQUEST_CLUBS_DATA
        // i32 clubIdCount, i32 requestCount; clubIdCount x i32;
        // requestCount x { tag:3, reset, filter record }; bits {type:3, flag:1}
        [Parser(Opcode.CMSG_CLUB_FINDER_REQUEST_CLUBS_DATA, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderRequestClubsData(Packet packet)
        {
            var clubIdCount = packet.ReadInt32("ClubIDCount");
            var requestCount = packet.ReadInt32("RequestCount");

            for (var i = 0; i < clubIdCount; ++i)
                packet.ReadInt32("ClubID", i);

            for (var i = 0; i < requestCount; ++i)
            {
                packet.ReadBits("Tag", 3, i);
                packet.ResetBitReader();
                ReadClubFinderRequestFilter(packet, i);
            }

            packet.ReadBits("Type", 3);
            packet.ReadBit("UnkFlag");
        }

        // CMSG_CLUB_FINDER_REQUEST_MEMBERSHIP_TO_CLUB
        // guid posting, u64 specIds, comment (10-bit length)
        [Parser(Opcode.CMSG_CLUB_FINDER_REQUEST_MEMBERSHIP_TO_CLUB, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderRequestMembershipToClub(Packet packet)
        {
            packet.ReadPackedGuid128("ClubFinderGUID");
            packet.ReadUInt64("SpecIDs");

            var commentLen = packet.ReadBits("CommentLength", 10);
            packet.ResetBitReader();
            packet.ReadWoWString("Comment", commentLen);
        }

        // CMSG_CLUB_FINDER_GET_APPLICANTS_LIST — single 3-bit type field
        [Parser(Opcode.CMSG_CLUB_FINDER_GET_APPLICANTS_LIST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderGetApplicantsList(Packet packet)
        {
            packet.ReadBits("Type", 3);
        }

        // CMSG_CLUB_FINDER_RESPOND_TO_APPLICANT
        // guid posting, guid player, bit accept, type:3, bit force
        [Parser(Opcode.CMSG_CLUB_FINDER_RESPOND_TO_APPLICANT, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderRespondToApplicant(Packet packet)
        {
            packet.ReadPackedGuid128("ClubFinderGUID");
            packet.ReadPackedGuid128("ApplicantGUID");
            packet.ReadBit("Accept");
            packet.ReadBits("Type", 3);
            packet.ReadBit("Force");
        }

        // CMSG_CLUB_FINDER_APPLICATION_RESPONSE
        // guid posting, type:3, updateType:3 (ClubFinderApplicationUpdateType)
        [Parser(Opcode.CMSG_CLUB_FINDER_APPLICATION_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderApplicationResponse(Packet packet)
        {
            packet.ReadPackedGuid128("ClubFinderGUID");
            packet.ReadBits("Type", 3);
            packet.ReadBits("UpdateType", 3);
        }

        // CMSG_CLUB_FINDER_REQUEST_PENDING_CLUBS_LIST — single 3-bit type field
        [Parser(Opcode.CMSG_CLUB_FINDER_REQUEST_PENDING_CLUBS_LIST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderRequestPendingClubsList(Packet packet)
        {
            packet.ReadBits("Type", 3);
        }

        // CMSG_CLUB_FINDER_REQUEST_SUBSCRIBED_CLUB_POSTING_IDS — plain u32 type
        [Parser(Opcode.CMSG_CLUB_FINDER_REQUEST_SUBSCRIBED_CLUB_POSTING_IDS, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderRequestSubscribedClubPostingIds(Packet packet)
        {
            packet.ReadUInt32("Type");
        }

        // CMSG_CLUB_FINDER_WHISPER_APPLICANT_REQUEST — two guids
        [Parser(Opcode.CMSG_CLUB_FINDER_WHISPER_APPLICANT_REQUEST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderWhisperApplicantRequest(Packet packet)
        {
            packet.ReadPackedGuid128("ClubFinderGUID");
            packet.ReadPackedGuid128("ApplicantGUID");
        }

        // SMSG_CLUB_FINDER_LOOKUP_CLUB_POSTINGS_LIST
        // u32 count, postings, bits {type:3, isLastPart:1}
        private static void ReadClubFinderPostingEntry(Packet packet, params object[] idx)
        {
            var nameLen = packet.ReadBits("ClubNameLength", 7, idx);
            var descLen = packet.ReadBits("DescriptionLength", 12, idx);
            var leaderLen = packet.ReadBits("LeaderNameLength", 6, idx);
            packet.ResetBitReader();

            packet.ReadPackedGuid128("ClubFinderGUID", idx);
            packet.ReadUInt32("NumActiveMembers", idx);
            packet.ReadInt64("ClubID", idx);
            packet.ReadInt32("MinItemLevel", idx);
            packet.ReadInt32("EmblemInfo", idx);
            packet.ReadUInt32("RecruitmentFlags", idx);
            packet.ReadPackedGuid128("LastPosterGUID", idx);
            packet.ReadInt64("LastUpdatedTime", idx);
            packet.ReadUInt64("RecruitingSpecIDs", idx);

            packet.ReadWoWString("ClubName", nameLen, idx);
            packet.ReadWoWString("Description", descLen, idx);
            packet.ReadWoWString("LeaderName", leaderLen, idx);
        }

        [Parser(Opcode.SMSG_CLUB_FINDER_LOOKUP_CLUB_POSTINGS_LIST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderLookupPostingsList(Packet packet)
        {
            var count = packet.ReadUInt32("PostingsCount");
            for (uint i = 0; i < count; ++i)
                ReadClubFinderPostingEntry(packet, i);

            packet.ReadBits("Type", 3);
            packet.ReadBit("IsLastPart");
        }

        // SMSG_RETURN_APPLICANT_LIST
        // guid club, u32 count, applicant records, bits {type:3}
        private static void ReadClubFinderApplicantEntry(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("ClubFinderGUID", idx);
            packet.ReadPackedGuid128("PlayerGUID", idx);
            packet.ReadUInt32("Closed", idx);
            packet.ReadSByte("Level", idx);
            packet.ReadSByte("ClassID", idx);
            packet.ReadInt32("ItemLevel", idx);
            packet.ReadInt32("UnkInt", idx);
            packet.ReadInt64("LastUpdatedTime", idx);
            packet.ReadUInt64("SpecIDs", idx);
            packet.ReadSByte("Faction", idx);

            var nameLen = packet.ReadBits("NameLength", 6, idx);
            var commentLen = packet.ReadBits("CommentLength", 10, idx);
            packet.ReadBits("RequestStatus", 4, idx);
            packet.ReadBit("LookupSuccess", idx);
            packet.ResetBitReader();

            packet.ReadWoWString("Name", nameLen, idx);
            packet.ReadWoWString("Comment", commentLen, idx);
        }

        [Parser(Opcode.SMSG_RETURN_APPLICANT_LIST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderApplicantList(Packet packet)
        {
            packet.ReadPackedGuid128("ClubFinderGUID");

            var count = packet.ReadUInt32("ApplicantsCount");
            for (uint i = 0; i < count; ++i)
                ReadClubFinderApplicantEntry(packet, i);

            packet.ReadBits("Type", 3);
        }

        // SMSG_CLUB_FINDER_RESPONSE_CHARACTER_APPLICATION_LIST /
        // SMSG_CLUB_FINDER_UPDATE_APPLICATIONS — identical record layout
        private static void ReadClubFinderPlayerApplication(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("ClubFinderGUID", idx);
            packet.ReadPackedGuid128("PlayerGUID", idx);
            packet.ReadUInt32("Closed", idx);
            packet.ReadUInt64("LastUpdatedTime", idx);
            packet.ReadBits("RequestStatus", 4, idx);
            packet.ResetBitReader();
        }

        private static void HandleClubFinderPlayerApplicationList(Packet packet)
        {
            var count = packet.ReadUInt32("ApplicationsCount");
            for (uint i = 0; i < count; ++i)
                ReadClubFinderPlayerApplication(packet, i);

            packet.ReadBits("Type", 3);
        }

        [Parser(Opcode.SMSG_CLUB_FINDER_RESPONSE_CHARACTER_APPLICATION_LIST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderCharacterApplicationList(Packet packet)
        {
            HandleClubFinderPlayerApplicationList(packet);
        }

        [Parser(Opcode.SMSG_CLUB_FINDER_UPDATE_APPLICATIONS, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderUpdateApplications(Packet packet)
        {
            HandleClubFinderPlayerApplicationList(packet);
        }

        // SMSG_CLUB_FINDER_GET_CLUB_POSTING_IDS_RESPONSE
        // u32 count, { i64 clubId, u32 clubFinderId, u32 type }
        [Parser(Opcode.SMSG_CLUB_FINDER_GET_CLUB_POSTING_IDS_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderGetClubPostingIdsResponse(Packet packet)
        {
            var count = packet.ReadUInt32("PostingsCount");
            for (uint i = 0; i < count; ++i)
            {
                packet.ReadInt64("ClubID", i);
                packet.ReadUInt32("ClubFinderID", i);
                packet.ReadUInt32("Type", i);
            }
        }

        // SMSG_CLUB_FINDER_RESPONSE_POST_RECRUITMENT_MESSAGE
        // guid clubFinder, bits {type:3, result:3}
        [Parser(Opcode.SMSG_CLUB_FINDER_RESPONSE_POST_RECRUITMENT_MESSAGE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderPostRecruitmentMessage(Packet packet)
        {
            packet.ReadPackedGuid128("ClubFinderGUID");
            packet.ReadBits("Type", 3);
            packet.ReadBits("Result", 3);
        }

        // SMSG_CLUB_FINDER_ERROR_MESSAGE — bits {type:3, reason:4}
        [Parser(Opcode.SMSG_CLUB_FINDER_ERROR_MESSAGE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderErrorMessage(Packet packet)
        {
            packet.ReadBits("Type", 3);
            packet.ReadBits("Error", 4);
        }

        // SMSG_CLUB_FINDER_WHISPER_APPLICANT_RESPONSE — two guids
        [Parser(Opcode.SMSG_CLUB_FINDER_WHISPER_APPLICANT_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClubFinderWhisperApplicantResponse(Packet packet)
        {
            packet.ReadPackedGuid128("ClubFinderGUID");
            packet.ReadPackedGuid128("ApplicantGUID");
        }
    }
}
