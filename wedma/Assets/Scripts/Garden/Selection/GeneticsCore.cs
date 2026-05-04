using System.Collections.Generic;
using UnityEngine;

public static class GeneticsCore
{
    // --- ГЕНЫ ПО СЕМЕЙСТВАМ ---

    public static Dictionary<PlantFamily, List<GeneType>> familyGenes = new Dictionary<PlantFamily, List<GeneType>>()
    {
        {
            PlantFamily.FieldFamily,
            new List<GeneType>
            {
                GeneType.Vigor,
                GeneType.Calm,
                GeneType.Recovery,
                GeneType.Energy,
                GeneType.Endurance,
                GeneType.Concentration,
                GeneType.Fortitude,
                GeneType.Clarity,
                GeneType.Assimilation
            }
        },
        {
            PlantFamily.SunMoonFamily,
            new List<GeneType>
            {
                GeneType.Acceleration,
                GeneType.Slowdown,
                GeneType.FireResist,
                GeneType.ColdResist,
                GeneType.Cleansing,
                GeneType.Decay,
                GeneType.Fermentation,
                GeneType.Sensitivity,
                GeneType.Stimulation
            }
        },
        {
            PlantFamily.NobleFamily,
            new List<GeneType>
            {
                GeneType.Charm,
                GeneType.Oblivion,
                GeneType.Insight,
                GeneType.Confidence,
                GeneType.Charisma,
                GeneType.Fun,
                GeneType.Fortune,
                GeneType.Inspiration,
                GeneType.Logic
            }
        },
        {
            PlantFamily.SpringFamily,
            new List<GeneType>
            {
                GeneType.Scaring,
                GeneType.Attraction,
                GeneType.Toxic,
                GeneType.Suppression,
                GeneType.Disorientation,
                GeneType.Stickiness,
                GeneType.Paralysis,
                GeneType.Distortion,
                GeneType.Masking
            }
        }
    };

    // --- АКТИВНЫЕ ГЕНЫ ПО СЕМЕЙСТВАМ ---

    public static Dictionary<PlantFamily, GeneType[]> activeGenesMap = new Dictionary<PlantFamily, GeneType[]>()
    {
        { PlantFamily.FieldFamily, new[] { GeneType.Vigor, GeneType.Calm } },
        { PlantFamily.SunMoonFamily, new[] { GeneType.Acceleration, GeneType.Slowdown } },
        { PlantFamily.NobleFamily, new[] { GeneType.Charm, GeneType.Oblivion } },
        { PlantFamily.SpringFamily, new[] { GeneType.Scaring, GeneType.Attraction } }
    };

    // --- НАСТРОЙКИ ЗНАЧЕНИЙ ---

    private const int ActiveGeneValue = 10;
    private const int DormantGeneMinValue = 1;
    private const int DormantGeneMaxValue = 3;

    // --- ГЕНЕРАЦИЯ ---

    public static void GenerateGenes(PlantInstance plant)
    {
        if (plant == null)
        {
            Debug.LogWarning("[GeneticsCore] PlantInstance is null.");
            return;
        }

        if (!familyGenes.ContainsKey(plant.family))
        {
            Debug.LogWarning($"[GeneticsCore] Нет списка генов для семейства: {plant.family}");
            return;
        }

        if (!activeGenesMap.ContainsKey(plant.family))
        {
            Debug.LogWarning($"[GeneticsCore] Нет активных генов для семейства: {plant.family}");
            return;
        }

        plant.activeGenes.Clear();
        plant.dormantGenes.Clear();

        // 1. Активные гены всегда фиксированные и максимальные
        foreach (GeneType geneType in activeGenesMap[plant.family])
        {
            plant.activeGenes.Add(new Gene(geneType, ActiveGeneValue));
        }

        // 2. Пул спящих генов из семейства
        List<GeneType> pool = new List<GeneType>(familyGenes[plant.family]);

        // Убираем активные из пула, чтобы они не попали в спящие
        foreach (GeneType active in activeGenesMap[plant.family])
        {
            pool.Remove(active);
        }

        // 3. Рандомим 5 уникальных спящих генов
        for (int i = 0; i < 5; i++)
        {
            if (pool.Count == 0) break;

            int index = Random.Range(0, pool.Count);
            GeneType selected = pool[index];
            pool.RemoveAt(index);

            int value = Random.Range(DormantGeneMinValue, DormantGeneMaxValue + 1);

            plant.dormantGenes.Add(new Gene(selected, value));
        }
    }
}