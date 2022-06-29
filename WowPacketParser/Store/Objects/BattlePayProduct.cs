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

        [DBFieldName("NormalPriceFixedPoint", true)]
        public uint NormalPriceFixedPoint;

        [DBFieldName("CurrentPriceFixedPoint", true)]
        public uint CurrentPriceFixedPoint;
        
        [DBFieldName("Type", true)]
        public uint Type;
        
        [DBFieldName("WebsiteType", true)]
        public uint WebsiteType;

        [DBFieldName("CustomValue", true)]
        public uint CustomValue;
        
        [DBFieldName("ChoiceType", true)]
        public uint ChoiceType;
        
        [DBFieldName("CurrencyID", true)]
        public uint CurrencyID;
        
        [DBFieldName("Flags", true)]
        public uint Flags;
        
        [DBFieldName("DisplayInfoID", true)]
        public uint DisplayInfoID;
        
        [DBFieldName("SpellID", true)]
        public uint SpellID;
        
        [DBFieldName("CreatureID", true)]
        public uint CreatureID;
        
        [DBFieldName("ClassMask", true)]
        public uint ClassMask;

        [DBFieldName("ScriptName")]
        public string ScriptName;
    }
}
