using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_shop_entry")]
    public sealed record BattlePayShopEntry : IDataModel
    {
        [DBFieldName("EntryID", true)]
        public uint EntryID;

        [DBFieldName("GroupID", true)]
        public uint GroupID;

        [DBFieldName("ProductID", true)]
        public uint ProductID;

        [DBFieldName("Ordering", true)]
        public uint Ordering;

        [DBFieldName("VasServiceType", true)]
        public uint VasServiceType;

        [DBFieldName("StoreDeliveryType", true)]
        public uint StoreDeliveryType;

        [DBFieldName("DisplayInfoID", true)]
        public uint DisplayInfoID;
    }
}
