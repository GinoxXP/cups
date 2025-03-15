using System;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> eyes = new NetworkVariable<int>(2);

    [SerializeField]
    private GameObject body;
    [SerializeField]
    private new Camera camera;

    private PlayerInput playerInput;
    private Vector2 pointPosition;

    public string Name => playerName.Value.ToString();

    public event Action<ulong, ulong, Cup.ContainmentType> CupSelected;

    public override void OnNetworkSpawn()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = IsLocalPlayer;

        SetActiveBody(false);
        SetActiveCamera(false);

        if (!IsOwner)
            return;

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

    public void OnPoint(InputAction.CallbackContext context)
    {
        pointPosition = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Canceled)
            return;

        Interact();
    }

    private void Interact()
    {
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(pointPosition);

        if (Physics.Raycast(ray, out hit))
        {
            var cup = hit.transform.GetComponentInParent<Cup>();
            if (cup == null)
                return;

            var networkObject = cup.GetComponent<NetworkObject>();

            CupSelected?.Invoke(OwnerClientId, networkObject.NetworkObjectId, cup.Containment);
        }
    }
}
