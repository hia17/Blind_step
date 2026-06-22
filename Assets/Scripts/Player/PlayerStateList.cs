using UnityEngine;

public class PlayerStateList : MonoBehaviour
{
    public static bool firstKeyFounded = false;
    public static bool secondKeyFounded = false;

    public static int damagedCount = 0;
    public static bool isHurt = false;

    public static bool indigest = false;
    public static float originSpeed;
    public static bool isDamaged = false;

    public static float rayUpgrade = 0;
    public static float raySpeedUpgrade = 0;
    public static float healthUpgrade = 0;


    public static void LoadUpgradeData()
    {
        healthUpgrade = PlayerPrefs.GetInt("StaminaLevel", 0);
        rayUpgrade = PlayerPrefs.GetInt("WaveCountLevel", 0);
        raySpeedUpgrade = PlayerPrefs.GetInt("WaveSpeedLevel", 0);
    }

    private void Awake()
    {
        LoadUpgradeData();
    }
}
