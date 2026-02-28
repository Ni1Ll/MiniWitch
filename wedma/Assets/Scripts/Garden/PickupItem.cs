using UnityEngine;

public enum ItemType
{
    None,
    Seeds,
    WateringCan
}

public class PickupItem : MonoBehaviour
{
    public ItemType itemType;
}