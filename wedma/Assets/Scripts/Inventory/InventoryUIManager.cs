using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager instance;

    [Header("Панели интерфейса")]
    public GameObject fullInventoryPanel; // Сюда кидаем большой инвентарь
    public GameObject hotbarPanel;        // Сюда кидаем нижнюю панель (хотбар)

    private bool isInventoryOpen = false;
    private bool isMechanicActive = false;

    void Awake()
    {
        instance = this;
        UpdateUIVisibility(); // Прячем всё при старте игры
    }

    void Update()
    {
        // Открытие/закрытие большого инвентаря на Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isInventoryOpen = !isInventoryOpen;
            UpdateUIVisibility();
        }
    }

    // Эту функцию будет дергать котел и другие рабочие станции
    public void SetMechanicMode(bool isActive)
    {
        isMechanicActive = isActive;
        UpdateUIVisibility();
    }

    private void UpdateUIVisibility()
    {
        if (fullInventoryPanel != null)
            fullInventoryPanel.SetActive(isInventoryOpen);

        if (hotbarPanel != null)
            hotbarPanel.SetActive(isInventoryOpen || isMechanicActive);
    }
}