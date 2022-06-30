using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_group")]

    /*
DROP TABLE IF EXISTS `battlepay_group`;
CREATE TABLE IF NOT EXISTS `battlepay_group` (
  `Entry` int(10) unsigned NOT NULL AUTO_INCREMENT,  
  `GroupID` int(10) unsigned DEFAULT 0,  
  `IconFileDataID` int(10) unsigned DEFAULT 0,  
  `DisplayType` int(10) unsigned DEFAULT 0,  
  `Ordering` int(10) unsigned DEFAULT 0,  
  `Unkt` int(10) unsigned DEFAULT 0,  
  `NameLength` int(10) unsigned DEFAULT 0,  
  `IsAvailableDescription` int(10) unsigned DEFAULT 0,  
  `Name` varchar(255) NOT NULL DEFAULT '',
  `Description` TEXT DEFAULT '',
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
     */
    public sealed record BattlePayGroup : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("GroupID", true)]
        public uint GroupID;

        [DBFieldName("IconFileDataID")]
        public uint IconFileDataID;

        [DBFieldName("DisplayType")]
        public uint DisplayType;

        [DBFieldName("Ordering")]
        public uint Ordering;

        [DBFieldName("Unkt")]
        public uint Unkt;

        [DBFieldName("NameLength")]
        public uint NameLength;

        [DBFieldName("IsAvailableDescription")]
        public uint IsAvailableDescription;

        [DBFieldName("Name")]
        public string Name;

        [DBFieldName("Description")]
        public string Description;
    }
}
