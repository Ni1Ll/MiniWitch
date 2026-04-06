using UnityEngine;

// Эта строчка добавит менюшку по правому клику
[CreateAssetMenu(fileName = "New Potion", menuName = "Garden/Potion Data")]
public class PotionData : ItemData // Наследуем от базового предмета!
{
    [Header("Эффекты Зелья (На будущее)")]
    public int healAmount = 10; // Сколько здоровья восстановит
    public float speedBoost = 0f; // Даст ли ускорение

    // Позже мы научим Ведьму пить эти зелья и будем брать цифры отсюда
}