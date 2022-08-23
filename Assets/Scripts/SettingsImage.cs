using UnityEngine;

/// <summary>
/// The image for the settings menu
/// </summary>
public class SettingsImage : MonoBehaviour
{
    // Get settings panel
    public GameObject settingsPanel;

    // Set the panel active on click
    private void OnMouseDown()
    {
        settingsPanel.SetActive(true);
    }
}
