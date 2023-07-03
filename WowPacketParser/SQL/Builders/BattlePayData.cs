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
        public static string BattlePayProductTemplateData()
        {
            if (Storage.BattlePayProductTemplates.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.battlepay_product_template))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.BattlePayProductTemplates);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.BattlePayProductTemplates.OrderBy(x => x.Item1.Entry).ToArray() : Storage.BattlePayProductTemplates.ToArray(), templateDb, x => string.Empty);
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
    }
}