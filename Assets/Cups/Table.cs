using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Table : NetworkBehaviour
{
    [SerializeField]
    private Transform playerSitsOrigin;
    [SerializeField]
    private Transform gameBoard;
    [SerializeField]
    private GameObject cupPrefab;

    private List<Transform> playersSits = new();

    public List<Transform> PlayersSits => playersSits;

    private void Awake()
    {
        for (int i = 0; i < playerSitsOrigin.childCount; i++)
            playersSits.Add(playerSitsOrigin.GetChild(i));
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        var cup = Instantiate(cupPrefab, gameBoard);
        var instanceNetworkObject = cup.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
    }
}
