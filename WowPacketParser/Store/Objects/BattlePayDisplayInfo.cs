using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_displayinfo")]

    /*
DROP TABLE IF EXISTS `battlepay_displayinfo`;
CREATE TABLE IF NOT EXISTS `battlepay_displayinfo` (
  `Entry` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `HasCreatureDisplayInfoID` int(10) unsigned NOT NULL DEFAULT 0,
  `HasFileDataID` int(10) unsigned DEFAULT NULL,
  `bits16` int(10) unsigned DEFAULT 0,
  `bits529` int(10) unsigned DEFAULT 0,
  `bits1042` int(10) unsigned DEFAULT 0,
  `bits5139` int(10) unsigned DEFAULT 0,
  `bits9236` int(10) unsigned DEFAULT 0,
  `bits13396` int(10) unsigned DEFAULT 0,
  `bits17493` int(10) unsigned DEFAULT 0,
  `VisualsSize` int(10) unsigned DEFAULT 0,
  `unktint1` int(10) unsigned DEFAULT 0,
  `unktint2` int(10) unsigned DEFAULT 0,
  `unktint3` int(10) unsigned DEFAULT 0,
  `CreatureDisplayInfoID` int(10) unsigned DEFAULT 0,
  `FileDataID` int(10) unsigned DEFAULT 0,
  `Name1` varchar(255) DEFAULT NULL,
  `Name2` varchar(255) NOT NULL DEFAULT '',
  `Name3` TEXT DEFAULT NULL,
  `Name4` TEXT DEFAULT NULL,
  `Name5` TEXT DEFAULT NULL,
  `Flags` int(10) unsigned NOT NULL DEFAULT 0,
  `Unkt2Id` int(10) unsigned NOT NULL DEFAULT 0,
  `Unkt4Id` int(10) unsigned NOT NULL DEFAULT 0,
  `Unkt3Id` int(10) unsigned NOT NULL DEFAULT 0,  
  `Name6` varchar(255) DEFAULT NULL,
  `Name7` varchar(255) DEFAULT NULL,
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
     */
    public sealed record BattlePayDisplayInfo : IDataModel
    {
        [DBFieldName("HasCreatureDisplayInfoID", true)]
        public uint HasCreatureDisplayInfoID;

        [DBFieldName("HasFileDataID", true)]
        public uint HasFileDataID;

        [DBFieldName("bits16", true)]
        public uint bits16;

        [DBFieldName("bits529", true)]
        public uint bits529;

        [DBFieldName("bits1042", true)]
        public uint bits1042;

        [DBFieldName("bits5139", true)]
        public uint bits5139;

        [DBFieldName("bits9236", true)]
        public uint bits9236;

        [DBFieldName("bits13396", true)]
        public uint bits13396;

        [DBFieldName("bits17493", true)]
        public uint bits17493;

        [DBFieldName("VisualsSize", true)]
        public uint VisualsSize;

        [DBFieldName("unktint1", true)]
        public uint unktint1;

        [DBFieldName("unktint2", true)]
        public uint unktint2;

        [DBFieldName("unktint3", true)]
        public uint unktint3;

        [DBFieldName("CreatureDisplayInfoID", true)]
        public uint CreatureDisplayInfoID;

        [DBFieldName("FileDataID", true)]
        public uint FileDataID;

        [DBFieldName("Name1")]
        public string Name1;

        [DBFieldName("Name2")]
        public string Name2;

        [DBFieldName("Name3")]
        public string Name3;

        [DBFieldName("Name4")]
        public string Name4;

        [DBFieldName("Name5")]
        public string Name5;

        [DBFieldName("Flags", true)]
        public uint Flags;

        [DBFieldName("Unkt2Id", true)]
        public uint Unkt2Id;

        [DBFieldName("Unkt4Id", true)]
        public uint Unkt4Id;

        [DBFieldName("Unkt3Id", true)]
        public uint Unkt3Id;

        [DBFieldName("Name6")]
        public string Name6;

        [DBFieldName("Name7")]
        public string Name7;
    }
}
