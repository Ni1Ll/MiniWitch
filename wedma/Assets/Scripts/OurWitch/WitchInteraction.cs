using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WitchInteraction : MonoBehaviour
{
    public Animator animator;
    [Header("Настройки")]
    public float interactionRadius = 2.5f;

    [Header("Настройки Зажатия (Hold)")]
    public float waterHoldTime = 1.5f;   
    public float plantHoldTime = 2.0f;  
    public float harvestHoldTime = 1.0f; 
                                      

    public Image holdProgressBar;
    public string defaultIdleState = "Blend Tree";

    [Header("Слот в руке")]
    public Transform handSocket;

    private PlayerInventory inventory;
    private GameObject currentSpawnedModel;

    private bool isHolding = false;
    private float currentHoldTimer = 0f;
    private GameObject targetObject = null;
    private PlantActionType pendingAction = PlantActionType.None;
    private float currentRequiredTime = 1.0f; 

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        UpdateHandVisuals();

        if (holdProgressBar != null)
            holdProgressBar.transform.parent.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isHolding) DropItem();

        HandleHotbarInput();
        HandleHoldInteraction();
    }

    void HandleHoldInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            targetObject = FindClosestInteractable();
            if (targetObject != null)
            {
                PlantPot pot = targetObject.GetComponent<PlantPot>();
                if (pot != null)
                {
                    pendingAction = pot.GetAvailableAction(inventory);

                    if (pendingAction != PlantActionType.None)
                    {
                        isHolding = true;
                        currentHoldTimer = 0f;

                        if (pendingAction == PlantActionType.Water) currentRequiredTime = waterHoldTime;
                        else if (pendingAction == PlantActionType.Plant) currentRequiredTime = plantHoldTime;
                        else if (pendingAction == PlantActionType.Harvest) currentRequiredTime = harvestHoldTime;

                        if (holdProgressBar != null)
                            holdProgressBar.transform.parent.gameObject.SetActive(true);

                        if (animator != null)
                        {
                            if (pendingAction == PlantActionType.Water) animator.SetTrigger("Water");
                            else if (pendingAction == PlantActionType.Plant) animator.SetTrigger("Plant");
                            else if (pendingAction == PlantActionType.Harvest) animator.SetTrigger("Harvest");
                        }
                    }
                }
                else
                {
                    ExecuteInteraction(targetObject);
                }
            }
        }

        if (Input.GetKey(KeyCode.E) && isHolding)
        {
            currentHoldTimer += Time.deltaTime;

            if (holdProgressBar != null)
                holdProgressBar.fillAmount = currentHoldTimer / currentRequiredTime;

            if (currentHoldTimer >= currentRequiredTime)
            {
                FinishHoldInteraction();
            }
        }

        if (Input.GetKeyUp(KeyCode.E) && isHolding)
        {
            CancelHoldInteraction();
        }
    }

    void FinishHoldInteraction()
    {
        if (targetObject != null) ExecuteInteraction(targetObject);

        // Сбрасываем переменные и UI
        isHolding = false;
        currentHoldTimer = 0f;
        targetObject = null;
        pendingAction = PlantActionType.None;

        if (holdProgressBar != null)
        {
            holdProgressBar.fillAmount = 0f;
            holdProgressBar.transform.parent.gameObject.SetActive(false);
        }
    }

    void CancelHoldInteraction()
    {
        isHolding = false;
        currentHoldTimer = 0f;
        targetObject = null;

        if (holdProgressBar != null)
        {
            holdProgressBar.fillAmount = 0f;
            holdProgressBar.transform.parent.gameObject.SetActive(false);
        }

        if (animator != null && pendingAction != PlantActionType.None)
        {
            if (animator.HasState(0, Animator.StringToHash(defaultIdleState)))
            {
                animator.CrossFade(defaultIdleState, 0.2f, 0); // Добавили , 0 для явного указания слоя
            }
            else
            {
                animator.Play(0);
                Debug.LogWarning($"[Внимание] Аниматор не нашел состояние '{defaultIdleState}'. Проверь название в инспекторе!");
            }

            pendingAction = PlantActionType.None;
        }
    }

    GameObject FindClosestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.GetComponent<PlantPot>() != null || hit.GetComponent<PickupItem>() != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hit.gameObject;
                }
            }
        }
        return closest;
    }

    void ExecuteInteraction(GameObject obj)
    {
        PlantPot pot = obj.GetComponent<PlantPot>();
        if (pot != null)
        {
            pot.Interact(inventory);
            UpdateHandVisuals();
            return;
        }

        PickupItem item = obj.GetComponent<PickupItem>();
        if (item != null && item.itemData != null)
        {
            int leftover = inventory.AddItem(item.itemData, 1);
            if (leftover == 0)
            {
                if (currentSpawnedModel != null) Destroy(currentSpawnedModel);
                if (animator != null) animator.SetTrigger("Pickup");
                Collider itemCol = item.GetComponent<Collider>();
                if (itemCol != null) itemCol.enabled = false;
                Rigidbody itemRb = item.GetComponent<Rigidbody>();
                if (itemRb != null) itemRb.isKinematic = true;
                StartCoroutine(DelayedPickup(1.0f, item.gameObject));
            }
        }
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
        if (worldItem != null) Destroy(worldItem);
    }
}