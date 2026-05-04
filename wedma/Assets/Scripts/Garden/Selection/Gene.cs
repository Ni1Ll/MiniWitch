using System;

[System.Serializable]
public class Gene
{
    public GeneType type;
    public int value;

    public Gene(GeneType type, int value)
    {
        this.type = type;
        this.value = value;
    }
}