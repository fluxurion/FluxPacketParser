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
        [Parser(Opcode.SMSG_GARRISON_ADD_MISSION_RESULT)]
        public static void HandleGarrisonAddMissionResult(Packet packet)
        {
            packet.ReadInt32("Result");
            packet.ReadInt32("GarrTypeID");
            packet.ReadByte("State");
            packet.ReadBit("CanStart");

            ReadGarrisonMission(packet, "MissionData");
        }

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

            var encounterCount = packet.ReadInt32("EncounterCount", index);
            var rewardCount = packet.ReadInt32("RewardCount", index);
            var overmaxRewardCount = packet.ReadUInt32("OvermaxRewardCount", index);

            for (int i = 0; i < encounterCount; i++)
                ReadGarrisonEncounter(packet, "Encounter", index, i);
            for (int i = 0; i < rewardCount; i++)
                ReadGarrisonMissionReward(packet, "Reward", index, i);
            for (int i = 0; i < overmaxRewardCount; i++)
                ReadGarrisonMissionReward(packet, "OvermaxReward", index, i);
        }

        private static void ReadGarrisonEncounter(Packet packet, params object[] index)
        {
            packet.ReadInt32("GarrEncounterID", index);
            var mechanicCount = packet.ReadInt32("MechanicCount", index);
            packet.ReadInt32("GarrAutoCombatantID", index);
            packet.ReadInt32("Health", index);
            packet.ReadInt32("MaxHealth", index);
            packet.ReadInt32("Attack", index);
            packet.ReadSByte("BoardIndex", index);

            for (int i = 0; i < mechanicCount; i++)
                packet.ReadInt32("Mechanic", index, i);
        }

        private static void ReadGarrisonMissionReward(Packet packet, params object[] index)
        {
            packet.ReadInt32("ItemID", index);
            packet.ReadInt32("ItemQuantity", index);
            packet.ReadInt32("CurrencyID", index);
            packet.ReadInt32("CurrencyQuantity", index);
            packet.ReadInt32("FollowerXP", index);
            packet.ReadInt32("GarrMssnBonusAbilityID", index);
            packet.ReadInt32("ItemFileDataID", index);

            packet.ResetBitReader();
            var hasItemInstance = packet.ReadBit("HasItemInstance", index);

            if (hasItemInstance)
                ReadItemInstance(packet, index);
        }
        
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
