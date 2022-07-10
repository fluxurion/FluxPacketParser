using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_shop")]
    public sealed record BattlePayShop : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("EntryID", true)]
        public uint EntryID;

        [DBFieldName("GroupID", true)]
        public uint GroupID;

        [DBFieldName("ProductID")]
        public uint ProductID;

        [DBFieldName("Ordering")]
        public uint Ordering;

        [DBFieldName("VasServiceType")]
        public uint VasServiceType;

        [DBFieldName("StoreDeliveryType")]
        public uint StoreDeliveryType;
    }
}
