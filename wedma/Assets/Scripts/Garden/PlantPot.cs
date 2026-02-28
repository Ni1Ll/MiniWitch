using UnityEngine;

public class PlantPot : MonoBehaviour
{
    [Header("Настройки")]
    public Transform plantVisual; 
    public float growthSpeed = 10f; 

    private bool hasSeed = false;
    private bool isWatered = false;
    private float currentGrowth = 0f;

    void Start()
    {
        if (plantVisual != null)
            plantVisual.localScale = Vector3.zero;
    }

    void Update()
    {
        if (hasSeed && isWatered && currentGrowth < 100f)
        {
            currentGrowth += growthSpeed * Time.deltaTime;
            UpdateVisual();
        }
    }

    public void Interact(ItemType itemInHand)
    {
        if (!hasSeed && itemInHand == ItemType.Seeds)
        {
            hasSeed = true;
            Debug.Log("Семя посажено!");
        }
        else if (hasSeed && !isWatered && itemInHand == ItemType.WateringCan)
        {
            isWatered = true;
            Debug.Log("Полито! Растение начало расти.");
        }
        else if (currentGrowth >= 100f)
        {
            Harvest();
        }
    }

    void UpdateVisual()
    {
        if (plantVisual != null)
        {
            float finalHeight = 2f; 
            float thickness = 0.5f;   

            float currentHeight = (currentGrowth / 100f) * finalHeight;

            plantVisual.localScale = new Vector3(thickness, currentHeight, thickness);

            plantVisual.localPosition = new Vector3(0, currentHeight, 0);
        }
    }

    void Harvest()
    {
        Debug.Log("Урожай собран! +1 Цветок в инвентарь.");
        hasSeed = false;
        isWatered = false;
        currentGrowth = 0f;
        plantVisual.localScale = Vector3.zero;
    }
}