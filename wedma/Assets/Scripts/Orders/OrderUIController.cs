using UnityEngine;

public class OrderUIController : MonoBehaviour
{
    public OrderManager orderManager;      // Ссылка на менеджер заказов
    public Transform listParent;           // Content внутри ScrollView
    public GameObject listItemPrefab;      // Префаб кнопки OrderListItem
    public OrderDetailsUI detailsUI;       // Панель с деталями заказа справа

    void OnEnable()
    {
        if (orderManager != null && listItemPrefab != null && listParent != null)
        {
            BuildList();
        }
    }

    public void BuildList()
    {
        if (orderManager == null)
        {
            Debug.LogWarning("[OrderUIController] OrderManager не назначен.");
            return;
        }

        if (listParent == null)
        {
            Debug.LogWarning("[OrderUIController] ListParent не назначен.");
            return;
        }

        if (listItemPrefab == null)
        {
            Debug.LogWarning("[OrderUIController] ListItemPrefab не назначен.");
            return;
        }

        Debug.Log($"Попытка создать список. Заказов в менеджере: {orderManager.orders.Count}");

        // Очищаем старые кнопки
        foreach (Transform child in listParent)
        {
            Destroy(child.gameObject);
        }

        if (orderManager.orders.Count == 0)
        {
            Debug.LogWarning("Список заказов пуст! Спавнить нечего.");
            return;
        }

        // Создаём кнопку для каждого заказа
        foreach (Order order in orderManager.orders)
        {
            if (order == null) continue;

            GameObject go = Instantiate(listItemPrefab, listParent);
            Debug.Log($"Создан объект для заказа #{order.id}");

            OrderListItem item = go.GetComponent<OrderListItem>();

            if (item != null)
            {
                item.Init(order, OnOrderSelected);
            }
            else
            {
                Debug.LogWarning("[OrderUIController] На prefab нет OrderListItem.");
            }
        }
    }

    // Срабатывает при нажатии на кнопку заказа
    void OnOrderSelected(Order order)
    {
        if (order == null) return;

        // Показываем подробности
        if (detailsUI != null)
        {
            detailsUI.Show(order);
        }

        // Заказ считается прочитанным, когда его раскрыли
        if (orderManager != null)
        {
            orderManager.MarkOrderAsRead(order);
        }
    }
}