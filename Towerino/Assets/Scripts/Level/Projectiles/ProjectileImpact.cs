using UnityEngine;

namespace Towerino
{
    // Script component for small gameobject that is used when the projectile hits a target or the ground
    // Used to check when its inactive and return it to the PoolingSystem
    public class ProjectileImpact : MonoBehaviour
    {
        public void OnParticleSystemStopped() { TurnOff(); }

        // Triggered when the particle effect stopped
        public void TurnOff(bool instant = false)
        {
            gameObject.SetActive(false);
            GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
        }
    }
}
