using UnityEngine;

namespace Towerino
{
    public class FireBombTowerFx : MonoBehaviour, ICallableTowerFx
    {
        [SerializeField]
        private Animator _catapultAnimator = null;

        private int _animFire = Animator.StringToHash("fire");

        public void ProjectileReady() { }

        public void ProjectileFired()
        {
            _catapultAnimator.SetTrigger(_animFire);
        }

        public void ProjectileHit() { }
    }
}
