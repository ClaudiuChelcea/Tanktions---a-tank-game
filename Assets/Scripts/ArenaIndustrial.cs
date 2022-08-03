using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaIndustrial : MonoBehaviour
{
        // Get audio
        public AudioSource audioSource;

	private void Update()
	{
		// Keep audio level up to date
		audioSource.volume = SettingsMenu.levelMusicVolume;
	}

	// On scene load, play music
	private void Awake()
	{
		if(SettingsMenu.levelMusicVolume > 0f)
			audioSource.Play();
	}
}
