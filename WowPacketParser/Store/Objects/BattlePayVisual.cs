using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_visual")]

    /*
DROP TABLE IF EXISTS `battlepay_visual`;
CREATE TABLE IF NOT EXISTS `battlepay_visual` (
  `Entry` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Visual1` int(10) unsigned DEFAULT NULL,
  `Visual2` int(10) unsigned DEFAULT NULL,
  `Visual3` int(10) unsigned DEFAULT NULL,  
  `ProductName` varchar(255) DEFAULT NULL,
  UNIQUE KEY `Entry` (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
     */
    public sealed record BattlePayVisual : IDataModel
    {
        [DBFieldName("Visual1", true)]
        public uint Visual1;

        [DBFieldName("Visual2", true)]
        public uint Visual2;

        [DBFieldName("Visual3", true)]
        public uint Visual3;

        [DBFieldName("ProductName")]
        public string ProductName;
    }
}
