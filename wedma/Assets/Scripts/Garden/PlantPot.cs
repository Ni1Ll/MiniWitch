using UnityEngine;
using System.Collections.Generic;

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

[System.Serializable]
public struct SoilVisualState
{
    public string name;
    public float minWater;
    public GameObject prefab;
}

public class PlantPot : MonoBehaviour
{
    [Header("Состояние")]
    public float currentWater = 0f;
    public float maxWater = 100f;
    public float currentTemperature = 20f;

    [Header("Кто здесь живет?")]
    public PlantData currentPlant;
    public PlantInstance currentPlantInstance;

    [Header("Точка спавна")]
    public Transform spawnPoint;

    private GameObject spawnedVisual;
    public float currentGrowth = 0f;
    public float currentHealth = 100f;
    public bool isDead = false;
    private int currentPhaseIndex = -1;

    [Header("DEBUG")]
    public bool debugControlGrowth = true;

    [Range(0, 100)]
    public float debugGrowthValue = 0f;

    [Tooltip("Пока вручную задаём этап донора: 2 или 3")]
    public int donorStageDebug = 2;

    [Header("Визуал Почвы")]
    public GameObject baseSoilModel;
    public Transform soilSpawnPoint;
    public SoilVisualState[] soilStates;

    private GameObject currentSoilVisual;
    private int currentSoilIndex = -1;

    private bool isBeingWatered = false;
    public float waterFillSpeed = 50f;

    public void SetWatering(bool state)
    {
        isBeingWatered = state;
    }

    void Update()
    {
        if (isBeingWatered && currentWater < maxWater)
            currentWater += waterFillSpeed * Time.deltaTime;

        if (!isBeingWatered && currentWater > 0)
            currentWater -= 1.0f * Time.deltaTime;

        currentWater = Mathf.Clamp(currentWater, 0, maxWater);

        if (currentPlant != null && !isDead)
        {
            if (debugControlGrowth)
                currentGrowth = debugGrowthValue;
            else
                ProcessGrowth();

            UpdateVisualPhase();
        }

        UpdateSoilVisual();
    }

    // ---------------------------
    // СЕЛЕКЦИЯ
    // ---------------------------

    int GetGrowthStage()
    {
        if (currentGrowth < 25f) return 1;
        if (currentGrowth < 75f) return 2;
        return 3;
    }

    // true = донор тратится сразу
    // false = донор не тратится сейчас
    bool TryCrossbreed(PlantInstance donor, PlayerInventory inventory)
    {
        if (currentPlantInstance == null)
        {
            Debug.LogWarning("❌ У текущего растения нет PlantInstance!");
            return false;
        }

        if (donor == null)
        {
            Debug.LogWarning("❌ Донор пустой!");
            return false;
        }

        if (donor.family != currentPlantInstance.family)
        {
            Debug.Log("❌ Разные семейства!");
            return false;
        }

        int donorStage = donorStageDebug;
        int targetStage = GetGrowthStage();

        float chance = 0f;
        int min = 0;
        int max = 0;

        if (donorStage == 2 && targetStage == 2)
        {
            chance = 1f;
            min = 1;
            max = 3;
        }
        else if (donorStage == 2 && targetStage == 3)
        {
            chance = 0.8f;
            min = 3;
            max = 4;
        }
        else if (donorStage == 3 && targetStage == 2)
        {
            chance = 0.2f;
            min = 3;
            max = 4;
        }
        else if (donorStage == 3 && targetStage == 3)
        {
            chance = 0.03f;
            min = 5;
            max = 6;
        }
        else
        {
            Debug.Log("❌ Нельзя скрещивать на этой стадии.");
            return false;
        }

        if (Random.value > chance)
        {
            Debug.Log("💀 Растение погибло при селекции!");
            Die();

            // донор потрачен, потому что попытка была
            return true;
        }

        if (GeneSelectionUI.instance == null)
        {
            Debug.LogError("❌ На сцене нет GeneSelectionUI! Создай UI-панель и повесь на неё GeneSelectionUI.");
            return false;
        }

        GeneSelectionUI.instance.Open(currentPlantInstance, donor, min, max, inventory);

        Debug.Log($"✅ Успех! Открыт UI выбора генов. Диапазон бонуса: +{min}...+{max}");

        // донор НЕ тратим сейчас. Он потратится после выбора гена в GeneSelectionUI
        return false;
    }

    // ---------------------------
    // БАЗОВАЯ ЛОГИКА
    // ---------------------------

    void ProcessGrowth()
    {
        if (currentGrowth >= 100f) return;

        float tempDiff = Mathf.Abs(currentTemperature - currentPlant.optimalTemp);
        if (tempDiff > currentPlant.tempRange)
            currentHealth -= 5f * Time.deltaTime;

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
        currentGrowth = Mathf.Clamp(currentGrowth, 0, 100f);

        if (currentHealth <= 0)
            Die();
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
        if (spawnedVisual != null)
            Destroy(spawnedVisual);

        if (prefab == null) return;

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        spawnedVisual = Instantiate(prefab, origin.position, origin.rotation);
    }

    void UpdateSoilVisual()
    {
        if (soilStates == null || soilStates.Length == 0) return;

        int targetIndex = -1;

        for (int i = soilStates.Length - 1; i >= 0; i--)
        {
            if (currentWater >= soilStates[i].minWater)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex != currentSoilIndex)
        {
            currentSoilIndex = targetIndex;

            if (targetIndex != -1)
            {
                if (baseSoilModel != null)
                    baseSoilModel.SetActive(false);

                SpawnSoil(soilStates[targetIndex].prefab);
            }
            else
            {
                if (baseSoilModel != null)
                    baseSoilModel.SetActive(true);

                if (currentSoilVisual != null)
                    Destroy(currentSoilVisual);
            }
        }
    }

