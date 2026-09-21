using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V12_0_0_65390.Parsers
{
    // Housing system opcode inventory (12.1.0.69214) - hex / decimal
    //
    // Structures ported from agatho/TrinityCore feature/housing-system:
    //   src/server/game/Server/Packets/HousingPackets.h/.cpp
    //   src/server/game/Server/Packets/HousingBlueprintPackets.h/.cpp
    //   src/server/game/Handlers/NeighborhoodHandler.cpp
    //
    // House exterior / interior
    //   CMSG_HOUSE_EXTERIOR_LOCK                       = 0x2F0000 / 3080192
    //   CMSG_HOUSE_EXTERIOR_SET_HOUSE_POSITION         = 0x2F0001 / 3080193
    //   CMSG_HOUSE_INTERIOR_LEAVE_HOUSE                = 0x300001 / 3145729
    //   SMSG_HOUSE_EXTERIOR_LOCK_RESPONSE              = 0x530000 / 5439488
    //   SMSG_HOUSE_EXTERIOR_SET_HOUSE_POSITION_RESPONSE= 0x530001 / 5439489
    //
    // Blueprints
    //   CMSG_HOUSING_BLUEPRINT_EXPORT                  = 0x310000 / 3211264
    //   CMSG_HOUSING_BLUEPRINT_REQUEST_COLLECTION      = 0x310001 / 3211265
    //   CMSG_HOUSING_BLUEPRINT_RENAME                  = 0x310002 / 3211266
    //   CMSG_HOUSING_BLUEPRINT_DELETE                  = 0x310003 / 3211267
    //   CMSG_HOUSING_BLUEPRINT_IMPORT                  = 0x310005 / 3211269
    //   CMSG_HOUSING_BLUEPRINT_REQUEST_CONTENTS        = 0x310008 / 3211272
    //   SMSG_HOUSING_BLUEPRINT_EXPORT_RESPONSE         = 0x540000 / 5505024
    //   SMSG_HOUSING_BLUEPRINT_GET_RESPONSE            = 0x540001 / 5505025
    //   SMSG_HOUSING_BLUEPRINT_RENAME_RESPONSE         = 0x540002 / 5505026
    //   SMSG_HOUSING_BLUEPRINT_DELETE_RESPONSE         = 0x540003 / 5505027
    //   SMSG_HOUSING_BLUEPRINT_IMPORT_RESPONSE         = 0x540004 / 5505028
    //   SMSG_HOUSING_BLUEPRINT_CHECK_RESPONSE          = 0x540007 / 5505031
    //
    // Decor editing
    //   CMSG_HOUSING_DECOR_SET_EDIT_MODE               = 0x320000 / 3276800
    //   CMSG_HOUSING_DECOR_PLACE                       = 0x320001 / 3276801
    //   CMSG_HOUSING_DECOR_MOVE                        = 0x320002 / 3276802
    //   CMSG_HOUSING_DECOR_SET_PET                     = 0x320003 / 3276803
    //   CMSG_HOUSING_DECOR_REMOVE                      = 0x320004 / 3276804
    //   CMSG_HOUSING_DECOR_LOCK                        = 0x320005 / 3276805
    //   CMSG_HOUSING_DECOR_SET_DYE_SLOTS               = 0x320007 / 3276807
    //   CMSG_HOUSING_DECOR_DELETE_FROM_STORAGE         = 0x32000C / 3276812
    //   CMSG_HOUSING_DECOR_REQUEST_STORAGE             = 0x320011 / 3276817
    //   CMSG_HOUSING_DECOR_REDEEM_DEFERRED_DECOR       = 0x320013 / 3276819
    //   SMSG_HOUSING_DECOR_SET_EDIT_MODE_RESPONSE      = 0x550000 / 5570560
    //   SMSG_HOUSING_DECOR_DRAW_SERVER_LIGHTING_DEBUG_SPHERES_RESPONSE = 0x550001 / 5570561 (no TC impl)
    //   SMSG_HOUSING_DECOR_MOVE_RESPONSE               = 0x550002 / 5570562
    //   SMSG_HOUSING_DECOR_PLACE_RESPONSE              = 0x550003 / 5570563
    //   SMSG_HOUSING_DECOR_REMOVE_RESPONSE             = 0x550004 / 5570564
    //   SMSG_HOUSING_DECOR_LOCK_RESPONSE               = 0x550005 / 5570565
    //   SMSG_HOUSING_DECOR_DELETE_FROM_STORAGE_RESPONSE= 0x550006 / 5570566
    //   SMSG_HOUSING_DECOR_REQUEST_STORAGE_RESPONSE    = 0x550007 / 5570567
    //   SMSG_HOUSING_DECOR_ADD_TO_HOUSE_CHEST_RESPONSE = 0x550008 / 5570568
    //   SMSG_HOUSING_DECOR_SYSTEM_SET_DYE_SLOTS_RESPONSE = 0x550009 / 5570569
    //   SMSG_HOUSING_REDEEM_DEFERRED_DECOR_RESPONSE    = 0x55000A / 5570570
    //   SMSG_HOUSING_FIRST_TIME_DECOR_ACQUISITION      = 0x55000B / 5570571
    //
    // Fixtures / house structure
    //   CMSG_HOUSING_FIXTURE_SET_EDIT_MODE             = 0x330000 / 3342336
    //   CMSG_HOUSING_FIXTURE_SET_HOUSE_SIZE            = 0x330003 / 3342339
    //   CMSG_HOUSING_FIXTURE_SET_HOUSE_TYPE            = 0x330004 / 3342340
    //   CMSG_HOUSING_FIXTURE_SET_CORE_FIXTURE          = 0x330005 / 3342341
    //   CMSG_HOUSING_FIXTURE_CREATE_FIXTURE            = 0x330006 / 3342342
    //   CMSG_HOUSING_FIXTURE_DELETE_FIXTURE            = 0x330007 / 3342343
    //   SMSG_HOUSING_FIXTURE_SET_EDIT_MODE_RESPONSE    = 0x560000 / 5636096
    //   SMSG_HOUSING_FIXTURE_CREATE_BASIC_HOUSE_RESPONSE = 0x560001 / 5636097
    //   SMSG_HOUSING_FIXTURE_DELETE_HOUSE_RESPONSE     = 0x560002 / 5636098 (TC retired)
    //   SMSG_HOUSING_FIXTURE_SET_HOUSE_SIZE_RESPONSE   = 0x560003 / 5636099
    //   SMSG_HOUSING_FIXTURE_SET_HOUSE_TYPE_RESPONSE   = 0x560004 / 5636100
    //   SMSG_HOUSING_FIXTURE_SET_CORE_FIXTURE_RESPONSE = 0x560005 / 5636101
    //   SMSG_HOUSING_FIXTURE_CREATE_FIXTURE_RESPONSE   = 0x560006 / 5636102
    //   SMSG_HOUSING_FIXTURE_DELETE_FIXTURE_RESPONSE   = 0x560007 / 5636103
    //
    // Room editing
    //   CMSG_HOUSING_ROOM_SET_LAYOUT_EDIT_MODE         = 0x340000 / 3407872
    //   CMSG_HOUSING_ROOM_ADD                          = 0x340001 / 3407873
    //   CMSG_HOUSING_ROOM_REMOVE                       = 0x340002 / 3407874
    //   CMSG_HOUSING_ROOM_ROTATE                       = 0x340003 / 3407875
    //   CMSG_HOUSING_ROOM_MOVE                         = 0x340004 / 3407876
    //   CMSG_HOUSING_ROOM_SET_COMPONENT_THEME          = 0x340005 / 3407877
    //   CMSG_HOUSING_ROOM_APPLY_COMPONENT_MATERIALS    = 0x340006 / 3407878
    //   CMSG_HOUSING_ROOM_SET_DOOR_TYPE                = 0x340007 / 3407879
    //   CMSG_HOUSING_ROOM_SET_CEILING_TYPE             = 0x340008 / 3407880
    //   SMSG_HOUSING_ROOM_SET_LAYOUT_EDIT_MODE_RESPONSE= 0x570000 / 5701632
    //   SMSG_HOUSING_ROOM_ADD_RESPONSE                 = 0x570001 / 5701633
    //   SMSG_HOUSING_ROOM_REMOVE_RESPONSE              = 0x570002 / 5701634
    //   SMSG_HOUSING_ROOM_UPDATE_RESPONSE              = 0x570003 / 5701635
    //   SMSG_HOUSING_ROOM_SET_COMPONENT_THEME_RESPONSE = 0x570004 / 5701636
    //   SMSG_HOUSING_ROOM_APPLY_COMPONENT_MATERIALS_RESPONSE = 0x570005 / 5701637
    //   SMSG_HOUSING_ROOM_SET_DOOR_TYPE_RESPONSE       = 0x570006 / 5701638
    //   SMSG_HOUSING_ROOM_SET_CEILING_TYPE_RESPONSE    = 0x570007 / 5701639
    //
    // Housing services (0x35 / 0x58)
    //   CMSG_HOUSING_SVCS_GUILD_CREATE_NEIGHBORHOOD        = 0x350001 / 3473409
    //   CMSG_HOUSING_SVCS_NEIGHBORHOOD_RESERVE_PLOT        = 0x350007 / 3473415
    //   CMSG_HOUSING_SVCS_RELINQUISH_HOUSE                 = 0x35000A / 3473418
    //   CMSG_HOUSING_SVCS_UPDATE_HOUSE_SETTINGS            = 0x35000B / 3473419
    //   CMSG_HOUSING_SVCS_PLAYER_VIEW_HOUSES_BY_PLAYER     = 0x350010 / 3473424
    //   CMSG_HOUSING_SVCS_PLAYER_VIEW_HOUSES_BY_BNET_ACCOUNT = 0x350011 / 3473425
    //   CMSG_HOUSING_SVCS_GET_PLAYER_HOUSES_INFO           = 0x350013 / 3473427
    //   CMSG_HOUSING_SVCS_TELEPORT_TO_PLOT                 = 0x350019 / 3473433
    //   CMSG_HOUSING_SVCS_START_TUTORIAL                   = 0x35001A / 3473434
    //   CMSG_HOUSING_SVCS_ACCEPT_NEIGHBORHOOD_OWNERSHIP    = 0x35001E / 3473438
    //   CMSG_HOUSING_SVCS_REJECT_NEIGHBORHOOD_OWNERSHIP    = 0x35001F / 3473439
    //   CMSG_HOUSING_SVCS_GET_POTENTIAL_HOUSE_OWNERS       = 0x350020 / 3473440
    //   CMSG_HOUSING_SVCS_GET_HOUSE_FINDER_INFO            = 0x350021 / 3473441
    //   CMSG_HOUSING_SVCS_GET_HOUSE_FINDER_NEIGHBORHOOD    = 0x350022 / 3473442
    //   CMSG_HOUSING_SVCS_GET_BNET_FRIEND_NEIGHBORHOODS    = 0x350023 / 3473443
    //   CMSG_HOUSING_SVCS_DELETE_ALL_NEIGHBORHOOD_INVITES  = 0x350025 / 3473445
    //   CMSG_HOUSING_SVCS_HOUSE_FINDER_IGNORE_NEIGHBORHOOD = 0x350026 / 3473446
    //   SMSG_HOUSING_SVCS_NOTIFY_PERMISSIONS_FAILURE               = 0x580000 / 5767168
    //   SMSG_HOUSING_SVCS_GUILD_CREATE_NEIGHBORHOOD_NOTIFICATION   = 0x580001 / 5767169
    //   SMSG_HOUSING_SVCS_CREATE_CHARTER_NEIGHBORHOOD_RESPONSE     = 0x580003 / 5767171
    //   SMSG_HOUSING_SVCS_NEIGHBORHOOD_RESERVE_PLOT_RESPONSE       = 0x580004 / 5767172
    //   SMSG_HOUSING_SVCS_CLEAR_PLOT_RESERVATION_RESPONSE          = 0x580005 / 5767173 (TC retired)
    //   SMSG_HOUSING_SVCS_RELINQUISH_HOUSE_RESPONSE                = 0x580007 / 5767175
    //   SMSG_HOUSING_SVCS_CANCEL_RELINQUISH_HOUSE_RESPONSE         = 0x580008 / 5767176
    //   SMSG_HOUSING_SVCS_GET_PLAYER_HOUSES_INFO_RESPONSE          = 0x58000B / 5767179
    //   SMSG_HOUSING_SVCS_PLAYER_VIEW_HOUSES_RESPONSE              = 0x58000C / 5767180
    //   SMSG_HOUSING_SVCS_CHANGE_HOUSE_COSMETIC_OWNER              = 0x580010 / 5767184
    //   SMSG_HOUSING_SVCS_UPDATE_HOUSES_LEVEL_FAVOR                = 0x580011 / 5767185
    //   SMSG_HOUSING_SVCS_GUILD_ADD_HOUSE_NOTIFICATION             = 0x580012 / 5767186
    //   SMSG_HOUSING_SVCS_GUILD_REMOVE_HOUSE_NOTIFICATION          = 0x580013 / 5767187
    //   SMSG_HOUSING_SVCS_GUILD_APPEND_NEIGHBORHOOD_NOTIFICATION   = 0x580014 / 5767188 (TC retired)
    //   SMSG_HOUSING_SVCS_GUILD_RENAME_NEIGHBORHOOD_NOTIFICATION   = 0x580015 / 5767189
    //   SMSG_HOUSING_SVCS_GUILD_GET_HOUSING_INFO_RESPONSE          = 0x580016 / 5767190
    //   SMSG_HOUSING_SVCS_ACCEPT_NEIGHBORHOOD_OWNERSHIP_RESPONSE   = 0x580017 / 5767191
    //   SMSG_HOUSING_SVCS_REJECT_NEIGHBORHOOD_OWNERSHIP_RESPONSE   = 0x580018 / 5767192
    //   SMSG_HOUSING_SVCS_NEIGHBORHOOD_OWNERSHIP_TRANSFERRED_RESPONSE = 0x580019 / 5767193
    //   SMSG_HOUSING_SVCS_GET_POTENTIAL_HOUSE_OWNERS_RESPONSE      = 0x58001A / 5767194
    //   SMSG_HOUSING_SVCS_UPDATE_HOUSE_SETTINGS_RESPONSE           = 0x58001B / 5767195
    //   SMSG_HOUSING_SVCS_GET_HOUSE_FINDER_INFO_RESPONSE           = 0x58001C / 5767196
    //   SMSG_HOUSING_SVCS_GET_HOUSE_FINDER_NEIGHBORHOOD_RESPONSE   = 0x58001D / 5767197
    //   SMSG_HOUSING_SVCS_GET_BNET_FRIEND_NEIGHBORHOODS_RESPONSE   = 0x58001E / 5767198
    //   SMSG_HOUSING_SVCS_HOUSE_FINDER_FORCE_REFRESH               = 0x58001F / 5767199
    //   SMSG_HOUSING_SVC_REQUEST_PLAYER_RELOAD_DATA                = 0x580020 / 5767200
    //   SMSG_HOUSING_SVCS_DELETE_ALL_NEIGHBORHOOD_INVITES_RESPONSE = 0x580021 / 5767201
    //   SMSG_HOUSING_SVCS_IGNORE_NEIGHBORHOOD_INVITE_RESPONSE      = 0x580022 / 5767202
    //   SMSG_HOUSING_SVCS_NEIGHBORHOOD_UPDATE_NAME_NOTIFICATION    = 0x580024 / 5767204
    //
    // Core house state (0x37 / 0x59)
    //   CMSG_HOUSING_HOUSE_STATUS                    = 0x370003 / 3604483
    //   CMSG_HOUSING_GET_CURRENT_HOUSE_INFO          = 0x370004 / 3604484
    //   CMSG_HOUSING_GET_PLAYER_PERMISSIONS          = 0x370005 / 3604485
    //   CMSG_HOUSING_RESET_KIOSK_MODE                = 0x370006 / 3604486
    //   CMSG_HOUSING_RESET_HOUSE                     = 0x370008 / 3604488
    //   SMSG_HOUSING_HOUSE_STATUS_RESPONSE           = 0x590000 / 5832704
    //   SMSG_HOUSING_GET_CURRENT_HOUSE_INFO_RESPONSE = 0x590001 / 5832705
    //   SMSG_HOUSING_GET_PLAYER_PERMISSIONS_RESPONSE = 0x590004 / 5832708
    //   SMSG_HOUSING_RESET_KIOSK_MODE_RESPONSE       = 0x590005 / 5832709
    //   SMSG_HOUSING_RESET_HOUSE_RESPONSE            = 0x590006 / 5832710
    //
    // Neighborhood charter (0x39 / 0x5F)
    //   CMSG_NEIGHBORHOOD_CHARTER_OPEN_CONFIRMATION_UI       = 0x390000 / 3735552
    //   CMSG_NEIGHBORHOOD_CHARTER_CREATE                     = 0x390001 / 3735553
    //   CMSG_NEIGHBORHOOD_CHARTER_EDIT                       = 0x390003 / 3735555
    //   CMSG_NEIGHBORHOOD_CHARTER_FINALIZE                   = 0x390004 / 3735556
    //   CMSG_NEIGHBORHOOD_CHARTER_ADD_SIGNATURE              = 0x390006 / 3735558
    //   CMSG_NEIGHBORHOOD_CHARTER_SEND_SIGNATURE_REQUEST     = 0x390007 / 3735559
    //   SMSG_NEIGHBORHOOD_CHARTER_UPDATE_RESPONSE            = 0x5F0000 / 6225920
    //   SMSG_NEIGHBORHOOD_CHARTER_OPEN_UI_RESPONSE           = 0x5F0001 / 6225921
    //   SMSG_NEIGHBORHOOD_CHARTER_SIGN_REQUEST               = 0x5F0002 / 6225922
    //   SMSG_NEIGHBORHOOD_CHARTER_ADD_SIGNATURE_RESPONSE     = 0x5F0003 / 6225923
    //   SMSG_NEIGHBORHOOD_CHARTER_OPEN_CONFIRMATION_UI_RESPONSE = 0x5F0004 / 6225924
    //   SMSG_NEIGHBORHOOD_CHARTER_SIGNATURE_REMOVED_NOTIFICATION = 0x5F0005 / 6225925
    //
    // Neighborhood management (0x3A/0x3B/0x3E/0x3F / 0x60/0x64)
    //   CMSG_NEIGHBORHOOD_INITIATIVE_SERVICE_STATUS_CHECK    = 0x3A0000 / 3801088
    //   CMSG_GET_AVAILABLE_INITIATIVE_REQUEST                = 0x3A0002 / 3801090
    //   CMSG_GET_INITIATIVE_ACTIVITY_LOG_REQUEST             = 0x3A0004 / 3801092
    //   CMSG_NEIGHBORHOOD_UPDATE_NAME                        = 0x3B0000 / 3866624
    //   CMSG_NEIGHBORHOOD_SET_PUBLIC_FLAG                    = 0x3B0001 / 3866625
    //   CMSG_NEIGHBORHOOD_ADD_SECONDARY_OWNER                = 0x3B0002 / 3866626
    //   CMSG_NEIGHBORHOOD_REMOVE_SECONDARY_OWNER             = 0x3B0003 / 3866627
    //   CMSG_NEIGHBORHOOD_INVITE_RESIDENT                    = 0x3B0004 / 3866628
    //   CMSG_NEIGHBORHOOD_CANCEL_INVITATION                  = 0x3B0005 / 3866629
    //   CMSG_NEIGHBORHOOD_PLAYER_DECLINE_INVITE              = 0x3B0006 / 3866630
    //   CMSG_NEIGHBORHOOD_PLAYER_GET_INVITE                  = 0x3B0007 / 3866631
    //   CMSG_NEIGHBORHOOD_GET_INVITES                        = 0x3B0008 / 3866632
    //   CMSG_NEIGHBORHOOD_BUY_HOUSE                          = 0x3B0009 / 3866633
    //   CMSG_NEIGHBORHOOD_MOVE_HOUSE                         = 0x3B000A / 3866634
    //   CMSG_NEIGHBORHOOD_OPEN_CORNERSTONE_UI                = 0x3B000B / 3866635
    //   CMSG_NEIGHBORHOOD_OFFER_OWNERSHIP                    = 0x3B000D / 3866637
    //   CMSG_NEIGHBORHOOD_GET_ROSTER                         = 0x3B000E / 3866638
    //   CMSG_NEIGHBORHOOD_EVICT_PLOT                         = 0x3B000F / 3866639
    //   CMSG_DECLINE_NEIGHBORHOOD_INVITES                    = 0x3E0134 / 4063540
    //   CMSG_INITIATIVE_UPDATE_ACTIVE_NEIGHBORHOOD           = 0x3F0000 / 4128768
    //   CMSG_QUERY_NEIGHBORHOOD_INFO                         = 0x4300B7 / 4391095
    //   CMSG_INVITE_PLAYER_TO_NEIGHBORHOOD                   = 0x43019B / 4391323
    //   SMSG_NEIGHBORHOOD_EVICT_PLAYER                       = 0x600000 / 6291456
    //   SMSG_NEIGHBORHOOD_UPDATE_NAME_RESPONSE               = 0x600001 / 6291457
    //   SMSG_NEIGHBORHOOD_ADD_SECONDARY_OWNER_RESPONSE       = 0x600003 / 6291459
    //   SMSG_NEIGHBORHOOD_REMOVE_SECONDARY_OWNER_RESPONSE    = 0x600004 / 6291460
    //   SMSG_NEIGHBORHOOD_BUY_HOUSE_RESPONSE                 = 0x600005 / 6291461
    //   SMSG_NEIGHBORHOOD_MOVE_HOUSE_RESPONSE                = 0x600006 / 6291462
    //   SMSG_NEIGHBORHOOD_OPEN_CORNERSTONE_UI_RESPONSE       = 0x600007 / 6291463
    //   SMSG_NEIGHBORHOOD_INVITE_RESIDENT_RESPONSE           = 0x600008 / 6291464
    //   SMSG_NEIGHBORHOOD_CANCEL_INVITATION_RESPONSE         = 0x600009 / 6291465
    //   SMSG_NEIGHBORHOOD_DECLINE_INVITATION_RESPONSE        = 0x60000A / 6291466
    //   SMSG_NEIGHBORHOOD_PLAYER_GET_INVITE_RESPONSE         = 0x60000B / 6291467
    //   SMSG_NEIGHBORHOOD_GET_INVITES_RESPONSE               = 0x60000C / 6291468
    //   SMSG_NEIGHBORHOOD_INVITE_NOTIFICATION                = 0x60000D / 6291469
    //   SMSG_NEIGHBORHOOD_OFFER_OWNERSHIP_RESPONSE           = 0x60000E / 6291470
    //   SMSG_NEIGHBORHOOD_GET_ROSTER_RESPONSE                = 0x60000F / 6291471
    //   SMSG_NEIGHBORHOOD_ROSTER_RESIDENT_UPDATE             = 0x600010 / 6291472
    //   SMSG_NEIGHBORHOOD_INVITE_NAME_LOOKUP_RESULT          = 0x600011 / 6291473
    //   SMSG_NEIGHBORHOOD_EVICT_PLOT_RESPONSE                = 0x600012 / 6291474
    //   SMSG_NEIGHBORHOOD_EVICT_PLOT_NOTICE                  = 0x600013 / 6291475
    //   SMSG_INVALIDATE_NEIGHBORHOOD                         = 0x640008 / 6553608
    //
    // Misc
    //   CMSG_GET_DECOR_REFUND_LIST                       = 0x2A0031 / 2752561
    //   CMSG_GET_ALL_LICENSED_DECOR_QUANTITIES           = 0x2A0034 / 2752564
    //   CMSG_GET_LAST_CATALOG_FETCH                      = 0x2A0036 / 2752566  (FluxCatalogHandler)
    //   CMSG_UPDATE_LAST_CATALOG_FETCH                   = 0x2A0035 / 2752565  (FluxCatalogHandler)
    //   CMSG_GUILD_GET_OTHERS_OWNED_HOUSES               = 0x2E0026 / 3014694
    //   CMSG_BULK_REFUND                                 = 0x2A0033 / 2752563
    //   CMSG_HOUSING_PHOTO_SHARING_COMPLETE_AUTHORIZATION= 0x4301A4 / 4391332
    //   CMSG_HOUSING_PHOTO_SHARING_CLEAR_AUTHORIZATION   = 0x4301A5 / 4391333
    //   SMSG_QUERY_NEIGHBORHOOD_NAME_RESPONSE            = 0x490012 / 4784146
    //   SMSG_INVALIDATE_NEIGHBORHOOD_NAME                = 0x490013 / 4784147
    //   SMSG_GUILD_OTHERS_OWNED_HOUSES_RESULT            = 0x510047 / 5308487
    //   SMSG_ACCOUNT_ROOM_COLLECTION_UPDATE              = 0x450054 / 4522068
    //   SMSG_ACCOUNT_EXTERIOR_FIXTURE_COLLECTION_UPDATE  = 0x450055 / 4522069
    //   SMSG_ACCOUNT_ROOM_THEME_COLLECTION_UPDATE        = 0x450056 / 4522070
    //   SMSG_ACCOUNT_ROOM_MATERIAL_COLLECTION_UPDATE     = 0x450057 / 4522071
    //   SMSG_ACCOUNT_HOUSE_TYPE_COLLECTION_UPDATE        = 0x450058 / 4522072
    //   SMSG_CRAFTING_HOUSE_HELLO_RESPONSE               = 0x45033E / 4522814
    //   SMSG_INITIATIVE_SERVICE_STATUS                   = 0x450366 / 4522854
    //   SMSG_INITIATIVE_TASK_COMPLETE                    = 0x450367 / 4522855
    //   SMSG_INITIATIVE_COMPLETE                         = 0x450368 / 4522856
    //   SMSG_CLEAR_INITIATIVE_TASK_CRITERIA_PROGRESS     = 0x450369 / 4522857
    //   SMSG_GET_PLAYER_INITIATIVE_INFO_RESULT           = 0x45036A / 4522858
    //   SMSG_GET_INITIATIVE_ACTIVITY_LOG_RESULT          = 0x45036B / 4522859
    //   SMSG_GET_INITIATIVE_REWARDS_RESULT               = 0x45036C / 4522860
    //   SMSG_INITIATIVE_REWARD_AVAILABLE                 = 0x45036D / 4522861
    //   SMSG_GET_DECOR_REFUND_LIST_RESPONSE              = 0x450377 / 4522871  (FluxMiscHandler)
    //   SMSG_BULK_REFUND_RESPONSE                        = 0x45037A / 4522874
    //   SMSG_GET_ALL_LICENSED_DECOR_QUANTITIES_RESPONSE  = 0x45037C / 4522876  (FluxMiscHandler)
    //   SMSG_LICENSED_DECOR_QUANTITIES_UPDATE            = 0x45037D / 4522877
    //   SMSG_LAST_CATALOG_FETCH_RESPONSE                 = 0x450380 / 4522880  (FluxCatalogHandler)
    //   SMSG_HOUSING_PHOTO_SHARING_AUTHORIZATION_RESULT  = 0x450381 / 4522881
    //   SMSG_HOUSING_PHOTO_SHARING_AUTHORIZATION_CLEARED_RESULT = 0x450382 / 4522882
    public static class FluxHousingHandler
    {
        // JamCliHouse (TC WriteJamCliHouse, 12.0.7 order):
        //   PackedGUID House + PackedGUID Owner + PackedGUID Neighborhood
        //   + uint8 HouseLevel + uint32 PlotIndex + uint8(bit7=hasOpt) [+ uint64]
        private static void ReadJamCliHouse(Packet packet, params object[] index)
        {
            packet.ResetBitReader();
            packet.ReadPackedGuid128("GUID", index);
            packet.ReadPackedGuid128("CosmeticOwner", index);
            packet.ReadPackedGuid128("NeighborhoodGUID", index);

            packet.ReadByte("PlotID", index);
            packet.ReadUInt32("HouseSettingFlags", index);

            var hasHasReservationTime = packet.ReadBit("HasReservationTime", index);
            if (hasHasReservationTime)
                packet.ReadTime64("HasReservationTime", index);
        }

        // JamCliHouseFinderNeighborhood BASE (IDA Housing_ReadNeighborhoodDetails):
        //   PackedGUID + PackedGUID + uint64 + uint64 + uint32(housesCount)
        //   + JamCliHouse[count] + uint8(nameLen, size+1) + uint8(bit7=BoolFlag) + String(nameLen)
        private static void ReadHouseFinderNeighborhoodBase(Packet packet, params object[] index)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid", index);
            packet.ReadPackedGuid128("OwnerGuid", index);
            packet.ReadUInt64("Field1", index);
            packet.ReadUInt64("Field2", index);
            var houseCount = packet.ReadUInt32("HousesCount", index);
            for (uint i = 0; i < houseCount; i++)
                ReadJamCliHouse(packet, index, i);
            var nameLen = packet.ReadByte("NameLength", index);
            var flags = packet.ReadByte("Flags", index);
            packet.AddValue("BoolFlag", (flags & 0x80) != 0, index);
            packet.ReadWoWString("Name", nameLen, index);
        }

        // FULL format (IDA Housing_ReadNeighborhoodResponsePayload) =
        //   details + int64 @+120 + int8 @+128
        private static void ReadHouseFinderNeighborhood(Packet packet, params object[] index)
        {
            ReadHouseFinderNeighborhoodBase(packet, index);
            packet.ReadUInt64("ExtraField", index);
            packet.ReadByte("ExtraFlags", index);
        }

        // InviteEntry (TC Housing_ParseInviteEntry): uint64 + PackedGUID + PackedGUID + uint64
        private static void ReadInviteEntry(Packet packet, params object[] index)
        {
            packet.ReadUInt64("Timestamp", index);
            packet.ReadPackedGuid128("PlayerGuid", index);
            packet.ReadPackedGuid128("HouseGuid", index);
            packet.ReadUInt64("ExtraData", index);
        }

        // JamHousingBlueprint (IDA JamHousingBlueprint_Read):
        //   int64 + int8 + int64 + int64 + int8 + int8
        //   + bits<24> strLen + bits<24> strLen + string + string
        private static void ReadBlueprint(Packet packet, params object[] index)
        {
            packet.ReadInt64("BlueprintID", index);
            packet.ReadByte("Flags", index);
            packet.ReadInt64("DateCreated", index);
            packet.ReadInt64("DateDeleted", index);
            packet.ReadByte("Type", index);
            packet.ReadByte("DeleteReason", index);
            var uuidLen = packet.ReadBits("UuidLength", 24, index);
            var nameLen = packet.ReadBits("NameLength", 24, index);
            packet.ResetBitReader();
            packet.ReadWoWString("Uuid", (int)uuidLen, index);
            packet.ReadWoWString("Name", (int)nameLen, index);
        }

        // JamBlueprintItemList: 4 x uint32-vector (Decor, DyeItem, Room, Fixture)
        private static void ReadBlueprintItemList(Packet packet, params object[] index)
        {
            var decorCount = packet.ReadUInt32("DecorIDsCount", index);
            for (uint i = 0; i < decorCount; i++)
                packet.ReadUInt32("DecorID", index, i);
            var dyeCount = packet.ReadUInt32("DyeItemIDsCount", index);
            for (uint i = 0; i < dyeCount; i++)
                packet.ReadUInt32("DyeItemID", index, i);
            var roomCount = packet.ReadUInt32("RoomIDsCount", index);
            for (uint i = 0; i < roomCount; i++)
                packet.ReadUInt32("RoomID", index, i);
            var fixtureCount = packet.ReadUInt32("FixtureIDsCount", index);
            for (uint i = 0; i < fixtureCount; i++)
                packet.ReadUInt32("FixtureID", index, i);
        }

        // AccountCollectionUpdateBase::WriteCollection:
        //   uint8(bit7=IsIncrementalUpdate) + uint32 IDs.size() + uint32 StateFlags.size()
        //   + uint32 IDs[] + Bits<1> StateFlags[] (8 per byte, bit 7 first)
        private static void ReadAccountCollectionUpdate(Packet packet)
        {
            var flags = packet.ReadByte("Flags");
            packet.AddValue("IsIncrementalUpdate", (flags & 0x80) != 0);
            var idCount = packet.ReadUInt32("IDsCount");
            var stateFlagCount = packet.ReadUInt32("StateFlagsCount");
            for (uint i = 0; i < idCount; i++)
                packet.ReadUInt32("ID", i);
            for (uint i = 0; i < stateFlagCount; i++)
                packet.ReadBit("StateFlag", i);
            packet.ResetBitReader();
        }

        // Charter response shape shared by 0x5F0000 / 0x5F0001:
        //   uint8 Result + PackedGUID + uint32 MapID + uint32 SignatureCount
        //   + uint32 SignersCount + uint32 Unknown + PackedGUID[SignersCount]
        //   + uint8(NameLen, size+1) + CString
        private static void ReadCharterResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("CharterGuid");
            packet.ReadUInt32("MapID");
            packet.ReadUInt32("SignatureCount");
            var signersCount = packet.ReadUInt32("SignersCount");
            packet.ReadUInt32("Unknown");
            for (uint i = 0; i < signersCount; i++)
                packet.ReadPackedGuid128("SignerGuid", i);
            var nameLen = packet.ReadByte("NeighborhoodNameLength");
            packet.ReadWoWString("NeighborhoodName", nameLen);
        }

        // ============================================================
        // House exterior / interior (0x2F / 0x30 / 0x53)
        // ============================================================

        [Parser(Opcode.CMSG_HOUSE_EXTERIOR_LOCK, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHouseExteriorLock(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadPackedGuid128("PlotGuid");
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ResetBitReader();
            packet.ReadBit("Locked");
            packet.ResetBitReader();
        }

        // TC HouseExteriorCommitPosition: Bits<1> HasPosition + HouseGuid + optional 7 floats
        [Parser(Opcode.CMSG_HOUSE_EXTERIOR_SET_HOUSE_POSITION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHouseExteriorSetHousePosition(Packet packet)
        {
            packet.ResetBitReader();
            var hasPosition = packet.ReadBit("HasPosition");
            packet.ResetBitReader();
            packet.ReadPackedGuid128("HouseGuid");
            if (hasPosition)
            {
                packet.ReadSingle("PositionX");
                packet.ReadSingle("PositionY");
                packet.ReadSingle("PositionZ");
                packet.ReadSingle("RotationX");
                packet.ReadSingle("RotationY");
                packet.ReadSingle("RotationZ");
                packet.ReadSingle("RotationW");
            }
        }

        [Parser(Opcode.SMSG_HOUSE_EXTERIOR_LOCK_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHouseExteriorLockResponse(Packet packet)
        {
            packet.ReadPackedGuid128("FixtureEntityGuid");
            packet.ReadPackedGuid128("EditorPlayerGuid");
            packet.ReadByte("Result");
            packet.ResetBitReader();
            packet.ReadBit("Active");
            packet.ResetBitReader();
        }

        [Parser(Opcode.SMSG_HOUSE_EXTERIOR_SET_HOUSE_POSITION_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHouseExteriorSetHousePositionResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("HouseGuid");
        }

        // ============================================================
        // Blueprints (0x31 / 0x54)
        // TC names/values differ from the 12.1 enum for SMSGs (TC's SMSG values
        // are inferred, ours come from the client table) — mapped by semantics:
        //   EXPORT_RESPONSE <- HousingBlueprintExportResult
        //   GET_RESPONSE    <- HousingBlueprintContents
        //   RENAME_RESPONSE <- HousingBlueprintRenameResult
        //   DELETE_RESPONSE <- HousingBlueprintDeleteResult
        //   IMPORT_RESPONSE <- HousingBlueprintImportResult
        //   CHECK_RESPONSE  <- HousingBlueprintsAvailabilityChanged
        // TC also defines HousingBlueprintCollection (u32 result + u32 count +
        // JamBlueprint[]) which has no matching enum name here — likely the real
        // carrier of the blueprint list; verify against a sniff.
        // ============================================================

        // TC HousingBlueprintExport: bits<6> Name + uint8 TypeByte + PackedGUID + Blob
        [Parser(Opcode.CMSG_HOUSING_BLUEPRINT_EXPORT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintExport(Packet packet)
        {
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 6);
            packet.ResetBitReader();
            packet.ReadByte("TypeByte");
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadWoWString("Name", (int)nameLen);
        }

        // TC HousingBlueprintRename: uint64 + bits<6> Name + Blob
        [Parser(Opcode.CMSG_HOUSING_BLUEPRINT_RENAME, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintRename(Packet packet)
        {
            packet.ReadUInt64("BlueprintId");
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 6);
            packet.ResetBitReader();
            packet.ReadWoWString("Name", (int)nameLen);
        }

        // TC HousingBlueprintDelete: uint64
        [Parser(Opcode.CMSG_HOUSING_BLUEPRINT_DELETE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintDelete(Packet packet)
        {
            packet.ReadUInt64("BlueprintId");
        }

        // TC HousingBlueprintImport: bits<24> Code + bits<1> Flag + uint8 TypeByte + PackedGUID + uint32 + Blob
        [Parser(Opcode.CMSG_HOUSING_BLUEPRINT_IMPORT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintImport(Packet packet)
        {
            packet.ResetBitReader();
            var codeLen = packet.ReadBits("CodeLength", 24);
            packet.ReadBit("Flag");
            packet.ResetBitReader();
            packet.ReadByte("TypeByte");
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadUInt32("BlueprintId");
            packet.ReadWoWString("Code", (int)codeLen);
        }

        // TC calls the 0x310008 packet HousingBlueprintExportRoom (binary-verified
        // 12.1 writer); our enum names it REQUEST_CONTENTS.
        // Wire: bits<24> Name + bits<1> Flag + uint8 TypeByte + PackedGUID RoomGuid + Blob
        [Parser(Opcode.CMSG_HOUSING_BLUEPRINT_REQUEST_CONTENTS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintRequestContents(Packet packet)
        {
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 24);
            packet.ReadBit("Flag");
            packet.ResetBitReader();
            packet.ReadByte("TypeByte");
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadWoWString("Name", (int)nameLen);
        }

        [Parser(Opcode.SMSG_HOUSING_BLUEPRINT_EXPORT_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintExportResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            ReadBlueprint(packet);
        }

        [Parser(Opcode.SMSG_HOUSING_BLUEPRINT_GET_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintGetResponse(Packet packet)
        {
            // IDA case 5505025: int8 + int32 count + count x JamHousingBlueprint
            packet.ReadByte("Result");
            var count = packet.ReadUInt32("BlueprintCount");
            for (uint i = 0; i < count; i++)
                ReadBlueprint(packet, i);
        }

        [Parser(Opcode.SMSG_HOUSING_BLUEPRINT_RENAME_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintRenameResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadUInt64("BlueprintId");
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 7);
            packet.ResetBitReader();
            packet.ReadWoWString("Name", (int)nameLen);
        }

        [Parser(Opcode.SMSG_HOUSING_BLUEPRINT_DELETE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintDeleteResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadUInt64("BlueprintId");
        }

        [Parser(Opcode.SMSG_HOUSING_BLUEPRINT_IMPORT_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintImportResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadPackedGuid128("HouseGuid");
            ReadBlueprintItemList(packet);
        }

        // TC HousingBlueprintsAvailabilityChanged: Bits<1> + uint32 + uint32
        [Parser(Opcode.SMSG_HOUSING_BLUEPRINT_CHECK_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingBlueprintCheckResponse(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("Available");
            packet.ResetBitReader();
            packet.ReadUInt32("MaxPerBnetAccount");
            packet.ReadUInt32("MaxBackupsPerBnetAccount");
        }

        // ============================================================
        // Decor editing (0x32 / 0x55)
        // ============================================================

        [Parser(Opcode.CMSG_HOUSING_DECOR_SET_EDIT_MODE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorSetEditMode(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("Active");
            packet.ResetBitReader();
        }

        // IDA sub_7FF7CD4F56B0: PackedGUID + 11 floats + 3 PackedGUIDs + int32.
        // 11 floats = Position(3) + Rotation(3) + Scale(1) + 4 extra (unverified semantics)
        [Parser(Opcode.CMSG_HOUSING_DECOR_PLACE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorPlace(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadVector3("Position");
            packet.ReadVector3("Rotation");
            packet.ReadSingle("Scale");
            packet.ReadVector4("Field_44");
            packet.ReadPackedGuid128("AttachParentGuid");
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadPackedGuid128("AnchorMeshObjectGuid");
            packet.ReadInt32("AttachPoint");
        }

        // IDA sub_7FF7CD4F5900: PackedGUID + 11 floats + 3 PackedGUIDs + int32 + int8 + int8 + bit
        [Parser(Opcode.CMSG_HOUSING_DECOR_MOVE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorMove(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadVector3("Position");
            packet.ReadVector3("Rotation");
            packet.ReadSingle("Scale");
            packet.ReadVector4("Field_44");
            packet.ReadPackedGuid128("AttachParentGuid");
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadPackedGuid128("AnchorMeshObjectGuid");
            packet.ReadInt32("Field_80");
            packet.ReadByte("Field_85");
            packet.ReadByte("Field_86");
            packet.ReadBit("IsBasicMove");
            packet.ResetBitReader();
        }

        [Parser(Opcode.CMSG_HOUSING_DECOR_SET_PET, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorSetPet(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadPackedGuid128("PetGuid");
            packet.ReadByte("Flag");
        }

        [Parser(Opcode.CMSG_HOUSING_DECOR_REMOVE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorRemove(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
        }

        // TC HousingDecorLock: PackedGUID + 2 bits packed in one byte
        [Parser(Opcode.CMSG_HOUSING_DECOR_LOCK, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorLock(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            packet.ResetBitReader();
            packet.ReadBit("Locked");
            packet.ReadBit("Field_49");
            packet.ResetBitReader();
        }

        [Parser(Opcode.CMSG_HOUSING_DECOR_SET_DYE_SLOTS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorSetDyeSlots(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            for (uint i = 0; i < 3; i++)
                packet.ReadInt32("DyeColorID", i);
        }

        // TC: Bits<5> count + PackedGUID[count]
        [Parser(Opcode.CMSG_HOUSING_DECOR_DELETE_FROM_STORAGE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorDeleteFromStorage(Packet packet)
        {
            packet.ResetBitReader();
            var count = packet.ReadBits("Count", 5);
            packet.ResetBitReader();
            for (uint i = 0; i < count; i++)
                packet.ReadPackedGuid128("DecorGuid", i);
        }

        [Parser(Opcode.CMSG_HOUSING_DECOR_REQUEST_STORAGE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorRequestStorage(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_DECOR_REDEEM_DEFERRED_DECOR, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorRedeemDeferredDecor(Packet packet)
        {
            packet.ReadUInt32("DeferredDecorID");
            packet.ReadUInt32("RedemptionToken");
        }

        // TC HousingDecorSetEditModeResponse: guid + guid + u32 count + u8 result + guids[]
        [Parser(Opcode.SMSG_HOUSING_DECOR_SET_EDIT_MODE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorSetEditModeResponse(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadPackedGuid128("BNetAccountGuid");
            var count = packet.ReadUInt32("AllowedEditorCount");
            packet.ReadByte("Result");
            for (uint i = 0; i < count; i++)
                packet.ReadPackedGuid128("AllowedEditorGuid", i);
        }

        // TC: PackedGUID PlayerGuid + uint32 + PackedGUID DecorGuid + uint8 Result + uint8(bit7)
        [Parser(Opcode.SMSG_HOUSING_DECOR_MOVE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorMoveResponse(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadUInt32("Field_09");
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_DECOR_PLACE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorPlaceResponse(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadUInt32("Field_09");
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadByte("Result");
        }

        // IDA case 4 + sniff-verified 27B (69814):
        //   PackedGUID PlayerGuid + int32 + PackedGUID DecorGuid + int8 Result
        [Parser(Opcode.SMSG_HOUSING_DECOR_REMOVE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorRemoveResponse(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadUInt32("Field_09");
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadByte("Result");
        }

        // TC: guid + guid + u32 + u8 Result + u8 flags(bit7=Locked, bit6=Field_17)
        [Parser(Opcode.SMSG_HOUSING_DECOR_LOCK_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorLockResponse(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadUInt32("Field_16");
            packet.ReadByte("Result");
            // Sniff shows 20B total — no trailing flags byte (TC's Locked/Field_17 byte unverified)
        }

        // IDA case 5570566: PackedGUID + PackedGUID + int32 + int8 + int8 (bit7 + bit6 bools)
        [Parser(Opcode.SMSG_HOUSING_DECOR_DELETE_FROM_STORAGE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorDeleteFromStorageResponse(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadUInt32("Field_22");
            packet.ReadByte("Result");
            var flags = packet.ReadByte("Flags");
            packet.AddValue("Field_23", (flags & 0x80) != 0);
            packet.AddValue("Field_24", (flags & 0x40) != 0);
        }

        // TC: PackedGUID (BNetAccount) + uint8 ResultCode + uint8 Flags
        [Parser(Opcode.SMSG_HOUSING_DECOR_REQUEST_STORAGE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorRequestStorageResponse(Packet packet)
        {
            packet.ReadPackedGuid128("BNetAccountGuid");
            packet.ReadByte("ResultCode");
            packet.ReadByte("Flags");
        }

        // Sniff-verified failure: uint32 Result (0x80000000), no flag byte/count.
        // Success variant (with decor guid list) unverified; TC assumed u8+u32+guids.
        [Parser(Opcode.SMSG_HOUSING_DECOR_ADD_TO_HOUSE_CHEST_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorAddToHouseChestResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            if (!packet.CanRead())
                return;
            var count = packet.ReadUInt32("DecorCount");
            for (uint i = 0; i < count; i++)
                packet.ReadPackedGuid128("DecorGuid", i);
        }

        // IDA case 5570569: int8 (bit7 flag) + int32 count + PackedGUID[count]
        [Parser(Opcode.SMSG_HOUSING_DECOR_SYSTEM_SET_DYE_SLOTS_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingDecorSystemSetDyeSlotsResponse(Packet packet)
        {
            var flag = packet.ReadByte("Flags");
            packet.AddValue("Success", (flag & 0x80) != 0);
            var count = packet.ReadInt32("GuidCount");
            for (int i = 0; i < count; i++)
                packet.ReadPackedGuid128("DecorGuid", i);
        }

        // TC sniff-verified 17B: PackedGUID + uint8 Result + uint32 SequenceIndex
        [Parser(Opcode.SMSG_HOUSING_REDEEM_DEFERRED_DECOR_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRedeemDeferredDecorResponse(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadByte("Result");
            packet.ReadUInt32("SequenceIndex");
        }

        // IDA case 5570571: PackedGUID + int8 + int32
        [Parser(Opcode.SMSG_HOUSING_FIRST_TIME_DECOR_ACQUISITION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFirstTimeDecorAcquisition(Packet packet)
        {
            packet.ReadPackedGuid128("DecorGuid");
            packet.ReadByte("Result");
            packet.ReadInt32("DecorEntryID");
        }

        // ============================================================
        // Fixtures (0x33 / 0x56)
        // ============================================================

        [Parser(Opcode.CMSG_HOUSING_FIXTURE_SET_EDIT_MODE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureSetEditMode(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("Active");
            packet.ResetBitReader();
        }

        // TC: PackedGUID + uint8 Size + uint8 Flags
        [Parser(Opcode.CMSG_HOUSING_FIXTURE_SET_HOUSE_SIZE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureSetHouseSize(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadByte("Size");
            packet.ReadByte("Flags");
        }

        // TC: PackedGUID + uint32 HouseExteriorWmoDataID + uint8 Flags
        [Parser(Opcode.CMSG_HOUSING_FIXTURE_SET_HOUSE_TYPE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureSetHouseType(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadUInt32("HouseExteriorWmoDataID");
            packet.ReadByte("Flags");
        }

        // TC: PackedGUID + uint32 ExteriorComponentID + uint8 Flags
        [Parser(Opcode.CMSG_HOUSING_FIXTURE_SET_CORE_FIXTURE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureSetCoreFixture(Packet packet)
        {
            packet.ReadPackedGuid128("FixtureGuid");
            packet.ReadUInt32("ExteriorComponentID");
            packet.ReadByte("Flags");
        }

        // TC: 2xPackedGUID + uint32 HookID + uint32 ComponentID + uint8 Flags
        [Parser(Opcode.CMSG_HOUSING_FIXTURE_CREATE_FIXTURE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureCreateFixture(Packet packet)
        {
            packet.ReadPackedGuid128("AttachParentGuid");
            packet.ReadPackedGuid128("HookEntityGuid");
            packet.ReadUInt32("ExteriorComponentHookID");
            packet.ReadUInt32("ExteriorComponentID");
            packet.ReadByte("Flags");
        }

        // TC: 2xPackedGUID + uint32 ExteriorComponentID + uint8 Flags
        [Parser(Opcode.CMSG_HOUSING_FIXTURE_DELETE_FIXTURE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureDeleteFixture(Packet packet)
        {
            packet.ReadPackedGuid128("FixtureGuid");
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadUInt32("ExteriorComponentID");
            packet.ReadByte("Flags");
        }

        // TC: PackedGUID HouseGuid (always empty) + PackedGUID EditorPlayer + uint8 Result
        [Parser(Opcode.SMSG_HOUSING_FIXTURE_SET_EDIT_MODE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureSetEditModeResponse(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadPackedGuid128("EditorPlayerGuid");
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_FIXTURE_CREATE_BASIC_HOUSE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureCreateBasicHouseResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_FIXTURE_SET_HOUSE_SIZE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureSetHouseSizeResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadByte("Size");
        }

        // TC: uint8 Result + uint32 HouseExteriorTypeID + uint8 ExtraField
        [Parser(Opcode.SMSG_HOUSING_FIXTURE_SET_HOUSE_TYPE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureSetHouseTypeResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadUInt32("HouseExteriorTypeID");
            packet.ReadByte("ExtraField");
        }

        [Parser(Opcode.SMSG_HOUSING_FIXTURE_SET_CORE_FIXTURE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureSetCoreFixtureResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_FIXTURE_CREATE_FIXTURE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureCreateFixtureResponse(Packet packet)
        {
            packet.ReadPackedGuid128("FixtureGuid");
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_FIXTURE_DELETE_FIXTURE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingFixtureDeleteFixtureResponse(Packet packet)
        {
            packet.ReadPackedGuid128("FixtureGuid");
            packet.ReadByte("Result");
        }

        // ============================================================
        // Rooms (0x34 / 0x57)
        // ============================================================

        [Parser(Opcode.CMSG_HOUSING_ROOM_SET_LAYOUT_EDIT_MODE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomSetLayoutEditMode(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("Active");
            packet.ResetBitReader();
        }

        // TC: PackedGUID SourceRoom + uint32 + uint32 + uint32 + bit AutoFurnish
        [Parser(Opcode.CMSG_HOUSING_ROOM_ADD, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomAdd(Packet packet)
        {
            packet.ReadPackedGuid128("SourceRoomGuid");
            packet.ReadUInt32("TargetDoorComponentID");
            packet.ReadUInt32("HouseRoomID");
            packet.ReadUInt32("FloorIndex");
            packet.ResetBitReader();
            packet.ReadBit("AutoFurnish");
            packet.ResetBitReader();
        }

        [Parser(Opcode.CMSG_HOUSING_ROOM_REMOVE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomRemove(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_ROOM_ROTATE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomRotate(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            packet.ResetBitReader();
            packet.ReadBit("Clockwise");
            packet.ResetBitReader();
        }

        [Parser(Opcode.CMSG_HOUSING_ROOM_MOVE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomMove(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadUInt32("TargetSlotIndex");
            packet.ReadPackedGuid128("TargetGuid");
            packet.ReadUInt32("FloorIndex");
        }

        // TC: PackedGUID + uint32 OptionCount + uint32 HouseThemeID + uint32[OptionCount]
        [Parser(Opcode.CMSG_HOUSING_ROOM_SET_COMPONENT_THEME, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomSetComponentTheme(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            var count = packet.ReadUInt32("OptionIDsCount");
            packet.ReadUInt32("HouseThemeID");
            for (uint i = 0; i < count; i++)
                packet.ReadUInt32("OptionID", i);
        }

        // TC: PackedGUID + uint32 count + int32 ColorOverride + uint32 TextureID
        //     + uint8 ComponentSlot + uint32[count]
        [Parser(Opcode.CMSG_HOUSING_ROOM_APPLY_COMPONENT_MATERIALS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomApplyComponentMaterials(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            var count = packet.ReadUInt32("OptionIDsCount");
            packet.ReadInt32("ColorOverride");
            packet.ReadUInt32("RoomComponentTextureID");
            packet.ReadByte("ComponentSlot");
            for (uint i = 0; i < count; i++)
                packet.ReadUInt32("OptionID", i);
        }

        [Parser(Opcode.CMSG_HOUSING_ROOM_SET_DOOR_TYPE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomSetDoorType(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadUInt32("ThemeOptionID");
            packet.ReadByte("DoorType");
        }

        [Parser(Opcode.CMSG_HOUSING_ROOM_SET_CEILING_TYPE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomSetCeilingType(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadUInt32("ThemeOptionID");
            packet.ReadByte("CeilingType");
        }

        // TC: PackedGUID + uint8 Result + uint8(bit7=Active)
        [Parser(Opcode.SMSG_HOUSING_ROOM_SET_LAYOUT_EDIT_MODE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomSetLayoutEditModeResponse(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadByte("Result");
            var flags = packet.ReadByte("Flags");
            packet.AddValue("Active", (flags & 0x80) != 0);
        }

        [Parser(Opcode.SMSG_HOUSING_ROOM_ADD_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomAddResponse(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_ROOM_REMOVE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomRemoveResponse(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadPackedGuid128("SecondGuid");
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_ROOM_UPDATE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomUpdateResponse(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadByte("Result");
        }

        // TC: PackedGUID + uint32 count + uint32 ThemeSetID + uint8 Result + uint32[count]
        [Parser(Opcode.SMSG_HOUSING_ROOM_SET_COMPONENT_THEME_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomSetComponentThemeResponse(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            var count = packet.ReadUInt32("OptionIDsCount");
            packet.ReadUInt32("ThemeSetID");
            packet.ReadByte("Result");
            for (uint i = 0; i < count; i++)
                packet.ReadUInt32("OptionID", i);
        }

        // TC: PackedGUID + uint32 count + uint32 TextureID + uint8 Result + uint32[count]
        [Parser(Opcode.SMSG_HOUSING_ROOM_APPLY_COMPONENT_MATERIALS_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomApplyComponentMaterialsResponse(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            var count = packet.ReadUInt32("OptionIDsCount");
            packet.ReadUInt32("RoomComponentTextureID");
            packet.ReadByte("Result");
            for (uint i = 0; i < count; i++)
                packet.ReadUInt32("OptionID", i);
        }

        // TC: PackedGUID + uint32 ComponentID + uint8 DoorType + uint8 Result
        [Parser(Opcode.SMSG_HOUSING_ROOM_SET_DOOR_TYPE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomSetDoorTypeResponse(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadUInt32("ComponentID");
            packet.ReadByte("DoorType");
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_ROOM_SET_CEILING_TYPE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingRoomSetCeilingTypeResponse(Packet packet)
        {
            packet.ReadPackedGuid128("RoomGuid");
            packet.ReadUInt32("ComponentID");
            packet.ReadByte("CeilingType");
            packet.ReadByte("Result");
        }

        // ============================================================
        // Housing services (0x35 / 0x58)
        // ============================================================

        // TC: uint32 NeighborhoodTypeID + uint32 SecondaryID + SizedCString<8> name
        [Parser(Opcode.CMSG_HOUSING_SVCS_GUILD_CREATE_NEIGHBORHOOD, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGuildCreateNeighborhood(Packet packet)
        {
            packet.ReadUInt32("NeighborhoodTypeID");
            packet.ReadUInt32("SecondaryID");
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NeighborhoodNameLength", 8);
            packet.ResetBitReader();
            packet.ReadWoWString("NeighborhoodName", (int)nameLen);
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_NEIGHBORHOOD_RESERVE_PLOT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsNeighborhoodReservePlot(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ReadByte("PlotIndex");
            packet.ResetBitReader();
            packet.ReadBit("Reserve");
            packet.ResetBitReader();
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_RELINQUISH_HOUSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsRelinquishHouse(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
        }

        // TC: PackedGUID + OptionalInit(uint32) + OptionalInit(PackedGUID) + values
        [Parser(Opcode.CMSG_HOUSING_SVCS_UPDATE_HOUSE_SETTINGS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsUpdateHouseSettings(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
            packet.ResetBitReader();
            var hasPlotSettings = packet.ReadBit("HasPlotSettingsID");
            var hasVisitorPerm = packet.ReadBit("HasVisitorPermissionGuid");
            packet.ResetBitReader();
            if (hasPlotSettings)
                packet.ReadUInt32("PlotSettingsID");
            if (hasVisitorPerm)
                packet.ReadPackedGuid128("VisitorPermissionGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_PLAYER_VIEW_HOUSES_BY_PLAYER, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsPlayerViewHousesByPlayer(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_PLAYER_VIEW_HOUSES_BY_BNET_ACCOUNT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsPlayerViewHousesByBnetAccount(Packet packet)
        {
            packet.ReadPackedGuid128("BnetAccountGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_TELEPORT_TO_PLOT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsTeleportToPlot(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ReadPackedGuid128("OwnerGuid");
            packet.ReadUInt32("PlotIndex");
            packet.ReadByte("TeleportType");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_ACCEPT_NEIGHBORHOOD_OWNERSHIP, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsAcceptNeighborhoodOwnership(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_REJECT_NEIGHBORHOOD_OWNERSHIP, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsRejectNeighborhoodOwnership(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_GET_POTENTIAL_HOUSE_OWNERS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGetPotentialHouseOwners(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_GET_HOUSE_FINDER_NEIGHBORHOOD, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGetHouseFinderNeighborhood(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_GET_BNET_FRIEND_NEIGHBORHOODS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGetBnetFriendNeighborhoods(Packet packet)
        {
            packet.ReadPackedGuid128("BnetAccountGuid");
        }

        [Parser(Opcode.CMSG_HOUSING_SVCS_HOUSE_FINDER_IGNORE_NEIGHBORHOOD, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsHouseFinderIgnoreNeighborhood(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_NOTIFY_PERMISSIONS_FAILURE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsNotifyPermissionsFailure(Packet packet)
        {
            packet.ReadByte("FailureType");
            packet.ReadByte("ErrorCode");
        }

        // TC: PackedGUID + uint8 Flag + uint8 nameLen(size+1) + String
        [Parser(Opcode.SMSG_HOUSING_SVCS_GUILD_CREATE_NEIGHBORHOOD_NOTIFICATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGuildCreateNeighborhoodNotification(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ReadByte("Flag");
            var nameLen = packet.ReadByte("NameLength");
            packet.ReadWoWString("Name", nameLen);
        }

        // TC: JamCliHouseFinderNeighborhood base + uint8 TrailingResult
        [Parser(Opcode.SMSG_HOUSING_SVCS_CREATE_CHARTER_NEIGHBORHOOD_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsCreateCharterNeighborhoodResponse(Packet packet)
        {
            ReadHouseFinderNeighborhoodBase(packet);
            packet.ReadByte("TrailingResult");
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_NEIGHBORHOOD_RESERVE_PLOT_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsNeighborhoodReservePlotResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_RELINQUISH_HOUSE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsRelinquishHouseResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_CANCEL_RELINQUISH_HOUSE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsCancelRelinquishHouseResponse(Packet packet)
        {
            packet.ReadUInt32("Field1");
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadByte("Result");
        }

        // TC: uint32 count + uint8 Result + JamCliHouse[count]
        [Parser(Opcode.SMSG_HOUSING_SVCS_GET_PLAYER_HOUSES_INFO_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGetPlayerHousesInfoResponse(Packet packet)
        {
            var count = packet.ReadUInt32("HouseCount");
            packet.ReadByte("Result");
            for (uint i = 0; i < count; i++)
                ReadJamCliHouse(packet, i);
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_PLAYER_VIEW_HOUSES_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsPlayerViewHousesResponse(Packet packet)
        {
            var count = packet.ReadUInt32("HouseCount");
            packet.ReadByte("Result");
            for (uint i = 0; i < count; i++)
                ReadJamCliHouse(packet, i);
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_CHANGE_HOUSE_COSMETIC_OWNER, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsChangeHouseCosmeticOwner(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadPackedGuid128("NewOwnerGuid");
        }

        // TC: uint8 Result + uint32 ChangeAmount + uint32 Reason + uint32 count
        //     + count x { 3xPackedGUID, int32 HouseLevel, int32 FavorValue, uint8 UpdateSource, uint32 SourceDataDecorID, uint8(bit7 IsAdditive) }
        [Parser(Opcode.SMSG_HOUSING_SVCS_UPDATE_HOUSES_LEVEL_FAVOR, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsUpdateHousesLevelFavor(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadUInt32("ChangeAmount");
            packet.ReadUInt32("Reason");
            var count = packet.ReadUInt32("HouseCount");
            for (uint i = 0; i < count; i++)
            {
                packet.ReadPackedGuid128("BnetAccountGuid", i);
                packet.ReadPackedGuid128("NeighborhoodGuid", i);
                packet.ReadPackedGuid128("HouseGuid", i);
                packet.ReadInt32("HouseLevel", i);
                packet.ReadInt32("FavorValue", i);
                packet.ReadByte("UpdateSource", i);
                packet.ReadUInt32("SourceDataDecorID", i);
                var flags = packet.ReadByte("Flags", i);
                packet.AddValue("IsAdditive", (flags & 0x80) != 0, i);
            }
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_GUILD_ADD_HOUSE_NOTIFICATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGuildAddHouseNotification(Packet packet)
        {
            ReadJamCliHouse(packet);
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_GUILD_REMOVE_HOUSE_NOTIFICATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGuildRemoveHouseNotification(Packet packet)
        {
            ReadJamCliHouse(packet);
        }

        // TC: uint8 nameLen(size+1) + String — no GUID
        [Parser(Opcode.SMSG_HOUSING_SVCS_GUILD_RENAME_NEIGHBORHOOD_NOTIFICATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGuildRenameNeighborhoodNotification(Packet packet)
        {
            var nameLen = packet.ReadByte("NameLength");
            packet.ReadWoWString("NewName", nameLen);
        }

        // TC: uint32 count + JamCliHouseFinderNeighborhood base[count]
        [Parser(Opcode.SMSG_HOUSING_SVCS_GUILD_GET_HOUSING_INFO_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGuildGetHousingInfoResponse(Packet packet)
        {
            var count = packet.ReadUInt32("NeighborhoodCount");
            for (uint i = 0; i < count; i++)
                ReadHouseFinderNeighborhoodBase(packet, i);
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_ACCEPT_NEIGHBORHOOD_OWNERSHIP_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsAcceptNeighborhoodOwnershipResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_REJECT_NEIGHBORHOOD_OWNERSHIP_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsRejectNeighborhoodOwnershipResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        // TC: uint8 header (top 6 bits = blobSize, low 2 = Result); if Result==0:
        //     3x 16-byte raw GUIDs + uint8 HouseLevel
        [Parser(Opcode.SMSG_HOUSING_SVCS_NEIGHBORHOOD_OWNERSHIP_TRANSFERRED_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsNeighborhoodOwnershipTransferredResponse(Packet packet)
        {
            var header = packet.ReadByte("Header");
            packet.AddValue("BlobSize", header >> 2);
            var result = header & 0x03;
            packet.AddValue("Result", result);
            if (result == 0)
            {
                packet.ReadBytes("OwnerGuidRaw", 16);
                packet.ReadBytes("HouseGuidRaw", 16);
                packet.ReadBytes("AccountGuidRaw", 16);
                packet.ReadByte("HouseLevel");
            }
        }

        // TC sniff-verified: uint32 count + per entry { PackedGUID, uint32 ClassID,
        //   uint8 Error, uint8 lenByte1(=nameLen>>1), uint8 lenByte2(=(nameLen&1)<<7), char[nameLen] }
        [Parser(Opcode.SMSG_HOUSING_SVCS_GET_POTENTIAL_HOUSE_OWNERS_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGetPotentialHouseOwnersResponse(Packet packet)
        {
            var count = packet.ReadUInt32("OwnerCount");
            for (uint i = 0; i < count; i++)
            {
                packet.ReadPackedGuid128("PlayerGuid", i);
                packet.ReadUInt32("ClassID", i);
                packet.ReadByte("Error", i);
                var lenByte1 = packet.ReadByte("NameLen1", i);
                var lenByte2 = packet.ReadByte("NameLen2", i);
                var nameLen = (lenByte1 << 1) | (lenByte2 >> 7);
                if (nameLen > 0)
                    packet.ReadWoWString("CharacterName", nameLen, i);
            }
        }

        // TC sniff-validated: uint8 Result + 3xPackedGUID + uint8 HouseLevel + uint8 PlotIndex + uint32 SettingsFlags
        [Parser(Opcode.SMSG_HOUSING_SVCS_UPDATE_HOUSE_SETTINGS_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsUpdateHouseSettingsResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadPackedGuid128("OwnerGuid");
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ReadByte("HouseLevel");
            packet.ReadByte("PlotIndex");
            packet.ReadUInt32("SettingsFlags");
        }

        // IDA case 5767196: uint8 Result (ReadByte, NOT bit) + int32 count
        //   + count x Housing_ReadNeighborhoodResponsePayload (full)
        [Parser(Opcode.SMSG_HOUSING_SVCS_GET_HOUSE_FINDER_INFO_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGetHouseFinderInfoResponse(Packet packet)
        {
            packet.ReadByte("Result");
            var count = packet.ReadUInt32("EntryCount");
            for (uint i = 0; i < count; i++)
                ReadHouseFinderNeighborhood(packet, i);
        }

        // IDA case 5767197: uint8 Result (ReadByte, NOT bit)
        //   + Housing_ReadNeighborhoodResponsePayload (full neighborhood)
        [Parser(Opcode.SMSG_HOUSING_SVCS_GET_HOUSE_FINDER_NEIGHBORHOOD_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGetHouseFinderNeighborhoodResponse(Packet packet)
        {
            packet.ReadByte("Result");
            ReadHouseFinderNeighborhood(packet);
        }

        // TC: uint8 Result + uint32 count + JamCliHouseFinderNeighborhood full[count]
        [Parser(Opcode.SMSG_HOUSING_SVCS_GET_BNET_FRIEND_NEIGHBORHOODS_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsGetBnetFriendNeighborhoodsResponse(Packet packet)
        {
            packet.ReadByte("Result");
            var count = packet.ReadUInt32("EntryCount");
            for (uint i = 0; i < count; i++)
                ReadHouseFinderNeighborhood(packet, i);
        }

        [Parser(Opcode.SMSG_HOUSING_SVCS_DELETE_ALL_NEIGHBORHOOD_INVITES_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsDeleteAllNeighborhoodInvitesResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        // TC: WriteBit(Success) + FlushBits + PackedGUID
        [Parser(Opcode.SMSG_HOUSING_SVCS_IGNORE_NEIGHBORHOOD_INVITE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsIgnoreNeighborhoodInviteResponse(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("Success");
            packet.ResetBitReader();
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        // TC: PackedGUID + SizedString<8> name
        [Parser(Opcode.SMSG_HOUSING_SVCS_NEIGHBORHOOD_UPDATE_NAME_NOTIFICATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingSvcsNeighborhoodUpdateNameNotification(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 8);
            packet.ResetBitReader();
            packet.ReadWoWString("NewName", (int)nameLen);
        }

        // ============================================================
        // Core house state (0x37 / 0x59)
        // ============================================================

        // TC: OptionalInit(HouseGuid) + optional PackedGUID — replaces the
        // provisional zero-length handler
        [Parser(Opcode.CMSG_HOUSING_GET_PLAYER_PERMISSIONS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingGetPlayerPermissions(Packet packet)
        {
            packet.ResetBitReader();
            var hasHouseGuid = packet.ReadBit("HasHouseGuid");
            packet.ResetBitReader();
            if (hasHouseGuid)
                packet.ReadPackedGuid128("HouseGuid");
        }

        // TC: uint8 ResetScope (0=None, 1=Interior, 2=Exterior)
        [Parser(Opcode.CMSG_HOUSING_RESET_HOUSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingResetHouse(Packet packet)
        {
            packet.ReadByte("ResetScope");
        }

        // TC: 4xPackedGUID + uint8 Status + uint8 PermissionFlags
        //     (bit7=houseEditing, bit6=plotEntry, bit5=houseEntry)
        [Parser(Opcode.SMSG_HOUSING_HOUSE_STATUS_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingHouseStatusResponse(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadPackedGuid128("AccountGuid");
            packet.ReadPackedGuid128("OwnerPlayerGuid");
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ReadByte("Status");
            var permFlags = packet.ReadByte("PermissionFlags");
            packet.AddValue("CanEditHouse", (permFlags & 0x80) != 0);
            packet.AddValue("CanEnterPlot", (permFlags & 0x40) != 0);
            packet.AddValue("CanEnterHouse", (permFlags & 0x20) != 0);
        }

        // TC: JamCliHouse + uint8 Result
        [Parser(Opcode.SMSG_HOUSING_GET_CURRENT_HOUSE_INFO_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingGetCurrentHouseInfoResponse(Packet packet)
        {
            ReadJamCliHouse(packet);
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_GET_PLAYER_PERMISSIONS_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingGetPlayerPermissionsResponse(Packet packet)
        {
            packet.ReadPackedGuid128("HouseGuid");
            packet.ReadByte("ResultCode");
            packet.ReadByte("PermissionFlags");
        }

        [Parser(Opcode.SMSG_HOUSING_RESET_KIOSK_MODE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingResetKioskModeResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_HOUSING_RESET_HOUSE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingResetHouseResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
        }

        // ============================================================
        // Neighborhood charter (0x39 / 0x5F)
        // ============================================================

        // TC NeighborhoodCharterCreate/Edit: uint32 MapID + uint32 FactionFlags + SizedCString<8>
        [Parser(Opcode.CMSG_NEIGHBORHOOD_CHARTER_CREATE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_NEIGHBORHOOD_CHARTER_EDIT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCharterCreateOrEdit(Packet packet)
        {
            packet.ReadUInt32("NeighborhoodMapID");
            packet.ReadUInt32("FactionFlags");
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 8);
            packet.ResetBitReader();
            packet.ReadWoWString("Name", (int)nameLen);
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_CHARTER_ADD_SIGNATURE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCharterAddSignature(Packet packet)
        {
            packet.ReadPackedGuid128("CharterGuid");
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_CHARTER_SEND_SIGNATURE_REQUEST, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCharterSendSignatureRequest(Packet packet)
        {
            packet.ReadPackedGuid128("TargetPlayerGuid");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_CHARTER_UPDATE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.SMSG_NEIGHBORHOOD_CHARTER_OPEN_UI_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCharterResponse(Packet packet)
        {
            ReadCharterResponse(packet);
        }

        // TC: uint8 Result + PackedGUID + uint32 MapID + uint32 Unknown + uint8 nameLen + CString
        [Parser(Opcode.SMSG_NEIGHBORHOOD_CHARTER_SIGN_REQUEST, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCharterSignRequest(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("CharterGuid");
            packet.ReadUInt32("MapID");
            packet.ReadUInt32("Unknown");
            var nameLen = packet.ReadByte("NameLength");
            packet.ReadWoWString("NeighborhoodName", nameLen);
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_CHARTER_ADD_SIGNATURE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCharterAddSignatureResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("CharterGuid");
        }

        // TC: uint8 Result + uint32 + uint32 + uint8 nameLen + CString
        [Parser(Opcode.SMSG_NEIGHBORHOOD_CHARTER_OPEN_CONFIRMATION_UI_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCharterOpenConfirmationUIResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadUInt32("Field1");
            packet.ReadUInt32("Field2");
            var nameLen = packet.ReadByte("NameLength");
            packet.ReadWoWString("NeighborhoodName", nameLen);
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_CHARTER_SIGNATURE_REMOVED_NOTIFICATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCharterSignatureRemovedNotification(Packet packet)
        {
            packet.ReadPackedGuid128("CharterGuid");
        }

        // ============================================================
        // Neighborhood management (0x3A/0x3B/0x3E/0x3F / 0x60/0x64)
        // ============================================================

        [Parser(Opcode.CMSG_NEIGHBORHOOD_UPDATE_NAME, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodUpdateName(Packet packet)
        {
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("NameLength", 8);
            packet.ResetBitReader();
            packet.ReadWoWString("NewName", (int)nameLen);
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_SET_PUBLIC_FLAG, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodSetPublicFlag(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ResetBitReader();
            packet.ReadBit("IsPublic");
            packet.ResetBitReader();
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_ADD_SECONDARY_OWNER, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodAddSecondaryOwner(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_REMOVE_SECONDARY_OWNER, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodRemoveSecondaryOwner(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_INVITE_RESIDENT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodInviteResident(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_CANCEL_INVITATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCancelInvitation(Packet packet)
        {
            packet.ReadPackedGuid128("InviteeGuid");
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_PLAYER_DECLINE_INVITE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodPlayerDeclineInvite(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        // TC: PackedGUID Cornerstone + PackedGUID House
        [Parser(Opcode.CMSG_NEIGHBORHOOD_BUY_HOUSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodBuyHouse(Packet packet)
        {
            packet.ReadPackedGuid128("CornerstoneGuid");
            packet.ReadPackedGuid128("HouseGuid");
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_MOVE_HOUSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodMoveHouse(Packet packet)
        {
            packet.ReadPackedGuid128("CornerstoneGuid");
            packet.ReadPackedGuid128("HouseGuid");
        }

        // TC Read order: uint32 PlotIndex + PackedGUID NeighborhoodGuid
        [Parser(Opcode.CMSG_NEIGHBORHOOD_OPEN_CORNERSTONE_UI, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodOpenCornerstoneUI(Packet packet)
        {
            packet.ReadUInt32("PlotIndex");
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_OFFER_OWNERSHIP, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodOfferOwnership(Packet packet)
        {
            packet.ReadPackedGuid128("NewOwnerGuid");
        }

        [Parser(Opcode.CMSG_NEIGHBORHOOD_GET_ROSTER, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodGetRoster(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        // TC Read order: uint32 PlotIndex + PackedGUID NeighborhoodGuid
        [Parser(Opcode.CMSG_NEIGHBORHOOD_EVICT_PLOT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodEvictPlot(Packet packet)
        {
            packet.ReadUInt32("PlotIndex");
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.CMSG_DECLINE_NEIGHBORHOOD_INVITES, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleDeclineNeighborhoodInvites(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("Allow");
            packet.ResetBitReader();
        }

        [Parser(Opcode.CMSG_GET_AVAILABLE_INITIATIVE_REQUEST, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_GET_INITIATIVE_ACTIVITY_LOG_REQUEST, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_INITIATIVE_UPDATE_ACTIVE_NEIGHBORHOOD, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleInitiativeNeighborhoodGuidRequest(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.CMSG_QUERY_NEIGHBORHOOD_INFO, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleQueryNeighborhoodInfo(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        // TC: single SizedString<6> player name (invite by name, not GUID)
        [Parser(Opcode.CMSG_INVITE_PLAYER_TO_NEIGHBORHOOD, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleInvitePlayerToNeighborhood(Packet packet)
        {
            packet.ResetBitReader();
            var nameLen = packet.ReadBits("PlayerNameLength", 6);
            packet.ResetBitReader();
            packet.ReadWoWString("PlayerName", (int)nameLen);
        }

        // TC: UNVERIFIED — client consumes body as opaque bytes
        [Parser(Opcode.SMSG_NEIGHBORHOOD_EVICT_PLAYER, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodEvictPlayer(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_UPDATE_NAME_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodUpdateNameResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_ADD_SECONDARY_OWNER_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodAddSecondaryOwnerResponse(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadByte("Result");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_REMOVE_SECONDARY_OWNER_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodRemoveSecondaryOwnerResponse(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
            packet.ReadByte("Result");
        }

        // TC: JamCliHouse + uint8 Result
        [Parser(Opcode.SMSG_NEIGHBORHOOD_BUY_HOUSE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodBuyHouseResponse(Packet packet)
        {
            ReadJamCliHouse(packet);
            packet.ReadByte("Result");
        }

        // TC: JamCliHouse + PackedGUID MoveTransactionGuid + uint8 Result
        [Parser(Opcode.SMSG_NEIGHBORHOOD_MOVE_HOUSE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodMoveHouseResponse(Packet packet)
        {
            ReadJamCliHouse(packet);
            packet.ReadPackedGuid128("MoveTransactionGuid");
            packet.ReadByte("Result");
        }

        // TC retail-verified complex packet:
        //   uint32 PlotIndex + PackedGUID PlotOwner + PackedGUID Neighborhood + uint64 Cost
        //   + uint8 PurchaseStatus + PackedGUID Cornerstone
        //   then bit-run (15 bits): IsPlotOwned + SizedCString<8> nameLen + OptionalInit(AlternatePrice)
        //     + CanPurchase + HasExistingHouse + HasResidents + OptionalInit(StatusValue) + IsInitiative
        //   then: optional JamCliHouse, name data, optional uint64, optional uint32
        [Parser(Opcode.SMSG_NEIGHBORHOOD_OPEN_CORNERSTONE_UI_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodOpenCornerstoneUIResponse(Packet packet)
        {
            packet.ReadUInt32("PlotIndex");
            packet.ReadPackedGuid128("PlotOwnerGuid");
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ReadUInt64("Cost");
            packet.ReadByte("PurchaseStatus");
            packet.ReadPackedGuid128("CornerstoneGuid");

            packet.ResetBitReader();
            packet.ReadBit("IsPlotOwned");
            var nameLen = packet.ReadBits("NeighborhoodNameLength", 8);
            var hasAlternatePrice = packet.ReadBit("HasAlternatePrice");
            packet.ReadBit("CanPurchase");
            var hasExistingHouse = packet.ReadBit("HasExistingHouse");
            packet.ReadBit("HasResidents");
            var hasStatusValue = packet.ReadBit("HasStatusValue");
            packet.ReadBit("IsInitiative");
            packet.ResetBitReader();

            if (hasExistingHouse)
                ReadJamCliHouse(packet, "ExistingHouse");
            packet.ReadWoWString("NeighborhoodName", (int)nameLen);
            if (hasAlternatePrice)
                packet.ReadUInt64("AlternatePrice");
            if (hasStatusValue)
                packet.ReadUInt32("StatusValue");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_INVITE_RESIDENT_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodInviteResidentResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("InviteeGuid");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_CANCEL_INVITATION_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodCancelInvitationResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("InviteeGuid");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_DECLINE_INVITATION_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodDeclineInvitationResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        // TC: uint8 Result + InviteEntry (48 bytes)
        [Parser(Opcode.SMSG_NEIGHBORHOOD_PLAYER_GET_INVITE_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodPlayerGetInviteResponse(Packet packet)
        {
            packet.ReadByte("Result");
            ReadInviteEntry(packet);
        }

        // TC: uint8 Result + uint32 Count + InviteEntry[Count]
        [Parser(Opcode.SMSG_NEIGHBORHOOD_GET_INVITES_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodGetInvitesResponse(Packet packet)
        {
            packet.ReadByte("Result");
            var count = packet.ReadUInt32("InviteCount");
            for (uint i = 0; i < count; i++)
                ReadInviteEntry(packet, i);
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_INVITE_NOTIFICATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodInviteNotification(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_OFFER_OWNERSHIP_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodOfferOwnershipResponse(Packet packet)
        {
            packet.ReadByte("Result");
        }

        // TC sniff-verified complex roster:
        //   uint8 Result + uint32 MemberCount + uint32 GroupCount
        //   per group: 2xPackedGUID + 2xuint64 + uint32 ResidentCount + uint8 nameLen(size+1)
        //     + uint8 GroupFlags + residents{ 3xPackedGUID + uint8 PlotIndex + uint32 JoinTime
        //     + uint8(bit7=hasOpt u64) } + name data
        //   + uint8 MainFlags(bit7=has trailing guid)
        //   + MemberCount x { PackedGUID + uint8 + uint8(bit7=IsOnline) }
        //   + optional trailing PackedGUID
        [Parser(Opcode.SMSG_NEIGHBORHOOD_GET_ROSTER_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodGetRosterResponse(Packet packet)
        {
            packet.ReadByte("Result");
            var memberCount = packet.ReadUInt32("MemberCount");
            var groupCount = packet.ReadUInt32("GroupCount");
            for (uint g = 0; g < groupCount; g++)
            {
                packet.ReadPackedGuid128("NeighborhoodGuid", g);
                packet.ReadPackedGuid128("OwnerGuid", g);
                packet.ReadUInt64("Field1", g);
                packet.ReadUInt64("Field2", g);
                var residentCount = packet.ReadUInt32("ResidentCount", g);
                var nameLen = packet.ReadByte("NeighborhoodNameLength", g);
                packet.ReadByte("GroupFlags", g);
                for (uint i = 0; i < residentCount; i++)
                {
                    packet.ReadPackedGuid128("HouseGuid", g, i);
                    packet.ReadPackedGuid128("PlayerGuid", g, i);
                    packet.ReadPackedGuid128("BnetAccountGuid", g, i);
                    packet.ReadByte("PlotIndex", g, i);
                    packet.ReadUInt32("JoinTime", g, i);
                    var entryFlags = packet.ReadByte("EntryFlags", g, i);
                    if ((entryFlags & 0x80) != 0)
                        packet.ReadUInt64("OptionalValue", g, i);
                }
                if (nameLen > 1)
                    packet.ReadWoWString("NeighborhoodName", nameLen, g);
            }

            var mainFlags = packet.ReadByte("MainFlags");
            for (uint i = 0; i < memberCount; i++)
            {
                packet.ReadPackedGuid128("PlayerGuid", i);
                packet.ReadByte("StatusField1", i);
                var status2 = packet.ReadByte("StatusField2", i);
                packet.AddValue("IsOnline", (status2 & 0x80) != 0, i);
            }
            if ((mainFlags & 0x80) != 0)
                packet.ReadPackedGuid128("TrailingGuid");
        }

        // TC: uint32 count + per entry { PackedGUID + uint8 UpdateType + uint8(bit7=IsPrivileged) }
        [Parser(Opcode.SMSG_NEIGHBORHOOD_ROSTER_RESIDENT_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodRosterResidentUpdate(Packet packet)
        {
            var count = packet.ReadUInt32("ResidentCount");
            for (uint i = 0; i < count; i++)
            {
                packet.ReadPackedGuid128("PlayerGuid", i);
                packet.ReadByte("UpdateType", i);
                var flags = packet.ReadByte("Flags", i);
                packet.AddValue("IsPrivileged", (flags & 0x80) != 0, i);
            }
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_INVITE_NAME_LOOKUP_RESULT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodInviteNameLookupResult(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("PlayerGuid");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_EVICT_PLOT_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodEvictPlotResponse(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.SMSG_NEIGHBORHOOD_EVICT_PLOT_NOTICE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleNeighborhoodEvictPlotNotice(Packet packet)
        {
            packet.ReadUInt32("PlotId");
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ReadPackedGuid128("PlotGuid");
        }

        [Parser(Opcode.SMSG_INVALIDATE_NEIGHBORHOOD, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleInvalidateNeighborhood(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        // ============================================================
        // Misc / decor licensing / initiative (0x2A/0x2E/0x43 / 0x45/0x49/0x51)
        // ============================================================

        [Parser(Opcode.CMSG_GUILD_GET_OTHERS_OWNED_HOUSES, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleGuildGetOthersOwnedHouses(Packet packet)
        {
            packet.ReadPackedGuid128("PlayerGuid");
        }

        // TC BulkRefund: uint32 count + PackedGUID[count]
        [Parser(Opcode.CMSG_BULK_REFUND, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleBulkRefund(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (uint i = 0; i < count; i++)
                packet.ReadPackedGuid128("DecorGuid", i);
        }

        [Parser(Opcode.SMSG_BULK_REFUND_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleBulkRefundResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
        }

        // TC LicensedDecorQuantitiesUpdate: uint32 count + { uint32, uint32, uint32 }[]
        [Parser(Opcode.SMSG_LICENSED_DECOR_QUANTITIES_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleLicensedDecorQuantitiesUpdate(Packet packet)
        {
            var count = packet.ReadUInt32("QuantityCount");
            for (uint i = 0; i < count; i++)
            {
                packet.ReadUInt32("HouseDecorID", i);
                packet.ReadUInt32("PlacedQuantity", i);
                packet.ReadUInt32("StoredQuantity", i);
            }
        }

        // TC: PackedGUID + uint8 Result(bit7) + optional SizedString<8> name
        [Parser(Opcode.SMSG_QUERY_NEIGHBORHOOD_NAME_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleQueryNeighborhoodNameResponse(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
            packet.ResetBitReader();
            var result = packet.ReadBit("Result");
            packet.ResetBitReader();
            if (result)
            {
                var nameLen = packet.ReadBits("NameLength", 8);
                packet.ResetBitReader();
                packet.ReadWoWString("NeighborhoodName", (int)nameLen);
            }
        }

        [Parser(Opcode.SMSG_INVALIDATE_NEIGHBORHOOD_NAME, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleInvalidateNeighborhoodName(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
        }

        [Parser(Opcode.SMSG_ACCOUNT_EXTERIOR_FIXTURE_COLLECTION_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.SMSG_ACCOUNT_HOUSE_TYPE_COLLECTION_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.SMSG_ACCOUNT_ROOM_COLLECTION_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.SMSG_ACCOUNT_ROOM_THEME_COLLECTION_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.SMSG_ACCOUNT_ROOM_MATERIAL_COLLECTION_UPDATE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleAccountCollectionUpdate(Packet packet)
        {
            ReadAccountCollectionUpdate(packet);
        }

        // TC: uint8 Result + PackedGUID GuildGuid + uint32 count + JamCliHouse[count]
        [Parser(Opcode.SMSG_GUILD_OTHERS_OWNED_HOUSES_RESULT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleGuildOthersOwnedHousesResult(Packet packet)
        {
            packet.ReadByte("Result");
            packet.ReadPackedGuid128("GuildGuid");
            var count = packet.ReadUInt32("HouseCount");
            for (uint i = 0; i < count; i++)
                ReadJamCliHouse(packet, i);
        }

        // IDA sub_7FF7CD4832C0: SizedString — Bits<6> length (buffer max 41) + bit + raw string bytes.
        // 1-byte sniff (00) = len 0 + flag 0.
        [Parser(Opcode.CMSG_HOUSING_PHOTO_SHARING_COMPLETE_AUTHORIZATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingPhotoSharingCompleteAuthorization(Packet packet)
        {
            var len = packet.ReadBits("TextLength", 6);
            packet.ReadBit("Flag");
            packet.ResetBitReader();
            packet.ReadWoWString("Text", (int)len);
        }

        // TC: PackedGUID + uint8 flags(bit7=Field0, bit6=OpenForBusiness)
        [Parser(Opcode.SMSG_CRAFTING_HOUSE_HELLO_RESPONSE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleCraftingHouseHelloResponse(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            var flags = packet.ReadByte("Flags");
            packet.AddValue("Field0", (flags & 0x80) != 0);
            packet.AddValue("OpenForBusiness", (flags & 0x40) != 0);
        }

        // TC: WriteBit + FlushBits
        [Parser(Opcode.SMSG_INITIATIVE_SERVICE_STATUS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleInitiativeServiceStatus(Packet packet)
        {
            packet.ResetBitReader();
            packet.ReadBit("ServiceEnabled");
            packet.ResetBitReader();
        }

        // TC: PackedGUID + uint8 Flags; data block only when (Flags >> 6) == 1:
        //   int64 + 3x int32 + 3x float + uint32 taskCount + tasks{uint32, uint32}
        [Parser(Opcode.SMSG_GET_PLAYER_INITIATIVE_INFO_RESULT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleGetPlayerInitiativeInfoResult(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
            var flags = packet.ReadByte("Flags");
            packet.AddValue("HasData", (flags >> 6) == 1);
            if ((flags >> 6) == 1)
            {
                packet.ReadInt64("RemainingDuration");
                packet.ReadInt32("CurrentInitiativeID");
                packet.ReadInt32("CurrentMilestoneID");
                packet.ReadInt32("CurrentCycleID");
                packet.ReadSingle("ProgressRequired");
                packet.ReadSingle("CurrentProgress");
                packet.ReadSingle("PlayerTotalContribution");
                var taskCount = packet.ReadUInt32("TaskCount");
                for (uint i = 0; i < taskCount; i++)
                {
                    packet.ReadUInt32("TaskID", i);
                    packet.ReadUInt32("Progress", i);
                }
            }
        }

        // TC: PackedGUID + uint32 count + { 2xPackedGUID + uint32 Contribution + uint64 Time + uint32 TaskID }[]
        [Parser(Opcode.SMSG_GET_INITIATIVE_ACTIVITY_LOG_RESULT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleGetInitiativeActivityLogResult(Packet packet)
        {
            packet.ReadPackedGuid128("NeighborhoodGuid");
            var count = packet.ReadUInt32("CompletedTaskCount");
            for (uint i = 0; i < count; i++)
            {
                packet.ReadPackedGuid128("PlayerGuid", i);
                packet.ReadPackedGuid128("TargetGuid", i);
                packet.ReadUInt32("ContributionAmount", i);
                packet.ReadUInt64("CompletionTime", i);
                packet.ReadUInt32("TaskID", i);
            }
        }

        [Parser(Opcode.SMSG_INITIATIVE_TASK_COMPLETE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleInitiativeTaskComplete(Packet packet)
        {
            packet.ReadUInt32("InitiativeID");
            packet.ReadUInt32("TaskID");
        }

        [Parser(Opcode.SMSG_INITIATIVE_COMPLETE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleInitiativeComplete(Packet packet)
        {
            packet.ReadUInt32("InitiativeID");
        }

        [Parser(Opcode.SMSG_CLEAR_INITIATIVE_TASK_CRITERIA_PROGRESS, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleClearInitiativeTaskCriteriaProgress(Packet packet)
        {
            var count = packet.ReadUInt32("CriteriaCount");
            for (uint i = 0; i < count; i++)
                packet.ReadUInt64("CriteriaID", i);
        }

        [Parser(Opcode.SMSG_GET_INITIATIVE_REWARDS_RESULT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleGetInitiativeRewardsResult(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadPackedGuid128("SourceGuid");
            packet.ReadPackedGuid128("TargetGuid");
        }

        // TC: uint32 count + PackedGUID[count]
        [Parser(Opcode.SMSG_INITIATIVE_REWARD_AVAILABLE, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleInitiativeRewardAvailable(Packet packet)
        {
            var count = packet.ReadUInt32("RewardGuidCount");
            for (uint i = 0; i < count; i++)
                packet.ReadPackedGuid128("RewardGuid", i);
        }

        // TC: uint8 Result + uint8(length << 1) + char[length]
        [Parser(Opcode.SMSG_HOUSING_PHOTO_SHARING_AUTHORIZATION_RESULT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingPhotoSharingAuthorizationResult(Packet packet)
        {
            packet.ReadByte("Result");
            var lenByte = packet.ReadByte("PartnerNameLengthRaw");
            var len = lenByte >> 1;
            if (len > 0)
                packet.ReadWoWString("PartnerName", len);
        }

        [Parser(Opcode.SMSG_HOUSING_PHOTO_SHARING_AUTHORIZATION_CLEARED_RESULT, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingPhotoSharingAuthorizationClearedResult(Packet packet)
        {
            packet.ReadByte("Result");
        }

        // ============================================================
        // Empty CMSGs (TC Read() bodies are empty / no payload on wire)
        // ============================================================

        [Parser(Opcode.CMSG_HOUSING_HOUSE_STATUS, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSING_GET_CURRENT_HOUSE_INFO, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSING_BLUEPRINT_REQUEST_COLLECTION, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSE_INTERIOR_LEAVE_HOUSE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSING_SVCS_GET_PLAYER_HOUSES_INFO, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSING_SVCS_START_TUTORIAL, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSING_SVCS_GET_HOUSE_FINDER_INFO, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSING_SVCS_DELETE_ALL_NEIGHBORHOOD_INVITES, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSING_RESET_KIOSK_MODE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_NEIGHBORHOOD_CHARTER_OPEN_CONFIRMATION_UI, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_NEIGHBORHOOD_CHARTER_FINALIZE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_NEIGHBORHOOD_INITIATIVE_SERVICE_STATUS_CHECK, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_NEIGHBORHOOD_PLAYER_GET_INVITE, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_NEIGHBORHOOD_GET_INVITES, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_GET_DECOR_REFUND_LIST, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_GET_ALL_LICENSED_DECOR_QUANTITIES, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.CMSG_HOUSING_PHOTO_SHARING_CLEAR_AUTHORIZATION, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingZeroLength(Packet packet)
        {
        }

        // Empty SMSGs
        [Parser(Opcode.SMSG_HOUSING_SVCS_HOUSE_FINDER_FORCE_REFRESH, ClientVersionBuild.V12_1_0_69214)]
        [Parser(Opcode.SMSG_HOUSING_SVC_REQUEST_PLAYER_RELOAD_DATA, ClientVersionBuild.V12_1_0_69214)]
        public static void HandleHousingZeroLengthSmsg(Packet packet)
        {
        }
    }
}
