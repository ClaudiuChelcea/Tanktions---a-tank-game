using UnityEngine;
using TMPro;

public class CountdownTimerUI : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_Text textComponent;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        textComponent = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        textComponent.text = gameManager.timeLeft.ToString("0.00");
    }
}
