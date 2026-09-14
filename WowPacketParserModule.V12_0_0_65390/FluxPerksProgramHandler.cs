using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V12_0_0_65390.Parsers
{
    public static class FluxPerksProgramHandler
    {
        // JamPerksVendorItem (12.1, struct size 56 / 49 bytes on the wire):
        // int64 +0, int32 +8..+44 (x10), bool +48, bool +49
        private static void ReadPerksVendorItem(Packet packet, params object[] index)
        {
            packet.ReadTime64("AvailableUntil", index);
            var vendorItemID = packet.ReadInt32("VendorItemID", index);
            var mountSourceSpellID = packet.ReadInt32("MountSourceSpellID", index);
            var battlePetSpeciesID = packet.ReadInt32("BattlePetSpeciesID", index);
            var transmogSetID = packet.ReadInt32("TransmogSetID", index);
            var itemModifiedAppearanceID = packet.ReadInt32("ItemModifiedAppearanceID", index);
            var transmogIllusionID = packet.ReadInt32("TransmogIllusionID", index);
            var toyID = packet.ReadInt32("ToyID", index);
            var price = packet.ReadInt32("Price", index);
            packet.ReadInt32("OriginalPrice", index);
            packet.ReadInt32("WarbandSceneID", index);

            var disabled = packet.ReadBit("Disabled", index);
            packet.ReadBit("DoesNotExpire", index);
            packet.ResetBitReader();

            PerksProgramVendorData vendorData = new PerksProgramVendorData
            {
                ItemID = vendorItemID,
                MountSourceSpellID = mountSourceSpellID,
                BattlePetSpeciesID = battlePetSpeciesID,
                TransmogSetID = transmogSetID,
                ItemModifiedAppearanceID = itemModifiedAppearanceID,
                TransmogIllusionID = transmogIllusionID,
                ToyID = toyID,
                Price = price,
                Disabled = disabled
            };
            Storage.PerksProgramVendorDatas.Add(vendorData, packet.TimeSpan);
        }

        [Parser(Opcode.CMSG_PERKS_PROGRAM_STATUS_REQUEST, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_PERKS_PROGRAM_REQUEST_PENDING_REWARDS, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_PERKS_PROGRAM_GET_RECENT_PURCHASES, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramZeroLength(Packet packet)
        {
        }

        // sub_7FF7CD4A1EE0 + sniff: int32 count, packed vendor guid, count x int32
        [Parser(Opcode.CMSG_PERKS_PROGRAM_ITEMS_REFRESHED, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramItemsRefreshed(Packet packet)
        {
            var itemCount = packet.ReadUInt32("ItemCount");
            packet.ReadPackedGuid128("VendorGuid");
            for (uint i = 0; i < itemCount; i++)
                packet.ReadUInt32("PerksVendorItemID", i);
        }

        // sub_7FF7CD4A1F90 + sniff: int32 item id, then packed vendor guid
        [Parser(Opcode.CMSG_PERKS_PROGRAM_REQUEST_PURCHASE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramRequestPurchase(Packet packet)
        {
            packet.ReadUInt32("PerksVendorItemID");
            packet.ReadPackedGuid128("VendorGuid");
        }

        // sub_7FF7CD4A2060 + sniff: int32 count, packed vendor guid, count x int32
        [Parser(Opcode.CMSG_PERKS_PROGRAM_REQUEST_CART_CHECKOUT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramRequestCartCheckout(Packet packet)
        {
            var itemCount = packet.ReadUInt32("ItemCount");
            packet.ReadPackedGuid128("VendorGuid");
            for (uint i = 0; i < itemCount; i++)
                packet.ReadUInt32("PerksVendorItemID", i);
        }

        // sub_7FF7CD4A20F0 + purchase sniff pattern: int32 item id, then packed vendor guid
        [Parser(Opcode.CMSG_PERKS_PROGRAM_REQUEST_REFUND, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramRequestRefund(Packet packet)
        {
            packet.ReadUInt32("PerksVendorItemID");
            packet.ReadPackedGuid128("VendorGuid");
        }

        // sub_7FF7CD4A21C0 + sniff pattern: bit, int32 item id, packed vendor guid
        [Parser(Opcode.CMSG_PERKS_PROGRAM_SET_FROZEN_VENDOR_ITEM, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramSetFrozenVendorItem(Packet packet)
        {
            packet.ReadBit("Set");
            packet.ResetBitReader();
            packet.ReadUInt32("PerksVendorItemID");
            packet.ReadPackedGuid128("VendorGuid");
        }

        // PerksRecentPurchasesData (stride 24): int32 +0, int64 +8, bool +16
        private static void ReadPerksRecentPurchasesData(Packet packet, params object[] index)
        {
            packet.ReadUInt32("VendorItemID", index);
            packet.ReadTime64("BuyTime", index);
            packet.ReadBit("Flag", index);
            packet.ResetBitReader();
        }

        [Parser(Opcode.SMSG_PERKS_PROGRAM_VENDOR_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramVendorUpdate(Packet packet)
        {
            var itemsCount = packet.ReadUInt32("ItemsCount");
            for (uint i = 0; i < itemsCount; i++)
                ReadPerksVendorItem(packet, i);
        }

        [Parser(Opcode.SMSG_PERKS_PROGRAM_ACTIVITY_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramActivityUpdate(Packet packet)
        {
            var activityCount = packet.ReadUInt32("ActivityCount");
            packet.ReadTime64("RemainingTime");
            packet.ReadTime64("StartingTime");
            packet.ReadInt32("UiThemeID");

            for (uint i = 0; i < activityCount; i++)
                packet.ReadInt32("ActivityID", i);

            packet.ReadBit("UnkBit");
        }

        // Payload is taken via GetReadPointer (raw data); read as a bare int32
        [Parser(Opcode.SMSG_PERKS_PROGRAM_ACTIVITY_COMPLETE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramActivityComplete(Packet packet)
        {
            packet.ReadInt32("ActivityID");
        }

        // JamPerksProgramResult_Unpack: type byte packs type (hi nibble),
        // a 2-bit field (bits 2-3) and a hasTrailingInt64 flag (bit1)
        [Parser(Opcode.SMSG_PERKS_PROGRAM_RESULT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramResult(Packet packet)
        {
            var typeByte = packet.ReadByte("TypeByte");
            var type = typeByte >> 4;
            packet.AddValue("Type", type);
            packet.AddValue("Field36", (typeByte & 0x0F) >> 2);
            var hasTrailingInt64 = (typeByte & 0x02) != 0;
            packet.AddValue("HasTrailingInt64", hasTrailingInt64);

            switch (type)
            {
                case 4: // Collectors Cache
                    packet.ReadUInt32("Field56");
                    packet.ReadUInt32("Field60");
                    packet.ReadUInt32("RewardAmount"); // Field64
                    var unkIntsCount = packet.ReadUInt32("UnkIntsCount");
                    for (uint i = 0; i < unkIntsCount; i++)
                        packet.ReadUInt32("UnkInt", i);
                    break;
                case 2: // BoughtItem
                case 3: // Same structure as case 2
                    packet.ReadUInt32("VendorItemID"); // Field96
                    var buyItemCount = packet.ReadUInt32("BuyItemCount");
                    for (uint i = 0; i < buyItemCount; i++)
                        ReadPerksRecentPurchasesData(packet, i);
                    break;
                case 5: // AvailableItems
                    packet.ReadPackedGuid128("VendorGuid");
                    packet.ReadPackedGuid128("ModelSceneCameraGuid");
                    var itemCount = packet.ReadUInt32("VendorItemCount");
                    packet.ReadInt32("Field184");
                    packet.ReadInt32("Field188");
                    packet.ReadInt32("Field192");
                    packet.ReadInt32("Field196");
                    packet.ReadInt32("Field200");
                    packet.ReadInt32("Field204");
                    packet.ReadInt32("Field208");
                    for (uint i = 0; i < itemCount; i++)
                        ReadPerksVendorItem(packet, i);
                    break;
                case 9: // Frozen items
                    var frozenCount = packet.ReadUInt32("FrozenItemCount");
                    for (uint i = 0; i < frozenCount; i++)
                        ReadPerksVendorItem(packet, "FrozenItems", i);
                    break;
                case 8:
                    packet.ReadInt32("UnkInt32"); // Field240
                    break;
                default:
                    break;
            }

            if (hasTrailingInt64)
                packet.ReadInt64("TrailingValue"); // Field40
        }

        // SMSG_RESPONSE_PERK_PENDING_REWARDS_Read: int32 count, then per element
        // (stride 120) a 3-bit type followed by type-specific tails
        [Parser(Opcode.SMSG_RESPONSE_PERK_PENDING_REWARDS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramPendingRewards(Packet packet)
        {
            var rewardsCount = packet.ReadUInt32("RewardsCount");
            for (uint i = 0; i < rewardsCount; i++)
            {
                var type = packet.ReadBits("Type", 3, i);

                // type 4: 24-bit string length continues the bitstream
                // (5 leftover bits of the type byte + 19 stream bits),
                // then the stream realigns (remaining 5 bits are padding)
                uint strLen = 0;
                if (type == 4)
                    strLen = packet.ReadBits("StringLength", 24, i);

                packet.ResetBitReader();

                if (type == 4)
                {
                    packet.ReadUInt32("Field56", i);
                    packet.ReadWoWString("String", (int)strLen, i); // Field64
                }

                packet.ReadPackedGuid128("BnetAccountID", i); // +8
                packet.ReadUInt32("Field24", i);

                switch (type)
                {
                    case 1:
                        packet.ReadUInt32("Field28", i);
                        packet.ReadUInt32("Field32", i);
                        break;
                    case 2:
                        packet.ReadUInt32("Field36", i);
                        packet.ReadUInt32("Field40", i);
                        break;
                    case 3:
                    case 7:
                        packet.ReadUInt32("Field44", i);
                        packet.ReadUInt32("Field48", i);
                        packet.ReadUInt32("Field52", i);
                        break;
                    case 5:
                        packet.ReadInt64("Field104", i);
                        break;
                    case 6:
                        packet.ReadUInt32("Field112", i);
                        break;
                    default:
                        break;
                }
            }
        }

        [Parser(Opcode.SMSG_RESPONSE_PERK_RECENT_PURCHASES, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramRecentPurchases(Packet packet)
        {
            var timesCount = packet.ReadUInt32("TimesCount");
            for (uint i = 0; i < timesCount; i++)
                ReadPerksRecentPurchasesData(packet, i);
        }

        // single bool
        [Parser(Opcode.SMSG_PERKS_PROGRAM_DISABLED, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksProgramDisabled(Packet packet)
        {
            packet.ReadBit("Disabled");
        }

        // two bools (bit7 and bit6 of the flag byte)
        [Parser(Opcode.SMSG_PERKS_ANIM_TOGGLE_KILL_SWITCH, ClientVersionBuild.V12_1_0_69214)]
        public static void HandlePerksAnimToggleKillSwitch(Packet packet)
        {
            packet.ReadBit("UnkBit7");
            packet.ReadBit("UnkBit6");
        }
    }
}
