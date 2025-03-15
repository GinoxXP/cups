using Unity.Netcode;

public class Cup : NetworkBehaviour
{
    private NetworkVariable<ContainmentType> containment = new();

    public ContainmentType Containment
    {
        get => containment.Value;
        set => containment.Value = value;
    }

    public enum ContainmentType
    {
        Vodka,
        Poison,
    }
}
