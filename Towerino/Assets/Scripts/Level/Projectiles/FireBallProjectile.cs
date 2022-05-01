using UnityEngine;

namespace Towerino
{
    public class FireBallProjectile : BaseProjectile
    {
        [SerializeField] private float _fireAheadMultiplier = 0.5f;
        [SerializeField] private ParticleSystem _fireTrail = null;
        [SerializeField] private ParticleSystem _smokeTrail = null;
        [SerializeField] private GameObject _fireGround = null;

        private Rigidbody _rigidBody;

        protected override void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _type = ProjectileType.fireBomb;
            base.Awake();
        }

        public override void Fire(EnemyController targetedEnemy)
        {
            base.Fire(targetedEnemy);

            _smokeTrail.Play();

            _rigidBody.isKinematic = false;
            _rigidBody.useGravity = true;

            Vector3 velocity = BallisticVel(targetedEnemy.BoundsCenter + targetedEnemy.transform.forward * (targetedEnemy.EnemySpeed * _fireAheadMultiplier));

            if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z))
            {
                Debug.Log("<color=orange>WARNING: Error in velocity. Invalid NaN values detected!</color>");
                Hit();
            }
            else _rigidBody.velocity = velocity;
        }

        protected override void Hit(EnemyController enemyDirectHit = null)
        {
            _fireTrail.transform.SetParent(null);
            _fireTrail.Stop();

            _smokeTrail.transform.SetParent(null);
            _smokeTrail.Stop();

            _rigidBody.isKinematic = true;
            _rigidBody.useGravity = false;
            base.Hit(enemyDirectHit);
        }

        protected override void UpdateModifier(float timeDelta) { }

        public override void TurnOff(bool instant = false)
        {
            if (!instant) CreateFireGround();
            base.TurnOff(instant);
        }

        private void CreateFireGround()
        {
            //Debug.Log("Fire ground");
        }

        // As seen in aldonaletto's answer
        // https://answers.unity.com/questions/145972/how-to-make-enemy-canon-ball-fall-on-mooving-targe.html
        private Vector3 BallisticVel(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            float directionHeight = direction.y;
            direction.y = 0;
            float distance = direction.magnitude;
            direction.y = distance;
            distance += directionHeight;
            return Mathf.Sqrt(distance * Physics.gravity.magnitude) * direction.normalized;
        }
    }
}
