using System;
using System.Reflection;
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
            packet.ReadTime64("TimeUntilEnd");
            packet.ReadTime64("TimeUntilStart");
            packet.ReadInt32("MonthlyProgress");

            for (var i = 0; i < activityCount; i++)
            {
                packet.ReadInt32("ActivityID", i);
            }
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
                        packet.ReadByte("Flags");
                    }
                    break;
                case 4: // Collectors Cache
                    packet.ReadUInt32("UnkInt1");
                    packet.ReadUInt32("UnkInt2");
                    packet.ReadUInt32("RewardAmount"); // Monthly 500 Trader's Tender
                    var unkIntsCount = packet.ReadUInt32("UnkIntsCount");
                    for (var i = 0; i < unkIntsCount; ++i)
                        packet.ReadUInt32("UnkInt", i);
                    break;
                case 5: // AvailableItems
                    packet.ReadPackedGuid128("VendorGuid");
                    packet.ReadPackedGuid128("ModelSceneCameraGuid");
                    var itemCount = packet.ReadUInt32("VendorItemCount");
                    for (var i = 0; i < itemCount; ++i)
                        ReadPerksVendorItem(packet, i);
                    if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V12_0_0_65390))
                    {
                        packet.ReadByte("UnkByte1");
                        packet.ReadByte("UnkByte2");
                        packet.ReadInt32("CurrencyID");
                        packet.ReadInt16("UnkInt16");
                        packet.ReadInt32("UnkInt32");
                        ReadPerksVendorItem(packet, 999);
                        packet.ReadInt32("UnkInt32");
                        packet.ReadInt16("UnkInt16");
                        packet.ReadByte("UnkByte");
                        packet.ReadInt16("UnkInt16");
                    }
                    break;
                case 8:
                    packet.ReadInt32("UnkInt32");
                    break;
                case 10:
                    packet.ReadInt32("UnkInt32");
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
            // 11.1.7+ and 12.0 move AvailableUntil to the front
            if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_1_7_61491)
                || ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V12_0_0_65390))
            {
                packet.ReadTime64("AvailableUntil", index); // 8 bytes
            }
            int _VendorItemID = 0, _MountSourceSpellID = 0, _BattlePetSpeciesID = 0, _TransmogSetID = 0;
            int _ItemModifiedAppearanceID = 0, _TransmogIllusionID = 0, _ToyID = 0, _Price = 0;
            if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V12_0_0_65390))
            {
                _VendorItemID = packet.ReadInt32("VendorItemID", index);
                packet.ReadInt32("MountSourceSpellID", index);
                packet.ReadInt32("BattlePetSpeciesID", index);
                packet.ReadInt32("TransmogSetID", index);
                packet.ReadInt32("ItemModifiedAppearanceID", index);

                if (_VendorItemID > 0)
                {
                    packet.ReadInt32("TransmogIllusionID", index);
                    packet.ReadInt32("ToyID", index);
                    packet.ReadInt32("Price", index);
                    packet.ReadInt32("OriginalPrice?", index);
                    packet.ReadInt32("WarbandSceneID?", index);
                    packet.ReadByte("Flag", index);
                }
            }
            else
            {
                // Standard Legacy Order (10.x / 11.x)
                _VendorItemID = packet.ReadInt32("VendorItemID", index);
                _MountSourceSpellID = packet.ReadInt32("MountSourceSpellID", index);
                _BattlePetSpeciesID = packet.ReadInt32("BattlePetSpeciesCreatureID", index);
                _TransmogSetID = packet.ReadInt32("TransmogSetID", index);
                _ItemModifiedAppearanceID = packet.ReadInt32("ItemModifiedAppearanceID", index);
                _TransmogIllusionID = packet.ReadInt32("TransmogIllusionID", index);
                _ToyID = packet.ReadInt32("ToyID", index);
                _Price = packet.ReadInt32("Price", index);
                if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_0_7_58123))
                    packet.ReadInt32("OriginalPrice", index);
                if (ClientVersion.RemovedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_1_7_61491))
                    packet.ReadTime64("AvailableUntil", index);
                if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_1_0_59347))
                    packet.ReadInt32("WarbandSceneID", index);
                var _Disabled = false;
                // Bits only exist in pre-12.0
                if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V10_2_0_52038))
                {
                    packet.ReadBit("isFrozen", index);
                    packet.ReadBit("isPurchased", index);
                    packet.ReadBit("isRefundable", index);
                    packet.ReadBit("isAvailable", index);
                }
                else
                    _Disabled = packet.ReadBit("Disabled", index);
                // DB Storage Condition for Dragonflight (10.x)
                if (ClientVersion.AddedInVersion(ClientBranch.Retail, ClientVersionBuild.V10_0_0_46181) &&
                    ClientVersion.RemovedInVersion(ClientBranch.Retail, ClientVersionBuild.V11_0_0_55666))
                {
                    PerksProgramVendorData perksvendordata = new PerksProgramVendorData
                    {
                        ItemID = _VendorItemID,
                        MountSourceSpellID = _MountSourceSpellID,
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
    }
}
