using UnityEngine;

namespace Towerino
{
    public class ProjectileImpact : MonoBehaviour
    {
        public void OnParticleSystemStopped() { TurnOff(); }

        public void TurnOff(bool instant = false)
        {
            gameObject.SetActive(false);
            GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
        }
    }
}
