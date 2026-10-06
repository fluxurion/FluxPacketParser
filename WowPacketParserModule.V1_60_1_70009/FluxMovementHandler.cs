using System.Collections.Generic;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store.Objects;
using WowPacketParser.Enums.Version;
using WowPacketParserModule.V5_5_0_61735.Parsers;
using WowPacketParserModule.V6_0_2_19033.Enums;
using CoreParsers = WowPacketParser.Parsing.Parsers;
using MovementFlag = WowPacketParser.Enums.v12.MovementFlag;
using SplineFacingType = WowPacketParserModule.V6_0_2_19033.Enums.SplineFacingType;
using SplineFlag = WowPacketParserModule.V6_0_2_19033.Enums.SplineFlag;

namespace WowPacketParserModule.V1_60_1_70009.Parsers
{
    // Classic Era 1.60.1.70009 movement packet layouts.
    // Transcribed from TrinityCoreLuaSol MovementPackets.cpp writers/readers and
    // wire-verified against the dump_1.60.1.70009 sniff (guid + MovementMonsterSpline
    // + position ordering, 64-bit movement flags, 9-bit optional block, optionals in
    // standingOn/transport/fall/inertia/advFlying/driveStatus order).
    public static class FluxMovementHandler
    {
        private sealed class MovementSplineData160
        {
            public readonly List<Vector3> Points = new();
            public readonly List<Vector3> PackedDeltas = new();
            public Vector3 Destination;
        }

        // MovementSpline — TrinityCoreLuaSol operator<<(MovementPackets.cpp):
        // flags, facingType, elapsed, moveTime, fadeObjectTime, mode, transportGuid,
        // vehicleSeat, conditional facing, point/packedDelta counts + optional bits.
        private static MovementSplineData160 ReadMovementSpline160(Packet packet, params object[] indexes)
        {
            var movementSplineData = new MovementSplineData160();
            PacketMonsterMove monsterMove = packet.Holder.MonsterMove;

            var splineFlag = packet.ReadUInt32E<SplineFlag>("Flags", indexes);
            monsterMove.Flags = splineFlag.ToUniversal();

            var type = packet.ReadByteE<SplineFacingType>("Face", indexes);

            monsterMove.ElapsedTime = packet.ReadInt32("Elapsed", indexes);
            monsterMove.MoveTime = packet.ReadUInt32("MoveTime", indexes);
            monsterMove.FadeObjectTime = packet.ReadUInt32("FadeObjectTime", indexes);

            packet.ReadByte("Mode", indexes);

            monsterMove.TransportGuid = packet.ReadPackedGuid128("TransportGUID", indexes);
            monsterMove.VehicleSeat = packet.ReadSByte("VehicleSeat", indexes);

            switch (type)
            {
                case SplineFacingType.Spot:
                    monsterMove.LookPosition = packet.ReadVector3("FaceSpot", indexes);
                    break;
                case SplineFacingType.Target:
                    SplineLookTarget lookTarget = monsterMove.LookTarget = new();
                    lookTarget.Orientation = packet.ReadSingle("FaceDirection", indexes);
                    lookTarget.Target = packet.ReadPackedGuid128("FacingGUID", indexes);
                    break;
                case SplineFacingType.Angle:
                    monsterMove.LookOrientation = packet.ReadSingle("FaceDirection", indexes);
                    break;
                default:
                    break;
            }

            packet.ResetBitReader();

            var pointsCount = packet.ReadBits("PointsCount", 16, indexes);
            packet.ReadBit("VehicleExitVoluntary", indexes);
            packet.ReadBit("Interpolate", indexes);
            var packedDeltasCount = packet.ReadBits("PackedDeltasCount", 16, indexes);
            var hasSplineFilter = packet.ReadBit("HasSplineFilter", indexes);
            var hasSpellEffectExtraData = packet.ReadBit("HasSpellEffectExtraData", indexes);
            var hasJumpExtraData = packet.ReadBit("HasJumpExtraData", indexes);
            var hasTurnData = packet.ReadBit("HasTurnData", indexes);
            var hasAnimTier = packet.ReadBit("HasAnimTierTransition", indexes);
            var hasSpellVisualData = packet.ReadBit("HasSpellVisualData", indexes);

            for (var i = 0; i < pointsCount; ++i)
            {
                var spot = packet.ReadVector3();
                movementSplineData.Points.Add(spot);

                // client always taking first point
                if (i == 0)
                    movementSplineData.Destination = spot;
            }

            for (var i = 0; i < packedDeltasCount; ++i)
                movementSplineData.PackedDeltas.Add(packet.ReadPackedVector3());

            if (hasSplineFilter)
                MovementHandler.ReadMonsterSplineFilter(packet, indexes, "MonsterSplineFilter");

            if (hasSpellEffectExtraData)
                monsterMove.SpellEffect = MovementHandler.ReadMonsterSplineSpellEffectExtraData(packet, indexes, "MonsterSplineSpellEffectExtra");

            if (hasJumpExtraData)
                monsterMove.Jump = MovementHandler.ReadMonsterSplineJumpExtraData(packet, indexes, "MonsterSplineJumpExtraData");

            if (hasTurnData)
                MovementHandler.ReadMonsterSplineTurnData(packet, indexes, "MonsterSplineTurnData");

            if (hasAnimTier)
            {
                packet.ReadInt32("TierTransitionID", indexes);
                monsterMove.AnimTier = packet.ReadByte("AnimTier", indexes);
                packet.ReadUInt32("StartTime", indexes);
                packet.ReadUInt32("EndTime", indexes);
            }

            if (hasSpellVisualData)
            {
                for (var i = 0; i < 16; ++i)
                {
                    packet.ReadInt32("SpellID", indexes, "SpellVisualData", i);
                    V9_0_1_36216.Parsers.SpellHandler.ReadSpellCastVisual(packet, indexes, "SpellVisualData", i, "Visual");
                    packet.ReadInt32("StartNodeIndex", indexes, "SpellVisualData", i);
                }
            }

            return movementSplineData;
        }

