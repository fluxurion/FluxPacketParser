using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_product")]

    /*
DROP TABLE IF EXISTS `battlepay_product`;
CREATE TABLE IF NOT EXISTS `battlepay_product` (
  `Entry` int(10) unsigned NOT NULL AUTO_INCREMENT,  
  `ProductID` int(10) unsigned DEFAULT 0,
  `Type` int(10) unsigned DEFAULT 0,
  `Flags` int(10) unsigned DEFAULT 0,
  `UnkInt1` int(10) unsigned DEFAULT 0,
  `DisplayID` int(10) unsigned DEFAULT 0,
  `ItemID` int(10) unsigned DEFAULT 0,
  `UnkInt4` int(10) unsigned DEFAULT 0,
  `UnkInt5` int(10) unsigned DEFAULT 0,
  `UnkInt6` int(10) unsigned DEFAULT 0,
  `UnkInt7` int(10) unsigned DEFAULT 0,
  `UnkInt8` int(10) unsigned DEFAULT 0,
  `UnkInt9` int(10) unsigned DEFAULT 0,
  `NameSize` int(10) unsigned DEFAULT 0,
  `UnkBit` int(10) unsigned DEFAULT 0,
  `UnkBits` int(10) unsigned DEFAULT 0,
  `ItemsSize` int(10) unsigned DEFAULT 0,
  `HasDisplayInfo` int(10) unsigned DEFAULT 0,
  `PetResultVariable` int(10) unsigned DEFAULT 0,
  `Name` varchar(255) DEFAULT NULL,
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
     */
    public sealed record BattlePayProduct : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("ProductID", true)]
        public uint ProductID;

        [DBFieldName("Type", true)]
        public uint Type;

        [DBFieldName("Flags", true)]
        public uint Flags;

        [DBFieldName("UnkInt1", true)]
        public uint UnkInt1;

        [DBFieldName("DisplayID", true)]
        public uint DisplayID;

        [DBFieldName("ItemID", true)]
        public uint ItemID;

        [DBFieldName("UnkInt4", true)]
        public uint UnkInt4;

        [DBFieldName("UnkInt5", true)]
        public uint UnkInt5;

        [DBFieldName("UnkInt6", true)]
        public uint UnkInt6;

        [DBFieldName("UnkInt7", true)]
        public uint UnkInt7;

        [DBFieldName("UnkInt8", true)]
        public uint UnkInt8;

        [DBFieldName("UnkInt9", true)]
        public uint UnkInt9;

        [DBFieldName("NameSize")]
        public uint NameSize;

        [DBFieldName("UnkBit", true)]
        public uint UnkBit;

        [DBFieldName("UnkBits", true)]
        public uint UnkBits;

        [DBFieldName("ItemsSize", true)]
        public uint ItemsSize;

        [DBFieldName("HasDisplayInfo", true)]
        public uint HasDisplayInfo;

        [DBFieldName("PetResultVariable", true)]
        public uint PetResultVariable;

        [DBFieldName("Name")]
        public string Name;
    }
}
