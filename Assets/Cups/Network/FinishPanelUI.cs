using TMPro;
using Unity.Netcode;
using UnityEngine;

public class FinishPanelUI : NetworkBehaviour
{
    private PartyManager partyManager;

    [SerializeField]
    private TMP_Text winnerText;
    [SerializeField]
    private GameObject restartButton;

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        partyManager.StateChanged += OnStateChanged;

        OnStateChanged(partyManager.State);
    }

    private void OnStateChanged(PartyManager.GameState state)
    {
        gameObject.SetActive(state == PartyManager.GameState.Finish);
    }

    private void OnEnable()
    {
        var winner = partyManager.CurrentMovePlayer;
        if (winner == null)
            return;

        winnerText.text = $"{winner.Name} is winner this game!";

        restartButton.SetActive(partyManager.IsSessionOwner);
    }

    public void OnRestart()
    {
        partyManager.StartGame();
    }
}
