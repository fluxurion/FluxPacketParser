using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using System.Collections.Generic;

namespace WowPacketParserModule.V12_0_0_65390.Parsers
{
    public static class FluxBattlePayHandler
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
            var unknown2 = packet.ReadUInt32("Unknown2", index);
            var deliverableProductIDExtra = packet.ReadUInt32("DeliverableProductIDExtra", index);

            var deliverableCount2 = packet.ReadUInt32("DeliverableProductIDCount2", index);
            var unk1027 = packet.ReadUInt32("Unk1027", index);
            var unkUInt64 = packet.ReadUInt64("UnkUInt64", index);

            var deliverableProducts = new List<uint>();
            for (uint i = 0; i < deliverableCount; i++)
                deliverableProducts.Add(packet.ReadUInt32("DeliverableProductID", index, i));

            var deliverableProducts2 = new List<uint>();
            for (uint i = 0; i < deliverableCount2; i++)
                deliverableProducts2.Add(packet.ReadUInt32("DeliverableProductID2", index, i));

            var hasDisplayByte = packet.ReadByte("HasDisplayByte", index);
            var hasVisualMetadata = (hasDisplayByte >> 7) != 0;

            if (hasVisualMetadata)
                ReadVisualMetadata(packet, index);

            BattlePayProductInfo productInfo = new BattlePayProductInfo
            {
                Entry = (uint)index[0],
                ProductInfoID = productid,
                NormalPrice = (long)normalprice,
                CurrentPrice = (long)currentprice,
                Unknown1 = (int)unknown1,
                Unknown2 = (int)unknown2,
                Unknown3 = 0,
                Unknown4 = 0,
                Unknown5 = 0,
                DeliverableProductIDExtra = deliverableProductIDExtra,
                Unk1027 = unk1027,
                ProductInfoFlags = 0,
                UnknownIfFlags1_1 = 0,
                UnknownIfFlags1_2 = 0,
                UnknownIfFlags2_1 = 0,
                UnknownIfFlags2_2 = 0,
                UnknownIfFlags2_3 = 0,
                UnknownIfFlags2_4 = 0,
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
            var unknownByte = packet.ReadUInt32("UnknownByte", index);
            var itemID = packet.ReadUInt32("ItemID", index);
            var quantity = packet.ReadUInt32("Quantity", index);
            var unknownInt1 = packet.ReadUInt32("UnknownInt1", index);
            var unknownInt2 = packet.ReadUInt32("UnknownInt2", index);

            var flagByte = packet.ReadByte("FlagByte", index);
            var isPet         = (flagByte & 0x80) != 0;
            var hasPetResult  = (flagByte & 0x40) != 0;
            var hasPetSubFlag = (flagByte & 0x20) != 0;

            var flagByte2 = packet.ReadByte("FlagByte2", index);
            var hasDisplayInfo = (flagByte2 & 0x80) != 0;

            var flagByte3 = packet.ReadByte("FlagByte3", index);
            var petResultFlags = (flagByte3 >> 4) & 0xF;

            uint petResultVariable = 0;
            if (hasPetSubFlag)
                petResultVariable = packet.ReadUInt32("PetResultVariable", index);

            Storage.BattlePayProductItems.Add(new BattlePayProductItem
            {
                ProductEntry = (uint)index[0],
                ItemOrder = (uint)index[1],
                ID = id,
                UnknownByte = (byte)unknownByte,
                ItemID = itemID,
                Quantity = quantity,
                UnknownInt1 = unknownInt1,
                UnknownInt2 = unknownInt2,
                IsPet = isPet ? 1 : 0,
                HasPetResult = hasPetResult ? 1 : 0,
                PetResultFlags = (uint)petResultFlags,
                HasVisualMetadata = hasDisplayInfo ? 1 : 0
            }, packet.TimeSpan);

            if (hasDisplayInfo)
                ReadVisualMetadata(packet, index);
        }

