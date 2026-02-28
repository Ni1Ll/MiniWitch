using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Настройки предмета")]
    public PlantData plantData;

    public bool isWateringCan = false;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}