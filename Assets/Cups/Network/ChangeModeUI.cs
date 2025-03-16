using UnityEngine;

public class ChangeModeUI : MonoBehaviour
{
    private ConnectionManager connectionManager;
    private string code;

    private void Awake()
    {
        connectionManager = FindFirstObjectByType<ConnectionManager>();
        connectionManager.StateChanged += OnStateChanged;

        OnStateChanged(connectionManager.State);
    }

    private void OnStateChanged(ConnectionManager.ConnectionState state)
    {
        gameObject.SetActive(state == ConnectionManager.ConnectionState.ChangingMode);
    }

    public void OnCodeChanged(string code)
    {
        this.code = code;
    }

    public void OnHost()
    {
        connectionManager.CreateSessionAsync();
    }

    public void OnJoin()
    {
        connectionManager.JoinToSessionAsync(code);
    }
}
