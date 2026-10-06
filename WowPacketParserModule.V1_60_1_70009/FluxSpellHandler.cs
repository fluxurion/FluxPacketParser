using System.Collections.Generic;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.PacketStructures;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V1_60_1_70009.Parsers
{
    // Classic Era 1.60.1.70009 spell/aura layouts.
    // Transcribed from TrinityCoreLuaSol SpellPackets.cpp writers/readers:
    // SpellCastData puts Target BEFORE MissileTrajectory and carries an
    // AmmoDisplayID field + HitStatus array; AuraUpdate sends UnitGUID right
    // after the 9-bit count and AuraDataInfo carries CastItem + DstLocation.
    public static class FluxSpellHandler
    {
        // SpellCastVisual — { i32 SpellXSpellVisualID, i32 ScriptVisualID }
        private static void ReadSpellCastVisual160(Packet packet, params object[] idx)
        {
            packet.ReadInt32("SpellXSpellVisualID", idx);
            packet.ReadInt32("ScriptVisualID", idx);
        }

        // TargetLocation — { ObjectGuid Transport, Vector3 Location }
        private static Vector3 ReadTargetLocation160(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("Transport", idx);
            return packet.ReadVector3("Location", idx);
        }

        // SpellTargetData (server->client writer form):
        // { u32 Flags, guid Unit, guid Item, guid HousingGUID,
        //   bits: resident(1), srcLoc(1), dstLoc(1), orient(1), map(1), name(7),
        //   [TargetLoc], [TargetLoc], [f32], [i32], [string] }
        private static void ReadSpellTargetData160(Packet packet, PacketSpellData packetSpellData, params object[] idx)
        {
            packet.ReadUInt32E<TargetFlag>("Flags", idx);
            var targetUnit = packet.ReadPackedGuid128("Unit", idx);
            if (packetSpellData != null)
                packetSpellData.TargetUnit = targetUnit;
            packet.ReadPackedGuid128("Item", idx);
            packet.ReadPackedGuid128("HousingGUID", idx);

            packet.ResetBitReader();
            packet.ReadBit("HousingIsResident", idx);
            var hasSrcLoc = packet.ReadBit("HasSrcLocation", idx);
            var hasDstLoc = packet.ReadBit("HasDstLocation", idx);
            var hasOrient = packet.ReadBit("HasOrientation", idx);
            var hasMapID = packet.ReadBit("HasMapID", idx);
            var nameLength = (int)packet.ReadBits(7);

            if (hasSrcLoc)
                ReadTargetLocation160(packet, idx, "SrcLocation");

            if (hasDstLoc)
            {
                var dstLocation = ReadTargetLocation160(packet, idx, "DstLocation");
                if (packetSpellData != null)
                    packetSpellData.DstLocation = dstLocation;
            }

            if (hasOrient)
                packet.ReadSingle("Orientation", idx);

            if (hasMapID)
                packet.ReadInt32<MapId>("MapID", idx);

            packet.ReadWoWString("Name", nameLength, idx);
        }

        // MissileTrajectoryResult — { u32 TravelTime, f32 Pitch }
        private static void ReadMissileTrajectoryResult160(Packet packet, params object[] idx)
        {
            packet.ReadUInt32("TravelTime", idx);
            packet.ReadSingle("Pitch", idx);
        }

        // MissileTrajectoryRequest — { f32 Pitch, f32 Speed }
        private static void ReadMissileTrajectoryRequest160(Packet packet, params object[] idx)
        {
            packet.ReadSingle("Pitch", idx);
            packet.ReadSingle("Speed", idx);
        }

        // CreatureImmunities — { i32 School, i32 Value }
        private static void ReadCreatureImmunities160(Packet packet, params object[] idx)
        {
            packet.ReadInt32("School", idx);
            packet.ReadInt32("Value", idx);
        }

        // SpellHealPrediction — { i32 Points, u32 Type, guid BeaconGUID }
        private static void ReadSpellHealPrediction160(Packet packet, params object[] idx)
        {
            packet.ReadInt32("Points", idx);
            packet.ReadUInt32("Type", idx);
            packet.ReadPackedGuid128("BeaconGUID", idx);
        }

        // SpellPowerData — { i8 Type, i32 Cost }
        private static void ReadSpellPowerData160(Packet packet, params object[] idx)
        {
            packet.ReadSByte("Type", idx);
            packet.ReadInt32("Cost", idx);
        }

        // SpellHitStatus — { u8 Reason }
        // SpellMissStatus — { u8 Reason, [u8 ReflectStatus if Reason == 11 (SPELL_MISS_REFLECT)] }
        private static void ReadSpellMissStatus160(Packet packet, params object[] idx)
        {
            var reason = packet.ReadByte("Reason", idx);
            if (reason == 11)
                packet.ReadByte("ReflectStatus", idx);
        }

        // RuneData — { u8 Start, u8 Count, u32 cooldowns, u8[] }
        private static void ReadRuneData160(Packet packet, params object[] idx)
        {
            packet.ReadByte("Start", idx);
            packet.ReadByte("Count", idx);
            var cooldownCount = packet.ReadUInt32("CooldownsCount", idx);
            for (var i = 0; i < cooldownCount; ++i)
                packet.ReadByte("Cooldowns", idx, i);
        }

        // SpellCastLogData — { i64 Health, i32 AP, i32 SP, i32 Armor, i32 Vers,
        //   i32 Avoidance, bit hideFromLog, 9-bit powerData count, {i8,i32,i32}[] }
        public static void ReadSpellCastLogData160(Packet packet, params object[] idx)
        {
            packet.ReadInt64("Health", idx);
            packet.ReadInt32("AttackPower", idx);
            packet.ReadInt32("SpellPower", idx);
            packet.ReadInt32("Armor", idx);
            packet.ReadInt32("Versatility", idx);
            packet.ReadInt32("Avoidance", idx);

            packet.ResetBitReader();
            packet.ReadBit("HideFromCombatLog", idx);
            var powerCount = packet.ReadBits("PowerDataCount", 9, idx);

            for (var i = 0; i < powerCount; ++i)
            {
                packet.ReadSByte("PowerType", idx, i);
                packet.ReadInt32("Amount", idx, i);
                packet.ReadInt32("Cost", idx, i);
            }
        }

        // SpellCastData (TC 1.60 wire order):
        // caster, casterUnit, castID, originalCastID, spellID, visual,
        // castFlags, castFlagsEx, castFlagsEx2, castTime,
        // Target, MissileTrajectory, AmmoDisplayID, DestLocSpellCastIndex,
        // Immunities, Predict,
        // counts(16/16/16/16/9) + optRunes + targetPoints(16),
        // hitTargets, missTargets, hitStatus, missStatus, power, runes, targetPoints
        public static PacketSpellData ReadSpellCastData160(Packet packet, params object[] idx)
        {
            var packetSpellData = new PacketSpellData();
            packet.ReadPackedGuid128("CasterGUID", idx);
            packetSpellData.Caster = packet.ReadPackedGuid128("CasterUnit", idx);

            packetSpellData.CastGuid = packet.ReadPackedGuid128("CastID", idx);
            packet.ReadPackedGuid128("OriginalCastID", idx);

            var spellID = packetSpellData.Spell = packet.ReadUInt32<SpellId>("SpellID", idx);
            ReadSpellCastVisual160(packet, idx, "Visual");

            packetSpellData.Flags = packet.ReadUInt32("CastFlags", idx);
            packetSpellData.Flags2 = packet.ReadUInt32("CastFlagsEx", idx);
            packet.ReadUInt32("CastFlagsEx2", idx);
            packetSpellData.CastTime = packet.ReadUInt32("CastTime", idx);

            ReadSpellTargetData160(packet, packetSpellData, idx, "Target");

            ReadMissileTrajectoryResult160(packet, idx, "MissileTrajectory");

            packetSpellData.AmmoDisplayId = packet.ReadInt32("AmmoDisplayID", idx);
            packet.ReadByte("DestLocSpellCastIndex", idx);

            ReadCreatureImmunities160(packet, idx, "Immunities");
            ReadSpellHealPrediction160(packet, idx, "Predict");

            packet.ResetBitReader();
            var hitTargetsCount = packet.ReadBits("HitTargetsCount", 16, idx);
            var missTargetsCount = packet.ReadBits("MissTargetsCount", 16, idx);
            var hitStatusCount = packet.ReadBits("HitStatusCount", 16, idx);
            var missStatusCount = packet.ReadBits("MissStatusCount", 16, idx);
            var remainingPowerCount = packet.ReadBits("RemainingPowerCount", 9, idx);
            var hasRuneData = packet.ReadBit("HasRuneData", idx);
            var targetPointsCount = packet.ReadBits("TargetPointsCount", 16, idx);

            for (var i = 0; i < hitTargetsCount; ++i)
                packetSpellData.HitTargets.Add(packet.ReadPackedGuid128("HitTarget", idx, i));

            for (var i = 0; i < missTargetsCount; ++i)
                packetSpellData.MissedTargets.Add(packet.ReadPackedGuid128("MissTarget", idx, i));

            for (var i = 0; i < hitStatusCount; ++i)
                packet.ReadByte("HitStatus", idx, i);

            for (var i = 0; i < missStatusCount; ++i)
                ReadSpellMissStatus160(packet, idx, "MissStatus", i);

            for (var i = 0; i < remainingPowerCount; ++i)
                ReadSpellPowerData160(packet, idx, "RemainingPower", i);

            if (hasRuneData)
                ReadRuneData160(packet, idx, "RemainingRunes");

            for (var i = 0; i < targetPointsCount; ++i)
                packetSpellData.TargetPoints.Add(ReadTargetLocation160(packet, idx, "TargetPoints", i));

            return packetSpellData;
        }

        [Parser(Opcode.SMSG_SPELL_START)]
        public static void HandleSpellStart(Packet packet)
        {
            PacketSpellStart packetSpellStart = new();
            packetSpellStart.Data = ReadSpellCastData160(packet, "Cast");
            packet.Holder.SpellStart = packetSpellStart;
        }

        [Parser(Opcode.SMSG_SPELL_GO)]
        public static void HandleSpellGo(Packet packet)
        {
            PacketSpellGo packetSpellGo = new();
            packetSpellGo.Data = ReadSpellCastData160(packet, "Cast");
            packet.Holder.SpellGo = packetSpellGo;

            packet.ResetBitReader();
            var hasLogData = packet.ReadBit();
            if (hasLogData)
                ReadSpellCastLogData160(packet, "LogData");
        }

        // ContentTuningParams (1.60):
        // { f32, f32, i16, i32, i32, i32, u8, u8, i8, u32, i32, i32, i32, f32, 4-bit Type }
        public static void ReadContentTuningParams160(Packet packet, params object[] idx)
        {
            packet.ResetBitReader();
            packet.ReadSingle("PlayerItemLevel", idx);
            packet.ReadSingle("TargetItemLevel", idx);
            packet.ReadInt16("PlayerLevelDelta", idx);
            packet.ReadInt32("ScalingHealthItemLevelCurveID", idx);
            packet.ReadInt32("Unused1117", idx);
            packet.ReadInt32("ScalingHealthPrimaryStatCurveID", idx);
            packet.ReadByte("TargetLevel", idx);
            packet.ReadByte("Expansion", idx);
            packet.ReadSByte("TargetScalingLevelDelta", idx);
            packet.ReadUInt32("Flags", idx);
            packet.ReadInt32("PlayerContentTuningID", idx);
            packet.ReadInt32("TargetContentTuningID", idx);
            packet.ReadInt32("TargetHealingContentTuningID", idx);
            packet.ReadSingle("PlayerPrimaryStatToExpectedRatio", idx);
            packet.ResetBitReader();
            packet.ReadBits("Type", 4, idx);
        }

        // AuraDataInfo — castID, spellID, visual, u16 flags, u32 activeFlags,
        // u16 castLevel, u8 applications, i32 contentTuningID, vec3 dstLocation,
        // opt bits (castUnit, castItem, duration, remaining, timeMod,
        // points(6), estPoints(6), contentTuning) then payloads.
        private static void ReadAuraDataInfo160(Packet packet, PacketAuraUpdateEntry auraEntry, Aura aura, params object[] idx)
        {
            packet.ReadPackedGuid128("CastID", idx);
            aura.SpellId = auraEntry.Spell = (uint)packet.ReadInt32<SpellId>("SpellID", idx);
            ReadSpellCastVisual160(packet, idx, "Visual");
            var flags = (AuraFlagMoP)packet.ReadUInt16("Flags", idx);
            aura.AuraFlags = flags;
            auraEntry.Flags = flags.ToUniversal();
            packet.ReadUInt32("ActiveFlags", idx);
            aura.Level = packet.ReadUInt16("CastLevel", idx);
            aura.Charges = packet.ReadByte("Applications", idx);
            packet.ReadInt32("ContentTuningID", idx);
            packet.ReadVector3("DstLocation", idx);

            packet.ResetBitReader();
            var hasCastUnit = packet.ReadBit("HasCastUnit", idx);
            var hasCastItem = packet.ReadBit("HasCastItem", idx);
            var hasDuration = packet.ReadBit("HasDuration", idx);
            var hasRemaining = packet.ReadBit("HasRemaining", idx);
            var hasTimeMod = packet.ReadBit("HasTimeMod", idx);
            var pointsCount = packet.ReadBits("PointsCount", 6, idx);
            var estimatedPointsCount = packet.ReadBits("EstimatedPoints", 6, idx);
            var hasContentTuning = packet.ReadBit("HasContentTuning", idx);

            if (hasCastUnit)
                auraEntry.CasterUnit = packet.ReadPackedGuid128("CastUnit", idx);

            if (hasCastItem)
                packet.ReadPackedGuid128("CastItem", idx);

            aura.Duration = hasDuration ? packet.ReadInt32("Duration", idx) : 0;
            if (hasDuration)
                auraEntry.Duration = aura.Duration;

            aura.MaxDuration = hasRemaining ? packet.ReadInt32("Remaining", idx) : 0;
            if (hasRemaining)
                auraEntry.Remaining = aura.MaxDuration;

            if (hasTimeMod)
                packet.ReadSingle("TimeMod", idx);

            for (var j = 0; j < pointsCount; ++j)
                packet.ReadSingle("Points", idx, j);

            for (var j = 0; j < estimatedPointsCount; ++j)
                packet.ReadSingle("EstimatedPoints", idx, j);

            if (hasContentTuning)
                ReadContentTuningParams160(packet, idx, "ContentTuning");
        }

        // AuraUpdate — bits{UpdateAll(1), AurasCount(9)}, UnitGUID,
        // entries { u16 Slot, opt AuraData }.
        [HasSniffData]
        [Parser(Opcode.SMSG_AURA_UPDATE)]
        public static void HandleAuraUpdate(Packet packet)
        {
            PacketAuraUpdate packetAuraUpdate = packet.Holder.AuraUpdate = new();
            packet.ResetBitReader();
            packet.ReadBit("UpdateAll");
            var count = packet.ReadBits("AurasCount", 9);
            var guid = packet.ReadPackedGuid128("UnitGUID");
            packetAuraUpdate.Unit = guid;

            var auras = new List<Aura>();
            for (var i = 0; i < count; ++i)
            {
                var auraEntry = new PacketAuraUpdateEntry();
                packetAuraUpdate.Updates.Add(auraEntry);
                var aura = new Aura();

                auraEntry.Slot = packet.ReadUInt16("Slot", i);

                packet.ResetBitReader();
                var hasAura = packet.ReadBit("HasAura", i);
                auraEntry.Remove = !hasAura;
                if (hasAura)
                {
                    ReadAuraDataInfo160(packet, auraEntry, aura, i);
                    auras.Add(aura);
                    packet.AddSniffData(StoreNameType.Spell, (int)aura.SpellId, "AURA_UPDATE");
                }
            }

            if (Storage.Objects.ContainsKey(guid))
            {
                var unit = Storage.Objects[guid].Item1 as Unit;
                if (unit != null)
                {
                    // If this is the first packet that sends auras
                    // (hopefully at spawn time) add it to the "Auras" field,
                    // if not create another row of auras in AddedAuras
                    // (similar to ChangedUpdateFields)
                    if (unit.Auras == null)
                        unit.Auras = auras;
                    else
                        unit.AddedAuras.Add(auras);
                }
            }
        }

        // SpellCastRequest (CMSG_CAST_SPELL / CMSG_PET_CAST_SPELL / CMSG_USE_ITEM):
        // { guid CastID, u8 SendCastFlags, i32 Misc[3], i32 SpellID, Visual,
        //   Target, MissileTrajectory{f32,f32}, guid CraftingNPC,
        //   u32 extraCurrencyCosts, u32 craftingReagents, u32 removedReagents,
        //   u8 CraftingCastFlags, cost[], reagent[], removed[],
        //   bits: optReceiveTime, optMoveUpdate, weight(2), optCraftingOrderID,
        //   [u32], [MovementInfo], weights{i32,u32(2bit type first)}[], [u64] }
        private static uint ReadSpellCastRequest160(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("CastID", idx);
            packet.ReadByte("SendCastFlags", idx);
            for (var i = 0; i < 3; ++i)
                packet.ReadInt32("Misc", idx, i);

            var spellId = packet.ReadUInt32<SpellId>("SpellID", idx);
            ReadSpellCastVisual160(packet, idx, "Visual");

            ReadSpellTargetData160(packet, null, idx, "Target");
            ReadMissileTrajectoryRequest160(packet, idx, "MissileTrajectory");

            packet.ReadPackedGuid128("CraftingNPC", idx);

            var extraCurrencyCostsCount = packet.ReadUInt32("ExtraCurrencyCostsCount", idx);
            var craftingReagentsCount = packet.ReadUInt32("CraftingReagentsCount", idx);
            var removedReagentsCount = packet.ReadUInt32("RemovedReagentsCount", idx);
            packet.ReadByte("CraftingCastFlags", idx);

            for (var i = 0; i < extraCurrencyCostsCount; ++i)
            {
                packet.ReadInt32("CurrencyID", idx, i);
                packet.ReadInt32("Count", idx, i);
            }

            for (var i = 0; i < craftingReagentsCount; ++i)
                ReadSpellCraftingReagent160(packet, idx, "CraftingReagent", i);

            for (var i = 0; i < removedReagentsCount; ++i)
                ReadSpellCraftingReagent160(packet, idx, "RemovedReagent", i);

            packet.ResetBitReader();
            var hasReceiveTime = packet.ReadBit("HasReceiveTime", idx);
            var hasMoveUpdate = packet.ReadBit("HasMoveUpdate", idx);
            var weightCount = packet.ReadBits("WeightCount", 2, idx);
            var hasCraftingOrderID = packet.ReadBit("HasCraftingOrderID", idx);

            if (hasReceiveTime)
                packet.ReadUInt32("ReceiveTime", idx);

            if (hasMoveUpdate)
                FluxMovementHandler.ReadMovementStats160(packet, idx, "MoveUpdate");

            for (var i = 0; i < weightCount; ++i)
            {
                packet.ResetBitReader();
                packet.ReadBits("Type", 2, idx, i);
                packet.ReadInt32("ID", idx, i);
                packet.ReadUInt32("Quantity", idx, i);
            }

            if (hasCraftingOrderID)
                packet.ReadUInt64("CraftingOrderID", idx);

            return spellId;
        }

        // SpellCraftingReagent — { i32 Slot, i32 Quantity,
        //   CraftingReagentBase{opt i32 ItemID, opt i32 CurrencyID}, opt u8 Source }
        private static void ReadSpellCraftingReagent160(Packet packet, params object[] idx)
        {
            packet.ReadInt32("Slot", idx);
            packet.ReadInt32("Quantity", idx);

            packet.ResetBitReader();
            var hasItemID = packet.ReadBit("HasItemID", idx);
            var hasCurrencyID = packet.ReadBit("HasCurrencyID", idx);
            if (hasItemID)
                packet.ReadInt32("ItemID", idx);
            if (hasCurrencyID)
                packet.ReadInt32("CurrencyID", idx);

            packet.ResetBitReader();
            var hasSource = packet.ReadBit("HasSource", idx);
            if (hasSource)
                packet.ReadByte("Source", idx);
        }

        [Parser(Opcode.CMSG_CAST_SPELL)]
        public static void HandleCastSpell(Packet packet)
        {
            ReadSpellCastRequest160(packet, "Cast");
        }

        [Parser(Opcode.CMSG_PET_CAST_SPELL)]
        public static void HandlePetCastSpell(Packet packet)
        {
            packet.ReadPackedGuid128("PetGUID");
            ReadSpellCastRequest160(packet, "Cast");
        }

        [Parser(Opcode.CMSG_USE_ITEM)]
        public static void HandleUseItem(Packet packet)
        {
            packet.ReadByte("PackSlot");
            packet.ReadByte("Slot");
            packet.ReadPackedGuid128("CastItem");
            ReadSpellCastRequest160(packet, "Cast");
        }

        [Parser(Opcode.CMSG_CANCEL_AURA)]
        public static void HandleCancelAura(Packet packet)
        {
            packet.ReadInt32<SpellId>("SpellID");
            packet.ReadPackedGuid128("CasterGUID");
        }

        [Parser(Opcode.CMSG_CANCEL_CHANNELLING)]
        public static void HandleCancelChannelling(Packet packet)
        {
            packet.ReadInt32<SpellId>("ChannelSpell");
            packet.ReadInt32("Reason");
        }

        // SpellPrepare — { guid ClientCastID, guid ServerCastID }
        [Parser(Opcode.SMSG_SPELL_PREPARE)]
        public static void HandleSpellPrepare(Packet packet)
        {
            packet.ReadPackedGuid128("ClientCastID");
            packet.ReadPackedGuid128("ServerCastID");
        }

        // CastFailed — { guid, i32 SpellID, Visual, i32 Reason, i32, i32, u8 FailedBy }
        [Parser(Opcode.SMSG_CAST_FAILED)]
        public static void HandleCastFailed(Packet packet)
        {
            packet.ReadPackedGuid128("CastID");
            packet.ReadInt32<SpellId>("SpellID");
            ReadSpellCastVisual160(packet, "Visual");
            packet.ReadInt32("Reason");
            packet.ReadInt32("FailedArg1");
            packet.ReadInt32("FailedArg2");
            packet.ReadByte("FailedBy");
        }

        // PetCastFailed — { guid, i32 SpellID, i32 Reason, i32, i32 }
        [Parser(Opcode.SMSG_PET_CAST_FAILED)]
        public static void HandlePetCastFailed(Packet packet)
        {
            packet.ReadPackedGuid128("CastID");
            packet.ReadInt32<SpellId>("SpellID");
            packet.ReadInt32("Reason");
            packet.ReadInt32("FailedArg1");
            packet.ReadInt32("FailedArg2");
        }

        // SpellFailure — { guid CasterUnit, guid CastID, i32 SpellID, Visual, u16 Reason, u8 FailedBy }
        [Parser(Opcode.SMSG_SPELL_FAILURE)]
        public static void HandleSpellFailure(Packet packet)
        {
            packet.ReadPackedGuid128("CasterUnit");
            packet.ReadPackedGuid128("CastID");
            packet.ReadInt32<SpellId>("SpellID");
            ReadSpellCastVisual160(packet, "Visual");
            packet.ReadUInt16("Reason");
            packet.ReadByte("FailedBy");
        }

        // SpellFailedOther — { guid CasterUnit, guid CastID, u32 SpellID, Visual, u8 Reason, u8 FailedBy }
        [Parser(Opcode.SMSG_SPELL_FAILED_OTHER)]
        public static void HandleSpellFailedOther(Packet packet)
        {
            packet.ReadPackedGuid128("CasterUnit");
            packet.ReadPackedGuid128("CastID");
            packet.ReadUInt32<SpellId>("SpellID");
            ReadSpellCastVisual160(packet, "Visual");
            packet.ReadByte("Reason");
            packet.ReadByte("FailedBy");
        }
    }
}
