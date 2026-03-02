using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Garden/Plant Data")]
public class PlantData : ScriptableObject
{
    public string plantName;

    [Header("Визуал в Мире")]
    public GameObject healthyPrefab;
    public GameObject deadPrefab;   

    [Header("Визуал в Руке")]
    public GameObject handVisualPrefab; 

    [Header("Настройки")]
    public float waterConsumption = 5f;
    public float optimalTemp = 20f;
    public float tempRange = 10f;
    public float growthSpeed = 10f;
}