        // MovementMonsterSpline — id, then crz/stop bits, then MovementSpline.
        // (Bits are BEFORE the spline body on 1.60, matching MonsterMove::Write.)
        private static MovementSplineData160 ReadMovementMonsterSpline160(Packet packet, params object[] indexes)
        {
            PacketMonsterMove monsterMove = packet.Holder.MonsterMove;
            monsterMove.Id = packet.ReadUInt32("Id", indexes);

            packet.ResetBitReader();
            packet.ReadBit("CrzTeleport", indexes);
            packet.ReadBit("StopUseFaceDirection", indexes);
            packet.ReadBits("StopDistanceTolerance", 3, indexes);

            return ReadMovementSpline160(packet, indexes, "MovementSpline");
        }

        [Parser(Opcode.SMSG_ON_MONSTER_MOVE)]
        public static void HandleOnMonsterMove(Packet packet)
        {
            PacketMonsterMove monsterMove = packet.Holder.MonsterMove = new();
            monsterMove.Mover = packet.ReadPackedGuid128("MoverGUID");

            var splineData = ReadMovementMonsterSpline160(packet, "MovementMonsterSpline");
            Vector3 pos = monsterMove.Position = packet.ReadVector3("Position");

            var distance = 0.0;
            if (splineData.Points.Count > 0)
            {
                var prevpos = pos;
                for (var i = 0; i < splineData.Points.Count; ++i)
                {
                    var spot = splineData.Points[i];
                    packet.AddValue("Points", spot, i);
                    monsterMove.Points.Add(spot);
                    distance += Vector3.GetDistance(prevpos, spot);
                    prevpos = spot;
                }
            }

            if (splineData.PackedDeltas.Count > 0)
            {
                // Calculate mid pos
                var mid = (pos + splineData.Destination) * 0.5f;

                // ignore distance set by Points array if packed deltas are used
                distance = 0;

                var prevpos = pos;
                for (var i = 0; i < splineData.PackedDeltas.Count; ++i)
                {
                    var vec = mid - splineData.PackedDeltas[i];
                    packet.AddValue("WayPoints", vec, i);
                    monsterMove.PackedPoints.Add(vec);
                    distance += Vector3.GetDistance(prevpos, vec);
                    prevpos = vec;
                }
                distance += Vector3.GetDistance(prevpos, splineData.Destination);
            }

            if (splineData.Destination.X != 0 && splineData.Destination.Y != 0 && splineData.Destination.Z != 0)
                CoreParsers.MovementHandler.PrintComputedSplineMovementParams(packet, distance, monsterMove);
        }

        // ============================ MovementInfo ============================
        // operator>>(ByteBuffer, MovementInfo) — guid, u64 flags, u32 time, XYZO,
        // pitch, stepUpStartElevation, removeForcesCount, moveIndex, gravityModifier,
        // removeForces guids, 9-bit optional block, then optionals.

        private static MovementInfo ReadTransportInfo160(Packet packet, params object[] indexes)
        {
            var info = new MovementInfo
            {
                Transport = new MovementInfo.TransportInfo
                {
                    Guid = packet.ReadPackedGuid128("TransportGUID", indexes)
                }
            };
            info.Transport.Offset = packet.ReadVector4("TransportOffset", indexes);
            packet.ReadSByte("VehicleSeatIndex", indexes);
            packet.ReadInt32("MoveTime", indexes);

            packet.ResetBitReader();
            var hasPrevTime = packet.ReadBit("HasPrevMoveTime", indexes);
            var hasVehicleId = packet.ReadBit("HasVehicleRecID", indexes);

            if (hasPrevTime)
                packet.ReadInt32("PrevMoveTime", indexes);
            if (hasVehicleId)
                packet.ReadInt32("VehicleRecID", indexes);

            return info;
        }

