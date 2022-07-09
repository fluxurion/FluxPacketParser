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
        [DBFieldName("ProductID", true)]
        public uint ProductID;

        [DBFieldName("Type", true)]
        public uint Type;

        [DBFieldName("Flags", true)]
        public uint Flags;

        [DBFieldName("Unk1", true)]
        public uint Unk1;

        [DBFieldName("DisplayId", true)]
        public uint DisplayId;

        [DBFieldName("ItemId", true)]
        public uint ItemId;

        [DBFieldName("Unk4", true)]
        public uint Unk4;

        [DBFieldName("Unk5", true)]
        public uint Unk5;

        [DBFieldName("Unk6", true)]
        public uint Unk6;

        [DBFieldName("Unk7", true)]
        public uint Unk7;

        [DBFieldName("Unk8", true)]
        public uint Unk8;

        [DBFieldName("Unk9", true)]
        public uint Unk9;

        [DBFieldName("UnkString")]
        public uint UnkString;

        [DBFieldName("UnkBit", true)]
        public uint UnkBit;

        [DBFieldName("UnkBits", true)]
        public uint UnkBits;

        [DBFieldName("Name")]
        public string Name;
    }
}
