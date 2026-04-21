using UnityEngine;
using System.Collections; // Добавлено для работы корутин

public class WitchInteraction : MonoBehaviour
{
    public Animator animator;
    [Header("Настройки")]
    public float interactionRadius = 2.5f;

    [Header("Слот в руке")]
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

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) inventory.ChangeSelectedSlot(-1);
        else if (scroll < 0f) inventory.ChangeSelectedSlot(1);

        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.selectedHotbarIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.selectedHotbarIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.selectedHotbarIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.selectedHotbarIndex = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.selectedHotbarIndex = 4;

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

                if (animator != null)
                {
                    switch (action)
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

                    Collider itemCol = item.GetComponent<Collider>();
                    if (itemCol != null) itemCol.enabled = false;

                    Rigidbody itemRb = item.GetComponent<Rigidbody>();
                    if (itemRb != null)
                    {
                        itemRb.isKinematic = true;
                    }

                    StartCoroutine(DelayedPickup(1.0f, item.gameObject));

                    Debug.Log($"Подготовка к подбору: {item.itemData.itemName}");
                }
                else Debug.Log("Инвентарь полон!");
                return;
            }
        }
    } // Скобка, закрывающая TryInteract

    void DropItem()
    {
        InventorySlot activeSlot = inventory.GetSelectedSlot();
        if (activeSlot.IsEmpty) return;

        Vector3 dropPos = transform.position + transform.forward * 0.3f + Vector3.up * 0.6f;

        if (activeSlot.item.dropPrefab != null)
        {
            GameObject droppedObj = Instantiate(activeSlot.item.dropPrefab, dropPos, transform.rotation);
            PickupItem pickup = droppedObj.GetComponent<PickupItem>();
            if (pickup != null) pickup.itemData = activeSlot.item;

            Rigidbody rb = droppedObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                Vector3 dropForce = (transform.forward * 1.2f) + (Vector3.up * 1.0f);
                rb.AddForce(dropForce, ForceMode.Impulse);
            }
        }

        inventory.ConsumeSelectedItem();
        if (activeSlot.item != null && activeSlot.item.isTool) activeSlot.Clear();

        UpdateHandVisuals();
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

            Rigidbody rb = currentSpawnedModel.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            currentSpawnedModel.transform.localPosition = Vector3.zero;
            currentSpawnedModel.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            currentSpawnedModel.transform.localScale = Vector3.one * 0.01f;
        }
    }

    private IEnumerator DelayedPickup(float delay, GameObject worldItem)
    {
        yield return new WaitForSeconds(delay);

        UpdateHandVisuals();

        if (worldItem != null)
        {
            Destroy(worldItem);
        }
    }
}