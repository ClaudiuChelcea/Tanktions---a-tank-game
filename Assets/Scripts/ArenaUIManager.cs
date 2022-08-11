using System;
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
    [SerializeField]
    private TMP_Text interrupted;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        playersConnection.enabled = false;
        timeLeft.enabled = false;
        gamePhase.enabled = false;
        winner.enabled = false;
        interrupted.enabled = false;
        turn.enabled = false;
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
        timeLeft.text = "Time left: " + time;
    }

    public void ShowGamePhase(GameManager.GamePhase phase)
    {
        gamePhase.enabled = true;
        gamePhase.text = "Game phase: " + phase;
    }

    public void ShowWaiting()
    {
        turn.enabled = true;
        turn.text = "Waiting for opponent to join...";
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
        interrupted.enabled = true;
        interrupted.text = "The opponent disconnected...";
    }

    public void HideText()
    {
        playersConnection.enabled = false;
        timeLeft.enabled = false;
        gamePhase.enabled = false;
        winner.enabled = false;
        interrupted.enabled = false;
        turn.enabled = false;
    }
}