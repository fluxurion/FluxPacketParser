using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V12_0_0_65390.Parsers
{
    public static class FluxCraftingHandler
    {
        public static void ReadCraftingOptionalIDs(Packet packet, params object[] indexes)
        {
            var hasItemID = packet.ReadBit("HasItemID", indexes);
            var hasDataSlotIndex = packet.ReadBit("HasDataSlotIndex", indexes);
            packet.ResetBitReader();

            if (hasItemID)
                packet.ReadInt32<ItemId>("ItemID", indexes);

            if (hasDataSlotIndex)
                packet.ReadInt32("DataSlotIndex", indexes);
        }

        public static void ReadCraftingItem(Packet packet, params object[] indexes)
        {
            packet.ReadInt32<ItemId>("ItemID", indexes);
            packet.ReadInt32("Quantity", indexes);

            ReadCraftingOptionalIDs(packet, indexes, "OptionalIDs");

            var hasReagentQuality = packet.ReadBit("HasReagentQuality", indexes);
            packet.ResetBitReader();

            if (hasReagentQuality)
                packet.ReadByte("ReagentQuality", indexes);
        }

        [Parser(Opcode.CMSG_CRAFTING_ORDER_CREATE)]
        public static void HandleCraftingOrderCreate(Packet packet)
        {
            packet.ReadInt32("SkillLineAbilityID");
            var orderType = packet.ReadByte("OrderType");
            packet.ReadByte("Duration");
            packet.ReadInt64("TipAmount");
            var count1 = packet.ReadInt32("Count1");
            var count2 = packet.ReadInt32("Count2");
            var count3 = packet.ReadInt32("Count3");
            var count4 = packet.ReadInt32("Count4");
            packet.ReadPackedGuid128("CustomerAccountGUID");

            if (orderType == 1 || orderType == 2)
                packet.ReadInt32("Price");

            var customerNoteLength = packet.ReadBits(10);
            var hasCustomerGUID = packet.ReadBit("HasCustomerGUID");
            var hasLegacyInternalNote = packet.ReadBit("HasLegacyInternalNote");

            uint internalNoteLength = 0;
            if (orderType == 2)
                internalNoteLength = packet.ReadBits(9);

            packet.ReadBit("UnknownBit");
            packet.ResetBitReader();

            for (var i = 0; i < count1; ++i)
            {
                packet.ReadInt32("Int1056_1", i);
                packet.ReadInt32("Int1056_2", i);
                if (packet.ReadBit("HasByte1056", i))
                {
                    packet.ResetBitReader();
                    packet.ReadByte("Byte1056", i);
                }
                packet.ResetBitReader();
            }

            if (customerNoteLength > 0)
                packet.ReadWoWString("CustomerNote", customerNoteLength);
            packet.ResetBitReader();

            if (hasCustomerGUID)
            {
                packet.ReadPackedGuid128("CustomerGUID");
                packet.ResetBitReader();
            }

            if (orderType == 2 && internalNoteLength > 0)
                packet.ReadWoWString("InternalNote", internalNoteLength);
            packet.ResetBitReader();

            for (var i = 0; i < count2; ++i)
            {
                packet.ReadInt32("Int1080_1", i);
                packet.ReadInt32("Int1080_2", i);
                if (packet.ReadBit("HasByte1080", i))
                {
                    packet.ResetBitReader();
                    packet.ReadByte("Byte1080", i);
                }
                packet.ResetBitReader();
            }

            for (var i = 0; i < count3; ++i)
            {
                ReadCraftingItem(packet, "Items1", i);
                packet.ResetBitReader();
            }

            for (var i = 0; i < count4; ++i)
            {
                ReadCraftingItem(packet, "Items2", i);
                packet.ResetBitReader();
            }

            if (hasLegacyInternalNote)
            {
                packet.ResetBitReader();
                var legacyNoteLen = packet.ReadBits(11);
                packet.ResetBitReader();
                packet.ReadWoWString("LegacyInternalNote", legacyNoteLen);
            }
        }

        public static void ReadCraftingOrderRecraftInfo(Packet packet, params object[] indexes)
        {
            packet.ReadByte("RecraftByte", indexes);
            packet.ReadInt32("RecraftInt", indexes);

            packet.ReadBit("RecraftBit1", indexes);
            packet.ReadBit("RecraftBit2", indexes);
            packet.ReadBit("RecraftBit3", indexes);
            packet.ReadBit("RecraftBit4", indexes);

            packet.ReadBit("RecraftUnknownBit", indexes);
            packet.ResetBitReader();
        }

        [Parser(Opcode.CMSG_CRAFTING_ORDER_LIST_CRAFTER_ORDERS)]
        public static void HandleCraftingOrderListCrafterOrders(Packet packet)
        {
            packet.ReadPackedGuid128("CrafterGUID");
            packet.ReadInt32("UnknownID");
            packet.ReadByte("OrderType");
            packet.ReadByte("Duration");
            var recipeCount = packet.ReadInt32("RecipeCount");

            var displayID = packet.ReadBits("DisplayID", 2);
            var hasRecraftInfo = packet.ReadBit("HasRecraftInfo");
            packet.ReadBit("UnknownBit");
            packet.ResetBitReader();

            for (var i = 0; i < recipeCount; ++i)
            {
                packet.ReadBits("SkillLineAbilityID", 20, i);
                packet.ReadBit("ItemUnknownBit", i);
                packet.ResetBitReader();
            }

            ReadCraftingOrderRecraftInfo(packet, "RecraftInfo");

            for (var i = 0; i < displayID; ++i)
            {
                packet.ReadByte("ItemByte", i, "RecraftItems");
                packet.ReadBit("ItemBit", i, "RecraftItems");
                packet.ResetBitReader();
            }

            if (hasRecraftInfo)
                packet.ReadWoWString("CustomerNote", packet.ReadBits("StringLength", 11));

            packet.ReadInt16("UnknownEnd");
        }

        [Parser(Opcode.CMSG_CRAFTING_ORDER_LIST_MY_ORDERS)]
        public static void HandleCraftingOrderListMyOrders(Packet packet)
        {
            packet.ReadInt32("FixedValue"); // 3866902
            packet.ReadPackedGuid128("CrafterGUID");
            packet.ReadInt32("UnknownID");

            var displayID = packet.ReadBits("DisplayID", 2);
            var hasRecraftInfo = packet.ReadBit("HasRecraftInfo");
            packet.ReadBit("UnknownBit");
            packet.ResetBitReader();

            ReadCraftingOrderRecraftInfo(packet, "RecraftInfo");

            for (var i = 0; i < displayID; ++i)
            {
                packet.ReadByte("ItemByte", i, "RecraftItems");
                packet.ReadBit("ItemBit", i, "RecraftItems");
                packet.ResetBitReader();
            }

            if (hasRecraftInfo)
                packet.ReadWoWString("CustomerNote", packet.ReadBits("StringLength", 11));

            packet.ReadInt16("UnknownEnd");
        }

        public static void ReadJamSpellReducedReagent(Packet packet, params object[] indexes)
        {
            var flags = packet.ReadByte("Flags", indexes);
            var hasItemID = (flags & 0x80) != 0;
            var hasQuantity = (flags & 0x40) != 0;

            if (hasItemID)
                packet.ReadInt32<ItemId>("ItemID", indexes);

            if (hasQuantity)
                packet.ReadInt32("Quantity", indexes);
        }

        // Loot Template Parsing Methods for Crafting System
        public static void StoreReferenceLoot(uint entry, uint item, float chance, byte minCount, byte maxCount, string comment = "")
        {
            Storage.ReferenceLootTemplates.Add(new ReferenceLootTemplate
            {
                Entry = entry,
                Item = item,
                Reference = 0,
                Chance = chance,
                QuestRequired = false,
                LootMode = 1,
                GroupId = 0,
                MinCount = minCount,
                MaxCount = maxCount,
                Comment = comment
            });
        }

        public static void StoreScrappingLoot(uint entry, uint item, float chance, byte minCount, byte maxCount, string comment = "")
        {
            Storage.ScrappingLootTemplates.Add(new ScrappingLootTemplate
            {
                Entry = entry,
                Item = item,
                Reference = 0,
                Chance = chance,
                QuestRequired = false,
                LootMode = 1,
                GroupId = 0,
                MinCount = minCount,
                MaxCount = maxCount,
                Comment = comment
            });
        }

        public static void StoreItemLoot(uint entry, uint item, float chance, byte minCount, byte maxCount, string comment = "")
        {
            Storage.ItemLootTemplates.Add(new ItemLootTemplate
            {
                Entry = entry,
                Item = item,
                Reference = 0,
                Chance = chance,
                QuestRequired = false,
                LootMode = 1,
                GroupId = 0,
                MinCount = minCount,
                MaxCount = maxCount,
                Comment = comment
            });
        }

        public static void StoreProspectingLoot(uint entry, uint item, float chance, byte minCount, byte maxCount, string comment = "")
        {
            Storage.ProspectingLootTemplates.Add(new ProspectingLootTemplate
            {
                Entry = entry,
                Item = item,
                Reference = 0,
                Chance = chance,
                QuestRequired = false,
                LootMode = 1,
                GroupId = 0,
                MinCount = minCount,
                MaxCount = maxCount,
                Comment = comment
            });
        }

        public static void StoreMillingLoot(uint entry, uint item, float chance, byte minCount, byte maxCount, string comment = "")
        {
            Storage.MillingLootTemplates.Add(new MillingLootTemplate
            {
                Entry = entry,
                Item = item,
                Reference = 0,
                Chance = chance,
                QuestRequired = false,
                LootMode = 1,
                GroupId = 0,
                MinCount = minCount,
                MaxCount = maxCount,
                Comment = comment
            });
        }

        public static void StoreSpellLoot(uint entry, uint item, float chance, byte minCount, byte maxCount, string comment = "")
        {
            Storage.SpellLootTemplates.Add(new SpellLootTemplate
            {
                Entry = entry,
                Item = item,
                Reference = 0,
                Chance = chance,
                QuestRequired = false,
                LootMode = 1,
                GroupId = 0,
                MinCount = minCount,
                MaxCount = maxCount,
                Comment = comment
            });
        }

        // Helper method to parse loot from crafting packets
        public static void ParseCraftingLoot(Packet packet, uint lootId, LootTemplateType lootType, params object[] indexes)
        {
            var itemCount = packet.ReadUInt32("LootItemCount", indexes);
            
            for (uint i = 0; i < itemCount; i++)
            {
                var itemId = packet.ReadUInt32<ItemId>("ItemID", indexes, i);
                var chance = packet.ReadSingle("Chance", indexes, i);
                var minCount = packet.ReadByte("MinCount", indexes, i);
                var maxCount = packet.ReadByte("MaxCount", indexes, i);

                switch (lootType)
                {
                    case LootTemplateType.Reference:
                        StoreReferenceLoot(lootId, itemId, chance, minCount, maxCount, $"Crafting loot from packet");
                        break;
                    case LootTemplateType.Scrapping:
                        StoreScrappingLoot(lootId, itemId, chance, minCount, maxCount, $"Salvaging loot from packet");
                        break;
                    case LootTemplateType.Item:
                        StoreItemLoot(lootId, itemId, chance, minCount, maxCount, $"Crafting loot from packet");
                        break;
                    case LootTemplateType.Prospecting:
                        StoreProspectingLoot(lootId, itemId, chance, minCount, maxCount, $"Prospecting loot from packet");
                        break;
                    case LootTemplateType.Milling:
                        StoreMillingLoot(lootId, itemId, chance, minCount, maxCount, $"Milling loot from packet");
                        break;
                    case LootTemplateType.Spell:
                        StoreSpellLoot(lootId, itemId, chance, minCount, maxCount, $"Spell loot from packet");
                        break;
                }
            }
        }

        public enum LootTemplateType
        {
            Reference,
            Scrapping,
            Item,
            Prospecting,
            Milling,
            Spell
        }

        public static void ReadJamItemEnchantments(Packet packet, params object[] indexes)
        {
            var val = packet.ReadByte("EnchantmentFlags", indexes);
            // A binárisban: v10 >> 2
            var count = val >> 2; 

            for (var i = 0; i < count; ++i)
            {
                packet.ReadByte("EnchantSlot", indexes, i);
                packet.ReadInt32("EnchantID", indexes, i);
            }
        }

        public static void ReadJamCraftBonusData(Packet packet, string label, params object[] indexes)
        {
            packet.ReadInt32("TargetItemID", label, indexes);

            var hasBonus = packet.ReadBit("HasBonus", label, indexes);
            packet.ResetBitReader();

            // JamItemEnchantments::Read always follows BonusFlags
            ReadJamItemEnchantments(packet, label, indexes);

            if (hasBonus)
            {
                packet.ReadByte("UnknownByte", label, indexes);
                var itemBonusCount = packet.ReadInt32("ItemBonusCount", label, indexes);
                for (var i = 0; i < itemBonusCount; ++i)
                    packet.ReadInt32("ItemBonusID", label, indexes, i);
            }
        }

        [Parser(Opcode.SMSG_RECRAFT_ITEM_RESULT )]
        [Parser(Opcode.SMSG_CRAFT_ENCHANT_RESULT)]
        public static void HandleRecraftItemAndCraftEnchantResult(Packet packet)
        {
            // 9x ReadInt32/Single (0..32 offset)
            packet.ReadInt32("CraftingQualityID");    // a1+0
            packet.ReadSingle("QualityProgress");     // a1+4 (Ez volt az InspirationBonus)
            packet.ReadInt32("SkillLineAbilityID");   // a1+8 (Ez volt a StatValueRaw)
            packet.ReadInt32("CraftingDataID");       // a1+12
            packet.ReadInt32("Multicraft");           // a1+16 (Int16)
            packet.ReadInt32("SkillFromReagents");    // a1+20 (Int20)
            packet.ReadInt32("Skill");                 // a1+24 (CraftingDetailsSkill)
            packet.ReadInt32("CritBonusSkill");        // a1+28 (SkillGain)
            packet.ReadSingle("ModSkillGain");         // a1+32 (InspirationChance)

            packet.ReadInt64("OrderID");               // a1+40 (OrderID/TransactionID)
            var resourcesReturnedCount = packet.ReadInt32("ResourcesReturnedCount"); // a1+56

            packet.ReadUInt32("OperationID");          // a1+80 (Ez volt a CraftingOrderID)
            packet.ReadPackedGuid128("ItemGUID");      // a1+88

            packet.ReadInt32("Quantity");              // a1+104 (Int104)
            packet.ReadInt32("EnchantID");             // a1+336 (Int336)
            
            // Itt a szerializáló kódban van egy kis eltérés az IDA-hoz képest. 
            // Ha az IDA-ban látsz még Int344, 348, 352-t, azok valószínűleg 
            // a szerver belső verziójához tartozó paddingek vagy elavult mezők.
            packet.ReadInt32("Unk344"); 
            packet.ReadInt32("Unk348"); 
            packet.ReadInt32("Unk352"); 

            // Bitek (a1+48-tól induló bitmask az IDA-ban)
            // A C++ kód data.WriteBit hívásai ide kerülnek:
            packet.ReadBit("IsCrit");                  // v12>=0x80
            packet.ReadBit("IsRecraft");               // v5>=0x80
            packet.ReadBit("IsInitialRecraft");        // 2*v5>=0x80
            packet.ReadBit("BonusCraft");              // 4*v5>=0x80
            packet.ResetBitReader();                   // FlushBits()

            // A tárgy adatok a bitek után jönnek
            ReadJamCraftBonusData(packet, "OldItem");  // ItemBefore
            ReadJamCraftBonusData(packet, "NewItem");  // ItemAfter

            // Reagensek
            for (var i = 0; i < resourcesReturnedCount; ++i)
            {
                ReadJamSpellReducedReagent(packet, "ResourcesReturned", i);
                packet.ReadInt32("ReagentUnkInt", i);
            }
        }
    }
}
