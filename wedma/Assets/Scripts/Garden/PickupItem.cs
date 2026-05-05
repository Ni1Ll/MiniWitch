using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;

    private float dropTime;

    [Header("Генетика растения")]
    public PlantInstance plantInstance;

    private void Awake()
    {
        dropTime = Time.time;
    }

    public bool HasPlantGenes()
    {
        return plantInstance != null &&
               plantInstance.baseData != null &&
               plantInstance.activeGenes != null &&
               plantInstance.dormantGenes != null &&
               plantInstance.activeGenes.Count > 0 &&
               plantInstance.dormantGenes.Count > 0;
    }

    public float GetDropTime()
    {
        return dropTime;
    }

    [ContextMenu("Clear Plant Genes")]
    public void ClearPlantGenes()
    {
        plantInstance = null;
        Debug.Log("[PickupItem] Гены очищены.");
    }
}