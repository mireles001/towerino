using System.Collections;
using UnityEngine;

namespace Towerino
{
    public class BaseProjectile : MonoBehaviour
    {
        [SerializeField] protected ProjectileType _type = ProjectileType.ballista;
        [SerializeField] protected float _impactDamage = 2;
        [SerializeField, Tooltip("If zero damage is applied entirely upon direct hit")] protected float _damageRadius = 1;
        [SerializeField] protected float _speed = 1;
        [SerializeField] protected float _turnOffProjectileDuration = 0.15f;

        [SerializeField] private float _autoKillTimer = 5;
        [SerializeField] private Transform _visualObject = null;
        [SerializeField] private TrailRenderer _trailFx = null;
        [SerializeField] private ParticleSystem _impactFx = null;

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
            _fired = true;
            transform.SetParent(GameMaster.Instance.Gameplay.ActivePoolingSystem.PoolingWrapper);
            transform.LookAt(targetedEnemy.BoundsCenter);

            if (_trailFx != null)
            {
                _trailFx.gameObject.SetActive(true);
                _trailFx.emitting = true;
            }

            if (_towerFx != null) _towerFx.ProjectileFired();

            if (gameObject.activeInHierarchy) StartCoroutine(AutoKill());
            else StopCoroutine(AutoKill());
        }

        private IEnumerator AutoKill()
        {
            if (!gameObject.activeInHierarchy || _hitDetected) StopCoroutine(AutoKill());

            yield return new WaitForSeconds(_autoKillTimer);

            TurnOff();
        }

        protected virtual void Hit(EnemyController enemyDirectHit = null)
        {
            if (!gameObject.activeInHierarchy) return;

            if (_impactFx != null)
            {
                //Instantiate(_impactFx, transform.position, Quaternion.identity).Play();
                Transform impact = GameMaster.Instance.Gameplay.ActivePoolingSystem.GetObject(_impactFx.gameObject, GameMaster.Instance.Gameplay.ActivePoolingSystem.PoolingWrapper).transform;
                impact.SetPositionAndRotation(transform.position, Quaternion.identity);
                impact.gameObject.SetActive(true);
                impact.GetComponent<ParticleSystem>().Play();
            }

            if (_trailFx != null) _trailFx.transform.SetParent(null);

            if (enemyDirectHit != null && _damageRadius == 0)
            {
                enemyDirectHit.ApplyDamage(_impactDamage);
            }
            else if (_damageRadius > 0)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, _damageRadius);

                EnemyController splashDamageEnemy;
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].gameObject.tag.Equals("Enemy"))
                    {
                        splashDamageEnemy = colliders[i].gameObject.GetComponent<EnemyController>();

                        if (enemyDirectHit != null && enemyDirectHit == splashDamageEnemy)
                        {
                            enemyDirectHit.ApplyDamage(_impactDamage);
                        }
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

        protected virtual void UpdateModifier(float timeDelta)
        {
            transform.Translate(Vector3.forward * _speed * timeDelta);
        }

        private void Update()
        {
            if (!_fired || _hitDetected || !gameObject.activeInHierarchy) return;

            UpdateModifier(Time.deltaTime);
        }

        private float GetDamage(float hitDistance)
        {
            return (1 - Mathf.Clamp(hitDistance, 0, _damageRadius) / _damageRadius) * _impactDamage;
        }

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
