using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_productinfo")]

    public sealed record BattlePayProductInfo : IDataModel
    {
        [DBFieldName("ProductId", true)]
        public uint ProductId;

        [DBFieldName("NormalPriceFixedPoint", true)]
        public uint NormalPriceFixedPoint;

        [DBFieldName("CurrentPriceFixedPoint", true)]
        public uint CurrentPriceFixedPoint;

        [DBFieldName("ProductIds")]
        public string ProductIds;

        [DBFieldName("Unk1", true)]
        public uint Unk1;

        [DBFieldName("Unk2", true)]
        public uint Unk2;

        [DBFieldName("UnkInts", true)]
        public uint UnkInts;

        [DBFieldName("Unk3", true)]
        public uint Unk3;

        [DBFieldName("ChoiceType", true)]
        public uint ChoiceType;
    }
}
