using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using static Grpc.Core.Metadata;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using Google.Protobuf;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Linq;

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{

    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        private static void ReadBattlepayDisplayInfo(Packet packet, uint entry, uint productInfoID, uint productID, uint shopID, bool HasUnk1InDisplayInfo, uint displayInfoFlag, params object[] index)
        {
            // BATTLEPAY DISPLAYINFO
            var TitleSize = packet.ReadBits("TitleSize", 10, index, "DisplayInfo");
            var Title2Size = packet.ReadBits("Title2Size", 10, index, "DisplayInfo");
            var DescriptionSize = packet.ReadBits("DescriptionSize", 13, index, "DisplayInfo");
            var Description2Size = packet.ReadBits("Description2Size", 13, index, "DisplayInfo");
            var Description3Size = packet.ReadBits("Description3Size", 13, index, "DisplayInfo");

            var HasIconBorder = packet.ReadBit("HasIconBorder", index, "DisplayInfo");
            var HasUnk2 = packet.ReadBit("HasUnk2", index, "DisplayInfo");
            var HasUiTextureAtlasMemberID = packet.ReadBit("HasUiTextureAtlasMemberID", index, "DisplayInfo");
            var HasUiTextureAtlasMemberID2 = packet.ReadBit("UiTextureAtlasMemberID2", index, "DisplayInfo");

            var Description4Size = packet.ReadBits("Description4Size", 13, index, "DisplayInfo");
            var Description5Size = packet.ReadBits("Description5Size", 12, index, "DisplayInfo");

            packet.ResetBitReader();

            var VisualsSize = packet.ReadInt32("VisualsSize", index, "DisplayInfo");
            var CardType = packet.ReadInt32("CardType", index, "DisplayInfo");

            var Unk1 = 0;
            if (HasUnk1InDisplayInfo)
                Unk1 = packet.ReadInt32("Unk1", index, "DisplayInfo");

            var ProductMultiplier = 0;
            if (displayInfoFlag != 1)
                ProductMultiplier = packet.ReadInt32("ProductMultiplier", index, "DisplayInfo"); // Like 6 Character Transfers

            var IconFileDataID = packet.ReadInt32("IconFileDataID", index, "DisplayInfo");            
            var UIModelSceneID = packet.ReadInt32("UIModelSceneID", index, "DisplayInfo");

            var Title = "Untitled";

            Title = packet.ReadWoWString("Title", TitleSize, index, "DisplayInfo");
            var Title2 = packet.ReadWoWString("Title2", Title2Size, index, "DisplayInfo");
            var Description = packet.ReadWoWString("Description", DescriptionSize, index, "DisplayInfo");
            var Description2 = packet.ReadWoWString("Description2", Description2Size, index, "DisplayInfo");
            var Description3 = packet.ReadWoWString("Description3", Description3Size, index, "DisplayInfo");

            var IconBorder = 0;
            var Unk2 = 0;
            var UiTextureAtlasMemberID = 0;
            var UiTextureAtlasMemberID2 = 0;

            if (HasIconBorder)
                IconBorder = packet.ReadInt32("IconBorder", index, "DisplayInfo");
            if (HasUnk2)
                Unk2 = packet.ReadInt32("Unk2", index, "DisplayInfo");
            if (HasUiTextureAtlasMemberID)
                UiTextureAtlasMemberID = packet.ReadInt32("UiTextureAtlasMemberID", index, "DisplayInfo");
            if (HasUiTextureAtlasMemberID2)
                UiTextureAtlasMemberID2 = packet.ReadInt32("UiTextureAtlasMemberID2", index, "DisplayInfo");

            var Description4 = packet.ReadWoWString("Description4", Description4Size, index, "DisplayInfo");
            var Description5 = packet.ReadWoWString("Description5", Description5Size, index, "DisplayInfo");

            // BATTLEPAY VISUALS

            List<string> previewTitles = new List<string>();
            List<string> previewCreatureDisplayIDs = new List<string>();
            List<string> previewUIModelSceneIDs = new List<string>();
            List<string> previewTransmogSets = new List<string>();

            for (int j = 0; j < VisualsSize; j++)
            {
                var TitleSize_Visual = packet.ReadBits(10);
                packet.ResetBitReader();
                var CreatureDisplayID = packet.ReadInt32("CreatureDisplayID", index, "Visual", j);
                var PreviewUIModelSceneID = packet.ReadInt32("PreviewUIModelSceneID", index, "Visual", j);
				var TransmogSetID = packet.ReadInt32("TransmogSetID", index, "Visual", j);
                var Title_Visual = packet.ReadWoWString("Title_Visual", TitleSize_Visual, index, "Visual", j);

                previewTitles.Add(Title_Visual);
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
            displayInfo.IconBorder = IconBorder;
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
                var ProductInfoID = packet.ReadUInt32("ProductInfoID", index, "ProductInfo");
                var NormalPrice = packet.ReadInt64("NormalPrice", index, "ProductInfo");
                var CurrentPrice = packet.ReadInt64("CurrentPrice", index, "ProductInfo");
                var ProductIDsSize = packet.ReadInt32("DeliverableProductIDCount", index, "ProductInfo");
                var ProductInfoFlags = packet.ReadInt32("ProductInfoFlags", index, "ProductInfo");
                var Unk3 = packet.ReadInt32("Unk3", index, "ProductInfo");
                var Unk4 = packet.ReadInt32("Unk4", index, "ProductInfo");
                var Unk5 = packet.ReadInt32("Unk5", index, "ProductInfo");
                var Unk6 = packet.ReadInt32("Unk6", index, "ProductInfo");
                List<uint> subProductIDs = new List<uint>();
                // Biggest ProductIDsSize is 10 but instead of new table which would be hard to follow, i add them to a text blob
                List<string> subproducts = new List<string>();
                for (int j = 0; j < ProductIDsSize; j++)
                {
                    var subproductid = packet.ReadInt32("DeliverableProductIDs", index, j);

                    subProductIDs.Add((uint)subproductid);

                    subproducts.Add(subproductid.ToString());
                }
                string subproducts_string = string.Join(",", subproducts);

                packet.ResetBitReader();

                var DisplayFlag = packet.ReadByte("DisplayFlag", index, "ProductInfo");
                var parentProductID = 0;
                var HasUnk1InDisplayInfo = 0;
                var HasBattlePayDisplayInfo = 0;
                var Unk7 = 0;
                var Unk8 = 0;

                if (DisplayFlag > 0)
                {
                    HasUnk1InDisplayInfo = packet.ReadBit("HasUnk1InDisplayInfo", index, "ProductInfo");
                    HasBattlePayDisplayInfo = packet.ReadBit("HasBattlePayDisplayInfo", index, "ProductInfo");

                    if (HasBattlePayDisplayInfo != 0)
                        ReadBattlepayDisplayInfo(packet, index, ProductInfoID, 0, 0, HasUnk1InDisplayInfo != 0, 0, index);

                    if (HasUnk1InDisplayInfo == 0 && HasBattlePayDisplayInfo == 0)
                    {
                        Unk7 = packet.ReadInt16("Unk7", index, "ProductInfo");

                        if (DisplayFlag != 42 && DisplayFlag != 95 && DisplayFlag != 165)
                            parentProductID = packet.ReadInt32("ParentProductID", index, "ProductInfo");

                        Unk8 = packet.ReadByte("Unk8", index, "ProductInfo");
                    }
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
                var productid = packet.ReadUInt32("ProductID", j, "Product Data");
                var Type = packet.ReadByte("Type", j, "Product Data");
                var Unk9 = packet.ReadUInt16("Unk9", j, "Product Data");
                var Unk10 = packet.ReadByte("Unk10", j, "Product Data");
                var ItemID = packet.ReadUInt32("ItemID", j, "Product Data");
                var Amount = packet.ReadUInt32("Amount", j, "Product Data");
                var MountSpellID = packet.ReadUInt32("MountSpellID", j, "Product Data");
                var BattlePetSpeciesCreatureID = packet.ReadUInt32("BattlePetSpeciesCreatureID", j, "Product Data");
                var Unk11 = packet.ReadUInt32("Unk11", j, "Product Data");
                var Unk12 = packet.ReadUInt32("Unk12", j, "Product Data");
                var Unk13 = packet.ReadUInt32("Unk13", j, "Product Data");
                var TransmogSetID = packet.ReadUInt32("TransmogSetID", j, "Product Data");
                var Unk14 = packet.ReadUInt32("Unk14", j, "Product Data");
                var Unk15 = packet.ReadUInt32("Unk15", j, "Product Data");

                var TitleSize = packet.ReadBits("TitleSize", 8, j, "Product Data");
                packet.ReadBit("AlreadyOwned", j, "Product Data");
                var HasPetResultVariable = packet.ReadBit("HasPetResultVariable", j, "Product Data");
                var ItemsSize = packet.ReadBits("ItemsSize", 7, j, "Product Data");
                var HasDisplayInfo = packet.ReadBit("HasDisplayInfo", j, "Product Data");

                uint PetResultVariable = 0;
                if (HasPetResultVariable)
                    PetResultVariable = packet.ReadBits("PetResultVariable", 4, j, "Product Data");

                packet.ResetBitReader();

                // BATTLEPAY ITEM (i don't care about that yet)
                for (uint g = 0; g < ItemsSize; g++)
                {
                    var id = packet.ReadUInt32("Id", g, "Item Data");
                    packet.ReadByte("Unk16", g, "Item Data");
                    var itemid = packet.ReadUInt32("ItemID", g, "Item Data");
                    var quantity = packet.ReadUInt32("Quantity", g, "Item Data");
                    packet.ReadUInt32("Unk17", g, "Item Data");
                    packet.ReadUInt32("Unk18", g, "Item Data");

                    var HasPet = packet.ReadBit("HasPet", g, "Item Data");
                    var PetResult = packet.ReadBit("PetResult", g, "Item Data");
                    var DisplayInfo = packet.ReadBit("DisplayInfos", g, "Item Data");

                    uint PetResultVariable_ = 0;
                    if (PetResult)
                        PetResultVariable_ = packet.ReadBits("PetResultVariable", 4, g, "Item Data");

                    packet.ResetBitReader();

                    if (DisplayInfo)
                        ReadBattlepayDisplayInfo(packet, 1000 + g, 0, 0, 0, true, 0, 0, g);
                }

                var name = packet.ReadWoWString("Name", TitleSize, j, "Product Data");

                uint DisplayFlag = 0;
                var Unk19 = 0;
                if (HasDisplayInfo)
                {
                    DisplayFlag = packet.ReadBits("DisplayFlag", 7, j, "Product Data");
                    ReadBattlepayDisplayInfo(packet, 2000 + j, 0, productid, 0, true, DisplayFlag, j);
                }

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
                var groupid = packet.ReadInt32("GroupID", i, "Group");
                var iconfiledataid = packet.ReadInt32("IconFileDataID", i, "Group");
                var displaytype = packet.ReadByte("DisplayType", i, "Group");
                var ordering = packet.ReadInt32("Ordering", i, "Group");
                var unkt = packet.ReadInt32("Unk20", i, "Group");
                var maingroupid = packet.ReadInt32("MainGroupID", i, "Group");

                packet.ResetBitReader();

                var bits4 = packet.ReadBits("nameLength", 8, i, "Group");
                var bits7 = packet.ReadBits("IsAvailableDescription", 24, i, "Group");
                var name = packet.ReadWoWString("Name", bits4, i, "Group");
                var description = "";
                if (bits7 > 1)
                    description = packet.ReadWoWString("Description", bits7, i, "Group");

                BattlePayGroup Group = new BattlePayGroup
                {
                    GroupID = ((uint)groupid),
                    IconFileDataID = ((uint)iconfiledataid),
                    DisplayType = ((uint)displaytype),
                    Ordering = ((uint)ordering),
                    Unk = ((uint)unkt),
                    MainGroupID = ((uint)maingroupid),
                    Name = name,
                    Description = description,
                };
                Storage.BattlePayGroups.Add(Group, packet.TimeSpan);
            }

            // BATTLEPAY SHOP
            for (uint i = 0; i < ShopSize; i++)
            {
                var entryid = packet.ReadUInt32("ShopEntry", i, "Shop Data");
                var GroupID = packet.ReadUInt32("GroupID", i, "Shop Data");
                var ProductInfoID = packet.ReadUInt32("ProductInfoID", i, "Shop Data");
                var Ordering = packet.ReadInt32("Ordering", i, "Shop Data");
                var VasServiceType = packet.ReadInt32("VasServiceType", i, "Shop Data");
                var StoreDeliveryType = packet.ReadByte("StoreDeliveryType", i, "Shop Data");
                
                var HasBattlePayDisplayInfo = packet.ReadBit("HasBattlePayDisplayInfo", i, "Shop Data");
                var Unk21 = packet.ReadBit("Unk21", i, "Shop Data");

                var Unk22 = 0;
                uint DisplayFlag = 0;
                if (HasBattlePayDisplayInfo)
                {
                    DisplayFlag = packet.ReadBits("DisplayFlag", 8, i, "Shop Data");

                    ReadBattlepayDisplayInfo(packet, 3000 + i, ProductInfoID, 0, entryid, true, DisplayFlag, i);
                }

                packet.ResetBitReader();

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

            var bit5248 = packet.ReadBit("HasBattlePayProduct", index);
            packet.ReadBit("Revoked", index);

            if (bit5248)
            {
                var productid = packet.ReadUInt32("ProductId", index);
                packet.ReadByte("Type", index);
                packet.ReadUInt32("ItemID", index);
                packet.ReadUInt32("Amount", index);
                packet.ReadUInt32("MountSpellID", index);
                packet.ReadUInt32("BattlePetSpeciesCreatureID", index);
                packet.ReadUInt32("Unk1", index);
                packet.ReadUInt32("Unk2", index);
                packet.ReadUInt32("Unk3", index);
                packet.ReadUInt32("TransmogSetID", index);
                packet.ReadUInt32("Unk8", index);
                packet.ReadUInt32("Unk9", index);

                packet.ResetBitReader();

                // unsure about 8 bit read here
                var UnkString = packet.ReadBits("NameSize", 8, index);
                var UnkBit = packet.ReadBit("AlreadyOwned", index);
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
                        ReadBattlepayDisplayInfo(packet, 5000, 0, 0, 0, true, 0, g);

                }
                packet.ReadWoWString("Name", UnkString, index);
                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, 6000, 0, 0, 0, true, 0, index);
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