using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance; // Делаем скрипт доступным отовсюду

    [Header("Ссылки")]
    public PlayerInventory playerInventory;
    public GameObject mainInventoryPanel;

    [Header("Слоты интерфейса")]
    public SlotUI[] hotbarSlots;
    public SlotUI[] allSlots;

    [Header("Перетаскивание (Drag & Drop)")]
    public Image dragIcon;
    [HideInInspector] public bool isDragging = false;

    void Awake()
    {
        instance = this; // Записываем себя в память при старте
    }

    void Start()
    {
        if (dragIcon != null) dragIcon.gameObject.SetActive(false);

        for (int i = 0; i < hotbarSlots.Length; i++) hotbarSlots[i].Setup(i, this);
        for (int i = 0; i < allSlots.Length; i++) allSlots[i].Setup(i + hotbarSlots.Length, this);

        if (mainInventoryPanel != null) mainInventoryPanel.SetActive(false);
        UpdateAllSlots();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !Cauldron.isCauldronOpen) ToggleInventory();
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

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            bool isSelected = (i == playerInventory.selectedHotbarIndex);
            hotbarSlots[i].UpdateSlot(playerInventory.slots[i], isSelected);
        }

        if (mainInventoryPanel != null && mainInventoryPanel.activeSelf)
        {
            for (int i = 0; i < allSlots.Length; i++)
            {
                int realIndex = i + hotbarSlots.Length;
                allSlots[i].UpdateSlot(playerInventory.slots[realIndex], false);
            }
        }
    }

    // --- УПРОЩЕННАЯ ЛОГИКА ПЕРЕТАСКИВАНИЯ ---
    public void StartDrag(Sprite icon)
    {
        isDragging = true;
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
        if (dragIcon != null) dragIcon.gameObject.SetActive(false);
    }
}