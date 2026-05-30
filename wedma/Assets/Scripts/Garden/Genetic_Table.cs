using UnityEngine;
using System.Collections;
using Invector.vCharacterController;

public class GeneticTable : MonoBehaviour
{
    public static GeneticTable instance;

    [Header("Камеры (ВАЖНО)")]
    public GameObject playerCameraObj;
    public GameObject tableCameraObj;
    public float transitionTime = 1.0f;

    [Header("Настройки Игрока")]
    public vThirdPersonInput playerMovementScript;
    public WitchInteraction playerInteractionScript;
    public GameObject witchVisualModel;

    [Header("Точки стола")]
    [Tooltip("Точка, куда будет падать и вставать цветок")]
    public Transform plateSnapPoint;

    [Header("UI Стола")]
    [Tooltip("Панель с кнопками 'Исследовать' и 'Модифицировать'")]
    public GameObject actionButtonsPanel;

    [Header("Новые UI Панели (Шаг 3 и 4)")]
    public PlantInfoPanelUI plantInfoPanel;
    public PlantModificationUI modificationUI;

    [Header("Точки обзора камеры")]
    public Transform researchCameraPoint; 
    public Transform modifyCameraPoint;

    public static bool isTableOpen = false;
    public static bool hasPlantOnPlate = false; 
    private bool isPlayerNear = false;
    private Coroutine transitionRoutine;

    private Vector3 tableTargetPos;
    private Quaternion tableTargetRot;

    private GameObject currentSpawnedPlant; 
    private ItemData currentPlantItem;
    private PlantInstance currentPlantInstance; 

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (tableCameraObj != null)
        {
            tableTargetPos = tableCameraObj.transform.position;
            tableTargetRot = tableCameraObj.transform.rotation;
            tableCameraObj.SetActive(false);
        }

