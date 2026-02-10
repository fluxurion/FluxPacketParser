using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V11_0_0_55666.Parsers
{
    public static class GarrisonHandler
    {
        private static void ReadGarrisonShipment(Packet packet, params object[] indexes)
        {
            /* backup
            packet.ReadUInt32("ShipmentID", indexes);
            packet.ReadPackedTime("CreationTime", indexes);
            packet.ReadUInt64("UnkInt64", indexes);
            packet.ReadUInt32("asd", indexes);
            packet.ReadUInt16("GarrFollowerID1", indexes);
            packet.ReadByte("GarrFollowerID3", indexes);
            packet.ReadByte("GarrFollowerID4", indexes);
            packet.ReadInt32("unk", indexes);
            packet.ReadDuration("ShipmentDuration", 4, WowPacketParser.Misc.DurationUnit.Seconds, indexes);
            packet.ReadUInt32("asd", indexes);
            packet.ReadByte("BuildingTypeID", indexes);
            */

            packet.ReadUInt32("ShipmentID", indexes);
            packet.ReadPackedTime("CreationTime", indexes);
            packet.ReadUInt64("UnkInt64", indexes);
            packet.ReadUInt32("asd", indexes);
            packet.ReadByte("idk", indexes);
            packet.ReadByte("idk", indexes); // idk could be uint64 uint32 uint16 or byte
            packet.ReadByte("idk", indexes);
            packet.ReadByte("idk", indexes);
            packet.ReadInt32("unk", indexes);
            packet.ReadDuration("ShipmentDuration", 4, WowPacketParser.Misc.DurationUnit.Seconds, indexes);
            packet.ReadUInt32("asd", indexes);
            packet.ReadByte("BuildingTypeID", indexes);
        }

        [Parser(Opcode.SMSG_GET_SHIPMENT_INFO_RESPONSE)]
        public static void HandleGetShipmentInfoResponse(Packet packet)
        {
            packet.ReadBit("Success");
            packet.ReadUInt32("ShipmentID");
            packet.ReadUInt32("MaxShipments");
            var characterShipmentCount = packet.ReadUInt16("CharacterShipmentCount");            
            packet.ReadUInt32("PlotInstanceID");
            packet.ReadUInt16("Unk");
            
            for (uint i = 0; i < characterShipmentCount; i++)
                ReadGarrisonShipment(packet, i);
        }

        [Parser(Opcode.SMSG_GET_LANDING_PAGE_SHIPMENTS_RESPONSE)]
        public static void HandleGetLandingPageShipmentsResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            var landingPageShipmentCount = packet.ReadUInt32("LandingPageShipmentCount");
            
            for (uint i = 0; i < landingPageShipmentCount; i++)
                ReadGarrisonShipment(packet, i);
        }

        [Parser(Opcode.SMSG_OPEN_SHIPMENT_NPC_RESULT)]
        public static void HandleOpenShipmentNpcResult(Packet packet)
        {
            packet.ReadPackedGuid("Npc");
            packet.ReadUInt32("CharShipmentContainerID");
            packet.ReadBool("IsSuccess");
        }

        [Parser(Opcode.SMSG_CREATE_SHIPMENT_RESPONSE)]
        public static void HandleCreateShipmentResponse(Packet packet)
        {
            packet.ReadUInt32("ShipmentID");
            packet.ReadUInt32("asd");
            packet.ReadUInt32("asd");
            packet.ReadUInt32("asd");
        }


    }
}