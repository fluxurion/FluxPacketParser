using System;
using WowPacketParser.Misc;
using WowPacketParser.SQL;

namespace WowPacketParser.Store.Objects
{
    [DBTableName("reference_loot_template")]
    public class ReferenceLootTemplate : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("Item", true)]
        public uint Item;

        [DBFieldName("Reference", true)]
        public int Reference;

        [DBFieldName("Chance")]
        public float Chance;

        [DBFieldName("QuestRequired")]
        public bool QuestRequired;

        [DBFieldName("LootMode")]
        public ushort LootMode;

        [DBFieldName("GroupId")]
        public byte GroupId;

        [DBFieldName("MinCount")]
        public byte MinCount;

        [DBFieldName("MaxCount")]
        public byte MaxCount;

        [DBFieldName("Comment")]
        public string Comment;

        [DBFieldName("VerifiedBuild")]
        public int VerifiedBuild = ClientVersion.BuildInt;

        public TimeSpan? TimeSpan;
    }

    [DBTableName("scrapping_loot_template")]
    public class ScrappingLootTemplate : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("Item", true)]
        public uint Item;

        [DBFieldName("Reference", true)]
        public int Reference;

        [DBFieldName("Chance")]
        public float Chance;

        [DBFieldName("QuestRequired")]
        public bool QuestRequired;

        [DBFieldName("LootMode")]
        public ushort LootMode;

        [DBFieldName("GroupId")]
        public byte GroupId;

        [DBFieldName("MinCount")]
        public byte MinCount;

        [DBFieldName("MaxCount")]
        public byte MaxCount;

        [DBFieldName("Comment")]
        public string Comment;

        [DBFieldName("VerifiedBuild")]
        public int VerifiedBuild = ClientVersion.BuildInt;

        public TimeSpan? TimeSpan;
    }

    [DBTableName("item_loot_template")]
    public class ItemLootTemplate : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("Item", true)]
        public uint Item;

        [DBFieldName("Reference", true)]
        public int Reference;

        [DBFieldName("Chance")]
        public float Chance;

        [DBFieldName("QuestRequired")]
        public bool QuestRequired;

        [DBFieldName("LootMode")]
        public ushort LootMode;

        [DBFieldName("GroupId")]
        public byte GroupId;

        [DBFieldName("MinCount")]
        public byte MinCount;

        [DBFieldName("MaxCount")]
        public byte MaxCount;

        [DBFieldName("Comment")]
        public string Comment;

        [DBFieldName("VerifiedBuild")]
        public int VerifiedBuild = ClientVersion.BuildInt;

        public TimeSpan? TimeSpan;
    }

    [DBTableName("prospecting_loot_template")]
    public class ProspectingLootTemplate : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("Item", true)]
        public uint Item;

        [DBFieldName("Reference", true)]
        public int Reference;

        [DBFieldName("Chance")]
        public float Chance;

        [DBFieldName("QuestRequired")]
        public bool QuestRequired;

        [DBFieldName("LootMode")]
        public ushort LootMode;

        [DBFieldName("GroupId")]
        public byte GroupId;

        [DBFieldName("MinCount")]
        public byte MinCount;

        [DBFieldName("MaxCount")]
        public byte MaxCount;

        [DBFieldName("Comment")]
        public string Comment;

        [DBFieldName("VerifiedBuild")]
        public int VerifiedBuild = ClientVersion.BuildInt;

        public TimeSpan? TimeSpan;
    }

    [DBTableName("milling_loot_template")]
    public class MillingLootTemplate : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("Item", true)]
        public uint Item;

        [DBFieldName("Reference", true)]
        public int Reference;

        [DBFieldName("Chance")]
        public float Chance;

        [DBFieldName("QuestRequired")]
        public bool QuestRequired;

        [DBFieldName("LootMode")]
        public ushort LootMode;

        [DBFieldName("GroupId")]
        public byte GroupId;

        [DBFieldName("MinCount")]
        public byte MinCount;

        [DBFieldName("MaxCount")]
        public byte MaxCount;

        [DBFieldName("Comment")]
        public string Comment;

        [DBFieldName("VerifiedBuild")]
        public int VerifiedBuild = ClientVersion.BuildInt;

        public TimeSpan? TimeSpan;
    }

    [DBTableName("spell_loot_template")]
    public class SpellLootTemplate : IDataModel
    {
        [DBFieldName("Entry", true)]
        public uint Entry;

        [DBFieldName("Item", true)]
        public uint Item;

        [DBFieldName("Reference", true)]
        public int Reference;

        [DBFieldName("Chance")]
        public float Chance;

        [DBFieldName("QuestRequired")]
        public bool QuestRequired;

        [DBFieldName("LootMode")]
        public ushort LootMode;

        [DBFieldName("GroupId")]
        public byte GroupId;

        [DBFieldName("MinCount")]
        public byte MinCount;

        [DBFieldName("MaxCount")]
        public byte MaxCount;

        [DBFieldName("Comment")]
        public string Comment;

        [DBFieldName("VerifiedBuild")]
        public int VerifiedBuild = ClientVersion.BuildInt;

        public TimeSpan? TimeSpan;
    }
}
