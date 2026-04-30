using UnityEngine;

public class OrderUIController : MonoBehaviour
{
    public OrderManager orderManager; // Ссылка на менеджер заказов
    public Transform listParent;    // Объект Content внутри ScrollView
    public GameObject listItemPrefab; // Префаб кнопки OrderListItem
    public OrderDetailsUI detailsUI; // Панель с деталями заказа справа

    void OnEnable()
    {
        // Каждый раз, когда открываешь панель, список будет пересобираться актуально
        if (orderManager != null && listItemPrefab != null)
        {
            BuildList();
        }
    }

    public void BuildList()
    {
        Debug.Log($"Попытка создать список. Заказов в менеджере: {orderManager.orders.Count}");
        // Очищаем старые кнопки, если они были
        foreach (Transform child in listParent)
        {
            Destroy(child.gameObject);
        }
        if (orderManager.orders.Count == 0)
        {
            Debug.LogWarning("Список заказов пуст! Спавнить нечего.");
            return;
        }

        // Создаем новую кнопку для каждого заказа
        foreach (var order in orderManager.orders) //[cite: 6]
        {
            GameObject go = Instantiate(listItemPrefab, listParent); //[cite: 7]
            Debug.Log($"Создан объект для заказа #{order.id}");
            OrderListItem item = go.GetComponent<OrderListItem>(); //[cite: 7]

            if (item != null)
            {
                item.Init(order, OnOrderSelected); //[cite: 5, 7]
            }
        }
    }

    // Метод, который сработает при нажатии на кнопку заказа
    void OnOrderSelected(Order order)
    {
        if (detailsUI != null)
        {
            detailsUI.Show(order); // Показываем подробности в окне рядом[cite: 4, 7]
        }
    }
}