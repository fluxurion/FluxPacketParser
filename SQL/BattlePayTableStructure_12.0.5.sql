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
-- Links to: battlepay_product_infos (SourceType=1), battlepay_product_datas (SourceType=2),
--           battlepay_shop_datas (SourceType=3), product item entries (SourceType=4)
--

DROP TABLE IF EXISTS `battlepay_display_infos`;
CREATE TABLE `battlepay_display_infos` (
  `SourceType` tinyint(3) unsigned NOT NULL COMMENT '1=ProductInfo, 2=Product, 3=Shop, 4=ProductItem',
  `SourceID` int(10) unsigned NOT NULL COMMENT 'FK to the respective table ID based on SourceType',
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
  PRIMARY KEY (`SourceType`,`SourceID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `battlepay_product_infos`
-- Represents a shop product LISTING (what appears in the Battle.net shop UI)
--

DROP TABLE IF EXISTS `battlepay_product_infos`;
CREATE TABLE `battlepay_product_infos` (
  `Entry` int(10) unsigned NOT NULL,
  `ShopListingID` int(10) unsigned NOT NULL DEFAULT '0',
  `NormalPrice` bigint(20) NOT NULL DEFAULT '0',
  `CurrentPrice` bigint(20) NOT NULL DEFAULT '0',
  `ProductInfoFlags` int(11) NOT NULL DEFAULT '0',
  `Unknown1` int(11) NOT NULL DEFAULT '0' COMMENT 'Type enum: 2=Pet, 3=Mount, 4=WoWToken, 5=NameChange, 6=FactionChange, 8=RaceChange, 11=Transfer, 14=Toy, 15=Bundle, 18=Edition, 26=Transmog',
  `Unknown2` int(11) NOT NULL DEFAULT '0' COMMENT 'Flags: 15=Standard, 47=Pet/Mount, 262223=Service',
  `Unknown3` int(11) NOT NULL DEFAULT '0',
  `Unknown4` int(11) NOT NULL DEFAULT '0',
  `Unknown5` int(11) NOT NULL DEFAULT '0',
  `DeliverableIDExtra` int(10) unsigned NOT NULL DEFAULT '0',
  `Unk1027` int(10) unsigned NOT NULL DEFAULT '0',
  `UnkUInt64` bigint(20) unsigned NOT NULL DEFAULT '0' COMMENT 'ListingID — monotonically incrementing',
  `UnknownIfFlags1_1` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags1_2` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags2_1` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags2_2` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags2_3` int(11) NOT NULL DEFAULT '0',
  `UnknownIfFlags2_4` int(11) NOT NULL DEFAULT '0',
  `HasVisualMetadata` int(11) NOT NULL DEFAULT '0',
  `DeliverableIDs` text COMMENT 'Comma-separated DeliverableIDs -> battlepay_product_datas.DeliverableID',
  `DeliverableIDs2` text,
  `DisplayFlag` int(10) unsigned NOT NULL DEFAULT '0',
  `HasUnknown1InDisplayInfo` int(11) NOT NULL DEFAULT '0',
  `HasBattlePayDisplayInfo` int(11) NOT NULL DEFAULT '0',
  `ChoiceType` int(11) NOT NULL DEFAULT '0',
  `Name` text COMMENT 'DisplayCard Title',
  PRIMARY KEY (`Entry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `battlepay_product_datas`
-- Represents a deliverable PRODUCT (what the player actually receives)
--

DROP TABLE IF EXISTS `battlepay_product_datas`;
CREATE TABLE `battlepay_product_datas` (
  `Entry` int(10) unsigned NOT NULL,
  `DeliverableID` int(10) unsigned NOT NULL DEFAULT '0',
  `Type` int(11) NOT NULL DEFAULT '0' COMMENT '1=Gold, 2=Pet, 3=Mount, 4=WoWToken, 5=NameChange, 6=FactionChange, 8=RaceChange, 11=CharTransfer, 12=Tender, 13=Illusion, 14=Toy/Effect, 18=GameTime, 20=Subscription, 26=Transmog',
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
  `Items` text COMMENT 'Colon-separated item entries. Format: ID,UnknownByte,ItemID,Quantity,UnknownInt1,UnknownInt2,IsPet,HasPetResult,PetResultFlags,HasVisualMetadata',
  PRIMARY KEY (`Entry`)
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
-- IDA-verified field order: ShopFlags, Ordering, ProductID, GroupID, ShopListingID, Field20, Flag
--

DROP TABLE IF EXISTS `battlepay_shop_datas`;
CREATE TABLE `battlepay_shop_datas` (
  `Entry` int(10) unsigned NOT NULL,
  `ShopFlags` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Shop entry flags',
  `GroupID` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'FK to groups.GroupID',
  `ProductID` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'FK to product_infos.ShopListingID',
  `Ordering` int(10) unsigned NOT NULL DEFAULT '0',
  `ShopListingID` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The shop listing ID',
  `Field20` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `HasBattlePayDisplayInfo` int(11) NOT NULL DEFAULT '0',
  `Unknown` int(11) NOT NULL DEFAULT '0',
  `DisplayFlag` int(10) unsigned NOT NULL DEFAULT '0',
  `Name` text COMMENT 'DisplayCard Title',
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
