using UnityEngine;

namespace TowerDefense.Utils
{
    public class PulseAnimation : MonoBehaviour
    {
        [SerializeField] private float pulseSpeed = 2.5f;
        [SerializeField] private float pulseAmount = 0.05f;

        private Vector3 _baseScale;

        private void Start()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = _baseScale * pulse;
        }
    }
}
