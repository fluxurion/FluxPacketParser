using System.Linq;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Store;

namespace WowPacketParser.SQL.Builders
{
    [BuilderClass]
    public static class CraftingLootData
    {
        [BuilderMethod]
        public static string ReferenceLootTemplateData()
        {
            if (Storage.ReferenceLootTemplates.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.reference_loot_template))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.ReferenceLootTemplates);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.ReferenceLootTemplates.OrderBy(x => x.Item1.Entry).ThenBy(x => x.Item1.Item).ToArray() : Storage.ReferenceLootTemplates.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string ScrappingLootTemplateData()
        {
            if (Storage.ScrappingLootTemplates.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.scrapping_loot_template))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.ScrappingLootTemplates);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.ScrappingLootTemplates.OrderBy(x => x.Item1.Entry).ThenBy(x => x.Item1.Item).ToArray() : Storage.ScrappingLootTemplates.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string ItemLootTemplateData()
        {
            if (Storage.ItemLootTemplates.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.item_loot_template))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.ItemLootTemplates);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.ItemLootTemplates.OrderBy(x => x.Item1.Entry).ThenBy(x => x.Item1.Item).ToArray() : Storage.ItemLootTemplates.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string ProspectingLootTemplateData()
        {
            if (Storage.ProspectingLootTemplates.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.prospecting_loot_template))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.ProspectingLootTemplates);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.ProspectingLootTemplates.OrderBy(x => x.Item1.Entry).ThenBy(x => x.Item1.Item).ToArray() : Storage.ProspectingLootTemplates.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string MillingLootTemplateData()
        {
            if (Storage.MillingLootTemplates.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.milling_loot_template))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.MillingLootTemplates);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.MillingLootTemplates.OrderBy(x => x.Item1.Entry).ThenBy(x => x.Item1.Item).ToArray() : Storage.MillingLootTemplates.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string SpellLootTemplateData()
        {
            if (Storage.SpellLootTemplates.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.spell_loot_template))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.SpellLootTemplates);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.SpellLootTemplates.OrderBy(x => x.Item1.Entry).ThenBy(x => x.Item1.Item).ToArray() : Storage.SpellLootTemplates.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string TreasureLootTemplateData()
        {
            if (Storage.TreasureLootTemplates.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.treasure_loot_template))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.TreasureLootTemplates);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.TreasureLootTemplates.OrderBy(x => x.Item1.TreasureID).ThenBy(x => x.Item1.Item).ToArray() : Storage.TreasureLootTemplates.ToArray(), templateDb, x => string.Empty);
        }
    }
}
