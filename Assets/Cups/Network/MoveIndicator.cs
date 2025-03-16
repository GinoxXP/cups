using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MoveIndicator : NetworkBehaviour
{
    private PartyManager partyManager;

    [SerializeField]
    private TMP_Text anotherPlayerMoveText;
    [SerializeField]
    private GameObject yourMoveText;

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        partyManager.StateChanged += OnStateChanged;
        partyManager.MoveChanged += OnMoveChanged;

        OnStateChanged(partyManager.State);
    }

    private void OnStateChanged(PartyManager.GameState state)
    {
        gameObject.SetActive(state == PartyManager.GameState.Started);
    }

    private void OnMoveChanged(Player currentPlayerMove)
    {
        var isYourMove = NetworkManager.LocalClientId == currentPlayerMove.OwnerClientId;

        anotherPlayerMoveText.gameObject.SetActive(!isYourMove);
        yourMoveText.SetActive(isYourMove);

        anotherPlayerMoveText.text = $"Wait. {currentPlayerMove.Name} is moving right now";
    }
}
