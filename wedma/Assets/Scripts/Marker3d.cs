using System.Collections.Generic;
using UnityEngine;

public class Marker3d : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private GameObject markerRoot;

    private List<Transform> childMarkers = new List<Transform>();
    private Transform rootTransform;
    private bool isAnyMarkerActive = false;

    void Awake()
    {
        if (markerRoot == null)
        {
            Debug.LogWarning("[Marker3d] Marker Root не назначен.");
            return;
        }

        rootTransform = markerRoot.transform;

        foreach (Transform child in markerRoot.transform)
        {
            childMarkers.Add(child);
            child.gameObject.SetActive(false);
        }

        CheckActiveStatus();
    }

    void LateUpdate()
    {
        if (isAnyMarkerActive && head != null && rootTransform != null)
        {
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

    public void DisableAllMarkers()
    {
        foreach (Transform marker in childMarkers)
        {
            if (marker != null)
                marker.gameObject.SetActive(false);
        }

        isAnyMarkerActive = false;
    }

    private void CheckActiveStatus()
    {
        isAnyMarkerActive = childMarkers.Exists(m => m != null && m.gameObject.activeSelf);
    }
}