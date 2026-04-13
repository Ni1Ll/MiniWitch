using UnityEngine;
using UnityEngine.UI; // Для работы с кнопкой
using System.Collections.Generic; // Для работы со списками

public class Cauldron : MonoBehaviour
{
    public static Cauldron instance;

    [Header("Интерфейс")]
    public GameObject cauldronUIPanel;
    public SlotUI[] uiSlots;
    public Button brewButton; // Наша новая кнопка "Сварить"

    [Header("Внутренние карманы Котла")]
    public InventorySlot[] slots = new InventorySlot[3];

    [Header("База Рецептов")]
    public List<RecipeData> allRecipes; // Список всех рецептов в игре
    private RecipeData validRecipe; // Рецепт, который совпал прямо сейчас

    private bool isPlayerNear = false;
    public static bool isCauldronOpen = false;

    void Awake()
    {
        instance = this;
        for (int i = 0; i < slots.Length; i++) slots[i] = new InventorySlot();
    }

    void Start()
    {
        // Подключаем кнопку к функции варки (чтобы не перетаскивать OnClick вручную)
        if (brewButton != null) brewButton.onClick.AddListener(BrewPotion);
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E)) ToggleCauldron();
    }

    public void ToggleCauldron()
    {
        isCauldronOpen = !isCauldronOpen;

        if (cauldronUIPanel != null)
        {
            cauldronUIPanel.SetActive(isCauldronOpen);
            if (isCauldronOpen) UpdateUI();
        }

        if (InventoryUIManager.instance != null)
        {
            InventoryUIManager.instance.SetMechanicMode(isCauldronOpen);
        }
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