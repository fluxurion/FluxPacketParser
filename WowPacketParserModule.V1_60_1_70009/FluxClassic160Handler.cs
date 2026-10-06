using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using CoreParsers = WowPacketParser.Parsing.Parsers;
using WowPacketParserModule.V5_5_0_61735.Parsers;

namespace WowPacketParserModule.V1_60_1_70009.Parsers
{
    // Classic 1.60.1.70009 packet layouts. Sourced from TrinityCoreLuaSol
    // (E:\TrinityCoreLuaSol) packet writers/readers — the 1.60 client speaks
    // the modern retail-style wire formats with Classic-specific deltas,
    // so the 1.15.x era handlers do not apply.
    public static class FluxClassic160Handler
    {
        // ===== Trainer =====

        // 0x460189 — TC TrainerList::Write, 33 bytes per spell
        // (client decoder rva 0x8162E0): {i32 SpellID, i32 unk, u32 MoneyCost,
        // u8 ReqLevel, u32 ReqSkillLine, u32 ReqSkillRank, i32 ReqAbility[3]}.
        [Parser(Opcode.SMSG_TRAINER_LIST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleTrainerList160(Packet packet)
        {
            Trainer trainer = new Trainer();

            WowGuid guid = packet.ReadPackedGuid128("TrainerGUID");
            float? discount = CoreParsers.NpcHandler.GetFactionVendorDiscount(guid);

            trainer.Type = packet.ReadSByteE<TrainerType>("TrainerType");
            trainer.Id = packet.ReadUInt32("TrainerID");

            var count = packet.ReadUInt32("Spells");
            for (var i = 0; i < count; ++i)
            {
                TrainerSpell trainerSpell = new TrainerSpell
                {
                    TrainerId = trainer.Id,
                    SpellId = packet.ReadUInt32<SpellId>("SpellID", i)
                };

                packet.ReadInt32("Unk160", i);

                uint moneyCost = packet.ReadUInt32("MoneyCost", i);
                uint moneyCostOriginal = moneyCost;

                if (Settings.UseDBC && Settings.RecalcDiscount && discount != null)
                {
                    moneyCostOriginal = (uint)(Math.Round((moneyCost / discount.Value) / 5)) * 5;
                    packet.WriteLine("[{0}] ReputationDiscount: {1}%", i, (int)(100 - (discount * 100)));
                    packet.WriteLine("[{0}] MoneyCostOriginal: {1}", i, moneyCostOriginal);
                    trainerSpell.FactionHelper = "MoneyCost recalculated";
                }
                else
                {
                    trainerSpell.FactionHelper = "No Faction found! MoneyCost not recalculated!";
                }

                trainerSpell.MoneyCost = moneyCostOriginal;
                trainerSpell.ReqLevel = packet.ReadByte("ReqLevel", i);
                trainerSpell.ReqSkillLine = packet.ReadUInt32("ReqSkillLine", i);
                trainerSpell.ReqSkillRank = packet.ReadUInt32("ReqSkillRank", i);

                trainerSpell.ReqAbility = new uint[3];
                for (var j = 0; j < 3; ++j)
                    trainerSpell.ReqAbility[j] = packet.ReadUInt32("ReqAbility", i, j);

                Storage.TrainerSpells.Add(trainerSpell, packet.TimeSpan);
            }

            packet.ResetBitReader();
            uint greetingLength = packet.ReadBits(11);
            trainer.Greeting = packet.ReadWoWString("Greeting", greetingLength);

            Storage.Trainers.Add(trainer, packet.TimeSpan);
            CoreParsers.NpcHandler.AddToCreatureTrainers(trainer.Id, packet.TimeSpan);
        }

        // ===== Guild =====

        // 0x52002C — TC QueryGuildInfoResponse::Write: retail field order with
        // an extra i32 after the five emblem ints (reader rva 0xA235E0).
        [Parser(Opcode.SMSG_QUERY_GUILD_INFO_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleGuildQueryResponse160(Packet packet)
        {
            packet.ReadPackedGuid128("Guild Guid");

            packet.ResetBitReader();
            var hasData = packet.ReadBit("HasData");
            packet.ResetBitReader();
            if (!hasData)
                return;

            packet.ReadPackedGuid128("GuildGUID");
            packet.ReadUInt32("VirtualRealmAddress");
            var rankCount = packet.ReadUInt32("RankCount");
            packet.ReadUInt32("EmblemStyle");
            packet.ReadUInt32("EmblemColor");
            packet.ReadUInt32("BorderStyle");
            packet.ReadUInt32("BorderColor");
            packet.ReadUInt32("BackgroundColor");
            packet.ReadInt32("Unk160"); // Classic 1.60: sixth emblem int32

            for (var i = 0u; i < rankCount; ++i)
            {
                packet.ReadUInt32("RankID", i);
                packet.ReadUInt32("RankOrder", i);
                packet.ResetBitReader();
                var rankNameLen = packet.ReadBits(7);
                packet.ReadWoWString("Rank Name", rankNameLen, i);
            }

            packet.ResetBitReader();
            var nameLen = packet.ReadBits(7);
            packet.ReadWoWString("Guild Name", nameLen);
        }

        // Classic 1.60 GuildRosterMemberData (member reader rva 0x8F5C10):
        // DungeonScore before GuildClubMemberID, i32 TimerunningSeasonID,
        // and a 9-bit surname length after the 6-bit name length.
        private static void ReadGuildRosterMemberData160(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("Guid", idx);

            packet.ReadInt32("RankID", idx);
            packet.ReadInt32("AreaID", idx);
            packet.ReadInt32("PersonalAchievementPoints", idx);
            packet.ReadInt32("GuildReputation", idx);

            packet.ReadSingle("LastSave", idx);

            for (var j = 0; j < 2; ++j)
            {
                packet.ReadInt32("DbID", idx, j);
                packet.ReadInt32("Rank", idx, j);
                packet.ReadInt32("Step", idx, j);
            }

            packet.ReadUInt32("VirtualRealmAddress", idx);
            packet.ReadByteE<GuildMemberFlag>("Status", idx);
            packet.ReadByte("Level", idx);
            packet.ReadByteE<Class>("ClassID", idx);
            packet.ReadByteE<Gender>("Gender", idx);

            // MythicPlus::DungeonScoreSummary {f32, f32, u32 count, runs[]}
            packet.ReadSingle("OverallScoreCurrentSeason", idx, "DungeonScoreSummary");
            packet.ReadSingle("LadderScoreCurrentSeason", idx, "DungeonScoreSummary");
            var runCount = packet.ReadUInt32("RunCount", idx, "DungeonScoreSummary");
            for (var r = 0u; r < runCount; ++r)
            {
                packet.ReadInt32("ChallengeModeID", idx, "DungeonScoreSummary", "Run", r);
                packet.ReadSingle("MapScore", idx, "DungeonScoreSummary", "Run", r);
                packet.ReadInt32("BestRunLevel", idx, "DungeonScoreSummary", "Run", r);
                packet.ReadInt32("BestRunDurationMS", idx, "DungeonScoreSummary", "Run", r);
                packet.ReadByte("Unknown1110", idx, "DungeonScoreSummary", "Run", r);
                packet.ResetBitReader();
                packet.ReadBit("FinishedSuccess", idx, "DungeonScoreSummary", "Run", r);
            }

            packet.ReadUInt64("GuildClubMemberID", idx);
            packet.ReadByte("RaceID", idx);
            packet.ReadInt32("TimerunningSeasonID", idx);

            packet.ResetBitReader();
            var nameLen = packet.ReadBits(6);
            var surnameLen = packet.ReadBits(9); // LuaSol: Classic 1.60 surname
            var noteLen = packet.ReadBits(8);
            var officersNoteLen = packet.ReadBits(8);
            packet.ReadBit("Authenticated", idx);

            packet.ReadWoWString("Name", nameLen, idx);
            packet.ReadWoWString("Surname", surnameLen, idx);
            packet.ReadWoWString("Note", noteLen, idx);
            packet.ReadWoWString("OfficerNote", officersNoteLen, idx);
        }

        // 0x520035 — TC GuildRoster::Write: member array comes BEFORE the
        // welcome/info text lengths (retail-era upstream handler reads them
        // first and desyncs on 1.60).
        [Parser(Opcode.SMSG_GUILD_ROSTER, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleGuildRoster160(Packet packet)
        {
            packet.ReadUInt32("NumAccounts");
            packet.ReadPackedTime("CreateDate");
            packet.ReadUInt32("GuildFlags");
            var memberCount = packet.ReadUInt32("MemberDataCount");

            for (var i = 0u; i < memberCount; ++i)
                ReadGuildRosterMemberData160(packet, "MemberData", i);

            packet.ResetBitReader();
            var welcomeTextLen = packet.ReadBits(11);
            var infoTextLen = packet.ReadBits(11);

            packet.ReadWoWString("WelcomeText", welcomeTextLen);
            packet.ReadWoWString("InfoText", infoTextLen);
        }

        // 0x520031 — TC GuildEventPlayerJoined::Write: name (6 bits) and
        // surname (9 bits) after {guid, u32 VirtualRealmAddress}.
        [Parser(Opcode.SMSG_GUILD_EVENT_PLAYER_JOINED, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleGuildEventPlayerJoined160(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadUInt32("VirtualRealmAddress");

            packet.ResetBitReader();
            var nameLen = packet.ReadBits(6);
            var surnameLen = packet.ReadBits(9);

            packet.ReadWoWString("Name", nameLen);
            packet.ReadWoWString("Surname", surnameLen);
        }

        // 0x520036 — TC GuildEventPresenceChange::Write: name (6 bits) and
        // surname (9 bits), no Mobile bit.
        [Parser(Opcode.SMSG_GUILD_EVENT_PRESENCE_CHANGE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleGuildEventPresenceChange160(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadUInt32("VirtualRealmAddress");

            packet.ResetBitReader();
            var nameLen = packet.ReadBits(6);
            var surnameLen = packet.ReadBits(9);
            packet.ReadBit("LoggedOn");

            packet.ReadWoWString("Name", nameLen);
            packet.ReadWoWString("Surname", surnameLen);
        }

        // ===== Talents / Traits =====

        // 0x4600BC — TC RespecWipeConfirm::Write: trailing i32 the client
        // stores at +0x38 (decoder rva 0x803190).
        [Parser(Opcode.SMSG_RESPEC_WIPE_CONFIRM, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleRespecWipeConfirm160(Packet packet)
        {
            packet.ReadSByte("RespecType");
            packet.ReadUInt32("Cost");
            packet.ReadPackedGuid128("RespecMaster");
            packet.ReadInt32("Unk160");
        }

        // TC TraitPacketsCommon.cpp — Classic 1.60 wire type 4 is Combat
        // ("CamelotCombat"); Generic carries TraitSystemID + VariationID; the
        // 9-bit name length comes AFTER the subtree array.
        private static void ReadTraitEntry160(Packet packet, params object[] indexes)
        {
            packet.ReadInt32("TraitNodeID", indexes);
            packet.ReadInt32("TraitNodeEntryID", indexes);
            packet.ReadInt32("Rank", indexes);
            packet.ReadInt32("GrantedRanks", indexes);
            packet.ReadInt32("BonusRanks", indexes);
        }

        private static void ReadTraitSubTreeCache160(Packet packet, params object[] indexes)
        {
            packet.ReadInt32("TraitSubTreeID", indexes);
            var entries = packet.ReadUInt32();

            for (var i = 0u; i < entries; ++i)
                ReadTraitEntry160(packet, indexes, "TraitEntry", i);

            packet.ResetBitReader();
            packet.ReadBit("Active", indexes);
        }

        private static void ReadTraitConfig160(Packet packet, params object[] indexes)
        {
            packet.ReadInt32("ID", indexes);
            var type = packet.ReadInt32("Type", indexes);
            var entries = packet.ReadUInt32();
            var subtrees = packet.ReadUInt32();

            switch (type)
            {
                case 1: // Combat (retail numbering)
                case 4: // Combat — Classic 1.60 "CamelotCombat" wire id
                    packet.ReadInt32("ChrSpecializationID", indexes);
                    packet.ReadInt32("CombatConfigFlags", indexes);
                    packet.ReadInt32("LocalIdentifier", indexes);
                    break;
                case 2: // Profession
                    packet.ReadInt32("SkillLineID", indexes);
                    break;
                case 3: // Generic
                    packet.ReadInt32("TraitSystemID", indexes);
                    packet.ReadInt32("VariationID", indexes);
                    break;
            }

            for (var i = 0u; i < entries; ++i)
                ReadTraitEntry160(packet, indexes, "TraitEntry", i);

            for (var i = 0u; i < subtrees; ++i)
                ReadTraitSubTreeCache160(packet, indexes, "TraitSubTreeCache", i);

            packet.ResetBitReader();
            var nameLength = packet.ReadBits(9);
            packet.ReadWoWString("Name", nameLength, indexes);
        }

        // 0x3E02C0
        [Parser(Opcode.CMSG_TRAITS_COMMIT_CONFIG, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleTraitsCommitConfig160(Packet packet)
        {
            ReadTraitConfig160(packet, "Config");
            packet.ReadInt32("SavedConfigID");
            packet.ReadInt32("SavedLocalIdentifier");
        }

        // ===== Action bar =====

        // 0x460083 — Classic 1.60 has 360 saved action buttons (decoder asserts
        // datasize == 8 * 360 + 1); upstream reads 180.
        [Parser(Opcode.SMSG_UPDATE_ACTION_BUTTONS, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleActionButtons160(Packet packet)
        {
            for (int i = 0; i < 360; ++i)
            {
                var packedData = packet.ReadUInt64();

                if (packedData == 0)
                    continue;

                var actionVal = packedData & 0xFFFFFFFFFFFFFFu;
                var type = (byte)((packedData >> 56) & 0xFF);

                packet.AddValue("Action", actionVal, i);
                packet.AddValue("Type", type, i);

                if (type != 0)
                    continue;

                if (CoreParsers.SessionHandler.LoginGuid != null)
                {
                    WoWObject character;
                    if (Storage.Objects.TryGetValue(CoreParsers.SessionHandler.LoginGuid, out character))
                    {
                        Player player = character as Player;
                        if (player != null && player.FirstLogin)
                        {
                            var action = new PlayerCreateInfoAction
                            {
                                Button = (uint)i,
                                Action = (uint)actionVal,
                                Race = player.Race,
                                Class = player.Class,
                                Type = (ActionButtonType)type
                            };

                            Storage.StartActions.Add(action, packet.TimeSpan);
                        }
                    }
                }
            }

            packet.ReadByte("Reason");
        }

        // 0x440062 — Classic 1.60: uint16 button index (360 buttons).
        [Parser(Opcode.CMSG_SET_ACTION_BUTTON, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleActionButton160(Packet packet)
        {
            var data = packet.ReadUInt64();

            packet.AddValue("Type", (ActionButtonType)((data & 0xFF00000000000000) >> 56));
            packet.AddValue("ID", data & 0x00FFFFFFFFFFFFFF);
            packet.ReadUInt16("Button");
        }

        // ===== Creature query =====

        // 0x4A0006 — Classic 1.60 batches creature query responses
        // (client decoder rva 0xA0D800): a 6-bit entry count byte, then per
        // entry {u32 CreatureID, u8 result, stats when result == 0}.
        // No Civilian bit and no PetSpellDataID vs. the retail layout.
        [HasSniffData]
        [Parser(Opcode.SMSG_QUERY_CREATURE_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleCreatureQueryResponse160(Packet packet)
        {
            packet.ResetBitReader();
            var entryCount = packet.ReadBits(6);
            packet.ResetBitReader();

            for (var e = 0u; e < entryCount; ++e)
            {
                PacketQueryCreatureResponse response = packet.Holder.QueryCreatureResponse = new PacketQueryCreatureResponse();
                var entry = packet.ReadInt32("Entry", e);

                CreatureTemplate creature = new CreatureTemplate
                {
                    Entry = (uint)entry
                };
                response.Entry = (uint)entry;

                var result = packet.ReadByte("Result", e);
                response.HasData = result == 0;
                if (result != 0)
                    continue;

                packet.ResetBitReader();
                uint titleLen = packet.ReadBits(11);
                uint titleAltLen = packet.ReadBits(11);
                uint cursorNameLen = packet.ReadBits(6);
                creature.RacialLeader = response.Leader = packet.ReadBit("Leader");

                var stringLens = new int[4][];
                for (int i = 0; i < 4; i++)
                {
                    stringLens[i] = new int[2];
                    stringLens[i][0] = (int)packet.ReadBits(11);
                    stringLens[i][1] = (int)packet.ReadBits(11);
                }
                packet.ResetBitReader();

                for (var i = 0; i < 4; ++i)
                {
                    if (stringLens[i][0] > 1)
                    {
                        string name = packet.ReadDynamicString("Name", stringLens[i][0], i);
                        if (i == 0)
                            creature.Name = response.Name = name;
                    }
                    if (stringLens[i][1] > 1)
                    {
                        string nameAlt = packet.ReadDynamicString("NameAlt", stringLens[i][1], i);
                        if (i == 0)
                            creature.FemaleName = response.NameAlt = nameAlt;
                    }
                }

                creature.TypeFlags = packet.ReadUInt32E<CreatureTypeFlag>("Type Flags", e);
                response.TypeFlags = (uint?)creature.TypeFlags ?? 0;
                creature.TypeFlags2 = response.TypeFlags2 = packet.ReadUInt32("Creature Type Flags 2", e);
                packet.ReadUInt32("Creature Type Flags 3", e);

                creature.Type = packet.ReadByteE<CreatureType>("CreatureType", e);
                creature.Family = packet.ReadInt32E<CreatureFamily>("CreatureFamily", e);
                creature.Rank = packet.ReadSByteE<CreatureRank>("Classification", e);
                response.Type = (int?)creature.Type ?? 0;
                response.Family = (int?)creature.Family ?? 0;
                response.Rank = (int?)creature.Rank ?? 0;

                creature.KillCredits = new uint?[2];
                for (int i = 0; i < 2; ++i)
                {
                    creature.KillCredits[i] = (uint)packet.ReadInt32("ProxyCreatureID", e, i);
                    response.KillCredits.Add(creature.KillCredits[i] ?? 0);
                }

                var displayIdCount = packet.ReadUInt32("DisplayIdCount", e);
                packet.ReadSingle("TotalProbability", e);

                for (var i = 0; i < displayIdCount; ++i)
                {
                    CreatureTemplateModel model = new CreatureTemplateModel
                    {
                        CreatureID = (uint)entry,
                        Idx = (uint)i
                    };

                    model.CreatureDisplayID = (uint)packet.ReadInt32("CreatureDisplayID", e, i);
                    model.DisplayScale = packet.ReadSingle("DisplayScale", e, i);
                    model.Probability = packet.ReadSingle("Probability", e, i);

                    response.Models.Add(model.CreatureDisplayID ?? 0);
                    Storage.CreatureTemplateModels.Add(model, packet.TimeSpan);
                }

                creature.HealthModifier = response.HpMod = packet.ReadSingle("HpMulti", e);
                creature.ManaModifier = response.ManaMod = packet.ReadSingle("EnergyMulti", e);
                uint questItems = packet.ReadUInt32("QuestItems", e);
                uint questCurrencies = packet.ReadUInt32("QuestCurrencies", e);
                creature.MovementID = response.MovementId = (uint)packet.ReadInt32("CreatureMovementInfoID", e);
                creature.HealthScalingExpansion = packet.ReadInt32E<ClientType>("HealthScalingExpansion", e);
                response.HpScalingExp = (uint?)creature.HealthScalingExpansion ?? 0;
                creature.RequiredExpansion = packet.ReadInt32E<ClientType>("RequiredExpansion", e);
                response.Expansion = (uint?)creature.RequiredExpansion ?? 0;
                creature.VignetteID = (uint)packet.ReadInt32("VignetteID", e);
                creature.UnitClass = (uint)packet.ReadInt32E<Class>("UnitClass", e);
                creature.CreatureDifficultyID = packet.ReadInt32("CreatureDifficultyID", e);
                creature.WidgetSetID = packet.ReadInt32("WidgetSetID", e);
                creature.WidgetSetUnitConditionID = packet.ReadInt32("WidgetSetUnitConditionID", e);

                if (titleLen > 1)
                    creature.SubName = response.Title = packet.ReadCString("Title");

                if (titleAltLen > 1)
                    creature.TitleAlt = response.TitleAlt = packet.ReadCString("TitleAlt");

                if (cursorNameLen > 1)
                    creature.IconName = response.IconName = packet.ReadCString("CursorName");

                for (uint i = 0; i < questItems; ++i)
                {
                    CreatureTemplateQuestItem questItem = new CreatureTemplateQuestItem
                    {
                        CreatureEntry = (uint)entry,
                        Idx = i,
                        ItemId = (uint)packet.ReadInt32<ItemId>("QuestItem", e, i)
                    };

                    questItem.DifficultyID = WowPacketParser.Parsing.Parsers.MovementHandler.CurrentDifficultyID;

                    Storage.CreatureTemplateQuestItems.Add(questItem, packet.TimeSpan);
                    response.QuestItems.Add(questItem.ItemId ?? 0);
                }

                for (uint i = 0; i < questCurrencies; ++i)
                {
                    CreatureTemplateQuestCurrency questCurrency = new CreatureTemplateQuestCurrency
                    {
                        CreatureId = (uint)entry,
                        CurrencyId = packet.ReadInt32<CurrencyId>("QuestCurrency", e, i)
                    };

                    Storage.CreatureTemplateQuestCurrencies.Add(questCurrency, packet.TimeSpan);
                    response.QuestCurrencies.Add(questCurrency.CurrencyId ?? 0);
                }

                packet.AddSniffData(StoreNameType.Unit, entry, "QUERY_RESPONSE");

                if (ClientLocale.PacketLocale != LocaleConstant.enUS)
                {
                    CreatureTemplateLocale localesCreature = new CreatureTemplateLocale
                    {
                        ID = (uint)entry,
                        Name = creature.Name,
                        NameAlt = creature.FemaleName,
                        Title = creature.SubName,
                        TitleAlt = creature.TitleAlt
                    };

                    Storage.LocalesCreatures.Add(localesCreature, packet.TimeSpan);
                }

                Storage.CreatureTemplates.Add(creature.Entry ?? (uint)entry, creature, packet.TimeSpan);

                CreatureTemplateDifficultyWDB creatureTemplateDifficultyWDB = new CreatureTemplateDifficultyWDB
                {
                    Entry = creature.Entry,
                    DifficultyID = WowPacketParser.Parsing.Parsers.MovementHandler.CurrentDifficultyID,
                    HealthScalingExpansion = creature.HealthScalingExpansion,
                    HealthModifier = creature.HealthModifier,
                    ManaModifier = creature.ManaModifier,
                    CreatureDifficultyID = creature.CreatureDifficultyID,
                    TypeFlags = creature.TypeFlags,
                    TypeFlags2 = creature.TypeFlags2
                };
                creatureTemplateDifficultyWDB = WowPacketParser.SQL.SQLDatabase.CheckCreatureTemplateDifficultyWDBFallbacks(creatureTemplateDifficultyWDB, creatureTemplateDifficultyWDB.DifficultyID);
                Storage.CreatureTemplateDifficultiesWDB.Add(creatureTemplateDifficultyWDB);

                ObjectName objectName = new ObjectName
                {
                    ObjectType = StoreNameType.Unit,
                    ID = entry,
                    Name = creature.Name
                };
                Storage.ObjectNames.Add(objectName, packet.TimeSpan);
            }
        }

        // 0x3E0143 — Classic 1.60 sends 5 bytes: a leading byte then the
        // creature id (TC QueryCreature::Read).
        [Parser(Opcode.CMSG_QUERY_CREATURE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleCreatureQuery160(Packet packet)
        {
            if (packet.Length - packet.Position == 5)
                packet.ReadByte("Unk160");
            packet.ReadInt32("Entry");
        }

        // ===== Quest query =====

        // 0x660016 — Classic 1.60.1.70009 (client reader rva 0x961170): two
        // extra i32s after the inner QuestID (without them the client shows
        // QuestPackageID in the title and QuestSortID 0 = "Unsorted"),
        // ContentTuningID in the header block, a variable-length
        // RewardDisplaySpell list, RewardFavor, ManagedWorldStateID,
        // QuestSessionBonus, QuestGiverCreatureID, per-objective
        // ConditionalAmount + ParentObjectiveID + Visible bit, and the house
        // reward id lists. String lengths and the objectives' data blocks come
        // after the arrays, matching the modern TC writer.
        [HasSniffData]
        [Parser(Opcode.SMSG_QUERY_QUEST_INFO_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleQuestQueryResponse160(Packet packet)
        {
            packet.ReadInt32("Entry");

            Bit hasData = packet.ReadBit("Has Data");
            if (!hasData)
                return;

            var id = packet.ReadEntry("Quest ID");

            QuestTemplate quest = new QuestTemplate
            {
                ID = (uint)id.Key
            };

            packet.ReadInt32("Unk160_1"); // Classic 1.60
            packet.ReadInt32("Unk160_2"); // Classic 1.60

            quest.QuestType = packet.ReadInt32E<QuestType>("QuestType");
            quest.QuestPackageID = (uint)packet.ReadInt32("QuestPackageID");
            quest.ContentTuningID = packet.ReadInt32("ContentTuningID");
            quest.QuestSortID = (QuestSort)packet.ReadInt32("QuestSortID");
            quest.QuestInfoID = packet.ReadInt32E<QuestInfo>("QuestInfoID");
            quest.SuggestedGroupNum = (uint)packet.ReadInt32("SuggestedGroupNum");
            quest.RewardNextQuest = (uint)packet.ReadInt32("RewardNextQuest");
            quest.RewardXPDifficulty = (uint)packet.ReadInt32("RewardXPDifficulty");

            quest.RewardXPMultiplier = packet.ReadSingle("RewardXPMultiplier");

            quest.RewardMoney = packet.ReadInt32("RewardMoney");
            quest.RewardMoneyDifficulty = (uint)packet.ReadInt32("RewardMoneyDifficulty");

            quest.RewardMoneyMultiplier = packet.ReadSingle("RewardMoneyMultiplier");

            quest.RewardBonusMoney = (uint)packet.ReadInt32("RewardBonusMoney");

            var rewardDisplaySpellCount = packet.ReadUInt32("RewardDisplaySpellCount");

            quest.RewardSpellWod = (uint)packet.ReadInt32("RewardSpell");
            quest.RewardHonorWod = (uint)packet.ReadInt32("RewardHonor");

            quest.RewardKillHonor = packet.ReadSingle("RewardKillHonor");
            quest.RewardFavor = packet.ReadInt32("RewardFavor");

            quest.RewardArtifactXPDifficulty = (uint)packet.ReadInt32("RewardArtifactXPDifficulty");
            quest.RewardArtifactXPMultiplier = packet.ReadSingle("RewardArtifactXPMultiplier");
            quest.RewardArtifactCategoryID = (uint)packet.ReadInt32("RewardArtifactCategoryID");

            quest.StartItem = (uint)packet.ReadInt32("StartItem");
            quest.Flags = packet.ReadInt32E<QuestFlags>("Flags");
            quest.FlagsEx = packet.ReadInt32E<QuestFlagsEx>("FlagsEx");
            quest.FlagsEx2 = packet.ReadInt32E<QuestFlagsEx2>("FlagsEx2");
            quest.FlagsEx3 = packet.ReadInt32E<QuestFlagsEx3>("FlagsEx3");

            quest.RewardItem = new uint?[4];
            quest.RewardAmount = new uint?[4];
            quest.ItemDrop = new uint?[4];
            quest.ItemDropQuantity = new uint?[4];
            for (int i = 0; i < 4; ++i)
            {
                quest.RewardItem[i] = (uint)packet.ReadInt32("RewardItems", i);
                quest.RewardAmount[i] = (uint)packet.ReadInt32("RewardAmount", i);
                quest.ItemDrop[i] = (uint)packet.ReadInt32("ItemDrop", i);
                quest.ItemDropQuantity[i] = (uint)packet.ReadInt32("ItemDropQuantity", i);
            }

            quest.RewardChoiceItemID = new uint?[6];
            quest.RewardChoiceItemQuantity = new uint?[6];
            quest.RewardChoiceItemDisplayID = new uint?[6];
            for (int i = 0; i < 6; ++i)
            {
                quest.RewardChoiceItemID[i] = (uint)packet.ReadInt32("RewardChoiceItemID", i);
                quest.RewardChoiceItemQuantity[i] = (uint)packet.ReadInt32("RewardChoiceItemQuantity", i);
                quest.RewardChoiceItemDisplayID[i] = (uint)packet.ReadInt32("RewardChoiceItemDisplayID", i);
            }

            quest.POIContinent = (uint)packet.ReadInt32("POIContinent");

            quest.POIx = packet.ReadSingle("POIx");
            quest.POIy = packet.ReadSingle("POIy");

            quest.POIPriorityWod = packet.ReadInt32("POIPriority");
            quest.RewardTitle = (uint)packet.ReadInt32("RewardTitle");
            quest.RewardArenaPoints = (uint)packet.ReadInt32("RewardArenaPoints");
            quest.RewardSkillLineID = (uint)packet.ReadInt32("RewardSkillLineID");
            quest.RewardNumSkillUps = (uint)packet.ReadInt32("RewardNumSkillUps");
            quest.QuestGiverPortrait = (uint)packet.ReadInt32("PortraitGiver");
            quest.PortraitGiverMount = (uint)packet.ReadInt32("PortraitGiverMount");
            quest.PortraitGiverModelSceneID = packet.ReadInt32("PortraitGiverModelSceneID");

            quest.QuestTurnInPortrait = (uint)packet.ReadInt32("PortraitTurnIn");

            quest.RewardFactionID = new uint?[5];
            quest.RewardFactionValue = new int?[5];
            quest.RewardFactionOverride = new int?[5];
            quest.RewardFactionCapIn = new int?[5];
            for (int i = 0; i < 5; ++i)
            {
                quest.RewardFactionID[i] = (uint)packet.ReadInt32("RewardFactionID", i);
                quest.RewardFactionValue[i] = packet.ReadInt32("RewardFactionValue", i);
                quest.RewardFactionOverride[i] = packet.ReadInt32("RewardFactionOverride", i);
                quest.RewardFactionCapIn[i] = packet.ReadInt32("RewardFactionCapIn", i);
            }

            quest.RewardFactionFlags = (uint)packet.ReadInt32("RewardFactionFlags");

            quest.RewardCurrencyID = new uint?[4];
            quest.RewardCurrencyCount = new uint?[4];
            for (int i = 0; i < 4; ++i)
            {
                quest.RewardCurrencyID[i] = (uint)packet.ReadInt32("RewardCurrencyID", i);
                quest.RewardCurrencyCount[i] = (uint)packet.ReadInt32("RewardCurrencyQty", i);
            }

            quest.SoundAccept = (uint)packet.ReadInt32("AcceptedSoundKitID");
            quest.SoundTurnIn = (uint)packet.ReadInt32("CompleteSoundKitID");
            quest.AreaGroupID = (uint)packet.ReadInt32("AreaGroupID");
            quest.TimeAllowed = packet.ReadInt64("TimeAllowed");
            uint objectiveCount = packet.ReadUInt32("ObjectiveCount");
            quest.AllowableRacesWod = packet.ReadUInt64("AllowableRaces");

            var treasurePickerCount = packet.ReadUInt32();
            var nonDisplayableTreasurePickerCount = packet.ReadUInt32();

            quest.Expansion = packet.ReadInt32("Expansion");
            quest.ManagedWorldStateID = packet.ReadInt32("ManagedWorldStateID");
            quest.QuestSessionBonus = packet.ReadInt32("QuestSessionBonus");
            packet.ReadInt32("QuestGiverCreatureID");

            var conditionalQuestDescriptionCount = packet.ReadUInt32();
            var conditionalQuestCompletionLogCount = packet.ReadUInt32();

            var rewardHouseRoomCount = packet.ReadUInt32("RewardHouseRoomIDCount");
            var rewardHouseDecorCount = packet.ReadUInt32("RewardHouseDecorIDCount");

            for (uint i = 0; i < rewardDisplaySpellCount; ++i)
            {
                QuestRewardDisplaySpell questRewardDisplaySpell = new QuestRewardDisplaySpell
                {
                    QuestID = (uint)id.Key,
                    Idx = i,
                    SpellID = (uint)packet.ReadInt32<SpellId>("SpellID", i, "RewardDisplaySpell"),
                    PlayerConditionID = (uint)packet.ReadInt32("PlayerConditionID", i, "RewardDisplaySpell"),
                    Type = packet.ReadInt32("Type", i, "RewardDisplaySpell")
                };

                if (questRewardDisplaySpell.SpellID != 0)
                    Storage.QuestRewardDisplaySpells.Add(questRewardDisplaySpell, packet.TimeSpan);
            }

            for (uint i = 0; i < objectiveCount; ++i)
            {
                var objectiveId = packet.ReadEntry("Id", i);

                QuestObjective questInfoObjective = new QuestObjective
                {
                    ID = (uint)objectiveId.Key,
                    QuestID = (uint)id.Key
                };

                questInfoObjective.Type = packet.ReadInt32E<QuestRequirementType>("Quest Requirement Type", i);

                questInfoObjective.StorageIndex = packet.ReadSByte("StorageIndex", i);
                questInfoObjective.Order = i;
                questInfoObjective.ObjectID = packet.ReadInt32("ObjectID", i);
                questInfoObjective.Amount = packet.ReadInt32("Amount", i);
                questInfoObjective.ConditionalAmount = packet.ReadInt32("ConditionalAmount", i); // objective type 22
                questInfoObjective.Flags = packet.ReadUInt32("Flags", i);
                questInfoObjective.Flags2 = packet.ReadUInt32("Flags2", i);
                questInfoObjective.ProgressBarWeight = packet.ReadSingle("ProgressBarWeight", i);

                var visualEffectsCount = packet.ReadUInt32("VisualEffects", i);
                questInfoObjective.ParentObjectiveID = packet.ReadInt32("ParentObjectiveID", i);

                for (var j = 0; j < visualEffectsCount; ++j)
                {
                    QuestVisualEffect questVisualEffect = new QuestVisualEffect
                    {
                        ID = questInfoObjective.ID,
                        Index = (uint)j,
                        VisualEffect = packet.ReadInt32("VisualEffectId", i, j)
                    };

                    Storage.QuestVisualEffects.Add(questVisualEffect, packet.TimeSpan);
                }

                packet.ResetBitReader();

                uint descriptionLength = packet.ReadBits(8);
                questInfoObjective.Visible = packet.ReadBit("Visible", i);
                packet.ResetBitReader();

                questInfoObjective.Description = packet.ReadWoWString("Description", descriptionLength, i);

                if (ClientLocale.PacketLocale != LocaleConstant.enUS && questInfoObjective.Description != string.Empty)
                {
                    QuestObjectivesLocale localesQuestObjectives = new QuestObjectivesLocale
                    {
                        ID = (uint)objectiveId.Key,
                        QuestId = (uint)id.Key,
                        StorageIndex = questInfoObjective.StorageIndex,
                        Description = questInfoObjective.Description
                    };

                    Storage.LocalesQuestObjectives.Add(localesQuestObjectives, packet.TimeSpan);
                }

                Storage.QuestObjectives.Add((uint)questInfoObjective.ID, questInfoObjective, packet.TimeSpan);
            }

            for (uint i = 0; i < treasurePickerCount; ++i)
            {
                var treasurePickerID = packet.ReadInt32("TreasurePickerID");
                QuestTreasurePickers pickers = new()
                {
                    QuestID = quest.ID,
                    TreasurePickerID = treasurePickerID,
                    OrderIndex = (int)i
                };
                Storage.QuestTreasurePickersStorage.Add(pickers);
            }

            for (uint i = 0; i < nonDisplayableTreasurePickerCount; ++i)
                packet.ReadInt32("NonDisplayableTreasurePickerID", i);

            for (int i = 0; i < conditionalQuestDescriptionCount; i++)
                QuestHandler.ReadConditionalQuestText(packet, id.Key, i, QuestHandler.ConditionalTextType.Description, i, "ConditionalDescriptionText");

            for (int i = 0; i < conditionalQuestCompletionLogCount; i++)
                QuestHandler.ReadConditionalQuestText(packet, id.Key, i, QuestHandler.ConditionalTextType.CompletionLog, i, "ConditionalCompletionLogText");

            for (uint i = 0; i < rewardHouseRoomCount; ++i)
                packet.ReadInt32("RewardHouseRoomID", i);

            for (uint i = 0; i < rewardHouseDecorCount; ++i)
                packet.ReadInt32("RewardHouseDecorID", i);

            packet.ResetBitReader();

            uint logTitleLen = packet.ReadBits(9);
            uint logDescriptionLen = packet.ReadBits(12);
            uint questDescriptionLen = packet.ReadBits(12);
            uint areaDescriptionLen = packet.ReadBits(9);
            uint questGiverTextWindowLen = packet.ReadBits(10);
            uint questGiverTargetNameLen = packet.ReadBits(8);
            uint questTurnTextWindowLen = packet.ReadBits(10);
            uint questTurnTargetNameLen = packet.ReadBits(8);
            uint questCompletionLogLen = packet.ReadBits(11);

            packet.ReadBit("ResetByScheduler");
            packet.ReadBit("ReadyForTranslation");

            packet.ResetBitReader();

            quest.LogTitle = packet.ReadWoWString("LogTitle", logTitleLen);
            quest.LogDescription = packet.ReadWoWString("LogDescription", logDescriptionLen);
            quest.QuestDescription = packet.ReadWoWString("QuestDescription", questDescriptionLen);
            quest.AreaDescription = packet.ReadWoWString("AreaDescription", areaDescriptionLen);
            quest.QuestGiverTextWindow = packet.ReadWoWString("PortraitGiverText", questGiverTextWindowLen);
            quest.QuestGiverTargetName = packet.ReadWoWString("PortraitGiverName", questGiverTargetNameLen);
            quest.QuestTurnTextWindow = packet.ReadWoWString("PortraitTurnInText", questTurnTextWindowLen);
            quest.QuestTurnTargetName = packet.ReadWoWString("PortraitTurnInName", questTurnTargetNameLen);
            quest.QuestCompletionLog = packet.ReadWoWString("QuestCompletionLog", questCompletionLogLen);

            ObjectName objectName = new ObjectName
            {
                ObjectType = StoreNameType.Quest,
                ID = (int?)quest.ID,
                Name = quest.LogTitle
            };
            Storage.ObjectNames.Add(objectName, packet.TimeSpan);

            if (ClientLocale.PacketLocale != LocaleConstant.enUS)
            {
                LocalesQuest localesQuest = new LocalesQuest
                {
                    ID = (uint)id.Key,
                    LogTitle = quest.LogTitle,
                    LogDescription = quest.LogDescription,
                    QuestDescription = quest.QuestDescription,
                    AreaDescription = quest.AreaDescription,
                    PortraitGiverText = quest.QuestGiverTextWindow,
                    PortraitGiverName = quest.QuestGiverTargetName,
                    PortraitTurnInText = quest.QuestTurnTextWindow,
                    PortraitTurnInName = quest.QuestTurnTargetName,
                    QuestCompletionLog = quest.QuestCompletionLog
                };

                Storage.LocalesQuests.Add(localesQuest, packet.TimeSpan);
            }

            packet.AddSniffData(StoreNameType.Quest, id.Key, "QUERY_RESPONSE");

            Storage.QuestTemplates.Add(quest, packet.TimeSpan);
        }

        // ===== Misc client packets =====

        // 0x440070 — Classic 1.60.1.70009 (TC CreateCharacter::Read): two
        // unknown bits and a 6-bit surname length in the flag block, an
        // unknown i32 (-1) before TimerunningSeasonID, and the surname string
        // after the name.
        [Parser(Opcode.CMSG_CREATE_CHARACTER, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleClientCharCreate160(Packet packet)
        {
            packet.ResetBitReader();
            var nameLen = packet.ReadBits(6);
            var hasTemplateSet = packet.ReadBit("HasTemplateSet");
            packet.ReadBit("IsTrialBoost");
            packet.ReadBit("UseNPE");
            packet.ReadBit("HardcoreSelfFound");
            packet.ReadBits(2); // unknown
            var surnameLen = packet.ReadBits(6);
            packet.ResetBitReader();

            packet.ReadByteE<Race>("RaceID");
            packet.ReadByteE<Class>("ClassID");
            packet.ReadByteE<Gender>("SexID");

            var customizationCount = packet.ReadUInt32();
            packet.ReadInt32("Unk160"); // always -1 on captures
            packet.ReadInt32("TimerunningSeasonID");

            packet.ReadWoWString("Name", nameLen);
            packet.ReadWoWString("Surname", surnameLen);

            if (hasTemplateSet)
                packet.ReadInt32("TemplateSetID");

            for (var i = 0u; i < customizationCount; ++i)
                CharacterHandler.ReadChrCustomizationChoice(packet, "Customizations", i);
        }

        // 0x440071 — Classic 1.60.1.70009 (TC CheckCharacterNameAvailability::
        // Read): u32 SequenceIndex, 6-bit name length, 3 unknown bits, 6-bit
        // surname length, then name and surname data.
        [Parser(Opcode.CMSG_CHECK_CHARACTER_NAME_AVAILABILITY, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleCheckCharacterNameAvailability160(Packet packet)
        {
            packet.ReadUInt32("SequenceIndex");

            packet.ResetBitReader();
            var nameLen = packet.ReadBits(6);
            packet.ReadBits(3); // unknown
            var surnameLen = packet.ReadBits(6);
            packet.ResetBitReader();

            packet.ReadWoWString("Name", nameLen);
            packet.ReadWoWString("Surname", surnameLen);
        }

        // 0x3F0075 — Classic 1.60.1.70009 sends an empty payload; a trailing
        // IdleLogout bit may be present in other layouts (TC LogoutRequest).
        [Parser(Opcode.CMSG_LOGOUT_REQUEST, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleLogoutRequest160(Packet packet)
        {
            if (packet.CanRead())
            {
                packet.ResetBitReader();
                packet.ReadBit("IdleLogout");
            }
        }

        // ===== Movement / spells =====

        // 0x5F0064 — LuaSol: SMSG_MOVE_ADD_IMPULSE (Skyriding impulse spells):
        // {packedGuid MoverGUID, u32 SequenceIndex, vec3 Direction}.
        [Parser(Opcode.SMSG_MOVE_ADD_IMPULSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleMoveAddImpulse160(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
            packet.ReadVector3("Direction");
        }

        // 0x42006D — client ACK for SMSG_MOVE_ADD_IMPULSE; standard
        // MovementAckMessage payload {MovementInfo, i32 AckIndex}
        // (TC maps it to WorldSession::HandleMovementAckMessage).
        [Parser(Opcode.CMSG_MOVE_ADD_IMPULSE_ACK, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleMoveAddImpulseAck160(Packet packet)
        {
            FluxMovementHandler.ReadMovementAck160(packet);
        }

        // 0x680044 — LuaSol: SMSG_PUSH_SPELL_TO_ACTION_BAR sends a spell ID to
        // be placed on the action bar (Skyriding mount activation).
        [Parser(Opcode.SMSG_PUSH_SPELL_TO_ACTION_BAR, ClientVersionBuild.V1_60_1_70009)]
        public static void HandlePushSpellToActionBar160(Packet packet)
        {
            packet.ReadInt32<SpellId>("SpellID");
        }

        // Client reader (0x24F1E50): {u32, u8, u32 count, count * {i32, u8,
        // i64, i32}, u8 flagByte(bit7)}. LuaSol sends a 9-byte release packet
        // (u32, u8, u32=0) after CharEnum — no flag byte — so the trailing
        // read is guarded.
        [Parser(Opcode.SMSG_RECENT_ALLY_DATA_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleRecentAllyDataResponse160(Packet packet)
        {
            packet.ReadUInt32("Unk32");
            packet.ReadByte("Kind"); // client only uses the list when this is 7
            var count = packet.ReadUInt32("RecentAllyCount");

            for (var i = 0u; i < count; ++i)
            {
                packet.ReadInt32("Unk32", "RecentAllies", i);
                packet.ReadByte("Unk8", "RecentAllies", i);
                packet.ReadInt64("Unk64", "RecentAllies", i);
                packet.ReadInt32("Unk32_2", "RecentAllies", i);
            }

            if (packet.CanRead())
                packet.ReadByte("FlagByte");
        }
    }
}
