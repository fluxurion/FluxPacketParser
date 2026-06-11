using System.Collections.Generic;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using WowPacketParserModule.Substructures;

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{
    public static class GarrisonHandler
    {
        [Parser(Opcode.SMSG_GET_GARRISON_INFO_RESULT)]
        public static void HandleGetGarrisonInfoResult(Packet packet)
        {
            packet.ReadInt32("FactionIndex");
            var garrisonCount = packet.ReadUInt32("GarrisonCount");
            var followerSoftCapCount = packet.ReadUInt32("FollowerSoftCapCount");

            for (var i = 0u; i < followerSoftCapCount; ++i)
            {
                packet.ReadInt32("GarrFollowerTypeID", i);
                packet.ReadUInt32("Count", i);
            }

            for (int g = 0; g < garrisonCount; g++)
            {
                packet.ReadInt32("GarrTypeID", g);
                packet.ReadInt32("GarrSiteID", g);
                packet.ReadInt32("GarrSiteLevelID", g);

                // Counts — TC 10.2.7 order
                var buildingCount = packet.ReadUInt32("BuildingCount", g);
                var plotCount = packet.ReadUInt32("PlotCount", g);
                var followerCount = packet.ReadUInt32("FollowerCount", g);
                var autoTroopCount = packet.ReadUInt32("AutoTroopCount", g);
                var missionCount = packet.ReadUInt32("MissionCount", g);
                var missionRewardCount = packet.ReadUInt32("MissionRewardGroupCount", g);
                var missionOvermaxCount = packet.ReadUInt32("MissionOvermaxGroupCount", g);
                var areaBonusCount = packet.ReadUInt32("MissionAreaBonusCount", g);
                var talentCount = packet.ReadUInt32("TalentCount", g);
                var collectionCount = packet.ReadUInt32("CollectionCount", g);
                var eventListCount = packet.ReadUInt32("EventListCount", g);
                var specGroupCount = packet.ReadUInt32("SpecGroupCount", g);
                var canStartMissionCount = packet.ReadUInt32("CanStartMissionCount", g);
                var archivedMissionCount = packet.ReadUInt32("ArchivedMissionCount", g);

                packet.ReadInt32("NumFollowerActivationsRemaining", g);
                packet.ReadUInt32("NumMissionsStartedToday", g);
                packet.ReadInt32("MinAutoTroopLevel", g);

                // Data phase — TC 10.2.7 order

                // Plots
                for (var i = 0u; i < plotCount; i++)
                    ReadPlotInfo(packet, g, "PlotInfo", i);

                // MissionReward per-group sizes
                var missionRewardSizes = new uint[missionRewardCount];
                for (var i = 0u; i < missionRewardCount; i++)
                    missionRewardSizes[i] = packet.ReadUInt32("MissionRewardSize", g, i);

                // MissionOvermax per-group sizes
                var missionOvermaxSizes = new uint[missionOvermaxCount];
                for (var i = 0u; i < missionOvermaxCount; i++)
                    missionOvermaxSizes[i] = packet.ReadUInt32("MissionOvermaxSize", g, i);

                // Area bonuses
                for (var i = 0u; i < areaBonusCount; i++)
                {
                    packet.ReadTime64("StartTime", g, "MissionAreaBonus", i);
                    packet.ReadUInt32("GarrMssnBonusAbilityID", g, "MissionAreaBonus", i);
                }

                // Collections
                for (var i = 0u; i < collectionCount; i++)
                {
                    packet.ReadInt32("Type", g, "Collection", i);
                    var entryCount = packet.ReadUInt32("EntryCount", g, "Collection", i);
                    for (var j = 0u; j < entryCount; j++)
                    {
                        packet.ReadInt32("EntryID", g, "Collection", i, j);
                        packet.ReadInt32("Rank", g, "Collection", i, j);
                    }
                }

                // Event lists
                for (var i = 0u; i < eventListCount; i++)
                {
                    packet.ReadInt32("Type", g, "EventList", i);
                    var eventCount = packet.ReadUInt32("EventCount", g, "EventList", i);
                    for (var j = 0u; j < eventCount; j++)
                    {
                        packet.ReadInt64("EventValue", g, "EventList", i, j);
                        packet.ReadInt32("EntryID", g, "EventList", i, j);
                    }
                }

                // Spec groups
                for (var i = 0u; i < specGroupCount; i++)
                {
                    packet.ReadInt32("ChrSpecializationID", g, "SpecGroup", i);
                    packet.ReadInt32("SoulbindID", g, "SpecGroup", i);
                }

                // Archived missions (raw int32 blob)
                for (var i = 0u; i < archivedMissionCount; i++)
                    packet.ReadInt32("MissionRecID", g, "ArchivedMission", i);

                // Buildings
                for (var i = 0u; i < buildingCount; i++)
                    ReadBuildingInfo(packet, g, "BuildingInfo", i);

                // CanStartMission bits
                for (var i = 0u; i < canStartMissionCount; i++)
                    packet.ReadBit("CanStartMission", g, i);

                packet.ResetBitReader();

                // Followers
                for (var i = 0u; i < followerCount; i++)
                    ReadFollower(packet, g, "Follower", i);

                // Auto troops
                for (var i = 0u; i < autoTroopCount; i++)
                    ReadFollower(packet, g, "AutoTroop", i);

                // Missions
                for (var i = 0u; i < missionCount; i++)
                    ReadGarrisonMission(packet, g, "Mission", i);

                // Talents
                for (var i = 0u; i < talentCount; i++)
                    ReadTalent(packet, g, "Talent", i);

                // MissionReward items
                for (var i = 0u; i < missionRewardCount; i++)
                    for (var j = 0u; j < missionRewardSizes[i]; j++)
                        ReadMissionReward(packet, g, "MissionReward", i, j);

                // MissionOvermax items
                for (var i = 0u; i < missionOvermaxCount; i++)
                    for (var j = 0u; j < missionOvermaxSizes[i]; j++)
                        ReadMissionReward(packet, g, "MissionOvermaxReward", i, j);
            }
        }

        [Parser(Opcode.SMSG_GARRISON_ADD_MISSION_RESULT)]
        public static void HandleGarrisonAddMissionResult(Packet packet)
        {
            packet.ReadInt32("GarrTypeId");
            packet.ReadUInt32("Result");
            packet.ReadByte("State");
            ReadGarrisonMission(packet, "Mission");

            var rewardsCount = packet.ReadUInt32("RewardsCount");
            var overmaxRewardsCount = packet.ReadUInt32("OvermaxRewardsCount");

            packet.ReadBit("CanStart");
            packet.ResetBitReader();

            for (var i = 0u; i < rewardsCount; i++)
                ReadMissionReward(packet, "MissionReward", i);

            for (var i = 0u; i < overmaxRewardsCount; i++)
                ReadMissionReward(packet, "MissionOvermaxReward", i);
        }

        private static void ReadPlotInfo(Packet packet, params object[] index)
        {
            packet.ReadInt32("GarrPlotInstanceID", index);
            packet.ReadVector4("PlotPos", index);
            packet.ReadUInt32("PlotType", index);
        }

        private static void ReadBuildingInfo(Packet packet, params object[] index)
        {
            packet.ReadInt32("GarrPlotInstanceID", index);
            packet.ReadInt32("GarrBuildingID", index);
            packet.ReadTime64("TimeBuilt", index);
            packet.ReadInt32("CurrentGarSpecID", index);
            packet.ReadTime64("TimeSpecCooldown", index);
            packet.ReadBit("Active", index);
            packet.ResetBitReader();
        }

        private static void ReadFollower(Packet packet, params object[] index)
        {
            packet.ReadUInt64("DbID", index);
            packet.ReadUInt32("GarrFollowerID", index);
            packet.ReadUInt32("Quality", index);
            packet.ReadUInt32("FollowerLevel", index);
            packet.ReadUInt32("ItemLevelWeapon", index);
            packet.ReadUInt32("ItemLevelArmor", index);
            packet.ReadUInt32("Xp", index);
            packet.ReadUInt32("Durability", index);
            packet.ReadUInt32("CurrentBuildingID", index);
            packet.ReadUInt32("CurrentMissionID", index);
            var abilityCount = packet.ReadUInt32("AbilityCount", index);
            packet.ReadUInt32("ZoneSupportSpellID", index);
            packet.ReadUInt32("FollowerStatus", index);
            packet.ReadInt32("Health", index);
            packet.ReadSByte("BoardIndex", index);
            packet.ReadTime64("HealingTimestamp", index);
            for (var i = 0u; i < abilityCount; i++)
                packet.ReadUInt32("AbilityID", index, i);
            var nameLen = packet.ReadBits(7);
            packet.ResetBitReader();
            packet.ReadWoWString("CustomName", nameLen, index);
        }

        private static void ReadTalent(Packet packet, params object[] index)
        {
            packet.ReadInt32("GarrTalentID", index);
            packet.ReadInt32("Rank", index);
            packet.ReadTime64("ResearchStartTime", index);
            packet.ReadInt32("Flags", index);
            var hasSocket = packet.ReadBit("HasSocket", index);
            packet.ResetBitReader();
            if (hasSocket)
            {
                packet.ReadInt32("SoulbindConduitID", index);
                packet.ReadInt32("SoulbindConduitRank", index);
            }
        }

        private static void ReadMissionReward(Packet packet, params object[] index)
        {
            packet.ReadInt32("ItemID", index);
            packet.ReadUInt32("ItemQuantity", index);
            packet.ReadInt32("CurrencyID", index);
            packet.ReadUInt32("CurrencyQuantity", index);
            packet.ReadUInt32("FollowerXP", index);
            packet.ReadUInt32("GarrMssnBonusAbilityID", index);
            packet.ReadInt32("ItemFileDataID", index);
            var hasItem = packet.ReadBit("HasItemInstance", index);
            packet.ResetBitReader();
            if (hasItem)
                Substructures.ItemHandler.ReadItemInstance(packet, index, "ItemInstance");
        }

        private static void ReadGarrisonMission(Packet packet, params object[] index)
        {
            packet.ReadUInt64("DbID", index);
            packet.ReadInt32("MissionRecID", index);
            packet.ReadTime64("OfferTime", index);
            packet.ReadInt64("OfferDuration", index);
            packet.ReadTime64("StartTime", index);
            packet.ReadInt64("TravelDuration", index);
            packet.ReadInt64("MissionDuration", index);
            packet.ReadInt32("MissionState", index);
            packet.ReadInt32("SuccessChance", index);
            packet.ReadUInt32("Flags", index);
            packet.ReadSingle("MissionScalar", index);
            packet.ReadInt32("ContentTuningID", index);
            var encounterCount = packet.ReadUInt32("EncounterCount", index);
            var rewardCount = packet.ReadUInt32("MissionRewardCount", index);
            var overmaxCount = packet.ReadUInt32("MissionOvermaxCount", index);

            for (var i = 0u; i < encounterCount; i++)
                ReadGarrisonEncounter(packet, index, "Encounter", i);

            for (var i = 0u; i < rewardCount; i++)
                ReadMissionReward(packet, index, "Reward", i);

            for (var i = 0u; i < overmaxCount; i++)
                ReadMissionReward(packet, index, "OvermaxReward", i);
        }

        private static void ReadGarrisonEncounter(Packet packet, params object[] index)
        {
            packet.ReadInt32("GarrEncounterID", index);
            var mechanicCount = packet.ReadUInt32("MechanicCount", index);
            packet.ReadInt32("GarrAutoCombatantID", index);
            packet.ReadInt32("Health", index);
            packet.ReadInt32("MaxHealth", index);
            packet.ReadInt32("Attack", index);
            packet.ReadSByte("BoardIndex", index);
            for (var i = 0u; i < mechanicCount; i++)
                packet.ReadInt32("MechanicID", index, i);
        }
    }
}
