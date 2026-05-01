using UnityEngine;

namespace Invector.vCharacterController
{
    public class vIdleDirectBlendTreeController : MonoBehaviour
    {
        [Header("References")]
        public Animator animator;

        [Header("Settings")]
        public float inputThreshold = 0.1f;

        [Tooltip("Когда готовить следующую idle")]
        [Range(0.7f, 0.99f)]
        public float prepareNextAt = 0.9f;

        [Tooltip("Сглаживание idle")]
        [Range(0.05f, 1f)]
        public float blendTransitionTime = 0.25f;

        [Tooltip("Насколько резко выходить в движение")]
        [Range(0f, 0.2f)]
        public float movementSnap = 0f; // 0 = мгновенно

        private int currentIdleIndex;
        private int lastIdleIndex = -1;
        private int nextIdleIndex = -1;

        private bool nextPrepared = false;
        private int lastLoop = 0;

        private const int idleCount = 8;
        private int[] idleWeightHashes;

        private static readonly int InputMagnitude = Animator.StringToHash("InputMagnitude");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        void Start()
        {
            currentIdleIndex = 0;
            lastIdleIndex = 0;

            idleWeightHashes = new int[idleCount];
            for (int i = 0; i < idleCount; i++)
            {
                idleWeightHashes[i] = Animator.StringToHash("IdleWeight" + i);
            }

            if (animator)
            {
                for (int i = 0; i < idleCount; i++)
                {
                    animator.SetFloat(idleWeightHashes[i], i == 0 ? 1f : 0f);
                }
            }
        }

        void Update()
        {
            if (!animator || !animator.enabled) return;

            float input = animator.GetFloat(InputMagnitude);
            bool grounded = animator.GetBool(IsGrounded);

            // 🚀 ДВИЖЕНИЕ — МГНОВЕННЫЙ ВЫХОД ИЗ IDLE
            if (!grounded || input > inputThreshold)
            {
                currentIdleIndex = 0;
                nextPrepared = false;
                lastLoop = 0;

                // 💥 ЖЁСТКО убираем все idle (без сглаживания)
                for (int i = 0; i < idleCount; i++)
                {
                    float value = (i == 0) ? 1f : 0f;
                    animator.SetFloat(idleWeightHashes[i], value, 0f, Time.deltaTime);
                }

                // 💥 форсим быстрый отклик InputMagnitude (если используется damp)
                animator.SetFloat(InputMagnitude, input, movementSnap, Time.deltaTime);

                return;
            }

            var state = animator.GetCurrentAnimatorStateInfo(0);

            float normalized = state.normalizedTime % 1f;
            int loop = Mathf.FloorToInt(state.normalizedTime);

            // Подготовка следующей idle
            if (!nextPrepared && normalized >= prepareNextAt)
            {
                nextIdleIndex = GetRandomIdleIndex();
                nextPrepared = true;
            }

            // Переключение на новом цикле
            if (nextPrepared && loop > lastLoop)
            {
                lastLoop = loop;

                lastIdleIndex = currentIdleIndex;
                currentIdleIndex = nextIdleIndex;

                nextPrepared = false;
            }

            UpdateBlendTreeWeights(blendTransitionTime);
        }

        private void UpdateBlendTreeWeights(float dampTime)
        {
            for (int i = 0; i < idleCount; i++)
            {
                float targetWeight = (i == currentIdleIndex) ? 1f : 0f;
                animator.SetFloat(idleWeightHashes[i], targetWeight, dampTime, Time.deltaTime);
            }
        }

        int GetRandomIdleIndex()
        {
            if (idleCount <= 1) return 0;

            for (int i = 0; i < 100; i++)
            {
                int newIdle = Random.Range(0, idleCount);

                if (newIdle != currentIdleIndex && newIdle != lastIdleIndex)
                    return newIdle;
            }

            return (currentIdleIndex + 1) % idleCount;
        }
    }
}