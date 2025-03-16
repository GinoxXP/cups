using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PartyManager : NetworkBehaviour
{
    private NetworkManager networkManager;
    private Table table;
    private HealthIndicator healthIndicator;

    private List<Player> players = new();
    private CircularList<Player> moveQueue = new();

    private NetworkVariable<GameState> state = new NetworkVariable<GameState>();

    private CircularEnumerator<Player> moveQueueEnumerator { get; set; }

    public event Action<Player[]> PlayersChanged;

    public event Action<GameState> StateChanged;

    public event Action<Player> MoveChanged;

    public Player[] Players => players.ToArray();

    public GameState State => state.Value;

    public Player CurrentMovePlayer => moveQueueEnumerator?.Current;

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

    private void OnClientConnected(ulong id)
    {
        if (!IsSessionOwner)
            return;

        AddPlayerToListRpc(id);
    }

    private void AddConnectedPlayers()
    {
        var ids = networkManager.ConnectedClientsIds;
        foreach (var id in ids)
        {
            StartCoroutine(AddPlayerToList(id));
        }
    }

    [Rpc(SendTo.Everyone)]
    private void AddPlayerToListRpc(ulong id)
    {
        if (players.Count == 0)
            AddConnectedPlayers();
        else
            StartCoroutine(AddPlayerToList(id));
    }

    private IEnumerator AddPlayerToList(ulong id)
    {
        yield return new WaitForSeconds(0.1f);

        var players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);
        var player = players.First(x => x.OwnerClientId == id);

        if (player.IsLocalPlayer)
            healthIndicator.RegisterPlayer(player);

        this.players.Add(player);
        PlayersChanged.Invoke(Players);
    }

    public void StartGame()
    {
        state.Value = GameState.Started;

        StartGameRpc();

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var sit = table.PlayersSits[i];

            player.HealRpc();

            player.SetPositionRpc(sit.position);

            var lookDirection = table.transform.position - sit.position;
            lookDirection.y = 0;

            player.SetRotationRpc(Quaternion.LookRotation(lookDirection, Vector3.up));
        }

        table.SpawnCups(players.Count);
    }

    [Rpc(SendTo.Owner)]
    private void OnCupSelectedRpc(ulong playerId, ulong networkId, Cup.ContainmentType containment)
    {
        var currentPlayerMove = moveQueueEnumerator.Current;

        if (playerId != currentPlayerMove.OwnerClientId)
            return;

        if (containment == Cup.ContainmentType.Poison)
            currentPlayerMove.DamageRpc();

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

        MoveChanged?.Invoke(moveQueueEnumerator.Current);
    }

    [Rpc(SendTo.Everyone)]
    private void StartGameRpc()
    {
        moveQueue.Clear();
        foreach(var player in players)
        {
            player.SetActiveBody(!player.IsLocalPlayer);
            player.SetActiveCamera(player.IsLocalPlayer);

            player.CupSelected += OnCupSelectedRpc;

            moveQueue.Add(player);
        }

        moveQueueEnumerator = new CircularEnumerator<Player>(moveQueue);

        MoveChanged?.Invoke(moveQueueEnumerator.Current);
    }

    public enum GameState
    {
        Lobby,
        Started,
        Finish,
    }
}