        public static MovementInfo ReadMovementStats160(Packet packet, params object[] indexes)
        {
            var info = new MovementInfo
            {
                MoverGuid = packet.ReadPackedGuid128("MoverGUID", indexes),
                Flags64 = (ulong)packet.ReadUInt64E<MovementFlag>("MovementFlags", indexes)
            };

            packet.ReadUInt32("MoveTime", indexes);
            var position = packet.ReadVector4("Position", indexes);
            info.Position = new Vector3 { X = position.X, Y = position.Y, Z = position.Z };
            info.Orientation = position.O;

            packet.ReadSingle("Pitch", indexes);
            packet.ReadSingle("StepUpStartElevation", indexes);

            var removeForcesCount = packet.ReadUInt32("RemoveForcesCount", indexes);
            packet.ReadInt32("MoveIndex", indexes);
            packet.ReadSingle("GravityModifier", indexes);

            for (var i = 0; i < removeForcesCount; ++i)
                packet.ReadPackedGuid128("RemoveForcesIDs", indexes, i);

            packet.ResetBitReader();

            var hasStandingOnGameObjectGUID = packet.ReadBit("HasStandingOnGameObjectGUID", indexes);
            var hasTransport = packet.ReadBit("HasTransportData", indexes);
            var hasFall = packet.ReadBit("HasFallData", indexes);
            packet.ReadBit("HasSpline", indexes);
            packet.ReadBit("HeightChangeFailed", indexes);
            packet.ReadBit("RemoteTimeValid", indexes);
            var hasInertia = packet.ReadBit("HasInertia", indexes);
            var hasAdvFlying = packet.ReadBit("HasAdvFlying", indexes);
            var hasDriveStatus = packet.ReadBit("HasDriveStatus", indexes);

            if (hasStandingOnGameObjectGUID)
                packet.ReadPackedGuid128("StandingOnGameObjectGUID", indexes);

            if (hasTransport)
                ReadTransportInfo160(packet, indexes, "TransportData");

            if (hasFall)
            {
                packet.ReadUInt32("FallTime", indexes);
                packet.ReadSingle("JumpVelocity", indexes);

                packet.ResetBitReader();
                var hasFallDirection = packet.ReadBit("HasFallDirection", indexes);
                if (hasFallDirection)
                {
                    packet.ReadSingle("JumpSinAngle", indexes);
                    packet.ReadSingle("JumpCosAngle", indexes);
                    packet.ReadSingle("JumpXYSpeed", indexes);
                }
            }

            if (hasInertia)
            {
                packet.ReadUInt32("InertiaID", indexes);
                packet.ReadVector3("InertiaForce", indexes);
                packet.ReadUInt32("InertiaLifetimeMs", indexes);
            }

            if (hasAdvFlying)
            {
                packet.ReadSingle("ForwardVelocity", indexes);
                packet.ReadSingle("UpVelocity", indexes);
            }

            if (hasDriveStatus)
            {
                packet.ReadSingle("Speed", indexes);
                packet.ReadSingle("MovementAngle", indexes);
                packet.ResetBitReader();
                packet.ReadBit("Accelerating", indexes);
                packet.ReadBit("Drifting", indexes);
            }

            return info;
        }

        // MovementAck = { MovementInfo Status, int32 AckIndex }
        public static MovementInfo ReadMovementAck160(Packet packet, params object[] indexes)
        {
            var stats = ReadMovementStats160(packet, indexes);
            packet.ReadInt32("AckIndex", indexes);
            return stats;
        }

        // ============================ CMSG ============================

        [Parser(Opcode.CMSG_MOVE_START_FORWARD)]
        [Parser(Opcode.CMSG_MOVE_START_BACKWARD)]
        [Parser(Opcode.CMSG_MOVE_STOP)]
        [Parser(Opcode.CMSG_MOVE_START_STRAFE_LEFT)]
        [Parser(Opcode.CMSG_MOVE_START_STRAFE_RIGHT)]
        [Parser(Opcode.CMSG_MOVE_STOP_STRAFE)]
        [Parser(Opcode.CMSG_MOVE_JUMP)]
        [Parser(Opcode.CMSG_MOVE_DOUBLE_JUMP)]
        [Parser(Opcode.CMSG_MOVE_START_TURN_LEFT)]
        [Parser(Opcode.CMSG_MOVE_START_TURN_RIGHT)]
        [Parser(Opcode.CMSG_MOVE_STOP_TURN)]
        [Parser(Opcode.CMSG_MOVE_START_PITCH_UP)]
        [Parser(Opcode.CMSG_MOVE_START_PITCH_DOWN)]
        [Parser(Opcode.CMSG_MOVE_STOP_PITCH)]
        [Parser(Opcode.CMSG_MOVE_SET_RUN_MODE)]
        [Parser(Opcode.CMSG_MOVE_SET_WALK_MODE)]
        [Parser(Opcode.CMSG_MOVE_FALL_LAND)]
        [Parser(Opcode.CMSG_MOVE_START_SWIM)]
        [Parser(Opcode.CMSG_MOVE_STOP_SWIM)]
        [Parser(Opcode.CMSG_MOVE_SET_FACING)]
        [Parser(Opcode.CMSG_MOVE_SET_PITCH)]
        [Parser(Opcode.CMSG_MOVE_HEARTBEAT)]
        [Parser(Opcode.CMSG_MOVE_REMOVE_MOVEMENT_FORCES)]
        [Parser(Opcode.CMSG_MOVE_FALL_RESET)]
        [Parser(Opcode.CMSG_MOVE_UPDATE_FALL_SPEED)]
        [Parser(Opcode.CMSG_MOVE_SET_FLY)]
        [Parser(Opcode.CMSG_MOVE_START_ASCEND)]
        [Parser(Opcode.CMSG_MOVE_STOP_ASCEND)]
        [Parser(Opcode.CMSG_MOVE_CHANGE_TRANSPORT)]
        [Parser(Opcode.CMSG_MOVE_START_DESCEND)]
        [Parser(Opcode.CMSG_MOVE_DISMISS_VEHICLE)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLY)]
        [Parser(Opcode.CMSG_MOVE_START_DRIVE_FORWARD)]
        [Parser(Opcode.CMSG_MOVE_SET_FACING_HEARTBEAT)]
        public static void HandleClientPlayerMove(Packet packet)
        {
            var stats = ReadMovementStats160(packet, "MovementStats");
            packet.Holder.ClientMove = new() { Mover = stats.MoverGuid, Position = stats.PositionAsVector4 };
        }

