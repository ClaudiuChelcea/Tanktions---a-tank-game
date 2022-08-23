using UnityEngine;

/// <summary>
/// Contains the scenes' names as public static variables.
/// Also contains an array of all the arenas.
/// </summary>
public class ScenesNames : MonoBehaviour
{
    public static string MainScreen = "MainScreen";
    public static string MultiplayerMenu = "MultiplayerMenu";
    public static string LoadingScreen = "LoadingScreen";
    public static string ArenaIndustrial = "ArenaIndustrial";

    public static string[] Arenas = new string[] {
        ArenaIndustrial
    };

    public static string SelectRandomArena()
    {
        return Arenas[UnityEngine.Random.Range(0, Arenas.Length)];
    }
}