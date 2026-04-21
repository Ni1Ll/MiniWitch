using UnityEngine;

public class CauldronWater : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PickupItem droppedItem = other.GetComponent<PickupItem>();

        if (droppedItem != null && droppedItem.itemData != null)
        {

            if (droppedItem.itemData is PotionData)
            {
                return; // Прерываем код, вода ничего не делает
            }

            for (int i = 0; i < 2; i++)
            {
                if (Cauldron.instance.slots[i].IsEmpty)
                {
                    Cauldron.instance.slots[i].item = droppedItem.itemData;
                    Cauldron.instance.slots[i].count = 1;

                    Debug.Log("Бульк! В котел растворился: " + droppedItem.itemData.itemName);

                    Destroy(other.gameObject);

                    Cauldron.instance.UpdateUI();
                    return;
                }
            }

            Debug.Log("Котел уже полон!");
        }
    }
}