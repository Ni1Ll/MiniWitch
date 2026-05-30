using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlantModificationUI : MonoBehaviour
{
    public static PlantModificationUI instance;

    [Header("Root")]
    public GameObject panel;

    [Header("Активные гены")]
    public Button[] activeButtons = new Button[2];
    public TextMeshProUGUI[] activeTexts = new TextMeshProUGUI[2];
    public GameObject[] activeHighlights = new GameObject[2]; 

    [Header("Спящие гены")]
    public Button[] dormantButtons = new Button[5];
    public TextMeshProUGUI[] dormantTexts = new TextMeshProUGUI[5];
    public GameObject[] dormantHighlights = new GameObject[5]; 
    [Header("Управление")]
    public Button crossButton; 

    private PlantInstance currentPlant;
    
    private int selectedActiveIndex = -1;
    private int selectedDormantIndex = -1;

    void Awake()
    {
        instance = this;

        for (int i = 0; i < activeButtons.Length; i++)
        {
            int index = i;
            if (activeButtons[i] != null)
                activeButtons[i].onClick.AddListener(() => SelectActiveGene(index));
        }

        for (int i = 0; i < dormantButtons.Length; i++)
        {
            int index = i;
            if (dormantButtons[i] != null)
                dormantButtons[i].onClick.AddListener(() => SelectDormantGene(index));
        }

        if (crossButton != null)
            crossButton.onClick.AddListener(ApplyCross);
    }

    public void Open(PlantInstance plant)
    {
        if (plant == null) return;

        currentPlant = plant;
        
        selectedActiveIndex = -1;
        selectedDormantIndex = -1;

        if (panel != null) panel.SetActive(true);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (currentPlant == null) return;

        for (int i = 0; i < activeButtons.Length; i++)
        {
            bool hasGene = currentPlant.activeGenes != null && i < currentPlant.activeGenes.Count;
            
            if (activeButtons[i] != null) 
                activeButtons[i].interactable = hasGene;
                
            if (activeHighlights[i] != null) 
                activeHighlights[i].SetActive(i == selectedActiveIndex);

            if (activeTexts[i] != null)
            {
                if (hasGene) activeTexts[i].text = $"{GeneName.Ru(currentPlant.activeGenes[i].type)} +{currentPlant.activeGenes[i].value}";
                else activeTexts[i].text = "Пусто";
            }
        }

        for (int i = 0; i < dormantButtons.Length; i++)
        {
            bool hasGene = currentPlant.dormantGenes != null && i < currentPlant.dormantGenes.Count;
            bool isReadyForCross = false;

            if (hasGene)
            {
                Gene gene = currentPlant.dormantGenes[i];
                isReadyForCross = (gene.value == 10); 
                
                if (dormantTexts[i] != null) dormantTexts[i].text = $"{GeneName.Ru(gene.type)} +{gene.value}";
            }
            else
            {
                if (dormantTexts[i] != null) dormantTexts[i].text = "-";
            }

            if (dormantButtons[i] != null) 
                dormantButtons[i].interactable = hasGene && isReadyForCross;

            if (dormantHighlights[i] != null) 
                dormantHighlights[i].SetActive(i == selectedDormantIndex);
        }

        if (crossButton != null)
            crossButton.interactable = (selectedActiveIndex != -1 && selectedDormantIndex != -1);
    }

    private void SelectActiveGene(int index)
    {
        if (selectedActiveIndex == index) selectedActiveIndex = -1;
        else selectedActiveIndex = index;

        RefreshUI();
    }

    private void SelectDormantGene(int index)
    {
        if (currentPlant.dormantGenes[index].value < 10) return;

        if (selectedDormantIndex == index) selectedDormantIndex = -1;
        else selectedDormantIndex = index;

        RefreshUI();
    }

    private void ApplyCross()
    {
        if (selectedActiveIndex == -1 || selectedDormantIndex == -1) return;

        Gene activeGene = currentPlant.activeGenes[selectedActiveIndex];
        Gene dormantGene = currentPlant.dormantGenes[selectedDormantIndex];

        Debug.Log($"[Модификация] Меняем: {GeneName.Ru(activeGene.type)} ↔ {GeneName.Ru(dormantGene.type)}");

        GeneType oldActiveType = activeGene.type;

        activeGene.type = dormantGene.type;
        activeGene.value = 10; 

        dormantGene.type = oldActiveType;
        dormantGene.value = 10; 

        selectedActiveIndex = -1;
        selectedDormantIndex = -1;

        RefreshUI();
    }

    public void Close()
    {
        selectedActiveIndex = -1;
        selectedDormantIndex = -1;
        currentPlant = null;

        if (panel != null) panel.SetActive(false);
    }
}