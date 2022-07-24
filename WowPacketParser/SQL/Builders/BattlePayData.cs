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
        public static string BattlePayDisplayInfoData()
        {
            if (Storage.BattlePayDisplayInfos.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_displayinfo))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayDisplayInfos);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayDisplayInfos.OrderBy(x => x.Item1.Entry).ToArray() : Storage.BattlePayDisplayInfos.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayVisualData()
        {
            if (Storage.BattlePayVisuals.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_visual))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayVisuals);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayVisuals.OrderBy(x => x.Item1.VisualId).ToArray() : Storage.BattlePayVisuals.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayProductInfoData()
        {
            if (Storage.BattlePayProductInfos.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_productinfo))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayProductInfos);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayProductInfos.OrderBy(x => x.Item1.Entry).ToArray() : Storage.BattlePayProductInfos.ToArray(), templateDb, x => string.Empty);
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
        public static string BattlePayItemData()
        {
            if (Storage.BattlePayItems.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_item))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayItems);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayItems.OrderBy(x => x.Item1.ID).ToArray() : Storage.BattlePayItems.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayGroupData()
        {
            if (Storage.BattlePayGroups.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_group))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayGroups);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayGroups.OrderBy(x => x.Item1.GroupID).ToArray() : Storage.BattlePayGroups.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayShopData()
        {
            if (Storage.BattlePayShops.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_shop))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayShops);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayShops.OrderBy(x => x.Item1.Entry).ToArray() : Storage.BattlePayShops.ToArray(), templateDb, x => string.Empty);
        }
    }
}