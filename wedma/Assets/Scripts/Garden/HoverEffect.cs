using UnityEngine;

public class HoverEffect : MonoBehaviour
{
    [Header("Настройки левитации")]
    public float amplitude = 0.15f; // Насколько высоко/низко будет летать
    public float speed = 2f;        // Скорость покачивания
    public float rotationSpeed = 40f; // Скорость вращения вокруг своей оси

    private Vector3 startPos;
    private Rigidbody rb;

    void Start()
    {
        // Запоминаем точку, в которой появились
        startPos = transform.position;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Отключаем физику падения
        }
    }

    void Update()
    {
        // 1. Плавное движение вверх-вниз (используем математический синус)
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 2. Красивое вращение
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}