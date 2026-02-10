using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using System.Collections.Generic;

namespace WowPacketParserModule.V11_0_0_55666.Parsers
{
    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        [Parser(Opcode.CMSG_UPDATE_VAS_PURCHASE_STATES)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        private static void ReadDisplayInfo(Packet packet, params object[] index)
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
            var productMultiplier = packet.ReadUInt32("Unknown4", index);

            var iconFileDataID = 0;
            if (hasIconFileDataID)
                iconFileDataID = (int)packet.ReadUInt32("IconFileDataID", index);

            var uiModelSceneID = packet.ReadUInt32("UIModelSceneID", index);

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
                creatureDisplayIDs.Add(packet.ReadUInt32("CreatureDisplayID", index, i));
                previewUIModelSceneIDs.Add(packet.ReadUInt32("PreviewUIModelSceneID", index, i));
                transmogSetIDs.Add(packet.ReadUInt32("TransmogSetID", index, i));
                visualNames.Add(packet.ReadWoWString("VisualName", nameLen, index, i));
            }

            BattlePayDisplayInfo displayInfo = new BattlePayDisplayInfo
            {
                Entry = (uint)index[0],
                ProductInfoID = 0,
                ProductDataID = 0,
                ShopDataID = 0,
                CardType = (int)cardType,
                Unknown3 = (int)unknown3,
                ProductMultiplier = (int)productMultiplier,
                IconFileDataID = iconFileDataID,
                UIModelSceneID = (int)uiModelSceneID,
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
            var normalprice = packet.ReadUInt64("NormalPrice", index);
            var currentprice = packet.ReadUInt64("CurrentPrice", index);
            var deliverableCount = packet.ReadUInt32("DeliverableProductIDCount", index);
            var productinfoflags = packet.ReadUInt32("ProductInfoFlags", index);
            var unknown2 = packet.ReadUInt32("Unknown2", index);
            var unknown3 = packet.ReadUInt32("Unknown3", index);
            var unknown4 = packet.ReadUInt32("Unknown4", index);
            var unknown5 = packet.ReadUInt32("Unknown5", index);

            var deliverableProducts = new List<uint>();
            for (uint i = 0; i < deliverableCount; i++)
                deliverableProducts.Add(packet.ReadUInt32("DeliverableProductID", index, i));
            var deliverableproductids = string.Join(",", deliverableProducts);

            packet.ResetBitReader();
            var choiceType = packet.ReadBits("ChoiceType", 7, index);
            var hasbattlepaydisplayinfo = (choiceType != 0 && choiceType != 32 && choiceType != 103) ? 1 : 0;

            BattlePayProductInfo productInfo = new BattlePayProductInfo
            {
                Entry = (uint)index[0],
                ProductInfoID = productid,
                NormalPrice = (long)normalprice,
                CurrentPrice = (long)currentprice,
                ProductInfoFlags = (int)productinfoflags,
                Unknown2 = (int)unknown2,
                Unknown3 = (int)unknown3,
                Unknown4 = (int)unknown4,
                Unknown5 = (int)unknown5,
                DeliverableProductIDs = deliverableproductids,
                ChoiceType = (int)choiceType,
                DisplayFlag = 0,
                HasUnknown1InDisplayInfo = 0,
                HasBattlePayDisplayInfo = hasbattlepaydisplayinfo
            };
            Storage.BattlePayProductInfos.Add(productInfo, packet.TimeSpan);

            if (choiceType != 0 && choiceType != 32 && choiceType != 103)
                ReadDisplayInfo(packet, index);

            if (choiceType == 32)
            {
                packet.ReadUInt16("Unknown32_1", index);
                packet.ReadUInt16("Unknown32_2", index);
            }

            if (choiceType == 103)
            {
                packet.ReadUInt16("Unknown103_1", index);
                packet.ReadUInt32("Unknown103_2", index);
                packet.ReadUInt16("Unknown103_3", index);
            }
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
            packet.ReadBit("IsPet", index);
            var hasPetResult = packet.ReadBit("HasPetResult", index);
            var hasDisplay = packet.ReadBit("HasDisplayInfo", index);

            if (hasPetResult)
                packet.ReadBits("PetResult", 4, index);

            if (hasDisplay)
                ReadDisplayInfo(packet, index);
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
            var unknown10 = packet.ReadUInt16("Unknown10", index);
            var unknown11 = packet.ReadUInt16("Unknown11", index);

            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 8, index);
            var alreadyowned = packet.ReadBit("AlreadyOwned", index) ? 1 : 0;
            var hasUnknownBits = packet.ReadBit("HasUnknownBits", index);
            var itemCount = packet.ReadBits("ItemCount", 7, index);
            var hasdisplayinfo = packet.ReadBit("HasDisplayInfo", index) ? 1 : 0;

            uint petresultvariable = 0;
            if (hasUnknownBits)
                petresultvariable = packet.ReadBits("UnknownBits", 4, index);

            for (uint i = 0; i < itemCount; i++)
                ReadProductItem(packet, index, i);

