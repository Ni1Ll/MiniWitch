using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Garden/Plant Data")]
public class PlantData : ScriptableObject
{
    public string plantName;

    [Header("Визуал")]
    public GameObject healthyPrefab; // Живой цветок
    public GameObject deadPrefab;    // Мертвый цветок

    [Header("Настройки")]
    public float waterConsumption = 5f;
    public float optimalTemp = 20f;
    public float tempRange = 10f;
    public float growthSpeed = 10f;
}