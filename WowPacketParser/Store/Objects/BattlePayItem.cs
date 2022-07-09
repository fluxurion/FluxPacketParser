using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_item")]

    /*
DROP TABLE IF EXISTS `battlepay_item`;
CREATE TABLE IF NOT EXISTS `battlepay_item` (
  `Entry` int(10) unsigned NOT NULL AUTO_INCREMENT,  
  `ID` int(10) unsigned DEFAULT 0,  
  `UnkByte` int(10) unsigned DEFAULT 0,  
  `ItemID` int(10) unsigned DEFAULT 0,  
  `Quantity` int(10) unsigned DEFAULT 0,  
  `UnkInt1` int(10) unsigned DEFAULT 0,  
  `UnkInt2` int(10) unsigned DEFAULT 0,  
  `HasPet` int(10) unsigned DEFAULT 0,  
  `PetResult` int(10) unsigned DEFAULT 0,  
  `DisplayInfo` int(10) unsigned DEFAULT 0,  
  `PetResultVariable` int(10) unsigned DEFAULT 0,  
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
     */
    public sealed record BattlePayItem : IDataModel
    {
        [DBFieldName("ID", true)]
        public uint ID;

        [DBFieldName("UnkByte", true)]
        public uint UnkByte;

        [DBFieldName("ItemID", true)]
        public uint ItemID;

        [DBFieldName("Quantity", true)]
        public uint Quantity;

        [DBFieldName("UnkInt1", true)]
        public uint UnkInt1;

        [DBFieldName("UnkInt2", true)]
        public uint UnkInt2;

        [DBFieldName("HasPet", true)]
        public uint IsPet;

        [DBFieldName("PetResult", true)]
        public uint PetResult;

        [DBFieldName("Display", true)]
        public uint Display;
    }
}
