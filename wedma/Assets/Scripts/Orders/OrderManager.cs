using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Orders")]
    public List<Order> orders = new List<Order>();

    [Header("UI")]
    public OrderUIController uiController;

    [Header("Courier")]
    [Tooltip("Объект с SmartFollower, который должен идти за заказом")]
    public SmartFollower courier;

    [Header("Marker3D")]
    [Tooltip("Объект, на котором висит Marker3d. Обычно это курьер")]
    public Marker3d orderMarker;

    [Tooltip("Индекс дочернего маркера внутри MarkerRoot. Обычно 0")]
    public int markerIndex = 0;

    [Header("Input")]
    public KeyCode generateOrderKey = KeyCode.U;
    public int ordersPerPress = 1;

    [Header("Debug")]
    public bool generateOrdersOnStart = false;
    public int startOrderCount = 5;

    private int nextId = 1;

    void Start()
    {
        if (generateOrdersOnStart)
        {
            GenerateTestOrders(startOrderCount);
        }

        RefreshUI();

        // На старте маркер курьера скрыт.
        HideOrderMarker();
    }

    void Update()
    {
        if (Input.GetKeyDown(generateOrderKey))
        {
            GenerateTestOrders(ordersPerPress);

            RefreshUI();

            // ВАЖНО:
            // Маркер тут НЕ включаем.
            // Курьер сам включит маркер, когда будет ReturnWithOrder.

            if (courier != null)
            {
                courier.AcceptOrder();
                Debug.Log("🚶 Курьер отправлен за заказом.");
            }
            else
            {
                Debug.LogWarning("[OrderManager] Courier не назначен.");
            }

            Debug.Log($"📦 Создано заказов: {ordersPerPress}. Всего заказов: {orders.Count}");
        }
    }

    public void GenerateTestOrders(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Order newOrder = CreateRandomOrder();
            orders.Add(newOrder);

            Debug.Log($"🆕 Новый заказ #{newOrder.id} от {newOrder.customerName}");
        }

        RefreshUI();
    }

    private Order CreateRandomOrder()
    {
        return new Order
        {
            id = nextId++,
            customerName = "NPC_" + Random.Range(1, 100),

            // true = новый / непрочитанный
            isPending = true,

            itemType = (ItemType)Random.Range(0, 2),
            recipe = GenerateRandomRecipe(),
            price = Random.Range(50, 200)
        };
    }

    private Recipe GenerateRandomRecipe()
    {
        int count = Random.Range(2, 4);

        List<EffectType> effects = new List<EffectType>();

        for (int i = 0; i < count; i++)
        {
            EffectType randomEffect =
                (EffectType)Random.Range(0, System.Enum.GetValues(typeof(EffectType)).Length);

            if (!effects.Contains(randomEffect))
                effects.Add(randomEffect);
        }

        return new Recipe
        {
            requiredEffects = effects
        };
    }

    // Вызывается, когда игрок нажал на заказ и открыл его детали
    public void MarkOrderAsRead(Order order)
    {
        if (order == null)
            return;

        if (order.isPending)
        {
            order.isPending = false;
            Debug.Log($"✅ Заказ #{order.id} прочитан.");
        }

        RefreshUI();

        // Если непрочитанных заказов больше нет — убираем маркер с курьера.
        if (!HasUnreadOrders())
        {
            HideOrderMarker();
        }
    }

    public bool HasUnreadOrders()
    {
        foreach (Order order in orders)
        {
            if (order != null && order.isPending)
                return true;
        }

        return false;
    }

    public void HideOrderMarker()
    {
        if (orderMarker == null)
        {
            Debug.LogWarning("[OrderManager] Order Marker не назначен.");
            return;
        }

        orderMarker.DisableMarker(markerIndex);
        Debug.Log("🔕 Маркер заказа выключен.");
    }

    public void RefreshUI()
    {
        if (uiController != null)
        {
            uiController.BuildList();
        }
    }
}