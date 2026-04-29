using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;

public class WitchInteraction : MonoBehaviour
{
    public Animator animator;

    [Header("Настройки взаимодействия")]
    public float interactionRadius = 2.5f;
    public float minHighlightThreshold = 0.5f;

    [Header("Настройки Зажатия (Hold)")]
    public float waterHoldTime = 1.5f;
    public float plantHoldTime = 2.0f;
    public float harvestHoldTime = 1.0f;

    public Image holdProgressBar;
    public string defaultIdleState = "Blend Tree";

    [Header("Слот в руке")]
    public Transform handSocket;

    [Header("UI и NPC")]
    [SerializeField] private GameObject npcAgent;
    [SerializeField] private GameObject popupUI;

    private vThirdPersonController invectorController;
    private PlayerInventory inventory;
    private GameObject currentSpawnedModel;

    private bool isUIOpen = false;
    private bool isHolding = false;
    private float currentHoldTimer = 0f;
    private GameObject targetObject = null;
    private PlantActionType pendingAction = PlantActionType.None;
    private float currentRequiredTime = 1.0f;

    private List<Outline> reachableOutlines = new List<Outline>();
    private Outline currentTargetOutline = null;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        invectorController = GetComponent<vThirdPersonController>();

        UpdateHandVisuals();

        if (holdProgressBar != null)
            holdProgressBar.transform.parent.gameObject.SetActive(false);

        if (popupUI != null)
            popupUI.SetActive(false);
    }

    void Update()
    {
        if (isUIOpen)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
                ClosePopupUI();
            return;
        }

        if (Input.GetKeyDown(KeyCode.G) && !isHolding) DropItem();

        HandleHotbarInput();
        UpdateHighlighting();
        HandleHoldInteraction();
    }

    // ==========================================
    // УПРАВЛЕНИЕ UI И БЛОКИРОВКА INVECTOR
    // ==========================================
    void OpenPopupUI()
    {
        isUIOpen = true;
        if (popupUI != null) popupUI.SetActive(true);

        if (invectorController != null)
        {
            invectorController.lockMovement = true;
            invectorController.lockRotation = true;
            invectorController.lockActions = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ClosePopupUI()
    {
        isUIOpen = false;
        if (popupUI != null) popupUI.SetActive(false);

        if (invectorController != null)
        {
            invectorController.lockMovement = false;
            invectorController.lockRotation = false;
            invectorController.lockActions = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ==========================================
    // СИСТЕМА ВЫДЕЛЕНИЯ (OUTLINE)
    // ==========================================
    void UpdateHighlighting()
    {
        if (isHolding) return;

        Outline bestOutline = null;
        float maxDot = minHighlightThreshold;

        for (int i = reachableOutlines.Count - 1; i >= 0; i--)
        {
            if (reachableOutlines[i] == null)
            {
                reachableOutlines.RemoveAt(i);
                continue;
            }

            Vector3 dirToTarget = (reachableOutlines[i].transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToTarget);

            if (dot > maxDot)
            {
                maxDot = dot;
                bestOutline = reachableOutlines[i];
            }
        }

        if (bestOutline != currentTargetOutline)
        {
            if (currentTargetOutline != null) currentTargetOutline.enabled = false;
            currentTargetOutline = bestOutline;
            if (currentTargetOutline != null) currentTargetOutline.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Outline outline = other.GetComponent<Outline>();
        if (outline != null && !reachableOutlines.Contains(outline))
            reachableOutlines.Add(outline);
    }

    void OnTriggerExit(Collider other)
    {
        Outline outline = other.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
            reachableOutlines.Remove(outline);
            if (currentTargetOutline == outline) currentTargetOutline = null;
        }
    }

    // ==========================================
    // ЛОГИКА ВЗАИМОДЕЙСТВИЯ (E)
    // ==========================================
    void HandleHoldInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            targetObject = currentTargetOutline != null ? currentTargetOutline.gameObject : null;

            if (targetObject != null)
            {
                if (targetObject == npcAgent)
                {
                    if (currentTargetOutline != null) currentTargetOutline.enabled = false;
                    OpenPopupUI();
                    return;
                }

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
                FinishHoldInteraction();
        }

        if (Input.GetKeyUp(KeyCode.E) && isHolding)
            CancelHoldInteraction();
    }

    void FinishHoldInteraction()
    {
        if (targetObject != null) ExecuteInteraction(targetObject);
        ResetHoldState();
    }

    void CancelHoldInteraction()
    {
        if (animator != null && pendingAction != PlantActionType.None)
        {
            if (animator.HasState(0, Animator.StringToHash(defaultIdleState)))
                animator.CrossFade(defaultIdleState, 0.2f, 0);
            else
                animator.Play(0);
        }
        ResetHoldState();
    }

    void ResetHoldState()
    {
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
                Outline o = obj.GetComponent<Outline>();
                if (o != null) o.enabled = false;

                Collider itemCol = item.GetComponent<Collider>();
                if (itemCol != null) itemCol.enabled = false;

                if (animator != null) animator.SetTrigger("Pickup");

                StartCoroutine(FullPickupSequence(item.itemData.handVisualPrefab, item.gameObject));
            }
        }
    }

    private void ShowTemporaryItem(GameObject temporaryPrefab)
    {
        if (currentSpawnedModel != null) Destroy(currentSpawnedModel);

        if (temporaryPrefab != null && handSocket != null)
        {
            currentSpawnedModel = Instantiate(temporaryPrefab, handSocket);
            SetupInHand(currentSpawnedModel);
        }
    }

    private IEnumerator FullPickupSequence(GameObject itemPrefab, GameObject worldObject)
    {
        yield return new WaitForSeconds(1.0f);

        ShowTemporaryItem(itemPrefab);

        if (worldObject != null) Destroy(worldObject);

        yield return new WaitForSeconds(2.0f);

        UpdateHandVisuals();
    }

    // --- Инвентарь и Дроп ---

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
        if (currentSpawnedModel != null) Destroy(currentSpawnedModel);

        InventorySlot activeSlot = inventory.GetSelectedSlot();

        if (!activeSlot.IsEmpty && activeSlot.item.handVisualPrefab != null && handSocket != null)
        {
            currentSpawnedModel = Instantiate(activeSlot.item.handVisualPrefab, handSocket);
            SetupInHand(currentSpawnedModel);
        }
    }

    private void SetupInHand(GameObject model)
    {
        Rigidbody rb = model.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.detectCollisions = false; }
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
        model.transform.localScale = Vector3.one * 0.01f;
    }

    private IEnumerator DelayedPickup(float delay, GameObject worldItem)
    {
        yield return new WaitForSeconds(delay);
        if (worldItem != null) Destroy(worldItem);
        UpdateHandVisuals();
    }
}