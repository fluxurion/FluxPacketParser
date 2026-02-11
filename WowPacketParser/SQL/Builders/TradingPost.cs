using System.Linq;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Store;

namespace WowPacketParser.SQL.Builders
{
    [BuilderClass]
    public static class TradingPost
    {
        [BuilderMethod]
        public static string TradingPostData()
        {
            if (Storage.PerksProgramVendorDatas.IsEmpty())
                return string.Empty;

            if (!Settings.SQLOutputFlag.HasAnyFlagBit(SQLOutput.perks_program_vendor_data))
                return string.Empty;

            var templateDb = SQLDatabase.Get(Storage.PerksProgramVendorDatas);

            return SQLUtil.Compare(Settings.SQLOrderByKey ? Storage.PerksProgramVendorDatas.OrderBy(x => x.Item1.Entry).ToArray() : Storage.PerksProgramVendorDatas.ToArray(), templateDb, x => string.Empty);
        }
    }
}