using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GeneSelectionUI : MonoBehaviour
{
    public static GeneSelectionUI instance;

    [Header("Root")]
    public GameObject panel;

    [Header("Close")]
    public Button closeButton;

    [Header("Предложенные гены")]
    public Button[] offeredButtons = new Button[5];
    public TextMeshProUGUI[] offeredTexts = new TextMeshProUGUI[5];

    [Header("Спящие гены основного растения")]
    public Button[] targetButtons = new Button[5];
    public TextMeshProUGUI[] targetTexts = new TextMeshProUGUI[5];

    private PlantInstance targetPlant;
    private PlantInstance donorPlant;
    private PlayerInventory inventory;

    private List<Gene> offeredGenes = new List<Gene>();
    private bool[] offeredUsed = new bool[5];

    private int selectedOfferedIndex = -1;
    private bool hasAppliedAnyChange = false;

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

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseAndApply);
    }

    public void Open(PlantInstance target, PlantInstance donor, int minBonus, int maxBonus, PlayerInventory playerInventory)
    {
        if (target == null)
        {
            Debug.LogWarning("[GeneSelectionUI] Target plant is null.");
            return;
        }

        if (donor == null)
        {
            Debug.LogWarning("[GeneSelectionUI] Donor plant is null.");
            return;
        }

        targetPlant = target;
        donorPlant = donor;
        inventory = playerInventory;

        selectedOfferedIndex = -1;
        hasAppliedAnyChange = false;

        for (int i = 0; i < offeredUsed.Length; i++)
            offeredUsed[i] = false;

        GenerateOfferedGenesFromFamilies(minBonus, maxBonus);

        if (panel != null)
            panel.SetActive(true);

        RefreshUI();
    }

    private void GenerateOfferedGenesFromFamilies(int minBonus, int maxBonus)
    {
        offeredGenes.Clear();

        List<GeneType> pool = new List<GeneType>();

        // 1. Гены семейства основного растения
        if (GeneticsCore.familyGenes.ContainsKey(targetPlant.family))
        {
            foreach (GeneType gene in GeneticsCore.familyGenes[targetPlant.family])
            {
                if (!pool.Contains(gene))
                    pool.Add(gene);
            }
        }

        // 2. Гены семейства донора
        if (GeneticsCore.familyGenes.ContainsKey(donorPlant.family))
        {
            foreach (GeneType gene in GeneticsCore.familyGenes[donorPlant.family])
            {
                if (!pool.Contains(gene))
                    pool.Add(gene);
            }
        }

        // 3. Убираем активные гены основного растения из предложений
        if (targetPlant.activeGenes != null)
        {
            foreach (Gene active in targetPlant.activeGenes)
                pool.Remove(active.type);
        }

        // 4. Перемешиваем
        for (int i = 0; i < pool.Count; i++)
        {
            int rand = Random.Range(i, pool.Count);

            GeneType temp = pool[i];
            pool[i] = pool[rand];
            pool[rand] = temp;
        }

        // 5. Берём 5 предложений
        int count = Mathf.Min(5, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int value = Random.Range(minBonus, maxBonus + 1);
            offeredGenes.Add(new Gene(pool[i], value));
        }

        Debug.Log($"[GeneSelectionUI] Предложено {offeredGenes.Count} генов из {targetPlant.family} + {donorPlant.family}");
    }

    private void RefreshUI()
    {
        // Левая сторона — предложенные гены
        for (int i = 0; i < offeredButtons.Length; i++)
        {
            bool hasGene = i < offeredGenes.Count;
            bool usable = hasGene && !offeredUsed[i];

            if (offeredButtons[i] != null)
                offeredButtons[i].interactable = usable;

            if (offeredTexts[i] != null)
            {
                if (hasGene)
                {
                    Gene gene = offeredGenes[i];

                    string marker = i == selectedOfferedIndex ? "▶ " : "";
                    string usedText = offeredUsed[i] ? " (USED)" : "";

                    offeredTexts[i].text = $"{marker}{gene.type} +{gene.value}{usedText}";
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
            bool hasGene =
                targetPlant != null &&
                targetPlant.dormantGenes != null &&
                i < targetPlant.dormantGenes.Count;

            if (targetButtons[i] != null)
                targetButtons[i].interactable = hasGene;

            if (targetTexts[i] != null)
            {
                if (hasGene)
                {
                    Gene gene = targetPlant.dormantGenes[i];
                    targetTexts[i].text = $"{gene.type} +{gene.value}";
                }
                else
                {
                    targetTexts[i].text = "-";
                }
            }
        }
    }

    private void SelectOfferedGene(int index)
    {
        if (index < 0 || index >= offeredGenes.Count)
            return;

        if (offeredUsed[index])
            return;

        selectedOfferedIndex = index;

        Gene selected = offeredGenes[index];
        Debug.Log($"[GeneSelectionUI] Выбран предложенный ген: {selected.type} +{selected.value}");

        RefreshUI();
    }

    private void SelectTargetGene(int index)
    {
        if (selectedOfferedIndex < 0)
        {
            Debug.Log("Сначала выбери предложенный ген слева.");
            return;
        }

        if (targetPlant == null || targetPlant.dormantGenes == null)
            return;

        if (index < 0 || index >= targetPlant.dormantGenes.Count)
            return;

        if (selectedOfferedIndex >= offeredGenes.Count)
            return;

        if (offeredUsed[selectedOfferedIndex])
            return;

        Gene offered = offeredGenes[selectedOfferedIndex];

        // Ищем такой же ген среди спящих генов основного растения
        Gene existingSameGene = targetPlant.dormantGenes.Find(g => g.type == offered.type);

        // -------------------------------
        // 1. ЕСЛИ ТАКОЙ ГЕН УЖЕ ЕСТЬ — СУММИРУЕМ
        // -------------------------------
        if (existingSameGene != null)
        {
            int oldValue = existingSameGene.value;

            existingSameGene.value += offered.value;
            existingSameGene.value = Mathf.Clamp(existingSameGene.value, 1, 10);

            Debug.Log($"🧬 Ген уже есть: {existingSameGene.type}. Было +{oldValue}, добавили +{offered.value}, стало +{existingSameGene.value}");

            // Только при суммировании донорская кнопка используется
            offeredUsed[selectedOfferedIndex] = true;

            // Снимаем выбор с левой кнопки
            selectedOfferedIndex = -1;

            hasAppliedAnyChange = true;

            RefreshUI();
            return;
        }

        // -------------------------------
        // 2. ЕСЛИ ТАКОГО ГЕНА НЕТ — МЕНЯЕМ МЕСТАМИ
        // -------------------------------
        Gene target = targetPlant.dormantGenes[index];

        Debug.Log($"🔁 Свап: LEFT {offered.type} +{offered.value} ↔ RIGHT {target.type} +{target.value}");

        // сохраняем старый правый ген
        Gene oldTargetGene = new Gene(target.type, target.value);

        // правый слот получает предложенный ген
        target.type = offered.type;
        target.value = Mathf.Clamp(offered.value, 1, 10);

        // левая кнопка получает старый правый ген
        offeredGenes[selectedOfferedIndex] = oldTargetGene;

        // ВАЖНО:
        // кнопку НЕ деактивируем
        // selectedOfferedIndex оставляем выбранным,
        // чтобы можно было сразу поменять обратно

        hasAppliedAnyChange = true;

        RefreshUI();
    }

    public void CloseAndApply()
    {
        // Донор тратим один раз, только если игрок реально сделал хотя бы одну замену/суммирование
        if (hasAppliedAnyChange && inventory != null)
        {
            inventory.ConsumeSelectedItem();
            Debug.Log("[GeneSelectionUI] Выбор завершён. Донор потрачен.");
        }
        else
        {
            Debug.Log("[GeneSelectionUI] Закрыто без изменений. Донор не потрачен.");
        }

        Close();
    }

    public void Close()
    {
        selectedOfferedIndex = -1;
        hasAppliedAnyChange = false;

        targetPlant = null;
        donorPlant = null;
        inventory = null;

        offeredGenes.Clear();

        for (int i = 0; i < offeredUsed.Length; i++)
            offeredUsed[i] = false;

        if (panel != null)
            panel.SetActive(false);
    }
}