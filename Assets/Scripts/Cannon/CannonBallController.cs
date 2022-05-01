using System;
using Mirror;
using UnityEngine;

namespace Cannon
{
    public class CannonBallController : MonoBehaviour
    {
        public int damageAmount = 10;

        private void FixedUpdate()
        {
            if (transform.position.y < 0)
            {
                Destroy(gameObject);
            }
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            Destroy(gameObject);
        }
    }
}