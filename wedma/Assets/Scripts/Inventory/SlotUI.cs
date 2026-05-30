using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TextMeshProUGUI countText;
    public GameObject highlight;

    [Header("��������� �����")]
    public bool isResultSlot = false; // ������� ������ (������ ������ �������� �������)

    [HideInInspector] public int slotIndex;
    public InventorySlot currentSlot; // ������� ���������, ����� ����� ����� ��������

    public static SlotUI draggedSlot; // ���������� ������: ����� ���� �� ������ �����

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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSlot == null || currentSlot.IsEmpty) return;

        draggedSlot = this; 
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

        bool isPointerOverUI = eventData.pointerEnter != null && eventData.pointerEnter.GetComponent<SlotUI>() != null;

        if (!isPointerOverUI && currentSlot != null && !currentSlot.IsEmpty && !isResultSlot)
        {
            Camera activeCam = Camera.main;
            if (activeCam == null) activeCam = FindFirstObjectByType<Camera>();

            if (activeCam != null && currentSlot.item.dropPrefab != null)
            {
                float dropDistance = 1.7f;
                Ray ray = activeCam.ScreenPointToRay(Input.mousePosition);
                Vector3 spawnPos = ray.GetPoint(dropDistance);

                if (GeneticTable.isTableOpen && !GeneticTable.hasPlantOnPlate)
                {
                    if (GeneticTable.instance != null)
                    {
                        GeneticTable.instance.AnimatePlantDrop(currentSlot.item, currentSlot.plantInstance);
                    }

                    currentSlot.count--;
                    if (currentSlot.count <= 0) currentSlot.Clear();
                    if (InventoryUI.instance != null) InventoryUI.instance.UpdateAllSlots();

                    draggedSlot = null;
                    return;
                }

                if (GeneticTable.isTableOpen && GeneticTable.hasPlantOnPlate)
                {
                    draggedSlot = null;
                    return;
                }

                if (GeneticTable.isTableOpen && GeneticTable.hasPlantOnPlate)
                {
                    Debug.Log("На тарелке уже есть цветок. Бросок отменен.");
                    draggedSlot = null;
                    return;
                }

                GameObject worldDroppedObj = Instantiate(currentSlot.item.dropPrefab, spawnPos, Quaternion.identity);

                PickupItem worldPickup = worldDroppedObj.GetComponent<PickupItem>();
                if (worldPickup == null) worldPickup = worldDroppedObj.AddComponent<PickupItem>();
                worldPickup.itemData = currentSlot.item;

                currentSlot.count--;
                if (currentSlot.count <= 0) currentSlot.Clear();
                if (InventoryUI.instance != null) InventoryUI.instance.UpdateAllSlots();
            }
        }

        draggedSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot != null && draggedSlot != this && this.currentSlot != null && !this.isResultSlot)
        {
            ItemData tempItem = this.currentSlot.item;
            int tempCount = this.currentSlot.count;

            this.currentSlot.item = draggedSlot.currentSlot.item;
            this.currentSlot.count = draggedSlot.currentSlot.count;

            draggedSlot.currentSlot.item = tempItem;
            draggedSlot.currentSlot.count = tempCount;

            // ����������� ���� �������� ����������� �������� ��������
            if (InventoryUI.instance != null) InventoryUI.instance.UpdateAllSlots();

            // ��������� ������ � ���� ������
            WitchInteraction witch = FindFirstObjectByType<WitchInteraction>();
            if (witch != null) witch.UpdateHandVisuals();
        }
    }
}