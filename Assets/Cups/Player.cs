using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Owner);

    [SerializeField]
    private GameObject body;
    [SerializeField]
    private new Camera camera;

    public string Name => playerName.Value.ToString();

    public override void OnNetworkSpawn()
    {
        SetActiveBody(false);
        SetActiveCamera(false);

        //if (IsServer)
        //    return;

        if (!IsOwner)
        {
            
            return;
        }

        playerName.Value = AuthenticationService.Instance.Profile;
    }

    public void SetActiveBody(bool state)
    {
        body.SetActive(state);
    }

    public void SetActiveCamera(bool state)
    {
        camera.gameObject.SetActive(state);
    }
}
