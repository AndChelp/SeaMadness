using UnityEngine;

namespace Cannon.Cannonball
{
    public class CommonCannonball : AbstractCannonball
    {
        public ParticleSystem explosionParticles;

        protected override void PlayExplosion()
        {
            explosionParticles.transform.parent = null;
            explosionParticles.Play();
        }
    }
}