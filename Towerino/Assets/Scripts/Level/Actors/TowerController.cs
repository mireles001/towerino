using System.Collections;
using UnityEngine;

namespace Towerino
{
    public class TowerController : MonoBehaviour
    {
        public TowerType TowerType { get { return _towerType; } }

        [SerializeField] private TowerType _towerType;
        [SerializeField] private float _attackRange = 5;
        [SerializeField] private float _attackInterval = 2;
        [SerializeField] private GameObject _projectilePrefab = null;
        [SerializeField] private Transform _projectileAnchor = null;
        [SerializeField, Tooltip("Required horizontal movement")] private Transform _yRotator = null;
        [SerializeField, Tooltip("Optional vertical movement")] private Transform _xRotator = null;
        [SerializeField] private float _idleSpeed = 100f;
        [SerializeField] private float _aimSpeed = 10f;

        private bool _ready;
        private float _intervalTimer;
        private BaseProjectile _currentProjectile;
        private EnemyController _currentTarget;
        private TowerBaseController _currentTowerBase;

        public void TurnOn(TowerBaseController towerBase)
        {
            _intervalTimer = _attackInterval;
            _currentTowerBase = towerBase;
            transform.position = _currentTowerBase.transform.position;

            gameObject.transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
            LeanTween.scale(gameObject, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
            {
                _ready = true;
                ReadyProjectile();
            });
        }

        public void TurnOff(bool instant = false)
        {
            StopCoroutine(FireProjectile());

            _ready = false;

            if (_currentProjectile != null) _currentProjectile.TurnOff(true);
            _currentProjectile = null;
            _currentTarget = null;
            _currentTowerBase = null;

            if (!instant)
            {
                LeanTween.scale(gameObject, Vector3.zero, 0.3f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
                {
                    gameObject.SetActive(false);
                    GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
                });
            }
            else
            {
                gameObject.SetActive(false);
                GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
            }
        }

        private void Update()
        {
            if (!_ready) return;

            _intervalTimer -= Time.deltaTime;

            if (HasNoTarget())
            {
                IdleMovement(Time.deltaTime);
                LookForTarget();
            }
            else
            {
                LookAtTarget(Time.deltaTime);

                if (_intervalTimer <= 0 && _currentProjectile != null)
                {
                    _intervalTimer = _attackInterval;
                    StartCoroutine(FireProjectile());
                }
            }
        }

        private void ReadyProjectile()
        {
            if (_currentProjectile != null) _currentProjectile.TurnOff(true);

            _currentProjectile = Instantiate(_projectilePrefab, _projectileAnchor).GetComponent<BaseProjectile>();
            //_currentProjectile = GameMaster.Instance.Gameplay.ActivePoolingSystem.GetObject(_projectilePrefab, _projectileAnchor).GetComponent<BaseProjectile>();

            _currentProjectile.TurnOn(this);
        }

        private IEnumerator FireProjectile()
        {
            _currentProjectile.Fire(_currentTarget);
            _currentProjectile = null;

            yield return new WaitForSeconds(_attackInterval / 2);

            ReadyProjectile();
        }

        private void IdleMovement(float timeDelta)
        {
            float rotationSpeed = _idleSpeed * timeDelta;

            _yRotator.localEulerAngles = new Vector3(0, _yRotator.localEulerAngles.y + rotationSpeed, 0);

            if (_xRotator != null && _xRotator.localRotation != Quaternion.identity)
            {
                _xRotator.localRotation = Quaternion.Slerp(_xRotator.localRotation, Quaternion.identity, rotationSpeed);
            }
        }

        private void LookAtTarget(float timeDelta)
        {
            float rotationSpeed = _aimSpeed * timeDelta;

            Vector3 yRotation = Quaternion.LookRotation(_currentTarget.BoundsCenter - _yRotator.position).eulerAngles;
            yRotation.x = yRotation.z = 0;
            _yRotator.rotation = Quaternion.Slerp(_yRotator.rotation, Quaternion.Euler(yRotation), rotationSpeed);

            if (_xRotator != null)
            {
                Vector3 xRotation = Quaternion.LookRotation(_currentTarget.BoundsCenter - _xRotator.position).eulerAngles;
                xRotation.z = 0;
                _xRotator.rotation = Quaternion.Slerp(_xRotator.rotation, Quaternion.Euler(xRotation), rotationSpeed);
            }
        }

        private void LookForTarget()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, _attackRange, transform.forward, 0);
            if (hits.Length > 0)
            {
                int enemyIndex = -1;
                float enemyDistance = Mathf.Infinity;

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.gameObject.tag.Equals("Enemy"))
                    {
                        float hitDistance = Vector3.Distance(transform.position, hits[i].collider.transform.position);
                        if (hitDistance < enemyDistance)
                        {
                            enemyIndex = i;
                            enemyDistance = hitDistance;
                        }
                    }
                }

                if (enemyIndex >= 0)
                {
                    _currentTarget = hits[enemyIndex].collider.gameObject.GetComponent<EnemyController>();
                }
            }
        }

        private bool HasNoTarget()
        {
            return _currentTarget == null || !_currentTarget.IsAlive || _currentTarget.ReachedDestination || Vector3.Distance(transform.position, _currentTarget.transform.position) > _attackRange;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_ready) return;

            if (HasNoTarget())
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, _attackRange);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_projectileAnchor.position, _currentTarget.BoundsCenter);
                Gizmos.DrawWireSphere(_currentTarget.BoundsCenter, 0.75f);
            }
        }
#endif
    }
}