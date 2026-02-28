using UnityEngine;

public class WitchInteraction : MonoBehaviour
{
    [Header("Настройки")]
    public float pickupRadius = 2f;

    [Header("Инвентарь")]
    public PlantData currentPlantData; // Какие семена держим (ссылка на файл)
    public bool hasWateringCan = false;

    [Header("Визуал рук")]
    public GameObject seedsBagVisual;
    public GameObject wateringCanVisual;

    void Start() { UpdateHandVisuals(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) TryInteract();
        if (Input.GetKeyDown(KeyCode.G)) DropItem();
    }

    void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);

        // 1. Грядки
        foreach (var hit in hits)
        {
            PlantPot pot = hit.GetComponent<PlantPot>();
            if (pot != null)
            {
                pot.Interact(this); // Передаем СЕБЯ (ведьму) грядке
                return;
            }
        }

        // 2. Предметы
        foreach (var hit in hits)
        {
            PickupItem item = hit.GetComponent<PickupItem>();
            if (item != null)
            {
                PickUp(item);
                return;
            }
        }
    }

    void PickUp(PickupItem item)
    {
        if (currentPlantData != null || hasWateringCan) return; // Руки заняты

        if (item.isWateringCan)
        {
            hasWateringCan = true;
            Debug.Log("Взяла лейку");
        }
        else if (item.plantData != null)
        {
            currentPlantData = item.plantData; // Берем данные из предмета!
            Debug.Log($"Взяла семена: {item.plantData.plantName}");
        }

        Destroy(item.gameObject);
        UpdateHandVisuals();
    }

    void DropItem()
    {
        if (!hasWateringCan && currentPlantData == null) return;

        // Тут по-хорошему нужно создавать префаб обратно на земле,
        // но пока просто очистим руки для теста.
        hasWateringCan = false;
        currentPlantData = null;

        UpdateHandVisuals();
        Debug.Log("Выбросила предмет");
    }

    public void UpdateHandVisuals()
    {
        if (seedsBagVisual) seedsBagVisual.SetActive(false);
        if (wateringCanVisual) wateringCanVisual.SetActive(false);

        if (hasWateringCan && wateringCanVisual) wateringCanVisual.SetActive(true);
        else if (currentPlantData != null && seedsBagVisual) seedsBagVisual.SetActive(true);
    }
}