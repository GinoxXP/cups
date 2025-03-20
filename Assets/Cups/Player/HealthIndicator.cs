using TMPro;
using UnityEngine;

public class HealthIndicator : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void RegisterPlayer(Player player)
    {
        player.EyesChanged += OnEyesChanged;
        gameObject.SetActive(true);

        OnEyesChanged(player.Eyes);
    }

    private void OnEyesChanged(int value)
    {
        var faceId = Faces.FaceStates.Length - 1 - value;
        text.text = Faces.FaceStates[faceId];
    }
}
