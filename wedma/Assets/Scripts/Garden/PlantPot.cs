using UnityEngine;

public enum PlantActionType
{
    None,
    Water,
    Plant,
    Harvest,
    Clear
}

public enum ToolType
{
    None,
    WateringCan,  
    Shovel,     
}

public class PlantPot : MonoBehaviour
{
    [Header("Состояние")]
    public float currentWater = 0f;
    public float maxWater = 100f;
    public float currentTemperature = 20f;

    [Header("Кто здесь живет?")]
    public PlantData currentPlant;

    [Header("Точка спавна")]
    public Transform spawnPoint;

    private GameObject spawnedVisual;
    public float currentGrowth = 0f;
    public float currentHealth = 100f;
    public bool isDead = false;
    private int currentPhaseIndex = -1;

    void Update()
    {
        if (currentPlant != null && !isDead)
        {
            ProcessGrowth();
            UpdateVisualPhase();
        }
    }

    void ProcessGrowth()
    {
        if (currentGrowth >= 100f) return;

        float tempDiff = Mathf.Abs(currentTemperature - currentPlant.optimalTemp);
        if (tempDiff > currentPlant.tempRange) currentHealth -= 5f * Time.deltaTime;

        if (currentWater > 0)
        {
            currentWater -= currentPlant.waterConsumption * Time.deltaTime;
            currentGrowth += currentPlant.growthSpeed * Time.deltaTime;
            currentHealth += 5f * Time.deltaTime;
        }
        else
        {
            currentHealth -= 1f * Time.deltaTime;
        }

        currentWater = Mathf.Clamp(currentWater, 0, maxWater);
        currentHealth = Mathf.Clamp(currentHealth, 0, 100f);
        currentGrowth = Mathf.Clamp(currentGrowth, 0, 100);

        if (currentHealth <= 0) Die();
    }

    void UpdateVisualPhase()
    {
        if (currentPlant == null || currentPlant.growthPrefabs == null) return;

        int phase = Mathf.Clamp((int)(currentGrowth / 25f), 0, currentPlant.growthPrefabs.Length - 1);
        if (phase == currentPhaseIndex) return;

        currentPhaseIndex = phase;
        SpawnVisual(currentPlant.growthPrefabs[phase]);
    }

    void SpawnVisual(GameObject prefab)
    {
        if (spawnedVisual != null) Destroy(spawnedVisual);

        if (prefab == null)
        {
            Debug.LogWarning($"[PlantPot] Префаб для фазы {currentPhaseIndex} не назначен!");
            return;
        }

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        spawnedVisual = Instantiate(prefab, origin.position, origin.rotation);
    }

    public PlantActionType Interact(PlayerInventory inventory)
    {
        if (isDead)
        {
            ClearPot();
            return PlantActionType.Clear;
        }

        if (currentPlant != null && currentGrowth >= 100f)
        {
            Harvest(inventory);
            return PlantActionType.Harvest;
        }

        InventorySlot activeSlot = inventory.GetSelectedSlot();
        if (activeSlot.IsEmpty) return PlantActionType.None;

        // ИСПРАВЛЕНИЕ: проверяем что инструмент — именно лейка, а не любой инструмент
        if (activeSlot.item.isTool && activeSlot.item.toolType == ToolType.WateringCan)
        {
            currentWater = maxWater;
            Debug.Log("Полито!");
            return PlantActionType.Water;
        }

        if (currentPlant == null && activeSlot.item is PlantData)
        {
            PlantData seedData = (PlantData)activeSlot.item;
            Plant(seedData);
            inventory.ConsumeSelectedItem();
            return PlantActionType.Plant;
        }

        return PlantActionType.None;
    }

    private void Harvest(PlayerInventory inventory)
    {
        if (currentPlant.harvestResult != null)
        {
            int leftover = inventory.AddItem(currentPlant.harvestResult, currentPlant.harvestAmount);

            if (leftover == 0)
            {
                Debug.Log($"Собрано: {currentPlant.harvestResult.itemName}");
                ClearPot();
            }
            else
            {
                Debug.Log("Инвентарь полон! Освободите место для урожая.");
            }
        }
        else
        {
            Debug.LogWarning($"У {currentPlant.itemName} не настроен harvestResult!");
            ClearPot();
        }
    }

    void Plant(PlantData newData)
    {
        currentPlant = newData;
        currentGrowth = 0f;
        currentHealth = 100f;
        isDead = false;
        currentPhaseIndex = -1;

        Debug.Log($"Посажена: {currentPlant.itemName}");
    }

    void Die()
    {
        isDead = true;
        currentPhaseIndex = -1;
        Debug.Log("Погибло!");
        SpawnVisual(currentPlant.deadPrefab);
    }

    void ClearPot()
    {
        if (spawnedVisual != null) Destroy(spawnedVisual);
        spawnedVisual = null;
        currentPlant = null;
        isDead = false;
        currentPhaseIndex = -1;
    }

    public PlantActionType GetAvailableAction(PlayerInventory inventory)
    {
        if (isDead) return PlantActionType.Clear;
        if (currentPlant != null && currentGrowth >= 100f) return PlantActionType.Harvest;

        InventorySlot activeSlot = inventory.GetSelectedSlot();
        if (activeSlot.IsEmpty) return PlantActionType.None;

        // ИСПРАВЛЕНИЕ: та же проверка типа инструмента
        if (activeSlot.item.isTool && activeSlot.item.toolType == ToolType.WateringCan)
            return PlantActionType.Water;

        if (currentPlant == null && activeSlot.item is PlantData)
            return PlantActionType.Plant;

        return PlantActionType.None;
    }
}