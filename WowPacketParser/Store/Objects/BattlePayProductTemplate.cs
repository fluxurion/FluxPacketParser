using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("battlepay_display_infos")]
    public sealed record BattlePayDisplayInfo : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("ProductInfoID", true)]
        public uint ProductInfoID;

        [DBFieldName("ProductDataID", true)]
        public uint ProductDataID;

        [DBFieldName("ShopDataID", true)]
        public uint ShopDataID;

        [DBFieldName("HasIconFileDataID")]
        public int HasIconFileDataID;

        [DBFieldName("HasPreview")]
        public int HasPreview;

        [DBFieldName("HasIconBorder")]
        public int HasIconBorder;

        [DBFieldName("HasUnknown1")]
        public int HasUnknown1;

        [DBFieldName("HasUiTextureAtlasMemberID")]
        public int HasUiTextureAtlasMemberID;

        [DBFieldName("HasUiTextureAtlasMemberID2")]
        public int HasUiTextureAtlasMemberID2;

        [DBFieldName("VisualCount")]
        public uint VisualCount;

        [DBFieldName("CardType")]
        public int CardType;

        [DBFieldName("Unknown3")]
        public int Unknown3;

        [DBFieldName("ProductMultiplier")]
        public int ProductMultiplier;

        [DBFieldName("IconFileDataID")]
        public uint IconFileDataID;

        [DBFieldName("UIModelSceneID")]
        public uint UIModelSceneID;

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

        [DBFieldName("IconBorder")]
        public int IconBorder;

        [DBFieldName("Unknown1")]
        public int Unknown1;

        [DBFieldName("UiTextureAtlasMemberID")]
        public int UiTextureAtlasMemberID;

        [DBFieldName("UiTextureAtlasMemberID2")]
        public int UiTextureAtlasMemberID2;

        [DBFieldName("Description4")]
        public string Description4;

        [DBFieldName("Description5")]
        public string Description5;

        [DBFieldName("PreviewCreatureDisplayIDs")]
        public string PreviewCreatureDisplayIDs;

        [DBFieldName("PreviewUIModelSceneIDs")]
        public string PreviewUIModelSceneIDs;

        [DBFieldName("PreviewTransmogSets")]
        public string PreviewTransmogSets;

        [DBFieldName("PreviewTitles")]
        public string PreviewTitles;

    }

    [DBTableName("battlepay_display_info_visuals")]
    public sealed record BattlePayDisplayInfoVisual : IDataModel
    {
        [DBFieldName("DisplayInfoEntry", true)]
        public uint DisplayInfoEntry;

        [DBFieldName("VisualIndex", true)]
        public uint VisualIndex;

        [DBFieldName("CreatureDisplayID")]
        public uint CreatureDisplayID;

        [DBFieldName("PreviewUIModelSceneID")]
        public uint PreviewUIModelSceneID;

        [DBFieldName("TransmogSetID")]
        public uint TransmogSetID;

        [DBFieldName("VisualName")]
        public string VisualName;
    }

    [DBTableName("battlepay_product_infos")]
    public sealed record BattlePayProductInfo : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("ProductInfoID")]
        public uint ProductInfoID;

        [DBFieldName("NormalPrice")]
        public long NormalPrice;

        [DBFieldName("CurrentPrice")]
        public long CurrentPrice;

        [DBFieldName("ProductInfoFlags")]
        public int ProductInfoFlags;

        [DBFieldName("Unknown1")]
        public int Unknown1;
        
        [DBFieldName("Unknown2")]
        public int Unknown2;
        
        [DBFieldName("Unknown3")]
        public int Unknown3;
        
        [DBFieldName("Unknown4")]
        public int Unknown4;
        
        [DBFieldName("Unknown5")]
        public int Unknown5;

        [DBFieldName("DeliverableProductIDExtra")]
        public uint DeliverableProductIDExtra;

        [DBFieldName("Unk1027")]
        public uint Unk1027;

        [DBFieldName("UnknownIfFlags1_1")]
        public int UnknownIfFlags1_1;

        [DBFieldName("UnknownIfFlags1_2")]
        public int UnknownIfFlags1_2;

        [DBFieldName("UnknownIfFlags2_1")]
        public int UnknownIfFlags2_1;

        [DBFieldName("UnknownIfFlags2_2")]
        public int UnknownIfFlags2_2;

        [DBFieldName("UnknownIfFlags2_3")]
        public int UnknownIfFlags2_3;

        [DBFieldName("UnknownIfFlags2_4")]
        public int UnknownIfFlags2_4;

        [DBFieldName("HasVisualMetadata")]
        public int HasVisualMetadata;

        [DBFieldName("DeliverableProductIDs")]
        public string DeliverableProductIDs;

        [DBFieldName("DisplayFlag")]
        public uint DisplayFlag;

        [DBFieldName("HasUnknown1InDisplayInfo")]
        public int HasUnknown1InDisplayInfo;

        [DBFieldName("HasBattlePayDisplayInfo")]
        public int HasBattlePayDisplayInfo;

        [DBFieldName("ChoiceType")]
        public int ChoiceType;
    }

    [DBTableName("battlepay_product_datas")]
    public sealed record BattlePayProduct : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("ProductID")]
        public uint ProductID;

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

        [DBFieldName("Unknown1")]
        public uint Unknown1;

        [DBFieldName("Unknown2")]
        public uint Unknown2;

        [DBFieldName("Unknown3")]
        public uint Unknown3;

        [DBFieldName("TransmogSetID")]
        public uint TransmogSetID;

        [DBFieldName("Unknown8")]
        public uint Unknown8;

        [DBFieldName("Unknown9")]
        public uint Unknown9;

        [DBFieldName("Unknown10")]
        public uint Unknown10;

        [DBFieldName("Unknown11")]
        public uint Unknown11;

        [DBFieldName("HasDisplayInfo")]
        public int HasDisplayInfo;

        [DBFieldName("PetResultVariable")]
        public uint PetResultVariable;

        [DBFieldName("Name")]
        public string Name;

        [DBFieldName("AlreadyOwned")]
        public int AlreadyOwned;

        [DBFieldName("DisplayFlag")]
        public uint DisplayFlag;
    }

    [DBTableName("battlepay_product_items")]
    public sealed record BattlePayProductItem : IDataModel
    {
        [DBFieldName("ProductEntry", true)]
        public uint ProductEntry;

        [DBFieldName("ItemOrder", true)]
        public uint ItemOrder;

        [DBFieldName("ID")]
        public uint ID;

        [DBFieldName("UnknownByte")]
        public byte UnknownByte;

        [DBFieldName("ItemID")]
        public uint ItemID;

        [DBFieldName("Quantity")]
        public uint Quantity;

        [DBFieldName("UnknownInt1")]
        public uint UnknownInt1;

        [DBFieldName("UnknownInt2")]
        public uint UnknownInt2;

        [DBFieldName("IsPet")]
        public int IsPet;

        [DBFieldName("HasPetResult")]
        public int HasPetResult;

        [DBFieldName("PetResultFlags")]
        public uint PetResultFlags;

        [DBFieldName("HasVisualMetadata")]
        public int HasVisualMetadata;
    }

    [DBTableName("battlepay_groups")]
    public sealed record BattlePayGroup_ : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("GroupID")]
        public uint GroupID;

        [DBFieldName("IconFileDataID")]
        public uint IconFileDataID;

        [DBFieldName("DisplayType")]
        public byte DisplayType;

        [DBFieldName("Ordering")]
        public uint Ordering;

        [DBFieldName("Unknown")]
        public uint Unknown;

        [DBFieldName("MainGroupID")]
        public uint MainGroupID;

        [DBFieldName("Name")]
        public string Name;

        [DBFieldName("Description")]
        public string Description;
    }

    [DBTableName("battlepay_shop_datas")]
    public sealed record BattlePayShop : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("EntryID")]
        public uint EntryID;

        [DBFieldName("GroupID")]
        public uint GroupID;

        [DBFieldName("ProductID")]
        public uint ProductID;

        [DBFieldName("Ordering")]
        public uint Ordering;

        [DBFieldName("VasServiceType")]
        public uint VasServiceType;

        [DBFieldName("StoreDeliveryType")]
        public byte StoreDeliveryType;

        [DBFieldName("HasBattlePayDisplayInfo")]
        public int HasBattlePayDisplayInfo;

        [DBFieldName("Unknown")]
        public int Unknown;

        [DBFieldName("DisplayFlag")]
        public uint DisplayFlag;
    }
}