using UnityEngine;

public class SetNameUI : MonoBehaviour
{
    private new string name;
    private ConnectionManager connectionManager;

    private void Awake()
    {
        connectionManager = FindFirstObjectByType<ConnectionManager>();
        connectionManager.StateChanged += OnStateChanged;

        OnStateChanged(connectionManager.State);
    }

    private void OnStateChanged(ConnectionManager.ConnectionState state)
    {
        gameObject.SetActive(state == ConnectionManager.ConnectionState.SettingName);
    }

    public void OnNameChanged(string name)
    {
        this.name = name;
    }

    public void OnSetName()
    {
        connectionManager.ProfileName = name;
    }
}
