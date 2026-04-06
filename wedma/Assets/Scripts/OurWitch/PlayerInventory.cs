using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;

    public void Clear()
    {
        item = null;
        count = 0;
    }
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Интерфейс (UI)")]
    public InventoryUI ui; // Ссылка на скрипт отрисовки меню

    [Header("Слоты инвентаря")]
    public InventorySlot[] slots = new InventorySlot[21]; // 15 ячеек всего
    public int selectedHotbarIndex = 0; // Активный слот (от 0 до 4)

    void Awake()
    {
        for (int i = 0; i < slots.Length; i++) slots[i] = new InventorySlot();
    }

    public int AddItem(ItemData data, int amount)
    {
        if (!data.isTool)
        {
            foreach (var slot in slots)
            {
                if (slot.item == data && slot.count < data.maxStack)
                {
                    int spaceLeft = data.maxStack - slot.count;
                    if (amount <= spaceLeft)
                    {
                        slot.count += amount;
                        UpdateUI();
                        return 0;
                    }
                    else
                    {
                        slot.count += spaceLeft;
                        amount -= spaceLeft;
                    }
                }
            }
        }

        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.item = data;
                slot.count = amount;
                UpdateUI();
                return 0;
            }
        }

        return amount;
    }

    public InventorySlot GetSelectedSlot()
    {
        return slots[selectedHotbarIndex];
    }

    public void ConsumeSelectedItem()
    {
        InventorySlot active = GetSelectedSlot();
        if (!active.IsEmpty && !active.item.isTool)
        {
            active.count--;
            if (active.count <= 0) active.Clear();
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

    // --- НОВАЯ ЛОГИКА: Меняем предметы местами (Drag & Drop) ---
    public void SwapSlots(int index1, int index2)
    {
        // 1. Меняем данные в "карманах"
        InventorySlot temp = slots[index1];
        slots[index1] = slots[index2];
        slots[index2] = temp;

        // 2. Обновляем картинки на экране
        UpdateUI();

        // 3. Обновляем визуал в руке Ведьмы (если перетащили то, что держали в руках)
        WitchInteraction witch = GetComponent<WitchInteraction>();
        if (witch != null) witch.UpdateHandVisuals();
    }

    // Тот самый потерянный метод!
    private void UpdateUI()
    {
        if (ui != null) ui.UpdateAllSlots();
    }
}