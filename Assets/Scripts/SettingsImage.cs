using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
