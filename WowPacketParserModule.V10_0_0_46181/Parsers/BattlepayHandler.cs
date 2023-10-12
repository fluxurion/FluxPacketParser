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

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{

    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        private static void ReadBattlepayDisplayInfo(Packet packet, uint templateEntry, params object[] index)
        {
            packet.ResetBitReader();
            var HasIconFileDataID = packet.ReadBit("HasIconFileDataID", index);
            var HasUIModelSceneID = packet.ReadBit("HasUIModelSceneID", index);

            // check if wrong todo
            var TitleSize = packet.ReadBits(10); // 6+4 = 10 good
            var Title2Size = packet.ReadBits(10); // might be 8 bits here
            var DescriptionSize = packet.ReadBits(13); //
            var Description2Size = packet.ReadBits(13);
            var Description3Size = packet.ReadBits(13);

            var _IconBorder = packet.ReadBit("IconBorder", index);
            var _Unk1 = packet.ReadBit("Unk1", index);
            var _Unk2 = packet.ReadBit("Unk2", index);
            var _UiTextureAtlasMemberID = packet.ReadBit("UiTextureAtlasMemberID", index);

            var Description4Size = packet.ReadBits(13);
            var Description5Size = packet.ReadBits(13);

            var VisualsSize = packet.ReadInt32("VisualsSize", index);

            var CardType = packet.ReadInt32("CardType", index);
            var Unk4 = packet.ReadInt32("Unk4", index);
            var Unk5 = packet.ReadInt32("Unk5", index);

            var IconFileDataID = 0;
            var UIModelSceneID = 0;

            if (HasIconFileDataID)
                IconFileDataID = packet.ReadInt32("IconFileDataID", index);
            if (HasUIModelSceneID)
                UIModelSceneID = packet.ReadInt32("UIModelSceneID", index);

            var Title = packet.ReadWoWString("Title", TitleSize, index);
            var Title2 = packet.ReadWoWString("Title2", Title2Size, index);
            var Description = packet.ReadWoWString("Description", DescriptionSize, index);
            var Description2 = packet.ReadWoWString("Description2", Description2Size, index);
            var Description3 = packet.ReadWoWString("Description3", Description3Size, index);

            var IconBorder = 0;
            var Unk1 = 0;
            var Unk2 = 0;
            var UiTextureAtlasMemberID = 0;

            if (_IconBorder)
                IconBorder = packet.ReadInt32("IconBorder", index);
            if (_Unk1)
                Unk1 = packet.ReadInt32("Unk1", index);
            if (_Unk2)
                Unk2 = packet.ReadInt32("Unk2", index);
            if (_UiTextureAtlasMemberID)
                UiTextureAtlasMemberID = packet.ReadInt32("UiTextureAtlasMemberID", index);

            var Description4 = packet.ReadWoWString("Description4", Description4Size, index);
            var Description5 = packet.ReadWoWString("Description5", Description5Size, index);

            // BATTLEPAY DISPLAYINFO
            foreach (var productTemplate in Storage.BattlePayProductTemplates)
            {
                if (productTemplate.Item1.Entry == templateEntry)
                {
                    productTemplate.Item1.IconFileDataID = IconFileDataID;
                    productTemplate.Item1.UIModelSceneID = UIModelSceneID;
                    productTemplate.Item1.Title = Title;
                    productTemplate.Item1.Title2 = Title2;
                    productTemplate.Item1.Description = Description;
                    productTemplate.Item1.Description2 = Description2;
                    productTemplate.Item1.Description3 = Description3;
                    productTemplate.Item1.CardType = CardType;
                    productTemplate.Item1.IconBorder = IconBorder;
                    productTemplate.Item1.UiTextureAtlasMemberID = UiTextureAtlasMemberID;
                    productTemplate.Item1.Description4 = Description4;
                    productTemplate.Item1.Description5 = Description5;
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
                var CreatureDisplayID = packet.ReadInt32("CreatureDisplayID", j, j);
                var PreviewUIModelSceneID = packet.ReadInt32("PreviewUIModelSceneID",j,j);
				var TransmogSetID = packet.ReadInt32("TransmogSetID", j, j);
                var Title_Visual = packet.ReadWoWString("Title_Visual", TitleSize_Visual, j, j);

                previewTitles.Add("'" + Title_Visual + "'");
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
                if (productTemplate.Item1.Entry == templateEntry)
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
                var ProductInfoID = packet.ReadInt32("ProductInfoID", index);
                var NormalPrice = packet.ReadInt64("NormalPrice", index);
                var CurrentPrice = packet.ReadInt64("CurrentPrice", index);
                var ProductIDsSize = packet.ReadInt32("BundleProductIDs", index);
                var UnkInt2 = packet.ReadInt32("Unk1", index);
                var UnkInt3 = packet.ReadInt32("Unk2", index);
                var UnkIntsSize = packet.ReadInt32("UnkIntsSize", index);

                var Flags = packet.ReadInt32("Flags", index);
                List<uint> subProductIDs = new List<uint>();
                // Biggest ProductIDsSize is 10 but instead of new table which would be hard to follow, i add them to a text blob
                List<string> subproducts = new List<string>();
                for (int j = 0; j < ProductIDsSize; j++)
                {
                    var subproductid = packet.ReadInt32("BundleProductIDs", index, j);

                    subProductIDs.Add((uint)subproductid);

                    subproducts.Add(subproductid.ToString());
                }
                string subproducts_string = string.Join(",", subproducts);

                var UnkInts = 0;
                //Biggest UnkIntsSize is 1 so no need new table
                for (int j = 0; j < UnkIntsSize; j++)
                {
                    UnkInts = packet.ReadInt32("UnkInts", index, j);
                }

                packet.ResetBitReader();

                var ChoiceType = packet.ReadBits("ChoiceType", 7, index);

                var HasBattlepayDisplayInfo = packet.ReadBit("HasBattlepayDisplayInfo", index);

                BattlePayProductTemplate productTemplate = new BattlePayProductTemplate();
                foreach (uint productID in subProductIDs)
                {
                    productTemplate.Entry = productID;
                    productTemplate.ProductInfoID = ProductInfoID;
                    productTemplate.NormalPrice = NormalPrice;
                    productTemplate.CurrentPrice = CurrentPrice;
                    productTemplate.Flags = Flags;
                    productTemplate.BundleProductIDs = subproducts_string;
                    productTemplate.ChoiceType = ChoiceType;
                }

                bool alreadyThere = false;
                foreach (var _productTemplate in Storage.BattlePayProductTemplates)
                    if (_productTemplate.Item1.Entry == productTemplate.Entry)
                        alreadyThere = true;

                // If the product is already there cuz it can be there more times! we add it with entry 5000+
                if (alreadyThere)
                    productTemplate.Entry += 5000 + index; 

                    Storage.BattlePayProductTemplates.Add(productTemplate);

                if (HasBattlepayDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, productTemplate.Entry, index);
            }

            // BATTLEPAY PRODUCT
            for (uint j = 0; j < ProductSize; j++)
            {
                var productid = packet.ReadUInt32("ProductId", j);
                var Type = packet.ReadByte("Type", j);
                var ItemID = packet.ReadUInt32("ItemID", j);
                var ItemCount = packet.ReadUInt32("ItemCount", j);
                var MountSpellID = packet.ReadUInt32("MountSpellID", j);
                var BattlePetSpeciesCreatureID = packet.ReadUInt32("BattlePetSpeciesCreatureID", j);
                var UnkInt4 = packet.ReadUInt32("Unk1", j);
                var UnkInt5 = packet.ReadUInt32("Unk2", j);
                var UnkInt6 = packet.ReadUInt32("Unk3", j);
                var TransmogSetID = packet.ReadUInt32("TransmogSetID", j);
                var UnkInt8 = packet.ReadUInt32("Unk8", j);
                var UnkInt9 = packet.ReadUInt32("Unk9", j);

                packet.ResetBitReader();

                var UnkString = packet.ReadBits("UnkString", 8, j);
                var AlreadyOwned = packet.ReadBit("AlreadyOwned", j);
                var UnkBits = packet.ReadBit("UnkBits", j);
                var ItemsSize = packet.ReadBits("ItemsSize", 7, j);
                var HasDisplayInfo = packet.ReadBit("HasDisplayInfo", j);

                uint PetResultVariable = 0;
                if (UnkBits)
                    PetResultVariable = packet.ReadBits("PetResultVariable", 4, j);

                BattlePayProductTemplate _productTemplate = new BattlePayProductTemplate();
                foreach (var productTemplate in Storage.BattlePayProductTemplates)
                {
                    if (productTemplate.Item1.Entry == productid)
                    {
                        _productTemplate = productTemplate.Item1;
                        productTemplate.Item1.Type = Type;
                        productTemplate.Item1.ItemID = ItemID;
                        productTemplate.Item1.ItemCount = ItemCount;
                        productTemplate.Item1.MountSpellID = MountSpellID;
                        productTemplate.Item1.BattlePetSpeciesCreatureID = BattlePetSpeciesCreatureID;
                        productTemplate.Item1.TransmogSetID = TransmogSetID;
                        productTemplate.Item1.AlreadyOwned = AlreadyOwned;
                    }
                }

                // BATTLEPAY ITEM (i don't care about that yet)
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
                        ReadBattlepayDisplayInfo(packet, _productTemplate.Entry, g);
                }

                var name = packet.ReadWoWString("Name", UnkString, j);

                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, _productTemplate.Entry, j);
            }

            // BATTLE PAY GROUP
            for (int i = 0; i < ProductGroupSize; i++)
            {
                var groupid = packet.ReadInt32("GroupID", i);
                var iconfiledataid = packet.ReadInt32("IconFileDataID", i);
                var displaytype = packet.ReadByte("DisplayType", i);
                var ordering = packet.ReadInt32("Ordering", i);
                var unkt = packet.ReadInt32("unkt", i);
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
                foreach (var productTemplate in Storage.BattlePayProductTemplates)
                {
                    if (productTemplate.Item1.ProductInfoID == ProductInfoID)
                    {
                        _productTemplate = productTemplate.Item1;
                        productTemplate.Item1.GroupID = GroupID;
                        productTemplate.Item1.Ordering = Ordering;
                        productTemplate.Item1.VasServiceType = VasServiceType;
                        productTemplate.Item1.StoreDeliveryType = StoreDeliveryType;
                    }
                }

                packet.ResetBitReader();

                var bit5172 = packet.ReadBit("HasBattlepayDisplayInfo", i);
                if (bit5172)
                    ReadBattlepayDisplayInfo(packet, _productTemplate.Entry, i);
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