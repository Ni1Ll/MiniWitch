using System.Collections.Generic;
using UnityEngine;

public class Marker3d : MonoBehaviour
{
    [SerializeField] private Transform head; // Тянем сюда кость головы
    [SerializeField] private GameObject markerRoot; // Тот самый пустой объект с дочерними маркерами
    
    private List<Transform> childMarkers = new List<Transform>();
    private Transform rootTransform;
    private bool isAnyMarkerActive = false;

    void Awake()
    {
        // Кэшируем трансформ самого корня маркеров для скорости
        rootTransform = markerRoot.transform;

        if (markerRoot != null)
        {
            foreach (Transform child in markerRoot.transform)
            {
                childMarkers.Add(child);
            }
        }
    }

    // Используем LateUpdate, чтобы позиция обновлялась ПОСЛЕ того, 
    // как аниматор повернул персонажа и голову
    void LateUpdate()
    {
        if (isAnyMarkerActive && head != null)
        {
            // Просто копируем позицию. Вращение остается мировым (какое настроил).
            rootTransform.position = head.position;
        }
    }

    public void EnableMarker(int index)
    {
        if (index >= 0 && index < childMarkers.Count)
        {
            childMarkers[index].gameObject.SetActive(true);
            isAnyMarkerActive = true;
        }
    }

    public void DisableMarker(int index)
    {
        if (index >= 0 && index < childMarkers.Count)
        {
            childMarkers[index].gameObject.SetActive(false);
            CheckActiveStatus();
        }
    }

    private void CheckActiveStatus()
    {
        // Если ни один маркер не включен, Update/LateUpdate не будет гонять копирование позиции
        isAnyMarkerActive = childMarkers.Exists(m => m.gameObject.activeSelf);
    }
}