        if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);

        if (modificationUI != null) modificationUI.Close();
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && transitionRoutine == null)
        {
            ToggleTable();
        }
    }

    public void ToggleTable()
    {
        isTableOpen = !isTableOpen;

        if (isTableOpen)
        {
            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (playerInteractionScript != null) playerInteractionScript.enabled = false;
            if (witchVisualModel != null) witchVisualModel.SetActive(false);
            hasPlantOnPlate = false;
        }
        else
        {
            ReturnPlantToInventory();
        }

        if (InventoryUI.instance != null)
            InventoryUI.instance.UpdateUIVisibility();

        transitionRoutine = StartCoroutine(MoveCameraRoutine(isTableOpen));
    }

    private IEnumerator MoveCameraRoutine(bool isEntering)
    {
        if (isEntering)
        {
            tableCameraObj.transform.position = playerCameraObj.transform.position;
            tableCameraObj.transform.rotation = playerCameraObj.transform.rotation;

            playerCameraObj.SetActive(false);
            tableCameraObj.SetActive(true);

            yield return StartCoroutine(LerpCamera(tableTargetPos, tableTargetRot));
        }
        else
        {
            yield return StartCoroutine(LerpCamera(playerCameraObj.transform.position, playerCameraObj.transform.rotation));

            tableCameraObj.SetActive(false);
            playerCameraObj.SetActive(true);

            if (playerMovementScript != null) playerMovementScript.enabled = true;
            if (playerInteractionScript != null) playerInteractionScript.enabled = true;
            if (witchVisualModel != null) witchVisualModel.SetActive(true);

            if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);

            if (plantInfoPanel != null && plantInfoPanel.panel != null) plantInfoPanel.panel.SetActive(false);
            if (modificationUI != null) modificationUI.Close();
        }

        transitionRoutine = null;
    }

    private IEnumerator LerpCamera(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = tableCameraObj.transform.position;
        Quaternion startRot = tableCameraObj.transform.rotation;
        float timeElapsed = 0f;

        while (timeElapsed < transitionTime)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / transitionTime;
            t = t * t * (3f - 2f * t); 

            tableCameraObj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            tableCameraObj.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        tableCameraObj.transform.position = targetPos;
        tableCameraObj.transform.rotation = targetRot;
    }

    public void AnimatePlantDrop(ItemData plantItem, PlantInstance plantInstance)
    {
        if (currentSpawnedPlant != null || plantItem == null || plantItem.dropPrefab == null) return;

        currentPlantItem = plantItem;
        currentPlantInstance = plantInstance; 

        Vector3 startPos = plateSnapPoint.position + new Vector3(0, 0.8f, 0);
        currentSpawnedPlant = Instantiate(plantItem.dropPrefab, startPos, Quaternion.identity);

        PickupItem pickup = currentSpawnedPlant.GetComponent<PickupItem>();
        if (pickup == null) pickup = currentSpawnedPlant.AddComponent<PickupItem>();
        pickup.itemData = plantItem;

        StartCoroutine(DropRoutine(currentSpawnedPlant));
    }

    private IEnumerator DropRoutine(GameObject plantObj)
    {
        Rigidbody rb = plantObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Vector3 startPos = plantObj.transform.position;
        Quaternion startRot = plantObj.transform.rotation;

        Vector3 endPos = plateSnapPoint.position;
        Quaternion endRot = Quaternion.identity;

        float duration = 0.35f; 
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t; 

            plantObj.transform.position = Vector3.Lerp(startPos, endPos, t);
            plantObj.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        plantObj.transform.position = endPos;
        plantObj.transform.rotation = endRot;
        plantObj.transform.up = Vector3.up; 

        hasPlantOnPlate = true;
        if (InventoryUI.instance != null)
            InventoryUI.instance.UpdateUIVisibility();

        if (actionButtonsPanel != null)
            actionButtonsPanel.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !other.isTrigger) isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            isPlayerNear = false;
            if (isTableOpen) ToggleTable();
        }
    }

    public void GoToResearch()
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);

        if (InventoryUI.instance != null && InventoryUI.instance.hotbarPanel != null)
            InventoryUI.instance.hotbarPanel.SetActive(false);

        if (plantInfoPanel != null)
        {
            if (plantInfoPanel.panel != null) plantInfoPanel.panel.SetActive(true);

            string pName = currentPlantItem != null ? currentPlantItem.itemName : "Растение";
            plantInfoPanel.Refresh(currentPlantInstance, pName);
        }

        transitionRoutine = StartCoroutine(MoveToPoint(researchCameraPoint.position, researchCameraPoint.rotation));
    }

    public void GoToModify()
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);

        if (modificationUI != null)
            modificationUI.Open(currentPlantInstance);

        transitionRoutine = StartCoroutine(MoveToPoint(modifyCameraPoint.position, modifyCameraPoint.rotation));
    }

    public void BackToCenter()
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);

        if (plantInfoPanel != null)
        {
            plantInfoPanel.ClearDisplayTarget();
            if (plantInfoPanel.panel != null) plantInfoPanel.panel.SetActive(false);
        }

        if (modificationUI != null) modificationUI.Close();

        if (InventoryUI.instance != null && InventoryUI.instance.hotbarPanel != null)
            InventoryUI.instance.hotbarPanel.SetActive(false);

        if (actionButtonsPanel != null) actionButtonsPanel.SetActive(true);

        transitionRoutine = StartCoroutine(MoveToPoint(tableTargetPos, tableTargetRot));
    }

    private void ReturnPlantToInventory()
    {
        if (hasPlantOnPlate && currentSpawnedPlant != null && InventoryUI.instance != null && InventoryUI.instance.playerInventory != null)
        {
            bool added = false;

            for (int i = 0; i < InventoryUI.instance.playerInventory.slots.Length; i++)
            {
                if (InventoryUI.instance.playerInventory.slots[i].IsEmpty)
                {
                    InventoryUI.instance.playerInventory.slots[i].item = currentPlantItem;
                    InventoryUI.instance.playerInventory.slots[i].count = 1;
                    InventoryUI.instance.playerInventory.slots[i].plantInstance = currentPlantInstance;
                    added = true;
                    break;
                }
            }

            if (added)
            {
                Destroy(currentSpawnedPlant);
            }
            else
            {
                Rigidbody rb = currentSpawnedPlant.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }

            currentSpawnedPlant = null;
            currentPlantItem = null;
            currentPlantInstance = null;
            hasPlantOnPlate = false;

            InventoryUI.instance.UpdateAllSlots();
        }
    }

    private IEnumerator MoveToPoint(Vector3 targetPos, Quaternion targetRot)
    {
        yield return StartCoroutine(LerpCamera(targetPos, targetRot));
        transitionRoutine = null;
    }

}