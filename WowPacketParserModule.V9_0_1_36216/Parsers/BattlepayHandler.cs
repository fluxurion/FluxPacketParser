using System.Collections.Generic;
using System.Text.RegularExpressions;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using static Grpc.Core.Metadata;

namespace WowPacketParserModule.V9_0_1_36216.Parsers
{

    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        private static void ReadBattlepayDisplayInfo(Packet packet, int counter, params object[] idx)
        {
            packet.ResetBitReader();

            var HasIconFileDataID = packet.ReadBit("HasIconFileDataID", idx);
            var HasUIModelSceneID = packet.ReadBit("HasUIModelSceneID", idx);

            var TitleSize = packet.ReadBits(10);
            var Title2Size = packet.ReadBits(10); 
            var DescriptionSize = packet.ReadBits(13); 
            var Description2Size = packet.ReadBits(13);
            var Description3Size = packet.ReadBits(13);

            var HasDisplayInfoFlags = packet.ReadBit("HasDisplayInfoFlags", idx);
            var HasOverrideTextColor = packet.ReadBit("HasOverrideTextColor", idx);
            var HasOverrideTexture = packet.ReadBit("HasOverrideTexture", idx);
            var HasUiTextureAtlasMemberID = packet.ReadBit("HasUiTextureAtlasMemberID", idx);

            var Description4Size = packet.ReadBits(13);
            var Description5Size = packet.ReadBits(13);

            var VisualsSize = packet.ReadInt32("VisualsSize", idx);

            var CardType = packet.ReadInt32("CardType", idx);
            var IconFileDataID = packet.ReadInt32("IconFileDataID", idx);
            var UIModelSceneID = packet.ReadInt32("UIModelSceneID", idx);

            var iconFileDataID = 0;
            var uIModelSceneID = 0;

            if (HasIconFileDataID)
                iconFileDataID = packet.ReadInt32("IconFileDataID", idx);
            if (HasUIModelSceneID)
                uIModelSceneID = packet.ReadInt32("UIModelSceneID", idx);

            var Title = packet.ReadWoWString("Title", TitleSize, idx);
            var Title2 = packet.ReadWoWString("Title2", Title2Size, idx);
            var Description = packet.ReadWoWString("Description", DescriptionSize, idx);
            var Description2 = packet.ReadWoWString("Description2", Description2Size, idx);
            var Description3 = packet.ReadWoWString("Description3", Description3Size, idx);

            var displayInfoFlags = 0;
            var overrideTextColor = 0;
            var overrideTexture = 0;
            var uiTextureAtlasMemberID = 0;

            if (HasDisplayInfoFlags)
                displayInfoFlags = packet.ReadInt32("DisplayInfoFlags", idx);
            if (HasOverrideTextColor)
                overrideTextColor = packet.ReadInt32("OverrideTextColor", idx);
            if (HasOverrideTexture)
                overrideTexture = packet.ReadInt32("OverrideTexture", idx);
            if (HasUiTextureAtlasMemberID)
                uiTextureAtlasMemberID = packet.ReadInt32("UiTextureAtlasMemberID", idx);

            var Description4 = packet.ReadWoWString("Description4", Description4Size, idx);
            var Description5 = packet.ReadWoWString("Description5", Description5Size, idx);

            // BATTLEPAY VISUALS
            List<string> previewTitles = new List<string>();
            List<string> previewCreatureDisplayIDs = new List<string>();
            List<string> previewUIModelSceneIDs = new List<string>();
            List<string> previewTransmogSets = new List<string>();
            for (int j = 0; j < VisualsSize; j++)
            {
                packet.ResetBitReader();
                var VisualTitleSize = packet.ReadBits(10);
                var CreatureDisplayID = packet.ReadInt32("CreatureDisplayID", idx, j);
                var PreviewUIModelSceneID = packet.ReadInt32("PreviewUIModelSceneID", idx, j);
				var TransmogSetID = packet.ReadInt32("TransmogSetID", idx, j);
                var VisualTitle = packet.ReadWoWString("VisualTitle", VisualTitleSize, idx, j);

                previewTitles.Add(VisualTitle);
                previewCreatureDisplayIDs.Add(CreatureDisplayID.ToString());
                previewUIModelSceneIDs.Add(PreviewUIModelSceneID.ToString());
                previewTransmogSets.Add(TransmogSetID.ToString());
            }

            string previewTitles_String = string.Join(",", previewTitles);
            string previewCreatureDisplayIDs_String = string.Join(",", previewCreatureDisplayIDs);
            string previewUIModelSceneIDs_String = string.Join(",", previewUIModelSceneIDs);
            string previewTransmogSets_String = string.Join(",", previewTransmogSets);

            BattlePayDisplayInfo displayInfo = new BattlePayDisplayInfo();
            displayInfo.Entry = entry;
            displayInfo.ProductInfoID = productInfoID;
            displayInfo.ProductDataID = productID;
            displayInfo.ShopDataID = shopID;
            displayInfo.CardType = CardType;
            displayInfo.Unk1 = Unk1;
            displayInfo.ProductMultiplier = ProductMultiplier;
            displayInfo.IconFileDataID = IconFileDataID;
            displayInfo.UIModelSceneID = UIModelSceneID;
            displayInfo.Title = Title;
            displayInfo.Title2 = Title2;
            displayInfo.Description = Description;
            displayInfo.Description2 = Description2;
            displayInfo.Description3 = Description3;
            displayInfo.IconBorder = displayInfoFlags;
            displayInfo.Unk2 = Unk2;
            displayInfo.UiTextureAtlasMemberID = UiTextureAtlasMemberID;
            displayInfo.UiTextureAtlasMemberID2 = UiTextureAtlasMemberID2;
            displayInfo.Description4 = Description4;
            displayInfo.Description5 = Description5;
            displayInfo.PreviewCreatureDisplayIDs = previewCreatureDisplayIDs_String;
            displayInfo.PreviewUIModelSceneIDs = previewUIModelSceneIDs_String;
            displayInfo.PreviewTransmogSets = previewTransmogSets_String;
            displayInfo.PreviewTitles = previewTitles_String;
            Storage.BattlePayDisplayInfos.Add(displayInfo);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE)]
        public static void HandletBattlePayGetProductListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            var currencyid = packet.ReadUInt32("CurrencyID");

            var ProductInfoSize = packet.ReadUInt32("ProductInfoSize");
            var ProductSize = packet.ReadUInt32("ProductSize");
            var ProductGroupSize = packet.ReadUInt32("ProductGroupSize");
            var ShopSize = packet.ReadUInt32("ShopSize");

            // BATTLEPAY PRODUCT INFO
            for (uint index = 0; index < ProductInfoSize; index++)
            {
                var productid = packet.ReadInt32("ProductInfoID", index);
                var normalprice = packet.ReadInt64("NormalPrice", index);
                var currentprice = packet.ReadInt64("CurrentPrice", index);
                var DeliverableProductsSize = packet.ReadInt32("DeliverableProductsSize", index);
                var UnkInt2 = packet.ReadInt32("Unk1", index);
                var UnkInt3 = packet.ReadInt32("Unk2", index);
                var UnkIntsSize = packet.ReadInt32("UnkIntsSize", index);

                var UnkInt4 = packet.ReadInt32("Flags", index);

                List<string> subproducts = new List<string>();
                for (int j = 0; j < DeliverableProductsSize; j++)
                {
                    var DeliverableProduct = packet.ReadInt32("DeliverableProduct", index, j);

                    subproducts.Add(DeliverableProduct.ToString());
                }
                string subproducts_string = string.Join(",", subproducts);

                var UnkInts = 0;
                for (int j = 0; j < UnkIntsSize; j++)
                {
                    UnkInts = packet.ReadInt32("UnkInts", index, j);
                }

                packet.ResetBitReader();

                var choicetype = packet.ReadBits("ChoiceType", 7, index);

                var HasBattlepayDisplayInfo = packet.ReadBit("HasBattlepayDisplayInfo", index);

                uint displayentry = 0;
                if (HasBattlepayDisplayInfo)
                {
                    ReadBattlepayDisplayInfo(packet, ((int)index), index);
                    displayentry = index+1;
                }

                BattlePayProductInfo productInfo = new BattlePayProductInfo();
                productInfo.Entry = index;
                productInfo.ProductInfoID = ProductInfoID;
                productInfo.NormalPrice = NormalPrice;
                productInfo.CurrentPrice = CurrentPrice;
                productInfo.ProductInfoFlags = ProductInfoFlags;
                productInfo.Unk3 = Unk3;
                productInfo.Unk4 = Unk4;
                productInfo.Unk5 = Unk5;
                productInfo.Unk6 = Unk6;
                productInfo.DeliverableProductIDs = subproducts_string;
                productInfo.DisplayFlag = DisplayFlag;
                productInfo.HasUnk1InDisplayInfo = HasUnk1InDisplayInfo;
                productInfo.HasBattlePayDisplayInfo = HasBattlePayDisplayInfo;
                productInfo.Unk7 = Unk7;
                productInfo.ParentProductID = parentProductID;
                productInfo.Unk8 = Unk8;
                Storage.BattlePayProductInfos.Add(productInfo);
            }

            // BATTLEPAY PRODUCT
            for (uint j = 0; j < ProductSize; j++)
            {
                var productid = packet.ReadUInt32("ProductId", j);
                var type = packet.ReadByte("Type", j);
                var flags = packet.ReadUInt32("ItemID", j);
                var UnkInt1 = packet.ReadUInt32("ItemCount", j);
                var displayid = packet.ReadUInt32("MountSpellID", j);
                var ItemId = packet.ReadUInt32("BattlePetSpeciesCreatureID", j);
                var UnkInt4 = packet.ReadUInt32("Unk1", j);
                var UnkInt5 = packet.ReadUInt32("Unk2", j);
                var UnkInt6 = packet.ReadUInt32("Unk3", j);
                var UnkInt7 = packet.ReadUInt32("TransmogSetID", j);
                var UnkInt8 = packet.ReadUInt32("Unk8", j);
                var UnkInt9 = packet.ReadUInt32("Unk9", j);

                packet.ResetBitReader();

                var UnkString = packet.ReadBits("UnkString", 8, j);
                var UnkBit = packet.ReadBit("AlreadyOwned", j);
                var UnkBits = packet.ReadBit("UnkBits", j);
                var ItemsSize = packet.ReadBits("ItemsSize", 7, j);
                var HasDisplayInfo = packet.ReadBit("HasDisplayInfo", j);//OkHere

                uint PetResultVariable = 0;
                if (UnkBits)
                    PetResultVariable = packet.ReadBits("PetResultVariable", 4, j);

                // BATTLEPAY ITEM
                for (uint g = 0; g < ItemsSize; g++)
                {
                    var id = packet.ReadUInt32("Id", g);
                    var unkbyte = packet.ReadByte("UnkByte", g);
                    var itemid = packet.ReadUInt32("ItemID", g);
                    var quantity = packet.ReadUInt32("Quantity", g);
                    var UnkInt1_ = packet.ReadUInt32("UnkInt1", g);
                    var UnkInt2_ = packet.ReadUInt32("UnkInt2", g);

                    packet.ResetBitReader();

                    var HasPet = packet.ReadBit("HasPet", g);
                    var PetResult = packet.ReadBit("PetResult", g);
                    var DisplayInfo = packet.ReadBit("DisplayInfos", g);

                    uint PetResultVariable_ = 0;
                    if (PetResult)
                        PetResultVariable_ = packet.ReadBits("PetResultVariable", 4, g);

                    if (DisplayInfo)
                        ReadBattlepayDisplayInfo(packet, 1000, g);
                }

                var name = packet.ReadWoWString("Name", UnkString, j);

                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, 2000, j);

                BattlePayProduct productData = new BattlePayProduct();
                productData.Entry = j;
                productData.ProductID = productid;
                productData.Type = Type;
                productData.Unk9 = Unk9;
                productData.Unk10 = Unk10;
                productData.ItemID = ItemID;
                productData.Amount = Amount;
                productData.MountSpellID = MountSpellID;
                productData.BattlePetSpeciesCreatureID = BattlePetSpeciesCreatureID;
                productData.Unk11 = Unk11;
                productData.Unk12 = Unk12;
                productData.Unk13 = Unk13;
                productData.TransmogSetID = TransmogSetID;
                productData.Unk14 = Unk14;
                productData.Unk15 = Unk15;
                productData.HasDisplayInfo = HasDisplayInfo;
                productData.PetResultVariable = PetResultVariable;
                productData.Name = name;
                productData.Unk19 = Unk19;
                productData.DisplayFlag = DisplayFlag;
                Storage.BattlePayProductDatas.Add(productData);
            }


            // BATTLE PAY GROUP
            for (int i = 0; i < ProductGroupSize; i++)
            {
                var groupid = packet.ReadInt32("GroupID", i);
                var iconfiledataid = packet.ReadInt32("IconFileDataID", i);
                var displaytype = packet.ReadByte("DisplayType", i);
                var ordering = packet.ReadInt32("Ordering", i);
                var unkt = packet.ReadInt32("unkt", i);

                packet.ResetBitReader();

                var bits4 = packet.ReadBits("nameLength", 8, i);
                var bits7 = packet.ReadBits("IsAvailableDescription", 24, i);
                var name = packet.ReadWoWString("Name", bits4, i);
                var description = "";
                if (bits7 > 1)
                    description = packet.ReadWoWString("Description", bits7, i);

                BattlePayGroup Group = new BattlePayGroup
                {
                    GroupID = ((uint)groupid),
                    IconFileDataID = ((uint)iconfiledataid),
                    DisplayType = ((uint)displaytype),
                    Ordering = ((uint)ordering),
                    Unk = ((uint)unkt),
                    Name = name,
                    Description = description,
                };
                Storage.BattlePayGroups.Add(Group, packet.TimeSpan);
            }

            // BATTLEPAY SHOP
            for (uint i = 0; i < ShopSize; i++)
            {
                var entryid = packet.ReadUInt32("EntryID", i);
                var groupid = packet.ReadUInt32("GroupID", i);
                var productid = packet.ReadUInt32("ProductInfoID", i);
                var ordering = packet.ReadInt32("Ordering", i);
                var vasservicetype = packet.ReadInt32("VasServiceType", i);
                var storedeliverytype = packet.ReadByte("StoreDeliveryType", i);

                packet.ResetBitReader();

                var bit5172 = packet.ReadBit("HasBattlepayDisplayInfo", i);
                if (bit5172)
                    ReadBattlepayDisplayInfo(packet, ((int)i), i);

                BattlePayShop shopData = new BattlePayShop();
                shopData.Entry = i;
                shopData.ShopEntry = entryid;
                shopData.GroupID = GroupID;
                shopData.ProductInfoID = ProductInfoID;
                shopData.Ordering = Ordering;
                shopData.VasServiceType = VasServiceType;
                shopData.StoreDeliveryType = StoreDeliveryType;
                shopData.HasBattlePayDisplayInfo = HasBattlePayDisplayInfo;
                shopData.Unk21 = Unk21;
                shopData.Unk22 = Unk22;
                shopData.DisplayFlag = DisplayFlag;
                Storage.BattlePayShopDatas.Add(shopData);
            }
        }

        private static void ReadBattlePayDistributionObject(Packet packet, params object[] index)
        {
            packet.ReadInt64("DistributionID", index);

            packet.ReadInt32("Status", index);
            packet.ReadInt32("ProductID", index);

            packet.ReadPackedGuid128("TargetPlayer", index);
            packet.ReadPackedGuid128("UnkGuid", index);
            packet.ReadInt32("TargetVirtualRealm", index);
            packet.ReadInt32("TargetNativeRealm", index);

            packet.ReadInt64("PurchaseID", index);

            packet.ReadInt32("Unk55AC", index);

            packet.ResetBitReader();

            // 2 bits good
            var bit5248 = packet.ReadBit("HasBattlePayProduct", index);
            packet.ReadBit("Revoked", index);

            if (bit5248)
            {
                var productid = packet.ReadUInt32("ProductId", index);
                packet.ReadByte("Type", index);
                packet.ReadUInt32("Flags", index);
                packet.ReadUInt32("UnkInt1", index);
                packet.ReadUInt32("DisplayId", index);
                packet.ReadUInt32("ItemId", index);
                packet.ReadUInt32("UnkInt4", index);
                packet.ReadUInt32("UnkInt5", index);

                packet.ReadUInt32("UnkInt6", index);
                packet.ReadUInt32("UnkInt7", index);
                packet.ReadUInt32("UnkInt8", index);
                packet.ReadUInt32("UnkInt9", index);

                packet.ResetBitReader();

                // unsure about 8 bit read here
                var UnkString = packet.ReadBits("UnkString", 8, index);
                var UnkBit = packet.ReadBit("UnkBit", index);
                var UnkBits = packet.ReadBit("UnkBits", index);
                var ItemsSize = packet.ReadBits("ItemsSize", 7, index);
                var HasDisplayInfo = packet.ReadBit("HasDisplayInfo", index);//OkHere

                if (UnkBits)
                    packet.ReadBits("PetResultVariable", 4, index);

                for (uint g = 0; g < ItemsSize; g++)
                {
                    packet.ReadUInt32("Id", g);
                    packet.ReadByte("UnkByte", g);
                    packet.ReadUInt32("ItemID", g);
                    packet.ReadUInt32("Quantity", g);
                    packet.ReadUInt32("UnkInt1", g);
                    packet.ReadUInt32("UnkInt2", g);

                    packet.ResetBitReader();

                    var HasPet = packet.ReadBit("HasPet", g);
                    var PetResult = packet.ReadBit("PetResult", g);
                    var DisplayInfo = packet.ReadBit("DisplayInfos", g);

                    if (PetResult)
                        packet.ReadBits("PetResultVariable", 4, g);

                    if (DisplayInfo)
                        ReadBattlepayDisplayInfo(packet, 3000, g);

                }
                packet.ReadWoWString("Name", UnkString, index);
                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, 4000, index);
            }
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE)]
        public static void HandleBattlePayGetDistributionListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");

            packet.ResetBitReader();
            // 8 + (8 - 5) = 11 good
            var int6 = packet.ReadBits("BattlePayDistributionObjectCount", 11);
            for (uint index = 0; index < int6; index++)
                ReadBattlePayDistributionObject(packet, index);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_DISTRIBUTION_UPDATE)]
        public static void HandleBattlePayDistributionUpdate(Packet packet)
        {
            ReadBattlePayDistributionObject(packet);
        }

        [Parser(Opcode.CMSG_BATTLE_PAY_DISTRIBUTION_ASSIGN_TO_TARGET)]
        public static void HandleBattlePayDistributionAssignToTarget(Packet packet)
        {
            packet.ReadInt32("ClientToken");
            packet.ReadUInt64("DistributionID");
            packet.ReadPackedGuid128("TargetCharacter");
            packet.ReadInt32("ProductChoice");
        }



    }
}