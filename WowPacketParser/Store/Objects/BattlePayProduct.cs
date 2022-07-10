using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_product")]

    public sealed record BattlePayProduct : IDataModel
    {
        [DBFieldName("ProductID", true)]
        public uint ProductID;

        [DBFieldName("Type", true)]
        public uint Type;

        [DBFieldName("Flags", true)]
        public uint Flags;

        [DBFieldName("Unk1", true)]
        public uint Unk1;

        [DBFieldName("DisplayId", true)]
        public uint DisplayId;

        [DBFieldName("ItemId", true)]
        public uint ItemId;

        [DBFieldName("Unk4", true)]
        public uint Unk4;

        [DBFieldName("Unk5", true)]
        public uint Unk5;

        [DBFieldName("Unk6", true)]
        public uint Unk6;

        [DBFieldName("Unk7", true)]
        public uint Unk7;

        [DBFieldName("Unk8", true)]
        public uint Unk8;

        [DBFieldName("Unk9", true)]
        public uint Unk9;

        [DBFieldName("UnkString")]
        public uint UnkString;

        [DBFieldName("UnkBit", true)]
        public uint UnkBit;

        [DBFieldName("UnkBits", true)]
        public uint UnkBits;

        [DBFieldName("Name")]
        public string Name;
    }
}
