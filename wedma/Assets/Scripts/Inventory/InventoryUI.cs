using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Ссылки")]
    public PlayerInventory playerInventory;
    public GameObject mainInventoryPanel;

    [Header("Слоты интерфейса")]
    public SlotUI[] hotbarSlots;
    public SlotUI[] allSlots;

    [Header("Перетаскивание (Drag & Drop)")]
    public Image dragIcon; // Летающая иконка
    [HideInInspector] public bool isDragging = false;
    [HideInInspector] public int draggedSlotIndex = -1;

    void Start()
    {
        // Прячем летающую иконку при старте
        if (dragIcon != null) dragIcon.gameObject.SetActive(false);

        // Раздаем слотам их порядковые номера (Хотбар: 0-4, Главное меню: 0-14)
        for (int i = 0; i < hotbarSlots.Length; i++) hotbarSlots[i].Setup(i, this);
        for (int i = 0; i < allSlots.Length; i++) allSlots[i].Setup(i, this);

        if (mainInventoryPanel != null) mainInventoryPanel.SetActive(false);
        UpdateAllSlots();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ToggleInventory();
    }

    void ToggleInventory()
    {
        if (mainInventoryPanel != null)
        {
            mainInventoryPanel.SetActive(!mainInventoryPanel.activeSelf);
            UpdateAllSlots();
        }
    }

    public void UpdateAllSlots()
    {
        if (playerInventory == null) return;

        // Обновляем Хотбар (всегда)
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            bool isSelected = (i == playerInventory.selectedHotbarIndex);
            hotbarSlots[i].UpdateSlot(playerInventory.slots[i], isSelected);
        }

        // Обновляем Главное меню (если открыто)
        if (mainInventoryPanel != null && mainInventoryPanel.activeSelf)
        {
            for (int i = 0; i < allSlots.Length; i++)
            {
                bool isSelected = (i == playerInventory.selectedHotbarIndex);
                allSlots[i].UpdateSlot(playerInventory.slots[i], isSelected);
            }
        }
    }

    // --- ЛОГИКА ПЕРЕТАСКИВАНИЯ ---
    public void StartDrag(int index, Sprite icon)
    {
        isDragging = true;
        draggedSlotIndex = index;
        if (dragIcon != null)
        {
            dragIcon.sprite = icon;
            dragIcon.gameObject.SetActive(true);
        }
    }

    public void UpdateDragPosition(Vector2 pos)
    {
        if (dragIcon != null) dragIcon.transform.position = pos;
    }

    public void StopDrag()
    {
        isDragging = false;
        draggedSlotIndex = -1;
        if (dragIcon != null) dragIcon.gameObject.SetActive(false);
    }
}