        [Parser(Opcode.CMSG_MOVE_TELEPORT_ACK)]
        public static void HandleMoveTeleportAck(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadInt32("AckIndex");
            packet.ReadInt32("MoveTime");
        }

        // MovementAckMessage — { MovementAck Ack }
        [Parser(Opcode.CMSG_MOVE_COLLISION_DISABLE_ACK)]
        [Parser(Opcode.CMSG_MOVE_COLLISION_ENABLE_ACK)]
        [Parser(Opcode.CMSG_MOVE_DISABLE_JUMPING_ACK)]
        [Parser(Opcode.CMSG_MOVE_DISABLE_STRAFING_ACK)]
        [Parser(Opcode.CMSG_MOVE_ENABLE_DOUBLE_JUMP_ACK)]
        [Parser(Opcode.CMSG_MOVE_ENABLE_FULL_SPEED_PITCHING_ACK)]
        [Parser(Opcode.CMSG_MOVE_ENABLE_FULL_SPEED_TURNING_ACK)]
        [Parser(Opcode.CMSG_MOVE_ENABLE_SWIM_TO_FLY_TRANS_ACK)]
        [Parser(Opcode.CMSG_MOVE_FEATHER_FALL_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_ROOT_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_UNROOT_ACK)]
        [Parser(Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK)]
        [Parser(Opcode.CMSG_MOVE_GRAVITY_ENABLE_ACK)]
        [Parser(Opcode.CMSG_MOVE_HOVER_ACK)]
        [Parser(Opcode.CMSG_MOVE_INERTIA_DISABLE_ACK)]
        [Parser(Opcode.CMSG_MOVE_INERTIA_ENABLE_ACK)]
        [Parser(Opcode.CMSG_MOVE_REMOVE_INERTIA_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ALWAYS_ALLOW_PITCHING_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_CANNOT_SWIM_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_CAN_ADV_FLY_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_CAN_FLY_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_CAN_TURN_WHILE_FALLING_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_IGNORE_MOVEMENT_FORCES_ACK)]
        [Parser(Opcode.CMSG_MOVE_WATER_WALK_ACK)]
        public static void HandleMovementAck(Packet packet)
        {
            ReadMovementAck160(packet);
        }

        // MovementSpeedAck — { MovementAck Ack, float Speed }
        [Parser(Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_FORCE_GRAVITY_MODIFIER_CHANGE_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_MOD_MOVEMENT_FORCE_MAGNITUDE_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_AIR_FRICTION_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_MAX_VEL_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_LIFT_COEFFICIENT_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_DOUBLE_JUMP_VEL_MOD_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_GLIDE_START_MIN_HEIGHT_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_ADD_IMPULSE_MAX_SPEED_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_BANKING_RATE_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_PITCHING_RATE_DOWN_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_PITCHING_RATE_UP_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_TURN_VELOCITY_THRESHOLD_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_SURFACE_FRICTION_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_OVER_MAX_DECELERATION_ACK)]
        [Parser(Opcode.CMSG_MOVE_SET_ADV_FLYING_LAUNCH_SPEED_COEFFICIENT_ACK)]
        public static void HandleMovementSpeedAck(Packet packet)
        {
            ReadMovementAck160(packet);
            packet.ReadSingle("Speed");
        }

        [Parser(Opcode.CMSG_MOVE_KNOCK_BACK_ACK)]
        public static void HandleMoveKnockBackAck(Packet packet)
        {
            ReadMovementAck160(packet);

            packet.ResetBitReader();
            var hasSpeeds = packet.ReadBit("HasSpeeds");
            if (hasSpeeds)
            {
                packet.ReadSingle("HorzSpeed");
                packet.ReadSingle("VertSpeed");
            }
        }

