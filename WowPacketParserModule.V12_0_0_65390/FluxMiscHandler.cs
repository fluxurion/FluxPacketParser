using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V12_0_0_65390.Parsers
{
    public static class FluxMiscHandler
    {
        [Parser(Opcode.SMSG_BATTLE_PAY_DISPLAY_CARD)]
        public static void HandleBattlePayDisplayCard(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            ReadDisplayCard(packet);
        }

        [Parser(Opcode.CMSG_GET_REGIONWIDE_CHARACTER_RESTRICTION_AND_MAIL_DATA)]
        public static void HandleCmsgGetRegionwideCharacterRestrictionAndMailData(Packet packet)
        {
            var count = packet.ReadUInt32("CharacterCount");
            for (uint i = 0; i < count; i++)
                packet.ReadPackedGuid128("CharacterGuid", i);
        }

        [Parser(Opcode.SMSG_ACCOUNT_WARBAND_SCENE_UPDATE)]
        public static void HandleAccountWarbandSceneUpdate(Packet packet)
        {
            var flagByte = packet.ReadByte("Flags");
            packet.AddValue("HasData", (flagByte >> 7) != 0);

            var uintCount = packet.ReadUInt32("FieldCount");
            var boolCount1 = packet.ReadUInt32("BoolCount1");
            var boolCount2 = packet.ReadUInt32("BoolCount2");

            for (uint i = 0; i < uintCount; i++)
                packet.ReadUInt32("FieldValue", i);

            // Shared bitstream: MSB-first, count1 bits then count2 bits
            var totalBits = boolCount1 + boolCount2;
            ulong accumulator = 0;
            int bitsAvailable = 8; // triggers first read
            for (uint i = 0; i < totalBits; i++)
            {
                if (bitsAvailable == 8)
                {
                    accumulator = packet.ReadByte();
                    bitsAvailable = 0;
                }
                var bit = (accumulator & 0x80) != 0;
                accumulator <<= 1;
                bitsAvailable++;

                if (i < boolCount1)
                    packet.AddValue("BoolValue1", bit, i);
                else
                    packet.AddValue("BoolValue2", bit, i - boolCount1);
            }
        }

        [Parser(Opcode.SMSG_REGIONWIDE_CHARACTER_RESTRICTIONS_DATA)]
        public static void HandleRegionwideCharacterRestrictionsData(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (uint i = 0; i < count; i++)
            {
                var flags = packet.ReadByte("Flags", i);
                packet.AddValue("TopBits", (flags >> 5), i);
                packet.AddValue("IsRestricted", (flags & 0x10) != 0, i);
                packet.AddValue("CatchUpAvailable", (flags & 0x08) != 0, i);
                packet.ReadPackedGuid128("CharacterGUID", i);
                packet.ReadUInt32("RestrictionID", i);
            }
        }

        [Parser(Opcode.SMSG_SOCIAL_CONTRACT_REQUEST_RESPONSE)]
        public static void HandleSocialContractRequestResponse(Packet packet)
        {
            var value = packet.ReadByte("Byte");
            packet.AddValue("ShowContract", (value & 0x80) != 0);
        }

        [Parser(Opcode.SMSG_GENERATE_SSO_TOKEN_RESPONSE)]
        public static void HandleGenerateSsoTokenResponse(Packet packet)
        {
            packet.ReadUInt32("Field32");
            packet.ReadUInt32("Field36");
            packet.ReadTime64("TokenCreationTime");
            packet.ReadTime64("TokenExpirationTime");
            var stringLen = packet.ReadByte("StringLengthByte");
            var actualLength = (int)(stringLen >> 1);
            packet.ReadWoWString("TokenString", actualLength);
        }

        [Parser(Opcode.SMSG_GET_DECOR_REFUND_LIST_RESPONSE)]
        public static void HandleGetDecorRefundListResponse(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (uint i = 0; i < count; i++)
            {
                packet.ReadPackedGuid128("DecorGuid", i);
                packet.ReadUInt32("Field40", i);
                packet.ReadUInt64("Field48", i);
                var refundItemCount = packet.ReadUInt32("RefundItemCount", i);
                for (uint j = 0; j < refundItemCount; j++)
                {
                    packet.ReadPackedGuid128("ItemGuid", i, j);
                }
            }
        }

        [Parser(Opcode.SMSG_GET_ALL_LICENSED_DECOR_QUANTITIES_RESPONSE)]
        public static void HandleGetAllLicensedDecorQuantitiesResponse(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (uint i = 0; i < count; i++)
            {
                packet.ReadUInt32("DecorID", i);
                packet.ReadUInt32("Quantity", i);
                packet.ReadUInt32("Field8", i);
            }
        }

        [Parser(Opcode.SMSG_REGIONWIDE_CHARACTER_MAIL_DATA)]
        public static void HandleRegionwideCharacterMailData(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (uint i = 0; i < count; i++)
            {
                var typeByte = packet.ReadByte("Type", i);
                packet.AddValue("TypeMask", (typeByte >> 5), i);
                packet.ReadPackedGuid128("Guid", i);

                var mailSenderCount = packet.ReadUInt32("MailSenderCount", i);
                var mailSenderTypeCount = packet.ReadUInt32("MailSenderTypeCount", i);
                for (uint j = 0; j < mailSenderTypeCount; j++)
                    packet.ReadUInt32("MailSenderType", i, j);

                // Sender names: each preceded by a 6-bit compressed length (4 values per 3 bytes)
                ulong accumulator = 0;
                int bitsAvailable = 8;
                for (uint j = 0; j < mailSenderCount; j++)
                {
                    uint len;
                    if (bitsAvailable > 2)
                    {
                        var b = packet.ReadByte();
                        if (bitsAvailable == 8)
                        {
                            bitsAvailable = 6;
                            len = (uint)(b >> 2);
                            accumulator = (uint)(b << 6);
                        }
                        else
                        {
                            var carry = accumulator >> bitsAvailable;
                            bitsAvailable -= 2;
                            len = (uint)((carry << bitsAvailable) | ((uint)b >> (8 - bitsAvailable)));
                            accumulator = (uint)(b << bitsAvailable);
                        }
                    }
                    else
                    {
                        len = (uint)(accumulator >> 2);
                        accumulator <<= 6;
                        bitsAvailable += 6;
                    }

                    packet.ReadWoWString("MailSenderName", (int)len, i, j);
                }
            }
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
            // count = (flags3 >> 7) | (2 * (flags2 >> 2))
            // flags2 is multiplied by 4 (v6 = 4 * v23), then shifted right by 2 (v6 >> 2)
            var itemCount = (uint)((flags3 >> 7) | (2 * ((flags2 >> 2) & 0x3F)));
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
