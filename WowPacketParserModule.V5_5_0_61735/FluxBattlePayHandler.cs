using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using System.Collections.Generic;

namespace WowPacketParserModule.V5_5_0_61735.Parsers
{
    public static class FluxBattlePayHandler
    {
        // 1.15.9 (build 69722) BattlePay — ported from V12_0_0 FluxBattlePayHandler
        // (12.1 layouts). Verified against era client decompilation:
        //   - purchase record (JamBattlePayPurchase) = ReadPurchase below, 1:1
        //   - distribution object (JamBattlePayDistributionObject) = 12.1
        //     ReadDistributionObject121 layout, 1:1
        //   - product record reader (sub_1406DC0D0) = JamBattlePayProduct /
        //     DisplayCard shape (as 12.0 ReadProduct/ReadDisplayCard): items
        //     array BEFORE the name, {6x u32, u8 flags, optional visual}
        //     elements — NOT the 12.1 Deliverable bit-packed order. The same
        //     reader is used for product-list Products, the optional
        //     distribution Deliverable, and SMSG_BATTLE_PAY_DISPLAY_CARD.
        //   - ReadVisualMetadata internals are ported from 12.1 and only
        //     exercised when display info is present (unverified on era).

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

        // Era product/DisplayCard record — sub_1406DC0D0, verified against
        // decompile: 13x u32, u8 NameLength, u8 Flags1, u8 Flags2, then the
        // items array, then the name string, then optional visual metadata.
        // Count is interleaved: (Flags2 >> 7) | ((Flags1 & 0x3F) << 1).
        // Elements are {6x u32, u8 flags, optional visual block} — the flag
        // bytes are byte-aligned ReadInt8 reads, not bit-packed.
        private static void ReadDeliverable(Packet packet, params object[] index)
        {
            var deliverableID = packet.ReadUInt32("DeliverableID", index);
            packet.ReadUInt32("Type", index);
            packet.ReadUInt32("ItemID", index);
            packet.ReadUInt32("Quantity", index);
            packet.ReadUInt32("MountSpellID", index);
            packet.ReadUInt32("BattlePetCreatureID", index);
            packet.ReadUInt32("BoostID", index);
            packet.ReadUInt32("Flags", index);
            packet.ReadUInt32("TransItemModifiedAppearanceID", index);
            packet.ReadUInt32("TransmogSetID", index);
            packet.ReadUInt32("CharTitleID", index);
            packet.ReadUInt32("SpellItemEnchantmentID", index);
            packet.ReadUInt32("WarbandSceneID", index);

            var nameLen = packet.ReadByte("NameLength", index);

            var flags1 = packet.ReadByte("Flags1", index);
            packet.AddValue("AlreadyOwns", (flags1 & 0x80) != 0, index);
            var hasPetResult = (flags1 & 0x40) != 0;
            packet.AddValue("HasPetResult", hasPetResult, index);

            var flags2 = packet.ReadByte("Flags2", index);
            var itemCount = (uint)((flags2 >> 7) | ((flags1 & 0x3F) << 1));
            packet.AddValue("ChoicesCount", itemCount, index);
            var hasDisplayInfo = (flags2 & 0x40) != 0;
            packet.AddValue("HasDisplayInfo", hasDisplayInfo, index);
            if (hasPetResult)
                packet.AddValue("PetResult", (flags2 >> 2) & 0xF, index);

            for (uint i = 0; i < itemCount; i++)
            {
                packet.ReadUInt32("ID", index, i);
                packet.ReadUInt32("UnknownByte", index, i);
                packet.ReadUInt32("ItemID", index, i);
                packet.ReadUInt32("Quantity", index, i);
                packet.ReadUInt32("UnknownInt1", index, i);
                packet.ReadUInt32("UnknownInt2", index, i);

                var itemFlags = packet.ReadByte("ItemFlags", index, i);
                packet.AddValue("ItemIsPet", (itemFlags & 0x80) != 0, index, i);
                var itemHasPetResult = (itemFlags & 0x40) != 0;
                packet.AddValue("ItemHasPetResult", itemHasPetResult, index, i);
                if (itemHasPetResult)
                    packet.AddValue("ItemPetResult", (itemFlags >> 1) & 0xF, index, i);

                if ((itemFlags & 0x20) != 0)
                    _ = ReadVisualMetadata(packet, 4, 0, index, i);
            }

            packet.ReadWoWString("Name", nameLen, index);

            if (hasDisplayInfo)
                _ = ReadVisualMetadata(packet, 6, deliverableID, index);
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
            var groupid = packet.ReadUInt32("GroupID", index);
            var productid = packet.ReadUInt32("ProductID", index);
            var ordering = packet.ReadUInt32("Ordering", index);
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

        // Era JamBattlePayPurchase — verified field-for-field against decompile:
        // u64 PurchaseID, i32 Status, i32 ResultCode, u32 ProductID,
        // u64 BasePrice, u64 UserPrice, i64 TimeCreated, u8 WalletNameLen, string
        private static void ReadPurchase(Packet packet, params object[] index)
        {
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadInt32("Status", index);
            packet.ReadInt32("ResultCode", index);
            packet.ReadUInt32("ProductID", index);
            packet.ReadUInt64("BasePrice", index);
            packet.ReadUInt64("UserPrice", index);
            packet.ReadInt64("TimeCreated", index);

            var walletNameLen = packet.ReadByte("WalletNameLength", index);
            packet.ReadWoWString("WalletName", walletNameLen, index);
        }

        // Era JamBattlePayDistributionObject — verified against decompile:
        // u64 ID, u32 Status, u32 DeliverableID, 2x packed-guid(16B), u32 realm,
        // u32 realm, u64 PurchaseID, u32 ManualReview, bit7=HasDeliverable,
        // bit6=Revoked, optional Deliverable (same reader as product records)
        private static void ReadDistributionObject(Packet packet, params object[] index)
        {
            packet.ReadUInt64("DistributionID", index);
            packet.ReadUInt32("Status", index);
            packet.ReadUInt32("DeliverableID", index);
            packet.ReadPackedGuid128("LicenseGameAccountGUID", index);
            packet.ReadPackedGuid128("TargetPlayer", index);
            packet.ReadUInt32("TargetNativeRealm", index);
            packet.ReadUInt32("TargetVirtualRealm", index);
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadUInt32("ManualReview", index);

            packet.ResetBitReader();
            var hasDeliverable = packet.ReadBit("HasDeliverable", index);
            packet.ReadBit("Revoked", index);
            packet.ResetBitReader();

            if (hasDeliverable)
                ReadDeliverable(packet, index);
        }

        // 0x460224 — {u32 Result, u32 CurrencyID, 4x u32 counts, 4 arrays}
        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE, ClientVersionBuild.V1_15_9_69722)]
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
                ReadDeliverable(packet, i);