            var name = packet.ReadWoWString("Name", nameLen, index);

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
                Unknown11 = unknown11,
                Name = name,
                AlreadyOwned = alreadyowned,
                HasDisplayInfo = hasdisplayinfo,
                PetResultVariable = petresultvariable,
                DisplayFlag = 0
            };
            Storage.BattlePayProductDatas.Add(product, packet.TimeSpan);

            if (hasdisplayinfo == 1)
                ReadDisplayInfo(packet, index);
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

            var name = packet.ReadWoWString("Name", nameLen, index);
            var description = descLen > 1 ? packet.ReadWoWString("Description", descLen, index) : "";

            BattlePayGroup group = new BattlePayGroup
            {
                GroupID = groupid,
                IconFileDataID = iconfiledataid,
                DisplayType = displaytype,
                Ordering = ordering,
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
            var hasbattlepaydisplayinfo = packet.ReadBit("HasDisplayInfo", index) ? 1 : 0;

            BattlePayShop shop = new BattlePayShop
            {
                Entry = (uint)index[0],
                EntryID = entryid,
                GroupID = groupid,
                ProductID = productid,
                Ordering = ordering,
                VasServiceType = vasservicetype,
                StoreDeliveryType = storedeliverytype,
                HasBattlePayDisplayInfo = hasbattlepaydisplayinfo,
                Unknown = 0,
                DisplayFlag = 0
            };
            Storage.BattlePayShopDatas.Add(shop, packet.TimeSpan);

            if (hasbattlepaydisplayinfo == 1)
                ReadDisplayInfo(packet, index);
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
            packet.ReadWoWString("WalletName", walletLen, index);
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

        [Parser(Opcode.SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN)]
        public static void HandleFeatureSystemStatusGlueScreen(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("BpayStoreAvailable");
            packet.ReadBit("BpayStoreDisabledByParentalControls");
            packet.ReadBit("CharUndeleteEnabled");
            packet.ReadBit("CommerceServerEnabled");
            packet.ReadBit("VeteranTokenRedeemWillKick");
            packet.ReadBit("WorldTokenRedeemWillKick");
            packet.ReadBit("ExpansionPreorderInStore");
            packet.ReadBit("KioskModeEnabled");
            packet.ReadBit("CompetitiveModeEnabled");
            packet.ReadBit("BoostEnabled");
            packet.ReadBit("TrialBoostEnabled");
            packet.ReadBit("RedeemForBalanceAvailable");
            packet.ReadBit("PaidCharacterTransfersBetweenBnetAccountsEnabled");
            packet.ReadBit("LiveRegionCharacterListEnabled");
            packet.ReadBit("LiveRegionCharacterCopyEnabled");
            packet.ReadBit("LiveRegionAccountCopyEnabled");
            packet.ReadBit("LiveRegionKeyBindingsCopyEnabled");
            packet.ReadBit("BrowserCrashReporterEnabled");
            packet.ReadBit("IsEmployeeAccount");
            var hasEuropaTicketConfig = packet.ReadBit("HasEuropaTicketConfig");
            packet.ReadBit("NameReservationOnly");
            var hasLaunchDurationETA = packet.ReadBit("HasLaunchDurationETA");
            packet.ReadBit("TimerunningEnabled");
            packet.ReadBit("ScriptsDisallowedForBeta");
            packet.ReadBit("PlayerIdentityOptionsEnabled");
            packet.ReadBit("AccountExportEnabled");
            packet.ReadBit("AccountLockedPostExport");
            var realmHiddenAlertLength = packet.ReadBits("RealmHiddenAlertLength", 11);
            packet.ReadBit("BNSendWhisperUseV2Services");
            packet.ReadBit("BNSendGameDataUseV2Services");
            packet.ReadBit("CharacterSelectListModeRealmless");
            packet.ReadBit("WowTokenLimitedMode");
            packet.ReadBit("Unused1");
            packet.ReadBit("Unused2");
            packet.ReadBit("PandarenLevelBoostAllowed");

            if (hasEuropaTicketConfig)
            {
                packet.ReadBit("EuropaTicketSystemEnabled");
                packet.ReadBit("EuropaTicketSystemThrottleEnabled");
            }

            packet.ReadUInt32("CommercePricePollTimeSeconds");
            packet.ReadUInt32("KioskSessionDurationMinutes");
            packet.ReadInt64("RedeemForBalanceAmount");
            packet.ReadInt32("MaxCharactersOnThisRealm");
            var liveRegionCharacterCopySourceRegionsCount = packet.ReadUInt32("LiveRegionCharacterCopySourceRegionsCount");
            packet.ReadInt32("ActiveBoostType");
            packet.ReadInt32("TrialBoostType");
            packet.ReadInt32("MinimumExpansionLevel");
            packet.ReadInt32("MaximumExpansionLevel");
            packet.ReadInt32("ContentSetID");
            var gameRulesCount = packet.ReadUInt32("GameRulesCount");
            packet.ReadInt32("ActiveTimerunningSeasonID");
            packet.ReadInt32("RemainingTimerunningSeasonSeconds");
            packet.ReadInt16("MaxPlayerGuidLookupsPerRequest");
            packet.ReadInt16("NameLookupTelemetryInterval");
            packet.ReadUInt32("NotFoundCacheTimeSeconds");
            var debugTimeEventsCount = packet.ReadUInt32("DebugTimeEventsCount");
            packet.ReadInt32("MostRecentTimeEventID");
            packet.ReadUInt32("EventRealmQueues");

            if (hasLaunchDurationETA)
                packet.ReadInt32("LaunchDurationETA");

            packet.ReadWoWString("RealmHiddenAlert", realmHiddenAlertLength);

            for (uint i = 0; i < liveRegionCharacterCopySourceRegionsCount; i++)
                packet.ReadInt32("LiveRegionCharacterCopySourceRegion", i);

            for (uint i = 0; i < gameRulesCount; i++)
            {
                packet.ReadInt32("GameRuleType", i);
                packet.ReadInt32("GameRuleValue", i);
            }

            for (uint i = 0; i < debugTimeEventsCount; i++)
            {
                packet.ReadInt32("DebugTimeEventID", i);
                packet.ReadUInt64("DebugTimeEventTime", i);
                packet.ReadCString("DebugTimeEventText", i);
            }
        }

    }
}