using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using EraBattlePay = WowPacketParserModule.V5_5_0_61735.Parsers.FluxBattlePayHandler;

namespace WowPacketParserModule.V1_60_1_70009.Parsers
{
    // Classic 1.60.1.70009 BattlePay — same packet skeletons as era, but the
    // embedded product/deliverable records use the JAM bit-packed format.
    public static class FluxBattlePayHandler
    {
        // Classic 1.60.1.70009 product-list deliverable — the catalog writer
        // (LuaSolScripts/BattlePay/BattlePayCatalog.lua CatalogWriteDeliverable)
        // uses the JAM bit-packed format: same 13 u32 scalars + u8 nameLen, but
        // the flag block is a 16-bit run {AlreadyOwns, HasPetResult,
        // ChoicesCount(7), HasDisplayInfo, PetResult(4), pad(2)} and choice
        // records come AFTER the name string ({u8 ChoiceType, u32 ChoiceID} —
        // same as 12.1 ReadDeliverable121), not before it like the era reader.
        private static void ReadDeliverable160(Packet packet, params object[] index)
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

            packet.ResetBitReader();
            packet.ReadBit("AlreadyOwns", index);
            packet.ReadBit("HasPetResult", index);
            var choicesCount = packet.ReadBits("ChoicesCount", 7, index);
            var hasDisplayInfo = packet.ReadBit("HasDisplayInfo", index);
            packet.ReadBits("PetResult", 4, index);
            packet.ReadBits(2); // pad
            packet.ResetBitReader();

            packet.ReadWoWString("Name", nameLen, index);

            for (uint i = 0; i < choicesCount; i++)
            {
                packet.ReadByte("ChoiceType", index, i);
                packet.ReadUInt32("ChoiceID", index, i);
            }

            if (hasDisplayInfo)
                _ = EraBattlePay.ReadVisualMetadata(packet, 6, deliverableID, index);
        }

        // 0x460224 — {u32 Result, u32 CurrencyID, 4x u32 counts, 4 arrays};
        // product records use the 1.60 JAM deliverable layout.
        [Parser(Opcode.SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleProductListResponse160(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadUInt32("CurrencyID");

            var productInfoCount = packet.ReadUInt32("ProductInfoCount");
            var productCount = packet.ReadUInt32("ProductCount");
            var groupCount = packet.ReadUInt32("ProductGroupCount");
            var shopCount = packet.ReadUInt32("ShopCount");

            for (uint i = 0; i < productInfoCount; i++)
                EraBattlePay.ReadProductInfo(packet, i);

            for (uint i = 0; i < productCount; i++)
                ReadDeliverable160(packet, i);

            for (uint i = 0; i < groupCount; i++)
                EraBattlePay.ReadGroup(packet, i);

            for (uint i = 0; i < shopCount; i++)
                EraBattlePay.ReadShop(packet, i);
        }

        // 0x460221 — the 1.60 client reader is an opaque blob (reads all
        // remaining bytes into a pointer field) with a datasize==8 assert,
        // so the payload is a single u64 (retail analog: DistributionID).
        [Parser(Opcode.SMSG_BATTLE_PAY_DELIVERY_STARTED, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleDeliveryStarted160(Packet packet)
        {
            packet.ReadUInt64("Data");
        }

        // 0x460223 — opaque blob with datasize==4 assert → single u32.
        [Parser(Opcode.SMSG_BATTLE_PAY_MOUNT_DELIVERED, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleMountDelivered160(Packet packet)
        {
            packet.ReadUInt32("Data");
        }

        // 0x460224 — structured reader: { u32, packed guid } (same layout as
        // the retail DisplayID + BattlePetGuid pair).
        [Parser(Opcode.SMSG_BATTLE_PAY_BATTLE_PET_DELIVERED, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleBattlePetDelivered160(Packet packet)
        {
            packet.ReadUInt32("DisplayID");
            packet.ReadPackedGuid128("BattlePetGUID");
        }

        // 0x460225 — opaque blob with datasize==8 assert → single u64 (the
        // LuaSol writer's flag+count+items payload is tolerated by the client
        // reader but trips its datasize diagnostic).
        [Parser(Opcode.SMSG_BATTLE_PAY_COLLECTION_ITEM_DELIVERED, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleCollectionItemDelivered160(Packet packet)
        {
            packet.ReadUInt64("Data");
        }
    }
}
