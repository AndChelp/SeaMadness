using UnityEngine;

namespace Cannon.Cannonball
{
    public abstract class AbstractCannonball : MonoBehaviour
    {
        public uint ownerNetId;
        public int damageAmount;
        public double cooldown;
        public int weight;
        public int maxDistance;
        public float velocity;

        protected abstract void PlayExplosion();

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
    }
}