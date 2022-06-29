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
    }
}