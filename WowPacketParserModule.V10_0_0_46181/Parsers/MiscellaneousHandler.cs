using System;
using System.Collections.Generic;
using System.Reflection;
using WowPacketParser.DBC;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using DBCStore = WowPacketParser.DBC.DBC;

namespace WowPacketParserModule.V10_0_0_46181.Parsers
{
    public static class MiscellaneousHandler
    {
        // Track current FirstCraftTreasureID from SMSG_SET_CURRENCY for SMSG_DISPLAY_TOAST
        private static uint _currentFirstCraftTreasureID = 0;

        // Map OperationID -> CraftingDataID (which links to FirstCraftTreasureID)
        private static readonly Dictionary<uint, int> _operationToCraftingData = new();

        // ... (rest of the code remains the same)

        [Parser(Opcode.SMSG_SET_CURRENCY)]
        public static void HandleSetCurrency(Packet packet)
        {
            packet.ReadInt32("Type");
            packet.ReadInt32("Quantity");
            packet.ReadUInt32("Flags");
            uint toastCount = packet.ReadUInt32("UiEventToastCount");
            for (var i = 0; i < toastCount; i++)
                ItemHandler.ReadUIEventToast(packet, "UiEventToast", i);

            var hasWeeklyQuantity = packet.ReadBit("HasWeeklyQuantity");
            var hasTrackedQuantity = packet.ReadBit("HasTrackedQuantity");
            var hasMaxQuantity = packet.ReadBit("HasMaxQuantity");
            var hasTotalEarned = packet.ReadBit("HasTotalEarned");
            packet.ReadBit("SuppressChatLog");
            var hasQuantityChange = packet.ReadBit("HasQuantityChange");
            var hasQuantityLostSource = packet.ReadBit("HasQuantityLostSource");
            var hasQuantityGainSource = packet.ReadBit("HasQuantityGainSource");
            var hasFirstCraftOperationID = packet.ReadBit("HasFirstCraftOperationID");
            var hasHasNextRechargeTime = packet.ReadBit("HasNextRechargeTime");
            var hasRechargeCycleStartTime = false;
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_1_0_49407))
                hasRechargeCycleStartTime = packet.ReadBit("HasRechargeCycleStartTime");

            if (hasWeeklyQuantity)
                packet.ReadInt32("WeeklyQuantity");

            if (hasTrackedQuantity)
                packet.ReadInt32("TrackedQuantity");

            if (hasMaxQuantity)
                packet.ReadInt32("MaxQuantity");

            if (hasTotalEarned)
                packet.ReadInt32("TotalEarned");

            if (hasQuantityChange)
                packet.ReadInt32("QuantityChange");

            if (hasQuantityLostSource)
                packet.ReadInt32("QuantityLostSource");

            if (hasQuantityGainSource)
                packet.ReadInt32("QuantityGainSource");

            if (hasFirstCraftOperationID)
            {
                var operationId = packet.ReadUInt32("FirstCraftOperationID");
                
                // If DBC is not loaded, set TreasureID to 0
                if (!Settings.UseDBC)
                {
                    _currentFirstCraftTreasureID = 0;
                }
                else
                {
                    // Try to get TreasureID from the OperationID -> CraftingData mapping
                    _currentFirstCraftTreasureID = GetTreasureIdFromOperation(operationId);
                }
            }

            if (hasHasNextRechargeTime)
                packet.ReadTime64("NextRechargeTime");

