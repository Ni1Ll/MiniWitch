using UnityEngine;

public class WitchInteraction : MonoBehaviour
{
    [Header("Предметы в руке (перетащи сюда объекты из иерархии)")]
    public GameObject seedsModelInHand;      // Ссылка на выключенную модель семян в руке
    public GameObject wateringCanModelInHand;// Ссылка на выключенную модель лейки в руке

    [Header("Настройки")]
    public float pickupRadius = 2.0f; // Радиус, в котором ищем предметы
    public ItemType currentItem = ItemType.None; // Что сейчас у нас есть

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // 1. Создаем невидимую сферу вокруг игрока и получаем список всего, что в нее попало
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius);

        foreach (var hit in hitColliders)
        {
            // Проверяем: Это предмет (Item)?
            if (hit.CompareTag("Item"))
            {
                PickupItem itemScript = hit.GetComponent<PickupItem>();
                if (itemScript != null)
                {
                    PickUp(itemScript.itemType, hit.gameObject); // Подбираем
                    return; // Прерываем цикл, чтобы не подобрать 10 предметов за раз
                }
            }
            // Проверяем: Это грядка (Pot)?
            else if (hit.CompareTag("Pot"))
            {
                PlantPot potScript = hit.GetComponent<PlantPot>();
                if (potScript != null)
                {
                    potScript.Interact(currentItem); // Взаимодействуем
                    return;
                }
            }
        }
    }

    void PickUp(ItemType type, GameObject groundObject)
    {
        // 1. Сначала прячем всё, что было в руках до этого
        if (seedsModelInHand) seedsModelInHand.SetActive(false);
        if (wateringCanModelInHand) wateringCanModelInHand.SetActive(false);

        // 2. Запоминаем новый предмет
        currentItem = type;

        // 3. Включаем нужную модельку в руке
        if (type == ItemType.Seeds && seedsModelInHand != null)
        {
            seedsModelInHand.SetActive(true);
        }
        else if (type == ItemType.WateringCan && wateringCanModelInHand != null)
        {
            wateringCanModelInHand.SetActive(true);
        }

        Debug.Log($"Подобрал: {type}");

        // 4. Удаляем предмет с земли
        Destroy(groundObject);
    }

    // Рисуем сферу в редакторе, чтобы ты видела радиус подбора
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}