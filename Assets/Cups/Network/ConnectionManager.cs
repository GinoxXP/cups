using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private string profileName;
    private string sessionName = "testSessionName";
    private int maxPlayers = 4;
    private ConnectionState state = ConnectionState.SettingName;
    private ISession session;

    public int MaxPlayers => maxPlayers;

    public string ProfileName
    {
        get => profileName;
        set
        {
            if (State != ConnectionState.SettingName)
                return;

            profileName = value;
            State = ConnectionState.ChangingMode;
        }
    }

    public ConnectionState State
    {
        get => state;
        set
        {
            if (state == value)
                return;

            state = value;
            StateChanged?.Invoke(state);
        }
    }

    public string SessionCode => session.Code;

    public event Action<ConnectionState> StateChanged;

    public enum ConnectionState
    {
        SettingName,
        ChangingMode,
        Connecting,
        Connected,
        Disconnected,
    }

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
    }

    private void OnDestroy()
    {
        session?.LeaveAsync();
    }

    public async Task CreateSessionAsync()
    {
        State = ConnectionState.Connecting;

        try
        {
            AuthenticationService.Instance.SwitchProfile(profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = maxPlayers
            }.WithDistributedAuthorityNetwork();

            session = await MultiplayerService.Instance.CreateSessionAsync(options);

            State = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            State = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }

    public async Task JoinToSessionAsync(string code)
    {
        try
        {
            AuthenticationService.Instance.SwitchProfile(profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            State = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            State = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }
}
