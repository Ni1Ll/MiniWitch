using UnityEngine;

public class WitchInteraction : MonoBehaviour
{
    public Animator animator;
    [Header("���������")]
    public float interactionRadius = 2.5f;

    [Header("������ � ����")]
    public Transform handSocket;

    private PlayerInventory inventory;
    private GameObject currentSpawnedModel;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        UpdateHandVisuals();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) TryInteract();
        if (Input.GetKeyDown(KeyCode.G)) DropItem();

        HandleHotbarInput();
    }

    void HandleHotbarInput()
    {
        int oldIndex = inventory.selectedHotbarIndex;

        // �������� ����
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) inventory.ChangeSelectedSlot(-1);
        else if (scroll < 0f) inventory.ChangeSelectedSlot(1);

        // ����� 1-5
        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.selectedHotbarIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.selectedHotbarIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.selectedHotbarIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.selectedHotbarIndex = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.selectedHotbarIndex = 4;

        // ���� ����������� ���� � ��������� �������� � ���� � �������� � UI
        if (oldIndex != inventory.selectedHotbarIndex)
        {
            UpdateHandVisuals();
            if (inventory.ui != null) inventory.ui.UpdateAllSlots();
        }
    }

    void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);

        foreach (var hit in hits)
        {
            PlantPot pot = hit.GetComponent<PlantPot>();
            if (pot != null)
            {
                PlantActionType action = pot.Interact(inventory);

                if(animator != null)
                {
                    switch(action)
                    {
                        case PlantActionType.Water:
                            animator.SetTrigger("Water");
                            break;

                        case PlantActionType.Plant:
                            animator.SetTrigger("Plant");
                            break;

                        case PlantActionType.Harvest:
                            animator.SetTrigger("Harvest");
                            break;
                    }
                }

                UpdateHandVisuals();
                return;
            }
        }

        foreach (var hit in hits)
        {
            PickupItem item = hit.GetComponent<PickupItem>();
            if (item != null && item.itemData != null)
            {
                int leftover = inventory.AddItem(item.itemData, 1);

                if (leftover == 0)
                {
                    if (animator != null)
                    {
                        animator.SetTrigger("Pickup");
                    }
                    Destroy(item.gameObject);
                    UpdateHandVisuals();
                    Debug.Log($"���������: {item.itemData.itemName}");
                }
                else Debug.Log("��������� �����!");
                return;
            }
        }
    }

    void DropItem()
    {
        InventorySlot activeSlot = inventory.GetSelectedSlot();
        if (activeSlot.IsEmpty) return;

        Vector3 dropPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;

        if (activeSlot.item.dropPrefab != null)
        {
            GameObject droppedObj = Instantiate(activeSlot.item.dropPrefab, dropPos, Quaternion.identity);
            PickupItem pickup = droppedObj.GetComponent<PickupItem>();
            if (pickup != null) pickup.itemData = activeSlot.item;
        }

        inventory.ConsumeSelectedItem();

        // ���� ��� ��� ����������, ������� ���� ��������� ��� �������
        if (activeSlot.item != null && activeSlot.item.isTool) activeSlot.Clear();

        UpdateHandVisuals();

        // ��������� �������� UI ����� �������
        if (inventory.ui != null) inventory.ui.UpdateAllSlots();
    }

    public void UpdateHandVisuals()
    {
        if (currentSpawnedModel != null)
        {
            Destroy(currentSpawnedModel);
            currentSpawnedModel = null;
        }

        InventorySlot activeSlot = inventory.GetSelectedSlot();
        if (!activeSlot.IsEmpty && activeSlot.item.handVisualPrefab != null && handSocket != null)
        {
            currentSpawnedModel = Instantiate(activeSlot.item.handVisualPrefab, handSocket);

            currentSpawnedModel.transform.localPosition = Vector3.zero;
            currentSpawnedModel.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            currentSpawnedModel.transform.localScale = Vector3.one * 0.01f; // 👈 ВОТ ЭТО ДОБАВИЛ
        }
    }
    
}