        [Parser(Opcode.CMSG_MOVE_SET_COLLISION_HEIGHT_ACK)]
        public static void HandleMoveSetCollisionHeightAck(Packet packet)
        {
            ReadMovementAck160(packet);
            packet.ReadSingle("Height");
            packet.ReadUInt32("MountDisplayID");
            packet.ReadByte("Reason");
        }

        [Parser(Opcode.CMSG_MOVE_APPLY_MOVEMENT_FORCE_ACK)]
        public static void HandleMoveApplyMovementForceAck(Packet packet)
        {
            ReadMovementAck160(packet);
            MovementHandler1158.ReadMovementForce(packet, "MovementForce");
        }

        [Parser(Opcode.CMSG_MOVE_REMOVE_MOVEMENT_FORCE_ACK)]
        public static void HandleMoveRemoveMovementForceAck(Packet packet)
        {
            ReadMovementAck160(packet);
            packet.ReadPackedGuid128("MovementForceGUID");
        }

        [Parser(Opcode.CMSG_MOVE_SET_VEHICLE_REC_ID_ACK)]
        public static void HandleMoveSetVehicleRecIdAck(Packet packet)
        {
            ReadMovementAck160(packet);
            packet.ReadUInt32("VehicleRecID");
        }

        [Parser(Opcode.CMSG_MOVE_SPLINE_DONE)]
        public static void HandleMoveSplineDone(Packet packet)
        {
            ReadMovementStats160(packet, "MovementStats");
            packet.ReadUInt32("SplineID");
        }

        [Parser(Opcode.CMSG_MOVE_TIME_SKIPPED)]
        public static void HandleMoveTimeSkipped(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("TimeSkipped");
        }

        [Parser(Opcode.CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE)]
        public static void HandleMoveInitActiveMoverComplete(Packet packet)
        {
            packet.ReadUInt32("Ticks");
        }

        [Parser(Opcode.CMSG_SET_ACTIVE_MOVER)]
        public static void HandleSetActiveMover(Packet packet)
        {
            packet.ReadPackedGuid128("ActiveMover");
        }

        [Parser(Opcode.CMSG_SUMMON_RESPONSE)]
        public static void HandleSummonResponse(Packet packet)
        {
            packet.ReadPackedGuid128("SummonerGUID");
            packet.ResetBitReader();
            packet.ReadBit("Accept");
        }

        [Parser(Opcode.CMSG_SUSPEND_TOKEN_RESPONSE)]
        public static void HandleSuspendTokenResponse(Packet packet)
        {
            packet.ReadUInt32("SequenceIndex");
        }

        // ============================ SMSG ============================

