using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{
    public static class PerksProgramHandler
    {
        [Parser(Opcode.SMSG_PERKS_PROGRAM_ACTIVITY_UPDATE)]
        public static void HandlePerksProgramActivityUpdate(Packet packet)
        {
            var activityCount = packet.ReadUInt32("ActivityCount");
            packet.ReadTime("TimeUntilEnd");
            packet.ReadTime("TimeUntilStart");

            for (var i = 0; i < activityCount; i++)
                packet.ReadInt32("ActivityID", i);
        }

        [Parser(Opcode.SMSG_PERKS_PROGRAM_ACTIVITY_COMPLETE)]
        public static void HandlePerksProgramActivityComplete(Packet packet)
        {
            packet.ReadInt32("ActivityID");
        }

        [Parser(Opcode.CMSG_PERKS_PROGRAM_REQUEST_PURCHASE)]
        public static void HandlePerksProgramRequestPurchase(Packet packet)
        {
            packet.ReadUInt32("PerksVendorItemID");
            packet.ReadPackedGuid128("VendorGuid");
        }

        [Parser(Opcode.CMSG_PERKS_PROGRAM_SET_FROZEN_VENDOR_ITEM)]
        public static void HandlePerksProgramSetFrozenVendorItem(Packet packet)
        {
            packet.ReadBool("UnkBool");
            packet.ReadUInt32("PerksVendorItemID");
            packet.ReadPackedGuid128("VendorGuid");
        }

        [Parser(Opcode.CMSG_PERKS_PROGRAM_STATUS_REQUEST)]
        public static void HandlePerksProgramStatusRequest(Packet packet)
        {
            // No payload
        }

        [Parser(Opcode.CMSG_PERKS_PROGRAM_REQUEST_PENDING_REWARDS)]
        public static void HandlePerksProgramPendingReward(Packet packet)
        {
            // No payload
        }

        [Parser(Opcode.SMSG_PERKS_PROGRAM_RESULT)]
        public static void HandlePerksProgramResult(Packet packet)
        {
            var type = packet.ReadBits("Type", 4);
            var hasUnkLong = packet.ReadBit("HasUnkLong");
            packet.ResetBitReader();
            if (hasUnkLong)
                packet.ReadUInt64("UnkLong");

            switch (type)
            {
                case 2: // BoughtItem
                    packet.ReadUInt32("VendorItemID");
                    var buyItemCount = packet.ReadUInt32("BuyItemCount");
                    for (var i = 0; i < buyItemCount; ++i)
                    {
                        packet.ReadUInt32("VendorItemID", i);
                        packet.ReadTime64("BuyTime", i);
                    }
                    break;
                case 4: // Unk4
                    packet.ReadUInt32("UnkInt1");
                    packet.ReadUInt32("UnkInt2");
                    packet.ReadUInt32("UnkInt3");
                    var unkIntsCount = packet.ReadUInt32("UnkIntsCount");
                    for (var i = 0; i < unkIntsCount; ++i)
                        packet.ReadUInt32("UnkInt", i);
                    break;
                case 5: // AvailableItems
                    packet.ReadPackedGuid128("VendorGuid");
                    packet.ReadPackedGuid128("ModelSceneCameraGuid");
                    var frozenCount = packet.ReadUInt32("FrozenPerksVendorItemsCount");
                    for (var i = 0; i < frozenCount; ++i)
                        ReadPerksVendorItem(packet, i);
                    break;
                default:
                    break;
            }
        }

        [Parser(Opcode.SMSG_RESPONSE_PERK_RECENT_PURCHASES)]
        public static void HandlePerksProgramRecentPurchases(Packet packet)
        {
            var timesCount = packet.ReadUInt32("TimesCount");
            for (var i = 0; i < timesCount; ++i)
            {
                packet.ReadUInt32("VendorItemID", i);
                packet.ReadTime64("BuyTime", i);
                packet.ReadBool("UnkBit", i);
                packet.ResetBitReader();
            }
        }

        [Parser(Opcode.SMSG_RESPONSE_PERK_PENDING_REWARDS)]
        public static void HandlePerksProgramPendingRewards(Packet packet)
        {
            var rewardsCount = packet.ReadBits("RewardsCount", 3);
            packet.ResetBitReader();
            for (var i = 0; i < rewardsCount; ++i)
            {
                // If Type == 4, read case4 first
                var type = packet.ReadByte("Type", i); // You may need to adjust this if Type is not at a fixed offset
                if (type == 4)
                {
                    packet.ReadBytes("UnkBytes", 2, i);
                    packet.ReadUInt32("UnkInt1", i);
                    packet.ReadCString("UnkStr", i);
                }

                packet.ReadPackedGuid128("BnetAccountID", i);
                packet.ReadUInt32("UnkInt1", i);
                type = packet.ReadByte("Type", i);

                switch (type)
                {
                    case 1:
                        packet.ReadUInt32("case1_UnkInt1", i);
                        break;
                    case 2:
                        packet.ReadUInt32("case2_UnkInt1", i);
                        packet.ReadUInt32("case2_UnkInt2", i);
                        break;
                    case 3:
                        packet.ReadUInt32("case3_UnkInt1", i);
                        packet.ReadUInt32("case3_UnkInt2", i);
                        packet.ReadUInt32("case3_UnkInt3", i);
                        break;
                    case 5:
                        packet.ReadUInt64("case5_UnkLong", i);
                        break;
                    case 6:
                        packet.ReadUInt32("case6_UnkInt1", i);
                        break;
                    case 7:
                        packet.ReadUInt32("case7_UnkInt1", i);
                        packet.ReadUInt32("case7_UnkInt2", i);
                        packet.ReadUInt32("case7_UnkInt3", i);
                        break;
                    default:
                        break;
                }
            }
        }

        [Parser(Opcode.SMSG_PERKS_PROGRAM_VENDOR_UPDATE)]
        public static void HandlePerksProgramVendorUpdate(Packet packet)
        {
            var itemsCount = packet.ReadUInt32("ItemsCount");
            for (var i = 0; i < itemsCount; ++i)
                ReadPerksVendorItem(packet, i);
        }

        // Helper for PerksVendorItem
        private static void ReadPerksVendorItem(Packet packet, int index)
        {
            packet.ResetBitReader();

            // AvailableUntil (Retail 11.1.7+)
            if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_1_7_61491))
                packet.ReadTime64("AvailableUntil", index);


            var _VendorItemID = packet.ReadInt32("VendorItemID", index);
            var _MountID = packet.ReadInt32("MountID", index);
            var _BattlePetSpeciesID = packet.ReadInt32("BattlePetSpeciesID", index);
            var _TransmogSetID = packet.ReadInt32("TransmogSetID", index);
            var _ItemModifiedAppearanceID = packet.ReadInt32("ItemModifiedAppearanceID", index);
            var _TransmogIllusionID = packet.ReadInt32("TransmogIllusionID", index);
            var _ToyID = packet.ReadInt32("ToyID", index);
            var _Price = packet.ReadInt32("Price", index);

            if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_0_7_58123)
                || ClientVersion.AddedInVersion(ClientBranch.Cata, ClientVersionBuild.V4_4_2_59185)
                || ClientVersion.AddedInVersion(ClientBranch.Classic, ClientVersionBuild.V1_15_6_58797)
                || ClientVersion.AddedInVersion(ClientBranch.WotLK, ClientVersionBuild.V3_4_4_59817))
                packet.ReadInt32("OriginalPrice", index);

            // AvailableUntil (not Retail or removed in Retail 11.1.7+)
            if (ClientVersion.Branch != ClientBranch.Retail
                || ClientVersion.RemovedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_1_7_61491))
                packet.ReadTime64("AvailableUntil", index);

            if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_1_0_59347)
                || ClientVersion.AddedInVersion(ClientBranch.Classic, ClientVersionBuild.V1_15_7_60000)
                || ClientVersion.AddedInVersion(ClientBranch.WotLK, ClientVersionBuild.V3_4_4_59817))
                packet.ReadInt32("WarbandSceneID", index);

            var _Disabled = packet.ReadBit("Disabled", index);

            if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_0_5_57171)
                || ClientVersion.AddedInVersion(ClientBranch.Cata, ClientVersionBuild.V4_4_2_59185)
                || ClientVersion.AddedInVersion(ClientBranch.WotLK, ClientVersionBuild.V3_4_4_59817))
                packet.ReadBit("DoesNotExpire", index);

            PerksProgramVendorData perksvendordata = new PerksProgramVendorData
            {
                ItemID =  _VendorItemID,
                MountID = _MountID,
                BattlePetSpeciesID = _BattlePetSpeciesID,
                TransmogSetID = _TransmogSetID,
                ItemModifiedAppearanceID = _ItemModifiedAppearanceID,
                TransmogIllusionID = _TransmogIllusionID,
                ToyID = _ToyID,
                Price = _Price,
                Disabled = _Disabled
            };
            Storage.PerksProgramVendorDatas.Add(perksvendordata, packet.TimeSpan);

        }
    }
}
