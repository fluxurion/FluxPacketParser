using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V5_5_0_61735.Parsers
{
    public static class FluxCatalogHandler
    {
        // 0x460379 — {u64 LastFetchTimestamp}; retail 12.1 0x450380,
        // region drift -7 anchors (MIRROR_VARS 0x460368<->0x45036F verified,
        // UNSET_INSTANCE_LEAVER 0x46036A<->0x450371). 8-byte payload, all
        // zero at glue screen = no previous catalog fetch.
        [Parser(Opcode.SMSG_LAST_CATALOG_FETCH_RESPONSE, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleLastCatalogFetchResponse(Packet packet)
        {
            packet.ReadUInt64("LastFetchTimestamp");
        }
    }
}
