public enum ServerToClientCalls : ushort
{
    ACK_GROUPS_OBJECTS,
    SET_WAITING_ROOM_STATE,
    WAITINGROOM_TEAMNAME,
    WAITINGROOM_STATUS,
    WAITINGROOM_IMAGE
}

public enum ClientToServerCalls : ushort
{
    CHANGE_SCENE,
    ADD_GROUPS,
    ADD_OBJECTS,
    GROUPS_OBJECTS_LENGTH,
    AJOUTER_GROUPE_EFFETS,
    AJOUTER_EFFET_LUMINEUX,
    SONG_SELECT_CHANGE_BG,
    SYNCHRONISE_BREATHING,
    WAITINGROOM_STATE,
    WAITINGROOM_TEAMNAME,
    WAITINGROOM_STATUS,
    WAITINGROOM_IMAGE,
    PLAY_INTRO
}