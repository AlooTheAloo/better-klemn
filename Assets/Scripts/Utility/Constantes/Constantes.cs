using UnityEngine;

public sealed class Constantes
{
    public static KeyCode[,] TOUCHES = new KeyCode[,]
    {
        { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, },
        { KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, },
    };
    public static int NOMBRE_JOUEURS = 2;
    public static int NOMBRE_TOUCHES = 4;
    public static int NOMBRE_TOURS = 2;
    public static int SECONDES_DANS_MINUTE = 60;
    public static ulong[] POINTS_PRECISION = { 100, 75, 50, 0 };
    public static ushort MAX_MS_TOLERANCE = 120;
    public static ushort MIN_MS_TOLERANCE = 25;
    public static ushort MAX_OD = 10;
    public static ushort MIN_OD = 0;
    public static ushort MAX_AR = 10;
    public static ushort MIN_AR = 0;
    public static ushort DEUXIEME_PLATEAU_TOLERANCE = 2;
    public static ushort DERNIER_PLATEAU_TOLERANCE = 3;
    public static ushort MS_DANS_SECONDE = 1000;
    public static int AR_MULTIPLICATEUR = 2;
    public static float COULEUR_TRAIL_MULTIPLICATEUR = 0.8f;
    public static float COULEUR_NOTE_MULTIPLICATEUR = 1.3f;
    public static float MULTIPLICATEUR_COMBO = 0.12f;
    public static string FICHIER_BUILD_SETTINGS = "/BuildSettings.json";
    public static string API_BASE_URL = "";
    public static string LIGHT_API_BASE_URL = "";
}
