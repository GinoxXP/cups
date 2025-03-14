using Unity.Netcode;

public class Cup : NetworkBehaviour
{
    private ContainmenType containment;

    public ContainmenType Containment
    {
        get => containment;
        set => containment = value;
    }

    public enum ContainmenType
    {
        Vodka,
        Poison,
    }
}
