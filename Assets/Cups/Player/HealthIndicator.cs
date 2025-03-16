using TMPro;
using UnityEngine;

public class HealthIndicator : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    public void RegisterPlayer(Player player)
    {
        player.eyes.OnValueChanged += OnEyesChanged;
    }

    private void OnEyesChanged(int oldValue, int newValue)
    {
        var faceId = Faces.FaceStates.Length - 1 - newValue;
        text.text = Faces.FaceStates[faceId];
    }
}
