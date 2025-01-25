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

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{

    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        private static void ReadBattlepayDisplayInfo(Packet packet, uint entry, bool HasUnk1, uint titleSize, uint displayInfoFlag, params object[] index)
        {
            var TitleSize = packet.ReadBits("TitleSize", 10, "DisplayInfo", index);
            var Title2Size = packet.ReadBits("Title2Size", 10, "DisplayInfo", index);
            var DescriptionSize = packet.ReadBits("DescriptionSize", 13, "DisplayInfo", index);
            var Description2Size = packet.ReadBits("Description2Size", 13, "DisplayInfo", index);
            var Description3Size = packet.ReadBits("Description3Size", 13, "DisplayInfo", index);

            var HasIconBorder = packet.ReadBit("HasIconBorder", "DisplayInfo", index);
            var HasUnk2 = packet.ReadBit("HasUnk2", "DisplayInfo", index);
            var HasUiTextureAtlasMemberID = packet.ReadBit("HasUiTextureAtlasMemberID", "DisplayInfo", index);

            var HasUiTextureAtlasMemberID2 = packet.ReadBit("UiTextureAtlasMemberID2", "DisplayInfo", index);

            var Description4Size = packet.ReadBits("Description4Size", 13, "DisplayInfo", index);
            var Description5Size = packet.ReadBits("Description5Size", 12, "DisplayInfo", index);

            packet.ResetBitReader();

            var VisualsSize = packet.ReadInt32("VisualsSize", "DisplayInfo", index);

            var CardType = packet.ReadInt32("CardType", "DisplayInfo", index);

            if (HasUnk1)
                packet.ReadInt32("Unk1", "DisplayInfo", index);

            var ProductMultiplier = 0;
            if (displayInfoFlag != 1)
                ProductMultiplier = packet.ReadInt32("ProductMultiplier", "DisplayInfo", index); // Like 6 Character Transfers

            var IconFileDataID = packet.ReadInt32("IconFileDataID", "DisplayInfo", index);            
            var UIModelSceneID = packet.ReadInt32("UIModelSceneID", "DisplayInfo", index);

            var Title = "Untitled";

            Title = packet.ReadWoWString("Title", TitleSize, "DisplayInfo", index);

            var Title2 = packet.ReadWoWString("Title2", Title2Size, "DisplayInfo", index);
            var Description = packet.ReadWoWString("Description", DescriptionSize, "DisplayInfo", index);
            var Description2 = packet.ReadWoWString("Description2", Description2Size, "DisplayInfo", index);
            var Description3 = packet.ReadWoWString("Description3", Description3Size, "DisplayInfo", index);

            var IconBorder = 0;
            var Unk2 = 0;
            var UiTextureAtlasMemberID = 0;
            var UiTextureAtlasMemberID2 = 0;

            if (HasIconBorder)
                IconBorder = packet.ReadInt32("IconBorder", "DisplayInfo", index);
            if (HasUnk2)
                Unk2 = packet.ReadInt32("Unk2", "DisplayInfo", index);
            if (HasUiTextureAtlasMemberID)
                UiTextureAtlasMemberID = packet.ReadInt32("UiTextureAtlasMemberID", "DisplayInfo", index);
            if (HasUiTextureAtlasMemberID2)
                UiTextureAtlasMemberID2 = packet.ReadInt32("UiTextureAtlasMemberID2", "DisplayInfo", index);

            var Description4 = packet.ReadWoWString("Description4", Description4Size, "DisplayInfo", index);
            var Description5 = packet.ReadWoWString("Description5", Description5Size, "DisplayInfo", index);

            // BATTLEPAY DISPLAYINFO
            foreach (var productTemplate in Storage.BattlePayProductTemplates)
            {
                if (productTemplate.Item1.Entry == entry)
                {
                    productTemplate.Item1.IconFileDataID = IconFileDataID;
                    productTemplate.Item1.UIModelSceneID = UIModelSceneID;
                    productTemplate.Item1.Title = Title;
                    productTemplate.Item1.Title2 = Title2;
                    productTemplate.Item1.Description = Description;
                    productTemplate.Item1.Description2 = Description2;
                    productTemplate.Item1.Description3 = Description3;
                    productTemplate.Item1.DisplayInfoFlag = displayInfoFlag;
                    productTemplate.Item1.CardType = CardType;
                    productTemplate.Item1.IconBorder = IconBorder;
                    productTemplate.Item1.UiTextureAtlasMemberID = UiTextureAtlasMemberID;
                    productTemplate.Item1.Description4 = Description4;
                    productTemplate.Item1.Description5 = Description5;
                    productTemplate.Item1.ProductMultiplier = ProductMultiplier;
                }
            }

            List<string> previewTitles = new List<string>();
            List<string> previewCreatureDisplayIDs = new List<string>();
            List<string> previewUIModelSceneIDs = new List<string>();
            List<string> previewTransmogSets = new List<string>();

            for (int j = 0; j < VisualsSize; j++)
            {
                packet.ResetBitReader();
                var TitleSize_Visual = packet.ReadBits(10);
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

            // BATTLEPAY VISUALS
            foreach (var productTemplate in Storage.BattlePayProductTemplates)
            {
                if (productTemplate.Item1.Entry == entry)
                {
                    productTemplate.Item1.PreviewTitles = previewTitles_String;
                    productTemplate.Item1.PreviewCreatureDisplayIDs = previewCreatureDisplayIDs_String;
                    productTemplate.Item1.PreviewUIModelSceneIDs = previewUIModelSceneIDs_String;
                    productTemplate.Item1.PreviewTransmogSets = previewTransmogSets_String;
                }
            }
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
                var ProductInfoID = packet.ReadUInt32("ProductInfoID", index);
                var NormalPrice = packet.ReadInt64("NormalPrice", index);
                var CurrentPrice = packet.ReadInt64("CurrentPrice", index);
                var ProductIDsSize = packet.ReadInt32("DeliverableProductIDCount", index);
                var ProductFlags = packet.ReadInt32("ProductFlags", index);
                packet.ReadInt32("Unk3", index);
                packet.ReadInt32("Unk4", index);
                packet.ReadInt32("Unk5", index);
                packet.ReadInt32("Unk6", index);
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

                var ChoiceType = packet.ReadBits("ChoiceType", 7, index);

                bool HasUnk1InDisplayInfo = true;
                if (ChoiceType > 0)
                {
                    packet.ReadBit("Unk7", index);
                    HasUnk1InDisplayInfo = packet.ReadBit("Unk8", index);
                }

                BattlePayProductTemplate productTemplate = new BattlePayProductTemplate();
                foreach (uint productID in subProductIDs)
                {
                    productTemplate.Entry = index;
                    productTemplate.ProductID = productID;
                    productTemplate.ProductInfoID = ProductInfoID;
                    productTemplate.NormalPrice = NormalPrice;
                    productTemplate.CurrentPrice = CurrentPrice;
                    productTemplate.Flags = ProductFlags;
                    productTemplate.BundleProductIDs = subproducts_string;
                    productTemplate.ChoiceType = ChoiceType;
                }

                Storage.BattlePayProductTemplates.Add(productTemplate);

                if (ChoiceType > 0)
                {
                    var HasBattlePayDisplayInfo = packet.ReadBit("HasBattlePayDisplayInfo", index);

                    if (HasBattlePayDisplayInfo)
                    {
                        ReadBattlepayDisplayInfo(packet, productTemplate.Entry, HasUnk1InDisplayInfo, 0, 0, index);
                    }
                    else
                    {
                        packet.ReadInt16("Unk9", index);

                        if (ChoiceType != 21 && ChoiceType != 82)
                            packet.ReadInt32("Unk10", index);
                            
                        packet.ReadByte("Unk11", index);
                    }
                }
            }

            // BATTLEPAY PRODUCT
            for (uint j = 0; j < ProductSize; j++)
            {
                var productid = packet.ReadUInt32("ProductId", j);
                var Type = packet.ReadByte("Type", j);
                packet.ReadUInt16("Unk12", j);
                packet.ReadByte("Unk13", j);
                var ItemID = packet.ReadUInt32("ItemID", j);
                var Amount = packet.ReadUInt32("Amount", j);
                var MountSpellID = packet.ReadUInt32("MountSpellID", j);
                var BattlePetSpeciesCreatureID = packet.ReadUInt32("BattlePetSpeciesCreatureID", j);
                packet.ReadUInt32("Unk14", j);
                packet.ReadUInt32("Unk15", j);
                packet.ReadUInt32("Unk16", j);
                var TransmogSetID = packet.ReadUInt32("TransmogSetID", j);
                packet.ReadUInt32("Unk17", j);
                packet.ReadUInt32("Unk18", j);

                packet.ResetBitReader();

                var TitleSize = packet.ReadBits("TitleSize", 8, j);
                var AlreadyOwned = packet.ReadBit("AlreadyOwned", j);
                var HasPetResultVariable = packet.ReadBit("HasPetResultVariable", j);
                var ItemsSize = packet.ReadBits("ItemsSize", 7, j);
                var HasDisplayInfo = packet.ReadBit("HasDisplayInfo", j);

                uint PetResultVariable = 0;
                if (HasPetResultVariable)
                    PetResultVariable = packet.ReadBits("PetResultVariable", 4, j);

                BattlePayProductTemplate _productTemplate = new BattlePayProductTemplate();
                bool found = false;
                foreach (var productTemplate in Storage.BattlePayProductTemplates)
                {
                    if (productTemplate.Item1.ProductID == productid)
                    {
                        found = true;
                        productTemplate.Item1.Type = Type;
                        productTemplate.Item1.ItemID = ItemID;
                        productTemplate.Item1.ItemCount = Amount;
                        productTemplate.Item1.MountSpellID = MountSpellID;
                        productTemplate.Item1.BattlePetSpeciesCreatureID = BattlePetSpeciesCreatureID;
                        productTemplate.Item1.TransmogSetID = TransmogSetID;
                        productTemplate.Item1.AlreadyOwned = AlreadyOwned;
                        _productTemplate = productTemplate.Item1;
                    }
                }

                if (!found)
                {
                    _productTemplate.Entry = Storage.BattlePayProductTemplates.Last().Item1.Entry + 1;
                    _productTemplate.ProductID = productid;
                    _productTemplate.Type = Type;
                    _productTemplate.ItemID = ItemID;
                    _productTemplate.ItemCount = Amount;
                    _productTemplate.MountSpellID = MountSpellID;
                    _productTemplate.BattlePetSpeciesCreatureID = BattlePetSpeciesCreatureID;
                    _productTemplate.TransmogSetID = TransmogSetID;
                    _productTemplate.AlreadyOwned = AlreadyOwned;

                    Storage.BattlePayProductTemplates.Add(_productTemplate);
                }

                // BATTLEPAY ITEM (i don't care about that yet)
                for (uint g = 0; g < ItemsSize; g++)
                {
                    var id = packet.ReadUInt32("Id", g);
                    packet.ReadByte("Unk19", g);
                    var itemid = packet.ReadUInt32("ItemID", g);
                    var quantity = packet.ReadUInt32("Quantity", g);
                    packet.ReadUInt32("Unk20", g);
                    packet.ReadUInt32("Unk21", g);

                    packet.ResetBitReader();

                    var HasPet = packet.ReadBit("HasPet", g);
                    var PetResult = packet.ReadBit("PetResult", g);
                    var DisplayInfo = packet.ReadBit("DisplayInfos", g);

                    uint PetResultVariable_ = 0;
                    if (PetResult)
                        PetResultVariable_ = packet.ReadBits("PetResultVariable", 4, g);

                    if (DisplayInfo)
                        ReadBattlepayDisplayInfo(packet, _productTemplate.Entry, true, 0, 0, g);
                }

                var name = packet.ReadWoWString("Name", TitleSize, j);

                if (HasDisplayInfo)
                {
                    packet.ReadBit("Unk22", j);
                    var DisplayInfoFlag = packet.ReadBits("DisplayInfoFlag", 7, j);
                    ReadBattlepayDisplayInfo(packet, _productTemplate.Entry, HasDisplayInfo, TitleSize, DisplayInfoFlag, j);
                }
            }

            // BATTLE PAY GROUP
            for (int i = 0; i < ProductGroupSize; i++)
            {
                var groupid = packet.ReadInt32("GroupID", i);
                var iconfiledataid = packet.ReadInt32("IconFileDataID", i);
                var displaytype = packet.ReadByte("DisplayType", i);
                var ordering = packet.ReadInt32("Ordering", i);
                var unkt = packet.ReadInt32("Unk23", i);
                var maingroupid = packet.ReadInt32("MainGroupID", i);

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
                    MainGroupID = ((uint)maingroupid),
                    Name = name,
                    Description = description,
                };
                Storage.BattlePayGroups.Add(Group, packet.TimeSpan);
            }

            // BATTLEPAY SHOP
            for (uint i = 0; i < ShopSize; i++)
            {
                var entryid = packet.ReadUInt32("EntryID", i);
                var GroupID = packet.ReadUInt32("GroupID", i);
                var ProductInfoID = packet.ReadUInt32("ProductInfoID", i);
                var Ordering = packet.ReadInt32("Ordering", i);
                var VasServiceType = packet.ReadInt32("VasServiceType", i);
                var StoreDeliveryType = packet.ReadByte("StoreDeliveryType", i);

                BattlePayProductTemplate _productTemplate = new BattlePayProductTemplate();
                bool found = false;
                foreach (var productTemplate in Storage.BattlePayProductTemplates)
                {
                    if (productTemplate.Item1.ProductInfoID == ProductInfoID)
                    {
                        found = true;
                        productTemplate.Item1.GroupID = GroupID;
                        productTemplate.Item1.Ordering = Ordering;
                        productTemplate.Item1.VasServiceType = VasServiceType;
                        productTemplate.Item1.StoreDeliveryType = StoreDeliveryType;
                        _productTemplate = productTemplate.Item1;
                    }
                }

                if (!found)
                {
                    _productTemplate.Entry = Storage.BattlePayProductTemplates.Last().Item1.Entry + 1;
                    _productTemplate.ProductID = entryid;
                    _productTemplate.ProductInfoID = ProductInfoID;
                    _productTemplate.GroupID = GroupID;
                    _productTemplate.Ordering = Ordering;
                    _productTemplate.VasServiceType = VasServiceType;
                    _productTemplate.StoreDeliveryType = StoreDeliveryType;

                    Storage.BattlePayProductTemplates.Add(_productTemplate);
                }

                packet.ResetBitReader();
                
                var HasBattlePayDisplayInfo = packet.ReadBit("HasBattlePayDisplayInfo", i);
                packet.ReadBit("Unk24", i);

                if (HasBattlePayDisplayInfo)
                {
                    packet.ReadBit("Unk25", i);
                    var DisplayInfoFlag = packet.ReadBits("DisplayInfoFlag", 7, i);

                    ReadBattlepayDisplayInfo(packet, _productTemplate.Entry, true, 0, DisplayInfoFlag, i);
                }
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
                        ReadBattlepayDisplayInfo(packet, 3000, true, 0, 0, g);

                }
                packet.ReadWoWString("Name", UnkString, index);
                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, 4000, true, 0, 0, index);
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