using UnityEngine;

namespace Towerino
{
    // Handles some special effects for the firebomb tower
    public class FireBombTowerFx : MonoBehaviour, ICallableTowerFx
    {
        [SerializeField]
        private Animator _catapultAnimator = null;

        private int _animFire = Animator.StringToHash("fire");

        public void ProjectileReady() { }

        // Triggers the animation clip from animator component that moves the catapult when
        // tower fires
        public void ProjectileFired()
        {
            _catapultAnimator.SetTrigger(_animFire);
        }

        public void ProjectileHit() { }
    }
}
