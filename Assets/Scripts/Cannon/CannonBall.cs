using Mirror;
using UnityEngine;

namespace Cannon
{
    public class CannonBall : MonoBehaviour
    {
        public int damageAmount = 10;
        public uint ownerNetId;

        private void FixedUpdate()
        {
            if (transform.position.y < 0)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Destroy(gameObject);
        }
    }
}