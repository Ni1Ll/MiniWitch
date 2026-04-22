using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;

public class Cauldron : MonoBehaviour
{
    public static Cauldron instance;

    [Header("Интерфейс")]
    public GameObject cauldronUIPanel;
    public SlotUI[] uiSlots;
    public UnityEngine.UI.Image progressBar;

    [Header("Внутренние карманы Котла")]
    public InventorySlot[] slots = new InventorySlot[3];

    [Header("База Рецептов")]
    public List<RecipeData> allRecipes;
    public RecipeData validRecipe;

    [Header("Варка (Мешалка)")]
    public Transform potionSpawnPoint;
    public Transform spoonBone;
    public float requiredStirs = 3f;
    public float stirDecayRate = 120f;
    private float currentStirProgress = 0f;
    private float lastMouseAngle = 0f;
    private bool isStirring = false;

    [Header("Камеры (ВАЖНО)")]
    public GameObject playerCameraObj;
    public GameObject cauldronCameraObj;
    public float transitionTime = 1.0f;

    [Header("Настройки Игрока")]
    public vThirdPersonInput playerMovementScript;
    public WitchInteraction playerInteractionScript;

    private bool isPlayerNear = false;
    public static bool isCauldronOpen = false;

    private Vector3 cauldronTargetPos;
    private Quaternion cauldronTargetRot;
    private Coroutine transitionRoutine;
    public GameObject witchVisualModel;

    void Awake()
    {
        instance = this;
        for (int i = 0; i < slots.Length; i++) slots[i] = new InventorySlot();
    }

    void Start()
    {
        if (cauldronCameraObj != null)
        {
            cauldronTargetPos = cauldronCameraObj.transform.position;
            cauldronTargetRot = cauldronCameraObj.transform.rotation;
            cauldronCameraObj.SetActive(false);
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.transform.parent.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && transitionRoutine == null)
        {
            ToggleCauldron();
        }

        if (isCauldronOpen && transitionRoutine == null)
        {
            HandleStirring();
        }
    }

    public void ToggleCauldron()
    {
        isCauldronOpen = !isCauldronOpen;

        if (isCauldronOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (playerInteractionScript != null) playerInteractionScript.enabled = false;
            if (progressBar != null) progressBar.transform.parent.gameObject.SetActive(true);
            if (witchVisualModel != null) witchVisualModel.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (progressBar != null) progressBar.transform.parent.gameObject.SetActive(false);
        }

        if (InventoryUI.instance != null)
            InventoryUI.instance.SetMechanicMode(isCauldronOpen);

        transitionRoutine = StartCoroutine(MoveCameraRoutine(isCauldronOpen));
    }

    // --- МАГИЯ ПЛАВНОГО ПОЛЕТА ---
    private IEnumerator MoveCameraRoutine(bool isEntering)
    {
        if (isEntering)
        {
            // 1. Ставим камеру котла в голову игрока
            cauldronCameraObj.transform.position = playerCameraObj.transform.position;
            cauldronCameraObj.transform.rotation = playerCameraObj.transform.rotation;

            // 2. Переключаем "глаза"
            playerCameraObj.SetActive(false);
            cauldronCameraObj.SetActive(true);

            // 3. Летим к жиже
            yield return StartCoroutine(LerpCamera(cauldronTargetPos, cauldronTargetRot));

            // 4. Показываем UI только когда прилетели
            if (cauldronUIPanel != null)
            {
                cauldronUIPanel.SetActive(true);
                UpdateUI();
            }
        }
        else
        {
            if (cauldronUIPanel != null) cauldronUIPanel.SetActive(false);

            yield return StartCoroutine(LerpCamera(playerCameraObj.transform.position, playerCameraObj.transform.rotation));

            cauldronCameraObj.SetActive(false);
            playerCameraObj.SetActive(true);

            if (playerMovementScript != null) playerMovementScript.enabled = true;
            if (playerInteractionScript != null) playerInteractionScript.enabled = true;
            if (witchVisualModel != null) witchVisualModel.SetActive(true);
        }

        transitionRoutine = null;
    }

    private IEnumerator LerpCamera(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = cauldronCameraObj.transform.position;
        Quaternion startRot = cauldronCameraObj.transform.rotation;
        float timeElapsed = 0f;

        while (timeElapsed < transitionTime)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / transitionTime;

            // Математическая формула Ease In-Out (чтобы камера плавно разгонялась и плавно тормозила)
            t = t * t * (3f - 2f * t);

            cauldronCameraObj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cauldronCameraObj.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null; // Ждем один кадр
        }

