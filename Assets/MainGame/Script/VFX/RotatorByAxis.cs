using UnityEngine;

namespace Tag.Block
{
    public class RotatorByAxis : MonoBehaviour
    {
        public Vector3 direction = new Vector3(0, 0, 1f);
        public float speed = 1f;

        private void LateUpdate()
        {
            transform.Rotate(direction * (speed * Time.deltaTime * 100f));
        }
    }
}