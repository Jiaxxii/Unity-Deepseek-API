using UnityEngine;

namespace Xiyu
{
    public class Loading : MonoBehaviour
    {
        [SerializeField] private float speed;

        public bool IsRun { get; set; }

        private void Start()
        {
            Application.targetFrameRate = 90;
        }

        private void Update()
        {
            if (IsRun)
                transform.eulerAngles += Mathf.Sin(Time.time) * Time.deltaTime * speed * Vector3.forward;
        }
    }
}