using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_product_template")]
    public sealed record BattlePayProductTemplate : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;
        
        [DBFieldName("ProductInfoID", true)]
        public int ProductInfoID;

        [DBFieldName("IconFileDataID", true)]
        public int IconFileDataID;

        [DBFieldName("UIModelSceneID", true)]
        public int UIModelSceneID;

        [DBFieldName("PreviewTitles", true)]
        public string PreviewTitles;

        [DBFieldName("PreviewCreatureDisplayIDs", true)]
        public string PreviewCreatureDisplayIDs;

        [DBFieldName("PreviewUIModelSceneIDs", true)]
        public string PreviewUIModelSceneIDs;

        [DBFieldName("PreviewTransmogSets", true)]
        public string PreviewTransmogSets;

        [DBFieldName("Title")]
        public string Title;

        [DBFieldName("Title2")]
        public string Title2;

        [DBFieldName("Description")]
        public string Description;

        [DBFieldName("Description2")]
        public string Description2;

        [DBFieldName("Description3")]
        public string Description3;

        [DBFieldName("Description4")]
        public string Description4;

        [DBFieldName("Description5")]
        public string Description5;

        [DBFieldName("IconBorder", true)]
        public int IconBorder;

        [DBFieldName("UiTextureAtlasMemberID", true)]
        public int UiTextureAtlasMemberID;

        [DBFieldName("DisplayInfoFlag", true)]
        public uint DisplayInfoFlag;

        [DBFieldName("CardType", true)]
        public int CardType;

        [DBFieldName("ProductMultiplier", true)]
        public int ProductMultiplier;

        [DBFieldName("Type", true)]
        public int Type;

        [DBFieldName("ItemID", true)]
        public uint ItemID;

        [DBFieldName("ItemCount", true)]
        public uint ItemCount;

        [DBFieldName("MountSpellID", true)]
        public uint MountSpellID;

        [DBFieldName("BattlePetSpeciesCreatureID", true)]
        public uint BattlePetSpeciesCreatureID;

        [DBFieldName("TransmogSetID", true)]
        public uint TransmogSetID;

        [DBFieldName("AlreadyOwned", true)]
        public int AlreadyOwned;

        [DBFieldName("NormalPrice", true)]
        public long NormalPrice;

        [DBFieldName("CurrentPrice", true)]
        public long CurrentPrice;

        [DBFieldName("BundleProductIDs", true)]
        public string BundleProductIDs;

        [DBFieldName("Flags", true)]
        public int Flags;

        [DBFieldName("ChoiceType", true)]
        public uint ChoiceType;

        [DBFieldName("GroupID", true)]
        public uint GroupID;

        [DBFieldName("Ordering", true)]
        public int Ordering;

        [DBFieldName("VasServiceType", true)]
        public int VasServiceType;

        [DBFieldName("StoreDeliveryType", true)]
        public int StoreDeliveryType;
    }
}
