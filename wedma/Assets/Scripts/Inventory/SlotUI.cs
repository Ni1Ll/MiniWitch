using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TextMeshProUGUI countText;
    public GameObject highlight;

    [Header("Настройки Котла")]
    public bool isResultSlot = false; // Галочка защиты (нельзя класть предметы вручную)

    [HideInInspector] public int slotIndex;
    public InventorySlot currentSlot; // Сделали публичным, чтобы слоты могли общаться

    public static SlotUI draggedSlot; // Глобальная память: какой слот мы сейчас тащим

    public void Setup(int index, InventoryUI ui)
    {
        slotIndex = index;
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
        if (countText != null) countText.text = slot.item.isTool ? "" : slot.count.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentSlot != null && !currentSlot.IsEmpty) Tooltip.instance.ShowTooltip(currentSlot.item.itemName);
    }
    public void OnPointerExit(PointerEventData eventData) => Tooltip.instance.HideTooltip();

    // --- УНИВЕРСАЛЬНОЕ ПЕРЕТАСКИВАНИЕ ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Нельзя тащить пустоту или готовое зелье из слота результата
        if (currentSlot == null || currentSlot.IsEmpty) return;

        draggedSlot = this; // Запоминаем самих себя в глобальную переменную
        if (InventoryUI.instance != null) InventoryUI.instance.StartDrag(currentSlot.item.icon);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InventoryUI.instance != null && InventoryUI.instance.isDragging)
            InventoryUI.instance.UpdateDragPosition(Input.mousePosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (InventoryUI.instance != null) InventoryUI.instance.StopDrag();
        draggedSlot = null; // Очищаем память, когда отпустили мышку
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot != null && draggedSlot != this && this.currentSlot != null && !this.isResultSlot)
        {
            // МАГИЯ ОБМЕНА ДАННЫМИ (без индексов и проверок инвентарей)
            ItemData tempItem = this.currentSlot.item;
            int tempCount = this.currentSlot.count;

            this.currentSlot.item = draggedSlot.currentSlot.item;
            this.currentSlot.count = draggedSlot.currentSlot.count;

            draggedSlot.currentSlot.item = tempItem;
            draggedSlot.currentSlot.count = tempCount;

            // Приказываем ВСЕМ открытым интерфейсам обновить картинки
            if (InventoryUI.instance != null) InventoryUI.instance.UpdateAllSlots();
            if (Cauldron.instance != null && Cauldron.isCauldronOpen) Cauldron.instance.UpdateUI();

            // Обновляем визуал в руке Ведьмы
            WitchInteraction witch = FindFirstObjectByType<WitchInteraction>();
            if (witch != null) witch.UpdateHandVisuals();
        }
    }
}