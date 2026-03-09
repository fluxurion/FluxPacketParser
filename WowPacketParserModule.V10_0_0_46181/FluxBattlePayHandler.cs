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
            var hasIconFileDataID = packet.ReadBit("HasIconFileDataID", index);
            var hasPreview = packet.ReadBit("HasPreview", index);
            var titleLen = packet.ReadBits("TitleLength", 10, index);
            var title2Len = packet.ReadBits("Title2Length", 10, index);
            var descLen = packet.ReadBits("DescriptionLength", 13, index);
            var desc2Len = packet.ReadBits("Description2Length", 13, index);
            var desc3Len = packet.ReadBits("Description3Length", 13, index);
            var hasIconBorder = packet.ReadBit("HasIconBorder", index);
            var hasUnknown1 = packet.ReadBit("HasUnknown1", index);
            var hasUiTextureAtlas = packet.ReadBit("HasUiTextureAtlasMemberID", index);
            var hasUiTextureAtlas2 = packet.ReadBit("HasUiTextureAtlasMemberID2", index);
            var desc4Len = packet.ReadBits("Description4Length", 13, index);
            var desc5Len = packet.ReadBits("Description5Length", 12, index);

            var visualCount = packet.ReadUInt32("VisualCount", index);
            var cardType = packet.ReadUInt32("CardType", index);
            var unknown3 = packet.ReadUInt32("Unknown3", index);
            var productMultiplier = packet.ReadUInt32("ProductMultiplier", index);

            var iconFileDataID = 0;
            if (hasIconFileDataID)
                iconFileDataID = (int)packet.ReadUInt32("IconFileDataID", index);

            var uiModelSceneID = 0;
            if (hasPreview)
                uiModelSceneID = (int)packet.ReadUInt32("UIModelSceneID", index);

            var title = packet.ReadWoWString("Title", titleLen, index);
            var title2 = packet.ReadWoWString("Title2", title2Len, index);
            var description = packet.ReadWoWString("Description", descLen, index);
            var description2 = packet.ReadWoWString("Description2", desc2Len, index);
            var description3 = packet.ReadWoWString("Description3", desc3Len, index);

            var iconBorder = 0;
            if (hasIconBorder)
                iconBorder = (int)packet.ReadUInt32("IconBorder", index);
            var unknown1 = 0;
            if (hasUnknown1)
                unknown1 = (int)packet.ReadUInt32("Unknown1", index);
            var uiTextureAtlasMemberID = 0;
            if (hasUiTextureAtlas)
                uiTextureAtlasMemberID = (int)packet.ReadUInt32("UiTextureAtlasMemberID", index);
            var uiTextureAtlasMemberID2 = 0;
            if (hasUiTextureAtlas2)
                uiTextureAtlasMemberID2 = (int)packet.ReadUInt32("UiTextureAtlasMemberID2", index);

            var description4 = packet.ReadWoWString("Description4", desc4Len, index);
            var description5 = packet.ReadWoWString("Description5", desc5Len, index);

            var creatureDisplayIDs = new List<uint>();
            var previewUIModelSceneIDs = new List<uint>();
            var transmogSetIDs = new List<uint>();
            var visualNames = new List<string>();

            for (uint i = 0; i < visualCount; i++)
            {
                packet.ResetBitReader();
                var nameLen = packet.ReadBits("VisualNameLength", 10, index, i);
                var creatureDisplayID = packet.ReadUInt32("CreatureDisplayID", index, i);
                var previewUIModelSceneID = packet.ReadUInt32("PreviewUIModelSceneID", index, i);
                var transmogSetID = packet.ReadUInt32("TransmogSetID", index, i);
                var visualName = packet.ReadWoWString("VisualName", nameLen, index, i);

                creatureDisplayIDs.Add(creatureDisplayID);
                previewUIModelSceneIDs.Add(previewUIModelSceneID);
                transmogSetIDs.Add(transmogSetID);
                visualNames.Add(visualName);

                Storage.BattlePayDisplayInfoVisuals.Add(new BattlePayDisplayInfoVisual
                {
                    DisplayInfoEntry = (uint)index[0],
                    VisualIndex = i,
                    CreatureDisplayID = creatureDisplayID,
                    PreviewUIModelSceneID = previewUIModelSceneID,
                    TransmogSetID = transmogSetID,
                    VisualName = visualName
                }, packet.TimeSpan);
            }

            BattlePayDisplayInfo displayInfo = new BattlePayDisplayInfo
            {
                Entry = (uint)index[0],
                ProductInfoID = 0,
                ProductDataID = 0,
                ShopDataID = 0,
                HasIconFileDataID = hasIconFileDataID ? 1 : 0,
                HasPreview = hasPreview ? 1 : 0,
                HasIconBorder = hasIconBorder ? 1 : 0,
                HasUnknown1 = hasUnknown1 ? 1 : 0,
                HasUiTextureAtlasMemberID = hasUiTextureAtlas ? 1 : 0,
                HasUiTextureAtlasMemberID2 = hasUiTextureAtlas2 ? 1 : 0,
                VisualCount = visualCount,
                CardType = (int)cardType,
                Unknown3 = (int)unknown3,
                ProductMultiplier = (int)productMultiplier,
                IconFileDataID = (uint)iconFileDataID,
                UIModelSceneID = (uint)uiModelSceneID,
                Title = title,
                Title2 = title2,
                Description = description,
                Description2 = description2,
                Description3 = description3,
                IconBorder = iconBorder,
                Unknown1 = unknown1,
                UiTextureAtlasMemberID = uiTextureAtlasMemberID,
                UiTextureAtlasMemberID2 = uiTextureAtlasMemberID2,
                Description4 = description4,
                Description5 = description5,
                PreviewCreatureDisplayIDs = string.Join(",", creatureDisplayIDs),
                PreviewUIModelSceneIDs = string.Join(",", previewUIModelSceneIDs),
                PreviewTransmogSets = string.Join(",", transmogSetIDs),
                PreviewTitles = string.Join(",", visualNames)
            };
            Storage.BattlePayDisplayInfos.Add(displayInfo, packet.TimeSpan);
        }

        private static void ReadProductInfo(Packet packet, params object[] index)
        {
            var productid = packet.ReadUInt32("ProductID", index);
            var normalprice = packet.ReadInt64("NormalPrice", index);
            var currentprice = packet.ReadInt64("CurrentPrice", index);

            var deliverableCount = packet.ReadUInt32("DeliverableProductIDCount", index);
            var unknown1 = packet.ReadUInt32("Unknown1", index);

            uint unknown2 = packet.ReadUInt32("Unknown2", index);

            var deliverableProductIDExtra = packet.ReadUInt32("deliverableProductIDExtra", index);
            var productinfoflags = packet.ReadUInt32("ProductInfoFlags", index);

            var unk1027 = packet.ReadUInt32("Unk1027", index);

            var deliverableProducts = new List<uint>();
            for (uint i = 0; i < deliverableCount; i++)
                deliverableProducts.Add(packet.ReadUInt32("DeliverableProductID", index, i));

            packet.ResetBitReader();
            var hasVisualMetadata = packet.ReadBit("HasVisualMetadata", index);

            int unknownIfFlags1_1 = 0;
            int unknownIfFlags1_2 = 0;
            int unknownIfFlags2_1 = 0;
            int unknownIfFlags2_2 = 0;
            int unknownIfFlags2_3 = 0;
            int unknownIfFlags2_4 = 0;

            if (productinfoflags == 1)
            {
                unknownIfFlags1_1 = packet.ReadUInt16("UnknownIfFlags1_1", index);
                unknownIfFlags1_2 = packet.ReadUInt16("UnknownIfFlags1_2", index);
            }
            else if (productinfoflags == 2)
            {
                unknownIfFlags2_1 = packet.ReadUInt16("UnknownIfFlags2_1", index);
                unknownIfFlags2_2 = packet.ReadUInt16("UnknownIfFlags2_2", index);
                unknownIfFlags2_3 = packet.ReadUInt16("UnknownIfFlags2_3", index);
                unknownIfFlags2_4 = packet.ReadUInt16("UnknownIfFlags2_4", index);
            }
            else if (hasVisualMetadata)
                ReadVisualMetadata(packet, index);

            BattlePayProductInfo productInfo = new BattlePayProductInfo
            {
                Entry = (uint)index[0],
                ProductInfoID = productid,
                NormalPrice = (long)normalprice,
                CurrentPrice = (long)currentprice,
                ProductInfoFlags = (int)productinfoflags,
                Unknown1 = (int)unknown1,
                Unknown2 = (int)unknown2,
                Unknown3 = 0,
                Unknown4 = 0,
                Unknown5 = 0,
                DeliverableProductIDExtra = deliverableProductIDExtra,
                Unk1027 = unk1027,
                UnknownIfFlags1_1 = unknownIfFlags1_1,
                UnknownIfFlags1_2 = unknownIfFlags1_2,
                UnknownIfFlags2_1 = unknownIfFlags2_1,
                UnknownIfFlags2_2 = unknownIfFlags2_2,
                UnknownIfFlags2_3 = unknownIfFlags2_3,
                UnknownIfFlags2_4 = unknownIfFlags2_4,
                HasVisualMetadata = hasVisualMetadata ? 1 : 0,
                DeliverableProductIDs = string.Join(",", deliverableProducts),
                ChoiceType = 0,
                DisplayFlag = 0,
                HasUnknown1InDisplayInfo = 0,
                HasBattlePayDisplayInfo = hasVisualMetadata ? 1 : 0
            };
            Storage.BattlePayProductInfos.Add(productInfo, packet.TimeSpan);
        }

        private static void ReadProductItem(Packet packet, params object[] index)
        {
            var id = packet.ReadUInt32("ID", index);
            var unknownByte = packet.ReadByte("UnknownByte", index);
            var itemID = packet.ReadUInt32("ItemID", index);
            var quantity = packet.ReadUInt32("Quantity", index);
            var unknownInt1 = packet.ReadUInt32("UnknownInt1", index);
            var unknownInt2 = packet.ReadUInt32("UnknownInt2", index);

            packet.ResetBitReader();
            var isPet = packet.ReadBit("IsPet", index);
            var hasPetResult = packet.ReadBit("HasPetResult", index);
            var petResultFlags = packet.ReadBits("PetResultFlags", 4, index);
            var hasVisualMetadata = packet.ReadBit("HasVisualMetadata", index);

            Storage.BattlePayProductItems.Add(new BattlePayProductItem
            {
                ProductEntry = (uint)index[0],
                ItemOrder = (uint)index[1],
                ID = id,
                UnknownByte = unknownByte,
                ItemID = itemID,
                Quantity = quantity,
                UnknownInt1 = unknownInt1,
                UnknownInt2 = unknownInt2,
                IsPet = isPet ? 1 : 0,
                HasPetResult = hasPetResult ? 1 : 0,
                PetResultFlags = (uint)petResultFlags,
                HasVisualMetadata = hasVisualMetadata ? 1 : 0
            }, packet.TimeSpan);

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
            var nameLen = packet.ReadBits("NameLength", 8, index);
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

            if (hasdisplayinfo)
                ReadVisualMetadata(packet, index);

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
            var description = descLen > 1 ? packet.ReadWoWString("Description", descLen, index) : "";

            BattlePayGroup group = new BattlePayGroup
            {
                Entry = (uint)index[0],
                GroupID = groupid,
                IconFileDataID = iconfiledataid,
                DisplayType = displaytype,
                Ordering = ordering,
                Unknown = unknown,
                MainGroupID = maingroupid,
                Name = name,
                Description = description
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

            if (hasVisualMetadata)
                ReadVisualMetadata(packet, index);

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
        }

        private static void ReadPurchase(Packet packet, params object[] index)
        {
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadUInt32("Status", index);
            packet.ReadUInt32("ResultCode", index);
            packet.ReadUInt32("ProductID", index);
            packet.ReadUInt64("UnkLong", index);
            packet.ReadUInt64("UnkLong2", index);
            packet.ReadTime("PurchaseTime", index);
            packet.ReadUInt32("UnkInt1027", index);

            packet.ResetBitReader();
            uint nameLen = packet.ReadBits("WalletNameLength", 8, index);
            packet.ResetBitReader();
            packet.ReadWoWString("WalletName", nameLen, index);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PURCHASE_LIST_RESPONSE)]
        public static void HandlePurchaseListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");

            var purchaseCount = packet.ReadUInt32("PurchaseCount");
            for (uint i = 0; i < purchaseCount; i++)
                ReadPurchase(packet, i);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_CONFIRM_PURCHASE)]
        public static void HandleConfirmPurchase(Packet packet)
        {
            packet.ReadUInt32("PurchaseID");
            packet.ReadUInt32("UnknownField");

            // The client does: (UnkByte >> 5). To send value '3', we send (3 << 5).
            // Masking with 0x7 ensures we only use 3 bits.
            packet.ReadByte("StatusByte");

            // Serialized via BattlePay::ParseCurrencyInfo
            packet.ReadUInt32("CurrencyID");
            packet.ReadUInt64("CurrencyCount");
            packet.ReadUInt32("CurrencyUnkInt2");
            packet.ReadUInt32("CurrencyUnkInt3");
            packet.ReadUInt32("CurrencyUnkInt4");
            packet.ReadUInt32("CurrencyUnkInt6");
            packet.ReadUInt32("CurrencyUnkInt7");
            packet.ReadUInt32("CurrencyUnkInt8");

            packet.ResetBitReader();
            packet.ReadBit("CurrencyUnkBit1");
            packet.ReadBit("CurrencyUnkBit2");

            // Serialized via BattlePay::ParseProductInfo
            ReadProductInfo(packet);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_START_CHECKOUT)]
        public static void HandleStartCheckout(Packet packet)
        {
            packet.ReadUInt32("UnkInt1");
            packet.ReadUInt32("UnkInt2");
            packet.ReadUInt64("UnkLong");

            //var key = packet.ReadWoWString("Key");
            //var key2 = packet.ReadWoWString("Key2");

            packet.ReadBit("UnkBit");
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_CONFIRM_PURCHASE_RESPONSE)]
        public static void HandleConfirmPurchaseResponse(Packet packet)
        {
            packet.ResetBitReader();
            bool confirmed = packet.ReadBit("Confirmed");
            packet.ResetBitReader();
            packet.ReadUInt32("ServerToken");
            packet.ReadUInt64("ClientCurrentPriceFixedPoint");
        }

        [Parser(Opcode.SMSG_GET_ACCOUNT_CHARACTER_LIST_RESULT)]
        public static void HandleGetAccountCharacterListResult(Packet packet)
        {
            packet.ReadUInt32("Token");
            packet.ResetBitReader();
            var count = packet.ReadBits("AccountCharacterListEntryCount", 2);
            packet.ResetBitReader();

            packet.ReadBit("UnkBit");

            for (uint i = 0; i < count; i++)
            {
                packet.ReadUInt32("AccountID", i);
                packet.ReadUInt32("VirtualRealmAddress", i);
                packet.ReadWoWString("RealmName", i);
                packet.ReadPackedGuid128("CharacterGuid", i);
                packet.ReadWoWString("Name", i);
                packet.ReadByte("Race", i);
                packet.ReadByte("Class", i);
                packet.ReadByte("Sex", i);
                packet.ReadByte("Level", i);
                packet.ReadUInt32("LastPlayedTime", i);
            }
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_START_VAS_PURCHASE)]
        public static void HandleStartVasPurchase(Packet packet)
        {
            packet.ReadUInt32("UnkInt");

            packet.ResetBitReader();
            var vasPurchaseCount = packet.ReadBits("VasPurchaseCount", 2);
            packet.ResetBitReader();

            for (uint i = 0; i < vasPurchaseCount; i++)
            {
                packet.ReadPackedGuid128("PlayerGuid", i);
                packet.ReadUInt32("UnkInt", i);
                packet.ReadUInt32("UnkInt2", i);
                packet.ReadUInt64("UnkLong", i);

                packet.ResetBitReader();
                var itemCount = packet.ReadBits("ItemCount", 2, i);
                packet.ResetBitReader();

                for (uint j = 0; j < itemCount; j++)
                    packet.ReadUInt32("ItemID", i, j);
            }
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_REQUEST_PRICE_INFO)]
        public static void HandleBattlePayRequestPriceInfo(Packet packet)
        {
            packet.ReadUInt32("UnkInt");
            packet.ReadUInt32("ProductInfoID");
        }

        private static void ReadDistributionObject(Packet packet, params object[] index)
        {
            packet.ReadUInt64("DistributionID", index);
            packet.ReadUInt32("Status", index);
            packet.ReadUInt32("ProductID", index);
            packet.ReadPackedGuid128("TargetPlayerGUID", index);
            packet.ReadPackedGuid128("UnkGuid", index);
            packet.ReadUInt32("TargetVirtualRealmAddress", index);
            packet.ReadUInt32("TargetNativeRealmAddress", index);
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadUInt32("UnkInt1027", index);

            packet.ResetBitReader();
            bool hasProduct = packet.ReadBit("HasProductInfo", index);
            packet.ReadBit("IsRevoked", index);

            if (hasProduct)
                ReadProductInfo(packet, index);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE)]
        public static void HandleDistributionListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ResetBitReader();
            var count = packet.ReadBits("DistributionObjectCount", 11);

            for (int i = 0; i < count; ++i)
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
            packet.ReadUInt32("CurrencyID");
            packet.ReadUInt32("ProductID");
            packet.ReadPackedGuid128("TargetCharacter");
            packet.ResetBitReader();
            uint string1Len = packet.ReadBits("String1Length", 6);
            uint string2Len = packet.ReadBits("String2Length", 12);
            uint string3Len = packet.ReadBits("String3Length", 7);
            packet.ResetBitReader();
            packet.ReadWoWString("TargetName", string1Len);
            packet.ReadWoWString("WalletName", string2Len);
            packet.ReadWoWString("PromotionCode", string3Len);
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_OPEN_CHECKOUT)]
        public static void HandleOpenCheckout(Packet packet)
        {
            packet.ReadUInt32("ClientToken");
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_CANCEL_OPEN_CHECKOUT)]
        public static void HandleCancelOpenCheckout(Packet packet)
        {
            packet.ResetBitReader();
            uint walletNameLen = packet.ReadBits("WalletNameLength", 7);
            bool isPurchaseInProgress = packet.ReadBit("IsPurchaseInProgress");
            packet.ResetBitReader();
            packet.ReadWoWString("WalletName", walletNameLen);
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_VAS)]
        public static void HandleDistributionAssignVas(Packet packet)
        {
            // Empty packet structure
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
            packet.ReadUInt32("UnkInt1");
            packet.ReadUInt32("UnkInt2");
        }

        
    }
}