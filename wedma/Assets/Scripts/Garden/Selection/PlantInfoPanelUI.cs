using UnityEngine;
using TMPro;

public class PlantInfoPanelUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;

    [Header("Panel")]
    public GameObject panel;

    [Header("Texts")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI familyText;
    public TextMeshProUGUI activeGeneText1;
    public TextMeshProUGUI activeGeneText2;
    public TextMeshProUGUI[] dormantGeneTexts = new TextMeshProUGUI[5];

    // --- НОВОЕ: Запоминаем цветок со стола ---
    private PlantInstance fixedTargetPlant;

    void Update()
    {
        // Если мы рассматриваем цветок на столе, игнорируем инвентарь!
        if (fixedTargetPlant != null) return;

        if (inventory == null) return;
        InventorySlot slot = inventory.GetSelectedSlot();

        if (slot == null || slot.IsEmpty || slot.plantInstance == null)
        {
            ClearTexts();
            return;
        }

        UpdateVisuals(slot.plantInstance, slot.item.itemName);
    }

    // Метод, который вызывает Генетический Стол
    public void Refresh(PlantInstance plant, string plantName)
    {
        fixedTargetPlant = plant;

        if (plant == null)
        {
            ClearTexts();
            return;
        }

        UpdateVisuals(plant, plantName);
    }

    // Метод, который вызывает Стол при закрытии (чтобы снова читать инвентарь)
    public void ClearDisplayTarget()
    {
        fixedTargetPlant = null;
    }

    private void UpdateVisuals(PlantInstance plant, string plantName)
    {
        if (titleText != null) titleText.text = plantName;
        if (familyText != null) familyText.text = plant.family.ToString();

        if (activeGeneText1 != null)
            activeGeneText1.text = plant.activeGenes != null && plant.activeGenes.Count > 0
                ? $"{GeneName.Ru(plant.activeGenes[0].type)} +{plant.activeGenes[0].value}"
                : "Пусто";

        if (activeGeneText2 != null)
            activeGeneText2.text = plant.activeGenes != null && plant.activeGenes.Count > 1
                ? $"{GeneName.Ru(plant.activeGenes[1].type)} +{plant.activeGenes[1].value}"
                : "Пусто";

        for (int i = 0; i < dormantGeneTexts.Length; i++)
        {
            if (dormantGeneTexts[i] == null) continue;

            dormantGeneTexts[i].text =
                (plant.dormantGenes != null && i < plant.dormantGenes.Count)
                ? $"{GeneName.Ru(plant.dormantGenes[i].type)} +{plant.dormantGenes[i].value}"
                : "-";
        }
    }

    void ClearTexts()
    {
        if (titleText != null) titleText.text = "Нет растения";
        if (familyText != null) familyText.text = "-";
        if (activeGeneText1 != null) activeGeneText1.text = "-";
        if (activeGeneText2 != null) activeGeneText2.text = "-";

        for (int i = 0; i < dormantGeneTexts.Length; i++)
            if (dormantGeneTexts[i] != null) dormantGeneTexts[i].text = "-";
    }
}