        private static void ReadProduct(Packet packet, params object[] index)
        {
            var productid                  = packet.ReadUInt32("ProductID", index);
            var type                       = packet.ReadUInt32("Type", index);
            var itemid                     = packet.ReadUInt32("ItemID", index);
            var itemcount                  = packet.ReadUInt32("ItemCount", index);
            var mountspellid               = packet.ReadUInt32("MountSpellID", index);
            var battlepetspeciescreatureid = packet.ReadUInt32("BattlePetSpeciesCreatureID", index);
            var unknown1                   = packet.ReadUInt32("Unknown1", index);
            var unknown2                   = packet.ReadUInt32("Unknown2", index);
            
            // High-offset fields matching DB schema: Unknown3, TransmogSetID, Unknown8, Unknown9, Unknown10
            var unknown3                   = packet.ReadUInt32("Unknown3", index);
            var transmogsetid              = packet.ReadUInt32("TransmogSetID", index);
            var unknown8                   = packet.ReadUInt32("Unknown8", index);
            var unknown9                   = packet.ReadUInt32("Unknown9", index);
            var unknown10                  = packet.ReadUInt32("Unknown10", index);

            var nameLen = packet.ReadByte("NameLength", index);

            // FlagByte 1 (v23 in asm)
            var flagByte1 = packet.ReadByte("FlagByte1", index);
            var alreadyOwned = (flagByte1 & 0x80) != 0;
            
            // Logic for hasPetSubFlag (v3 + 292)
            // if ( (unsigned byte)(2 * v23) >= 0x80u ) -> basically checks bit 0x40
            var hasPetSubFlag = (flagByte1 & 0x40) != 0;

            // FlagByte 2
            var flagByte2 = packet.ReadByte("FlagByte2", index);
            
            // itemCountBits Calculation
            // Derived from IDA: ((flagByte2 >> 7) | (2 * (flagByte1 & 0x7F)))
            // Capped to reasonable max to prevent overflow
            var itemCountBits = (uint)((flagByte2 >> 7) | ((flagByte1 & 0x3F) << 1));

            // Display Flag Setter logic (v8 >= 0x80u where v8 is 2 * flagByte2)
            // This checks the 0x40 bit of flagByte2
            var hasDisplayInfo = (flagByte2 & 0x40) != 0;

            uint petResultVariable = 0;
            if (hasPetSubFlag)
            {
                // petResultVariable = (unsigned byte)(2 * v8) >> 4 where v8 is 2 * flagByte2
                // This is equivalent to (flagByte2 << 2) >> 4
                petResultVariable = (uint)((flagByte2 & 0x3F) >> 2); 
            }

            for (int i = 0; i < (int)itemCountBits; i++)
                ReadProductItem(packet, index, i);

            var name = packet.ReadWoWString("Name", nameLen, index);

            // Final check uses hasDisplayInfo already computed from flagByte2
            // This corresponds to: if ( *(_BYTE *)(v3 + 21832) ) in IDA
            if (hasDisplayInfo)
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
                Unknown10 = unknown10,
                Name = name,
                AlreadyOwned = alreadyOwned ? 1 : 0,
                HasDisplayInfo = hasDisplayInfo ? 1 : 0,
                PetResultVariable = petResultVariable,
                DisplayFlag = hasDisplayInfo ? (uint)1 : 0
            };

