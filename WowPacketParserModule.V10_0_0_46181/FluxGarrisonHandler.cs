using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using System.Collections.Generic;

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{
    public static class GarrisonHandler
    {
        [Parser(Opcode.SMSG_GET_GARRISON_INFO_RESULT)]
        public static void HandleGetGarrisonInfoResult(Packet packet)
        {
            packet.ReadInt32("FactionIndex");
            var garrisonCount = packet.ReadInt32("GarrisonCount");
            var followShipmentCount = packet.ReadInt32("FollowerShipmentCount");

            for (int i = 0; i < followShipmentCount; i++)
            {
                packet.ReadInt32("FollowerShipmentID", i);
                packet.ReadInt32("ShipmentID", i);
            }

            for (int g = 0; g < garrisonCount; g++)
            {
                packet.ReadInt32("GarrSiteID", g);
                packet.ReadInt32("GarrTypeID", g);
                packet.ReadInt32("GarrSiteLevelID", g);

                var buildingCount = packet.ReadInt32("BuildingCount", g);
                var followerCount = packet.ReadInt32("FollowerCount", g);
                var archivedMissionCount = packet.ReadInt32("ArchivedMissionCount", g);
                var activeMissionCount = packet.ReadInt32("ActiveMissionCount", g);
                var dailyMissionCount = packet.ReadInt32("DailyMissionCount", g);
                var rewardCount = packet.ReadInt32("RewardCount", g);
                var overmaxRewardCount = packet.ReadInt32("OvermaxRewardCount", g);
                var talentCount = packet.ReadInt32("TalentCount", g);
                var landPlotCount = packet.ReadInt32("LandPlotCount", g);
                var remoteBuildingCount = packet.ReadInt32("RemoteBuildingCount", g);
                var workOrderCount = packet.ReadInt32("WorkOrderCount", g);
                var chitCount = packet.ReadInt32("ChitCount", g);
                var unkCount304 = packet.ReadInt32("UnkCount304", g);
                var unkCount328 = packet.ReadInt32("UnkCount328", g);

                packet.ReadInt32("GarrGoldCurrencyID", g);
                packet.ReadInt32("GarrResourceCurrencyID", g);
                packet.ReadInt32("GarrOilCurrencyID", g);

                // Followers: 6 int32s = 24 bytes each (sub_1407F13C0)
                for (int i = 0; i < followerCount; i++)
                {
                    packet.ReadInt32("GarrFollowerID", g, "Follower", i);
                    packet.ReadInt32("Quality", g, "Follower", i);
                    packet.ReadInt32("FollowerLevel", g, "Follower", i);
                    packet.ReadInt32("ItemLevelWeapon", g, "Follower", i);
                    packet.ReadInt32("ItemLevelArmor", g, "Follower", i);
                    packet.ReadInt32("Flags", g, "Follower", i);
                }

                // Reward groups: read ItemInstance counts into each 24-byte entry
                // IDA: for each reward group, ReadInt32 count → sub_1408910F0(resize)
                var rewardItemCounts = new List<int>();
                for (int i = 0; i < rewardCount; i++)
                {
                    var itemCount = packet.ReadInt32("RewardItemCount", g, "Reward", i);
                    rewardItemCounts.Add(itemCount);
                }

                var overmaxItemCounts = new List<int>();
                for (int i = 0; i < overmaxRewardCount; i++)
                {
                    var itemCount = packet.ReadInt32("OvermaxItemCount", g, "Overmax", i);
                    overmaxItemCounts.Add(itemCount);
                }

                // Talents: int64 + int32 = 16 bytes each (sub_140890F20)
                for (int i = 0; i < talentCount; i++)
                {
                    packet.ReadInt64("GarrTalentID", g, "Talents", i);
                    packet.ReadInt32("Rank", g, "Talents", i);
                }

                // Remote Buildings: 32 bytes each (sub_1408902D0)
                // IDA: int32 at +0, array of (int32,int32) at +8, count at +16
                for (int i = 0; i < remoteBuildingCount; i++)
                {
                    packet.ReadInt32("GarrBuildingID", g, "RemoteBuildings", i);
                    var internalCount = packet.ReadInt32("InternalCount", g, "RemoteBuildings", i);
                    for (int j = 0; j < internalCount; j++)
                    {
                        packet.ReadInt32("PlotInstanceID", g, "RemoteBuildings", i, j);
                        packet.ReadInt32("UnkField", g, "RemoteBuildings", i, j);
                    }
                }

                // Work Orders: 32 bytes each (sub_140890AB0)
                // IDA: int32 at +0, array of (int64,int32) at +8, count at +16
                for (int i = 0; i < workOrderCount; i++)
                {
                    packet.ReadInt32("GarrBuildingID", g, "WorkOrders", i);
                    var internalCount = packet.ReadInt32("InternalCount", g, "WorkOrders", i);
                    for (int j = 0; j < internalCount; j++)
                    {
                        packet.ReadInt64("DbID", g, "WorkOrders", i, j);
                        packet.ReadInt32("UnkField", g, "WorkOrders", i, j);
                    }
                }

                // Chits: 2 int32s = 8 bytes each (sub_1408917B0)
                for (int i = 0; i < chitCount; i++)
                {
                    packet.ReadInt32("ChitID", g, "Chits", i);
                    packet.ReadInt32("ChitCount", g, "Chits", i);
                }

                // UnkCount328: int32 array (Int32Array_Resize at +328)
                for (int i = 0; i < unkCount328; i++)
                    packet.ReadInt32("Unk328_Field", g, i);

                // Buildings: 32 bytes each (sub_1407F0BA0)
                // IDA: int32(GarrBuildingID)+int32(PlotInstanceID)+int64(TimeBuilt)+byte(HasShipment)+int32(Active)+int64(Unk)
                for (int i = 0; i < buildingCount; i++)
                {
                    packet.ReadInt32("GarrBuildingID", g, "Buildings", i);
                    packet.ReadInt32("PlotInstanceID", g, "Buildings", i);
                    packet.ReadTime64("TimeBuilt", g, "Buildings", i);
                    var hasShipment = packet.ReadByte("HasShipment", g, "Buildings", i);
                    packet.ReadInt32("Active", g, "Buildings", i);
                    packet.ReadInt64("UnkInt64", g, "Buildings", i);
                    packet.AddValue("HasShipment", hasShipment);
                }

                // Bit-packed boolean array (sub_1408A6AD0)
                int bitIndex = 8;
                byte packedByte = 0;
                for (int i = 0; i < unkCount304; i++)
                {
                    if (bitIndex == 8)
                    {
                        packedByte = packet.ReadByte();
                        bitIndex = 0;
                    }
                    packet.AddValue("UnkBit", (packedByte & 0x80) != 0, g, i);
                    packedByte *= 2;
                    bitIndex++;
                }

                // Archived missions: 200 bytes each (sub_1407F0D00)
                for (int i = 0; i < archivedMissionCount; i++)
                    ReadGarrisonMission200(packet, g, "ArchivedMissions", i);

                // Active missions: 200 bytes each (sub_1407F0D00)
                for (int i = 0; i < activeMissionCount; i++)
                    ReadGarrisonMission200(packet, g, "ActiveMissions", i);

                // Daily missions: 152 bytes each (ReadGarrisonMission)
                // IDA flow: encounters at +80 (56 bytes each), rewards at +104 (24 bytes each), overmax at +128 (24 bytes each)
                for (int i = 0; i < dailyMissionCount; i++)
                    ReadGarrisonMission(packet, g, "DailyMissions", i);

                // Land Plots: 32 bytes each (sub_1407F1460)
                // IDA: int32+int32+int64(DbID)+int32+byte(hasExtra)+[conditional: int32+int32]
                for (int i = 0; i < landPlotCount; i++)
                {
                    packet.ReadInt32("PlotInstanceID", g, "LandPlots", i);
                    packet.ReadInt32("GarrPlotID", g, "LandPlots", i);
                    packet.ReadInt64("DbID", g, "LandPlots", i);
                    packet.ReadInt32("GarrPlotSizeID", g, "LandPlots", i);
                    var hasExtra = packet.ReadByte("HasExtra", g, "LandPlots", i);
                    if (hasExtra != 0)
                    {
                        packet.ReadInt32("UnkExtra1", g, "LandPlots", i);
                        packet.ReadInt32("UnkExtra2", g, "LandPlots", i);
                    }
                }

                // ItemInstances for rewards: 152 bytes each (sub_1407F12B0)
                for (int i = 0; i < rewardCount; i++)
                {
                    for (int j = 0; j < rewardItemCounts[i]; j++)
                        ReadItemInstance152(packet, g, "RewardItems", i, j);
                }

                // ItemInstances for overmax rewards: 152 bytes each (sub_1407F12B0)
                for (int i = 0; i < overmaxRewardCount; i++)
                {
                    for (int j = 0; j < overmaxItemCounts[i]; j++)
                        ReadItemInstance152(packet, g, "OvermaxRewardItems", i, j);
                }
            }
        }

        [Parser(Opcode.SMSG_GARRISON_ADD_MISSION_RESULT)]
        public static void HandleGarrisonAddMissionResult(Packet packet)
        {
            packet.ReadInt32("Result");
            packet.ReadInt32("GarrTypeID");
            packet.ReadByte("State");
            packet.ReadBit("CanStart");

            ReadGarrisonMission(packet, "MissionData");
        }

        // 152-byte daily mission (ReadGarrisonMission in IDA)
        // Layout: int64(DbID) + int32(MissionRecID) + 6×int64(OfferTime/Duration/StartTime/Travel/Mission/MissionDuration)
        //         + 5×int32(State/Chance/Flags/Scalar/ContentTuning) + encounters(56 each) + rewards(24 each) + overmax(24 each)
        private static void ReadGarrisonMission(Packet packet, params object[] index)
        {
            packet.ReadInt64("DbID", index);
            packet.ReadInt32("MissionRecID", index);
            packet.ReadTime64("OfferTime", index);
            packet.ReadInt64("OfferDuration", index);
            packet.ReadTime64("StartTime", index);
            packet.ReadInt64("TravelDuration", index);
            packet.ReadInt64("MissionDuration", index);
            packet.ReadInt32("MissionState", index);
            packet.ReadInt32("SuccessChance", index);
            packet.ReadInt32("Flags", index);
            packet.ReadInt32("MissionScalar", index);
            packet.ReadInt32("ContentTuningID", index);

            // Encounters at +80: each 56 bytes
            var encounterCount = packet.ReadInt32("EncounterCount", index);
            for (int i = 0; i < encounterCount; i++)
                ReadGarrisonEncounter56(packet, "Encounter", index, i);

            // Rewards at +104: 24-byte entries (pointer + count), then 152-byte ItemInstances
            var rewardCount = packet.ReadInt32("RewardCount", index);
            var rewardItemCounts = new List<int>();
            for (int i = 0; i < rewardCount; i++)
            {
                var itemCount = packet.ReadInt32("RewardItemCount", index, "Reward", i);
                rewardItemCounts.Add(itemCount);
            }
            for (int i = 0; i < rewardCount; i++)
            {
                for (int j = 0; j < rewardItemCounts[i]; j++)
                    ReadItemInstance152(packet, index, "Reward", i, j);
            }

            // Overmax rewards at +128: 24-byte entries, then 152-byte ItemInstances
            var overmaxCount = packet.ReadInt32("OvermaxRewardCount", index);
            var overmaxItemCounts = new List<int>();
            for (int i = 0; i < overmaxCount; i++)
            {
                var itemCount = packet.ReadInt32("OvermaxItemCount", index, "Overmax", i);
                overmaxItemCounts.Add(itemCount);
            }
            for (int i = 0; i < overmaxCount; i++)
            {
                for (int j = 0; j < overmaxItemCounts[i]; j++)
                    ReadItemInstance152(packet, index, "OvermaxReward", i, j);
            }
        }

        // 200-byte archived/active mission (sub_1407F0D00 in IDA)
        // Layout: int64 + 9×int32 + encounter ID array (int32s) + 3×int32 + byte + int64 + string
        private static void ReadGarrisonMission200(Packet packet, params object[] index)
        {
            packet.ReadInt64("DbID", index);
            packet.ReadInt32("MissionRecID", index);
            packet.ReadInt32("Unk12", index);
            packet.ReadInt32("Unk16", index);
            packet.ReadInt32("Unk20", index);
            packet.ReadInt32("Unk24", index);
            packet.ReadInt32("Unk28", index);
            packet.ReadInt32("Unk32", index);
            packet.ReadInt32("Unk36", index);
            packet.ReadInt32("Unk40", index);

            // Encounter IDs: int32 array at offset 48, count at offset 56
            var encounterCount = packet.ReadInt32("EncounterCount", index);
            for (int i = 0; i < encounterCount; i++)
                packet.ReadInt32("GarrEncounterID", index, i);

            packet.ReadInt32("Unk72", index);
            packet.ReadInt32("Unk76", index);
            packet.ReadInt32("Unk180", index);
            packet.ReadByte("Unk184", index);
            packet.ReadInt64("Unk192", index);

            // String at offset 80: length is (byte >> 1), ReadWoWString
            var strLenByte = packet.ReadByte("StringLength", index);
            var strLen = strLenByte >> 1;
            packet.ReadWoWString("StringData", strLen, index);
        }

        // 56-byte encounter from daily missions
        // IDA layout (per encounter): int32(GarrEncounterID) + mechanic array(int32s) + 4×int32 + byte(BoardIndex)
        private static void ReadGarrisonEncounter56(Packet packet, params object[] index)
        {
            packet.ReadInt32("GarrEncounterID", index);
            var mechanicCount = packet.ReadInt32("MechanicCount", index);
            packet.ReadInt32("Unk32", index);
            packet.ReadInt32("Unk36", index);
            packet.ReadInt32("Unk40", index);
            packet.ReadInt32("Unk44", index);
            packet.ReadSByte("BoardIndex", index);

            for (int i = 0; i < mechanicCount; i++)
                packet.ReadInt32("MechanicID", index, i);
        }

        // 152-byte ItemInstance (sub_1407F12B0 in IDA)
        // Layout: ReadGarrisonMissionReward header (7×int32) + byte(HasItemInstance) + conditional ReadItemInstance at +32
        private static void ReadItemInstance152(Packet packet, params object[] index)
        {
            packet.ReadInt32("ItemID", index);
            packet.ReadInt32("ItemQuantity", index);
            packet.ReadInt32("CurrencyID", index);
            packet.ReadInt32("CurrencyQuantity", index);
            packet.ReadInt32("FollowerXP", index);
            packet.ReadInt32("GarrMssnBonusAbilityID", index);
            packet.ReadInt32("ItemFileDataID", index);
            var hasItemInstance = packet.ReadByte("HasItemInstance", index);

            if (hasItemInstance != 0)
                ReadItemInstance(packet, index);
        }

        // Standard ItemInstance (embedded inside the 152-byte struct at offset +32)
        private static void ReadItemInstance(Packet packet, params object[] index)
        {
            packet.ReadInt32("ItemID", index);

            packet.ResetBitReader();
            var hasBonuses = packet.ReadBit("HasItemBonus", index);

            var modifierCount = packet.ReadByte("ModifierCount", index);

            for (int i = 0; i < modifierCount; i++)
            {
                packet.ReadInt32("ModifierID", index, i);
                packet.ReadByte("ModifierValue", index, i);
            }

            if (hasBonuses)
            {
                packet.ReadByte("UnkByte", index);
                var bonusIdCount = packet.ReadInt32("BonusIdCount", index);
                for (int i = 0; i < bonusIdCount; i++)
                {
                    packet.ReadInt32("BonusId", index, i);
                }
            }
        }
    }
}
