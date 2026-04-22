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

    public void UpdateUIVisibility()
    {
        bool isCauldronActive = Cauldron.isCauldronOpen;

        bool showHotbar = isInventoryOpen || isCauldronActive;
        if (hotbarPanel != null) hotbarPanel.SetActive(showHotbar);

        if (mainInventoryPanel != null) mainInventoryPanel.SetActive(isInventoryOpen);

        if (tooltipPanel != null) tooltipPanel.SetActive(showHotbar);

        if (showHotbar)
        {
            UpdateAllSlots();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
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