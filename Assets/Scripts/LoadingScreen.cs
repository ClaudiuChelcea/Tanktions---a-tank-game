using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Manages the loading operation for the arena.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    // Get level loader progress bar
    public Scrollbar progressbar_level;
    public TextMeshProUGUI loading_status;

    // Minimum loading time variables
    private float StartLoadingTime = 0f;
    private bool Loading = true;
    public float LoadingMinTime = 5; // most important, minimum time to load scene
    AsyncOperation loadingOperation;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadAsyncOperation());
    }

    private IEnumerator LoadAsyncOperation()
    {
        // For now only ArenaIndustrial
        // In the future, the play button should select a random map index, and that index is public in MainMenu, being sent here to load
        loadingOperation = SceneManager.LoadSceneAsync(GameManager.Instance.arenaName.Value.ToString());

        // Stop it so we can put a minimum loading time
        loadingOperation.allowSceneActivation = false;
        Loading = true;
        StartLoadingTime = Time.time;

        // As long as the level didn't load, fill loading progress bar
        while (loadingOperation.progress < 1)
        {
            // this is the progress bar of level load
            // will be, for this game, always up to 90%, because it just loads so fast due to the small game
            // but we will limit by time
            progressbar_level.value = (float)loadingOperation.progress;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();
    }

    private void Update()
    {
        if (Loading)
        {
            float loadingTime = Time.time - StartLoadingTime;
            if ((Math.Floor((LoadingMinTime - loadingTime) * 100) / 100) > 0)
                loading_status.text = "Game loaded! Prepare to fight in " + (Math.Floor((LoadingMinTime - loadingTime) * 100) / 100).ToString();

            // Checks if the min loading time has passed
            if (loadingTime >= LoadingMinTime)
            {
                // At this point if the loading is really over, the scene will load
                // If the load it's not over, it will continue to load and switch to the scene at the end
                loadingOperation.allowSceneActivation = true;
                Loading = false;
            }
        }
    }
}
