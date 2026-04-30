using UnityEngine;
using TMPro; // Нужно для текста
using UnityEngine.UI; // Нужно, если будешь обращаться к кнопке

public class OrderListItem : MonoBehaviour
{
    // Перетащи сюда объект текста из префаба в инспекторе
    public TextMeshProUGUI orderInfoText; 

    private Order order;
    private System.Action<Order> onClick;

    public void Init(Order order, System.Action<Order> onClick)
    {
        this.order = order; //[cite: 3]
        this.onClick = onClick; //

        if (orderInfoText != null)
        {
            // Устанавливаем текст: номер заказа и имя клиента
            orderInfoText.text = $"Заказ #{order.id}: {order.customerName}"; //[cite: 3]
        }
    }

    // Этот метод привяжи к кнопке (On Click) в Unity
    public void OnClick()
    {
        if (onClick != null)
        {
            Debug.Log("Invoke");
            onClick.Invoke(order); //[cite: 5, 7]
        }
    }
}