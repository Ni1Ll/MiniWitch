using UnityEngine;
using TMPro;

public class HandGeneDebugUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;
    public TextMeshProUGUI text;

    void Update()
    {
        if (text == null)
            return;

        if (inventory == null)
        {
            text.text = "Нет инвентаря";
            return;
        }

        InventorySlot slot = inventory.GetSelectedSlot();

        if (slot == null || slot.IsEmpty)
        {
            text.text = "В руке: пусто";
            return;
        }

        if (slot.item == null)
        {
            text.text = "В руке: предмет отсутствует";
            return;
        }

        string result = $"В руке: {slot.item.itemName}\n";

        if (slot.plantInstance == null)
        {
            result += "\nГенов нет";
            text.text = result;
            return;
        }

        result += "\nАКТИВНЫЕ:\n";
        if (slot.plantInstance.activeGenes != null)
        {
            foreach (var g in slot.plantInstance.activeGenes)
                result += $"{GeneName.Ru(g.type)} +{g.value}\n";
        }

        result += "\nСПЯЩИЕ:\n";
        if (slot.plantInstance.dormantGenes != null)
        {
            foreach (var g in slot.plantInstance.dormantGenes)
                result += $"{GeneName.Ru(g.type)} +{g.value}\n";
        }

        text.text = result;
    }
}