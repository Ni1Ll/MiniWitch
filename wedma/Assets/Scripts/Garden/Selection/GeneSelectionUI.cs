using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GeneSelectionUI : MonoBehaviour
{
    public static GeneSelectionUI instance;

    [Header("Root")]
    public GameObject panel;

    [Header("Предложенные гены донора")]
    public Button[] offeredButtons = new Button[5];
    public TextMeshProUGUI[] offeredTexts = new TextMeshProUGUI[5];

    [Header("Спящие гены основного растения")]
    public Button[] targetButtons = new Button[5];
    public TextMeshProUGUI[] targetTexts = new TextMeshProUGUI[5];

    private PlantInstance targetPlant;
    private PlayerInventory inventory;

    private List<Gene> offeredGenes = new List<Gene>();
    private int selectedOfferedIndex = -1;

    void Awake()
    {
        instance = this;

        if (panel != null)
            panel.SetActive(false);

        for (int i = 0; i < offeredButtons.Length; i++)
        {
            int index = i;
            if (offeredButtons[i] != null)
                offeredButtons[i].onClick.AddListener(() => SelectOfferedGene(index));
        }

        for (int i = 0; i < targetButtons.Length; i++)
        {
            int index = i;
            if (targetButtons[i] != null)
                targetButtons[i].onClick.AddListener(() => SelectTargetGene(index));
        }
    }

    public void Open(PlantInstance target, PlantInstance donor, int minBonus, int maxBonus, PlayerInventory playerInventory)
    {
        if (target == null || donor == null)
        {
            Debug.LogWarning("[GeneSelectionUI] Нет target или donor.");
            return;
        }

        targetPlant = target;
        inventory = playerInventory;
        selectedOfferedIndex = -1;

        offeredGenes.Clear();

        // Берём 5 спящих генов донора и превращаем их в предложения с бонусом селекции
        for (int i = 0; i < donor.dormantGenes.Count && offeredGenes.Count < 5; i++)
        {
            Gene donorGene = donor.dormantGenes[i];
            int value = Random.Range(minBonus, maxBonus + 1);

            offeredGenes.Add(new Gene(donorGene.type, value));
        }

        if (panel != null)
            panel.SetActive(true);

        RefreshUI();
    }

    void RefreshUI()
    {
        // Левая сторона — предложенные гены
        for (int i = 0; i < offeredButtons.Length; i++)
        {
            bool hasGene = i < offeredGenes.Count;

            if (offeredButtons[i] != null)
                offeredButtons[i].interactable = hasGene;

            if (offeredTexts[i] != null)
            {
                if (hasGene)
                {
                    Gene g = offeredGenes[i];
                    string marker = i == selectedOfferedIndex ? "▶ " : "";
                    offeredTexts[i].text = $"{marker}{g.type} +{g.value}";
                }
                else
                {
                    offeredTexts[i].text = "-";
                }
            }
        }

        // Правая сторона — спящие гены основного растения
        for (int i = 0; i < targetButtons.Length; i++)
        {
            bool hasGene = targetPlant != null &&
                           targetPlant.dormantGenes != null &&
                           i < targetPlant.dormantGenes.Count;

            if (targetButtons[i] != null)
                targetButtons[i].interactable = hasGene;

            if (targetTexts[i] != null)
            {
                if (hasGene)
                {
                    Gene g = targetPlant.dormantGenes[i];
                    targetTexts[i].text = $"{g.type} +{g.value}";
                }
                else
                {
                    targetTexts[i].text = "-";
                }
            }
        }
    }

    void SelectOfferedGene(int index)
    {
        if (index < 0 || index >= offeredGenes.Count)
            return;

        selectedOfferedIndex = index;
        RefreshUI();

        Debug.Log($"[GeneSelectionUI] Выбран донорский ген: {offeredGenes[index].type} +{offeredGenes[index].value}");
    }

    void SelectTargetGene(int index)
    {
        if (selectedOfferedIndex < 0)
        {
            Debug.Log("Сначала выбери донорский ген слева.");
            return;
        }

        if (targetPlant == null || index < 0 || index >= targetPlant.dormantGenes.Count)
            return;

        Gene offered = offeredGenes[selectedOfferedIndex];
        Gene target = targetPlant.dormantGenes[index];

        if (target.type == offered.type)
        {
            target.value += offered.value;
            target.value = Mathf.Clamp(target.value, 1, 10);

            Debug.Log($"🧬 Одинаковый ген: {target.type}. Сумма стала +{target.value}");
        }
        else
        {
            Debug.Log($"🔁 Замена гена: {target.type} +{target.value} → {offered.type} +{offered.value}");

            target.type = offered.type;
            target.value = Mathf.Clamp(offered.value, 1, 10);
        }

        if (inventory != null)
            inventory.ConsumeSelectedItem();

        Close();
    }

    public void Close()
    {
        selectedOfferedIndex = -1;
        targetPlant = null;
        inventory = null;
        offeredGenes.Clear();

        if (panel != null)
            panel.SetActive(false);
    }
}