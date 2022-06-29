using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_product_item")]
    public sealed record BattlePayProductItem : IDataModel
    {
        [DBFieldName("ID", true)]
        public uint ID;

        [DBFieldName("ProductID", true)]
        public uint ProductID;

        [DBFieldName("ItemID", true)]
        public uint ItemID;

        [DBFieldName("Quantity", true)]
        public uint Quantity;

        [DBFieldName("DisplayID", true)]
        public uint DisplayID;

        [DBFieldName("PetResult", true)]
        public uint PetResult;
    }
}
