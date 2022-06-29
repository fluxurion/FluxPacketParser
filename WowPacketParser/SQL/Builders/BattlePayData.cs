using System;
using System.Collections.Generic;
using System.Linq;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParser.SQL.Builders
{
    [BuilderClass]
    public static class BattlePayData
    {
        [BuilderMethod]
        public static string BattlePayProductGroupData()
        {
            if (Storage.BattlePayProductGroups.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_product_group))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayProductGroups);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayProductGroups.OrderBy(x => x.Item1.GroupID).ToArray() : Storage.BattlePayProductGroups.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayDisplayInfoData()
        {
            if (Storage.BattlePayDisplayInfos.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_display_info))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayDisplayInfos);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayDisplayInfos.OrderBy(x => x.Item1.DisplayInfoId).ToArray() : Storage.BattlePayDisplayInfos.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayDisplayInfoVisualData()
        {
            if (Storage.BattlePayDisplayInfoVisuals.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_display_info_visuals))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayDisplayInfoVisuals);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayDisplayInfoVisuals.OrderBy(x => x.Item1.DisplayInfoId).ToArray() : Storage.BattlePayDisplayInfoVisuals.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayProductData()
        {
            if (Storage.BattlePayProducts.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_product))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayProducts);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayProducts.OrderBy(x => x.Item1.ProductID).ToArray() : Storage.BattlePayProducts.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayProductItemData()
        {
            if (Storage.BattlePayProductItems.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_product_item))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayProductItems);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayProductItems.OrderBy(x => x.Item1.ID).ToArray() : Storage.BattlePayProductItems.ToArray(), templateDb, x => string.Empty);
        }
    }
}