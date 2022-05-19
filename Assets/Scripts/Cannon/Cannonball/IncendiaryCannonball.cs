using UnityEngine;

namespace Cannon.Cannonball {
    public class IncendiaryCannonball : AbstractCannonball {
        protected override void PlayExplosion(GameObject other) {
            particles.transform.parent = other.transform;
            particles.Play();
        }
    }
}