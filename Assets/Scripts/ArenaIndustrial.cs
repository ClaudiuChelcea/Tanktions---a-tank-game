using Unity.Netcode;
using UnityEngine;

public class ArenaIndustrial : MonoBehaviour
{
    // Get audio
    public AudioSource audioSource;
    private GameObject player;

    private void Update()
    {
        // Keep audio level up to date
        audioSource.volume = SettingsMenu.levelMusicVolume;
    }

    private void Start()
    {
        player = GameObject.Find("Player");
        player.GetComponent<NetworkObject>().Spawn();
    }

    // On scene load, play music
    private void Awake()
    {
        if (SettingsMenu.levelMusicVolume > 0f)
            audioSource.Play();
    }
}
