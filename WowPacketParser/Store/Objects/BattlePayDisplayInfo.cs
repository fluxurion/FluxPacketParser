using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_display_info")]
    public sealed record BattlePayDisplayInfo : IDataModel
    {
        [DBFieldName("DisplayInfoId", true)]
        public uint DisplayInfoId;

        [DBFieldName("CreatureDisplayInfoID", true)]
        public uint CreatureDisplayInfoID;

        [DBFieldName("FileDataID", true)]
        public uint FileDataID;

        [DBFieldName("DisplayCardWidth", true)]
        public uint DisplayCardWidth;

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

        [DBFieldName("Flags", true)]
        public uint Flags;
    }
}
