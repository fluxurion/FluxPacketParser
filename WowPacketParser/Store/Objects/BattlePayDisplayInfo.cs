using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_displayinfo")]
    public sealed record BattlePayDisplayInfo : IDataModel
    {
        [DBFieldName("CreatureDisplayID", true)]
        public uint CreatureDisplayID;

        [DBFieldName("VisualID", true)]
        public uint VisualID;

        [DBFieldName("Name1")]
        public string Name1;

        [DBFieldName("Name2")]
        public string Name2;

        [DBFieldName("Name3")]
        public string Name3;

        [DBFieldName("Name4")]
        public string Name4;

        [DBFieldName("Name5")]
        public string Name5;

        [DBFieldName("Name6")]
        public string Name6;

        [DBFieldName("Name7")]
        public string Name7;

        [DBFieldName("Flags", true)]
        public uint Flags;

        [DBFieldName("Unk1", true)]
        public uint Unk1;

        [DBFieldName("Unk2", true)]
        public uint Unk2;

        [DBFieldName("Unk3", true)]
        public uint Unk3;

        [DBFieldName("UnkInt1", true)]
        public uint UnkInt1;

        [DBFieldName("UnkInt2", true)]
        public uint UnkInt2;

        [DBFieldName("UnkInt3", true)]
        public uint UnkInt3;
    }
}
