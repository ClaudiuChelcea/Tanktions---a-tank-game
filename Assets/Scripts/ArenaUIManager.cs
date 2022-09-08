using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the HUD while in gameplay.
/// </summary>
public class ArenaUIManager : MonoBehaviour
{
    // Singleton
    public static ArenaUIManager Instance { get; private set; }
    [SerializeField]
    private TMP_Text playersConnection;
    [SerializeField]
    private TMP_Text timeLeft;
    [SerializeField]
    private TMP_Text gamePhase;
    [SerializeField]
    private TMP_Text turn;
    [SerializeField]
    private TMP_Text winner;
    [SerializeField]
    private Button healthButton;
    [SerializeField]
    private TMP_Text healthButtonText;
    [SerializeField]
    private TMP_InputField equationInput;
    [SerializeField]
    private Button exitButton;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideText();

        exitButton.onClick.AddListener(() =>
        {
            GameManager.Instance.BackToMenu();
        });
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public void ShowConnectedMessage(ulong id)
    {
        playersConnection.gameObject.SetActive(true);
        playersConnection.text = "Player " + id + " connected";
    }

    public void ShowDisconnectedMessage(ulong id)
    {
        playersConnection.gameObject.SetActive(true);
        playersConnection.text = "Player " + id + " disconnected";
    }

    public void ShowTimeLeft(float time)
    {
        timeLeft.gameObject.SetActive(true);
        timeLeft.text = "Time left: " + time.ToString("00.00");
    }

    public void ShowGamePhase(GameManager.GamePhase phase)
    {
        gamePhase.gameObject.SetActive(true);
        gamePhase.text = "Game phase: " + phase;
    }

    public void ShowWaiting()
    {
        winner.gameObject.SetActive(true);
        winner.text = "Waiting for opponent to join...";
    }

    public void ShowTurn(ulong id)
    {
        turn.gameObject.SetActive(true);
        turn.text = "Turn: Player " + id;
    }

    public void ShowHealth(Player player)
    {
        healthButton.gameObject.SetActive(true);
        healthButtonText.text = "Health: " + player.health.Value;
    }

    public void ShowWinner(ulong id)
    {
        winner.gameObject.SetActive(true);
        winner.text = "Player " + id + " won!";
    }

    public void ShowInterrupted()
    {
        winner.gameObject.SetActive(true);
        winner.text = "The opponent disconnected...";
    }

    public void ShowEquationInput()
    {
        equationInput.gameObject.SetActive(true);
    }

    public void HideEquationInput()
    {
        equationInput.gameObject.SetActive(false);
    }

    public string GetEquation()
    {
        return equationInput.text;
    }

    public void ShowBulletText(string text)
    {
        winner.gameObject.SetActive(true);
        winner.text = text;
    }

    public void HideText()
    {
        playersConnection.gameObject.SetActive(false);
        timeLeft.gameObject.SetActive(false);
        gamePhase.gameObject.SetActive(false);
        turn.gameObject.SetActive(false);
        winner.gameObject.SetActive(false);
        healthButton.gameObject.SetActive(false);
    }
}