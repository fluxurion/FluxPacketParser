using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using System.Collections.Generic;

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{
    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        [Parser(Opcode.CMSG_UPDATE_VAS_PURCHASE_STATES)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        private static void ReadVisualMetadata(Packet packet, params object[] index)
        {
            packet.ResetBitReader();
            
            var hasCreatureDisplayID = packet.ReadBit("HasCreatureDisplayID", index);
            var hasItemID = packet.ReadBit("HasItemID", index);
            var titleLen = packet.ReadBits("TitleLength", 10, index);
            var title2Len = packet.ReadBits("Title2Length", 10, index);
            var descriptionLen = packet.ReadBits("DescriptionLength", 13, index);
            var description2Len = packet.ReadBits("Description2Length", 13, index);
            var description3Len = packet.ReadBits("Description3Length", 13, index);
            var hasIconBorder = packet.ReadBit("HasIconBorder", index);
            var hasUnknown1 = packet.ReadBit("HasUnknown1", index);
            var hasUiTextureAtlasMemberID = packet.ReadBit("HasUiTextureAtlasMemberID", index);
            var hasUiTextureAtlasMemberID2 = packet.ReadBit("HasUiTextureAtlasMemberID2", index);
            var description4Len = packet.ReadBits("Description4Length", 13, index);
            var description5Len = packet.ReadBits("Description5Length", 12, index);

            var variationCount = packet.ReadUInt32("VisualsCount", index);
            var cardType = packet.ReadInt32("CardType", index);
            var unknown3 = packet.ReadInt32("Unknown3", index);
            var productMultiplier = packet.ReadInt32("ProductMultiplier", index);

            var creatureDisplayID = 0;
            if (hasCreatureDisplayID)
                creatureDisplayID = (int)packet.ReadInt32("CreatureDisplayID", index);

            var itemID = 0;
            if (hasItemID)
                itemID = (int)packet.ReadInt32("ItemID", index);

            var title = "";
            if (titleLen > 0)
                title = packet.ReadWoWString("Title", (int)titleLen, index);

            var title2 = "";
            if (title2Len > 0)
                title2 = packet.ReadWoWString("Title2", (int)title2Len, index);

            var description = "";
            if (descriptionLen > 0)
                description = packet.ReadWoWString("Description", (int)descriptionLen, index);

            var description2 = "";
            if (description2Len > 0)
                description2 = packet.ReadWoWString("Description2", (int)description2Len, index);

            var description3 = "";
            if (description3Len > 0)
                description3 = packet.ReadWoWString("Description3", (int)description3Len, index);

            var iconBorder = 0;
            if (hasIconBorder)
                iconBorder = (int)packet.ReadInt32("IconBorder", index);

            var unknown1 = 0;
            if (hasUnknown1)
                unknown1 = (int)packet.ReadInt32("Unknown1", index);

            var uiTextureAtlasMemberID = 0;
            if (hasUiTextureAtlasMemberID)
                uiTextureAtlasMemberID = (int)packet.ReadInt32("UiTextureAtlasMemberID", index);

            var uiTextureAtlasMemberID2 = 0;
            if (hasUiTextureAtlasMemberID2)
                uiTextureAtlasMemberID2 = (int)packet.ReadInt32("UiTextureAtlasMemberID2", index);

            var description4 = "";
            if (description4Len > 0)
                description4 = packet.ReadWoWString("Description4", (int)description4Len, index);

            var description5 = "";
            if (description5Len > 0)
                description5 = packet.ReadWoWString("Description5", (int)description5Len, index);

            var creatureDisplayIDs = new List<uint>();
            var previewUIModelSceneIDs = new List<uint>();
            var transmogSetIDs = new List<uint>();
            var visualNames = new List<string>();

            for (var i = 0u; i < variationCount; i++)
            {
                packet.ResetBitReader();
                var variationNameLen = packet.ReadBits("VisualNameLength", 10, index, i);

                creatureDisplayIDs.Add(packet.ReadUInt32("CreatureDisplayID", index, i));
                previewUIModelSceneIDs.Add(packet.ReadUInt32("PreviewUIModelSceneID", index, i));
                transmogSetIDs.Add(packet.ReadUInt32("TransmogSetID", index, i));

                if (variationNameLen > 0)
                    visualNames.Add(packet.ReadWoWString("VisualName", (int)variationNameLen, index, i));
            }
        }

        private static void ReadProductInfo(Packet packet, params object[] index)
        {
            var productid = packet.ReadUInt32("ProductID", index);
            var normalprice = packet.ReadUInt64("NormalPrice", index);
            var currentprice = packet.ReadUInt64("CurrentPrice", index);
            
            var deliverableCount = packet.ReadUInt32("DeliverableProductIDCount", index);
            var unknown1 = packet.ReadUInt32("Unknown1", index);

            uint unknown2 = 0;
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_2_0_52038))
                unknown2 = packet.ReadUInt32("Unknown2", index);

            var deliverableProductIDsSize = packet.ReadUInt32("DeliverableProductIDsSize", index);
            var productinfoflags = packet.ReadUInt32("ProductInfoFlags", index);

            var deliverableProducts = new List<uint>();
            for (uint i = 0; i < deliverableCount; i++)
                deliverableProducts.Add(packet.ReadUInt32("DeliverableProductID", index, i));

            for (uint i = 0; i < deliverableProductIDsSize; i++)
                packet.ReadUInt32("UnknownInt", index, i);

            packet.ResetBitReader();
            var hasVisualMetadata = packet.ReadBit("HasVisualMetadata", index);

            BattlePayProductInfo productInfo = new BattlePayProductInfo
            {
                Entry = (uint)index[0],
                ProductInfoID = productid,
                NormalPrice = (long)normalprice,
                CurrentPrice = (long)currentprice,
                ProductInfoFlags = (int)productinfoflags,
                Unknown2 = (int)unknown1,
                Unknown3 = (int)unknown2,
                Unknown4 = 0,
                Unknown5 = 0,
                DeliverableProductIDs = string.Join(",", deliverableProducts),
                ChoiceType = 0,
                DisplayFlag = 0,
                HasUnknown1InDisplayInfo = 0,
                HasBattlePayDisplayInfo = hasVisualMetadata ? 1 : 0
            };
            Storage.BattlePayProductInfos.Add(productInfo, packet.TimeSpan);

            if (hasVisualMetadata)
                ReadVisualMetadata(packet, index);
        }

        private static void ReadProductItem(Packet packet, params object[] index)
        {
            packet.ReadUInt32("ID", index);
            packet.ReadByte("UnknownByte", index);
            packet.ReadUInt32("ItemID", index);
            packet.ReadUInt32("Quantity", index);
            packet.ReadUInt32("UnknownInt1", index);
            packet.ReadUInt32("UnknownInt2", index);

            packet.ResetBitReader();
            var isPet = packet.ReadBit("IsPet", index);
            var hasPetResult = packet.ReadBit("HasPetResult", index);
            var petResultFlags = packet.ReadBits("PetResultFlags", 4, index);
            var hasVisualMetadata = packet.ReadBit("HasVisualMetadata", index);

            if (hasVisualMetadata)
                ReadVisualMetadata(packet, index);
        }

        private static void ReadProduct(Packet packet, params object[] index)
        {
            var productid = packet.ReadUInt32("ProductID", index);
            var type = packet.ReadUInt32("Type", index);
            var itemid = packet.ReadUInt32("ItemID", index);
            var itemcount = packet.ReadUInt32("ItemCount", index);
            var mountspellid = packet.ReadUInt32("MountSpellID", index);
            var battlepetspeciescreatureid = packet.ReadUInt32("BattlePetSpeciesCreatureID", index);
            var unknown1 = packet.ReadUInt32("Unknown1", index);
            var unknown2 = packet.ReadUInt32("Unknown2", index);
            var unknown3 = packet.ReadUInt32("Unknown3", index);
            var transmogsetid = packet.ReadUInt32("TransmogSetID", index);
            var unknown8 = packet.ReadUInt32("Unknown8", index);
            var unknown9 = packet.ReadUInt32("Unknown9", index);

            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 7, index);
            var alreadyowned = packet.ReadBit("AlreadyOwned", index);
            var hasUnknownBits = packet.ReadBit("HasUnknownBits", index);
            var itemCountBits = packet.ReadBits("ItemCount", 7, index);
            var hasdisplayinfo = packet.ReadBit("HasDisplayInfo", index);
            
            uint petresultvariable = 0;
            if (hasUnknownBits)
                petresultvariable = packet.ReadBits("PetResultVariable", 4, index);

            for (uint i = 0; i < itemCountBits; i++)
                ReadProductItem(packet, index, i);

            var name = packet.ReadWoWString("Name", (int)nameLen, index);

            BattlePayProduct product = new BattlePayProduct
            {
                Entry = (uint)index[0],
                ProductID = productid,
                Type = (int)type,
                ItemID = itemid,
                ItemCount = itemcount,
                MountSpellID = mountspellid,
                BattlePetSpeciesCreatureID = battlepetspeciescreatureid,
                Unknown1 = unknown1,
                Unknown2 = unknown2,
                Unknown3 = unknown3,
                TransmogSetID = transmogsetid,
                Unknown8 = unknown8,
                Unknown9 = unknown9,
                Unknown10 = 0,
                Unknown11 = 0,
                Name = name,
                AlreadyOwned = alreadyowned ? 1 : 0,
                HasDisplayInfo = hasdisplayinfo ? 1 : 0,
                PetResultVariable = petresultvariable,
                DisplayFlag = 0
            };
            Storage.BattlePayProductDatas.Add(product, packet.TimeSpan);

            if (hasdisplayinfo)
                ReadVisualMetadata(packet, index);
        }

        private static void ReadGroup(Packet packet, params object[] index)
        {
            var groupid = packet.ReadUInt32("GroupID", index);
            var iconfiledataid = packet.ReadUInt32("IconFileDataID", index);
            var displaytype = packet.ReadByte("DisplayType", index);
            var ordering = packet.ReadUInt32("Ordering", index);
            var unknown = packet.ReadUInt32("Unknown", index);
            var maingroupid = packet.ReadUInt32("MainGroupID", index);

            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 8, index);
            var descLen = packet.ReadBits("DescriptionLength", 24, index);
            
            var name = packet.ReadWoWString("Name", (int)nameLen, index);
            
            packet.ResetBitReader();
            var hasVisualMetadata = packet.ReadBit("HasVisualMetadata", index);
            
            if (hasVisualMetadata)
                ReadVisualMetadata(packet, index);

            BattlePayGroup group = new BattlePayGroup
            {
                GroupID = groupid,
                IconFileDataID = iconfiledataid,
                DisplayType = displaytype,
                Ordering = ordering,
                Unk = unknown,
                MainGroupID = maingroupid,
                Name = name,
                Description = ""
            };
            Storage.BattlePayGroups.Add(group, packet.TimeSpan);
        }

        private static void ReadShop(Packet packet, params object[] index)
        {
            var entryid = packet.ReadUInt32("EntryID", index);
            var groupid = packet.ReadUInt32("GroupID", index);
            var productid = packet.ReadUInt32("ProductID", index);
            var ordering = packet.ReadUInt32("Ordering", index);
            var vasservicetype = packet.ReadUInt32("VasServiceType", index);
            var storedeliverytype = packet.ReadByte("StoreDeliveryType", index);

            packet.ResetBitReader();
            var hasVisualMetadata = packet.ReadBit("HasVisualMetadata", index);

            BattlePayShop shop = new BattlePayShop
            {
                Entry = (uint)index[0],
                EntryID = entryid,
                GroupID = groupid,
                ProductID = productid,
                Ordering = ordering,
                VasServiceType = vasservicetype,
                StoreDeliveryType = storedeliverytype,
                HasBattlePayDisplayInfo = hasVisualMetadata ? 1 : 0,
                Unknown = 0,
                DisplayFlag = 0
            };
            Storage.BattlePayShopDatas.Add(shop, packet.TimeSpan);

            if (hasVisualMetadata)
                ReadVisualMetadata(packet, index);
        }

        private static void ReadDistributionObject(Packet packet, params object[] index)
        {
            packet.ReadUInt64("DistributionID", index);
            packet.ReadUInt32("Status", index);
            packet.ReadUInt32("ProductID", index);
            packet.ReadPackedGuid128("TargetPlayer", index);
            packet.ReadPackedGuid128("UnknownGuid", index);
            packet.ReadUInt32("TargetVirtualRealm", index);
            packet.ReadUInt32("TargetNativeRealm", index);
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadUInt32("Unknown55AC", index);

            packet.ResetBitReader();
            var hasProduct = packet.ReadBit("HasProduct", index);
            packet.ReadBit("Revoked", index);

            if (hasProduct)
                ReadProduct(packet, index);
        }

        private static void ReadPurchase(Packet packet, params object[] index)
        {
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadUInt32("Status", index);
            packet.ReadUInt32("ResultCode", index);
            packet.ReadUInt32("ProductID", index);
            packet.ReadUInt64("UnknownLong1", index);
            packet.ReadUInt64("UnknownLong2", index);
            packet.ReadUInt32("UnknownInt", index);

            packet.ResetBitReader();
            var walletLen = packet.ReadBits("WalletNameLength", 8, index);
            packet.ReadWoWString("WalletName", (int)walletLen, index);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PURCHASE_LIST_RESPONSE)]
        public static void HandlePurchaseListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            var purchaseCount = packet.ReadUInt32("PurchaseCount");
            for (uint i = 0; i < purchaseCount; i++)
                ReadPurchase(packet, i);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE)]
        public static void HandleDistributionListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ResetBitReader();
            var objectCount = packet.ReadBits("DistributionObjectCount", 11);
            for (uint i = 0; i < objectCount; i++)
                ReadDistributionObject(packet, i);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_DISTRIBUTION_UPDATE)]
        public static void HandleDistributionUpdate(Packet packet)
        {
            ReadDistributionObject(packet);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE)]
        public static void HandleProductListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_0_0_46181))
                packet.ReadUInt32("CurrencyID");

            var productInfoCount = packet.ReadUInt32("ProductInfoCount");
            var productCount = packet.ReadUInt32("ProductCount");
            var groupCount = packet.ReadUInt32("ProductGroupCount");
            var shopCount = packet.ReadUInt32("ShopCount");

            for (uint i = 0; i < productInfoCount; i++)
                ReadProductInfo(packet, i);

            for (uint i = 0; i < productCount; i++)
                ReadProduct(packet, i);

            for (uint i = 0; i < groupCount; i++)
                ReadGroup(packet, i);

            for (uint i = 0; i < shopCount; i++)
                ReadShop(packet, i);
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_START_PURCHASE)]
        public static void HandleStartPurchase(Packet packet)
        {
            packet.ReadUInt32("ClientToken");
            packet.ReadUInt32("ProductID");
            packet.ReadPackedGuid128("TargetCharacter");
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_START_PURCHASE_RESPONSE)]
        public static void HandleStartPurchaseResponse(Packet packet)
        {
            packet.ReadUInt64("PurchaseID");
            packet.ReadUInt32("PurchaseResult");
            packet.ReadUInt32("ClientToken");
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_ACK_FAILED)]
        public static void HandleBattlePayAckFailed(Packet packet)
        {
            packet.ReadUInt64("PurchaseID");
            packet.ReadUInt32("PurchaseResult");
            packet.ReadUInt32("ClientToken");
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_PURCHASE_UPDATE)]
        public static void HandlePurchaseUpdate(Packet packet)
        {
            var purchaseCount = packet.ReadUInt32("PurchaseCount");
            for (uint i = 0; i < purchaseCount; i++)
                ReadPurchase(packet, i);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_CONFIRM_PURCHASE)]
        public static void HandleConfirmPurchase(Packet packet)
        {
            packet.ReadUInt64("PurchaseID");
            packet.ReadUInt32("ServerToken");
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_CONFIRM_PURCHASE_RESPONSE)]
        public static void HandleConfirmPurchaseResponse(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("ConfirmPurchase");
            packet.ReadUInt32("ServerToken");
            packet.ReadUInt64("ClientCurrentPriceFixedPoint");
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_ACK_FAILED_RESPONSE)]
        public static void HandleBattlePayAckFailedResponse(Packet packet)
        {
            packet.ReadUInt32("ServerToken");
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_DELIVERY_ENDED)]
        public static void HandleDeliveryEnded(Packet packet)
        {
            packet.ReadUInt64("DistributionID");
            var itemCount = packet.ReadInt32("ItemCount");
            for (int i = 0; i < itemCount; i++)
                Substructures.ItemHandler.ReadItemInstance(packet, i);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_DELIVERY_STARTED)]
        public static void HandleBattlePayDeliveryStarted(Packet packet)
        {
            packet.ReadUInt64("DistributionID");
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_TO_TARGET)]
        public static void HandleDistributionAssignToTarget(Packet packet)
        {
            packet.ReadUInt32("ProductID");
            packet.ReadUInt64("DistributionID");
            packet.ReadPackedGuid128("CharGUID");
            packet.ReadInt32("SpecID");
        }

        [Parser(Opcode.SMSG_CHARACTER_UPGRADE_STARTED)]
        public static void HandleCharacterUpgradeStarted(Packet packet)
        {
            packet.ReadPackedGuid128("CharGUID");
        }

        [Parser(Opcode.SMSG_CHARACTER_UPGRADE_COMPLETE)]
        public static void HandleCharacterUpgradeComplete(Packet packet)
        {
            packet.ReadPackedGuid128("CharGUID");
            var itemCount = packet.ReadInt32("LoadOutItemCount");
            for (int i = 0; i < itemCount; i++)
                packet.ReadUInt32("LoadOutItem", i);
        }

        [Parser(Opcode.SMSG_ENUM_VAS_PURCHASE_STATES_RESPONSE)]
        public static void HandleEnumVasPurchaseStatesResponse(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBits("Result", 2);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_BATTLE_PET_DELIVERED)]
        public static void HandleBattlePayBattlePetDelivered(Packet packet)
        {
            packet.ReadUInt32("DisplayID");
            packet.ReadPackedGuid128("BattlePetGuid");
        }

        [Parser(Opcode.SMSG_DISPLAY_PROMOTION)]
        public static void HandleDisplayPromotion(Packet packet)
        {
            packet.ReadUInt32("PromotionID");
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_START_DISTRIBUTION_ASSIGN_TO_TARGET_RESPONSE)]
        public static void HandleBattlePayStartDistributionAssignToTargetResponse(Packet packet)
        {
            packet.ReadUInt64("DistributionID");
            packet.ReadUInt32("UnknownInt1");
            packet.ReadUInt32("UnknownInt2");
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_REQUEST_PRICE_INFO)]
        public static void HandleBattlePayRequestPriceInfo(Packet packet)
        {
            packet.ReadByte("UnknownByte");
        }
    }
}