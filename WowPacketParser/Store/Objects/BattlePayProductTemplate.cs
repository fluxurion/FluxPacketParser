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

        [DBFieldName("CardType")]
        public int CardType;

        [DBFieldName("Unk1")]
        public int Unk1;

        [DBFieldName("ProductMultiplier")]
        public int ProductMultiplier;

        [DBFieldName("IconFileDataID")]
        public int IconFileDataID;

        [DBFieldName("UIModelSceneID")]
        public int UIModelSceneID;

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

        [DBFieldName("Unk2")]
        public int Unk2;

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
        
        [DBFieldName("Unk3")]
        public int Unk3;
        
        [DBFieldName("Unk4")]
        public int Unk4;
        
        [DBFieldName("Unk5")]
        public int Unk5;
        
        [DBFieldName("Unk6")]
        public int Unk6;

        [DBFieldName("DeliverableProductIDs")]
        public string DeliverableProductIDs;

        [DBFieldName("DisplayFlag")]
        public uint DisplayFlag;

        [DBFieldName("HasUnk1InDisplayInfo")]
        public int HasUnk1InDisplayInfo;

        [DBFieldName("HasBattlePayDisplayInfo")]
        public int HasBattlePayDisplayInfo;

        [DBFieldName("Unk7")]
        public int Unk7;

        [DBFieldName("ParentProductID")]
        public int ParentProductID;

        [DBFieldName("Unk8")]
        public int Unk8;
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

        [DBFieldName("Unk9")]
        public uint Unk9;

        [DBFieldName("Unk10")]
        public int Unk10;

        [DBFieldName("ItemID")]
        public uint ItemID;

        [DBFieldName("Amount")]
        public uint Amount;

        [DBFieldName("MountSpellID")]
        public uint MountSpellID;

        [DBFieldName("BattlePetSpeciesCreatureID")]
        public uint BattlePetSpeciesCreatureID;

        [DBFieldName("Unk11")]
        public uint Unk11;

        [DBFieldName("Unk12")]
        public uint Unk12;

        [DBFieldName("Unk13")]
        public uint Unk13;

        [DBFieldName("TransmogSetID")]
        public uint TransmogSetID;

        [DBFieldName("Unk14")]
        public uint Unk14;

        [DBFieldName("Unk15")]
        public uint Unk15;

        [DBFieldName("HasDisplayInfo")]
        public int HasDisplayInfo;

        [DBFieldName("PetResultVariable")]
        public uint PetResultVariable;

        [DBFieldName("Name")]
        public string Name;

        [DBFieldName("Unk19")]
        public int Unk19;

        [DBFieldName("DisplayFlag")]
        public uint DisplayFlag;
    }

    [DBTableName("battlepay_shop_datas")]
    public sealed record BattlePayShop : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("ShopEntry")]
        public uint ShopEntry;

        [DBFieldName("GroupID")]
        public uint GroupID;

        [DBFieldName("ProductInfoID")]
        public uint ProductInfoID;

        [DBFieldName("Ordering")]
        public int Ordering;

        [DBFieldName("VasServiceType")]
        public int VasServiceType;

        [DBFieldName("StoreDeliveryType")]
        public int StoreDeliveryType;

        [DBFieldName("HasBattlePayDisplayInfo")]
        public int HasBattlePayDisplayInfo;

        [DBFieldName("Unk21")]
        public int Unk21;

        [DBFieldName("Unk22")]
        public int Unk22;

        [DBFieldName("DisplayFlag")]
        public uint DisplayFlag;
    }
}