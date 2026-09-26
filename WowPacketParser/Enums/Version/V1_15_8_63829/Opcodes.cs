using WowPacketParser.Misc;

namespace WowPacketParser.Enums.Version.V1_15_8_63829
{
    public static class Opcodes_1_15_8
    {
        public static BiDictionary<Opcode, int> Opcodes(Direction direction)
        {
            switch (direction)
            {
                case Direction.ClientToServer:
                    return ClientOpcodes;
                case Direction.ServerToClient:
                    return ServerOpcodes;
                default:
                    return MiscOpcodes;
            }
        }

        private static readonly BiDictionary<Opcode, int> ClientOpcodes = new()
        {
            { Opcode.CMSG_SUSPEND_COMMS_ACK, 0x450000 },
            { Opcode.CMSG_AUTH_SESSION, 0x450001 },
            { Opcode.CMSG_AUTH_CONTINUED_SESSION, 0x450003 },
            { Opcode.CMSG_ENTER_ENCRYPTED_MODE_ACK, 0x450005 },
            { Opcode.CMSG_UNK_NEW_CLASSIC, 0x450007 },
            { Opcode.CMSG_QUEUED_MESSAGES_END, 0x45000A },
            { Opcode.CMSG_DB_QUERY_BULK, 0x440010 },
            { Opcode.CMSG_HOTFIX_REQUEST, 0x440011 },
            { Opcode.CMSG_ENUM_CHARACTERS, 0x440014 },
            { Opcode.CMSG_CHARACTER_CHECK_UPGRADE, 0x4400F5 },
            { Opcode.CMSG_REQUEST_HOTFIX, 0x4400F6 },
            { Opcode.CMSG_SERVER_TIME_OFFSET_REQUEST, 0x4400CC },
            { Opcode.CMSG_BATTLENET_REQUEST, 0x44012F },
            { Opcode.CMSG_GET_LAST_CATALOG_FETCH, 0x2D0036 },
            { Opcode.CMSG_CHAT_MESSAGE_SAY, 0x2F0023 },
            { Opcode.CMSG_SEND_TEXT_EMOTE, 0x340013 },
            { Opcode.CMSG_USE_ITEM, 0x30016B },
        };

        private static readonly BiDictionary<Opcode, int> ServerOpcodes = new()
        {
            { Opcode.SMSG_AUTH_CHALLENGE, 0x4D0000 },
            { Opcode.SMSG_ENTER_ENCRYPTED_MODE, 0x4D0004 },
            { Opcode.SMSG_SUSPEND_COMMS, 0x4D0005 },
            { Opcode.SMSG_RESUME_COMMS, 0x4D0006 },
            { Opcode.SMSG_CONNECT_TO, 0x4D0008 },
            { Opcode.SMSG_ENUM_CHARACTERS_RESULT, 0x460018 },
            { Opcode.SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN, 0x460064 },
            { Opcode.SMSG_SET_TIME_ZONE_INFORMATION, 0x460124 },
            { Opcode.SMSG_UPDATE_BNET_SESSION_KEY, 0x4602D8 },             
            { Opcode.SMSG_WARDEN3_ENABLED, 0x4602D5 }, 
            { Opcode.SMSG_AUTH_RESPONSE, 0x460001 }, 
            { Opcode.SMSG_MIRROR_VARS, 0x460368 },
            { Opcode.SMSG_TEXT_EMOTE, 0x3A011D },
            { Opcode.SMSG_EMOTE, 0x3A026C },
            { Opcode.SMSG_CHAT, 0x3F0001 },
            { Opcode.SMSG_ON_MONSTER_MOVE, 0x4C0002 },
            { Opcode.SMSG_MOVE_UPDATE_TELEPORT, 0x4C000F },
            { Opcode.SMSG_UPDATE_OBJECT, 0x4A0000 },
            { Opcode.SMSG_AVAILABLE_HOTFIXES, 0x4A0001 },
            { Opcode.SMSG_HOTFIX_CONNECT, 0x4A0003 },
            { Opcode.SMSG_CACHE_VERSION, 0x4A000E },
            { Opcode.SMSG_BATTLE_NET_CONNECTION_STATUS, 0x4602BB },
            { Opcode.SMSG_BATTLE_PAY_GET_PURCHASE_LIST_RESPONSE, 0x460225 },
            { Opcode.SMSG_BATTLE_PAY_GET_PRODUCT_LIST_RESPONSE, 0x460224 },
            { Opcode.SMSG_SERVER_TIME_OFFSET, 0x4601C3 },
            { Opcode.SMSG_AURA_UPDATE, 0x510011 },
            { Opcode.SMSG_SPELL_GO, 0x510028 },
            { Opcode.SMSG_SPELL_START, 0x510029 },
            { Opcode.SMSG_PET_SPELLS_MESSAGE, 0x510014 },
        };

        private static readonly BiDictionary<Opcode, int> MiscOpcodes = new();
    }
}
