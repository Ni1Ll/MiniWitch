using UnityEngine;
using System.Collections.Generic;

// Этот мини-класс нужен, чтобы мы могли указывать количество предметов в Инспекторе
[System.Serializable]
public class Ingredient
{
    public ItemData item;
    public int amount = 1; // Сколько штук нужно
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Garden/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName = "Новый рецепт";

    [Header("Что нужно положить:")]
    public List<Ingredient> ingredients;

    [Header("Что получится:")]
    public ItemData resultPotion;
    public int resultAmount = 1;
}