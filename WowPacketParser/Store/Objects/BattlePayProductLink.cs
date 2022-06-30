using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_productlink")]

    /*
DROP TABLE IF EXISTS `battlepay_productlink`;
CREATE TABLE IF NOT EXISTS `battlepay_productlink` (
  `Entry` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `LinkID` int(10) unsigned DEFAULT 0,
  `ProductID` int(10) unsigned DEFAULT 0,
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
     */
    public sealed record BattlePayProductLink : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("LinkID", true)]
        public uint LinkID;

        [DBFieldName("ProductID", true)]
        public uint ProductID;
    }
}
