using UnityEngine;

namespace Towerino
{
    public class CannonTowerFx : MonoBehaviour, ICallableTowerFx
    {
        [SerializeField] private ParticleSystem _sparksFx = null;
        public void ProjectileReady()
        {
            _sparksFx.Play();
        }

        public void ProjectileFired()
        {
            _sparksFx.Stop();
        }

        public void ProjectileHit() { }
    }
}
