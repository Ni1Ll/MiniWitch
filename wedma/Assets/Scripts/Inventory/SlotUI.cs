using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TextMeshProUGUI countText;
    public GameObject highlight;

    [HideInInspector] public int slotIndex;
    private InventoryUI uiManager;
    private InventorySlot currentSlot;

    // Вызывается при старте из InventoryUI
    public void Setup(int index, InventoryUI ui)
    {
        slotIndex = index;
        uiManager = ui;
    }

    public void UpdateSlot(InventorySlot slot, bool isSelected)
    {
        currentSlot = slot;

        if (highlight != null) highlight.SetActive(isSelected);

        if (slot == null || slot.IsEmpty)
        {
            icon.sprite = null;
            icon.enabled = false;
            if (countText != null) countText.text = "";
            return;
        }

        icon.sprite = slot.item.icon;
        icon.enabled = true;

        if (countText != null)
        {
            countText.text = slot.item.isTool ? "" : slot.count.ToString();
        }
    }

    // --- ПОДСКАЗКИ ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentSlot != null && !currentSlot.IsEmpty) Tooltip.instance.ShowTooltip(currentSlot.item.itemName);
    }
    public void OnPointerExit(PointerEventData eventData) => Tooltip.instance.HideTooltip();

    // --- ПЕРЕТАСКИВАНИЕ ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Нельзя тащить пустой слот
        if (currentSlot == null || currentSlot.IsEmpty) return;
        uiManager.StartDrag(slotIndex, currentSlot.item.icon);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (uiManager.isDragging) uiManager.UpdateDragPosition(Input.mousePosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        uiManager.StopDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Если бросили предмет на этот слот — меняем их местами
        if (uiManager.isDragging)
        {
            uiManager.playerInventory.SwapSlots(uiManager.draggedSlotIndex, slotIndex);
        }
    }
}