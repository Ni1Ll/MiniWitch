using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Plant", menuName = "Garden/Plant Data")]
public class PlantData : ItemData
{
    [Header("Фазы роста")]
    public GameObject[] growthPrefabs = new GameObject[4];

    [Header("Мёртвое растение")]
    public GameObject deadPrefab;

    [Header("Параметры роста")]
    public float waterConsumption = 5f;
    public float optimalTemp = 20f;
    public float tempRange = 10f;
    public float growthSpeed = 10f;

    [Header("Урожай")]
    public ItemData harvestResult; 
    public int harvestAmount = 1;

}