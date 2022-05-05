using UnityEngine;

namespace Cannon.Cannonball
{
    public class ChainCannonball : AbstractCannonball
    {
        public ParticleSystem explosionParticles;

        protected override void PlayExplosion()
        {
            explosionParticles.transform.parent = null;
            explosionParticles.Play();
        }
    }
}