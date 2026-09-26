using Google.Protobuf.WellKnownTypes;
using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;

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

        // 1.15.9 (0x4A0000): wire layout matches retail SMSG_DB_REPLY —
        // {u32 TableHash(DB2Hash), i32 RecordId, u32 Timestamp, u8 flag byte
        //  (top 3 bits = HotfixStatus), i32 dataSize, blob}. The shared opcode
        //  table maps 0x4A0000 to SMSG_UPDATE_OBJECT for 1.15.8; era uses the
        //  modern numbering where this value is DB_REPLY.
        [Parser(Opcode.SMSG_DB_REPLY, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleDBReply(Packet packet)
        {
            var dbReply = packet.Holder.DbReply = new();
            var type = packet.ReadUInt32E<DB2Hash>("TableHash");
            dbReply.TableHash = (uint)type;
            dbReply.RecordId = packet.ReadInt32("RecordID");
            var timeStamp = packet.ReadUInt32();
            var time = packet.AddValue("Timestamp", Utilities.GetDateTimeFromUnixTime(timeStamp));
            dbReply.Time = Timestamp.FromDateTime(DateTime.SpecifyKind(time, DateTimeKind.Utc));
            packet.ResetBitReader();
            var status = packet.ReadBitsE<HotfixStatus>("Status", 3);
            packet.ResetBitReader();
            switch (status)
            {
                case HotfixStatus.Valid:
                    dbReply.Status = PacketDbReplyRecordStatus.RecordStatusValid;
                    break;
                case HotfixStatus.RecordRemoved:
                    dbReply.Status = PacketDbReplyRecordStatus.RecordStatusRecordRemoved;
                    break;
                case HotfixStatus.Invalid:
                    dbReply.Status = PacketDbReplyRecordStatus.RecordStatusInvalid;
                    break;
                case HotfixStatus.NotPublic:
                    dbReply.Status = PacketDbReplyRecordStatus.RecordStatusNotPublic;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var size = packet.ReadInt32("Size");
            packet.ReadBytes(size);
        }
    }
}
