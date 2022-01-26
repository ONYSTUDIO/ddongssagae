using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class CConstants
{
    /* Studio-N 에서 가져온 것

    public static bool PETROOM_DEBUG = true;

    public const int CODE_VERSION = 1;

    // file protocol
#if UNITY_EDITOR_WIN
    public const string FILE_PROTOCOL = "file:///";
#else
    public const string FILE_PROTOCOL = "file://";
#endif

    // screen
    public const float BASEPOSITION = 356f;
    public const float STANDARD_WIDTH = 1920f;
    public const float STANDARD_HEIGHT = 1080f;
    public const float STANDARD_RATIO = STANDARD_HEIGHT / STANDARD_WIDTH;

    public static string ON_COLLECTION_LEVEL_CHANGED = "ON_COLLECTION_LEVEL_CHANGED";
    public static string ON_COLLECTION_PUZZLE_SELECT_END = "ON_COLLECTION_GAME_START";

    public const int USER_OFFLINE = 0;
    public const int USER_ONLINE = 1;

    */

    public static int PLAYER_TYPE;
    public static string PLAYER_SPRITE_PATH;

    static CConstants()
    {
        // if (PLAYER_TYPE != null)
        // {
        //     switch (PLAYER_TYPE)
        //     {
        //         case "JJAJANG_SPRITE_PATH" :
        //             PLAYER_SPRITE_PATH = "Resources/Icon/player_jjajang.png";
        //             break;
        //         case "HODU_SPRITE_PATH" :
        //             PLAYER_SPRITE_PATH = "Resources/Icon/player_hodu.png";
        //             break;
        //         case "JJANGA_SPRITE_PATH" :
        //             PLAYER_SPRITE_PATH = "Resources/Icon/player_jjanga.png";
        //             break;
        //         case "DUBU_SPRITE_PATH" :
        //             PLAYER_SPRITE_PATH = "Resources/Icon/player_dubu.png";
        //             break;
        //     }
        // }
    }

    public static string GetResourcesPath(int PLAYER_TYPE)
    {
        //if (PLAYER_TYPE != null)
        //{
            switch (PLAYER_TYPE)
            {
                case 0 :
                    PLAYER_SPRITE_PATH = "Icon/player_jjajang";
                    break;
                case 1 :
                    PLAYER_SPRITE_PATH = "Icon/player_hodu";
                    break;
                case 2 :
                    PLAYER_SPRITE_PATH = "Icon/player_jjanga";
                    break;
                case 3 :
                    PLAYER_SPRITE_PATH = "Icon/player_dubu";
                    break;
            }
        //}

        return PLAYER_SPRITE_PATH;
    }

    // public static string GetResourcesPath(string PLAYER_TYPE)
    // {
    //     if (PLAYER_TYPE != null)
    //     {
    //         switch (PLAYER_TYPE)
    //         {
    //             case "JJAJANG_SPRITE_PATH" :
    //                 PLAYER_SPRITE_PATH = "Resources/Icon/player_jjajang.png";
    //                 break;
    //             case "HODU_SPRITE_PATH" :
    //                 PLAYER_SPRITE_PATH = "Resources/Icon/player_hodu.png";
    //                 break;
    //             case "JJANGA_SPRITE_PATH" :
    //                 PLAYER_SPRITE_PATH = "Resources/Icon/player_jjanga.png";
    //                 break;
    //             case "DUBU_SPRITE_PATH" :
    //                 PLAYER_SPRITE_PATH = "Resources/Icon/player_dubu.png";
    //                 break;
    //         }
    //     }

    //     return PLAYER_SPRITE_PATH;
    // }
    public static string JJAJANG_SPRITE_PATH = "Resources/Icon/player_jjajang.png";
    public static string  HODU_SPRITE_PATH = "Resources/Icon/player_hodu.png";
    public static string JJANGA_SPRITE_PATH = "Resources/Icon/player_jjanga.png";
    public static string DUBU_SPRITE_PATH = "Resources/Icon/player_dubu.png";

}
