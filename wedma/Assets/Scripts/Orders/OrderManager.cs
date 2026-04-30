using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public List<Order> orders = new List<Order>();
    public OrderUIController uiController; // Добавь эту ссылку

    private int nextId = 1;

    void Start()
    {
        GenerateTestOrders(5);//[cite: 6]
        
        // Теперь, когда заказы точно созданы, просим UI обновиться
        if (uiController != null)
        {
            uiController.BuildList();//[cite: 7]
        }
    }

    public void GenerateTestOrders(int count)
    {
        for (int i = 0; i < count; i++)
        {
            orders.Add(CreateRandomOrder());
        }
    }

    private Order CreateRandomOrder()
    {
        return new Order
        {
            id = nextId++,
            customerName = "NPC_" + Random.Range(1, 100),
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
            EffectType randomEffect = (EffectType)Random.Range(0, System.Enum.GetValues(typeof(EffectType)).Length);

            if (!effects.Contains(randomEffect))
                effects.Add(randomEffect);
        }

        return new Recipe { requiredEffects = effects };
    }
}