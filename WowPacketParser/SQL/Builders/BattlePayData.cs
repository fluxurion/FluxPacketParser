using System.Linq;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Store;

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

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_display_infos))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayDisplayInfos);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayDisplayInfos.OrderBy(x => x.Item1.Entry).ToArray() : Storage.BattlePayDisplayInfos.ToArray(), templateDb, x => string.Empty);
        }
        [BuilderMethod]
        public static string BattlePayProductInfoData()
        {
            if (Storage.BattlePayProductInfos.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_product_infos))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayProductInfos);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayProductInfos.OrderBy(x => x.Item1.Entry).ToArray() : Storage.BattlePayProductInfos.ToArray(), templateDb, x => string.Empty);
        }
        [BuilderMethod]
        public static string BattlePayProductData()
        {
            if (Storage.BattlePayProductDatas.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_product_datas))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayProductDatas);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayProductDatas.OrderBy(x => x.Item1.Entry).ToArray() : Storage.BattlePayProductDatas.ToArray(), templateDb, x => string.Empty);
        }
        [BuilderMethod]
        public static string BattlePayShopData()
        {
            if (Storage.BattlePayShopDatas.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_shop_datas))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayShopDatas);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayShopDatas.OrderBy(x => x.Item1.Entry).ToArray() : Storage.BattlePayShopDatas.ToArray(), templateDb, x => string.Empty);
        }

        [BuilderMethod]
        public static string BattlePayGroupData()
        {
            if (Storage.BattlePayGroups.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_groups))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayGroups);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayGroups.OrderBy(x => x.Item1.GroupID).ToArray() : Storage.BattlePayGroups.ToArray(), templateDb, x => string.Empty);
        }
    }
}