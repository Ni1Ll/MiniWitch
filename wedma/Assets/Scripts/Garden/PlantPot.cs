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
        // 1. Температура (читаем из файла!)
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

        if (inventory.hasWateringCan)
        {
            currentWater = maxWater;
            Debug.Log("Полито!");
            return; 
        }

        if (currentPlant == null && inventory.HasSeeds())
        {
            PlantData seedData = inventory.RemoveSeeds();

            Plant(seedData);
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
        Debug.Log($"Посажена: {currentPlant.plantName}");
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