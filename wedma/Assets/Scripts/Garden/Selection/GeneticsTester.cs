using UnityEngine;

public class GeneticsTester : MonoBehaviour
{
    public PlantData testPlant;

    void Start()
    {
        PlantInstance plant = new PlantInstance(testPlant);

        Debug.Log("=== ACTIVE GENES ===");
        foreach (var g in plant.activeGenes)
        {
            Debug.Log(g.type + " +" + g.value);
        }

        Debug.Log("=== DORMANT GENES ===");
        foreach (var g in plant.dormantGenes)
        {
            Debug.Log(g.type + " +" + g.value);
        }
    }
}