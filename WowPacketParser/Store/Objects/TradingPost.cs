using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("perks_program_results")]
    public sealed record PerksProgramVendorData : IDataModel
    {
        [DBFieldName("ItemID", true)]
        public int ItemID;

        [DBFieldName("MountID", true)]
        public int MountID;

        [DBFieldName("BattlePetSpeciesID", true)]
        public int BattlePetSpeciesID;

        [DBFieldName("TransmogSetID", true)]
        public int TransmogSetID;

        [DBFieldName("ItemModifiedAppearanceID", true)]
        public int ItemModifiedAppearanceID;

        [DBFieldName("TransmogIllusionID", true)]
        public int TransmogIllusionID;

        [DBFieldName("ToyID", true)]
        public int ToyID;

        [DBFieldName("Price", true)]
        public int Price;

        [DBFieldName("Disabled", true)]
        public bool Disabled;
    }
}