            if (hasRechargeCycleStartTime)
                packet.ReadTime64("RechargeCycleStartTime");
        }

        [Parser(Opcode.SMSG_DISPLAY_TOAST)]
        public static void HandleDisplayToast(Packet packet)
        {
            var quantity = packet.ReadUInt64("Quantity");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_4_1_57294))
                packet.ReadUInt32("DisplayToastMethod");
            else
                packet.ReadByte("DisplayToastMethod");

            packet.ReadUInt32("QuestID");

            packet.ResetBitReader();

            packet.ReadBit("Mailed");
            var type = packet.ReadBits("Type", 2);
            packet.ReadBit("IsSecondaryResult");

            uint itemId = 0;
            uint currencyId = 0;

            switch (type)
            {
                case 0:
                    packet.ReadBit("BonusRoll");
                    var itemInstance = Substructures.ItemHandler.ReadItemInstance(packet);
                    if (itemInstance != null)
                        itemId = (uint)itemInstance.ItemID;
                    packet.ReadInt32("LootSpec");
                    packet.ReadSByte("Gender");
                    break;
                case 1:
                    currencyId = packet.ReadUInt32("CurrencyID");
                    break;
            }

            // Store treasure loot data if we have a valid FirstCraftTreasureID
            if (_currentFirstCraftTreasureID > 0 && quantity > 0)
            {
                // Only store if we have valid data
                if ((type == 0 && itemId > 0) || (type == 1 && currencyId > 0))
                {
                    var treasureLoot = new TreasureLootTemplate
                    {
                        TreasureID = _currentFirstCraftTreasureID,
                        Item = type == 0 ? itemId : 0,
                        Currency = type == 1 ? currencyId : 0,
                        Chance = 100,
                        GroupID = 0,
                        MinCount = (uint)quantity,
                        MaxCount = (uint)quantity,
                        Comment = $"From spell {_currentFirstCraftTreasureID}",
                        TimeSpan = packet.TimeSpan
                    };
                    Storage.TreasureLootTemplates.Add(treasureLoot);
                }
            }
        }

        [Parser(Opcode.SMSG_START_TIMER)]
        public static void HandleStartTimer(Packet packet)
        {
            packet.ReadInt64("TotalTime");
            if (ClientVersion.RemovedInVersion(ClientVersionBuild.V10_2_7_54577))
                packet.ReadInt64("TimeLeft");
            packet.ReadUInt32E<TimerType>("Type");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_2_7_54577))
            {
                packet.ReadInt64("TimeLeft");
                var hasPlayerGUID = packet.ReadBit("HasPlayerGUID");
                if (hasPlayerGUID)
                    packet.ReadPackedGuid128("PlayerGUID");
            }
        }

        // Fluxurion >
        [Parser(Opcode.CMSG_RPE_RESET_CHARACTER)]
        public static void HandleRPEResetChar(Packet packet)
        {
            packet.ReadPackedGuid128("CharGuid");
            packet.ReadByte("Spec");
            packet.ReadByte("byte");
            packet.ReadByte("byte");
            packet.ReadByte("byte");
            packet.ReadBit("ResetQuests");
        }

        public static void ReadGameRuleValuePair(Packet packet, params object[] indexes)
        {
            packet.ReadInt32("Rule", indexes);
            packet.ReadInt32("Value", indexes);
        }

        [Parser(Opcode.SMSG_FEATURE_SYSTEM_STATUS)]
        public static void HandleFeatureSystemStatus(Packet packet)
        {
            packet.ReadByte("ComplaintStatus");

            if (ClientVersion.RemovedInVersion(ClientVersionBuild.V10_0_5_47777))
            {
                packet.ReadUInt32("ScrollOfResurrectionRequestsRemaining");
                packet.ReadUInt32("ScrollOfResurrectionMaxRequestsPerDay");
            }
            packet.ReadUInt32("CfgRealmID");
            packet.ReadInt32("CfgRealmRecID");

            packet.ReadUInt32("MaxRecruits", "RAFSystem");
            packet.ReadUInt32("MaxRecruitMonths", "RAFSystem");
            packet.ReadUInt32("MaxRecruitmentUses", "RAFSystem");
            packet.ReadUInt32("DaysInCycle", "RAFSystem");
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_0_7_48676))
            {
                packet.ReadUInt32("Unknown1007", "RAFSystem");
            }
            else
            {
                packet.ReadUInt32("TwitterPostThrottleLimit");
                packet.ReadUInt32("TwitterPostThrottleCooldown");
            }

            packet.ReadUInt32("TokenPollTimeSeconds");
            packet.ReadUInt32("KioskSessionMinutes");
            packet.ReadInt64("TokenBalanceAmount");

            packet.ReadUInt32("BpayStoreProductDeliveryDelay");

            packet.ReadUInt32("ClubsPresenceUpdateTimer");
            packet.ReadUInt32("HiddenUIClubsPresenceUpdateTimer");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V9_2_0_42423))
            {
                packet.ReadInt32("ActiveSeason");
                var gameRuleValuesCount = packet.ReadUInt32("GameRuleValuesCount");

                packet.ReadInt16("MaxPlayerNameQueriesPerPacket");
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V9_2_7_45114))
                    packet.ReadInt16("PlayerNameQueryTelemetryInterval");
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_0_2_46479))
                    packet.ReadUInt32("PlayerNameQueryInterval");

                // NEW: Addon chat throttle
                packet.ReadInt32("MaxTries", "AddonChatThrottle");
                packet.ReadInt32("TriesRestoredPerSecond", "AddonChatThrottle");
                packet.ReadInt32("UsedTriesPerMessage", "AddonChatThrottle");

                for (var i = 0; i < gameRuleValuesCount; ++i)
                    ReadGameRuleValuePair(packet, "GameRuleValues");
            }

            packet.ResetBitReader();
            packet.ReadBit("VoiceEnabled");
            var hasEuropaTicketSystemStatus = packet.ReadBit("HasEuropaTicketSystemStatus");
            if (ClientVersion.RemovedInVersion(ClientVersionBuild.V10_0_5_47777))
                packet.ReadBit("ScrollOfResurrectionEnabled");
            packet.ReadBit("BpayStoreEnabled");
            packet.ReadBit("BpayStoreAvailable");
            packet.ReadBit("BpayStoreDisabledByParentalControls");
            packet.ReadBit("ItemRestorationButtonEnabled");
            packet.ReadBit("BrowserEnabled");
            var hasSessionAlert = packet.ReadBit("HasSessionAlert");

            packet.ReadBit("Enabled", "RAFSystem");
            packet.ReadBit("RecruitingEnabled", "RAFSystem");
            packet.ReadBit("CharUndeleteEnabled");
            packet.ReadBit("RestrictedAccount");
            packet.ReadBit("CommerceSystemEnabled");
            packet.ReadBit("TutorialsEnabled");
            if (ClientVersion.RemovedInVersion(ClientVersionBuild.V10_0_7_48676))
                packet.ReadBit("TwitterEnabled");
            packet.ReadBit("Unk67");
            packet.ReadBit("WillKickFromWorld");

            packet.ReadBit("KioskModeEnabled");
            packet.ReadBit("CompetitiveModeEnabled");
            packet.ReadBit("TokenBalanceEnabled");
            packet.ReadBit("WarModeFeatureEnabled");
            packet.ReadBit("ClubsEnabled");
            packet.ReadBit("ClubsBattleNetClubTypeAllowed");
            packet.ReadBit("ClubsCharacterClubTypeAllowed");
            packet.ReadBit("ClubsPresenceUpdateEnabled");

            packet.ReadBit("VoiceChatDisabledByParentalControl");
            packet.ReadBit("VoiceChatMutedByParentalControl");
            packet.ReadBit("QuestSessionEnabled");
            packet.ReadBit("IsMuted");
            packet.ReadBit("ClubFinderEnabled");
            packet.ReadBit("CommunityFinderEnabled"); // NEW: was Unknown901CheckoutRelated
            packet.ReadBit("Unknown901CheckoutRelated");
            packet.ReadBit("TextToSpeechFeatureEnabled");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V9_1_5_40772))
            {
                packet.ReadBit("ChatDisabledByDefault");
                packet.ReadBit("ChatDisabledByPlayer");
                packet.ReadBit("LFGListCustomRequiresAuthenticator");
            }

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_0_2_46479))
            {
                packet.ReadBit("AddonsDisabled");
                packet.ReadBit("WarGamesEnabled"); // NEW: was Unused1000
            }

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_1_5_50232))
            {
                packet.ReadBit("ContentTrackingEnabled");
                packet.ReadBit("IsSellAllJunkEnabled"); // NEW
                packet.ReadBit("IsGroupFinderEnabled"); // NEW
            }

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_2_7_54577))
            {
                packet.ReadBit("IsLFDEnabled"); // NEW
                packet.ReadBit("IsLFREnabled"); // NEW
                packet.ReadBit("IsPremadeGroupEnabled"); // NEW
                packet.ReadBit("CanShowSetRoleButton"); // NEW
                packet.ReadBit("Unused1027_1"); // unused 10.2.7
                packet.ReadBit("Unused1027_2"); // unused 10.2.7
            }

            var unknown1027Len = packet.ReadBits("Unknown1027Length", 7);

            packet.ResetBitReader();

            {
                packet.ReadBit("ToastsDisabled", "QuickJoinConfig");
                packet.ReadSingle("ToastDuration", "QuickJoinConfig");
                packet.ReadSingle("DelayDuration", "QuickJoinConfig");
                packet.ReadSingle("QueueMultiplier", "QuickJoinConfig");
                packet.ReadSingle("PlayerMultiplier", "QuickJoinConfig");
                packet.ReadSingle("PlayerFriendValue", "QuickJoinConfig");
                packet.ReadSingle("PlayerGuildValue", "QuickJoinConfig");
                packet.ReadSingle("ThrottleInitialThreshold", "QuickJoinConfig");
                packet.ReadSingle("ThrottleDecayTime", "QuickJoinConfig");
                packet.ReadSingle("ThrottlePrioritySpike", "QuickJoinConfig");
                packet.ReadSingle("ThrottleMinThreshold", "QuickJoinConfig");
                packet.ReadSingle("ThrottlePvPPriorityNormal", "QuickJoinConfig");
                packet.ReadSingle("ThrottlePvPPriorityLow", "QuickJoinConfig");
                packet.ReadSingle("ThrottlePvPHonorThreshold", "QuickJoinConfig");
                packet.ReadSingle("ThrottleLfgListPriorityDefault", "QuickJoinConfig");
                packet.ReadSingle("ThrottleLfgListPriorityAbove", "QuickJoinConfig");
                packet.ReadSingle("ThrottleLfgListPriorityBelow", "QuickJoinConfig");
                packet.ReadSingle("ThrottleLfgListIlvlScalingAbove", "QuickJoinConfig");
                packet.ReadSingle("ThrottleLfgListIlvlScalingBelow", "QuickJoinConfig");
                packet.ReadSingle("ThrottleRfPriorityAbove", "QuickJoinConfig");
                packet.ReadSingle("ThrottleRfIlvlScalingAbove", "QuickJoinConfig");
                packet.ReadSingle("ThrottleDfMaxItemLevel", "QuickJoinConfig");
                packet.ReadSingle("ThrottleDfBestPriority", "QuickJoinConfig");
            }

            if (hasSessionAlert)
                V6_0_2_19033.Parsers.MiscellaneousHandler.ReadClientSessionAlertConfig(packet, "SessionAlert");

            if (unknown1027Len > 0)
                packet.ReadWoWString("Unknown1027", (int)unknown1027Len);

            // NEW: Squelch info
            {
                packet.ResetBitReader();
                packet.ReadBit("IsSquelched", "Squelch");
                packet.ReadPackedGuid128("BnetAccountGuid", "Squelch");
                packet.ReadPackedGuid128("GuildGuid", "Squelch");
            }

            V8_0_1_27101.Parsers.MiscellaneousHandler.ReadVoiceChatManagerSettings(packet, "VoiceChatManagerSettings");

            if (hasEuropaTicketSystemStatus)
            {
                packet.ResetBitReader();
                V6_0_2_19033.Parsers.MiscellaneousHandler.ReadCliEuropaTicketConfig(packet, "EuropaTicketSystemStatus");
            }
        }


        [Parser(Opcode.SMSG_FEATURE_SYSTEM_STATUS2)]
        public static void HandleFeatureSystemStatus2(Packet packet)
        {
            packet.ReadBit("TextToSpeechFeatureEnabled");
        }

        public static void ReadDebugTimeInfo(Packet packet, params object[] indexes)
        {
            packet.ReadUInt32("TimeEvent", indexes);
            packet.ResetBitReader();
            var textLen = packet.ReadBits(7);
            packet.ReadWoWString("Text", (int)textLen, indexes);
        }


        [Parser(Opcode.SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN)]
        public static void HandleFeatureSystemStatusGlueScreen(Packet packet)
        {
            packet.ResetBitReader();
            
            packet.ReadBit("BpayStoreEnabled");
            packet.ReadBit("BpayStoreAvailable");
            packet.ReadBit("BpayStoreDisabledByParentalControls");
            packet.ReadBit("CharUndeleteEnabled");
            packet.ReadBit("CommerceSystemEnabled");
            packet.ReadBit("Unk14");
            packet.ReadBit("WillKickFromWorld");
            packet.ReadBit("IsExpansionPreorderInStore");
            
            packet.ReadBit("KioskModeEnabled");
            packet.ReadBit("IsCompetitiveModeEnabled");
            packet.ReadBit("IsBoostEnabled"); // NEW: was Unused1002_1
            packet.ReadBit("TrialBoostEnabled");
            packet.ReadBit("TokenBalanceEnabled");
            packet.ReadBit("LiveRegionCharacterListEnabled");
            packet.ReadBit("LiveRegionCharacterCopyEnabled");
            packet.ReadBit("LiveRegionAccountCopyEnabled");

            packet.ReadBit("LiveRegionKeyBindingsCopyEnabled");
            packet.ReadBit("Unknown901CheckoutRelated");
            packet.ReadBit("Unused1002_2");
            var europaTicket = packet.ReadBit("IsEuropaTicketSystemStatusEnabled");
            packet.ReadBit("IsNameReservationEnabled"); // NEW
            var launchEta = packet.ReadBit("HasLaunchETA");
            packet.ReadBit("TimerunningEnabled"); // NEW
            packet.ReadBit("AddonsDisabled");
            
            packet.ReadBit("Unused1000");
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_0_7_48676))
            {
                packet.ReadBit("AccountSaveDataExportEnabled");
                packet.ReadBit("AccountLockedByExport");
            }

            var realmHiddenAlertLen = packet.ReadBits("RealmHiddenAlertLength", 11);

            packet.ResetBitReader();

            if (europaTicket)
                V6_0_2_19033.Parsers.MiscellaneousHandler.ReadCliEuropaTicketConfig(packet, "EuropaTicketSystemStatus");

            packet.ReadUInt32("TokenPollTimeSeconds");
            packet.ReadUInt32("KioskSessionMinutes");
            packet.ReadInt64("TokenBalanceAmount");
            packet.ReadInt32("MaxCharactersPerRealm");
            var liveRegionCharacterCopySourceRegionsCount = packet.ReadUInt32("LiveRegionCharacterCopySourceRegionsCount");
            packet.ReadUInt32("BpayStoreProductDeliveryDelay");
            packet.ReadInt32("ActiveCharacterUpgradeBoostType");
            packet.ReadInt32("ActiveClassTrialBoostType");
            packet.ReadInt32("MinimumExpansionLevel");
            packet.ReadInt32("MaximumExpansionLevel");

            var gameRuleValuesCount = 0u;

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V9_2_0_42423))
            {
                packet.ReadInt32("ActiveSeason");
                gameRuleValuesCount = packet.ReadUInt32("GameRuleValuesCount");
                packet.ReadInt32("ActiveTimerunningSeasonID"); // NEW
                packet.ReadInt32("RemainingTimerunningSeasonSeconds"); // NEW
                packet.ReadInt16("MaxPlayerNameQueriesPerPacket");
            }

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V9_2_7_45114))
                packet.ReadInt16("PlayerNameQueryTelemetryInterval");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_0_2_46479))
                packet.ReadUInt32("PlayerNameQueryInterval");

            uint debugTimeEventCount = 0;
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V10_0_7_48676))
            {
                debugTimeEventCount = packet.ReadUInt32("DebugTimeEventCount");
                packet.ReadInt32("Unused1007");
                packet.ReadUInt32("EventRealmQueues"); // NEW
            }

            if (launchEta)
                packet.ReadInt32("LaunchETA");

            if (realmHiddenAlertLen > 0)
                packet.ReadWoWString("RealmHiddenAlert", (int)realmHiddenAlertLen - 1); // -1 for null terminator

            for (int i = 0; i < liveRegionCharacterCopySourceRegionsCount; i++)
                packet.ReadUInt32("LiveRegionCharacterCopySourceRegion", i);

            for (var i = 0; i < gameRuleValuesCount; ++i)
                ReadGameRuleValuePair(packet, "GameRuleValues", i);

            for (var i = 0; i < debugTimeEventCount; ++i)
                ReadDebugTimeInfo(packet, "DebugTimeEvent", i);
        }

        [Parser(Opcode.SMSG_SET_TIME_ZONE_INFORMATION)]
        public static void HandleSetTimeZoneInformation(Packet packet)
        {
            packet.ResetBitReader();
            
            var serverTimeTZLen = packet.ReadBits("ServerTimeTZLength", 7);
            var gameTimeTZLen = packet.ReadBits("GameTimeTZLength", 7);
            var serverRegionalTZLen = packet.ReadBits("ServerRegionalTZLength", 7);
            
            packet.ResetBitReader();

            packet.ReadWoWString("ServerTimeTZ", (int)serverTimeTZLen);
            packet.ReadWoWString("GameTimeTZ", (int)gameTimeTZLen);
            packet.ReadWoWString("ServerRegionalTZ", (int)serverRegionalTZLen);
        }

        /// <summary>
        /// Gets the TreasureID from CraftingData DB2 using OperationID.
        /// We track OperationID -> CraftingDataID from craft result packets,
        /// then look up FirstCraftTreasureID from CraftingData DB2.
        /// </summary>
        public static uint GetTreasureIdFromOperation(uint operationId)
        {
            // Look up CraftingDataID from the operation mapping
            if (_operationToCraftingData.TryGetValue(operationId, out var craftingDataId))
            {
                return DBCStore.GetFirstCraftTreasureID(craftingDataId);
            }
            return 0;
        }

        /// <summary>
        /// Called from ReadCraftingData to track OperationID -> CraftingDataID mapping.
        /// </summary>
        public static void TrackCraftingOperation(uint operationId, int craftingDataId)
        {
            if (operationId > 0 && craftingDataId > 0)
            {
                _operationToCraftingData[operationId] = craftingDataId;
            }
        }

        /// <summary>
        /// Gets the TreasureID from CraftingData DB2 using spell misc values.
        /// FirstCraftOperationID is a spell ID. We look up its spell effects,
        /// get the EffectMiscValue which points to CraftingData.ID, then get FirstCraftTreasureID.
        /// </summary>
        private static uint GetTreasureIdFromSpell(uint spellId)
        {
            // Look through spell effects for this spell
            for (uint i = 0; i < 32; i++)
            {
                var tuple = Tuple.Create(spellId, i);
                if (DBCStore.SpellEffectStores.TryGetValue(tuple, out var effect))
                {
                    // EffectMiscValue[0] should point to CraftingData.ID
                    var craftingDataId = effect.EffectMiscValue[0];
                    if (craftingDataId > 0)
                    {
                        return DBCStore.GetFirstCraftTreasureID(craftingDataId);
                    }
                }
            }
            return 0;
        }
    }
}