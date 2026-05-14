using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public PlayerController player;

    [Header("Health Bars")]
    public Image hullBar;
    public Image shieldBar;

    [Header("Power Bars")]
    public Image weaponPowerBar;
    public Image enginePowerBar;
    public Image shieldPowerBar;

    void Update()
    {
        if (player == null) return;

        // Update Health/Shields (0 to 100 scaled down to 0.0 to 1.0 for UI)
        hullBar.fillAmount = player.hull / 100f;
        shieldBar.fillAmount = player.shield / 100f;

        // Update Power Bars (0 to 100 scaled down to 0.0 to 1.0 for UI)
        weaponPowerBar.fillAmount = player.powerWeapons / 100f;
        enginePowerBar.fillAmount = player.powerEngines / 100f;
        shieldPowerBar.fillAmount = player.powerShields / 100f;
    }
}