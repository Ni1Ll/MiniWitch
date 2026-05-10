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

    private const int ActiveGeneValue = 10;
    private const int DormantGeneMinValue = 1;
    private const int DormantGeneMaxValue = 3;

    public static void GenerateGenes(PlantInstance plant)
    {
        if (plant == null)
        {
            Debug.LogWarning("[GeneticsCore] PlantInstance is null.");
            return;
        }

        if (plant.baseData == null)
        {
            Debug.LogWarning("[GeneticsCore] PlantInstance has no baseData.");
            return;
        }

        if (!familyGenes.ContainsKey(plant.family))
        {
            Debug.LogWarning($"[GeneticsCore] Нет списка генов для семейства: {plant.family}");
            return;
        }

        plant.activeGenes.Clear();
        plant.dormantGenes.Clear();

        // 1. Первый активный ген — конкретный ген растения
        GeneType activeGene = plant.baseData.defaultActiveGene;
        plant.activeGenes.Add(new Gene(activeGene, ActiveGeneValue));

        // 2. Второй активный слот пока пустой.
        // Мы НЕ добавляем Empty в список, чтобы не ломать логику.
        // UI просто будет показывать "Пусто" как второй слот.

        // 3. Спящие гены — 5 случайных из семейства
        List<GeneType> pool = new List<GeneType>(familyGenes[plant.family]);

        // Убираем врождённый активный ген, чтобы он не дублировался в спящих
        pool.Remove(activeGene);

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