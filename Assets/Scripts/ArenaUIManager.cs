using TMPro;
using UnityEngine;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideText();
    }

    public void ShowConnectedMessage(ulong id)
    {
        playersConnection.enabled = true;
        playersConnection.text = "Player " + id + " connected";
    }

    public void ShowDisconnectedMessage(ulong id)
    {
        playersConnection.enabled = true;
        playersConnection.text = "Player " + id + " disconnected";
    }

    public void ShowTimeLeft(float time)
    {
        timeLeft.enabled = true;
        timeLeft.text = "Time left: " + time.ToString("00.00");
    }

    public void ShowGamePhase(GameManager.GamePhase phase)
    {
        gamePhase.enabled = true;
        gamePhase.text = "Game phase: " + phase;
    }

    public void ShowWaiting()
    {
        winner.enabled = true;
        winner.text = "Waiting for opponent to join...";
    }

    public void ShowTurn(ulong id)
    {
        turn.enabled = true;
        turn.text = "Turn: Player " + id;
    }

    public void ShowWinner(ulong id)
    {
        winner.enabled = true;
        winner.text = "Player " + id + " won!";
    }

    public void ShowInterrupted()
    {
        winner.enabled = true;
        winner.text = "The opponent disconnected...";
    }

    public void HideText()
    {
        playersConnection.enabled = false;
        timeLeft.enabled = false;
        gamePhase.enabled = false;
        winner.enabled = false;
        turn.enabled = false;
    }

    public void OnDestroy()
    {
        Instance = null;
    }
}