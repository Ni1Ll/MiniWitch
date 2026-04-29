using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Garden/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 99;
    public bool isTool = false;

    [Header("Тип инструмента")]
    public ToolType toolType = ToolType.None; // Заполни в Inspector для каждого инструмента

    [Header("Визуал")]
    public GameObject handVisualPrefab;
    public GameObject dropPrefab;

    [Header("Экономика")]
    public int sellPrice = 15;
    public int buyPrice = 30;
}