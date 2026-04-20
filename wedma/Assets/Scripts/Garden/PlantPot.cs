using UnityEngine;

public class PlantPot : MonoBehaviour
{
    [Header("Состояние")]
    public float currentWater = 0f;
    public float maxWater = 100f;
    public float currentTemperature = 20f; // Температура грядки

    [Header("Кто здесь живет?")]
    public PlantData currentPlant; // Ссылка на файл (какое растение посажено)

    // Внутренние переменные
    private GameObject spawnedVisual;
    public float currentGrowth = 0f;
    public float currentHealth = 100f;
    public bool isDead = false;

    void Update()
    {
        if (currentPlant != null && !isDead)
        {
            ProcessGrowth();
            UpdateVisualSize();
        }
    }

    void ProcessGrowth()
    {
        if (currentGrowth >= 100f) return;

        // 1. Температура
        float tempDiff = Mathf.Abs(currentTemperature - currentPlant.optimalTemp);
        if (tempDiff > currentPlant.tempRange) currentHealth -= 5f * Time.deltaTime;

        // 2. Вода
        if (currentWater > 0)
        {
            currentWater -= currentPlant.waterConsumption * Time.deltaTime;
            currentGrowth += currentPlant.growthSpeed * Time.deltaTime;
            currentHealth += 5f * Time.deltaTime;
        }
        else
        {
            currentHealth -= 10f * Time.deltaTime;
        }

        currentWater = Mathf.Clamp(currentWater, 0, maxWater);
        currentGrowth = Mathf.Clamp(currentGrowth, 0, 100);

        if (currentHealth <= 0) Die();
    }

    public void Interact(PlayerInventory inventory)
    {
        if (isDead)
        {
            ClearPot();
            return;
        }

        // --- НОВАЯ ЛОГИКА: СБОР УРОЖАЯ ---
        // Если на грядке что-то есть и оно выросло на 100%
        if (currentPlant != null && currentGrowth >= 100f)
        {
            Harvest(inventory);
            return; // Завершаем взаимодействие, чтобы не сработал полив или посадка
        }

        InventorySlot activeSlot = inventory.GetSelectedSlot();
        // Если руки пусты и сбор не сработал выше — ничего не делаем
        if (activeSlot.IsEmpty) return;

        // 1. Поливаем (инструментом)
        if (activeSlot.item.isTool)
        {
            currentWater = maxWater;
            Debug.Log("Полито!");
            return;
        }

        // 2. Сажаем
        if (currentPlant == null && activeSlot.item is PlantData)
        {
            PlantData seedData = (PlantData)activeSlot.item;
            Plant(seedData);
            inventory.ConsumeSelectedItem();
        }
    }

    private void Harvest(PlayerInventory inventory)
    {
        if (currentPlant.harvestResult != null)
        {
            // Пытаемся добавить предмет в инвентарь
            int leftover = inventory.AddItem(currentPlant.harvestResult, currentPlant.harvestAmount);

            if (leftover == 0)
            {
                Debug.Log($"Собрано: {currentPlant.harvestResult.itemName}");
                ClearPot(); // Грядка снова пуста и готова к посадке
            }
            else
            {
                // Если leftover > 0, значит инвентарь забился
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
        currentPlant = newData; // Запомнили файл

        // Создаем визуал
        if (currentPlant.healthyPrefab != null)
        {
            if (spawnedVisual != null) Destroy(spawnedVisual);
            spawnedVisual = Instantiate(currentPlant.healthyPrefab, transform);
            spawnedVisual.transform.localPosition = Vector3.zero;
            spawnedVisual.transform.localRotation = Quaternion.identity;
        }

        currentGrowth = 0f;
        currentHealth = 100f;
        isDead = false;
        Debug.Log($"Посажена: {currentPlant.itemName}");
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Погибло!");

        if (spawnedVisual != null) Destroy(spawnedVisual);

        if (currentPlant.deadPrefab != null)
        {
            spawnedVisual = Instantiate(currentPlant.deadPrefab, transform);
            spawnedVisual.transform.localPosition = Vector3.zero;
        }
    }

    void ClearPot()
    {
        if (spawnedVisual != null) Destroy(spawnedVisual);
        currentPlant = null;
        isDead = false;
    }

    void UpdateVisualSize()
    {
        if (spawnedVisual != null)
        {
            float percent = currentGrowth / 100f;
            float h = Mathf.Max(0.1f, percent * 3.0f);
            spawnedVisual.transform.localScale = new Vector3(0.3f, h, 0.3f);
        }
    }
}