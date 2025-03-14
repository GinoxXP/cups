using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private string profileName;
    private string sessionName = "testSessionName";
    private int maxPlayers = 10;
    private ConnectionState state = ConnectionState.Disconnected;
    //private ISession session;
    private NetworkManager networkManager;

    public int MaxPlayers => maxPlayers;

    private enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    private async void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        await UnityServices.InitializeAsync();
    }

    private void OnDestroy()
    {
        //session?.LeaveAsync();
    }

    private void OnGUI()
    {
        //if (state == ConnectionState.Connected)
        //    return;

        //GUI.enabled = state != ConnectionState.Connecting;

        //using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
        //{
        //    GUILayout.Label("Profile Name", GUILayout.Width(100));
        //    profileName = GUILayout.TextField(profileName);
        //}

        //using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
        //{
        //    GUILayout.Label("Session Name", GUILayout.Width(100));
        //    sessionName = GUILayout.TextField(sessionName);
        //}

        //GUI.enabled = GUI.enabled && !string.IsNullOrEmpty(profileName) && !string.IsNullOrEmpty(sessionName);

        //if (GUILayout.Button("Create or Join Session"))
        //{
        //    CreateOrJoinSessionAsync();
        //}

        if (!(GUI.enabled && !networkManager.IsClient && !networkManager.IsHost))
            return;

        if (GUILayout.Button("Host"))
            networkManager.StartHost();

        if (GUILayout.Button("Client"))
            networkManager.StartClient();
    }

    //private async Task CreateOrJoinSessionAsync()
    //{
    //    state = ConnectionState.Connecting;

    //    try
    //    {
    //        AuthenticationService.Instance.SwitchProfile(profileName);
    //        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    //        var options = new SessionOptions()
    //        {
    //            Name = sessionName,
    //            MaxPlayers = maxPlayers
    //        }.WithDistributedAuthorityNetwork();

    //        session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);

    //        state = ConnectionState.Connected;
    //    }
    //    catch (Exception e)
    //    {
    //        state = ConnectionState.Disconnected;
    //        Debug.LogException(e);
    //    }
    //}
}
