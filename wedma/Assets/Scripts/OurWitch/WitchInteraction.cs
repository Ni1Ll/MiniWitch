using UnityEngine;

public class WitchInteraction : MonoBehaviour
{
    [Header("Настройки")]
    public float interactionRadius = 2.5f;

    [Header("Ссылки на ПРЕФАБЫ (ОБЯЗАТЕЛЬНО!)")]
    // То, что создается на земле, когда выбрасываем
    public GameObject seedsPickupPrefab;
    public GameObject wateringCanPickupPrefab;

    [Header("Визуал в Руке")]
    public Transform handSocket;              // Пустышка в ладони (куда крепим)
    public GameObject wateringCanModelInHand; // Моделька лейки в руке (если есть)

    // Внутренние ссылки
    private PlayerInventory inventory;
    private GameObject currentSpawnedSeedModel; // Текущая моделька в руке

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        if (inventory == null) inventory = gameObject.AddComponent<PlayerInventory>();

        UpdateHandVisuals();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) TryInteract();
        if (Input.GetKeyDown(KeyCode.G)) DropItem();
    }

    void TryInteract()
    {
        // Ищем всё вокруг
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
        bool foundSomething = false;

        // 1. Сначала ищем ГРЯДКИ
        foreach (var hit in hits)
        {
            PlantPot pot = hit.GetComponent<PlantPot>();
            if (pot != null)
            {
                pot.Interact(inventory); // Грядка сама заберет/даст что надо
                UpdateHandVisuals();
                return;
            }
        }

        // 2. Если грядок нет, ищем ПРЕДМЕТЫ
        foreach (var hit in hits)
        {
            PickupItem item = hit.GetComponent<PickupItem>();
            if (item != null)
            {
                TryPickUp(item);
                foundSomething = true;
                return; // Берем только один предмет за раз
            }
        }

        if (!foundSomething) Debug.Log("Рядом ничего нет (пусто).");
    }

    void TryPickUp(PickupItem item)
    {
        bool success = false;

        if (item.isWateringCan)
        {
            success = inventory.AddWateringCan();
        }
        else if (item.plantData != null)
        {
            success = inventory.AddSeeds(item.plantData);
        }
        else
        {
            Debug.LogError($"ОШИБКА: У предмета '{item.name}' нет галочки Лейки и нет PlantData!");
        }

        if (success)
        {
            Debug.Log($"Подобрали: {item.name}");
            Destroy(item.gameObject); // Удаляем с земли
            UpdateHandVisuals();      // Обновляем руку
        }
    }

    void DropItem()
    {
        Vector3 dropPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;

        // --- ВЫБРАСЫВАЕМ ЛЕЙКУ ---
        if (inventory.hasWateringCan)
        {
            if (wateringCanPickupPrefab == null)
            {
                Debug.LogError("СТОП! Не могу выбросить лейку: В поле 'Watering Can Pickup Prefab' пусто!");
                return;
            }

            Instantiate(wateringCanPickupPrefab, dropPos, Quaternion.identity);
            inventory.RemoveWateringCan();
            Debug.Log("Лейка выброшена.");
        }
        // --- ВЫБРАСЫВАЕМ СЕМЕНА ---
        else if (inventory.HasSeeds())
        {
            if (seedsPickupPrefab == null)
            {
                Debug.LogError("СТОП! Не могу выбросить семена: В поле 'Seeds Pickup Prefab' пусто!");
                return;
            }

            // 1. Забираем данные
            PlantData dataToDrop = inventory.RemoveSeeds();

            // 2. Создаем мешочек
            GameObject newBag = Instantiate(seedsPickupPrefab, dropPos, Quaternion.identity);

            // 3. Передаем данные мешочку
            var pickupScript = newBag.GetComponent<PickupItem>();
            if (pickupScript != null)
            {
                pickupScript.plantData = dataToDrop;
                // pickupScript.isWateringCan = false; // На всякий случай
            }

            Debug.Log($"Выброшены семена: {dataToDrop.plantName}");
        }

        UpdateHandVisuals();
    }

    public void UpdateHandVisuals()
    {
        // 1. Очистка: Удаляем созданную модельку семян
        if (currentSpawnedSeedModel != null)
        {
            Destroy(currentSpawnedSeedModel);
            currentSpawnedSeedModel = null;
        }

        // 2. Очистка: Выключаем лейку
        if (wateringCanModelInHand) wateringCanModelInHand.SetActive(false);

        // 3. Включение ЛЕЙКИ
        if (inventory.hasWateringCan)
        {
            if (wateringCanModelInHand) wateringCanModelInHand.SetActive(true);
        }
        // 4. Включение СЕМЯН
        else if (inventory.HasSeeds())
        {
            PlantData data = inventory.currentPlantData;

            // Проверки на ошибки
            if (handSocket == null)
            {
                Debug.LogError("ОШИБКА ВИЗУАЛА: Не назначено поле 'Hand Socket' (пустышка в руке)!");
                return;
            }
            if (data.handVisualPrefab == null)
            {
                Debug.LogWarning($"ПРЕДУПРЕЖДЕНИЕ: У растения '{data.plantName}' нет префаба для руки (Hand Visual Prefab)!");
                return;
            }

            // Создаем модельку
            currentSpawnedSeedModel = Instantiate(data.handVisualPrefab, handSocket);
            currentSpawnedSeedModel.transform.localPosition = Vector3.zero;
            currentSpawnedSeedModel.transform.localRotation = Quaternion.identity;
        }
    }
}