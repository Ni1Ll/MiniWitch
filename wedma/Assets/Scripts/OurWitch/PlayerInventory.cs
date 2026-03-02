using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Состояние Карманов (Только чтение)")]
    public PlantData currentPlantData;
    public bool hasWateringCan = false;

    // Попытаться положить семена в карман
    public bool AddSeeds(PlantData data)
    {
        if (data == null)
        {
            Debug.LogError("ОШИБКА: Пытаемся подобрать предмет, но в нем нет PlantData!");
            return false;
        }
        if (currentPlantData != null || hasWateringCan)
        {
            Debug.Log("Инвентарь: Руки заняты, не могу взять.");
            return false;
        }

        currentPlantData = data;
        return true;
    }

    // Попытаться положить лейку
    public bool AddWateringCan()
    {
        if (currentPlantData != null || hasWateringCan)
        {
            Debug.Log("Инвентарь: Руки заняты.");
            return false;
        }

        hasWateringCan = true;
        return true;
    }

    // Забрать семена (для выброса или посадки)
    public PlantData RemoveSeeds()
    {
        var data = currentPlantData;
        currentPlantData = null;
        return data;
    }

    // Забрать лейку
    public void RemoveWateringCan()
    {
        hasWateringCan = false;
    }

    public bool HasSeeds() => currentPlantData != null;
}