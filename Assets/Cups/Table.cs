using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Table : NetworkBehaviour
{
    [SerializeField]
    private Transform playerSitsOrigin;
    [SerializeField]
    private Transform lookOrigin;
    [SerializeField]
    private Transform gameBoard;
    [SerializeField]
    private float gameBoardSize;
    [SerializeField]
    private GameObject cupPrefab;

    private List<Transform> playersSits = new();

    public List<Transform> PlayersSits => playersSits;

    public Transform LookOrigin => lookOrigin;

    private void Awake()
    {
        for (int i = 0; i < playerSitsOrigin.childCount; i++)
            playersSits.Add(playerSitsOrigin.GetChild(i));
    }

    public void SpawnCups(int players)
    {
        var vodkaCups = players * 5;
        var poisonCups = players * 2;

        for (var i = 0; i < vodkaCups; i++)
            SpawnCup(Cup.ContainmentType.Vodka);

        for (var i = 0; i < poisonCups; i++)
            SpawnCup(Cup.ContainmentType.Poison);

    }

    private void SpawnCup(Cup.ContainmentType containmenType)
    {
        var cupGameObject = Instantiate(cupPrefab, gameBoard);

        var cup = cupGameObject.GetComponent<Cup>();
        cup.Containment = containmenType;

        var position = gameBoard.position + new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y) * gameBoardSize;
        cup.transform.position = position;

        var instanceNetworkObject = cupGameObject.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
    }
}
