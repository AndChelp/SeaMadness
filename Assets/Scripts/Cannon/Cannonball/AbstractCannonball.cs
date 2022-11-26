using Common;
using UnityEngine;

namespace Cannon.Cannonball {
    public abstract class AbstractCannonball : MonoResource {
        public uint ownerNetId;
        public int damageAmount;
        public double cooldown;
        public int maxDistance;
        public float velocity;
        public ParticleSystem particles;

        void FixedUpdate() {
            if (transform.position.y < -3) {
                Destroy(gameObject);
            }
        }


        void OnCollisionEnter(Collision other) {
            if (!other.collider.CompareTag("ship")) return;
            Debug.Log("Cannonball collided with a ship");
            PlayExplosion(other.gameObject);
            Destroy(gameObject);
        }

        protected virtual void PlayExplosion(GameObject other) {
            particles.transform.parent = other.transform;
            particles.Play();
        }
    }
}