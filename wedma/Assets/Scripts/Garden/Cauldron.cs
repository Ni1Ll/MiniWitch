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
    public Button brewButton;

    [Header("Внутренние карманы Котла")]
    public InventorySlot[] slots = new InventorySlot[3];

    [Header("База Рецептов")]
    public List<RecipeData> allRecipes;
    private RecipeData validRecipe;

    [Header("Камеры (ВАЖНО)")]
    public GameObject playerCameraObj;
    public GameObject cauldronCameraObj;
    public float transitionTime = 1.0f; 

    [Header("Настройки Игрока")]
    public vThirdPersonInput playerMovementScript;

    private bool isPlayerNear = false;
    public static bool isCauldronOpen = false;

    // Внутренние переменные для полета
    private Vector3 cauldronTargetPos;
    private Quaternion cauldronTargetRot;
    private Coroutine transitionRoutine;

    void Awake()
    {
        instance = this;
        for (int i = 0; i < slots.Length; i++) slots[i] = new InventorySlot();
    }

    void Start()
    {
        if (brewButton != null) brewButton.onClick.AddListener(BrewPotion);

        // На старте ЗАПОМИНАЕМ ту самую идеальную позицию камеры котла
        if (cauldronCameraObj != null)
        {
            cauldronTargetPos = cauldronCameraObj.transform.position;
            cauldronTargetRot = cauldronCameraObj.transform.rotation;
            cauldronCameraObj.SetActive(false);
        }
    }

    void Update()
    {
        // Не даем нажать E, пока камера летит (чтобы не сломать анимацию)
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && transitionRoutine == null)
        {
            ToggleCauldron();
        }
    }

    public void ToggleCauldron()
    {
        isCauldronOpen = !isCauldronOpen;

        // Включаем/выключаем скрипт ходьбы и мышь сразу же
        if (isCauldronOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerMovementScript != null) playerMovementScript.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerMovementScript != null) playerMovementScript.enabled = true;
        }

        if (InventoryUIManager.instance != null)
            InventoryUIManager.instance.SetMechanicMode(isCauldronOpen);

        // Запускаем плавный полет камеры!
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
            // 1. Прячем UI перед отлетом
            if (cauldronUIPanel != null) cauldronUIPanel.SetActive(false);

            // 2. Летим обратно за спину (в позицию камеры Invector)
            yield return StartCoroutine(LerpCamera(playerCameraObj.transform.position, playerCameraObj.transform.rotation));

            // 3. Возвращаем глаза игроку
            cauldronCameraObj.SetActive(false);
            playerCameraObj.SetActive(true);
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

        if (brewButton != null) brewButton.interactable = (validRecipe != null);
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

    // Эта функция вызывается при нажатии на кнопку "Сварить"
    public void BrewPotion()
    {
        if (validRecipe == null) return;

        // 1. Сжигаем ингредиенты
        foreach (var required in validRecipe.ingredients)
        {
            for (int i = 0; i < 2; i++)
            {
                if (slots[i].item == required.item)
                {
                    slots[i].count -= required.amount;
                    if (slots[i].count <= 0) slots[i].Clear();
                    break;
                }
            }
        }

        // 2. Создаем зелье в 4-й ячейке
        if (slots[2].IsEmpty)
        {
            slots[2].item = validRecipe.resultPotion;
            slots[2].count = validRecipe.resultAmount;
        }
        else if (slots[2].item == validRecipe.resultPotion)
        {
            slots[2].count += validRecipe.resultAmount; // Складываем в стак, если такое зелье там уже лежит
        }

        // Обновляем картинки
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
}