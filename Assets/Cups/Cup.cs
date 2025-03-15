using Unity.Netcode;

public class Cup : NetworkBehaviour
{
    private ContainmentType containment;

    public ContainmentType Containment
    {
        get => containment;
        set => containment = value;
    }

    public enum ContainmentType
    {
        Vodka,
        Poison,
    }
}
