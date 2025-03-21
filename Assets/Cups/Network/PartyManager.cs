using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PartyManager : NetworkBehaviour
{
    private NetworkManager networkManager;
    private Table table;
    private HealthIndicator healthIndicator;

    private List<Player> players = new();
    private CircularList<Player> moveQueue = new();

    private NetworkVariable<GameState> state = new();

    private int currentMovePlayerIndex;

    public event Action<Player[]> PlayersChanged;

    public event Action<GameState> StateChanged;

    public event Action<Player> MoveChanged;

    public event Action<Player> PlayerWon;

    public Player[] Players => players.ToArray();

    public GameState State => state.Value;

    public override void OnNetworkSpawn()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        table = FindFirstObjectByType<Table>();
        healthIndicator = FindFirstObjectByType<HealthIndicator>(FindObjectsInactive.Include);

        networkManager.OnClientConnectedCallback += OnClientConnected;

        state.OnValueChanged += OnStateChanged;
    }

    private void OnStateChanged(GameState oldValue, GameState newValue)
    {
        StateChanged?.Invoke(State);
    }

    private void OnClientConnected(ulong _)
    {
        if (!IsSessionOwner || State != GameState.Lobby)
            return;

        StartCoroutine(WaitForSpawnPlayerGameObject());
    }

    private IEnumerator WaitForSpawnPlayerGameObject()
    {
        yield return new WaitForSeconds(0.1f);
        UpdatePlayersListRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void UpdatePlayersListRpc()
    {
        this.players.Clear();
        var players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);
        
        foreach (var player in players)
        {
            if (player.IsLocalPlayer)
                healthIndicator.RegisterPlayer(player);

            this.players.Add(player);
        }

        this.players.Reverse();
        PlayersChanged.Invoke(Players);
    }

    public void StartGame()
    {
        state.Value = GameState.Started;

        StartGameRpc();

        moveQueue.Clear();
        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var sit = table.PlayersSits[i];

            player.Heal();

            player.SetPositionRpc(sit.position);

            var lookDirection = table.transform.position - sit.position;
            lookDirection.y = 0;

            player.SetRotationRpc(Quaternion.LookRotation(lookDirection, Vector3.up));

            moveQueue.Add(player);
        }

        currentMovePlayerIndex = 0;

        table.SpawnCups(players.Count);

        
        MoveChangedRpc(0);
    }

    [Rpc(SendTo.Everyone)]
    private void StartGameRpc()
    {
        foreach (var player in players)
        {
            player.SetActiveBody(!player.IsLocalPlayer);
            player.SetActiveCamera(player.IsLocalPlayer);

            if (player.IsLocalPlayer)
            {
                player.CupSelected -= OnCupSelectedRpc;
                player.CupSelected += OnCupSelectedRpc;
            }
        }
    }

    [Rpc(SendTo.Owner)]
    private void OnCupSelectedRpc(ulong playerId, ulong networkId, Cup.ContainmentType containment)
    {
        if (State != GameState.Started)
            return;

        var currentPlayerMove = moveQueue[currentMovePlayerIndex];

        if (playerId != currentPlayerMove.OwnerClientId)
            return;

        if (containment == Cup.ContainmentType.Poison)
            currentPlayerMove.Damage();

        var networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkId];
        table.DespawnCup(networkObject);

        if (currentPlayerMove.Eyes <= 0)
            moveQueue.Remove(currentPlayerMove);
        else
            currentMovePlayerIndex++;

        if (currentMovePlayerIndex > moveQueue.Count - 1)
            currentMovePlayerIndex = 0;

        var indexMovePlayer = players.IndexOf(moveQueue[currentMovePlayerIndex]);

        if (moveQueue.Count == 1)
        {
            state.Value = GameState.Finish;
            PlayerWonRpc(indexMovePlayer);
        }

        MoveChangedRpc(indexMovePlayer);
    }

    [Rpc(SendTo.Everyone)]
    private void MoveChangedRpc(int playerIndex)
    {
        var player = players[playerIndex];
        MoveChanged?.Invoke(player);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayerWonRpc(int playerIndex)
    {
        var player = players[playerIndex];
        PlayerWon?.Invoke(player);
    }

    public enum GameState
    {
        Lobby,
        Started,
        Finish,
    }
}
