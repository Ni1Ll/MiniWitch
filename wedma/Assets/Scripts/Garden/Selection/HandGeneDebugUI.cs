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
            text.text = "No inventory";
            return;
        }

        InventorySlot slot = inventory.GetSelectedSlot();

        if (slot == null || slot.IsEmpty)
        {
            text.text = "Hand: Empty";
            return;
        }

        if (slot.item == null)
        {
            text.text = "Hand: Missing item";
            return;
        }

        string result = $"Hand: {slot.item.itemName}\n";

        // Инструменты, зелья, обычные предметы без генов
        if (slot.plantInstance == null)
        {
            result += "\nNo genes";
            text.text = result;
            return;
        }

        result += "\nACTIVE:\n";
        if (slot.plantInstance.activeGenes != null)
        {
            foreach (var g in slot.plantInstance.activeGenes)
                result += $"{g.type} +{g.value}\n";
        }

        result += "\nDORMANT:\n";
        if (slot.plantInstance.dormantGenes != null)
        {
            foreach (var g in slot.plantInstance.dormantGenes)
                result += $"{g.type} +{g.value}\n";
        }

        text.text = result;
    }
}