        cauldronCameraObj.transform.position = targetPos;
        cauldronCameraObj.transform.rotation = targetRot;
    }

    public void UpdateUI()
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (uiSlots[i] != null) uiSlots[i].UpdateSlot(slots[i], false);
        }

        // Каждый раз, когда предметы в котле двигаются, проверяем рецепт!
        CheckRecipe();
    }

    // --- ЛОГИКА АЛХИМИИ ---

    private void CheckRecipe()
    {
        validRecipe = null;

        // Перебираем все рецепты, чтобы найти подходящий
        foreach (var recipe in allRecipes)
        {
            if (IsRecipeMatch(recipe))
            {
                validRecipe = recipe;
                break;
            }
        }
    }

    private bool IsRecipeMatch(RecipeData recipe)
    {
        int matchesFound = 0;
        int itemsInCauldron = 0;

        for (int i = 0; i < 2; i++)
        {
            if (!slots[i].IsEmpty) itemsInCauldron++;
        }

        // Если количество предметов в котле не совпадает с рецептом (например, бросили лишний мусор) - отмена
        if (itemsInCauldron != recipe.ingredients.Count) return false;

        // Проверяем, есть ли нужные ингредиенты в нужном количестве
        foreach (var required in recipe.ingredients)
        {
            bool hasIngredient = false;
            for (int i = 0; i < 2; i++)
            {
                if (slots[i].item == required.item && slots[i].count >= required.amount)
                {
                    hasIngredient = true;
                    break;
                }
            }
            if (!hasIngredient) return false;
            else matchesFound++;
        }

        return matchesFound == recipe.ingredients.Count;
    }

    private void CompleteBrewing()
    {
        isStirring = false;
        currentStirProgress = 0f;
        if (progressBar != null) progressBar.fillAmount = 0f;

        // Ищем рецепт (твоя старая функция)
        CheckRecipe();

        if (validRecipe != null && validRecipe.resultPotion != null)
        {
            Debug.Log("УСПЕХ! Сварено: " + validRecipe.resultPotion.itemName);

            if (validRecipe.resultPotion.dropPrefab != null)
            {
                Vector3 spawnPos = potionSpawnPoint != null ? potionSpawnPoint.position : cauldronCameraObj.transform.position + cauldronCameraObj.transform.forward * 1.5f;

                GameObject potion = Instantiate(validRecipe.resultPotion.dropPrefab, spawnPos, Quaternion.identity);

                Rigidbody rb = potion.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.useGravity = false;
                    rb.isKinematic = true;
                }

                PickupItem pickup = potion.GetComponent<PickupItem>();
                if (pickup == null) pickup = potion.AddComponent<PickupItem>();
                pickup.itemData = validRecipe.resultPotion;
            }
        }
        else
        {
            Debug.Log("БУЛЬК! Получилась какая-то бурда. Ингредиенты сгорели.");
        }

        for (int i = 0; i < 2; i++)
        {
            slots[i].item = null;
            slots[i].count = 0;
        }
        validRecipe = null;
        UpdateUI();
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
            if (isCauldronOpen) ToggleCauldron();
        }
    }

    private void HandleStirring()
    {
        // Защита: не кликаем, если тащим предмет в инвентаре
        if (SlotUI.draggedSlot != null) return;

        // 1. СНАЧАЛА ПРОВЕРЯЕМ КЛИКИ
        if (Input.GetMouseButtonDown(0))
        {
            Camera activeCam = cauldronCameraObj.GetComponent<Camera>();
            Ray ray = activeCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            bool potionPickedUp = false;

            foreach (var hit in hits)
            {
                PickupItem item = hit.collider.GetComponentInParent<PickupItem>();
                if (item != null && item.itemData is PotionData)
                {
                    int leftover = InventoryUI.instance.playerInventory.AddItem(item.itemData, 1);
                    if (leftover == 0)
                    {
                        Destroy(item.gameObject);
                        InventoryUI.instance.UpdateAllSlots();
                        potionPickedUp = true;
                        break;
                    }
                }
            }

            if (potionPickedUp) return;

            isStirring = true;

            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 mouseDir = (Vector2)Input.mousePosition - screenCenter;
            lastMouseAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isStirring = false;
        }

        // 2. ПРОВЕРЯЕМ ИНГРЕДИЕНТЫ
        bool hasIngredients = !slots[0].IsEmpty || !slots[1].IsEmpty;

        if (!hasIngredients)
        {
            isStirring = false;
            return;
        }

        // 3. МАТЕМАТИКА КРУЧЕНИЯ
        if (isStirring)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 mouseDir = (Vector2)Input.mousePosition - screenCenter;
            float currentAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;

            float deltaAngle = Mathf.DeltaAngle(lastMouseAngle, currentAngle);
            currentStirProgress += Mathf.Abs(deltaAngle);
            lastMouseAngle = currentAngle;

            // --- ЗАМЕДЛЯЕМ ЛОЖКУ И КРУТИМ В ПРАВИЛЬНУЮ СТОРОНУ ---
            // Обрати внимание на минус перед deltaAngle!
            if (spoonBone != null) spoonBone.Rotate(Vector3.up, -deltaAngle * 0.6f, Space.Self);
        }

        // 4. ОСТЫВАНИЕ (Работает всегда, когда котел не пуст)
        if (currentStirProgress > 0f)
        {
            // Отнимаем прогресс каждую секунду
            currentStirProgress -= stirDecayRate * Time.deltaTime;

            // Чтобы шкала не ушла в минус
            if (currentStirProgress < 0f) currentStirProgress = 0f;
        }

        // 5. ОБНОВЛЕНИЕ UI И ПРОВЕРКА ГОТОВНОСТИ
        if (progressBar != null) progressBar.fillAmount = currentStirProgress / (requiredStirs * 360f);

        if (currentStirProgress >= requiredStirs * 360f)
        {
            CompleteBrewing();
        }

    }

}

