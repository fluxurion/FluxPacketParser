using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
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

        private static void ReadDisplayInfo(Packet packet, params object[] index)
        {
            packet.ResetBitReader();
            var hasUIModelSceneID = packet.ReadBit("HasUIModelSceneID", index);
            var hasIconFileDataID = packet.ReadBit("HasIconFileDataID", index);

            var titleLen = packet.ReadBits("TitleLength", 10, index);
            var title2Len = packet.ReadBits("Title2Length", 10, index);
            var descLen = packet.ReadBits("DescriptionLength", 13, index);
            var desc2Len = packet.ReadBits("Description2Length", 13, index);
            var desc3Len = packet.ReadBits("Description3Length", 13, index);

            var hasIconBorder = packet.ReadBit("HasIconBorder", index);
            var hasUnk1 = packet.ReadBit("HasUnk1", index);
            var hasUnk2 = packet.ReadBit("HasUnk2", index);
            var hasUiTextureAtlas = packet.ReadBit("HasUiTextureAtlasMemberID", index);

            var desc4Len = packet.ReadBits("Description4Length", 13, index);
            var desc5Len = packet.ReadBits("Description5Length", 13, index);
            packet.ResetBitReader();

            var visualCount = packet.ReadUInt32("VisualCount", index);
            packet.ReadUInt32("CardType", index);
            packet.ReadUInt32("Unk3", index);
            packet.ReadUInt32("Unk4", index);

            if (hasIconFileDataID)
                packet.ReadUInt32("IconFileDataID", index);
            if (hasUIModelSceneID)
                packet.ReadUInt32("UIModelSceneID", index);

            packet.ReadWoWString("Title", titleLen, index);
            packet.ReadWoWString("Title2", title2Len, index);
            packet.ReadWoWString("Description", descLen, index);
            packet.ReadWoWString("Description2", desc2Len, index);
            packet.ReadWoWString("Description3", desc3Len, index);

            if (hasIconBorder)
                packet.ReadUInt32("IconBorder", index);
            if (hasUnk1)
                packet.ReadUInt32("Unk1", index);
            if (hasUnk2)
                packet.ReadUInt32("Unk2", index);
            if (hasUiTextureAtlas)
                packet.ReadUInt32("UiTextureAtlasMemberID", index);

            packet.ReadWoWString("Description4", desc4Len, index);
            packet.ReadWoWString("Description5", desc5Len, index);

            for (uint i = 0; i < visualCount; i++)
            {
                packet.ResetBitReader();
                var nameLen = packet.ReadBits("VisualNameLength", 15, index, i);
                packet.ResetBitReader();
                packet.ReadUInt32("CreatureDisplayID", index, i);
                packet.ReadUInt32("PreviewUIModelSceneID", index, i);
                packet.ReadUInt32("TransmogSetID", index, i);
                packet.ReadWoWString("VisualName", nameLen, index, i);
            }
        }

        private static void ReadProductInfo(Packet packet, params object[] index)
        {
            packet.ReadUInt32("ProductID", index);
            packet.ReadUInt64("NormalPrice", index);
            packet.ReadUInt64("CurrentPrice", index);
            var bundleCount = packet.ReadUInt32("BundleProductIDCount", index);
            packet.ReadUInt32("Unk1", index);
            var unkIntCount = packet.ReadUInt32("UnkIntCount", index);
            packet.ReadUInt32("Flags", index);

            for (uint i = 0; i < bundleCount; i++)
                packet.ReadUInt32("BundleProductID", index, i);

            for (uint i = 0; i < unkIntCount; i++)
                packet.ReadUInt32("UnkInt", index, i);

            packet.ResetBitReader();
            packet.ReadBits("ChoiceType", 7, index);
            var hasDisplay = packet.ReadBit("HasDisplayInfo", index);
            packet.ResetBitReader();

            if (hasDisplay)
                ReadDisplayInfo(packet, index);
        }

        private static void ReadProductItem(Packet packet, params object[] index)
        {
            packet.ReadUInt32("ID", index);
            packet.ReadByte("UnkByte", index);
            packet.ReadUInt32("ItemID", index);
            packet.ReadUInt32("Quantity", index);
            packet.ReadUInt32("UnkInt1", index);
            packet.ReadUInt32("UnkInt2", index);

            packet.ResetBitReader();
            packet.ReadBit("IsPet", index);
            var hasPetResult = packet.ReadBit("HasPetResult", index);
            var hasDisplay = packet.ReadBit("HasDisplayInfo", index);
            if (hasPetResult)
                packet.ReadBits("PetResult", 4, index);
            packet.ResetBitReader();

            if (hasDisplay)
                ReadDisplayInfo(packet, index);
        }

        private static void ReadProduct(Packet packet, params object[] index)
        {
            packet.ReadUInt32("ProductId", index);
            packet.ReadByte("Type", index);
            packet.ReadUInt32("ItemID", index);
            packet.ReadUInt32("ItemCount", index);
            packet.ReadUInt32("MountSpellID", index);
            packet.ReadUInt32("BattlePetSpeciesCreatureID", index);
            packet.ReadUInt32("Unk1", index);
            packet.ReadUInt32("Unk2", index);
            packet.ReadUInt32("Unk3", index);
            packet.ReadUInt32("TransmogSetID", index);
            packet.ReadUInt32("Unk8", index);
            packet.ReadUInt32("Unk9", index);

            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 8, index);
            var hasUnkBits = packet.ReadBit("HasUnkBits", index);
            packet.ReadBit("AlreadyOwned", index);
            var hasDisplay = packet.ReadBit("HasDisplayInfo", index);
            if (hasUnkBits)
                packet.ReadBits("UnkBits", 4, index);
            packet.ResetBitReader();

            packet.ReadWoWString("Name", nameLen, index);

            if (hasDisplay)
                ReadDisplayInfo(packet, index);
        }

        private static void ReadGroup(Packet packet, params object[] index)
        {
            packet.ReadUInt32("GroupId", index);
            packet.ReadUInt32("IconFileDataID", index);
            packet.ReadByte("DisplayType", index);
            packet.ReadUInt32("Ordering", index);
            packet.ReadUInt32("Unk", index);
            packet.ReadUInt32("MainGroupID", index);

            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 8, index);
            var descLen = packet.ReadBits("DescriptionLength", 24, index);
            packet.ResetBitReader();

            packet.ReadWoWString("Name", nameLen, index);
            if (descLen > 1)
                packet.ReadWoWString("Description", descLen, index);
        }

        private static void ReadShop(Packet packet, params object[] index)
        {
            packet.ReadUInt32("EntryID", index);
            packet.ReadUInt32("GroupID", index);
            packet.ReadUInt32("ProductID", index);
            packet.ReadUInt32("Ordering", index);
            packet.ReadUInt32("VasServiceType", index);
            packet.ReadByte("StoreDeliveryType", index);
            packet.ResetBitReader();
            var hasDisplay = packet.ReadBit("HasDisplayInfo", index);
            if (hasDisplay)
                ReadDisplayInfo(packet, index);
        }

        private static void ReadDistributionObject(Packet packet, params object[] index)
        {
            packet.ReadUInt64("DistributionID", index);
            packet.ReadUInt32("Status", index);
            packet.ReadUInt32("ProductID", index);
            packet.ReadPackedGuid128("TargetPlayer", index);
            packet.ReadPackedGuid128("UnkGuid", index);
            packet.ReadUInt32("TargetVirtualRealm", index);
            packet.ReadUInt32("TargetNativeRealm", index);
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadUInt32("Unk55AC", index);

            packet.ResetBitReader();
            var hasProducts = packet.ReadBit("HasProducts", index);
            packet.ReadBit("Revoked", index);
            packet.ResetBitReader();

            if (hasProducts)
                ReadProduct(packet, index);
        }

        private static void ReadPurchase(Packet packet, params object[] index)
        {
            packet.ReadUInt64("PurchaseID", index);
            packet.ReadUInt32("Status", index);
            packet.ReadUInt32("ResultCode", index);
            packet.ReadUInt32("ProductID", index);
            packet.ReadUInt64("UnkLong", index);
            packet.ReadUInt64("UnkLong2", index);
            packet.ReadUInt32("UnkInt", index);

            packet.ResetBitReader();
            var walletLen = packet.ReadBits("WalletNameLength", 8, index);
            packet.ResetBitReader();
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
            packet.ResetBitReader();
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
            packet.ReadUInt32("UnkInt1");
            packet.ReadUInt32("UnkInt2");
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_REQUEST_PRICE_INFO)]
        public static void HandleBattlePayRequestPriceInfo(Packet packet)
        {
            packet.ReadByte("UnkByte");
        }
    }
}