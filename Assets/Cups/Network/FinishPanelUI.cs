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
        partyManager.PlayerWon += OnPlayerWon;

        OnStateChanged(partyManager.State);
    }

    private void OnPlayerWon(Player winner)
    {
        winnerText.text = $"{winner.Name} is winner this game!";

        restartButton.SetActive(partyManager.IsSessionOwner);
    }

    private void OnStateChanged(PartyManager.GameState state)
    {
        gameObject.SetActive(state == PartyManager.GameState.Finish);
    }

    public void OnRestart()
    {
        partyManager.StartGame();
    }
}
