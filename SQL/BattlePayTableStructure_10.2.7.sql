/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `battlepay_display_infos`
--

DROP TABLE IF EXISTS `battlepay_display_infos`;
CREATE TABLE `battlepay_display_infos` (
  `Entry` int(10) unsigned NOT NULL,
  `ProductInfoID` int(10) unsigned NOT NULL,
  `ProductDataID` int(10) unsigned NOT NULL,
  `ShopDataID` int(10) unsigned NOT NULL,
  `HasIconFileDataID` int(11) NOT NULL DEFAULT '0',
  `HasPreview` int(11) NOT NULL DEFAULT '0',
  `HasIconBorder` int(11) NOT NULL DEFAULT '0',
  `HasUnknown1` int(11) NOT NULL DEFAULT '0',
  `HasUiTextureAtlasMemberID` int(11) NOT NULL DEFAULT '0',
  `HasUiTextureAtlasMemberID2` int(11) NOT NULL DEFAULT '0',
  `VisualCount` int(10) unsigned NOT NULL DEFAULT '0',
  `CardType` int(11) NOT NULL DEFAULT '0',
  `Unknown3` int(11) NOT NULL DEFAULT '0',
  `ProductMultiplier` int(11) NOT NULL DEFAULT '0',
  `IconFileDataID` int(10) unsigned NOT NULL DEFAULT '0',
  `UIModelSceneID` int(10) unsigned NOT NULL DEFAULT '0',
  `Title` text,
  `Title2` text,
  `Description` text,
  `Description2` text,
  `Description3` text,
  `IconBorder` int(11) NOT NULL DEFAULT '0',
  `Unknown1` int(11) NOT NULL DEFAULT '0',
  `UiTextureAtlasMemberID` int(11) NOT NULL DEFAULT '0',
  `UiTextureAtlasMemberID2` int(11) NOT NULL DEFAULT '0',
  `Description4` text,
  `Description5` text,
  `PreviewCreatureDisplayIDs` text,
  `PreviewUIModelSceneIDs` text,
  `PreviewTransmogSets` text,
  `PreviewTitles` text,
  PRIMARY KEY (`Entry`,`ProductInfoID`,`ProductDataID`,`ShopDataID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `battlepay_display_info_visuals`
--

DROP TABLE IF EXISTS `battlepay_display_info_visuals`;
CREATE TABLE `battlepay_display_info_visuals` (
  `DisplayInfoEntry` int(10) unsigned NOT NULL,
  `VisualIndex` int(10) unsigned NOT NULL,
  `CreatureDisplayID` int(10) unsigned NOT NULL DEFAULT '0',
  `PreviewUIModelSceneID` int(10) unsigned NOT NULL DEFAULT '0',
  `TransmogSetID` int(10) unsigned NOT NULL DEFAULT '0',
  `VisualName` text,
  PRIMARY KEY (`DisplayInfoEntry`,`VisualIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `battlepay_product_infos`
--

DROP TABLE IF EXISTS `battlepay_product_infos`;
CREATE TABLE `battlepay_product_infos` (
  `Entry` int(10) unsigned NOT NULL,
  `ProductInfoID` int(10) unsigned NOT NULL DEFAULT '0',
  `NormalPrice` bigint(20) NOT NULL DEFAULT '0',
  `CurrentPrice` bigint(20) NOT NULL DEFAULT '0',
  `ProductInfoFlags` int(11) NOT NULL DEFAULT '0',
  `Unknown1` int(11) NOT NULL DEFAULT '0',
  `Unknown2` int(11) NOT NULL DEFAULT '0',
  `Unknown3` int(11) NOT NULL DEFAULT '0',
  `Unknown4` int(11) NOT NULL DEFAULT '0',
  `Unknown5` int(11) NOT NULL DEFAULT '0',
  `DeliverableProductIDExtra` int(10) unsigned NOT NULL DEFAULT '0',
  `Unk1027` int(10) unsigned NOT NULL DEFAULT '0',
  `UnknownIfFlags1_1` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags1_2` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags2_1` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags2_2` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags2_3` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags2_4` int(11) NOT NULL DEFAULT '0',
  `HasVisualMetadata` int(11) NOT NULL DEFAULT '0',
  `DeliverableProductIDs` text,
  `DisplayFlag` int(10) unsigned NOT NULL DEFAULT '0',
  `HasUnknown1InDisplayInfo` int(11) NOT NULL DEFAULT '0',
  `HasBattlePayDisplayInfo` int(11) NOT NULL DEFAULT '0',
  `ChoiceType` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `battlepay_product_datas`
--

DROP TABLE IF EXISTS `battlepay_product_datas`;
CREATE TABLE `battlepay_product_datas` (
  `Entry` int(10) unsigned NOT NULL,
  `ProductID` int(10) unsigned NOT NULL DEFAULT '0',
  `Type` int(11) NOT NULL DEFAULT '0',
  `ItemID` int(10) unsigned NOT NULL DEFAULT '0',
  `ItemCount` int(10) unsigned NOT NULL DEFAULT '0',
  `MountSpellID` int(10) unsigned NOT NULL DEFAULT '0',
  `BattlePetSpeciesCreatureID` int(10) unsigned NOT NULL DEFAULT '0',
  `Unknown1` int(10) unsigned NOT NULL DEFAULT '0',
  `Unknown2` int(10) unsigned NOT NULL DEFAULT '0',
  `Unknown3` int(10) unsigned NOT NULL DEFAULT '0',
  `TransmogSetID` int(10) unsigned NOT NULL DEFAULT '0',
  `Unknown8` int(10) unsigned NOT NULL DEFAULT '0',
  `Unknown9` int(10) unsigned NOT NULL DEFAULT '0',
  `Unknown10` int(10) unsigned NOT NULL DEFAULT '0',
  `Unknown11` int(10) unsigned NOT NULL DEFAULT '0',
  `HasDisplayInfo` int(11) NOT NULL DEFAULT '0',
  `PetResultVariable` int(10) unsigned NOT NULL DEFAULT '0',
  `Name` text,
  `AlreadyOwned` int(11) NOT NULL DEFAULT '0',
  `DisplayFlag` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `battlepay_product_items`
--

DROP TABLE IF EXISTS `battlepay_product_items`;
CREATE TABLE `battlepay_product_items` (
  `ProductEntry` int(10) unsigned NOT NULL,
  `ItemOrder` int(10) unsigned NOT NULL,
  `ID` int(10) unsigned NOT NULL DEFAULT '0',
  `UnknownByte` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `ItemID` int(10) unsigned NOT NULL DEFAULT '0',
  `Quantity` int(10) unsigned NOT NULL DEFAULT '0',
  `UnknownInt1` int(10) unsigned NOT NULL DEFAULT '0',
  `UnknownInt2` int(10) unsigned NOT NULL DEFAULT '0',
  `IsPet` int(11) NOT NULL DEFAULT '0',
  `HasPetResult` int(11) NOT NULL DEFAULT '0',
  `PetResultFlags` int(10) unsigned NOT NULL DEFAULT '0',
  `HasVisualMetadata` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ProductEntry`,`ItemOrder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `battlepay_groups`
--

DROP TABLE IF EXISTS `battlepay_groups`;
CREATE TABLE `battlepay_groups` (
  `Entry` int(10) unsigned NOT NULL,
  `GroupID` int(10) unsigned NOT NULL,
  `IconFileDataID` int(10) unsigned NOT NULL DEFAULT '0',
  `DisplayType` int(10) unsigned NOT NULL DEFAULT '0',
  `Ordering` int(10) unsigned NOT NULL DEFAULT '0',
  `Unknown` int(10) unsigned NOT NULL DEFAULT '0',
  `MainGroupID` int(10) unsigned NOT NULL DEFAULT '0',
  `Name` text,
  `Description` text,
  PRIMARY KEY (`Entry`,`GroupID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `battlepay_shop_datas`
--

DROP TABLE IF EXISTS `battlepay_shop_datas`;
CREATE TABLE `battlepay_shop_datas` (
  `Entry` int(10) unsigned NOT NULL,
  `EntryID` int(10) unsigned NOT NULL DEFAULT '0',
  `GroupID` int(10) unsigned NOT NULL DEFAULT '0',
  `ProductID` int(10) unsigned NOT NULL DEFAULT '0',
  `Ordering` int(10) unsigned NOT NULL DEFAULT '0',
  `VasServiceType` int(10) unsigned NOT NULL DEFAULT '0',
  `StoreDeliveryType` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `HasBattlePayDisplayInfo` int(11) NOT NULL DEFAULT '0',
  `Unknown` int(11) NOT NULL DEFAULT '0',
  `DisplayFlag` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
