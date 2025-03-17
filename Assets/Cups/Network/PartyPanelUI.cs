using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PartyPanelUI : NetworkBehaviour
{
    private ConnectionManager connectionManager;
    private PartyManager partyManager;

    [SerializeField]
    private TMP_Text sessionCode;
    [SerializeField]
    private TMP_Text playersList;
    [SerializeField]
    private GameObject startButton;

    private void Awake()
    {
        connectionManager = FindFirstObjectByType<ConnectionManager>();
        partyManager = FindFirstObjectByType<PartyManager>();

        connectionManager.StateChanged += OnStateChanged;
        partyManager.PlayersChanged += OnPlayersChanged;
        partyManager.StateChanged += OnStateChanged;

        OnStateChanged(connectionManager.State);
    }

    private void Update()
    {
        sessionCode.text = $"Session code: {connectionManager.SessionCode}";
        startButton.SetActive(IsSessionOwner);
    }

    private void OnStateChanged(PartyManager.GameState obj)
    {
        SetActive();
    }

    private void OnPlayersChanged(Player[] players)
    {
        var names = players.Select(x => x.Name);
        playersList.text = string.Join("\n", names);
    }

    private void OnStateChanged(ConnectionManager.ConnectionState state)
    {
        SetActive();
    }

    private void SetActive()
    {
        gameObject.SetActive(connectionManager.State == ConnectionManager.ConnectionState.Connected && partyManager.State == PartyManager.GameState.Lobby);
    }

    public void OnStart()
    {
        partyManager.StartGame();
    }
}
