using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PartyManager : NetworkBehaviour
{
    private NetworkManager networkManager;

    private List<Player> players = new();

    public override void OnNetworkSpawn()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
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

        if (IsSessionOwner)
        {
            if (GUILayout.Button("Start game"))
                StartGame();
        }
        GUILayout.Label($"Players connected: {players.Count}");

        foreach(var player in players)
        {
            GUILayout.Label($"{player.Name} {(player.IsOwner ? " (You)" : string.Empty)}");
        }
    }

    private void StartGame()
    {

    }
}
