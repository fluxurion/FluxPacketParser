using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_group")]
    public sealed record BattlePayGroup : IDataModel
    {
        [DBFieldName("GroupID", true)]
        public uint GroupID;

        [DBFieldName("IconFileDataID")]
        public uint IconFileDataID;

        [DBFieldName("DisplayType")]
        public uint DisplayType;

        [DBFieldName("Ordering")]
        public uint Ordering;

        [DBFieldName("Unk")]
        public uint Unk;

        [DBFieldName("MainGroupID")]
        public uint MainGroupID;

        [DBFieldName("Name")]
        public string Name;

        [DBFieldName("Description")]
        public string Description;
    }
}
