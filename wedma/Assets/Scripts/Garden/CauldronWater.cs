using UnityEngine;

public class CauldronWater : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PickupItem droppedItem = other.GetComponent<PickupItem>();

        if (droppedItem != null && droppedItem.itemData != null)
        {
            for (int i = 0; i < 2; i++)
            {
                if (Cauldron.instance.slots[i].IsEmpty)
                {
                    // Записываем данные предмета в память котла
                    Cauldron.instance.slots[i].item = droppedItem.itemData;
                    Cauldron.instance.slots[i].count = 1;

                    Debug.Log("Бульк! В котел растворился: " + droppedItem.itemData.itemName);

                    // Уничтожаем физический префаб (он утонул и растворился)
                    Destroy(other.gameObject);

                    // Дергаем котел, чтобы он проверил рецепт!
                    Cauldron.instance.UpdateUI();
                    return; // Выходим из цикла, чтобы предмет не лег сразу в оба слота
                }
            }

            Debug.Log("Котел уже полон!");

        }
    }
}