using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("perks_program_results")]
    public sealed record PerksProgramResult : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("Type")]
        public byte Type;

        [DBFieldName("UnkLong")]
        public ulong? UnkLong;

        [DBFieldName("BoughtVendorItemId")]
        public uint? BoughtVendorItemId;

        [DBFieldName("BoughtItemIds")]
        public string BoughtItemIds;

        [DBFieldName("BoughtItemTimes")]
        public string BoughtItemTimes;

        [DBFieldName("Unk4Int1")]
        public uint? Unk4Int1;

        [DBFieldName("Unk4Int2")]
        public uint? Unk4Int2;

        [DBFieldName("Unk4Int3")]
        public uint? Unk4Int3;

        [DBFieldName("Unk4Ints")]
        public string Unk4Ints;

        [DBFieldName("VendorGuid")]
        public byte[] VendorGuid;

        [DBFieldName("ModelSceneCameraGuid")]
        public byte[] ModelSceneCameraGuid;

        [DBFieldName("FrozenItems")]
        public string FrozenItems;
    }
}