using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_product_group")]
    public sealed record BattlePayProductGroup : IDataModel
    {
        [DBFieldName("GroupID", true)]
        public uint GroupID;

        [DBFieldName("Name")]
        public string Name;

        [DBFieldName("IconFileDataID")]
        public uint IconFileDataID;

        [DBFieldName("DisplayType")]
        public byte DisplayType;

        [DBFieldName("Ordering")]
        public uint Ordering;
    }
}
