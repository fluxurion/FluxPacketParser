-- treasure_loot_template table definition
-- This table is used for storing first craft treasure loot data

DROP TABLE IF EXISTS `treasure_loot_template`;
CREATE TABLE `treasure_loot_template` (
  `TreasureID` INT UNSIGNED NOT NULL COMMENT 'FirstCraftTreasureID from CraftingData.db2',
  `Item` INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Item ID (for item rewards)',
  `Currency` INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Currency ID (for currency rewards)',
  `Chance` FLOAT NOT NULL DEFAULT 100 COMMENT 'Drop chance (100 = 100%)',
  `GroupID` INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Loot group identifier',
  `MinCount` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Minimum quantity',
  `MaxCount` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Maximum quantity',
  `Comment` VARCHAR(255) NULL DEFAULT '' COMMENT 'Optional comment',
  PRIMARY KEY (`TreasureID`, `Item`, `Currency`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='First Craft Treasure Loot Template';
