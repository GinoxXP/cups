using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Owner);

    [SerializeField]
    private GameObject body;
    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private TMP_Text faceIndicator;

    private PlayerInput playerInput;
    private Vector2 pointPosition;
    private int eyes = 2;

    public int Eyes
    {
        get => eyes;
        set
        {
            if (eyes == value)
                return;

            eyes = value;
            EyesUpdateRpc(Eyes);

            UpdateFace();
        }
    }

    public string Name => playerName.Value.ToString();

    public event Action<ulong, ulong, Cup.ContainmentType> CupSelected;

    public event Action<int> EyesChanged;

    public override void OnNetworkSpawn()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = IsLocalPlayer;

        SetActiveBody(false);
        SetActiveCamera(false);

        if (!IsOwner)
            return;

        playerName.Value = AuthenticationService.Instance.Profile;
        UpdateFace();
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

    public void Heal()
    {
        Eyes = 2;
    }

    public void Damage()
    {
        Eyes--;
    }

    [Rpc(SendTo.Owner)]
    public void SetPositionRpc(Vector3 position)
    {
        transform.position = position;
    }

    [Rpc(SendTo.Owner)]
    public void SetRotationRpc(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    [Rpc(SendTo.Everyone)]
    private void SetFaceRpc(int faceId)
    {
        faceIndicator.text = Faces.FaceStates[faceId];
    }

    [Rpc(SendTo.Everyone)]
    private void EyesUpdateRpc(int eyes)
    {
        EyesChanged?.Invoke(eyes);
    }

    private void UpdateFace()
    {
        var faceId = Faces.FaceStates.Length - 1 - Eyes;
        SetFaceRpc(faceId);
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
