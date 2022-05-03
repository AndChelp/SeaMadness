using System;
using UnityEngine;

namespace Cannon
{
    public class CannonBall : MonoBehaviour
    {
        public int damageAmount = 10;
        public uint ownerNetId;

        public ParticleSystem explosionParticles;


        private void FixedUpdate()
        {
            if (transform.position.y < -3)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.collider.CompareTag("ship")) return;
            Debug.Log("Cannonball collided with a ship");
            PlayExplosion();
            Destroy(gameObject);
        }

        private void PlayExplosion()
        {
            explosionParticles.transform.parent = null;
            explosionParticles.Play();
        }
    }
}