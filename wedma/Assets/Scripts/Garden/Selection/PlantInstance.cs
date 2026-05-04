using System.Collections.Generic;

[System.Serializable]
public class PlantInstance
{
    public PlantData baseData;
    public PlantFamily family;

    public List<Gene> activeGenes = new List<Gene>();
    public List<Gene> dormantGenes = new List<Gene>();

    public PlantInstance(PlantData data)
    {
        baseData = data;
        family = data.family;

        GeneticsCore.GenerateGenes(this);
    }
}