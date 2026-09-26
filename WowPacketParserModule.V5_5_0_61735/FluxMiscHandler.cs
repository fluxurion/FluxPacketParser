using System;
using System.Text;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V5_5_0_61735.Parsers
{
    public static class FluxMiscHandler
    {
        // 0x450007
        [Parser(Opcode.CMSG_UNK_NEW_CLASSIC, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleUnkNewClassic(Packet packet)
        {
            packet.ReadInt32("UnkInt32");
        }

        // 1.15.9: same field order as retail SMSG_AUTH_RESPONSE, but each bit-group
        // is packed into a raw flag byte and realm name lengths are MSB-first packed.
        private static void ReadVirtualRealmInfoEra(Packet packet, params object[] indexes)
        {
            packet.ReadUInt32("RealmAddress", indexes);

            var b0 = packet.ReadByte();
            var b1 = packet.ReadByte();
            var b2 = packet.ReadByte();
            packet.AddValue("IsLocal", (b0 & 0x80) != 0, indexes);
            packet.AddValue("IsHiddenFromPlayers", (b0 & 0x40) != 0, indexes);
            var actualNameLength = ((b0 & 0x3F) << 2) | (b1 >> 6);
            var normalizedNameLength = ((b1 & 0x3F) << 2) | (b2 >> 6);

            packet.ReadWoWString("RealmNameActual", actualNameLength, indexes);
            packet.ReadWoWString("RealmNameNormalized", normalizedNameLength, indexes);
        }

        // 0x460001
        [Parser(Opcode.SMSG_AUTH_RESPONSE, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleAuthResponseEra(Packet packet)
        {
            packet.ReadUInt32E<BattlenetRpcErrorCode>("Result");

            var flags = packet.ReadByte("Flags");
            var ok = (flags & 0x80) != 0;
            var queued = (flags & 0x40) != 0;
            packet.AddValue("Success", ok);
            packet.AddValue("Queued", queued);

            if (ok)
            {
                packet.ReadUInt32("VirtualRealmAddress");
                var realms = packet.ReadUInt32();
                packet.ReadUInt32("TimeRested");
                packet.ReadByte("ActiveExpansionLevel");
                packet.ReadByte("AccountExpansionLevel");
                packet.ReadUInt32("TimeSecondsUntilPCKick");
                var classes = packet.ReadUInt32("AvailableClasses");
                var templates = packet.ReadUInt32("Templates");
                packet.ReadUInt32("AccountCurrency");
                packet.ReadTime64("Time", "SuccessInfo");

                for (var i = 0; i < classes; ++i)
                {
                    packet.ReadByteE<Race>("RaceID", "AvailableClasses", i);
                    var classesForRace = packet.ReadUInt32();
                    for (var j = 0u; j < classesForRace; ++j)
                    {
                        packet.ReadByteE<Class>("ClassID", "AvailableClasses", i, "Classes", j);
                        packet.ReadByteE<ClientType>("ActiveExpansionLevel", "AvailableClasses", i, "Classes", j);
                        packet.ReadByteE<ClientType>("AccountExpansionLevel", "AvailableClasses", i, "Classes", j);
                        packet.ReadByte("MinActiveExpansionLevel", "AvailableClasses", i, "Classes", j);
                    }
                }

                var flags2 = packet.ReadByte("Flags2");
                packet.AddValue("IsExpansionTrial", (flags2 & 0x80) != 0);
                packet.AddValue("ForceCharacterTemplate", (flags2 & 0x40) != 0);
                var horde = (flags2 & 0x20) != 0;
                var alliance = (flags2 & 0x10) != 0;
                var trialExpiration = (flags2 & 0x08) != 0;
                var hasNewBuildKeys = (flags2 & 0x04) != 0;

                packet.ReadUInt32("BillingPlan");
                packet.ReadUInt32("TimeRemain");
                packet.ReadUInt32("Unk_V7_3_5");

                var roomFlags = packet.ReadByte("RoomFlags");
                packet.AddValue("InGameRoom", (roomFlags & 0x80) != 0);
                packet.AddValue("InGameRoom", (roomFlags & 0x40) != 0);
                packet.AddValue("InGameRoom", (roomFlags & 0x20) != 0);

                if (horde)
                    packet.ReadUInt16("NumPlayersHorde");

                if (alliance)
                    packet.ReadUInt16("NumPlayersAlliance");

                if (trialExpiration)
                    packet.ReadInt64("ExpansionTrialExpiration");

                if (hasNewBuildKeys)
                {
                    var newBuildKey = new byte[16];
                    var someKey = new byte[16];
                    for (var i = 0; i < 16; i++)
                    {
                        newBuildKey[i] = packet.ReadByte();
                        someKey[i] = packet.ReadByte();
                    }
                    packet.AddValue("NewBuildKey", Encoding.UTF8.GetString(newBuildKey));
                    packet.AddValue("SomeKey", Encoding.UTF8.GetString(someKey));
                }

                for (var i = 0; i < realms; ++i)
                    ReadVirtualRealmInfoEra(packet, "VirtualRealms", i);

                for (var i = 0; i < templates; ++i)
                {
                    packet.ReadUInt32("TemplateSetId", i);
                    var templateClasses = packet.ReadUInt32();
                    for (var j = 0; j < templateClasses; ++j)
                    {
                        packet.ReadByteE<Class>("Class", i, j);
                        packet.ReadByte("FactionGroup", i, j);
                    }

                    var s1 = packet.ReadByte();
                    var s2 = packet.ReadByte();
                    var s3 = packet.ReadByte();
                    var nameLen = s1 >> 1;
                    var descLen = (s3 >> 7) | ((s2 & 0x01) << 1) | ((s2 & 0xFE) << 1) | ((s1 & 0x01) << 9);
                    packet.ReadWoWString("Name", nameLen, i);
                    packet.ReadWoWString("Description", descLen, i);
                }
            }

            if (queued)
            {
                packet.ReadUInt32("WaitCount");
                packet.ReadUInt32("WaitTime");
                packet.ReadByte("AllowedFactionGroupForCharacterCreate");
                var queueFlags = packet.ReadByte("QueueFlags");
                packet.AddValue("HasFCM", (queueFlags & 0x80) != 0);
                packet.AddValue("CanCreateOnlyIfExisting", (queueFlags & 0x40) != 0);
            }
        }

        private static void ReadCliSavedThrottleObjectStateEra(Packet packet, params object[] idx)
        {
            packet.ReadUInt32("MaxTries", idx);
            packet.ReadUInt32("PerMilliseconds", idx);
            packet.ReadUInt32("TryCount", idx);
            packet.ReadUInt32("LastResetTimeBeforeNow", idx);
        }

        // 0x460064
        // 1.15.9: field set matches retail ~12.1, but flags pack MSB-first into
        // byte-aligned groups and the bit stream is continuous across the
        // RealmHiddenAlert length. Flag names are positional guesses.
        [Parser(Opcode.SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleFeatureSystemStatusGlueScreenEra(Packet packet)
        {
            packet.ReadBit("BpayStoreAvailable");
            packet.ReadBit("CharUndeleteEnabled");
            packet.ReadBit("CommerceServerEnabled");
            packet.ReadBit("PaidCharacterTransfersBetweenBnetAccountsEnabled");
            packet.ReadBit("VeteranTokenRedeemWillKick");
            packet.ReadBit("WorldTokenRedeemWillKick");
            packet.ReadBit("ExpansionPreorderInStore");
            packet.ReadBit("KioskModeEnabled");

            packet.ReadBit("CompetitiveModeEnabled");
            packet.ReadBit("BoostEnabled");
            packet.ReadBit("TrialBoostEnabled");
            packet.ReadBit("RedeemForBalanceAvailable");
            packet.ReadBit("LiveRegionCharacterListEnabled");
            packet.ReadBit("LiveRegionCharacterCopyEnabled");
            packet.ReadBit("LiveRegionAccountCopyEnabled");
            packet.ReadBit("LiveRegionKeyBindingsCopyEnabled");

            packet.ReadBit("BrowserCrashReporterEnabled");
            packet.ReadBit("IsEmployeeAccount");
            packet.ReadBit("Unk_98");
            var europaTicket = packet.ReadBit("IsEuropaTicketSystemStatusEnabled");
            packet.ReadBit("NameReservationOnly");
            var launchEta = packet.ReadBit("HasLaunchETA");
            packet.ReadBit("TimerunningEnabled");
            packet.ReadBit("ScriptsDisallowedForBeta");

            packet.ReadBit("PlayerIdentityOptionsEnabled");
            packet.ReadBit("AccountExportEnabled");
            packet.ReadBit("AccountLockedPostExport");
            packet.ReadBit("BNSendWhisperUseV2Services");
            packet.ReadBit("BNSendGameDataUseV2Services");
            packet.ReadBit("CharacterSelectListModeRealmless");
            packet.ReadBit("WowTokenLimitedMode");

            var realmHiddenAlertLen = packet.ReadBits(11);

            packet.ReadBit("NavBarEnabled");
            packet.ReadBit("GlobalUserGeneratedContentMuteEnabled");
            packet.ReadBit("AccountUserGeneratedContentIsRisky");
            packet.ReadBit("Unk_366");
            packet.ReadBit("Unk_367");
            packet.ReadBit("Unk_368");

            if (europaTicket)
            {
                packet.ReadBit("TicketsEnabled", "EuropaTicketSystemStatus");
                packet.ReadBit("BugsEnabled", "EuropaTicketSystemStatus");
                packet.ReadBit("ComplaintsEnabled", "EuropaTicketSystemStatus");
                packet.ReadBit("SuggestionsEnabled", "EuropaTicketSystemStatus");

                ReadCliSavedThrottleObjectStateEra(packet, "EuropaTicketSystemStatus", "ThrottleState");
                ReadCliSavedThrottleObjectStateEra(packet, "EuropaTicketSystemStatus", "ThrottleState2");
            }

            packet.ReadUInt32("CommercePricePollTimeSeconds");
            packet.ReadUInt32("KioskSessionDurationMinutes");
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

            if (launchEta)
                packet.ReadInt32("LaunchETA");

            packet.ReadDynamicString("RealmHiddenAlert", realmHiddenAlertLen);

            for (var i = 0; i < liveRegionCharacterCopySourceRegionsCount; i++)
                packet.ReadUInt32("LiveRegionCharacterCopySourceRegion", i);

            for (var i = 0; i < disabledGameModesCount; ++i)
            {
                packet.ReadByte("GameMode", "DisabledGameModes", i);
                packet.ReadInt32("ContentSetID", "DisabledGameModes", i);
                packet.ReadInt32("GameModeRecordID", "DisabledGameModes", i);
            }

            for (var i = 0; i < gameRuleValuesCount; ++i)
            {
                packet.ReadInt32("Rule", "GameRuleValues", i);
                packet.ReadInt32("Value", "GameRuleValues", i);
                packet.ReadInt32("ValueF", "GameRuleValues", i);
            }

            for (var i = 0; i < availableGameModesCount; ++i)
                packet.ReadInt32("AvailableGameModeID", i);

            for (var i = 0; i < debugTimeEventCount; ++i)
            {
                packet.ReadUInt32("TimeEvent", "DebugTimeEvent", i);
                var textLen = packet.ReadByte() >> 1;
                packet.ReadWoWString("Text", textLen, "DebugTimeEvent", i);
            }
        }

        // 1.15.9: retail ~12.x layout, but flag bits pack MSB-first into
        // byte-aligned flag bytes, and element loops run in this order:
        // UnlockedConditionalAppearances, RaceLimitDisables, Characters,
        // RegionwideCharacters, RaceUnlockData, WarbandGroups.
        private static void ReadVisualItemInfoEra(Packet packet, params object[] idx)
        {
            packet.ReadInt32("ItemID", idx);
            packet.ReadInt32("TransmogrifiedItemID", idx);
            packet.ReadByte("Subclass", idx);
            packet.ReadByteE<InventoryType>("InvType", idx);
            packet.ReadUInt32("DisplayID", idx);
            packet.ReadUInt32("DisplayEnchantID", idx);
            packet.ReadInt32("SecondaryItemModifiedAppearanceID", idx);
            packet.ReadByte("SheatheCategory", idx);
        }

        private static void ReadBasicCharacterListEntryEra(Packet packet, params object[] idx)
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
                ReadVisualItemInfoEra(packet, idx, "VisualItems", j);

            packet.ReadInt32("SaveVersion", idx);
            packet.ReadTime64("CreateTime", idx);
            packet.ReadTime64("LastPlayedTime", idx);
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

            for (var j = 0u; j < customizationCount; ++j)
            {
                packet.ReadUInt32("ChrCustomizationOptionID", idx, "Customizations", j);
                packet.ReadUInt32("ChrCustomizationChoiceID", idx, "Customizations", j);
            }

            packet.ResetBitReader();

            var nameLength = packet.ReadBits(6);
            var firstLogin = packet.ReadBit("FirstLogin", idx);
            packet.ReadBit("RealmInfoFound", idx);
            packet.ReadBit("IsRealmOffline", idx);

            var name = packet.ReadWoWString("Character Name", nameLength, idx);

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

        private static void ReadRestrictionAndMailDataEra(Packet packet, params object[] idx)
        {
            var header = packet.ReadByte("RestrictionHeader", idx);
            packet.AddValue("BoostInProgress", (header & 0x80) != 0, idx);
            packet.AddValue("EraChoiceState", (header >> 5) & 3, idx);
            packet.AddValue("RpeResetAvailable", (header & 0x10) != 0, idx);

            packet.ReadUInt32("RestrictionFlags", idx);
            var mailSenderCount = packet.ReadUInt32();
            var mailSenderTypes = packet.ReadUInt32();
            packet.ReadInt32("NoRpeReason", idx);

            for (var j = 0; j < mailSenderTypes; ++j)
                packet.ReadUInt32("MailSenderType", idx, j);

            packet.ResetBitReader();

            var mailSenderLengths = new uint[mailSenderCount];
            for (var j = 0; j < mailSenderLengths.Length; ++j)
                mailSenderLengths[j] = packet.ReadBits(6);

            for (var j = 0; j < mailSenderLengths.Length; ++j)
                packet.ReadDynamicString("MailSender", mailSenderLengths[j], idx);
        }

        private static void ReadCharacterListEntryEra(Packet packet, params object[] idx)
        {
            ReadBasicCharacterListEntryEra(packet, idx, "Basic");
            ReadRestrictionAndMailDataEra(packet, idx, "RestrictionsAndMails");
        }

        private static void ReadRegionwideCharacterListEntryEra(Packet packet, params object[] idx)
        {
            ReadBasicCharacterListEntryEra(packet, idx, "Basic");

            packet.ReadUInt64("Money", idx);
            packet.ReadSingle("AvgEquippedItemLevel", idx);
            packet.ReadSingle("CurrentSeasonMythicPlusOverallScore", idx);
            packet.ReadInt32("CurrentSeasonBestPvpRating", idx);
            packet.ReadByte("PvpRatingBracket", idx);
            packet.ReadInt16("PvpRatingAssociatedSpecID", idx);
        }

        private static void ReadWarbandGroupEra(Packet packet, params object[] idx)
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

        // 0x460018
        [Parser(Opcode.SMSG_ENUM_CHARACTERS_RESULT, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleEnumCharactersResultEra(Packet packet)
        {
            packet.ReadBit("Success");
            packet.ReadBit("Realmless");
            packet.ReadBit("IsDeletedCharacters");
            packet.ReadBit("IsNewPlayerRestrictionSkipped");
            packet.ReadBit("IsNewPlayerRestricted");
            packet.ReadBit("IsNewPlayer");
            packet.ReadBit("IsTrialAccountRestricted");
            packet.ReadBit("IsAccountLapsedPlayer");

            var hasDisabledClassesMask = packet.ReadBit("HasDisabledClassesMask");
            packet.ReadBit("ForceCharacterListSort");

            var charsCount = packet.ReadUInt32("CharactersCount");
            var regionwideCharsCount = packet.ReadUInt32("RegionwideCharactersCount");
            packet.ReadInt32("MaxCharacterLevel");
            var raceUnlockCount = packet.ReadUInt32("RaceUnlockCount");
            var unlockedConditionalAppearanceCount = packet.ReadUInt32("UnlockedConditionalAppearanceCount");
            var raceLimitDisablesCount = packet.ReadUInt32("RaceLimitDisablesCount");
            var warbandGroupsCount = packet.ReadUInt32("WarbandGroupsCount");

            if (hasDisabledClassesMask)
                packet.ReadUInt32("DisabledClassesMask");

            for (var i = 0u; i < unlockedConditionalAppearanceCount; ++i)
            {
                packet.ReadInt32("AchievementId", "UnlockedConditionalAppearances", i);
                packet.ReadInt32("Unused", "UnlockedConditionalAppearances", i);
            }

            for (var i = 0u; i < raceLimitDisablesCount; ++i)
            {
                packet.ReadByteE<Race>("RaceID", "RaceLimitDisableInfo", i);
                packet.ReadByte("BlockReason", "RaceLimitDisableInfo", i);
            }

            for (var i = 0u; i < charsCount; ++i)
                ReadCharacterListEntryEra(packet, i, "Characters");

            for (var i = 0u; i < regionwideCharsCount; ++i)
                ReadRegionwideCharacterListEntryEra(packet, i, "RegionwideCharacters");

            for (var i = 0u; i < raceUnlockCount; ++i)
            {
                packet.ReadByteE<Race>("RaceID", "RaceUnlockData", i);
                var classCount = packet.ReadUInt32();
                var raceFlags = packet.ReadByte("RaceUnlockFlags", "RaceUnlockData", i);
                packet.AddValue("HasExpansion", (raceFlags & 0x80) != 0, "RaceUnlockData", i);
                packet.AddValue("HasUnlockedAchievement", (raceFlags & 0x40) != 0, "RaceUnlockData", i);
                packet.AddValue("HasHeritageArmorUnlockAchievement", (raceFlags & 0x20) != 0, "RaceUnlockData", i);
                packet.AddValue("HasEntitlement", (raceFlags & 0x10) != 0, "RaceUnlockData", i);
                packet.AddValue("HideRaceOnClient", (raceFlags & 0x08) != 0, "RaceUnlockData", i);

                for (var j = 0u; j < classCount; ++j)
                {
                    packet.ReadByteE<Class>("ClassID", "RaceUnlockData", i, "ClassUnlocks", j);
                    packet.ReadInt32("AchievementID", "RaceUnlockData", i, "ClassUnlocks", j);
                    var unlockFlags = packet.ReadByte("ClassUnlockFlags", "RaceUnlockData", i, "ClassUnlocks", j);
                    packet.AddValue("HasUnlockedAchievement", (unlockFlags & 0x80) != 0, "RaceUnlockData", i, "ClassUnlocks", j);
                }
            }

            for (var i = 0u; i < warbandGroupsCount; ++i)
                ReadWarbandGroupEra(packet, i, "WarbandGroups");
        }

        private static void ReadBleepTokenEra(Packet packet, params object[] idx)
        {
            packet.ResetBitReader();

            var tokenLength = packet.ReadBits(5);
            var proxyIdLength = packet.ReadBits(24);
            var addressLength = packet.ReadBits(6);
            packet.ReadInt64("TokenLifespanNanoSeconds", idx);
            packet.ReadWoWString("Token", tokenLength, idx);
            packet.ReadDynamicString("ProxyId", proxyIdLength, idx);
            packet.ReadWoWString("Address", addressLength, idx);
        }

        private static void ReadConnectPayloadEra(Packet packet, params object[] idx)
        {
            var type = packet.ReadByteE<AddressType>("Type", idx);
            switch (type)
            {
                case AddressType.IPv4:
                    packet.ReadIPAddress("Address", idx);
                    break;
                case AddressType.IPv6:
                    packet.ReadIPv6Address("Address", idx);
                    break;
                case AddressType.NamedSocket:
                    packet.ReadWoWString("Address", 128, idx);
                    break;
            }

            packet.ReadUInt16("Port", idx);
            ReadBleepTokenEra(packet, idx, "Token");
        }

        // 0x4D0008
        [Parser(Opcode.SMSG_CONNECT_TO, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleConnectToEra(Packet packet)
        {
            var payloadCount = packet.ReadUInt32();
            packet.ReadUInt32E<ConnectToSerial>("Serial");
            packet.ReadByte("Con");
            packet.ReadUInt64("Key");
            packet.ReadUInt32("NativeRealmAddress");
            packet.ReadUInt32("Key3");

            for (var i = 0u; i < payloadCount; ++i)
                ReadConnectPayloadEra(packet, "Payload", i);
        }

        // 0x450003 — same layout as retail 11.2.7 HandleAuthContinuedSession1127
        [Parser(Opcode.CMSG_AUTH_CONTINUED_SESSION, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleAuthContinuedSessionEra(Packet packet)
        {
            packet.ReadInt64("DosResponse");
            packet.ReadBytes("LocalChallenge", 32);
            packet.ReadBytes("Digest", 24);
            packet.ReadInt64("Key");
            packet.ReadUInt32("NativeRealmAddress");
            packet.ReadUInt32("Key3");
        }

        // 0x460280 — {i32, i32, flag bit (u8>>7)}; retail 12.1 pair = 0x450280
        [Parser(Opcode.SMSG_AUCTIONABLE_TOKEN_AUCTION_SOLD, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleAuctionableTokenAuctionSoldEra(Packet packet)
        {
            packet.ReadInt32("UnkInt1");
            packet.ReadInt32("UnkInt2");
            packet.ReadBit("UnkBit");
        }

        // 0x460358 — {i32, flag bit (u8>>7)}; retail 12.1 pair ≈ 0x45035A
        [Parser(Opcode.SMSG_ACCOUNT_CONVERSION_STATE_UPDATE, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleAccountConversionStateUpdateEra(Packet packet)
        {
            packet.ReadInt32("ConversionState");
            packet.ReadBit("UnkBit");
        }

        // 1.15.9 (0x460368): {u8 flag (bit7), u24be>>7 nameLen, u24be>>7 valueLen,
        // name, value} — each length is a 3-byte big-endian field encoding len<<7.
        private static int ReadMirrorVarStringLength(Packet packet)
        {
            var b0 = packet.ReadByte();
            var b1 = packet.ReadByte();
            var b2 = packet.ReadByte();
            return (b0 << 9) | (b1 << 1) | (b2 >> 7);
        }

        private static void ReadMirrorVarSingleEra(Packet packet, params object[] indexes)
        {
            packet.ReadByte("Flags", indexes);
            var nameLength = ReadMirrorVarStringLength(packet);
            var valueLength = ReadMirrorVarStringLength(packet);

            var name = Encoding.UTF8.GetString(packet.ReadBytes(nameLength)).TrimEnd('\0');
            var value = Encoding.UTF8.GetString(packet.ReadBytes(valueLength)).TrimEnd('\0');
            packet.AddValue(name, value, indexes);
        }

        // 0x460368
        [Parser(Opcode.SMSG_MIRROR_VARS, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleMirrorVarsEra(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (var i = 0u; i < count; ++i)
                ReadMirrorVarSingleEra(packet, i);
        }

        // 0x4601B9 — {PackedGuid128 Guid, u64 ServerTime, u64[20] Times};
        // 1.15.9 loops over all 20 AccountDataType entries (upstream used 17).
        [Parser(Opcode.SMSG_ACCOUNT_DATA_TIMES)]
        public static void HandleAccountDataTimesEra(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadTime64("ServerTime");

            var count = ClientVersion.AddedInVersion(ClientVersionBuild.V1_15_9_69722) ? 20 : 17;
            for (var i = 0; i < count; ++i)
                packet.ReadTime64($"[{(AccountDataType)i}] Time", i);
        }

        // 0x4600F6 — retail 12.1 pair 0x4500F6; {u32 count, u64[]}. Observed
        // empty (count=0); element width follows the modern TC definition.
        [Parser(Opcode.SMSG_SERVER_FIRST_ACHIEVEMENTS)]
        public static void HandleServerFirstAchievementsEra(Packet packet)
        {
            var count = packet.ReadUInt32("AchievementCount");
            for (var i = 0u; i < count; ++i)
                packet.ReadUInt64("AchievementID", i);
        }

        // 0x4602E3 — {u8 flags (bit7 = subscribe), u32 count, u64[count]};
        // retail 12.1 pair 0x4502D9 (+10 region drift, anchored by
        // BATTLE_NET_CONNECTION_STATUS 0x4602BB<->0x4502B1 and
        // WARDEN3_ENABLED 0x4602D5<->0x4502CB). Verified on 13-byte capture:
        // 0x80 flag, count=1, one u64 bnet account id.
        [Parser(Opcode.SMSG_BATCH_PRESENCE_SUBSCRIPTION, ClientVersionBuild.V1_15_9_69722)]
        public static void HandleBatchPresenceSubscriptionEra(Packet packet)
        {
            var flags = packet.ReadByte("Flags");
            packet.AddValue("Subscribe", (flags & 0x80) != 0);

            var count = packet.ReadUInt32("Count");
            for (var i = 0u; i < count; ++i)
                packet.ReadUInt64("BNetAccountID", i);
        }
    }
}
