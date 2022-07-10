using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_shop")]

    /*
DROP TABLE IF EXISTS `battlepay_shop`;
CREATE TABLE IF NOT EXISTS `battlepay_shop` (
  `Entry` int(10) unsigned NOT NULL AUTO_INCREMENT,  
  `ID` int(10) unsigned DEFAULT 0,
  `GroupID` int(10) unsigned DEFAULT 0,
  `ProductID` int(10) unsigned DEFAULT 0,
  `Ordering` int(10) unsigned DEFAULT 0,
  `VasServiceType` int(10) unsigned DEFAULT 0,
  `StoreDeliveryType` int(10) unsigned DEFAULT 0,
  `HasBattlepayDisplayInfo` int(10) unsigned DEFAULT 0,
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
     */
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
