using Common;
using UnityEngine;

namespace Cannon.Cannonball {
    public abstract class AbstractCannonball : MonoBehaviour, IItem {
        public uint ownerNetId;
        public int damageAmount;
        public double cooldown;
        [SerializeField] private int maxCount;
        public int maxDistance;
        public float velocity;
        public ParticleSystem particles;

        private void FixedUpdate() {
            if (transform.position.y < -3){
                Destroy(gameObject);
            }
        }

        public int maxCountProperty {
            get => maxCount;
            set => maxCount = value;
        }

        private void OnCollisionEnter(Collision other) {
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