            for (uint i = 0; i < groupCount; i++)
                ReadGroup(packet, i);

            for (uint i = 0; i < shopCount; i++)
                ReadShop(packet, i);
        }

        // 0x46022E — bare JamBattlePayProduct/DisplayCard record (sub_1406DC0D0);
        // no PackedGuid128 prefix unlike 12.x SMSG_BATTLE_PAY_DISPLAY_CARD.
        // Verified: 55-byte packet = 13x u32 + 3 flag/len bytes, fully consumed.
        [Parser(Opcode.SMSG_BATTLE_PAY_DISPLAY_CARD, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleBattlePayDisplayCard(Packet packet)
        {
            ReadDeliverable(packet);
        }

        // 0x460225 — {u32 Result, u32 PurchaseCount, purchases[]}
        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PURCHASE_LIST_RESPONSE, ClientVersionBuild.V1_15_9_69722)]
        public static void HandlePurchaseListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");

            var purchaseCount = packet.ReadUInt32("PurchaseCount");
            for (uint i = 0; i < purchaseCount; i++)
                ReadPurchase(packet, i);
        }

        // 0x460226 — {u32 Result, 11-bit count, objects[]}; count is bit-packed
        // as (byte0 << 3) | (byte1 >> 5) which is ReadBits(11) MSB-first
        [Parser(Opcode.SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleDistributionListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ResetBitReader();
            var count = packet.ReadBits("DistributionCount", 11);
            packet.ResetBitReader();

            for (uint i = 0; i < count; i++)
                ReadDistributionObject(packet, i);
        }
    }
}
