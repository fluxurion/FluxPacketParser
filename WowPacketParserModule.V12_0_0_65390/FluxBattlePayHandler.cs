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

        internal static string ReadVisualMetadata(Packet packet, byte sourceType, uint sourceID, params object[] index)
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
            }

            BattlePayDisplayInfo displayInfo = new BattlePayDisplayInfo
            {
                SourceType = sourceType,
                SourceID = sourceID,
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

            return title;
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

            var name = "";
            if (hasVisualMetadata)
                name = ReadVisualMetadata(packet, 1, productid, index);

            BattlePayProductInfo productInfo = new BattlePayProductInfo
            {
                Entry = (uint)index[0],
                ShopListingID = productid,
                NormalPrice = (long)normalprice,
                CurrentPrice = (long)currentprice,
                ProductInfoFlags = 0,
                Unknown1 = (int)unknown1,
                Unknown2 = (int)unknown2,
                Unknown3 = 0,
                Unknown4 = 0,
                Unknown5 = 0,
                DeliverableIDExtra = deliverableProductIDExtra,
                Unk1027 = unk1027,
                UnkUInt64 = unkUInt64,
                UnknownIfFlags1_1 = 0,
                UnknownIfFlags1_2 = 0,
                UnknownIfFlags2_1 = 0,
                UnknownIfFlags2_2 = 0,
                UnknownIfFlags2_3 = 0,
                UnknownIfFlags2_4 = 0,
                HasVisualMetadata = hasVisualMetadata ? 1 : 0,
                DeliverableIDs = string.Join(",", deliverableProducts),
                DeliverableIDs2 = string.Join(",", deliverableProducts2),
                DisplayFlag = 0,
                HasUnknown1InDisplayInfo = 0,
                HasBattlePayDisplayInfo = hasVisualMetadata ? 1 : 0,
                ChoiceType = 0,
                Name = name
            };
            Storage.BattlePayProductInfos.Add(productInfo, packet.TimeSpan);
        }

        private static string ReadProductItem(Packet packet, params object[] index)
        {
            var id = packet.ReadUInt32("ID", index);
            var unknownByte = packet.ReadUInt32("UnknownByte", index);
            var itemID = packet.ReadUInt32("ItemID", index);
            var quantity = packet.ReadUInt32("Quantity", index);
            var unknownInt1 = packet.ReadUInt32("UnknownInt1", index);
            var unknownInt2 = packet.ReadUInt32("UnknownInt2", index);

            var flagByte = packet.ReadByte("FlagByte", index);
            var isPet = (flagByte & 0x80) != 0;
            var hasPetResult = (flagByte & 0x40) != 0;
            var hasPetSubFlag = (flagByte & 0x20) != 0;

            var flagByte2 = packet.ReadByte("FlagByte2", index);
            var hasDisplayInfo = (flagByte2 & 0x80) != 0;

            var flagByte3 = packet.ReadByte("FlagByte3", index);
            var petResultFlags = (flagByte3 >> 4) & 0xF;

            uint petResultVariable = 0;
            if (hasPetSubFlag)
                petResultVariable = packet.ReadUInt32("PetResultVariable", index);

            if (hasDisplayInfo)
                _ = ReadVisualMetadata(packet, 4, id, index);

            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                id, unknownByte, itemID, quantity, unknownInt1, unknownInt2,
                isPet ? 1 : 0, hasPetResult ? 1 : 0, petResultFlags, hasDisplayInfo ? 1 : 0);
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
            var unknown10 = packet.ReadUInt32("Unknown10", index);

            var nameLen = packet.ReadByte("NameLength", index);

            var flagByte1 = packet.ReadByte("FlagByte1", index);
            var alreadyOwned = (flagByte1 & 0x80) != 0;
            var hasPetSubFlag = (flagByte1 & 0x40) != 0;

            var flagByte2 = packet.ReadByte("FlagByte2", index);
            var itemCountBits = (uint)((flagByte2 >> 7) | ((flagByte1 & 0x3F) << 1));
            var hasDisplayInfo = (flagByte2 & 0x40) != 0;

            uint petResultVariable = 0;
            if (hasPetSubFlag)
                petResultVariable = (uint)((flagByte2 & 0x3F) >> 2);

            var itemDataList = new List<string>();
            for (int i = 0; i < (int)itemCountBits; i++)
                itemDataList.Add(ReadProductItem(packet, index, i));

            var name = packet.ReadWoWString("Name", nameLen, index);

            if (hasDisplayInfo)
                _ = ReadVisualMetadata(packet, 2, productid, index);

            BattlePayProduct product = new BattlePayProduct
            {
                Entry = (uint)index[0],
                DeliverableID = productid,
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
                Unknown11 = 0,
                Name = name,
                AlreadyOwned = alreadyOwned ? 1 : 0,
                HasDisplayInfo = hasDisplayInfo ? 1 : 0,
                PetResultVariable = petResultVariable,
                DisplayFlag = hasDisplayInfo ? (uint)1 : 0,
                Items = itemDataList.Count > 0 ? string.Join(":", itemDataList) : ""
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

            var nameLen = packet.ReadByte("NameLength", index);

            packet.ResetBitReader();
            var descLen = packet.ReadBits("DescriptionLength", 24, index);

            var name = packet.ReadWoWString("Name", (int)nameLen, index);
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
            var shopFlags = packet.ReadUInt32("ShopFlags", index);
            var ordering = packet.ReadUInt32("Ordering", index);
            var productid = packet.ReadUInt32("ProductID", index);
            var groupid = packet.ReadUInt32("GroupID", index);
            var shopListingID = packet.ReadUInt32("ShopListingID", index);
            var field20 = packet.ReadByte("Field20", index);

            var flagByte = packet.ReadByte("Flag", index);
            var hasDisplayCard = (flagByte & 0x80) != 0;
            packet.AddValue("HasDisplayCard", hasDisplayCard, index);

            var name = "";
            if (hasDisplayCard)
                name = ReadVisualMetadata(packet, 3, shopFlags, index);

            BattlePayShop shop = new BattlePayShop
            {
                Entry = (uint)index[0],
                ShopEntryID = shopFlags,
                GroupID = groupid,
                ShopListingID = productid,
                Ordering = ordering,
                VasServiceType = shopListingID,
                StoreDeliveryType = field20,
                HasBattlePayDisplayInfo = hasDisplayCard ? 1 : 0,
                Unknown = 0,
                DisplayFlag = (uint)(flagByte & 0x7F),
                Name = name
            };
            Storage.BattlePayShopDatas.Add(shop, packet.TimeSpan);
        }

        private static void ReadPurchase(Packet packet, params object[] index)
        {
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadUInt32("Unk1", index);
            packet.ReadUInt32("Unk2", index);
            packet.ReadUInt32("Unk3", index);
            packet.ReadUInt64("Unk4", index);
            packet.ReadUInt64("Unk5", index);
            packet.ReadUInt64("Unk6", index);

            var nameLen = packet.ReadByte("NameLen", index);
            packet.ReadWoWString("Name", nameLen, index);
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
            packet.ReadUInt64("GUID1_Hi", index);
            packet.ReadUInt64("GUID1_Lo", index);
            packet.ReadUInt64("GUID2_Hi", index);
            packet.ReadUInt64("GUID2_Lo", index);
            packet.ReadUInt32("uint32_1", index);
            packet.ReadUInt32("uint32_2", index);
            packet.ReadUInt64("TargetID", index);
            packet.ReadUInt32("uint32_3", index);

            var hasDisplay = packet.ReadByte("HasDisplay", index);
            var hasVisualMetadata = (hasDisplay & 0x80) != 0;

            if (hasVisualMetadata)
            {
                // DisplayCard sub + DisplayCard deserializer (sub_7FF659252720)
                _ = ReadVisualMetadata(packet, 5, (uint)(index.Length > 0 ? (int)index[0] : 0), index);
            }

            var hasFlag = packet.ReadByte("HasFlag", index);
            var hasFlagBit6 = (hasFlag & 0x40) != 0;
            packet.AddValue("HasFlagBit6", hasFlagBit6, index);

            packet.ReadUInt32("uint32_4", index);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE)]
        public static void HandleDistributionListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");

            var byte0 = packet.ReadByte("Byte0");
            var byte1 = packet.ReadByte("Byte1");
            var count = (byte1 >> 5) | (8 * byte0);

            packet.AddValue("DistributionObjectCount", count);

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
            packet.ReadUInt32("ProductID");
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
            var countByte = packet.ReadByte("CountByte");
            var vasCount = countByte >> 2;
            packet.AddValue("VASCount", vasCount);

            for (int i = 0; i < vasCount; i++)
            {
                packet.ReadUInt64("GUID_Hi", i);
                packet.ReadUInt64("GUID_Lo", i);
                packet.ReadUInt32("uint32_0", i);
                packet.ReadUInt32("uint32_1", i);
                packet.ReadUInt64("uint64_0", i);

                var arrayCountByte = packet.ReadByte("ArrayCountByte", i);
                var arrayCount = arrayCountByte >> 6;
                packet.AddValue("ArrayCount", arrayCount, i);

                for (int j = 0; j < arrayCount; j++)
                    packet.ReadUInt32("uint32", i, j);
            }
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

        [Parser(Opcode.SMSG_BATTLE_PAY_VALIDATE_PURCHASE_RESPONSE)]
        public static void HandleValidatePurchaseResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadUInt32("uint32_0");
            packet.ReadUInt64("Balance");
            packet.ReadUInt64("uint64_1");

            var flagByte = packet.ReadByte("Byte");
            packet.AddValue("HasFlag_bit7", (flagByte & 0x80) != 0);
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

        [Parser(Opcode.SMSG_ACCOUNT_STORE_CURRENCY_UPDATE)]
        public static void HandleAccountStoreCurrencyUpdate(Packet packet)
        {
            var currencyCount = packet.ReadUInt32("CurrencyCount");

            for (uint i = 0; i < currencyCount; i++)
            {
                packet.ReadUInt32("CurrencyID", i);
                packet.ReadUInt32("Value1", i);
                packet.ReadUInt32("Value2", i);
            }
        }

        [Parser(Opcode.SMSG_ACCOUNT_STORE_FRONT_UPDATE)]
        public static void HandleAccountStoreFrontUpdate(Packet packet)
        {
            var flags = packet.ReadByte("Flags");
            packet.AddValue("Flags_bit7", (flags & 0x80) != 0);
            packet.AddValue("Flags_bit6", (flags & 0x40) != 0);

            packet.ReadUInt32("uint32_0");

            var array1Count = packet.ReadUInt32("Array1Count");
            for (uint i = 0; i < array1Count; i++)
            {
                packet.ReadUInt32("uint32_0", i);
                packet.ReadUInt32("uint32_1", i);
                packet.ReadUInt32("uint32_2", i);
            }

            var array2Count = packet.ReadUInt32("Array2Count");
            for (uint i = 0; i < array2Count; i++)
            {
                // Complex 32-byte struct via sub_7FF659251B30 — reading raw
                packet.ReadUInt32("Field0", i);
                packet.ReadUInt32("Field1", i);
                packet.ReadUInt32("Field2", i);
                packet.ReadUInt32("Field3", i);
                packet.ReadUInt32("Field4", i);
                packet.ReadUInt32("Field5", i);
                packet.ReadUInt32("Field6", i);
                packet.ReadUInt32("Field7", i);
            }

            var byte1 = packet.ReadByte("Byte1");
            packet.AddValue("Byte1_bit7", (byte1 & 0x80) != 0);
            packet.AddValue("Byte1_bit6", (byte1 & 0x40) != 0);
        }
    }
}
