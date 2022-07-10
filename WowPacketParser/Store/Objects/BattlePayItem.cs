using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_item")]

    public sealed record BattlePayItem : IDataModel
    {
        [DBFieldName("ID", true)]
        public uint ID;

        [DBFieldName("UnkByte", true)]
        public uint UnkByte;

        [DBFieldName("ItemID", true)]
        public uint ItemID;

        [DBFieldName("Quantity", true)]
        public uint Quantity;

        [DBFieldName("UnkInt1", true)]
        public uint UnkInt1;

        [DBFieldName("UnkInt2", true)]
        public uint UnkInt2;

        [DBFieldName("HasPet", true)]
        public uint IsPet;

        [DBFieldName("PetResult", true)]
        public uint PetResult;

        [DBFieldName("Display", true)]
        public uint Display;
    }
}
