using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PartyManager : NetworkBehaviour
{
    private NetworkManager networkManager;
    private Table table;

    private List<Player> players = new();
    private CircularList<Player> moveQueue = new();

    private NetworkVariable<GameState> state = new NetworkVariable<GameState>();

    private CircularEnumerator<Player> moveQueueEnumerator { get; set; }

    public override void OnNetworkSpawn()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        table = FindFirstObjectByType<Table>();

        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong id)
    {
        if (!IsServer)
            return;

        AddPlayerToListRpc(id);
    }

    private void AddConnectedPlayers()
    {
        var ids = networkManager.ConnectedClientsIds;
        foreach (var id in ids)
        {
            AddPlayerToList(id);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void AddPlayerToListRpc(ulong id)
    {
        if (players.Count == 0)
            AddConnectedPlayers();
        else
            AddPlayerToList(id);
    }

    private void AddPlayerToList(ulong id)
    {
        var players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);
        var player = players.First(x => x.OwnerClientId == id);

        this.players.Add(player);
    }

    private void OnGUI()
    {
        if (players.Count == 0)
            return;

        switch (state.Value)
        {
            case GameState.Lobby:
                LobbyUI();
                break;

            case GameState.Started:
                StartedUI();
                break;

            case GameState.Finish:
                FinishUI();
                break;

            default:
                break;
        }
    }

    private void LobbyUI()
    {
        if (IsHost)
        {
            if (GUILayout.Button("Start game"))
                StartGame();
        }

        GUILayout.Label($"Players connected: {players.Count}");

        foreach (var player in players)
        {
            GUILayout.Label($"{player.Name} {(player.IsOwner ? " (You)" : string.Empty)}");
        }
    }

    private void StartedUI()
    {
        var isYourMove = NetworkManager.LocalClientId == moveQueueEnumerator.Current.OwnerClientId;
        GUILayout.Label($"It's {(isYourMove ? "your" : moveQueueEnumerator.Current.Name)} move");
    }

    private void FinishUI()
    {
        GUILayout.Label($"Player {moveQueue.First().Name} won!");

        if (IsHost)
        {
            if (GUILayout.Button("Start game"))
                StartGame();
        }
    }

    private void StartGame()
    {
        state.Value = GameState.Started;

        StartGameRpc();

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var sit = table.PlayersSits[i];

            player.Heal();

            player.transform.position = sit.position;

            var lookDirection = table.transform.position - player.transform.position;
            lookDirection.y = 0;

            player.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }

        table.SpawnCups(players.Count);
    }

    [Rpc(SendTo.Server)]
    private void OnCupSelectedRpc(ulong playerId, ulong networkId, Cup.ContainmentType containment)
    {
        var currentPlayerMove = moveQueueEnumerator.Current;

        if (playerId != currentPlayerMove.OwnerClientId)
            return;

        if (containment == Cup.ContainmentType.Poison)
            currentPlayerMove.Eyes--;

        var networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkId];
        table.DespawnCup(networkObject);

        NextMoveRpc();

        if (moveQueue.Count == 1)
        {
            state.Value = GameState.Finish;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void NextMoveRpc()
    {
        moveQueueEnumerator.MoveNext();
        var nextPlayer = moveQueueEnumerator.Current;

        for (int i = moveQueue.Count - 1; i >= 0 ; i--)
        {
            if (moveQueue[i].Eyes == 0)
                moveQueue.RemoveAt(i);
        }

        moveQueueEnumerator = new CircularEnumerator<Player>(moveQueue);
        while (nextPlayer != moveQueueEnumerator.Current)
            moveQueueEnumerator.MoveNext();
    }

    [Rpc(SendTo.Everyone)]
    private void StartGameRpc()
    {
        moveQueue.Clear();
        foreach(var player in players)
        {
            player.SetActiveBody(true);
            player.SetActiveCamera(player.IsLocalPlayer);

            player.CupSelected += OnCupSelectedRpc;

            moveQueue.Add(player);
        }

        moveQueueEnumerator = new CircularEnumerator<Player>(moveQueue);
    }

    private enum GameState
    {
        Lobby,
        Started,
        Finish,
    }
}
