using System.Collections;
using UnityEngine;

namespace Towerino
{
    // Projectiles shot by towers controller. Handles the movement, hit validation and damage calculation upon hit.
    public class BaseProjectile : MonoBehaviour
    {
        [SerializeField]
        protected ProjectileType _type = ProjectileType.ballista;
        [SerializeField]
        protected float _impactDamage = 2;
        [SerializeField, Tooltip("If zero damage is applied entirely upon direct hit")]
        protected float _damageRadius = 1;
        [SerializeField]
        protected float _speed = 1;
        [SerializeField]
        protected float _turnOffProjectileDuration = 0.15f;

        [SerializeField]
        private float _autoKillTimer = 5;
        [SerializeField]
        private Transform _visualObject = null;
        [SerializeField]
        private TrailRenderer _trailFx = null;
        [SerializeField]
        private ParticleSystem _impactFx = null;

        private bool _fired, _hitDetected;
        private int _tweeningId;
        private Vector3 _baseScale;
        private Collider _collider;
        private ICallableTowerFx _towerFx;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
            gameObject.transform.localScale = Vector3.one;
            _baseScale = _visualObject.localScale;
        }

        // Base update function, using virtual UpdateModifier function so it can be overridden in other
        // projectile variations.
        private void Update()
        {
            if (!_fired || _hitDetected || !gameObject.activeInHierarchy) return;

            UpdateModifier(Time.deltaTime);
        }

        // Start up projectile, set it up in our parent tower anchor point
        // eg: The ballista start in its position before getting fired for some seconds until
        // tower fires it.
        public void TurnOn(TowerController parentTower)
        {
            _fired = false;
            _collider.enabled = true;

            if (_trailFx != null) _trailFx.gameObject.SetActive(false);

            gameObject.SetActive(true);

            if (_tweeningId != 0 && LeanTween.isTweening(_tweeningId)) LeanTween.cancel(_tweeningId);

            _visualObject.localScale = Vector3.zero;
            _tweeningId = LeanTween.scale(_visualObject.gameObject, _baseScale, 0.25f).setEase(LeanTweenType.easeOutBack).id;

            _towerFx = parentTower.GetComponent<ICallableTowerFx>();
            if (_towerFx != null) _towerFx.ProjectileReady();
        }

        // Turn off this projective object. Stopping autodestroy coroutine, canceling tweening effects and animating it
        // to shrink then destroy (soon to be added pooling system handler)
        public virtual void TurnOff(bool instant = false)
        {
            StopCoroutine(AutoKill());

            if (LeanTween.isTweening(_tweeningId)) LeanTween.cancel(_tweeningId);

            if (!_fired) transform.SetParent(GameMaster.Instance.Gameplay.ActivePoolingSystem.PoolingWrapper);

            if (!instant)
            {
                _tweeningId = LeanTween.scale(_visualObject.gameObject, Vector3.zero, _turnOffProjectileDuration).setOnComplete(() =>
                {
                    //gameObject.SetActive(false);
                    //GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
                    Destroy(gameObject);
                }).id;
            }
            else
            {
                //gameObject.SetActive(false);
                //GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
                Destroy(gameObject);
            }
        }

        public virtual void Fire(EnemyController targetedEnemy)
        {
            // We set projectile flag as true and make transform to be facing its initial target's center
            _fired = true;
            transform.SetParent(GameMaster.Instance.Gameplay.ActivePoolingSystem.PoolingWrapper);
            transform.LookAt(targetedEnemy.BoundsCenter);

            // If we have a trail effect we turn it on and begin emitting trail
            if (_trailFx != null)
            {
                _trailFx.gameObject.SetActive(true);
                _trailFx.emitting = true;
            }

            // If we have projectile FX for a interface extra component in our parent tower, we
            // trigger its function ProjectileFired. This give us specific effects between towers
            if (_towerFx != null) _towerFx.ProjectileFired();

            // Not sure why im covering the validation if the gameobject is not active.
            // It should be active at this point!
            if (gameObject.activeInHierarchy) StartCoroutine(AutoKill());
            else StopCoroutine(AutoKill());
        }

        protected virtual void Hit(EnemyController enemyDirectHit = null)
        {
            if (!gameObject.activeInHierarchy) return;

            // Creates new instance or reuse a pooled object of the impact fx gameobject (if any)
            if (_impactFx != null)
            {
                Transform impact = GameMaster.Instance.Gameplay.ActivePoolingSystem.GetObject(_impactFx.gameObject, GameMaster.Instance.Gameplay.ActivePoolingSystem.PoolingWrapper).transform;
                impact.SetPositionAndRotation(transform.position, Quaternion.identity);
                impact.gameObject.SetActive(true);
                impact.GetComponent<ParticleSystem>().Play();
            }

            // Release the trail effect attached to this object
            if (_trailFx != null) _trailFx.transform.SetParent(null);

            // If we have a direct hit we apply damage entirely
            if (enemyDirectHit != null && _damageRadius == 0)
            {
                enemyDirectHit.ApplyDamage(_impactDamage);
            }
            // If we have splash damage (damage radius above 0) we check for impact distance between
            // projectile and nearby enemies
            else if (_damageRadius > 0)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, _damageRadius);

                EnemyController splashDamageEnemy;
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].gameObject.tag.Equals("Enemy"))
                    {
                        splashDamageEnemy = colliders[i].gameObject.GetComponent<EnemyController>();

                        // If we have a direct hit enemy, we apply damage entirely to this enemy
                        if (enemyDirectHit != null && enemyDirectHit == splashDamageEnemy)
                        {
                            enemyDirectHit.ApplyDamage(_impactDamage);
                        }
                        // If the enemy is nearby we apply a proportional damage depending in distance
                        // from hit position.
                        else
                        {
                            splashDamageEnemy.ApplyDamage(GetDamage(Vector3.Distance(transform.position, splashDamageEnemy.BoundsCenter)));
                        }
                    }

                }
            }

            if (_towerFx != null) _towerFx.ProjectileHit();

            TurnOff();
        }

        // Base linear projectile position modifier
        protected virtual void UpdateModifier(float timeDelta)
        {
            transform.Translate(Vector3.forward * _speed * timeDelta);
        }

        // Get splash damage depending in distance from targeted and hit point
        private float GetDamage(float hitDistance)
        {
            return (1 - Mathf.Clamp(hitDistance, 0, _damageRadius) / _damageRadius) * _impactDamage;
        }

        // If the projectile didnt hit anything, we auto kill it after X time.
        private IEnumerator AutoKill()
        {
            if (!gameObject.activeInHierarchy || _hitDetected) StopCoroutine(AutoKill());

            yield return new WaitForSeconds(_autoKillTimer);

            TurnOff();
        }

        // Checks for collision with something that is not a tower (ground or enemies)
        // Calls Hit with a direct hit enemy or a null value if hit the ground.
        private void OnTriggerEnter(Collider other)
        {
            if (_hitDetected || other.gameObject.tag.Equals("Tower")) return;

            _hitDetected = true;
            _collider.enabled = false;

            EnemyController directHitEnemy;
            if (other.gameObject.tag.Equals("Enemy"))
            {
                directHitEnemy = other.gameObject.GetComponent<EnemyController>();
            }
            else directHitEnemy = null;

            Hit(directHitEnemy);
        }
    }
}
