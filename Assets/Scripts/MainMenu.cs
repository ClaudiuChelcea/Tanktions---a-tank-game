using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
        // Get audio
        public AudioSource audioSource;

        // Play button presset
        static public bool play_button_pressed = false;

        // Arena Selected
        static public int arenaSelected = (int) SceneINDEX.ArenaIndustrial;

        // Scenes
        public enum SceneINDEX {
                MainMenuScene,
                LoadingScreen,
                ArenaIndustrial
	};

        // Set play button pressed
        public void setPlayButtonPressed()
	{
                play_button_pressed = true;
	}

	// Start is called before the first frame update
	public void Update()
	{
                // Audio volume
                audioSource.volume = SettingsMenu.menuMusicVolume;

                // Check if play button is pressed
                if (play_button_pressed == true)
                {
                        SceneManager.LoadScene((int)SceneINDEX.LoadingScreen);
                }
        }

        // On scene load, play music
	private void Awake()
	{
                audioSource.Play();
	}
}