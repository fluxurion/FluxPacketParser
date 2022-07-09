using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_visual")]

    public sealed record BattlePayVisual : IDataModel
    {
        [DBFieldName("Name")]
        public string Name;

        [DBFieldName("DisplayId", true)]
        public uint DisplayId;

        [DBFieldName("VisualId", true)]
        public uint VisualId;

        [DBFieldName("Unk", true)]
        public uint Unk;
    }
}
