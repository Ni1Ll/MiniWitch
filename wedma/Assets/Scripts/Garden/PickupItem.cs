using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;

    private float dropTime;

    [Header("Генетика растения")]
    public PlantInstance plantInstance;

    [Header("Debug")]
    public bool generateGenesInEditor = true;

    private void OnValidate()
    {
        // Работает в редакторе, чтобы гены были видны в Inspector
        if (!generateGenesInEditor) return;

        TryGeneratePlantInstance();
    }

    private void Awake()
    {
        dropTime = Time.time;
        // На всякий случай генерируем при старте игры,
        // если в редакторе они не создались
        TryGeneratePlantInstance();
    }

    private void TryGeneratePlantInstance()
    {
        if (itemData == null) return;

        PlantData plantData = itemData as PlantData;
        if (plantData == null) return;

        if (plantInstance == null || plantInstance.baseData == null || plantInstance.dormantGenes == null || plantInstance.dormantGenes.Count == 0)
        {
            plantInstance = new PlantInstance(plantData);

            Debug.Log($"[PickupItem] Сгенерированы гены для {plantData.itemName}: " +
                      $"{plantInstance.activeGenes.Count} active / {plantInstance.dormantGenes.Count} dormant");
        }
    }

    [ContextMenu("Generate Plant Genes")]
    public void GeneratePlantGenes()
    {
        PlantData plantData = itemData as PlantData;

        if (plantData == null)
        {
            Debug.LogWarning("[PickupItem] itemData не является PlantData. Гены не созданы.");
            return;
        }

        plantInstance = new PlantInstance(plantData);

        Debug.Log($"[PickupItem] Гены пересозданы для {plantData.itemName}: " +
                  $"{plantInstance.activeGenes.Count} active / {plantInstance.dormantGenes.Count} dormant");
    }

    [ContextMenu("Clear Plant Genes")]
    public void ClearPlantGenes()
    {
        plantInstance = null;
        Debug.Log("[PickupItem] Гены очищены.");
    }

}