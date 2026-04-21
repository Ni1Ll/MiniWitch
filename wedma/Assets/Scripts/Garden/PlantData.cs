using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Garden/Plant Data")]
public class PlantData : ItemData
{
    [Header("Визуал на Грядке")]
    public GameObject healthyPrefab;
    public GameObject deadPrefab;

    [Header("Настройки Роста")]
    public float waterConsumption = 5f;
    public float optimalTemp = 20f;
    public float tempRange = 10f;
    public float growthSpeed = 10f;

    [Header("Урожай")]
    public ItemData harvestResult; 
    public int harvestAmount = 1;  

}