using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_groups")]
    public sealed record BattlePayGroup : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("GroupID", true)]
        public uint GroupID;

        [DBFieldName("IconFileDataID")]
        public uint IconFileDataID;

        [DBFieldName("DisplayType")]
        public uint DisplayType;

        [DBFieldName("Ordering")]
        public uint Ordering;

        [DBFieldName("Unknown")]
        public uint Unknown;

        [DBFieldName("MainGroupID")]
        public uint MainGroupID;

        [DBFieldName("Name")]
        public string Name;

        [DBFieldName("Description")]
        public string Description;
    }
}
