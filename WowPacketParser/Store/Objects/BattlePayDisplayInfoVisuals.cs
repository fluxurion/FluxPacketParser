using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_display_info_visuals")]
    public sealed record BattlePayDisplayInfoVisual : IDataModel
    {
        [DBFieldName("DisplayInfoId", true)]
        public uint DisplayInfoId;

        [DBFieldName("DisplayId", true)]
        public uint DisplayId;

        [DBFieldName("VisualId", true)]
        public uint VisualId;

        [DBFieldName("ProductName")]
        public string ProductName;
    }
}
