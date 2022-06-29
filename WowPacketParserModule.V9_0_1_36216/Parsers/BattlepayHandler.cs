using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

/// <summary>
/// struc of battlepay by NORDRASSIL WOW
/// </summary>

namespace WowPacketParserModule.V9_0_1_36216.Parsers
{
    public static class BattlePayHandler
    {
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PRODUCT_LIST)]
        [Parser(Opcode.CMSG_BATTLE_PAY_GET_PURCHASE_LIST)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }
        //This struct is ok
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

            if (bit9240)
                packet.ReadInt32("flags", idx);
            if (bit9248)
                packet.ReadInt32("Unkt2Id", idx);
            if (bit9256)
                packet.ReadInt32("Unkt4Id", idx);
            if (bit9264)
                packet.ReadInt32("Unkt3Id", idx);

            packet.ReadWoWString("Name6", bits13396, idx);
            packet.ReadWoWString("Name7", bits17493, idx);

            for (int j = 0; j < bit9272; j++)
            {
                packet.ResetBitReader();
                var ProductName = packet.ReadBits(10);
                var displayid = packet.ReadInt32("Visual", idx, j);
                var visualid = packet.ReadInt32("Visual", idx, j);
                packet.ReadInt32("unktvisual", idx, j);
                var productname = packet.ReadWoWString("ProductName", ProductName, idx, j);

                BattlePayDisplayInfoVisual DisplayInfoVisual = new BattlePayDisplayInfoVisual
                {
                    DisplayInfoId = ((uint)packet.Position),
                    DisplayId = ((uint)displayid),
                    VisualId = ((uint)visualid),
                    ProductName = productname
                };
                Storage.BattlePayDisplayInfoVisuals.Add(DisplayInfoVisual, packet.TimeSpan);
            }

            BattlePayDisplayInfo DisplayInfo = new BattlePayDisplayInfo
            {
                DisplayInfoId = ((uint)packet.Position),
                CreatureDisplayInfoID = ((uint)creaturedisplayinfoid),
                FileDataID = ((uint)filedataid),
                DisplayCardWidth = ((uint)bit13392),
                Name1 = name1,
                Name2 = name2,
                Name3 = name3,
                Name4 = name4,
                Name5 = name5,
                Flags = bit9240
            };
            Storage.BattlePayDisplayInfos.Add(DisplayInfo, packet.TimeSpan);
        }
        //This Struct is OK

        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE)]
        public static void HandletBattlePayGetProductListResponse(Packet packet)
        {
            uint _productid_ = 0;
            uint _normalprice_ = 0;
            uint _currentprice_ = 0;
            int _type_ = 0;
            uint _choicetype_ = 0;
            uint _flags_ = 0;
            uint _displayid_ = 0;

            packet.ReadUInt32("Result");
            var currencyid = packet.ReadUInt32("CurrencyID");

            var ProductInfoSize = packet.ReadUInt32("ProductInfoSize");
            var int36 = packet.ReadUInt32("ProductSize");
            var int20 = packet.ReadUInt32("ProductGroupSize");
            var int26 = packet.ReadUInt32("ShopSize");

            for (uint index = 0; index < ProductInfoSize; index++)
            {
                var productid = packet.ReadInt32("ProductID", index);
                var normalprice = packet.ReadInt64("NormalPriceFixedPoint", index);
                var currentprice = packet.ReadInt64("CurrentPriceFixedPoint", index);
                var ProductIDsSize = packet.ReadInt32("ProductIDsSize", index);
                packet.ReadInt32("UnkInt2", index);
                packet.ReadInt32("UnkInt3", index);
                var UnkIntsSize = packet.ReadInt32("UnkIntsSize", index);

                packet.ReadInt32("UnkInt4", index);

                for (int j = 0; j < ProductIDsSize; j++)
                {
                    packet.ReadInt32("ProductIDs", index, j);
                }
                for (int j = 0; j < UnkIntsSize; j++)
                {
                    packet.ReadInt32("UnkInts", index, j);
                }

                packet.ResetBitReader();

                var choicetype = packet.ReadBits("ChoiceType", 7, index);

                var HasBattlepayDisplayInfo = packet.ReadBit("HasBattlepayDisplayInfo", index);

                if (HasBattlepayDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, index);

                for (uint p = 0; p < int36; p++)
                {
                    if (p == index)
                    {
                        _normalprice_ = ((uint)normalprice);
                        _currentprice_ = ((uint)currentprice);
                        _choicetype_ = choicetype;
                    }
                }

            }

            //////////////////////////////////////////////////////////////

            for (uint j = 0; j < int36; j++)
            {
                var _productid = packet.ReadUInt32("ProductId", j);
                _productid_ = _productid;
                var type = packet.ReadByte("Type", j);
                _type_ = type;
                var flags = packet.ReadUInt32("Flags", j);
                _flags_ = flags;
                packet.ReadUInt32("UnkInt1", j);
                var displayid = packet.ReadUInt32("DisplayId", j);
                _displayid_ = displayid;
                packet.ReadUInt32("ItemId", j);
                packet.ReadUInt32("UnkInt4", j);
                packet.ReadUInt32("UnkInt5", j);

                packet.ReadUInt32("UnkInt6", j);
                packet.ReadUInt32("UnkInt7", j);
                packet.ReadUInt32("UnkInt8", j);
                packet.ReadUInt32("UnkInt9", j);

                packet.ResetBitReader();

                var UnkString = packet.ReadBits("UnkString", 8, j);
                var UnkBit = packet.ReadBit("UnkBit", j);
                var UnkBits = packet.ReadBit("UnkBits", j);
                var ItemsSize = packet.ReadBits("ItemsSize", 7, j);
                var HasDisplayInfo = packet.ReadBit("HasDisplayInfo", j);//OkHere

                if (UnkBits)
                    packet.ReadBits("PetResultVariable", 4, j);

                for (uint index = 0; index < int36; index++)
                {
                    BattlePayProduct Product = new BattlePayProduct
                    {
                        ProductID = ((uint)_productid_),
                        NormalPriceFixedPoint = ((uint)_normalprice_),
                        CurrentPriceFixedPoint = ((uint)_currentprice_),
                        Type = ((uint)_type_),
                        WebsiteType = 0,
                        CustomValue = 0,
                        ChoiceType = ((uint)_choicetype_),
                        CurrencyID = currencyid,
                        Flags = ((uint)_flags_),
                        DisplayInfoID = ((uint)_displayid_),
                        SpellID = 0,
                        CreatureID = 0,
                        ClassMask = 0,
                        ScriptName = ""
                    };
                    Storage.BattlePayProducts.Add(Product, packet.TimeSpan);
                }

                // battlepay product item
                for (uint g = 0; g < ItemsSize; g++)
                {
                    var id = packet.ReadUInt32("Id", g);
                    packet.ReadByte("UnkByte", g);
                    var itemid = packet.ReadUInt32("ItemID", g);
                    var quantity = packet.ReadUInt32("Quantity", g);
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

                    BattlePayProductItem ProductItem = new BattlePayProductItem
                    {
                        ID = id,
                        ProductID = _productid,
                        ItemID = itemid,
                        Quantity = quantity,
                        DisplayID = ((uint)DisplayInfo),
                        PetResult = ((uint)PetResult),
                    };
                    Storage.BattlePayProductItems.Add(ProductItem, packet.TimeSpan);
                }
                packet.ReadWoWString("Name", UnkString, j);
                if (HasDisplayInfo)
                    ReadBattlepayDisplayInfo(packet, j);
            }


            // Todo OK
            ///////////////////////////////////////////////
            for (int i = 0; i < int20; i++)
            {
                var _groupid = packet.ReadInt32("GroupID", i);
                var iconfiledataid = packet.ReadInt32("IconFileDataID", i);
                var displaytype = packet.ReadByte("DisplayType", i);
                var _ordering = packet.ReadInt32("Ordering", i);
                packet.ReadInt32("unkt", i);

                packet.ResetBitReader();

                var bits4 = packet.ReadBits("nameLength", 8, i);
                var bits7 = packet.ReadBits("IsAvailableDescription", 24, i);
                var name = packet.ReadWoWString("Name", bits4, i);
                if (bits7 > 1)
                    packet.ReadWoWString("Description", bits7, i);


                BattlePayProductGroup ProductGroup = new BattlePayProductGroup
                {
                    GroupID = ((uint)_groupid),
                    IconFileDataID = ((uint)iconfiledataid),
                    DisplayType = displaytype,
                    Ordering = ((uint)_ordering),
                    Name = name
                };
                Storage.BattlePayProductGroups.Add(ProductGroup, packet.TimeSpan);
            }
            //Ok
            ///////////////////////////////////////////////////////////

            // battlepay shop entry
            for (uint i = 0; i < int26; i++)
            {
                var entryid = packet.ReadUInt32("EntryID", i);
                var groupid_ = packet.ReadUInt32("GroupID", i);
                var productid_ = packet.ReadUInt32("ProductID", i);
                var ordering_ = packet.ReadInt32("Ordering", i);
                var vasservicetype = packet.ReadInt32("VasServiceType", i);
                var storedeliverytype = packet.ReadByte("StoreDeliveryType", i);

                packet.ResetBitReader();

                var bit5172 = packet.ReadBit("HasBattlepayDisplayInfo", i);
                if (bit5172)
                    ReadBattlepayDisplayInfo(packet, i);

                BattlePayShopEntry ShopEntry = new BattlePayShopEntry
                {
                    EntryID = entryid,
                    GroupID = groupid_,
                    ProductID = productid_,
                    Ordering = ((uint)ordering_),
                    VasServiceType = ((uint)vasservicetype),
                    StoreDeliveryType = storedeliverytype,
                    DisplayInfoID = bit5172
                };
                Storage.BattlePayShopEntries.Add(ShopEntry, packet.TimeSpan);
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