using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Настройки Предмета")]
    public bool isWateringCan = false; // Это лейка?
    public PlantData plantData;        // Если не лейка, то какие это семена?

    void OnValidate()
    {
        // Небольшая помощь: меняет имя объекта в редакторе, чтобы не путаться
        if (plantData != null) gameObject.name = "Pickup_" + plantData.plantName;
        else if (isWateringCan) gameObject.name = "Pickup_WateringCan";
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}