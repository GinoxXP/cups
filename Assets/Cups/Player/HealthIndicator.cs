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
        player.eyes.OnValueChanged += OnEyesChanged;
        gameObject.SetActive(true);

        OnEyesChanged(0, player.Eyes);
    }

    private void OnEyesChanged(int _, int newValue)
    {
        var faceId = Faces.FaceStates.Length - 1 - newValue;
        text.text = Faces.FaceStates[faceId];
    }
}
