using UnityEngine;

public class CauldronWater : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PickupItem droppedItem = other.GetComponent<PickupItem>();

        if (droppedItem != null && droppedItem.itemData != null)
        {
            // Зелья обратно не варим
            if (droppedItem.itemData is PotionData)
            {
                return;
            }

            // Вызываем наш новый метод из котла. 
            // Он сам найдет свободный слот и сам проверит, не собрался ли рецепт.
            bool success = Cauldron.instance.TryPutIngredient(droppedItem.itemData);

            if (success)
            {
                Debug.Log("Бульк! В котел растворился: " + droppedItem.itemData.itemName);

                // Уничтожаем 3D-объект, так как данные предмета уже внутри котла
                Destroy(other.gameObject);
            }
            else
            {
                // Если TryPutIngredient вернул false, значит все 3 слота заняты
                Debug.Log("Котел уже полон!");
            }
        }
    }
}