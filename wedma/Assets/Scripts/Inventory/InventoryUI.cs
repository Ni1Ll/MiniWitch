using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance;

    [Header("Панели")]
    public GameObject hotbarPanel;
    public GameObject mainInventoryPanel;
    public GameObject tooltipPanel;

    [Header("Ссылки")]
    public PlayerInventory playerInventory;
    public SlotUI[] hotbarSlots;
    public SlotUI[] allSlots;
    public Image dragIcon;

    private bool isInventoryOpen = false; // Состояние "открыто на Q"
    private bool isMechanicActive = false; // Состояние "открыт котел"

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (dragIcon != null) dragIcon.gameObject.SetActive(false);

        for (int i = 0; i < hotbarSlots.Length; i++) hotbarSlots[i].Setup(i, this);
        for (int i = 0; i < allSlots.Length; i++) allSlots[i].Setup(i + hotbarSlots.Length, this);

        UpdateUIVisibility();
    }

    void Update()
    {
        // Теперь Q работает ВСЕГДА (даже если котел открыт)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isInventoryOpen = !isInventoryOpen;
            UpdateUIVisibility();
        }
    }

    public void SetMechanicMode(bool isActive)
    {
        isMechanicActive = isActive;
        // При закрытии котла, если инвентарь на Q не был открыт, всё закроется
        UpdateUIVisibility();
    }

    private void UpdateUIVisibility()
    {
        // 1. ХОТБАР показываем, если нажат Q ИЛИ если мы у котла
        bool showHotbar = isInventoryOpen || isMechanicActive;
        if (hotbarPanel != null) hotbarPanel.SetActive(showHotbar);

        // 2. ОСНОВНОЕ МЕНЮ показываем ТОЛЬКО если нажат Q
        if (mainInventoryPanel != null) mainInventoryPanel.SetActive(isInventoryOpen);

        // 3. ТУЛТИПЫ показываем, если хоть что-то открыто
        if (tooltipPanel != null) tooltipPanel.SetActive(showHotbar || isInventoryOpen);

        // Управление курсором
        if (showHotbar || isInventoryOpen)
        {
            UpdateAllSlots();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // Закрываем всё, только если и котел закрыт, и Q не нажат
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            StopDrag();
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

        // Обновляем ячейки основного инвентаря только если он активен
        if (mainInventoryPanel != null && mainInventoryPanel.activeSelf)
        {
            for (int i = 0; i < allSlots.Length; i++)
            {
                int realIndex = i + hotbarSlots.Length;
                allSlots[i].UpdateSlot(playerInventory.slots[realIndex], false);
            }
        }
    }

    public void StartDrag(Sprite icon)
    {
        if (dragIcon == null) return;
        dragIcon.sprite = icon;
        dragIcon.gameObject.SetActive(true);
        isDragging = true;
    }

    public void UpdateDragPosition(Vector2 pos) => dragIcon.transform.position = pos;
    public void StopDrag() { if (dragIcon) dragIcon.gameObject.SetActive(false); isDragging = false; }
    [HideInInspector] public bool isDragging = false;
}