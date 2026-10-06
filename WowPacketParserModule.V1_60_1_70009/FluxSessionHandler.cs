using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V1_60_1_70009.Parsers
{
    // Classic 1.60.1.70009 session/auth/config packet layouts. Sourced from
    // TrinityCoreLuaSol (E:\TrinityCoreLuaSol) packet writers/readers:
    // AuthenticationPackets.cpp, ClientConfigPackets.cpp, HotfixPackets.cpp,
    // SystemPackets.cpp, MovementPackets.cpp (SuspendToken).
    // NOTE: for build 70009 the handler lookup never reaches the loaded
    // fallback modules (the first fallback hop, V5_5_0_61735, is a MoP-classic
    // build where HasFallback() == false stops the chain), so every opcode not
    // registered in this module falls back to the ancient Zero-build core
    // handlers — that is why AUTH_CHALLENGE/AUTH_SESSION/ACCOUNT_DATA_TIMES
    // desynced even though correct-looking handlers exist in V5_5/V4_4.
    public static class FluxSessionHandler
    {
        // ===== AuthenticationPackets.cpp =====

        // 0x4D0000 — AuthChallenge::Write: u32 DosChallenge[8] + byte
        // Challenge[32] + u8 DosZeroBits (65 bytes).
        [Parser(Opcode.SMSG_AUTH_CHALLENGE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleServerAuthChallenge160(Packet packet)
        {
            for (uint i = 0; i < 8; ++i)
                packet.ReadUInt32("DosChallenge", i);
            packet.ReadBytes("Challenge", 32);
            packet.ReadByte("DosZeroBits");
        }

        // 0x450001 — AuthSession::Read: u64 DosResponse + u32 RegionID +
        // u32 BattlegroupID + u32 RealmID + LocalChallenge[32] + Digest[24] +
        // bit UseIPv6 + u32 realmJoinTicketSize + ticket bytes (ticket size
        // clamped to remaining packet data, as TC does).
        [Parser(Opcode.CMSG_AUTH_SESSION, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleAuthSession160(Packet packet)
        {
            packet.ReadUInt64("DosResponse");
            packet.ReadUInt32("RegionID");
            packet.ReadUInt32("BattlegroupID");
            packet.ReadUInt32("RealmID");
            packet.ReadBytes("LocalChallenge", 32);
            packet.ReadBytes("Digest", 24);
            packet.ReadBit("UseIPv6");

            var realmJoinTicketSize = packet.ReadInt32();
            var remaining = (int)(packet.Length - packet.Position);
            packet.ReadBytes("RealmJoinTicket", Math.Min(Math.Max(realmJoinTicketSize, 0), Math.Max(remaining, 0)));
        }

        // 0x450003 — AuthContinuedSession::Read: u64 DosResponse +
        // LocalChallenge[32] + Digest[24] + u64 Key + u32 NativeRealmAddress +
        // u32 Key3.
        [Parser(Opcode.CMSG_AUTH_CONTINUED_SESSION, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleAuthContinuedSession160(Packet packet)
        {
            packet.ReadUInt64("DosResponse");
            packet.ReadBytes("LocalChallenge", 32);
            packet.ReadBytes("Digest", 24);
            packet.ReadUInt64("Key");
            packet.ReadUInt32("NativeRealmAddress");
            packet.ReadUInt32("Key3");
        }

        // 0x4D0004 — EnterEncryptedMode::Write: i32 RegionGroup +
        // byte Signature[64] + bit Enabled (69 bytes).
        [Parser(Opcode.SMSG_ENTER_ENCRYPTED_MODE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleEnterEncryptedMode160(Packet packet)
        {
            packet.ReadInt32("RegionGroup");
            packet.ReadBytes("Signature (ED25519)", 64);
            packet.ReadBit("Enabled");
        }

        // 0x450005 — handled at socket level in TC, empty payload.
        // 0x450000 — SuspendComms ack, empty payload.
        // 0x4D0005 / 0x4D0006 — SuspendComms / ResumeComms, empty payload.
        // 0x460003 — WaitQueueFinish, empty payload.
        [Parser(Opcode.CMSG_ENTER_ENCRYPTED_MODE_ACK, ClientVersionBuild.V1_60_1_70009)]
        [Parser(Opcode.CMSG_SUSPEND_COMMS_ACK, ClientVersionBuild.V1_60_1_70009)]
        [Parser(Opcode.SMSG_SUSPEND_COMMS, ClientVersionBuild.V1_60_1_70009)]
        [Parser(Opcode.SMSG_RESUME_COMMS, ClientVersionBuild.V1_60_1_70009)]
        [Parser(Opcode.SMSG_WAIT_QUEUE_FINISH, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleSessionEmpty160(Packet packet)
        {
        }

        // 0x460002 — WaitQueueUpdate::Write: AuthWaitInfo
        // {u32 WaitCount, u32 WaitTime, u8 AllowedFactionGroupForCharacterCreate,
        //  bit HasFCM, bit CanCreateOnlyIfExisting}.
        [Parser(Opcode.SMSG_WAIT_QUEUE_UPDATE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleWaitQueueUpdate160(Packet packet)
        {
            packet.ReadUInt32("WaitCount");
            packet.ReadUInt32("WaitTime");
            packet.ReadByte("AllowedFactionGroupForCharacterCreate");
            packet.ReadBit("HasFCM");
            packet.ReadBit("CanCreateOnlyIfExisting");
        }

        // NOTE: SMSG_SUSPEND_TOKEN / CMSG_SUSPEND_TOKEN_RESPONSE are already
        // registered in FluxMovementHandler.cs (u32 SequenceIndex + 2bit
        // Reason / u32 SequenceIndex) — do not re-register them here.

        // 0x440000 — ConnectToFailed::Read: u8 Con + u32 Serial.
        [Parser(Opcode.CMSG_CONNECT_TO_FAILED, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleConnectToFailed160(Packet packet)
        {
            packet.ReadByte("Con");
            packet.ReadUInt32("Serial");
        }

        // 0x45000B — QueuedMessagesEnd::Read: u32 Timestamp.
        [Parser(Opcode.CMSG_QUEUED_MESSAGES_END, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleQueuedMessagesEnd160(Packet packet)
        {
            packet.ReadUInt32("Timestamp");
        }

        // ===== ClientConfigPackets.cpp =====

        // 0x4601B5 — AccountDataTimes::Write: ObjectGuid PlayerGuid +
        // Timestamp<u64> ServerTime + Timestamp<u64> AccountTimes[20]
        // (NUM_ACCOUNT_DATA_TYPES == 20 in LuaSol).
        [Parser(Opcode.SMSG_ACCOUNT_DATA_TIMES, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleAccountDataTimes160(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadTime64("ServerTime");

            for (var i = 0; i < 20; ++i)
                packet.ReadTime64($"[{(AccountDataType)i}] Time", i);
        }

        // 0x4400C3 — RequestAccountData::Read: ObjectGuid PlayerGuid +
        // i32 DataType.
        [Parser(Opcode.CMSG_REQUEST_ACCOUNT_DATA, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleRequestAccountData160(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadInt32E<AccountDataType>("DataType");
        }

        // 0x4601B3 / 0x4400C4 — UpdateAccountData::Write /
        // UserClientUpdateAccountData::Read: u64 Time + u32 Size (decompressed)
        // + ObjectGuid + i32 DataType + u32 compressedSize + bytes.
        [Parser(Opcode.SMSG_UPDATE_ACCOUNT_DATA, ClientVersionBuild.V1_60_1_70009)]
        [Parser(Opcode.CMSG_UPDATE_ACCOUNT_DATA, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleUpdateAccountData160(Packet packet)
        {
            packet.ReadTime64("Time");
            var decompCount = packet.ReadInt32("Size");
            packet.ReadPackedGuid128("Guid");
            packet.ReadInt32E<AccountDataType>("DataType");
            var compCount = packet.ReadInt32("CompressedSize");

            var pkt = packet.Inflate(compCount, decompCount, false);
            var data = pkt.ReadWoWString(decompCount);

            packet.AddValue("Account Data", data);
        }

        // 0x4601B4 — UpdateAccountDataComplete::Write: ObjectGuid Player +
        // i32 DataType + i32 Result.
        [Parser(Opcode.SMSG_UPDATE_ACCOUNT_DATA_COMPLETE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleUpdateAccountDataComplete160(Packet packet)
        {
            packet.ReadPackedGuid128("Player");
            packet.ReadInt32E<AccountDataType>("DataType");
            packet.ReadInt32("Result");
        }

        // ===== SystemPackets.cpp =====

        // 0x460123 — SetTimeZoneInformation::Write: three SizedString<7>.
        [Parser(Opcode.SMSG_SET_TIME_ZONE_INFORMATION, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleSetTimeZoneInformation160(Packet packet)
        {
            var len1 = packet.ReadBits(7);
            var len2 = packet.ReadBits(7);
            var len3 = packet.ReadBits(7);

            packet.ReadWoWString("ServerTimeTZ", len1);
            packet.ReadWoWString("GameTimeTZ", len2);
            packet.ReadWoWString("ServerRegionalTZ", len3);
        }

        // 0x4602CE — marked STATUS_UNHANDLED in LuaSol, layout taken from the
        // retail-era reader (fits the 97-byte sniff: 7-bit len + Digest[32] +
        // SessionKey[64]).
        [Parser(Opcode.SMSG_UPDATE_BNET_SESSION_KEY, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleUpdateBnetSessionKey160(Packet packet)
        {
            var sessionKeyLength = (int)packet.ReadBits(7);

            packet.ReadBytes("Digest", 32);
            packet.ReadBytes("SessionKey", sessionKeyLength);
        }

        // ===== HotfixPackets.cpp =====

        // 0x4A0001 — AvailableHotfixes::Write: i32 VirtualRealmAddress +
        // u32 count + per entry {i32 PushID, u32 UniqueID}.
        [Parser(Opcode.SMSG_AVAILABLE_HOTFIXES, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleAvailableHotfixes160(Packet packet)
        {
            packet.ReadInt32("VirtualRealmAddress");
            var hotfixCount = packet.ReadUInt32("HotfixCount");
            for (var i = 0u; i < hotfixCount; ++i)
            {
                packet.ReadInt32("PushID", i, "HotfixUniqueID");
                packet.ReadUInt32("UniqueID", i, "HotfixUniqueID");
            }
        }

        // 0x4A0003 — HotfixConnect::Write: u32 count + per entry
        // {HotfixId {i32 PushID, u32 UniqueID} + u32 TableHash + i32 RecordID +
        //  u32 Size + 3bit HotfixStatus}, then u32 contentSize + bytes.
        [Parser(Opcode.SMSG_HOTFIX_CONNECT, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleHotfixConnect160(Packet packet)
        {
            var hotfixCount = packet.ReadUInt32("HotfixCount");
            for (var i = 0u; i < hotfixCount; ++i)
            {
                packet.ReadInt32("PushID", i, "HotfixRecord");
                packet.ReadUInt32("UniqueID", i, "HotfixRecord");
                packet.ReadUInt32("TableHash", i, "HotfixRecord");
                packet.ReadInt32("RecordID", i, "HotfixRecord");
                packet.ReadUInt32("Size", i);
                packet.ReadBits("HotfixStatus", 3, i);
            }

            var contentSize = packet.ReadUInt32("HotfixContentSize");
            packet.ReadBytes("HotfixContent", (int)contentSize);
        }

        // 0x440011 — HotfixRequest::Read: u32 ClientBuild + u32 DataBuild +
        // u32 count + i32 HotfixID[count].
        [Parser(Opcode.CMSG_HOTFIX_REQUEST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleHotfixRequest160(Packet packet)
        {
            packet.ReadUInt32("ClientBuild");
            packet.ReadUInt32("DataBuild");
            var hotfixCount = packet.ReadUInt32("HotfixCount");
            for (var i = 0u; i < hotfixCount; ++i)
                packet.ReadInt32("HotfixID", i);
        }
    }
}
