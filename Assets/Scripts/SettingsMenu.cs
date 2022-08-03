using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	// Get sliders values and make them public
	public Slider menuMusicSlider;
	public Slider levelMusicSlider;

	// Default initial volume
	static public float startingSoundVolume = 0.15f;

	// Make their values public
	static public float menuMusicVolume = startingSoundVolume;
	static public float levelMusicVolume = startingSoundVolume;

	// Keep their values up to date
	private void Update()
	{
		menuMusicVolume = menuMusicSlider.value;
		levelMusicVolume = levelMusicSlider.value;
	}

	// On start. set music level to a default value
	private void Start()
	{
		menuMusicVolume = startingSoundVolume;
		levelMusicVolume = startingSoundVolume;
		menuMusicSlider.value = startingSoundVolume;
		levelMusicSlider.value = startingSoundVolume;
	}
}
