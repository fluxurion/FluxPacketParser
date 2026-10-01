using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using System;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V1_60_1_70009.Parsers
{
    // Classic 1.60.1.70009 packet layouts. Sourced from TrinityCoreLuaSol
    // (E:\TrinityCoreLuaSol) packet writers/readers — the 1.60 client speaks
    // the modern retail-style wire formats with Classic-specific deltas,
    // so the 1.15.x era handlers do not apply.
    public static class FluxMiscHandler
    {
        // 0x460001 — Classic 1.60.1.70009 uses the modern TC AuthResponse
        // layout (AuthenticationPackets.cpp, unmodified by LuaSol):
        // {u32 Result, flag byte (bit7 HasSuccessInfo, bit6 HasWaitInfo),
        //  optional AuthSuccessInfo, optional AuthWaitInfo}.
        private static void ReadVirtualRealmInfo160(Packet packet, params object[] idx)
        {
            packet.ReadUInt32("RealmAddress", idx);

            packet.ResetBitReader();
            packet.ReadBit("IsLocal", idx);
            packet.ReadBit("IsInternalRealm", idx);
            var actualNameLen = packet.ReadBits(8);
            var normalizedNameLen = packet.ReadBits(8);

            packet.ReadWoWString("RealmNameActual", actualNameLen, idx);
            packet.ReadWoWString("RealmNameNormalized", normalizedNameLen, idx);
        }

        [Parser(Opcode.SMSG_AUTH_RESPONSE, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleAuthResponse160(Packet packet)
        {
            packet.ReadUInt32E<BattlenetRpcErrorCode>("Result");

            packet.ResetBitReader();
            var hasSuccessInfo = packet.ReadBit("HasSuccessInfo");
            var hasWaitInfo = packet.ReadBit("HasWaitInfo");

            if (hasSuccessInfo)
            {
                packet.ReadUInt32("VirtualRealmAddress", "SuccessInfo");
                var realmCount = packet.ReadUInt32("VirtualRealms", "SuccessInfo");
                packet.ReadUInt32("TimeRested", "SuccessInfo");
                packet.ReadByte("ActiveExpansionLevel", "SuccessInfo");
                packet.ReadByte("AccountExpansionLevel", "SuccessInfo");
                packet.ReadUInt32("TimeSecondsUntilPCKick", "SuccessInfo");
                var classCount = packet.ReadUInt32("AvailableClasses", "SuccessInfo");
                var templateCount = packet.ReadUInt32("Templates", "SuccessInfo");
                packet.ReadUInt32("CurrencyID", "SuccessInfo");

                // GameTime
                packet.ReadUInt32("BillingType", "SuccessInfo", "GameTimeInfo");
                packet.ReadUInt32("MinutesRemaining", "SuccessInfo", "GameTimeInfo");
                packet.ReadUInt32("RealBillingType", "SuccessInfo", "GameTimeInfo");
                packet.ResetBitReader();
                packet.ReadBit("IsInIGR", "SuccessInfo", "GameTimeInfo");
                packet.ReadBit("IsPaidForByIGR", "SuccessInfo", "GameTimeInfo");
                packet.ReadBit("IsCAISEnabled", "SuccessInfo", "GameTimeInfo");

                packet.ReadTime64("Time", "SuccessInfo");

                for (var i = 0u; i < realmCount; ++i)
                    ReadVirtualRealmInfo160(packet, "SuccessInfo", "VirtualRealms", i);

                for (var i = 0u; i < classCount; ++i)
                {
                    packet.ReadByteE<Race>("RaceID", "SuccessInfo", "AvailableClasses", i);
                    var classesForRace = packet.ReadUInt32();
                    for (var j = 0u; j < classesForRace; ++j)
                    {
                        packet.ReadByteE<Class>("ClassID", "SuccessInfo", "AvailableClasses", i, "Classes", j);
                        packet.ReadByteE<ClientType>("ActiveExpansionLevel", "SuccessInfo", "AvailableClasses", i, "Classes", j);
                        packet.ReadByteE<ClientType>("AccountExpansionLevel", "SuccessInfo", "AvailableClasses", i, "Classes", j);
                        packet.ReadByte("MinActiveExpansionLevel", "SuccessInfo", "AvailableClasses", i, "Classes", j);
                    }
                }

                for (var i = 0u; i < templateCount; ++i)
                {
                    packet.ReadUInt32("TemplateSetId", "SuccessInfo", "Templates", i);
                    var templateClasses = packet.ReadUInt32();
                    for (var j = 0u; j < templateClasses; ++j)
                    {
                        packet.ReadByteE<Class>("Class", "SuccessInfo", "Templates", i, "Classes", j);
                        packet.ReadByte("FactionGroup", "SuccessInfo", "Templates", i, "Classes", j);
                    }

                    packet.ResetBitReader();
                    var nameLen = packet.ReadBits(7);
                    var descLen = packet.ReadBits(10);
                    packet.ReadWoWString("Name", nameLen, "SuccessInfo", "Templates", i);
                    packet.ReadWoWString("Description", descLen, "SuccessInfo", "Templates", i);
                }

                packet.ResetBitReader();
                packet.ReadBit("IsExpansionTrial", "SuccessInfo");
                packet.ReadBit("ForceCharacterTemplate", "SuccessInfo");
                var hasNumPlayersHorde = packet.ReadBit();
                var hasNumPlayersAlliance = packet.ReadBit();
                var hasExpansionTrialExpiration = packet.ReadBit();
                var hasBuildKey = packet.ReadBit();
                packet.ResetBitReader();

                if (hasNumPlayersHorde)
                    packet.ReadUInt16("NumPlayersHorde", "SuccessInfo");

                if (hasNumPlayersAlliance)
                    packet.ReadUInt16("NumPlayersAlliance", "SuccessInfo");

                if (hasExpansionTrialExpiration)
                    packet.ReadTime64("ExpansionTrialExpiration", "SuccessInfo");

                if (hasBuildKey)
                    packet.ReadBytes("BuildKey", 32, "SuccessInfo");
            }

            if (hasWaitInfo)
            {
                packet.ReadUInt32("WaitCount", "WaitInfo");
                packet.ReadUInt32("WaitTime", "WaitInfo");
                packet.ReadByte("AllowedFactionGroupForCharacterCreate", "WaitInfo");
                packet.ResetBitReader();
                packet.ReadBit("HasFCM", "WaitInfo");
                packet.ReadBit("CanCreateOnlyIfExisting", "WaitInfo");
            }
        }

        // 0x460064 — Classic 1.60.1.70009 glue-screen layout (client reader
        // rva 0x7F9E70, SystemPackets.cpp): six raw flag bytes (retail bit
        // order does not apply and LuaSol sends them as zero), no
        // KioskSessionDurationMinutes, then the scalar/count fields.
        [Parser(Opcode.SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleFeatureSystemStatusGlueScreen160(Packet packet)
        {
            var flags = new byte[6];
            for (var i = 0; i < 6; ++i)
                flags[i] = packet.ReadByte("Flags", i);

            packet.ReadUInt32("CommercePricePollTimeSeconds");
            packet.ReadInt64("RedeemForBalanceAmount");
            packet.ReadInt32("MaxCharactersOnThisRealm");
            var liveRegionCharacterCopySourceRegionsCount = packet.ReadUInt32("LiveRegionCharacterCopySourceRegionsCount");
            packet.ReadInt32("ActiveBoostType");
            packet.ReadInt32("TrialBoostType");
            packet.ReadInt32("MinimumExpansionLevel");
            packet.ReadInt32("MaximumExpansionLevel");
            packet.ReadInt32("ContentSetID");
            var disabledGameModesCount = packet.ReadUInt32("DisabledGameModesCount");
            var gameRuleValuesCount = packet.ReadUInt32("GameRuleValuesCount");
            var availableGameModesCount = packet.ReadUInt32("AvailableGameModeIDCount");
            packet.ReadInt32("ActiveTimerunningSeasonID");
            packet.ReadInt32("RemainingTimerunningSeasonSeconds");
            packet.ReadInt32("TimerunningConversionMinCharacterAge");
            packet.ReadInt32("TimerunningConversionMaxSeasonID");
            packet.ReadInt16("MaxPlayerGuidLookupsPerRequest");
            packet.ReadInt16("NameLookupTelemetryInterval");
            packet.ReadUInt32("NotFoundCacheTimeSeconds");
            var debugTimeEventCount = packet.ReadUInt32("DebugTimeEventCount");
            packet.ReadInt32("MostRecentTimeEventID");
            packet.ReadUInt32("EventRealmQueues");

            // Client reader (0x7F9E70): flag byte 3 bit6 gates an optional
            // EuropaTicketConfig read BEFORE the array payloads. Wire form is
            // byte-aligned: u8 flags + 2x SavedThrottleObjectState (8x u32).
            if ((flags[2] & 0x40) != 0)
            {
                packet.ReadByte("EuropaTicketFlags");
                for (var i = 0; i < 2; ++i)
                {
                    var name = i == 0 ? "ThrottleState" : "ExpensiveThrottleState";
                    packet.ReadUInt32("MaxTries", "EuropaTicketConfig", name, i);
                    packet.ReadUInt32("PerMilliseconds", "EuropaTicketConfig", name, i);
                    packet.ReadUInt32("TryCount", "EuropaTicketConfig", name, i);
                    packet.ReadUInt32("LastResetTimeBeforeNow", "EuropaTicketConfig", name, i);
                }
            }

            if ((flags[2] & 0x10) != 0)
                packet.ReadInt32("LaunchDurationETA");

            // Trailing null-terminated string at reader offset +312: its byte
            // count (incl. terminator) is packed into flag bits — flag byte 4
            // (index 3) bit6 -> len bit10, flag byte 5 (index 4) bits4-7 ->
            // len bits0-3. len <= 1 means empty/absent.
            var tailStringLen = ((flags[4] >> 4) & 0xF) | (((flags[3] >> 6) & 1) << 10);
            if (tailStringLen > 1)
            {
                packet.ReadWoWString("UnknownTailString", (uint)(tailStringLen - 1));
                packet.ReadByte("UnknownTailStringTerminator");
            }

            for (var i = 0u; i < liveRegionCharacterCopySourceRegionsCount; ++i)
                packet.ReadUInt32("LiveRegionCharacterCopySourceRegion", i);

            for (var i = 0u; i < disabledGameModesCount; ++i)
            {
                packet.ReadByte("GameMode", "DisabledGameModes", i);
                packet.ReadInt32("ContentSetID", "DisabledGameModes", i);
                packet.ReadInt32("GameModeRecordID", "DisabledGameModes", i);
            }

            for (var i = 0u; i < gameRuleValuesCount; ++i)
            {
                packet.ReadInt32("Rule", "GameRuleValues", i);
                packet.ReadInt32("Value", "GameRuleValues", i);
                packet.ReadSingle("ValueF", "GameRuleValues", i);
            }

            for (var i = 0u; i < availableGameModesCount; ++i)
                packet.ReadInt32("AvailableGameModeID", i);

            // Client reads {i32, u8} per entry; the u8 packs a 7-bit text
            // length in its high bits (len = byte >> 1), then len bytes.
            for (var i = 0u; i < debugTimeEventCount; ++i)
            {
                packet.ReadInt32("TimeEvent", "DebugTimeEvent", i);
                var textByte = packet.ReadByte("TextPacked", "DebugTimeEvent", i);
                packet.ReadWoWString("Text", (uint)(textByte >> 1), "DebugTimeEvent", i);
            }
        }
        // ===== Classic 1.60.1.70009 variants =====
        // Sourced from TrinityCoreLuaSol packet writers (src/server/game/Server/
        // Packets/*.cpp) — the 1.60 client uses the modern retail-style layouts,
        // not the 1.15.x era forms.

        private static void ReadWarbandGroup160(Packet packet, params object[] idx)
        {
            packet.ReadUInt64("GroupID", idx);
            packet.ReadByte("OrderIndex", idx);
            packet.ReadInt32("WarbandSceneID", idx);
            packet.ReadInt32("Flags", idx);
            packet.ReadInt32("ContentSetID", idx);
            var memberCount = packet.ReadUInt32();
            for (var i = 0u; i < memberCount; ++i)
            {
                packet.ReadInt32("WarbandScenePlacementID", idx, "Members", i);
                var type = packet.ReadInt32("Type", idx, "Members", i);
                packet.ReadInt32("ContentSetID", idx, "Members", i);
                if (type == 0)
                    packet.ReadPackedGuid128("Guid", idx, "Members", i);
            }

            packet.ResetBitReader();
            var nameLength = packet.ReadBits(9);
            packet.ReadWoWString("Name", nameLength, idx);
        }

        // 0x4601B0 — Classic 1.60.1.70009 (TC DeleteChar::Write): {u32 Response}.
        [Parser(Opcode.SMSG_DELETE_CHAR, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleDeleteChar160(Packet packet)
        {
            packet.ReadUInt32E<ResponseCode>("Response");
        }

        // Classic 1.60.1.70009 (TC MirrorVarSingle writer, SystemPackets.cpp):
        // {1b UpdateType, 24b nameLen, 24b valueLen, FlushBits, name, value}.
        private static void ReadMirrorVarSingle160(Packet packet, params object[] indexes)
        {
            packet.ResetBitReader();
            packet.ReadBit("UpdateType", indexes);
            var nameLen = packet.ReadBits(24);
            var valueLen = packet.ReadBits(24);
            packet.ResetBitReader();

            var name = packet.ReadDynamicString("Name", nameLen, indexes);
            var value = packet.ReadDynamicString("Value", valueLen, indexes);
            packet.AddValue(name, value, indexes);
        }

        // 0x460368
        [Parser(Opcode.SMSG_MIRROR_VARS, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleMirrorVars160(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (var i = 0u; i < count; ++i)
                ReadMirrorVarSingle160(packet, i);
        }

        private static void ReadQuickJoinConfig160(Packet packet, params object[] idx)
        {
            packet.ReadSingle("ToastDuration", idx);
            packet.ReadSingle("DelayDuration", idx);
            packet.ReadSingle("QueueMultiplier", idx);
            packet.ReadSingle("PlayerMultiplier", idx);
            packet.ReadSingle("PlayerFriendValue", idx);
            packet.ReadSingle("PlayerGuildValue", idx);
            packet.ReadSingle("ThrottleInitialThreshold", idx);
            packet.ReadSingle("ThrottleDecayTime", idx);
            packet.ReadSingle("ThrottlePrioritySpike", idx);
            packet.ReadSingle("ThrottleMinThreshold", idx);
            packet.ReadSingle("ThrottlePvPPriorityNormal", idx);
            packet.ReadSingle("ThrottlePvPPriorityLow", idx);
            packet.ReadSingle("ThrottlePvPHonorThreshold", idx);
            packet.ReadSingle("ThrottleLfgListPriorityDefault", idx);
            packet.ReadSingle("ThrottleLfgListPriorityAbove", idx);
            packet.ReadSingle("ThrottleLfgListPriorityBelow", idx);
            packet.ReadSingle("ThrottleLfgListIlvlScalingAbove", idx);
            packet.ReadSingle("ThrottleLfgListIlvlScalingBelow", idx);
            packet.ReadSingle("ThrottleRfPriorityAbove", idx);
            packet.ReadSingle("ThrottleRfIlvlScalingAbove", idx);
            packet.ReadSingle("ThrottleDfMaxItemLevel", idx);
            packet.ReadSingle("ThrottleDfBestPriority", idx);
            packet.ResetBitReader();
            packet.ReadBit("ToastsDisabled", idx);
            packet.ResetBitReader();
        }

        // 0x460063 — Classic 1.60.1.70009 (client reader rva 0x7F9200,
        // TC FeatureSystemStatus::Write): no KioskSessionDurationMinutes,
        // ClubPresenceUnsubscribeDelay after ClubsPresenceDelay, inline
        // SquelchInfo {bit + 2 packed guids}, extra i32 SocialRestriction,
        // GameModeData/GameRuleValuePair element arrays, and a 52-bit flag
        // tail with a 10-bit Unknown1027 length.
        [Parser(Opcode.SMSG_FEATURE_SYSTEM_STATUS, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleFeatureSystemStatus160(Packet packet)
        {
            packet.ReadByte("ComplaintStatus");
            packet.ReadUInt32("CfgRealmID");
            packet.ReadInt32("CfgRealmRecID");

            packet.ReadUInt32("MaxRecruits", "RAFSystem");
            packet.ReadUInt32("MaxRecruitMonths", "RAFSystem");
            packet.ReadUInt32("MaxRecruitmentUses", "RAFSystem");
            packet.ReadUInt32("DaysInCycle", "RAFSystem");
            packet.ReadUInt32("RewardsVersion", "RAFSystem");

            packet.ReadUInt32("CommercePricePollTimeSeconds");
            ReadQuickJoinConfig160(packet, "QuickJoinConfig");
            packet.ReadInt64("RedeemForBalanceAmount");

            packet.ReadUInt32("ClubsPresenceDelay");
            packet.ReadUInt32("ClubPresenceUnsubscribeDelay");

            // SquelchInfo
            packet.ResetBitReader();
            packet.ReadBit("IsSquelched", "SquelchInfo");
            packet.ReadPackedGuid128("BnetAccountGuid", "SquelchInfo");
            packet.ReadPackedGuid128("GuildGuid", "SquelchInfo");

            packet.ReadInt32("ContentSetID");
            packet.ReadInt32("SocialRestriction");

            var disabledGameModesCount = packet.ReadUInt32("DisabledGameModesCount");
            var gameRuleValuesCount = packet.ReadUInt32("GameRuleValuesCount");

            packet.ReadInt32("ActiveTimerunningSeasonID");
            packet.ReadInt32("RemainingTimerunningSeasonSeconds");

            packet.ReadInt16("MaxPlayerGuidLookupsPerRequest");
            packet.ReadInt16("NameLookupTelemetryInterval");
            packet.ReadUInt32("NotFoundCacheTimeSeconds");

            packet.ReadUInt32("RealmPvpTypeOverride");

            packet.ReadInt32("MaxTries", "AddonChatThrottle");
            packet.ReadInt32("TriesRestoredPerSecond", "AddonChatThrottle");
            packet.ReadInt32("UsedTriesPerMessage", "AddonChatThrottle");
            packet.ReadInt32("UsedTriesPerMessage", "GuildChatThrottle");
            packet.ReadInt32("TriesRestoredPerSecond", "GuildChatThrottle");
            packet.ReadInt32("UsedTriesPerMessage", "GroupChatThrottle");
            packet.ReadInt32("TriesRestoredPerSecond", "GroupChatThrottle");

            packet.ReadSingle("AddonPerformanceMsgWarning");
            packet.ReadSingle("AddonPerformanceMsgError");
            packet.ReadSingle("AddonPerformanceMsgOverall");

            for (var i = 0u; i < disabledGameModesCount; ++i)
            {
                packet.ReadByte("GameMode", "DisabledGameModes", i);
                packet.ReadInt32("ContentSetID", "DisabledGameModes", i);
                packet.ReadInt32("GameModeRecordID", "DisabledGameModes", i);
            }

            for (var i = 0u; i < gameRuleValuesCount; ++i)
            {
                packet.ReadInt32("Rule", "GameRuleValues", i);
                packet.ReadInt32("Value", "GameRuleValues", i);
                packet.ReadSingle("ValueF", "GameRuleValues", i);
            }

            packet.ResetBitReader();
            packet.ReadBit("VoiceEnabled");
            var hasEuropaTicketSystemStatus = packet.ReadBit("HasEuropaTicketSystemStatus");
            packet.ReadBit("BpayStoreAvailable");
            packet.ReadBit("ItemRestorationButtonEnabled");
            var hasSessionAlert = packet.ReadBit("HasSessionAlert");
            packet.ReadBit("Enabled", "RAFSystem");
            packet.ReadBit("RecruitingEnabled", "RAFSystem");
            packet.ReadBit("CharUndeleteEnabled");

            packet.ReadBit("RestrictedAccount");
            packet.ReadBit("CommerceServerEnabled");
            packet.ReadBit("TutorialsEnabled");
            packet.ReadBit("VeteranTokenRedeemWillKick");
            packet.ReadBit("WorldTokenRedeemWillKick");
            packet.ReadBit("KioskModeEnabled");
            packet.ReadBit("CompetitiveModeEnabled");
            packet.ReadBit("RedeemForBalanceAvailable");

            packet.ReadBit("WarModeEnabled");
            packet.ReadBit("CommunitiesEnabled");
            packet.ReadBit("BnetGroupsEnabled");
            packet.ReadBit("CharacterCommunitiesEnabled");
            packet.ReadBit("ClubPresenceAllowSubscribeAll");
            packet.ReadBit("VoiceChatParentalDisabled");
            packet.ReadBit("VoiceChatParentalMuted");
            packet.ReadBit("QuestSessionEnabled");

            packet.ReadBit("IsChatMuted");
            packet.ReadBit("ClubFinderEnabled");
            packet.ReadBit("CommunityFinderEnabled");
            packet.ReadBit("BrowserCrashReporterEnabled");
            packet.ReadBit("SpeakForMeAllowed");
            packet.ReadBit("DoesAccountNeedAADCPrompt");
            packet.ReadBit("IsAccountOptedInToAADC");
            packet.ReadBit("LfgRequireAuthenticatorEnabled");

            packet.ReadBit("ScriptsDisallowedForBeta");
            packet.ReadBit("TimerunningEnabled");
            packet.ReadBit("PlayerIdentityOptionsEnabled");
            packet.ReadBit("IsPlayerContentTrackingEnabled");
            packet.ReadBit("LfdEnabled");
            packet.ReadBit("LfrEnabled");
            packet.ReadBit("PetHappinessEnabled");
            packet.ReadBit("GuildEventsEditsEnabled");

            packet.ReadBit("GuildTradeSkillsEnabled");
            var unknown1027StrLen = packet.ReadBits(10);
            packet.ReadBit("IsAccountCurrencyTransferEnabled");
            packet.ReadBit("NetEaseChatTelemetryEnabled");
            packet.ReadBit("LobbyMatchmakerQueueFromMainlineEnabled");
            packet.ReadBit("CanSendLobbyMatchmakerPartyCustomizations");
            packet.ReadBit("AddonProfilerEnabled");

            packet.ReadBit("GlobalUserGeneratedContentMuteEnabled");
            packet.ReadBit("AccountUserGeneratedContentIsRisky");
            packet.ReadBit("FriendsDisabled");

            packet.ResetBitReader();

            if (hasEuropaTicketSystemStatus)
            {
                packet.ResetBitReader();
                packet.ReadBit("TicketsEnabled", "EuropaTicketSystemStatus");
                packet.ReadBit("BugsEnabled", "EuropaTicketSystemStatus");
                packet.ReadBit("ComplaintsEnabled", "EuropaTicketSystemStatus");
                packet.ReadBit("SuggestionsEnabled", "EuropaTicketSystemStatus");
                packet.ResetBitReader();

                for (var i = 0; i < 2; ++i)
                {
                    packet.ReadUInt32("MaxTries", "EuropaTicketSystemStatus", "Throttle", i);
                    packet.ReadUInt32("PerMilliseconds", "EuropaTicketSystemStatus", "Throttle", i);
                    packet.ReadUInt32("TryCount", "EuropaTicketSystemStatus", "Throttle", i);
                    packet.ReadUInt32("LastResetTimeBeforeNow", "EuropaTicketSystemStatus", "Throttle", i);
                }
            }

            if (hasSessionAlert)
            {
                packet.ReadInt32("Delay", "SessionAlert");
                packet.ReadInt32("Period", "SessionAlert");
                packet.ReadInt32("DisplayTime", "SessionAlert");
            }

            packet.ReadWoWString("Unknown1027", unknown1027StrLen);
        }

        // 0x460018 — Classic 1.60.1.70009 enum-characters layout
        // (TC EnumCharactersResult::Write): retail/modern array order
        // (Characters, Regionwide, RaceUnlocks, ConditionalAppearances,
        // RaceLimitDisables, WarbandGroups), per-entry i32 SuperDistrictID and
        // a 6-bit surname length in the name bit block.
        private static void ReadVisualItemInfo160(Packet packet, params object[] idx)
        {
            packet.ReadUInt32("ItemID", idx);
            packet.ReadUInt32("TransmogrifiedItemID", idx);
            packet.ReadByte("Subclass", idx);
            packet.ReadByteE<InventoryType>("InvType", idx);
            packet.ReadUInt32("DisplayID", idx);
            packet.ReadUInt32("DisplayEnchantID", idx);
            packet.ReadInt32("SecondaryItemModifiedAppearanceID", idx);
            packet.ReadByte("SheatheCategory", idx);
        }

        private static void ReadBasicCharacterListEntry160(Packet packet, params object[] idx)
        {
            var playerGuid = packet.ReadPackedGuid128("Guid", idx);
            packet.ReadUInt32("VirtualRealmAddress", idx);
            packet.ReadUInt16("ListPosition", idx);
            var race = packet.ReadByteE<Race>("RaceID", idx);
            packet.ReadByteE<Gender>("SexID", idx);
            var @class = packet.ReadByteE<Class>("ClassID", idx);
            packet.ReadInt16("SpecID", idx);
            var customizationCount = packet.ReadUInt32();
            var level = packet.ReadByte("ExperienceLevel", idx);
            var mapId = packet.ReadInt32<MapId>("MapID", idx);
            var zone = packet.ReadInt32<ZoneId>("ZoneID", idx);
            var pos = packet.ReadVector3("PreloadPos", idx);
            packet.ReadUInt64("GuildClubMemberID", idx);
            packet.ReadPackedGuid128("GuildGUID", idx);
            packet.ReadUInt32("Flags", idx);
            packet.ReadUInt32("Flags2", idx);
            packet.ReadUInt32("Flags3", idx);
            packet.ReadUInt32("Flags4", idx);
            packet.ReadByte("CantLoginReason", idx);
            packet.ReadUInt32("PetCreatureDisplayID", idx);
            packet.ReadUInt32("PetExperienceLevel", idx);
            packet.ReadUInt32("PetCreatureFamilyID", idx);

            for (uint j = 0; j < 19; ++j)
                ReadVisualItemInfo160(packet, idx, "VisualItems", j);

            packet.ReadInt32("SaveVersion", idx);
            packet.ReadTime64("CreateTime", idx);
            packet.ReadTime64("LastActiveTime", idx);
            packet.ReadInt32("LastLoginVersion", idx);

            packet.ReadInt32("EmblemStyle", idx, "PersonalTabard");
            packet.ReadInt32("EmblemColor", idx, "PersonalTabard");
            packet.ReadInt32("BorderStyle", idx, "PersonalTabard");
            packet.ReadInt32("BorderColor", idx, "PersonalTabard");
            packet.ReadInt32("BackgroundColor", idx, "PersonalTabard");

            for (uint j = 0; j < 2; ++j)
                packet.ReadInt32("ProfessionIDs", idx, j);

            packet.ReadInt32("TimerunningSeasonID", idx);
            packet.ReadUInt32("OverrideSelectScreenFileDataID", idx);
            packet.ReadUInt32("RealmQueue", idx);
            // LuaSol: Classic 1.60.1.70009 — character's SuperDistrictID
            packet.ReadInt32("SuperDistrictID", idx);

            for (var j = 0u; j < customizationCount; ++j)
            {
                packet.ReadUInt32("ChrCustomizationOptionID", idx, "Customizations", j);
                packet.ReadUInt32("ChrCustomizationChoiceID", idx, "Customizations", j);
            }

            packet.ResetBitReader();

            var nameLength = packet.ReadBits(6);
            var surnameLength = packet.ReadBits(6);   // LuaSol: Classic 1.60
            var firstLogin = packet.ReadBit("FirstLogin", idx);
            packet.ReadBit("RealmInfoFound", idx);
            packet.ReadBit("IsRealmOffline", idx);

            var name = packet.ReadWoWString("Character Name", nameLength, idx);
            packet.ReadWoWString("Character Surname", surnameLength, idx);

            if (firstLogin)
            {
                PlayerCreateInfo startPos = new PlayerCreateInfo { Race = race, Class = @class, Map = (uint)mapId, Zone = (uint)zone, Position = pos, Orientation = 0 };
                Storage.StartPositions.Add(startPos, packet.TimeSpan);
            }

            var playerInfo = new Player { Race = race, Class = @class, Name = name, FirstLogin = firstLogin, Level = level, Type = ObjectType.Player };
            if (Storage.Objects.ContainsKey(playerGuid))
                Storage.Objects[playerGuid] = new Tuple<WoWObject, TimeSpan?>(playerInfo, packet.TimeSpan);
            else
                Storage.Objects.Add(playerGuid, playerInfo, packet.TimeSpan);
        }

        private static void ReadRestrictionAndMailData160(Packet packet, params object[] idx)
        {
            packet.ResetBitReader();
            packet.ReadBit("BoostInProgress", idx);
            packet.ReadBit("RpeAvailable", idx);

            packet.ReadUInt32("RestrictionFlags", idx);
            var mailSenderCount = packet.ReadUInt32();
            var mailSenderTypes = packet.ReadUInt32();
            packet.ReadInt32("NoRpeReason", idx);

            for (var j = 0; j < mailSenderTypes; ++j)
                packet.ReadUInt32("MailSenderType", idx, j);

            packet.ResetBitReader();

            var mailSenderLengths = new uint[mailSenderCount];
            for (var j = 0; j < mailSenderLengths.Length; ++j)
                mailSenderLengths[j] = (uint)packet.ReadBits(6);

            packet.ResetBitReader();

            for (var j = 0; j < mailSenderLengths.Length; ++j)
                packet.ReadDynamicString("MailSender", mailSenderLengths[j], idx);
        }

        private static void ReadCharacterListEntry160(Packet packet, params object[] idx)
        {
            ReadBasicCharacterListEntry160(packet, idx, "Basic");
            ReadRestrictionAndMailData160(packet, idx, "RestrictionsAndMails");
        }

        private static void ReadRegionwideCharacterListEntry160(Packet packet, params object[] idx)
        {
            ReadBasicCharacterListEntry160(packet, idx, "Basic");

            packet.ReadUInt64("Money", idx);
            packet.ReadSingle("AvgEquippedItemLevel", idx);
            packet.ReadSingle("CurrentSeasonMythicPlusOverallScore", idx);
            packet.ReadInt32("CurrentSeasonBestPvpRating", idx);
            packet.ReadByte("PvpRatingBracket", idx);
            packet.ReadInt16("PvpRatingAssociatedSpecID", idx);
        }

        private static void ReadClassUnlock160(Packet packet, params object[] idx)
        {
            packet.ReadSByte("ClassID", idx);
            packet.ReadUInt32("AchievementID", idx);
            packet.ResetBitReader();
            packet.ReadBit("HasExpansion", idx);
            packet.ReadBit("HasUnlockedAchievement", idx);
            packet.ReadBit("HasEntitlement", idx);
        }

        private static void ReadRaceUnlock160(Packet packet, params object[] idx)
        {
            packet.ReadSByte("RaceID", idx);
            var classCount = packet.ReadUInt32();

            for (var j = 0u; j < classCount; ++j)
                ReadClassUnlock160(packet, idx, "ClassUnlocks", j);

            packet.ResetBitReader();
            packet.ReadBit("HasUnlockedLicense", idx);
            packet.ReadBit("HasUnlockedAchievement", idx);
            packet.ReadBit("HasHeritageArmorUnlockAchievement", idx);
            packet.ReadBit("HasEntitlement", idx);
            packet.ReadBit("HideRaceOnClient", idx);
            packet.ReadBit("FactionBalanceDisabled", idx);
            packet.ReadBit("DoesNotHaveAvailableClasses", idx);
        }

        [Parser(Opcode.SMSG_ENUM_CHARACTERS_RESULT, ClientVersionBuild.V1_60_1_70009)]
        public static void HandleEnumCharactersResult160(Packet packet)
        {
            packet.ReadBit("Success");
            packet.ReadBit("Realmless");
            packet.ReadBit("IsDeletedCharacters");
            packet.ReadBit("IgnoreNewPlayerRestrictions");
            packet.ReadBit("IsRestrictedNewPlayer");
            packet.ReadBit("IsNewcomerChatCompleted");
            packet.ReadBit("IsRestrictedTrial");
            packet.ReadBit("IsAccountLapsedPlayer");
            var hasDisabledClassesMask = packet.ReadBit("HasClassDisableMask");
            packet.ReadBit("ForceCharacterListSort");

            var charsCount = packet.ReadUInt32("CharactersCount");
            var regionwideCharsCount = packet.ReadUInt32("RegionwideCharactersCount");
            packet.ReadInt32("MaxCharacterLevel");
            var raceUnlockCount = packet.ReadUInt32("RaceUnlockCount");
            var unlockedConditionalAppearanceCount = packet.ReadUInt32("UnlockedConditionalAppearanceCount");
            var raceLimitDisablesCount = packet.ReadUInt32("RaceLimitDisablesCount");
            var warbandGroupsCount = packet.ReadUInt32("WarbandGroupsCount");

            if (hasDisabledClassesMask)
                packet.ReadUInt32("ClassDisableMask");

            for (var i = 0u; i < charsCount; ++i)
                ReadCharacterListEntry160(packet, i, "Characters");

            for (var i = 0u; i < regionwideCharsCount; ++i)
                ReadRegionwideCharacterListEntry160(packet, i, "RegionwideCharacters");

            for (var i = 0u; i < raceUnlockCount; ++i)
                ReadRaceUnlock160(packet, i, "RaceUnlockData");

            for (var i = 0u; i < unlockedConditionalAppearanceCount; ++i)
            {
                packet.ReadInt32("AchievementID", "UnlockedConditionalAppearances", i);
                packet.ReadInt32("ConditionalType", "UnlockedConditionalAppearances", i);
            }

            for (var i = 0u; i < raceLimitDisablesCount; ++i)
            {
                packet.ReadSByte("RaceID", "RaceLimitDisableInfo", i);
                packet.ReadSByte("Reason", "RaceLimitDisableInfo", i);
            }

            for (var i = 0u; i < warbandGroupsCount; ++i)
                ReadWarbandGroup160(packet, i, "WarbandGroups");
        }
    }
}