        [Parser(Opcode.SMSG_MOVE_UPDATE)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_KNOCK_BACK)]
        public static void HandleMovementUpdate(Packet packet)
        {
            ReadMovementStats160(packet, "MovementStats");
        }

        // MoveUpdateSpeed — { *Status, float Speed }
        [Parser(Opcode.SMSG_MOVE_UPDATE_RUN_SPEED)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_RUN_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_SWIM_SPEED)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_SWIM_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_WALK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_FLIGHT_SPEED)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_FLIGHT_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_TURN_RATE)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_PITCH_RATE)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_SET_GRAVITY_MODIFIER)]
        [Parser(Opcode.SMSG_MOVE_UPDATE_MOD_MOVEMENT_FORCE_MAGNITUDE)]
        public static void HandleMovementUpdateSpeed(Packet packet)
        {
            ReadMovementStats160(packet, "MovementStats");
            packet.ReadSingle("Speed");
        }

        // MoveUpdateTeleport — { *Status, u32 forcesCount, forces[], 9 opt speed bits }
        [Parser(Opcode.SMSG_MOVE_UPDATE_TELEPORT)]
        public static void HandleMovementUpdateTeleport(Packet packet)
        {
            ReadMovementStats160(packet, "MovementStats");

            var forcesCount = packet.ReadUInt32("MovementForcesCount");
            for (var i = 0; i < forcesCount; ++i)
                MovementHandler1158.ReadMovementForce(packet, "MovementForces", i);

            packet.ResetBitReader();
            var hasWalkSpeed = packet.ReadBit("HasWalkSpeed");
            var hasRunSpeed = packet.ReadBit("HasRunSpeed");
            var hasRunBackSpeed = packet.ReadBit("HasRunBackSpeed");
            var hasSwimSpeed = packet.ReadBit("HasSwimSpeed");
            var hasSwimBackSpeed = packet.ReadBit("HasSwimBackSpeed");
            var hasFlightSpeed = packet.ReadBit("HasFlightSpeed");
            var hasFlightBackSpeed = packet.ReadBit("HasFlightBackSpeed");
            var hasTurnRate = packet.ReadBit("HasTurnRate");
            var hasPitchRate = packet.ReadBit("HasPitchRate");

            if (hasWalkSpeed)
                packet.ReadSingle("WalkSpeed");
            if (hasRunSpeed)
                packet.ReadSingle("RunSpeed");
            if (hasRunBackSpeed)
                packet.ReadSingle("RunBackSpeed");
            if (hasSwimSpeed)
                packet.ReadSingle("SwimSpeed");
            if (hasSwimBackSpeed)
                packet.ReadSingle("SwimBackSpeed");
            if (hasFlightSpeed)
                packet.ReadSingle("FlightSpeed");
            if (hasFlightBackSpeed)
                packet.ReadSingle("FlightBackSpeed");
            if (hasTurnRate)
                packet.ReadSingle("TurnRate");
            if (hasPitchRate)
                packet.ReadSingle("PitchRate");
        }

        [Parser(Opcode.SMSG_MOVE_UPDATE_COLLISION_HEIGHT)]
        public static void HandleMoveUpdateCollisionHeight(Packet packet)
        {
            ReadMovementStats160(packet, "MovementStats");
            packet.ReadSingle("Height");
            packet.ReadSingle("Scale");
        }

        [Parser(Opcode.SMSG_MOVE_UPDATE_APPLY_MOVEMENT_FORCE)]
        public static void HandleMoveUpdateApplyMovementForce(Packet packet)
        {
            ReadMovementStats160(packet, "MovementStats");
            MovementHandler1158.ReadMovementForce(packet, "MovementForce");
        }

        [Parser(Opcode.SMSG_MOVE_UPDATE_REMOVE_MOVEMENT_FORCE)]
        public static void HandleMoveUpdateRemoveMovementForce(Packet packet)
        {
            ReadMovementStats160(packet, "MovementStats");
            packet.ReadPackedGuid128("TriggerGUID");
        }

        // MoveSetSpeed / SetAdvFlyingSpeed — { guid, u32 seq, f32 speed }
        [Parser(Opcode.SMSG_MOVE_SET_RUN_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SET_RUN_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SET_SWIM_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SET_SWIM_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SET_WALK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SET_FLIGHT_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SET_FLIGHT_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SET_TURN_RATE)]
        [Parser(Opcode.SMSG_MOVE_SET_PITCH_RATE)]
        [Parser(Opcode.SMSG_MOVE_SET_GRAVITY_MODIFIER)]
        [Parser(Opcode.SMSG_MOVE_SET_MOD_MOVEMENT_FORCE_MAGNITUDE)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_AIR_FRICTION)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_MAX_VEL)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_LIFT_COEFFICIENT)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_DOUBLE_JUMP_VEL_MOD)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_GLIDE_START_MIN_HEIGHT)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_ADD_IMPULSE_MAX_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_BANKING_RATE)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_PITCHING_RATE_DOWN)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_PITCHING_RATE_UP)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_TURN_VELOCITY_THRESHOLD)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_SURFACE_FRICTION)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_OVER_MAX_DECELERATION)]
        [Parser(Opcode.SMSG_MOVE_SET_ADV_FLYING_LAUNCH_SPEED_COEFFICIENT)]
        public static void HandleMovementSetSpeed(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
            packet.ReadSingle("Speed");
        }

        // MoveSetFlag — { guid, u32 seq }
        [Parser(Opcode.SMSG_MOVE_ROOT)]
        [Parser(Opcode.SMSG_MOVE_UNROOT)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_GRAVITY)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_GRAVITY)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_COLLISION)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_COLLISION)]
        [Parser(Opcode.SMSG_MOVE_SET_FEATHER_FALL)]
        [Parser(Opcode.SMSG_MOVE_SET_NORMAL_FALL)]
        [Parser(Opcode.SMSG_MOVE_SET_HOVERING)]
        [Parser(Opcode.SMSG_MOVE_SET_WATER_WALK)]
        [Parser(Opcode.SMSG_MOVE_SET_LAND_WALK)]
        [Parser(Opcode.SMSG_MOVE_SET_CAN_FLY)]
        [Parser(Opcode.SMSG_MOVE_SET_CAN_TURN_WHILE_FALLING)]
        [Parser(Opcode.SMSG_MOVE_SET_CANNOT_SWIM)]
        [Parser(Opcode.SMSG_MOVE_SET_CAN_ADV_FLY)]
        [Parser(Opcode.SMSG_MOVE_SET_CAN_DRIVE)]
        [Parser(Opcode.SMSG_MOVE_SET_ALWAYS_ALLOW_PITCHING)]
        [Parser(Opcode.SMSG_MOVE_SET_IGNORE_MOVEMENT_FORCES)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_STRAFING)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_STRAFING)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_JUMPING)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_JUMPING)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_FULL_SPEED_TURNING)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_FULL_SPEED_TURNING)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_FULL_SPEED_PITCHING)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_FULL_SPEED_PITCHING)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_DOUBLE_JUMP)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_DOUBLE_JUMP)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_TRANSITION_BETWEEN_SWIM_AND_FLY)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_TRANSITION_BETWEEN_SWIM_AND_FLY)]
        [Parser(Opcode.SMSG_MOVE_DISABLE_INERTIA)]
        [Parser(Opcode.SMSG_MOVE_ENABLE_INERTIA)]
        [Parser(Opcode.SMSG_MOVE_APPLY_INERTIA)]
        [Parser(Opcode.SMSG_MOVE_REMOVE_INERTIA)]
        [Parser(Opcode.SMSG_MOVE_MARK_REMOTE_TIME_INVALID)]
        public static void HandleMovementSetFlag(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
        }

        [Parser(Opcode.SMSG_MOVE_SET_VEHICLE_REC_ID)]
        public static void HandleMoveSetVehicleRecId(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
            packet.ReadUInt32("VehicleRecID");
        }

        // MoveSplineSetSpeed — { guid, f32 }
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_RUN_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_RUN_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_SWIM_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_SWIM_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_WALK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_FLIGHT_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_FLIGHT_BACK_SPEED)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_TURN_RATE)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_PITCH_RATE)]
        public static void HandleMovementSplineSetSpeed(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadSingle("Speed");
        }

        // MoveSplineSetFlag / SetActiveMover — { guid }
        [Parser(Opcode.SMSG_MOVE_SET_ACTIVE_MOVER)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_ROOT)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_DISABLE_GRAVITY)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_ENABLE_GRAVITY)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_DISABLE_COLLISION)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_ENABLE_COLLISION)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_FEATHER_FALL)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_NORMAL_FALL)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_HOVER)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_WATER_WALK)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_LAND_WALK)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_FLYING)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_RUN_MODE)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_SET_WALK_MODE)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_START_SWIM)]
        [Parser(Opcode.SMSG_MOVE_SPLINE_STOP_SWIM)]
        public static void HandleSplineMovementFlag(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
        }

        [Parser(Opcode.SMSG_MOVE_TELEPORT)]
        public static void HandleMoveTeleport(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
            packet.ReadVector3("Position");
            packet.ReadSingle("Facing");

            packet.ResetBitReader();
            var hasTransportGuid = packet.ReadBit("HasTransportGUID");
            var hasVehicle = packet.ReadBit("HasVehicle");
            packet.ReadBit("PreloadWorld");

            if (hasTransportGuid)
                packet.ReadPackedGuid128("TransportGUID");

            if (hasVehicle)
            {
                packet.ReadByte("VehicleSeatIndex");
                packet.ResetBitReader();
                packet.ReadBit("VehicleExitVoluntary");
                packet.ReadBit("VehicleExitTeleport");
            }
        }

        [Parser(Opcode.SMSG_MOVE_KNOCK_BACK)]
        public static void HandleMoveKnockBack(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
            packet.ReadVector2("Direction");
            packet.ReadSingle("HorzSpeed");
            packet.ReadSingle("VertSpeed");
        }

        [Parser(Opcode.SMSG_MOVE_SET_COLLISION_HEIGHT)]
        public static void HandleMoveSetCollisionHeight(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
            packet.ReadSingle("Height");
            packet.ReadSingle("Scale");
            packet.ReadByte("Reason");
            packet.ReadUInt32("MountDisplayID");
            packet.ReadInt32("ScaleDuration");
        }

        [Parser(Opcode.SMSG_MOVE_APPLY_MOVEMENT_FORCE)]
        public static void HandleMoveApplyMovementForce(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
            MovementHandler1158.ReadMovementForce(packet, "MovementForce");
        }

        [Parser(Opcode.SMSG_MOVE_REMOVE_MOVEMENT_FORCE)]
        public static void HandleMoveRemoveMovementForce(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("SequenceIndex");
            packet.ReadPackedGuid128("MovementForceGUID");
        }

        [Parser(Opcode.SMSG_MOVE_SKIP_TIME)]
        public static void HandleMoveSkipTime(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            packet.ReadUInt32("TimeSkipped");
        }

        [Parser(Opcode.SMSG_FLIGHT_SPLINE_SYNC)]
        public static void HandleFlightSplineSync(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadSingle("SplineDist");
        }

        [Parser(Opcode.SMSG_CONTROL_UPDATE)]
        public static void HandleControlUpdate(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ResetBitReader();
            packet.ReadBit("On");
        }

        [Parser(Opcode.SMSG_NEW_WORLD)]
        public static void HandleNewWorld(Packet packet)
        {
            CoreParsers.MovementHandler.CurrentMapId = (uint)packet.ReadInt32<MapId>("Map");
            packet.ReadVector3("Position");
            packet.ReadInt32("FloorDifficulty");
            packet.ReadInt32("FloorIndex");
            packet.ReadUInt32("Reason");
            packet.ReadVector3("MovementOffset");
            packet.ReadInt32("Counter");
            packet.ReadUInt64("InstanceID");

            packet.AddSniffData(StoreNameType.Map, (int)CoreParsers.MovementHandler.CurrentMapId, "NEW_WORLD");
        }

        [Parser(Opcode.SMSG_TRANSFER_PENDING)]
        public static void HandleTransferPending(Packet packet)
        {
            packet.ReadInt32<MapId>("MapID");
            packet.ReadVector3("OldMapPosition");

            packet.ResetBitReader();
            var hasShipTransferPending = packet.ReadBit("HasShipTransferPending");
            var hasTransferSpell = packet.ReadBit("HasTransferSpell");
            var hasTaxiPathID = packet.ReadBit("HasTaxiPathID");

            if (hasShipTransferPending)
            {
                packet.ReadUInt32("ShipID");
                packet.ReadInt32<MapId>("OriginMapID");
            }

            if (hasTransferSpell)
                packet.ReadUInt32<SpellId>("TransferSpellID");

            if (hasTaxiPathID)
                packet.ReadInt32("TaxiPathID");
        }

        [Parser(Opcode.SMSG_TRANSFER_ABORTED)]
        public static void HandleTransferAborted(Packet packet)
        {
            packet.ReadUInt32("MapID");
            packet.ReadByte("Arg");
            packet.ReadInt32("MapDifficultyXConditionID");
            packet.ReadBits("TransfertAbort", 6);
        }

        [Parser(Opcode.SMSG_SUMMON_REQUEST)]
        public static void HandleSummonRequest(Packet packet)
        {
            packet.ReadPackedGuid128("SummonerGUID");
            packet.ReadUInt32("SummonerVirtualRealmAddress");
            packet.ReadInt32<ZoneId>("AreaID");
            packet.ReadByte("Reason");
            packet.ResetBitReader();
            packet.ReadBit("SkipStartingArea");
        }

        [Parser(Opcode.SMSG_SUSPEND_TOKEN)]
        [Parser(Opcode.SMSG_RESUME_TOKEN)]
        public static void HandleSuspendToken(Packet packet)
        {
            packet.ReadUInt32("SequenceIndex");
            packet.ReadBits("Reason", 2);
        }

        // MoveSetCompoundState — { guid, u32 count, MoveStateChange[] }
        [Parser(Opcode.SMSG_MOVE_SET_COMPOUND_STATE)]
        public static void HandleMoveSetCompoundState(Packet packet)
        {
            packet.ReadPackedGuid128("MoverGUID");
            var count = packet.ReadUInt32("StateChangesCount");
            for (var i = 0; i < count; ++i)
                ReadMoveStateChange160(packet, i);
        }

        private static void ReadMoveStateChange160(Packet packet, params object[] idx)
        {
            var opcode = packet.ReadInt32();
            var opcodeName = Opcodes.GetOpcodeName(opcode, packet.Direction);
            packet.AddValue("MessageID", $"{opcodeName} (0x{opcode:X4})", idx);

            packet.ReadInt32("SequenceIndex", idx);

            packet.ResetBitReader();
            var hasSpeed = packet.ReadBit("HasSpeed", idx);
            var hasSpeedRange = packet.ReadBit("HasSpeedRange", idx);
            var hasKnockBack = packet.ReadBit("HasKnockBack", idx);
            var hasVehicle = packet.ReadBit("HasVehicleRecID", idx);
            var hasCollisionHeight = packet.ReadBit("HasCollisionHeight", idx);
            var hasMovementForce = packet.ReadBit("HasMovementForce", idx);
            var hasMovementForceGUID = packet.ReadBit("HasMovementForceGUID", idx);
            var hasMovementInertiaID = packet.ReadBit("HasMovementInertiaID", idx);
            var hasMovementInertiaLifetimeMs = packet.ReadBit("HasMovementInertiaLifetimeMs", idx);
            var hasDriveCapabilityID = packet.ReadBit("HasDriveCapabilityRecID", idx);

            if (hasSpeed)
                packet.ReadSingle("Speed", idx);

            if (hasSpeedRange)
            {
                packet.ReadSingle("SpeedRangeMin", idx);
                packet.ReadSingle("SpeedRangeMax", idx);
            }

            if (hasKnockBack)
            {
                packet.ReadSingle("HorzSpeed", idx);
                packet.ReadVector2("Direction", idx);
                packet.ReadSingle("InitVertSpeed", idx);
            }

            if (hasVehicle)
                packet.ReadInt32("VehicleRecID", idx);

            if (hasCollisionHeight)
            {
                packet.ReadSingle("Height", idx);
                packet.ReadSingle("Scale", idx);
                packet.ReadByte("UpdateCollisionHeightReason", idx);
            }

            if (hasMovementForce)
                MovementHandler1158.ReadMovementForce(packet, "MovementForce", idx);

            if (hasMovementForceGUID)
                packet.ReadPackedGuid128("MovementForceGUID", idx);

            if (hasMovementInertiaID)
                packet.ReadInt32("MovementInertiaID", idx);

            if (hasMovementInertiaLifetimeMs)
                packet.ReadUInt32("MovementInertiaLifetimeMs", idx);

            if (hasDriveCapabilityID)
                packet.ReadInt32("DriveCapabilityRecID", idx);
        }
    }
}
