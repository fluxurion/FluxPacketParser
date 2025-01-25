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

        [DBFieldName("ProductID")]
        public uint ProductID;

        [DBFieldName("ProductInfoID")]
        public uint ProductInfoID;

        [DBFieldName("IconFileDataID")]
        public int IconFileDataID;

        [DBFieldName("UIModelSceneID")]
        public int UIModelSceneID;

        [DBFieldName("PreviewTitles")]
        public string PreviewTitles;

        [DBFieldName("PreviewCreatureDisplayIDs")]
        public string PreviewCreatureDisplayIDs;

        [DBFieldName("PreviewUIModelSceneIDs")]
        public string PreviewUIModelSceneIDs;

        [DBFieldName("PreviewTransmogSets")]
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

        [DBFieldName("IconBorder")]
        public int IconBorder;

        [DBFieldName("UiTextureAtlasMemberID")]
        public int UiTextureAtlasMemberID;

        [DBFieldName("DisplayInfoFlag")]
        public uint DisplayInfoFlag;

        [DBFieldName("CardType")]
        public int CardType;

        [DBFieldName("ProductMultiplier")]
        public int ProductMultiplier;

        [DBFieldName("Type")]
        public int Type;

        [DBFieldName("ItemID")]
        public uint ItemID;

        [DBFieldName("ItemCount")]
        public uint ItemCount;

        [DBFieldName("MountSpellID")]
        public uint MountSpellID;

        [DBFieldName("BattlePetSpeciesCreatureID")]
        public uint BattlePetSpeciesCreatureID;

        [DBFieldName("TransmogSetID")]
        public uint TransmogSetID;

        [DBFieldName("AlreadyOwned")]
        public int AlreadyOwned;

        [DBFieldName("NormalPrice")]
        public long NormalPrice;

        [DBFieldName("CurrentPrice")]
        public long CurrentPrice;

        [DBFieldName("BundleProductIDs")]
        public string BundleProductIDs;

        [DBFieldName("Flags")]
        public int Flags;

        [DBFieldName("ChoiceType")]
        public uint ChoiceType;

        [DBFieldName("GroupID")]
        public uint GroupID;

        [DBFieldName("Ordering")]
        public int Ordering;

        [DBFieldName("VasServiceType")]
        public int VasServiceType;

        [DBFieldName("StoreDeliveryType")]
        public int StoreDeliveryType;
    }
}
