using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Plant", menuName = "Garden/Plant Data")]
public class PlantData : ItemData
{
    // 🔥 ГЕНЕТИКА (ОБЯЗАТЕЛЬНО)
    [Header("Генетика")]
    public PlantFamily family;

    // 🌱 ФАЗЫ РОСТА
    [Header("Фазы роста")]
    public GameObject[] growthPrefabs = new GameObject[4];

    // 💀 МЁРТВОЕ СОСТОЯНИЕ
    [Header("Мёртвое растение")]
    public GameObject deadPrefab;

    // 🌡 ПАРАМЕТРЫ РОСТА
    [Header("Параметры роста")]
    public float waterConsumption = 5f;
    public float optimalTemp = 20f;
    public float tempRange = 10f;
    public float growthSpeed = 10f;

    // 🌾 УРОЖАЙ
    [Header("Урожай")]
    public ItemData harvestResult; 
    public int harvestAmount = 1;
}