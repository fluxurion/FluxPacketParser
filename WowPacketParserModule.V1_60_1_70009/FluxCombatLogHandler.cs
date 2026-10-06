using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V1_60_1_70009.Parsers
{
    // Classic Era 1.60.1.70009 combat-log / combat / threat layouts.
    // Transcribed from TrinityCoreLuaSol CombatPackets.cpp, CombatLogPackets.cpp
    // and CombatLogPacketsCommon.cpp. Every CombatLogServerPacket ends with a
    // WriteLogDataBit() (false in the wire packet) + optional SpellCastLogData.
    public static class FluxCombatLogHandler
    {
        private const uint HITINFO_UNK1 = 0x00000001;
        private const uint HITINFO_FULL_ABSORB = 0x00000020;
        private const uint HITINFO_PARTIAL_ABSORB = 0x00000040;
        private const uint HITINFO_FULL_RESIST = 0x00000080;
        private const uint HITINFO_PARTIAL_RESIST = 0x00000100;
        private const uint HITINFO_BLOCK = 0x00002000;
        private const uint HITINFO_UNK12 = 0x00001000;
        private const uint HITINFO_RAGE_GAIN = 0x00800000;

        // CombatWorldTextViewerInfo — { guid, opt u8 ColorType, opt u8 ScaleType }
        private static void ReadWorldTextViewerInfo160(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("ViewerGUID", idx);
            packet.ResetBitReader();
            var hasColorType = packet.ReadBit("HasColorType", idx);
            var hasScaleType = packet.ReadBit("HasScaleType", idx);
            if (hasColorType)
                packet.ReadByte("ColorType", idx);
            if (hasScaleType)
                packet.ReadByte("ScaleType", idx);
        }

        // SpellSupportInfo — { guid Supporter, i32 SupportSpellID, i32 AmountRaw, f32 AmountPortion }
        private static void ReadSpellSupportInfo160(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("Supporter", idx);
            packet.ReadInt32<SpellId>("SupportSpellID", idx);
            packet.ReadInt32("AmountRaw", idx);
            packet.ReadSingle("AmountPortion", idx);
        }

        private static void ReadLogDataBitAndData160(Packet packet)
        {
            packet.ResetBitReader();
            var hasLogData = packet.ReadBit("HasLogData");
            if (hasLogData)
                FluxSpellHandler.ReadSpellCastLogData160(packet, "LogData");
        }

        // SMSG_ATTACKER_STATE_UPDATE — the attack-round payload is encapsulated:
        // { logDataBit, u32 size, u32 Flags, attacker, victim, i32 dmg, i32 origDmg,
        //   i32 overkill, u8 hasSubDmg, [sub], u8 victimState, u32 attackerState,
        //   u32 meleeSpellID, conditional fields, ContentTuningParams }
        [Parser(Opcode.SMSG_ATTACKER_STATE_UPDATE)]
        public static void HandleAttackerStateUpdate(Packet packet)
        {
            ReadLogDataBitAndData160(packet);

            packet.ReadUInt32("AttackRoundInfoSize");

            var flags = packet.ReadUInt32("HitInfo");
            packet.ReadPackedGuid128("AttackerGUID");
            packet.ReadPackedGuid128("VictimGUID");
            packet.ReadInt32("Damage");
            packet.ReadInt32("OriginalDamage");
            packet.ReadInt32("OverDamage");

            var hasSubDmg = packet.ReadByte("HasSubDmg") != 0;
            if (hasSubDmg)
            {
                packet.ReadInt32("SchoolMask");
                packet.ReadSingle("FDamage");
                packet.ReadInt32("SubDamage");
                if ((flags & (HITINFO_FULL_ABSORB | HITINFO_PARTIAL_ABSORB)) != 0)
                    packet.ReadInt32("Absorbed");
                if ((flags & (HITINFO_FULL_RESIST | HITINFO_PARTIAL_RESIST)) != 0)
                    packet.ReadInt32("Resisted");
            }

            packet.ReadByte("VictimState");
            packet.ReadUInt32("AttackerState");
            packet.ReadUInt32("MeleeSpellID");

            if ((flags & HITINFO_BLOCK) != 0)
                packet.ReadInt32("BlockAmount");

            if ((flags & HITINFO_RAGE_GAIN) != 0)
                packet.ReadInt32("RageGained");

            if ((flags & HITINFO_UNK1) != 0)
            {
                packet.ReadUInt32("ArmorReduction");
                packet.ReadSingle("CritRollNeeded");
                packet.ReadSingle("CombatRoll");
                packet.ReadSingle("MissChance");
                packet.ReadSingle("DodgeChance");
                packet.ReadSingle("ParryChance");
                packet.ReadSingle("BlockChance");
                packet.ReadSingle("GlanceChance");
                packet.ReadSingle("CrushChance");
                packet.ReadSingle("MinDamage");
                packet.ReadSingle("MaxDamage");
                packet.ReadUInt32("SinceLastSwing");
            }

            if ((flags & (HITINFO_BLOCK | HITINFO_UNK12)) != 0)
                packet.ReadSingle("BlockRoll");

            FluxSpellHandler.ReadContentTuningParams160(packet, "ContentTuning");
        }

        // SpellNonMeleeDamageLog — victim, caster, castID, spellID, visual,
        // damage, origDmg, overkill, u8 school, absorbed, resisted, shieldBlock,
        // reflectSpellID, flags, viewers[], supporters[], bits, opt tuning.
        [Parser(Opcode.SMSG_SPELL_NON_MELEE_DAMAGE_LOG)]
        public static void HandleSpellNonMeleeDamageLog(Packet packet)
        {
            packet.ReadPackedGuid128("Victim");
            packet.ReadPackedGuid128("CasterGUID");
            packet.ReadPackedGuid128("CastID");
            packet.ReadInt32<SpellId>("SpellID");
            packet.ReadInt32("SpellXSpellVisualID");
            packet.ReadInt32("ScriptVisualID");
            packet.ReadInt32("Damage");
            packet.ReadInt32("OriginalDamage");
            packet.ReadInt32("Overkill");
            packet.ReadByte("SchoolMask");
            packet.ReadInt32("Absorbed");
            packet.ReadInt32("Resisted");
            packet.ReadInt32("ShieldBlock");
            packet.ReadInt32("ReflectingSpellID");
            packet.ReadInt32("Flags");

            var viewersCount = packet.ReadUInt32("WorldTextViewersCount");
            var supportersCount = packet.ReadUInt32("SupportersCount");

            for (var i = 0; i < viewersCount; ++i)
                ReadWorldTextViewerInfo160(packet, "WorldTextViewers", i);
            for (var i = 0; i < supportersCount; ++i)
                ReadSpellSupportInfo160(packet, "Supporters", i);

            packet.ResetBitReader();
            packet.ReadBit("Periodic");
            packet.ReadBit("HasDebugInfo");
            var hasLogData = packet.ReadBit("HasLogData");
            var hasContentTuning = packet.ReadBit("HasContentTuning");

            if (hasLogData)
                FluxSpellHandler.ReadSpellCastLogData160(packet, "LogData");
            if (hasContentTuning)
                FluxSpellHandler.ReadContentTuningParams160(packet, "ContentTuning");
        }

        // SpellDamageShield — { attacker, defender, spell, total, orig, overkill, school, logAbsorbed, logbit }
        [Parser(Opcode.SMSG_SPELL_DAMAGE_SHIELD)]
        public static void HandleSpellDamageShield(Packet packet)
        {
            packet.ReadPackedGuid128("Attacker");
            packet.ReadPackedGuid128("Defender");
            packet.ReadInt32<SpellId>("SpellID");
            packet.ReadInt32("TotalDamage");
            packet.ReadInt32("OriginalDamage");
            packet.ReadInt32("OverKill");
            packet.ReadInt32("SchoolMask");
            packet.ReadInt32("LogAbsorbed");
            ReadLogDataBitAndData160(packet);
        }

        // SpellHealLog — { target, caster, spell, health, origHeal, overHeal,
        //   absorbed, supporters[], bits(crit, optCritRoll, optCritRollNeeded,
        //   logData, optTuning), [f32], [f32], log, [tuning] }
        [Parser(Opcode.SMSG_SPELL_HEAL_LOG)]
        public static void HandleSpellHealLog(Packet packet)
        {
            packet.ReadPackedGuid128("TargetGUID");
            packet.ReadPackedGuid128("CasterGUID");
            packet.ReadInt32<SpellId>("SpellID");
            packet.ReadInt32("Health");
            packet.ReadInt32("OriginalHeal");
            packet.ReadInt32("OverHeal");
            packet.ReadInt32("Absorbed");

            var supportersCount = packet.ReadUInt32("SupportersCount");
            for (var i = 0; i < supportersCount; ++i)
                ReadSpellSupportInfo160(packet, "Supporters", i);

            packet.ResetBitReader();
            packet.ReadBit("Crit");
            var hasCritRollMade = packet.ReadBit("HasCritRollMade");
            var hasCritRollNeeded = packet.ReadBit("HasCritRollNeeded");
            var hasLogData = packet.ReadBit("HasLogData");
            var hasContentTuning = packet.ReadBit("HasContentTuning");

            if (hasCritRollMade)
                packet.ReadSingle("CritRollMade");
            if (hasCritRollNeeded)
                packet.ReadSingle("CritRollNeeded");

            if (hasLogData)
                FluxSpellHandler.ReadSpellCastLogData160(packet, "LogData");
            if (hasContentTuning)
                FluxSpellHandler.ReadContentTuningParams160(packet, "ContentTuning");
        }

        // SpellEnergizeLog — { target, caster, spell, i8 type, amount, overEnergize, logbit }
        [Parser(Opcode.SMSG_SPELL_ENERGIZE_LOG)]
        public static void HandleSpellEnergizeLog(Packet packet)
        {
            packet.ReadPackedGuid128("TargetGUID");
            packet.ReadPackedGuid128("CasterGUID");
            packet.ReadInt32<SpellId>("SpellID");
            packet.ReadSByte("Type");
            packet.ReadInt32("Amount");
            packet.ReadInt32("OverEnergize");
            ReadLogDataBitAndData160(packet);
        }

        // SpellPeriodicAuraLog — { target, caster, spell, u32 effects,
        //   effect{i32 effect, i32 amount, i32 origDmg, i32 over, i32 school,
        //   i32 absorbed, i32 resisted, u32 supporters[], bit crit, opt debug,
        //   opt tuning}, logbit }
        [Parser(Opcode.SMSG_SPELL_PERIODIC_AURA_LOG)]
        public static void HandleSpellPeriodicAuraLog(Packet packet)
        {
            packet.ReadPackedGuid128("TargetGUID");
            packet.ReadPackedGuid128("CasterGUID");
            packet.ReadInt32<SpellId>("SpellID");

            var effectCount = packet.ReadUInt32("EffectsCount");
            for (var i = 0; i < effectCount; ++i)
            {
                packet.ReadInt32("Effect", i);
                packet.ReadInt32("Amount", i);
                packet.ReadInt32("OriginalDamage", i);
                packet.ReadInt32("OverHealOrKill", i);
                packet.ReadInt32("SchoolMaskOrPower", i);
                packet.ReadInt32("AbsorbedOrAmplitude", i);
                packet.ReadInt32("Resisted", i);

                var supportersCount = packet.ReadUInt32("SupportersCount", i);
                for (var j = 0; j < supportersCount; ++j)
                    ReadSpellSupportInfo160(packet, i, "Supporters", j);

                packet.ResetBitReader();
                packet.ReadBit("Crit", i);
                var hasDebugInfo = packet.ReadBit("HasDebugInfo", i);
                var hasContentTuning = packet.ReadBit("HasContentTuning", i);

                if (hasDebugInfo)
                {
                    packet.ReadSingle("CritRollMade", i);
                    packet.ReadSingle("CritRollNeeded", i);
                }
                if (hasContentTuning)
                    FluxSpellHandler.ReadContentTuningParams160(packet, i, "ContentTuning");
            }

            ReadLogDataBitAndData160(packet);
        }

        // SpellMissLog — { i32 spell, caster, u32 count,
        //   entry{guid victim, u8 reason, opt{f32,f32} debug}, bit hideFromLog }
        [Parser(Opcode.SMSG_SPELL_MISS_LOG)]
        public static void HandleSpellMissLog(Packet packet)
        {
            packet.ReadInt32<SpellId>("SpellID");
            packet.ReadPackedGuid128("Caster");

            var count = packet.ReadUInt32("EntriesCount");
            for (var i = 0; i < count; ++i)
            {
                packet.ReadPackedGuid128("Victim", i);
                packet.ReadByte("MissReason", i);
                packet.ResetBitReader();
                var hasDebug = packet.ReadBit("HasDebug", i);
                if (hasDebug)
                {
                    packet.ReadSingle("HitRoll", i);
                    packet.ReadSingle("HitRollNeeded", i);
                }
            }

            packet.ResetBitReader();
            packet.ReadBit("HideFromCombatLog");
        }

        // SpellExecuteLog — { caster, spell, u32 effects,
        //   effect{i32 effect, 6×u32 counts, conditional arrays}, logbit }
        [Parser(Opcode.SMSG_SPELL_EXECUTE_LOG)]
        public static void HandleSpellExecuteLog(Packet packet)
        {
            packet.ReadPackedGuid128("Caster");
            packet.ReadInt32<SpellId>("SpellID");

            var effectCount = packet.ReadUInt32("EffectsCount");
            for (var i = 0; i < effectCount; ++i)
            {
                packet.ReadInt32("Effect", i);

                var powerDrainCount = packet.ReadUInt32("PowerDrainTargetsCount", i);
                var extraAttacksCount = packet.ReadUInt32("ExtraAttacksTargetsCount", i);
                var durabilityCount = packet.ReadUInt32("DurabilityDamageTargetsCount", i);
                var genericVictimCount = packet.ReadUInt32("GenericVictimTargetsCount", i);
                var tradeSkillCount = packet.ReadUInt32("TradeSkillTargetsCount", i);
                var feedPetCount = packet.ReadUInt32("FeedPetTargetsCount", i);

                for (var j = 0; j < powerDrainCount; ++j)
                {
                    packet.ReadPackedGuid128("Victim", i, "PowerDrain", j);
                    packet.ReadUInt32("Points", i, "PowerDrain", j);
                    packet.ReadSByte("PowerType", i, "PowerDrain", j);
                    packet.ReadSingle("Amplitude", i, "PowerDrain", j);
                }

                for (var j = 0; j < extraAttacksCount; ++j)
                {
                    packet.ReadPackedGuid128("Victim", i, "ExtraAttacks", j);
                    packet.ReadUInt32("NumAttacks", i, "ExtraAttacks", j);
                }

                for (var j = 0; j < durabilityCount; ++j)
                {
                    packet.ReadPackedGuid128("Victim", i, "Durability", j);
                    packet.ReadInt32("ItemID", i, "Durability", j);
                    packet.ReadInt32("Amount", i, "Durability", j);
                }

                for (var j = 0; j < genericVictimCount; ++j)
                    packet.ReadPackedGuid128("Victim", i, "GenericVictim", j);

                for (var j = 0; j < tradeSkillCount; ++j)
                    packet.ReadInt32("ItemID", i, "TradeSkill", j);

                for (var j = 0; j < feedPetCount; ++j)
                    packet.ReadInt32("ItemID", i, "FeedPet", j);
            }

            ReadLogDataBitAndData160(packet);
        }

        // SpellDispellLog / SpellStealLog — { bits(steal, break), target, caster,
        //   i32 dispelledBySpellID, u32 count, {i32 spell, bit harmful, opt i32, opt i32} }
        [Parser(Opcode.SMSG_SPELL_DISPELL_LOG)]
        public static void HandleSpellDispellLog(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("IsSteal");
            packet.ReadBit("IsBreak");
            packet.ReadPackedGuid128("TargetGUID");
            packet.ReadPackedGuid128("CasterGUID");
            packet.ReadInt32<SpellId>("DispelledBySpellID");

            var count = packet.ReadUInt32("DispellDataCount");
            for (var i = 0; i < count; ++i)
            {
                packet.ReadInt32<SpellId>("SpellID", i);
                packet.ResetBitReader();
                packet.ReadBit("Harmful", i);
                var hasRolled = packet.ReadBit("HasRolled", i);
                var hasNeeded = packet.ReadBit("HasNeeded", i);
                if (hasRolled)
                    packet.ReadInt32("Rolled", i);
                if (hasNeeded)
                    packet.ReadInt32("Needed", i);
            }
        }

        // SpellAbsorbLog — { attacker, victim, absorbedSpellID, absorbSpellID,
        //   caster, absorbed, origDmg, supporters[], bit crit, logbit }
        [Parser(Opcode.SMSG_SPELL_ABSORB_LOG)]
        public static void HandleSpellAbsorbLog(Packet packet)
        {
            packet.ReadPackedGuid128("Attacker");
            packet.ReadPackedGuid128("Victim");
            packet.ReadInt32<SpellId>("AbsorbedSpellID");
            packet.ReadInt32<SpellId>("AbsorbSpellID");
            packet.ReadPackedGuid128("Caster");
            packet.ReadInt32("Absorbed");
            packet.ReadInt32("OriginalDamage");

            var supportersCount = packet.ReadUInt32("SupportersCount");
            for (var i = 0; i < supportersCount; ++i)
                ReadSpellSupportInfo160(packet, "Supporters", i);

            packet.ResetBitReader();
            packet.ReadBit("Crit");
            var hasLogData = packet.ReadBit("HasLogData");
            if (hasLogData)
                FluxSpellHandler.ReadSpellCastLogData160(packet, "LogData");
        }

        // SpellHealAbsorbLog — { target, absorbCaster, healer, absorbSpellID,
        //   absorbedSpellID, absorbed, origHeal, logbit, opt tuning }
        [Parser(Opcode.SMSG_SPELL_HEAL_ABSORB_LOG)]
        public static void HandleSpellHealAbsorbLog(Packet packet)
        {
            packet.ReadPackedGuid128("Target");
            packet.ReadPackedGuid128("AbsorbCaster");
            packet.ReadPackedGuid128("Healer");
            packet.ReadInt32<SpellId>("AbsorbSpellID");
            packet.ReadInt32<SpellId>("AbsorbedSpellID");
            packet.ReadInt32("Absorbed");
            packet.ReadInt32("OriginalHeal");

            packet.ResetBitReader();
            var hasLogData = packet.ReadBit("HasLogData");
            var hasContentTuning = packet.ReadBit("HasContentTuning");

            if (hasLogData)
                FluxSpellHandler.ReadSpellCastLogData160(packet, "LogData");
            if (hasContentTuning)
                FluxSpellHandler.ReadContentTuningParams160(packet, "ContentTuning");
        }

        // SpellInterruptLog — { caster, victim, i32, i32, bit hide }
        [Parser(Opcode.SMSG_SPELL_INTERRUPT_LOG)]
        public static void HandleSpellInterruptLog(Packet packet)
        {
            packet.ReadPackedGuid128("Caster");
            packet.ReadPackedGuid128("Victim");
            packet.ReadInt32<SpellId>("InterruptedSpellID");
            packet.ReadInt32<SpellId>("SpellID");
            packet.ResetBitReader();
            packet.ReadBit("HideFromCombatLog");
        }

        // SpellInstakillLog — { target, caster, spell }
        [Parser(Opcode.SMSG_SPELL_INSTAKILL_LOG)]
        public static void HandleSpellInstakillLog(Packet packet)
        {
            packet.ReadPackedGuid128("Target");
            packet.ReadPackedGuid128("Caster");
            packet.ReadInt32<SpellId>("SpellID");
        }

        // EnvironmentalDamageLog — { victim, u8 type, i32 amount, i32 resisted, i32 absorbed, logbit }
        [Parser(Opcode.SMSG_ENVIRONMENTAL_DAMAGE_LOG)]
        public static void HandleEnvironmentalDamageLog(Packet packet)
        {
            packet.ReadPackedGuid128("Victim");
            packet.ReadByte("Type");
            packet.ReadInt32("Amount");
            packet.ReadInt32("Resisted");
            packet.ReadInt32("Absorbed");
            ReadLogDataBitAndData160(packet);
        }

        // SpellOrDamageImmune — { caster, victim, u32 spell, bit periodic }
        [Parser(Opcode.SMSG_SPELL_OR_DAMAGE_IMMUNE)]
        public static void HandleSpellOrDamageImmune(Packet packet)
        {
            packet.ReadPackedGuid128("CasterGUID");
            packet.ReadPackedGuid128("VictimGUID");
            packet.ReadUInt32<SpellId>("SpellID");
            packet.ResetBitReader();
            packet.ReadBit("IsPeriodic");
        }

        // ProcResist — { caster, target, i32 spell, opt f32 rolled, opt f32 needed }
        [Parser(Opcode.SMSG_PROC_RESIST)]
        public static void HandleProcResist(Packet packet)
        {
            packet.ReadPackedGuid128("Caster");
            packet.ReadPackedGuid128("Target");
            packet.ReadInt32<SpellId>("SpellID");
            packet.ResetBitReader();
            var hasRolled = packet.ReadBit("HasRolled");
            var hasNeeded = packet.ReadBit("HasNeeded");
            if (hasRolled)
                packet.ReadSingle("Rolled");
            if (hasNeeded)
                packet.ReadSingle("Needed");
        }

        // ThreatUpdate — { unit, u32 count, {guid, i64}[] }
        [Parser(Opcode.SMSG_THREAT_UPDATE)]
        public static void HandleThreatUpdate(Packet packet)
        {
            packet.ReadPackedGuid128("UnitGUID");
            var count = packet.ReadUInt32("ThreatListCount");
            for (var i = 0; i < count; ++i)
            {
                packet.ReadPackedGuid128("ThreatGUID", i);
                packet.ReadInt64("Threat", i);
            }
        }

        // HighestThreatUpdate — { unit, highest, u32 count, {guid, i64}[] }
        [Parser(Opcode.SMSG_HIGHEST_THREAT_UPDATE)]
        public static void HandleHighestThreatUpdate(Packet packet)
        {
            packet.ReadPackedGuid128("UnitGUID");
            packet.ReadPackedGuid128("HighestThreatGUID");
            var count = packet.ReadUInt32("ThreatListCount");
            for (var i = 0; i < count; ++i)
            {
                packet.ReadPackedGuid128("ThreatGUID", i);
                packet.ReadInt64("Threat", i);
            }
        }

        [Parser(Opcode.SMSG_THREAT_REMOVE)]
        public static void HandleThreatRemove(Packet packet)
        {
            packet.ReadPackedGuid128("UnitGUID");
            packet.ReadPackedGuid128("AboutGUID");
        }

        [Parser(Opcode.SMSG_THREAT_CLEAR)]
        public static void HandleThreatClear(Packet packet)
        {
            packet.ReadPackedGuid128("UnitGUID");
        }

        // ============================ misc SMSG ============================

        // Emote — { guid, u32 emoteID, u32 spellVisualKitIDs count, i32 seqVar, i32[] }
        [Parser(Opcode.SMSG_EMOTE)]
        public static void HandleEmote(Packet packet)
        {
            packet.ReadPackedGuid128("SourceGUID");
            packet.ReadUInt32("EmoteID");
            var count = packet.ReadUInt32("SpellVisualKitIDsCount");
            packet.ReadInt32("SequenceVariation");
            for (var i = 0; i < count; ++i)
                packet.ReadInt32("SpellVisualKitID", i);
        }

        // TextEmote — { source, sourceAccount, emote, soundIndex, target }
        [Parser(Opcode.SMSG_TEXT_EMOTE)]
        public static void HandleTextEmote(Packet packet)
        {
            packet.ReadPackedGuid128("SourceGUID");
            packet.ReadPackedGuid128("SourceAccountGUID");
            packet.ReadInt32("EmoteID");
            packet.ReadInt32("SoundIndex");
            packet.ReadPackedGuid128("TargetGUID");
        }

        // DBReply — { u32 tableHash, u32 recordID, u32 timestamp, 3-bit status, u32 size, data }
        [Parser(Opcode.SMSG_DB_REPLY)]
        public static void HandleDBReply(Packet packet)
        {
            packet.ReadUInt32E<DB2Hash>("TableHash");
            packet.ReadUInt32("RecordID");
            packet.ReadUInt32("Timestamp");
            packet.ResetBitReader();
            packet.ReadBits("Status", 3);
            var size = packet.ReadUInt32("DataSize");
            packet.ReadBytes("Data", (int)size);
        }

        // CriteriaUpdate — { u32 id, u64 qty, guid, u32 flags, u32 stateFlags,
        //   time, u32 elapsed, time, opt u64 dynamicID }
        [Parser(Opcode.SMSG_CRITERIA_UPDATE)]
        public static void HandleCriteriaUpdate(Packet packet)
        {
            packet.ReadUInt32("CriteriaID");
            packet.ReadUInt64("Quantity");
            packet.ReadPackedGuid128("PlayerGUID");
            packet.ReadUInt32("Flags");
            packet.ReadUInt32("StateFlags");
            packet.ReadTime("CurrentTime");
            packet.ReadUInt32("ElapsedTime");
            packet.ReadTime("CreationTime");
            packet.ResetBitReader();
            var hasDynamicID = packet.ReadBit("HasDynamicID");
            if (hasDynamicID)
                packet.ReadUInt64("DynamicID");
        }

        // SetAIAnimKit — { guid, u16 animKitID }
        [Parser(Opcode.SMSG_SET_AI_ANIM_KIT)]
        public static void HandleSetAIAnimKit(Packet packet)
        {
            packet.ReadPackedGuid128("Unit");
            packet.ReadUInt16("AnimKitID");
        }

        // ============================ misc CMSG ============================

        [Parser(Opcode.CMSG_ATTACK_SWING)]
        public static void HandleAttackSwing(Packet packet)
        {
            packet.ReadPackedGuid128("Victim");
        }

        [Parser(Opcode.CMSG_ATTACK_STOP)]
        [Parser(Opcode.CMSG_EMOTE)]
        public static void HandleEmptyPacket(Packet packet)
        {
        }

        [Parser(Opcode.CMSG_SET_SELECTION)]
        public static void HandleSetSelection(Packet packet)
        {
            packet.ReadPackedGuid128("Selection");
        }

        // SetSheathed — { u32 state, bit animate }
        [Parser(Opcode.CMSG_SET_SHEATHED)]
        public static void HandleSetSheathed(Packet packet)
        {
            packet.ReadUInt32("CurrentSheathState");
            packet.ResetBitReader();
            packet.ReadBit("Animate");
        }

        // CTextEmote — { target, i32 emote, i32 sound, u32 count, i32 seqVar, i32[] }
        [Parser(Opcode.CMSG_SEND_TEXT_EMOTE)]
        public static void HandleSendTextEmote(Packet packet)
        {
            packet.ReadPackedGuid128("Target");
            packet.ReadInt32("EmoteID");
            packet.ReadInt32("SoundIndex");
            var count = packet.ReadUInt32("SpellVisualKitIDsCount");
            packet.ReadInt32("SequenceVariation");
            for (var i = 0; i < count; ++i)
                packet.ReadInt32("SpellVisualKitID", i);
        }
    }
}