    void SpawnSoil(GameObject prefab)
    {
        if (currentSoilVisual != null)
            Destroy(currentSoilVisual);

        if (prefab == null) return;

        currentSoilVisual = Instantiate(prefab, soilSpawnPoint);
        currentSoilVisual.transform.localPosition = Vector3.zero;
        currentSoilVisual.transform.localRotation = Quaternion.identity;
        currentSoilVisual.transform.localScale = Vector3.one;
    }

    void Plant(PlantData newData)
    {
        currentPlant = newData;
        currentPlantInstance = new PlantInstance(newData);

        currentGrowth = 0f;
        currentHealth = 100f;
        isDead = false;
        currentPhaseIndex = -1;

        Debug.Log($"🌱 Посажено: {currentPlant.itemName}");
    }

    void Die()
    {
        isDead = true;
        currentPhaseIndex = -1;
        Debug.Log("💀 Погибло!");

        if (currentPlant != null)
            SpawnVisual(currentPlant.deadPrefab);
    }

    void ClearPot()
    {
        if (spawnedVisual != null)
            Destroy(spawnedVisual);

        spawnedVisual = null;
        currentPlant = null;
        currentPlantInstance = null;
        isDead = false;
        currentPhaseIndex = -1;
    }

    private void Harvest(PlayerInventory inventory)
    {
        if (currentPlant == null) return;

        if (currentPlant.harvestResult == null)
        {
            Debug.LogWarning($"У {currentPlant.itemName} не настроен harvestResult!");
            ClearPot();
            return;
        }

        PlantInstance savedGenes = currentPlantInstance;

        if (savedGenes == null)
            savedGenes = new PlantInstance(currentPlant);

        int amountToAdd = Mathf.Max(1, currentPlant.harvestAmount);
        int totalLeftover = 0;

        for (int i = 0; i < amountToAdd; i++)
        {
            totalLeftover += inventory.AddItem(currentPlant.harvestResult, 1, savedGenes);
        }

        if (totalLeftover == 0)
        {
            Debug.Log($"🌾 Собран урожай с генами: {currentPlant.harvestResult.itemName}");
            ClearPot();
        }
        else
        {
            Debug.Log("❌ Инвентарь полон! Урожай не собран полностью.");
        }
    }

    // ---------------------------
    // ВЗАИМОДЕЙСТВИЕ
    // ---------------------------

    public PlantActionType Interact(PlayerInventory inventory)
    {
        if (inventory == null) return PlantActionType.None;

        if (isDead)
        {
            ClearPot();
            return PlantActionType.Clear;
        }

        InventorySlot activeSlot = inventory.GetSelectedSlot();
        if (activeSlot == null) return PlantActionType.None;

        // 100% = урожай
        if (currentPlant != null && currentGrowth >= 100f)
        {
            Harvest(inventory);
            return PlantActionType.Harvest;
        }

        // Пустая рука + растение не готово = выкопать растение с генами
        if (currentPlant != null && currentGrowth < 100f && activeSlot.IsEmpty)
        {
            PlantInstance savedPlant = currentPlantInstance;

            if (savedPlant == null)
                savedPlant = new PlantInstance(currentPlant);

            int leftover = inventory.AddItem(currentPlant, 1, savedPlant);

            if (leftover == 0)
            {
                Debug.Log("🌱 Растение выкопано и добавлено в инвентарь:");
                Debug.Log(savedPlant.activeGenes.Count + " active / " + savedPlant.dormantGenes.Count + " dormant");

                ClearPot();
                return PlantActionType.Clear;
            }

            Debug.Log("❌ Инвентарь полон, растение не выкопано.");
            return PlantActionType.None;
        }

        if (activeSlot.IsEmpty)
            return PlantActionType.None;

        if (activeSlot.item.isTool && activeSlot.item.toolType == ToolType.WateringCan)
            return PlantActionType.Water;

        // В руке растение + в горшке растение = селекция
        if (currentPlant != null && activeSlot.item is PlantData)
        {
            PlantInstance donor = activeSlot.plantInstance;

            if (donor == null)
                donor = new PlantInstance((PlantData)activeSlot.item);

            bool consumeDonorNow = TryCrossbreed(donor, inventory);

            if (consumeDonorNow)
                inventory.ConsumeSelectedItem();

            return PlantActionType.Plant;
        }

        // Пустой горшок + в руке растение = посадка
        if (currentPlant == null && activeSlot.item is PlantData)
        {
            Plant((PlantData)activeSlot.item);
            inventory.ConsumeSelectedItem();
            return PlantActionType.Plant;
        }

        return PlantActionType.None;
    }

    public PlantActionType GetAvailableAction(PlayerInventory inventory)
    {
        if (isDead) return PlantActionType.Clear;
        if (inventory == null) return PlantActionType.None;

        InventorySlot activeSlot = inventory.GetSelectedSlot();
        if (activeSlot == null) return PlantActionType.None;

        // 100% = урожай
        if (currentPlant != null && currentGrowth >= 100f)
            return PlantActionType.Harvest;

        // Неготовое растение + пустая рука = выкопка
        if (currentPlant != null && currentGrowth < 100f && activeSlot.IsEmpty)
            return PlantActionType.Clear;

        if (activeSlot.IsEmpty)
            return PlantActionType.None;

        if (activeSlot.item.isTool && activeSlot.item.toolType == ToolType.WateringCan)
            return PlantActionType.Water;

        if (activeSlot.item is PlantData)
            return PlantActionType.Plant;

        return PlantActionType.None;
    }
}