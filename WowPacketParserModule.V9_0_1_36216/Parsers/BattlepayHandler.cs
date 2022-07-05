using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V9_0_1_36216.Parsers
{
    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        private static void ReadBattlepayDisplayInfo(Packet packet, params object[] idx)
        {
            packet.ResetBitReader();
            var bit4 = packet.ReadBit("HasCreatureDisplayInfoID", idx);
            var bit12 = packet.ReadBit("HasFileDataID", idx);

            var bits16 = packet.ReadBits(10);
            var bits529 = packet.ReadBits(10);
            var bits1042 = packet.ReadBits(13);
            var bits5139 = packet.ReadBits(13);
            var bits9236 = packet.ReadBits(13);

            var bit9240 = packet.ReadBit("flags", idx);
            var bit9248 = packet.ReadBit("unkt2", idx);
            var bit9264 = packet.ReadBit("unkt3", idx);
            var bit9256 = packet.ReadBit("unkt4", idx);

            var bits13396 = packet.ReadBits(13);
            var bits17493 = packet.ReadBits(13);

            var bit9272 = packet.ReadInt32("VisualsSize", idx);

            var bit13392 = packet.ReadInt32("unktint1", idx);
            var bit21496 = packet.ReadInt32("unktint2", idx);
            var bit21500 = packet.ReadInt32("unktint3", idx);

            var creaturedisplayinfoid = 0;
            var filedataid = 0;

            if (bit4)
                creaturedisplayinfoid = packet.ReadInt32("CreatureDisplayInfoID", idx);
            if (bit12)
                filedataid = packet.ReadInt32("FileDataID", idx);

            var name1 = packet.ReadWoWString("Name1", bits16, idx);
            var name2 = packet.ReadWoWString("Name2", bits529, idx);
            var name3 = packet.ReadWoWString("Name3", bits1042, idx);
            var name4 = packet.ReadWoWString("Name4", bits5139, idx);
            var name5 = packet.ReadWoWString("Name5", bits9236, idx);

            var flags = 0;
            var Unkt2Id = 0;
            var Unkt4Id = 0;
            var Unkt3Id = 0;

            if (bit9240)
                flags = packet.ReadInt32("flags", idx);
            if (bit9248)
                Unkt2Id = packet.ReadInt32("Unkt2Id", idx);
            if (bit9256)
                Unkt4Id = packet.ReadInt32("Unkt4Id", idx);
            if (bit9264)
                Unkt3Id = packet.ReadInt32("Unkt3Id", idx);

            var name6 = packet.ReadWoWString("Name6", bits13396, idx);
            var name7 = packet.ReadWoWString("Name7", bits17493, idx);

            // BATTLEPAY DISPLAYINFO
            BattlePayDisplayInfo DisplayInfo = new BattlePayDisplayInfo
            {
                HasCreatureDisplayInfoID = ((uint)bit4),
                HasFileDataID = ((uint)bit12),
                bits16 = ((uint)bits16),
                bits529 = ((uint)bits529),
                bits5139 = ((uint)bits5139),
                bits9236 = ((uint)bits9236),
                bits13396 = ((uint)bits13396),
                bits17493 = ((uint)bits17493),
                VisualsSize = ((uint)bit9272),
                unktint1 = ((uint)bit13392),
                unktint2 = ((uint)bit21496),
                unktint3 = ((uint)bit21500),
                CreatureDisplayInfoID = ((uint)creaturedisplayinfoid),
                FileDataID = ((uint)filedataid),
                Name1 = name1,
                Name2 = name2,
                Name3 = name3,
                Name4 = name4,
                Name5 = name5,
                Flags = ((uint)flags),
                Unkt2Id = ((uint)Unkt2Id),
                Unkt4Id = ((uint)Unkt4Id),
                Unkt3Id = ((uint)Unkt3Id),
                Name6 = name6,
                Name7 = name7,
            };
            Storage.BattlePayDisplayInfos.Add(DisplayInfo, packet.TimeSpan);

            // BATTLEPAY VISUALS
            for (int j = 0; j < bit9272; j++)
            {
                packet.ResetBitReader();
                var ProductName = packet.ReadBits(10);
                var visual1 = packet.ReadInt32("Visual", idx, j);
                var visual2 = packet.ReadInt32("Visual", idx, j);
                var visual3 = packet.ReadInt32("unktvisual", idx, j);
                var productname = packet.ReadWoWString("ProductName", ProductName, idx, j);

                BattlePayVisual Visual = new BattlePayVisual
                {
                    Visual1 = ((uint)visual1),
                    Visual2 = ((uint)visual2),
                    Visual3 = ((uint)visual3),
                    ProductName = productname,
                };
                Storage.BattlePayVisuals.Add(Visual, packet.TimeSpan);
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
                var productid = packet.ReadInt32("ProductID", index);
                var normalprice = packet.ReadInt64("NormalPriceFixedPoint", index);
                var currentprice = packet.ReadInt64("CurrentPriceFixedPoint", index);
                var ProductIDsSize = packet.ReadInt32("ProductIDsSize", index);
                var UnkInt2 = packet.ReadInt32("UnkInt2", index);
                var UnkInt3 = packet.ReadInt32("UnkInt3", index);
                var UnkIntsSize = packet.ReadInt32("UnkIntsSize", index);

                var UnkInt4 = packet.ReadInt32("UnkInt4", index);

                // Biggest ProductIDsSize is 10 so need new table to link them
                // BATTLEPAY PRODUCTLINKS
                for (int j = 0; j < ProductIDsSize; j++)
                {
                    var productid_ = packet.ReadInt32("ProductIDs", index, j);

                    BattlePayProductLink ProductLink = new BattlePayProductLink
                    {
                        LinkID = index,
                        ProductID = ((uint)productid_),
                    };
                    Storage.BattlePayProductLinks.Add(ProductLink, packet.TimeSpan);
                }

                var UnkInts = 0;
                //Biggest UnkIntsSize is 1 so no need new table
                for (int j = 0; j < UnkIntsSize; j++)
                {
                    UnkInts = packet.ReadInt32("UnkInts", index, j);
                }

                packet.ResetBitReader();

                var choicetype = packet.ReadBits("ChoiceType", 7, index);

                var HasBattlepayDisplayInfo = packet.ReadBit("HasBattlepayDisplayInfo", index);

                if (HasBattlepayDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, index);

                BattlePayProductInfo ProductInfo = new BattlePayProductInfo
                {
                    Entry = index,
                    ProductID = ((uint)productid),
                    NormalPriceFixedPoint = ((uint)normalprice),
                    CurrentPriceFixedPoint = ((uint)currentprice),
                    ProductIDsSize = ((uint)ProductIDsSize),
                    UnkInt2 = ((uint)UnkInt2),
                    UnkInt3 = ((uint)UnkInt3),
                    UnkIntsSize = ((uint)UnkIntsSize),
                    UnkInt4 = ((uint)UnkInt4),
                    UnkInts = ((uint)UnkInts),
                    ChoiceType = ((uint)choicetype),
                    HasBattlepayDisplayInfo = ((uint)HasBattlepayDisplayInfo),
                };
                Storage.BattlePayProductInfos.Add(ProductInfo, packet.TimeSpan);

            }

            // BATTLEPAY PRODUCT
            for (uint j = 0; j < ProductSize; j++)
            {
                var productid = packet.ReadUInt32("ProductId", j);
                var type = packet.ReadByte("Type", j);
                var flags = packet.ReadUInt32("Flags", j);
                var UnkInt1 = packet.ReadUInt32("UnkInt1", j);
                var displayid = packet.ReadUInt32("DisplayId", j);
                var ItemId = packet.ReadUInt32("ItemId", j);
                var UnkInt4 = packet.ReadUInt32("UnkInt4", j);
                var UnkInt5 = packet.ReadUInt32("UnkInt5", j);

                var UnkInt6 = packet.ReadUInt32("UnkInt6", j);
                var UnkInt7 = packet.ReadUInt32("UnkInt7", j);
                var UnkInt8 = packet.ReadUInt32("UnkInt8", j);
                var UnkInt9 = packet.ReadUInt32("UnkInt9", j);

                packet.ResetBitReader();

                var UnkString = packet.ReadBits("UnkString", 8, j);
                var UnkBit = packet.ReadBit("UnkBit", j);
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
                        ReadBattlepayDisplayInfo(packet, g);

                    BattlePayItem Item = new BattlePayItem
                    {
                        Entry = j,
                        ID = ((uint)id),
                        UnkByte = ((uint)unkbyte),
                        ItemID = ((uint)itemid),
                        Quantity = ((uint)quantity),
                        UnkInt1 = ((uint)UnkInt1_),
                        UnkInt2 = ((uint)UnkInt2_),
                        HasPet = ((uint)HasPet),
                        PetResult = ((uint)PetResult),
                        DisplayInfo = ((uint)DisplayInfo),
                        PetResultVariable = ((uint)PetResultVariable_),
                    };
                    Storage.BattlePayItems.Add(Item, packet.TimeSpan);
                }
                var name = packet.ReadWoWString("Name", UnkString, j);
                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, j);


                BattlePayProduct Product = new BattlePayProduct
                {
                    Entry = j,
                    ProductID = ((uint)productid),
                    Type = ((uint)type),
                    Flags = ((uint)flags),
                    UnkInt1 = ((uint)UnkInt1),
                    DisplayID = ((uint)displayid),
                    ItemID = ((uint)ItemId),
                    UnkInt4 = ((uint)UnkInt4),
                    UnkInt5 = ((uint)UnkInt5),
                    UnkInt6 = ((uint)UnkInt6),
                    UnkInt7 = ((uint)UnkInt7),
                    UnkInt8 = ((uint)UnkInt8),
                    UnkInt9 = ((uint)UnkInt9),
                    NameSize = ((uint)UnkString),
                    UnkBit = ((uint)UnkBit),
                    UnkBits = ((uint)UnkBits),
                    ItemsSize = ((uint)ItemsSize),
                    HasDisplayInfo = ((uint)HasDisplayInfo),
                    PetResultVariable = ((uint)PetResultVariable),
                    Name = name,
                };
                Storage.BattlePayProducts.Add(Product, packet.TimeSpan);
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
                    Entry = ((uint)i),
                    GroupID = ((uint)groupid),
                    IconFileDataID = ((uint)iconfiledataid),
                    DisplayType = ((uint)displaytype),
                    Ordering = ((uint)ordering),
                    Unkt = ((uint)unkt),
                    NameLength = ((uint)bits4),
                    IsAvailableDescription = ((uint)bits7),
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
                var productid = packet.ReadUInt32("ProductID", i);
                var ordering = packet.ReadInt32("Ordering", i);
                var vasservicetype = packet.ReadInt32("VasServiceType", i);
                var storedeliverytype = packet.ReadByte("StoreDeliveryType", i);

                packet.ResetBitReader();

                var bit5172 = packet.ReadBit("HasBattlepayDisplayInfo", i);
                if (bit5172)
                    ReadBattlepayDisplayInfo(packet, i);

                BattlePayShop Shop = new BattlePayShop
                {
                    Entry = i,
                    ID = ((uint)entryid),
                    GroupID = ((uint)groupid),
                    ProductID = ((uint)productid),
                    Ordering = ((uint)ordering),
                    VasServiceType = ((uint)vasservicetype),
                    StoreDeliveryType = ((uint)storedeliverytype),
                    HasBattlepayDisplayInfo = ((uint)bit5172),
                };
                Storage.BattlePayShops.Add(Shop, packet.TimeSpan);

            }

        }

        private static void ReadBattlePayDistributionObject(Packet packet, params object[] index)
        {
            packet.ReadInt64("DistributionID", index);

            packet.ReadInt32("Status", index);
            packet.ReadInt32("ProductID", index);

            packet.ReadPackedGuid128("TargetPlayer", index);
            packet.ReadInt32("TargetVirtualRealm", index);
            packet.ReadInt32("TargetNativeRealm", index);

            packet.ReadInt64("PurchaseID", index);

            packet.ResetBitReader();

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
                        ReadBattlepayDisplayInfo(packet, g);

                }
                packet.ReadWoWString("Name", UnkString, index);
                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, index);
            }
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_DISTRIBUTION_LIST_RESPONSE)]
        public static void HandleBattlePayGetDistributionListResponse(Packet packet)
        {
            packet.ReadUInt32("Result");

            packet.ResetBitReader();
            var int6 = packet.ReadBits("BattlePayDistributionObjectCount", 11);
            for (uint index = 0; index < int6; index++)
                ReadBattlePayDistributionObject(packet, index);
        }

        [Parser(Opcode.SMSG_BATTLE_PAY_DISTRIBUTION_UPDATE)]
        public static void HandleBattlePayDistributionUpdate(Packet packet)
        {
            ReadBattlePayDistributionObject(packet);
        }


    }
}