using UnityEngine;
using TMPro; // Для текста

public class Tooltip : MonoBehaviour
{
    // Делаем скрипт "Одиночкой" (Singleton), чтобы любой слот мог легко его вызвать
    public static Tooltip instance;

    public TextMeshProUGUI tooltipText; // Сам текст

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false); // Прячем тултип при старте
    }

    void Update()
    {
        // Заставляем тултип летать прямо за курсором мыши
        transform.position = Input.mousePosition;
    }

    public void ShowTooltip(string text)
    {
        gameObject.SetActive(true);
        tooltipText.text = text;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}