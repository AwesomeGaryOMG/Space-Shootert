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

        // Update Health/Shields (Only if the UI element is attached in the Inspector!)
        if (hullBar != null) hullBar.fillAmount = player.hull / 100f;
        if (shieldBar != null) shieldBar.fillAmount = player.shield / 100f;

        // Update Power Bars (Only if the UI element is attached in the Inspector!)
        if (weaponPowerBar != null) weaponPowerBar.fillAmount = player.powerWeapons / 100f;
        if (enginePowerBar != null) enginePowerBar.fillAmount = player.powerEngines / 100f;
        if (shieldPowerBar != null) shieldPowerBar.fillAmount = player.powerShields / 100f;
    }
}