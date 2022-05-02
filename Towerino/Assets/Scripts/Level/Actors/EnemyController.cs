using UnityEngine;
using UnityEngine.AI;

namespace Towerino
{
    public class EnemyController : MonoBehaviour
    {
        public bool ReachedDestination { get; private set; }

        [SerializeField]
        private int _rewardPerKill = 50;
        [SerializeField]
        private float _hp = 1;
        [SerializeField]
        private float _speed = 1;
        [SerializeField]
        private float _reachedDuration = 0.33f;
        [SerializeField]
        private float _deathDuration = 1;
        [SerializeField]
        private Transform _visualAsset = null;
        [SerializeField]
        private NavMeshAgent _agent = null;
        [SerializeField]
        private MeshRenderer _renderer = null;
        [SerializeField]
        private Color _damageColor = Color.red;

        private int _cutoffHeight, _baseColor;
        private float _maxHp;
        private Color[] _materialColors;
        private Material[] _sharedMaterials;
        private Collider _collider;

        public float EnemySpeed
        {
            get { return _speed; }
        }

        public bool IsAlive
        {
            get { return _hp > 0; }
        }
        public float HpPercentage
        {
            get { return _hp / _maxHp; }
        }

        public Vector3 BoundsCenter
        {
            get { return _collider.bounds.center; }
        }

        private void Awake()
        {
            _agent.speed = _speed;
            _maxHp = _hp;
            _collider = GetComponent<Collider>();
            _cutoffHeight = Shader.PropertyToID("Vector1_b59e3bdf317448bd9a65115a1cea1cb1");
            _baseColor = Shader.PropertyToID("Color_36667b28f67e4ffe93dd0988b3e41eea");

            Material[] originalMaterials = _renderer.sharedMaterials;
            _sharedMaterials = new Material[originalMaterials.Length];
            _materialColors = new Color[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                _sharedMaterials[i] = Instantiate(originalMaterials[i]);
                _sharedMaterials[i].SetFloat(_cutoffHeight, 0);
                _materialColors[i] = _sharedMaterials[i].GetColor(_baseColor);
            }

            _renderer.sharedMaterials = _sharedMaterials;
        }

        public void TurnOn(Vector3 destination, NavigationArea navMeshArea)
        {
            gameObject.SetActive(true);

            for (int i = 0; i < _sharedMaterials.Length; i++)
            {
                _sharedMaterials[i].SetFloat(_cutoffHeight, 0);
                _sharedMaterials[i].SetColor(_baseColor, _materialColors[i]);
            }

            _hp = _maxHp;
            ReachedDestination = false;
            _collider.enabled = true;
            _visualAsset.localScale = Vector3.zero;
            _agent.areaMask = 1 << NavMesh.GetAreaFromName("Nothing");

            if (navMeshArea == NavigationArea.pathA || navMeshArea == NavigationArea.both)
            {
                _agent.areaMask += 1 << NavMesh.GetAreaFromName("PathA");
            }

            if (navMeshArea == NavigationArea.pathB || navMeshArea == NavigationArea.both)
            {
                _agent.areaMask += 1 << NavMesh.GetAreaFromName("PathB");
            }

            _agent.enabled = true;
            _agent.isStopped = false;
            _agent.enabled = true;
            _agent.destination = destination;

            LeanTween.scale(_visualAsset.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutQuad);
        }

        private void TurnOff(bool instant = false)
        {
            if (_agent.enabled) _agent.isStopped = true;
            _agent.enabled = _collider.enabled = false;
            gameObject.SetActive(false);
            GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
        }

        public EnemyController SetReachedDestination(bool val)
        {
            ReachedDestination = val;

            return this;
        }

        public EnemyController ApplyDamage(float dmg)
        {
            if (!ReachedDestination)
            {
                _hp -= dmg;

                GameMaster.Instance.Gameplay.PlayHitSFX();

                if (_hp <= 0)
                {
                    _hp = 0;

                    DisposeDeath();
                }
                else
                {
                    LeanTween.value(gameObject, 0, 1, 0.33f).setOnUpdate((float val) => 
                    {
                        for(int i = 0; i < _materialColors.Length; i++)
                            _sharedMaterials[i].SetColor(_baseColor, Color.Lerp(_damageColor, _materialColors[i], val));
                    }).setOnComplete(() =>
                    {
                        for (int i = 0; i < _materialColors.Length; i++)
                            _sharedMaterials[i].SetColor(_baseColor, _materialColors[i]);
                    });
                }
            }

            return this;
        }

        public void DisposeReached()
        {
            LeanTween.scale(_visualAsset.gameObject, Vector3.one * 1.25f, _reachedDuration * 0.33f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
                LeanTween.scale(_visualAsset.gameObject, Vector3.zero, _reachedDuration * 0.66f).setEase(LeanTweenType.easeInQuad).setOnComplete(() => { TurnOff(); });
            });
        }

        private void DisposeDeath()
        {
            _agent.isStopped = true;
            _agent.enabled = _collider.enabled = false;

            GameMaster.Instance.Gameplay.CurrentLevel.RemoveEnemy();
            GameMaster.Instance.Gameplay.EnemyReward(_rewardPerKill, transform.position + Vector3.up * 2);

            LeanTween.value(gameObject, 0f, -1f, _deathDuration).setOnUpdate((float val) =>
            {
                for (int i = 0; i < _sharedMaterials.Length; i++)
                {
                    _sharedMaterials[i].SetFloat(_cutoffHeight, val);
                }
            }).setOnComplete(() => { TurnOff(); });
        }
    }
}

