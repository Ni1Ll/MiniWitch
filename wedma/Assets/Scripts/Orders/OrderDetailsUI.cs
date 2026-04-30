using TMPro;
using UnityEngine;

public class OrderDetailsUI : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI effectsText;
    public TextMeshProUGUI priceText;

    public void Show(Order order)
    {
        title.text = $"Order #{order.id} ({order.customerName})";
        priceText.text = order.price.ToString();

        effectsText.text = string.Join(", ", order.recipe.requiredEffects);
    }
}