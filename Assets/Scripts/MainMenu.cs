using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The main menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    // Get audio
    public AudioSource audioSource;
    [SerializeField]
    private Button startButton;

    // On scene load, play music
    private void Awake()
    {
        audioSource.Play();
    }

    void Start()
    {
        startButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(ScenesNames.MultiplayerMenu);
        });
    }

    void Update()
    {
        // Audio volume
        audioSource.volume = SettingsMenu.menuMusicVolume;
    }
}
