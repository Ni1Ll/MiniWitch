using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    // Данные растения: гены и т.д.
    public PlantInstance plantInstance;

    public bool IsEmpty => item == null || count <= 0;

    public void Clear()
    {
        item = null;
        count = 0;
        plantInstance = null;
    }

    public string GetPlantInfo()
    {
        Debug.Log("ITEM: " + item);
        Debug.Log("INSTANCE: " + plantInstance);

        if (plantInstance == null)
            return "Генов нет";

        if (plantInstance.activeGenes == null || plantInstance.activeGenes.Count == 0 ||
            plantInstance.dormantGenes == null || plantInstance.dormantGenes.Count == 0)
        {
            return "Генов нет";
        }

        string info = "=== ГЕНЫ ===\n";

        info += "Активные:\n";
        foreach (var g in plantInstance.activeGenes)
        {
            info += $"{GeneName.Ru(g.type)} +{g.value}\n";
        }

        info += "\nСпящие:\n";
        foreach (var g in plantInstance.dormantGenes)
        {
            info += $"{GeneName.Ru(g.type)} +{g.value}\n";
        }

        return info;
    }
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Интерфейс (UI)")]
    public InventoryUI ui;

    [Header("Слоты инвентаря")]
    public InventorySlot[] slots = new InventorySlot[21];

    [Header("Хотбар")]
    public int selectedHotbarIndex = 0;

    void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new InventorySlot();
        }
    }

    // ---------------------------
    // ОБЫЧНЫЕ ПРЕДМЕТЫ / СВЕЖИЕ РАСТЕНИЯ БЕЗ ГЕНОВ
    // ---------------------------
    public int AddItem(ItemData data, int amount)
    {
        if (data == null) return amount;

        // Если в инвентарь попадает PlantData без PlantInstance,
        // создаём гены прямо здесь.
        if (data is PlantData plantData)
        {
            return AddItem(data, amount, new PlantInstance(plantData));
        }

        if (!data.isTool)
        {
            foreach (var slot in slots)
            {
                // Стакаем только обычные предметы без PlantInstance
                if (!slot.IsEmpty &&
                    slot.item == data &&
                    slot.count < data.maxStack &&
                    slot.plantInstance == null)
                {
                    int spaceLeft = data.maxStack - slot.count;

                    if (amount <= spaceLeft)
                    {
                        slot.count += amount;
                        UpdateUI();
                        return 0;
                    }

                    slot.count += spaceLeft;
                    amount -= spaceLeft;
                }
            }
        }

        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.item = data;
                slot.count = amount;
                slot.plantInstance = null;

                UpdateUI();
                return 0;
            }
        }

        return amount;
    }

    // ---------------------------
    // РАСТЕНИЯ / УРОЖАЙ С ГЕНАМИ
    // ---------------------------
    public int AddItem(ItemData data, int amount, PlantInstance instance)
    {
        if (data == null) return amount;

        // Если это PlantData, но instance пустой/битый/без генов —
        // создаём нормальный PlantInstance.
        if (data is PlantData plantData && !PlantInstanceHasGenes(instance))
        {
            instance = new PlantInstance(plantData);
        }

        // 1. Сначала пробуем положить в выбранный хотбар-слот
        InventorySlot selectedSlot = GetSelectedSlot();

        if (selectedSlot != null && selectedSlot.IsEmpty)
        {
            selectedSlot.item = data;
            selectedSlot.count = 1;
            selectedSlot.plantInstance = ClonePlant(instance);

            UpdateUI();
            return 0;
        }

        // 2. Потом пробуем остальные хотбар-слоты 0-4
        for (int i = 0; i <= 4 && i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].item = data;
                slots[i].count = 1;
                slots[i].plantInstance = ClonePlant(instance);

                UpdateUI();
                return 0;
            }
        }

        // 3. Потом весь остальной инвентарь
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.item = data;
                slot.count = 1;
                slot.plantInstance = ClonePlant(instance);

                UpdateUI();
                return 0;
            }
        }

        return amount;
    }

    public InventorySlot GetSelectedSlot()
    {
        if (slots == null || slots.Length == 0) return null;

        selectedHotbarIndex = Mathf.Clamp(selectedHotbarIndex, 0, Mathf.Min(4, slots.Length - 1));
        return slots[selectedHotbarIndex];
    }

    public void ConsumeSelectedItem()
    {
        InventorySlot active = GetSelectedSlot();

        if (active != null && !active.IsEmpty && !active.item.isTool)
        {
            active.count--;

            if (active.count <= 0)
                active.Clear();

            UpdateUI();
        }
    }

    public void ChangeSelectedSlot(int direction)
    {
        selectedHotbarIndex += direction;

        if (selectedHotbarIndex > 4) selectedHotbarIndex = 0;
        if (selectedHotbarIndex < 0) selectedHotbarIndex = 4;

        UpdateUI();
    }

    // Drag & Drop
    public void SwapSlots(int index1, int index2)
    {
        if (index1 < 0 || index1 >= slots.Length) return;
        if (index2 < 0 || index2 >= slots.Length) return;

        InventorySlot temp = slots[index1];
        slots[index1] = slots[index2];
        slots[index2] = temp;

        UpdateUI();

        WitchInteraction witch = GetComponent<WitchInteraction>();
        if (witch != null) witch.UpdateHandVisuals();
    }

    private bool PlantInstanceHasGenes(PlantInstance instance)
    {
        return instance != null &&
               instance.baseData != null &&
               instance.activeGenes != null &&
               instance.dormantGenes != null &&
               instance.activeGenes.Count > 0 &&
               instance.dormantGenes.Count > 0;
    }

    private PlantInstance ClonePlant(PlantInstance original)
    {
        if (!PlantInstanceHasGenes(original))
            return null;

        PlantInstance clone = new PlantInstance(original.baseData);

        clone.activeGenes.Clear();
        clone.dormantGenes.Clear();

        foreach (var g in original.activeGenes)
        {
            clone.activeGenes.Add(new Gene(g.type, g.value));
        }

        foreach (var g in original.dormantGenes)
        {
            clone.dormantGenes.Add(new Gene(g.type, g.value));
        }

        return clone;
    }

    private void UpdateUI()
    {
        if (ui != null) ui.UpdateAllSlots();
    }
}