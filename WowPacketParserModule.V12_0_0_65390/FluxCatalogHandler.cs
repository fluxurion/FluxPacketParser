using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V12_0_0_65390.Parsers
{
    public static class FluxCatalogHandler
    {
        [Parser(Opcode.CMSG_GET_LAST_CATALOG_FETCH)]
        [Parser(Opcode.CMSG_UPDATE_LAST_CATALOG_FETCH)]
        public static void HandleZeroLengthPackets(Packet packet)
        {
        }

        [Parser(Opcode.CMSG_CATALOG_SHOP_LICENSE_GAME_DATA_REQUEST)]
        public static void HandleCatalogShopLicenseGameDataRequest(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (uint i = 0; i < count; i++)
                packet.ReadUInt32("ID", i);
        }

        [Parser(Opcode.SMSG_CATALOG_SHOP_OBTAIN_LICENSE)]
        public static void HandleCatalogShopObtainLicense(Packet packet)
        {
            packet.ReadUInt32("LicenseID");
        }

        [Parser(Opcode.SMSG_LAST_CATALOG_FETCH_RESPONSE)]
        public static void HandleLastCatalogFetchResponse(Packet packet)
        {
            packet.ReadUInt64("LastFetchTimestamp");
        }

        private static void ReadMissingLicenseGameDataInfo(Packet packet, params object[] index)
        {
            var flags = packet.ReadByte("Flags", index);
            var hasFlag0 = (flags & 0x80) != 0;
            var hasFlag12 = (flags & 0x40) != 0;
            var hasFlag13 = (flags & 0x20) != 0;
            var hasFlag112 = (flags & 0x10) != 0;
            var hasFlag113 = (flags & 0x08) != 0;
            packet.AddValue("HasFlag0", hasFlag0, index);
            packet.AddValue("HasFlag12", hasFlag12, index);
            packet.AddValue("HasFlag13", hasFlag13, index);
            packet.AddValue("HasFlag112", hasFlag112, index);
            packet.AddValue("HasFlag113", hasFlag113, index);

            packet.ReadUInt32("Field4", index);
            packet.ReadUInt32("Field8", index);
            packet.ReadUInt32("Field16", index);

            var vec1Count = packet.ReadUInt32("Vec1Count", index);
            var vec2Count = packet.ReadUInt32("Vec2Count", index);
            var vec3Count = packet.ReadUInt32("Vec3Count", index);

            packet.ReadUInt32("Field96", index);
            packet.ReadUInt32("Field100", index);
            packet.ReadUInt32("Field104", index);
            packet.ReadUInt32("Field108", index);
            packet.ReadUInt32("Field116", index);
            packet.ReadUInt32("Field120", index);

            for (uint i = 0; i < vec1Count; i++)
                packet.ReadUInt32("Vec1Value", index, i);

            for (uint i = 0; i < vec2Count; i++)
                packet.ReadUInt32("Vec2Value", index, i);

            for (uint i = 0; i < vec3Count; i++)
                packet.ReadUInt32("Vec3Value", index, i);
        }

        [Parser(Opcode.SMSG_CATALOG_SHOP_LICENSE_DATA)]
        public static void HandleCatalogShopLicenseData(Packet packet)
        {
            packet.ReadUInt32("Field32");

            var array1Count = packet.ReadUInt32("MissingLicenseGameDataCount");
            var array2Count = packet.ReadUInt32("Array2Count");

            packet.ReadUInt32("Field88");

            for (uint i = 0; i < array1Count; i++)
                ReadMissingLicenseGameDataInfo(packet, i);

            for (uint i = 0; i < array2Count; i++)
            {
                packet.ReadUInt32("Value", i);
                var flag = packet.ReadByte("Flag", i);
                packet.AddValue("HasFlag_bit7", (flag & 0x80) != 0, i);
            }
        }
    }
}
