using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_productinfo")]

    /*
DROP TABLE IF EXISTS `battlepay_productinfo`;
CREATE TABLE IF NOT EXISTS `battlepay_productinfo` (
  `Entry` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `ProductID` int(10) unsigned DEFAULT 0,
  `NormalPriceFixedPoint` int(10) unsigned DEFAULT 0,
  `CurrentPriceFixedPoint` int(10) unsigned DEFAULT 0,
  `ProductIDsSize` int(10) unsigned DEFAULT 0,
  `UnkInt2` int(10) unsigned DEFAULT 0,
  `UnkInt3` int(10) unsigned DEFAULT 0,
  `UnkIntsSize` int(10) unsigned DEFAULT 0,
  `UnkInt4` int(10) unsigned DEFAULT 0,
  `UnkInts` int(10) unsigned DEFAULT 0,
  `ChoiceType` int(10) unsigned DEFAULT 0,
  `HasBattlepayDisplayInfo` int(10) unsigned DEFAULT 0,
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
     */
    public sealed record BattlePayProductInfo : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("ProductID", true)]
        public uint ProductID;

        [DBFieldName("NormalPriceFixedPoint", true)]
        public uint NormalPriceFixedPoint;

        [DBFieldName("CurrentPriceFixedPoint", true)]
        public uint CurrentPriceFixedPoint;

        [DBFieldName("ProductIDsSize", true)]
        public uint ProductIDsSize;

        [DBFieldName("UnkInt2", true)]
        public uint UnkInt2;

        [DBFieldName("UnkInt3", true)]
        public uint UnkInt3;

        [DBFieldName("UnkIntsSize", true)]
        public uint UnkIntsSize;

        [DBFieldName("UnkInt4", true)]
        public uint UnkInt4;

        [DBFieldName("UnkInts", true)]
        public uint UnkInts;

        [DBFieldName("ChoiceType", true)]
        public uint ChoiceType;

        [DBFieldName("HasBattlepayDisplayInfo", true)]
        public uint HasBattlepayDisplayInfo;
    }
}
