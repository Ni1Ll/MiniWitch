using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Garden/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;             // Иконка для будущего UI меню
    public int maxStack = 99;       // Сколько влезает в одну ячейку
    public bool isTool = false;     // Инструмент ли это? (Лейка, лопата)

    [Header("Визуал")]
    public GameObject handVisualPrefab; // Что появляется в руке Ведьмы
    public GameObject dropPrefab;       // Какой мешочек/моделька падает на пол при нажатии G
}