            Storage.BattlePayProductDatas.Add(product, packet.TimeSpan);
        }

        private static void ReadGroup(Packet packet, params object[] index)
        {
            var groupid        = packet.ReadUInt32("GroupID", index);
            var iconfiledataid = packet.ReadUInt32("IconFileDataID", index);
            var displaytype    = packet.ReadByte("DisplayType", index);
            var ordering       = packet.ReadUInt32("Ordering", index);
            var unknown        = packet.ReadUInt32("Unknown", index);
            var maingroupid    = packet.ReadUInt32("MainGroupID", index);

            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 8, index);
            var descLen = packet.ReadBits("DescriptionLength", 24, index);

            var name        = packet.ReadWoWString("Name", nameLen, index);
            var description = descLen > 1 ? packet.ReadWoWString("Description", (int)descLen, index) : "";

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

            var displayFlagByte = packet.ReadByte("DisplayFlagByte", index);
            var hasVisualMetadata = (displayFlagByte >> 7) != 0;

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
                DisplayFlag = (uint)(displayFlagByte & 0x7F)
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

            packet.ReadByte("StatusByte");

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

            ReadProductInfo(packet);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_START_CHECKOUT)]
        public static void HandleStartCheckout(Packet packet)
        {
            packet.ReadUInt32("UnkInt1");
            packet.ReadUInt32("UnkInt2");
            packet.ReadUInt64("UnkLong");

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
            packet.ReadInt16("SpecID");
            packet.ReadInt16("Faction");
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

        [Parser(Opcode.SMSG_SYNC_WOW_ENTITLEMENTS)]
        public static void HandleSyncWowEntitlements(Packet packet)
        {
            var purchaseCountSize = packet.ReadUInt32("PurchaseCountSize");
            var productCount = packet.ReadUInt32("ProductCount");

            for (uint i = 0; i < purchaseCountSize; i++)
            {
                packet.ReadUInt32("ProductID", i);
                packet.ReadUInt32("Flags", i);
                packet.ReadUInt32("Flags2", i);
                packet.ReadUInt32("Unknown", i);
                packet.ResetBitReader();
                packet.ReadBits("UnknownBits", 7, i);
                packet.ReadBit("UnknownBit", i);
            }

            for (uint i = 0; i < productCount; i++)
            {
                packet.ReadUInt32("ProductID", i);
                packet.ReadUInt32("Type", i);
                packet.ReadUInt32("ItemID", i);
                packet.ReadUInt32("ItemCount", i);
                packet.ReadUInt32("MountSpellID", i);
                packet.ReadUInt32("BattlePetSpeciesCreatureID", i);
                packet.ReadUInt32("Unknown1", i);
                packet.ReadUInt32("Unknown2", i);
                packet.ReadUInt32("Unknown3", i);
                packet.ReadUInt32("Unknown4", i);
                packet.ReadUInt32("Unknown5", i);
                packet.ReadUInt32("Unknown6", i);

                packet.ResetBitReader();
                var nameLen = packet.ReadBits("NameLength", 8, i);
                var hasPetResultVariable = packet.ReadBit("HasPetResultVariable", i);
                var alreadyOwned = packet.ReadBit("AlreadyOwned", i);
                var hasDisplay = packet.ReadBit("HasDisplay", i);
                packet.ReadBit("UnknownBit", i);

                uint petResultVariable = 0;
                if (hasPetResultVariable)
                    petResultVariable = packet.ReadBits("PetResultVariable", 4, i);

                packet.ResetBitReader();
                var name = packet.ReadWoWString("Name", (int)nameLen, i);

                if (hasDisplay)
                    ReadVisualMetadata(packet, i);
            }
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_VALIDATE_PURCHASE_RESPONSE)]
        public static void HandleValidatePurchaseResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadUInt32("CurrencyID");
            packet.ReadUInt64("PurchaseID");
            packet.ReadUInt64("ClientToken");
            packet.ResetBitReader();
            packet.ReadBit("HasVasPurchase");
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_MOUNT_DELIVERED)]
        public static void HandleBattlePayMountDelivered(Packet packet)
        {
            Substructures.ItemHandler.ReadItemInstance(packet);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_COLLECTION_ITEM_DELIVERED)]
        public static void HandleBattlePayCollectionItemDelivered(Packet packet)
        {
            Substructures.ItemHandler.ReadItemInstance(packet);
        }
    }
}
