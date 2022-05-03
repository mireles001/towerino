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
        private EnemyHpController _hpBar;
        private Material[] _sharedMaterials;
        private Collider _collider;

        // Bunch of public getters for the commodity of other components 
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

        // When we awake (only once, not even if object is returned to pooling system) we check
        // materials exposed variables so we can create shader effects in execution.
        private void Awake()
        {
            _agent.speed = _speed;
            _maxHp = _hp;
            _collider = GetComponent<Collider>();
            // We look and store property ID for quicker usage up next
            _cutoffHeight = Shader.PropertyToID("Vector1_b59e3bdf317448bd9a65115a1cea1cb1");
            _baseColor = Shader.PropertyToID("Color_36667b28f67e4ffe93dd0988b3e41eea");

            // check for shared materials, we create a unique instance of it and assign it to this
            // specific enemy for individual effects (death and damage)
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

        // Starting up enemies involves mainly in reseting NavMesh component
        public void TurnOn(Vector3 destination, NavigationArea navMeshArea)
        {
            // We set active first (or else navmesh fails)
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

            // We clear the NavMeshArea to nothing and the assign the areas that our
            // new spawned enemy has assigned in its data struct
            _agent.areaMask = 1 << NavMesh.GetAreaFromName("Nothing");

            if (navMeshArea == NavigationArea.pathA || navMeshArea == NavigationArea.both)
            {
                _agent.areaMask += 1 << NavMesh.GetAreaFromName("PathA");
            }

            if (navMeshArea == NavigationArea.pathB || navMeshArea == NavigationArea.both)
            {
                _agent.areaMask += 1 << NavMesh.GetAreaFromName("PathB");
            }

            // Enable all our nav agent variables and component
            _agent.enabled = true;
            _agent.isStopped = false;
            _agent.enabled = true;
            // Lets not forget to let it know its destination!
            _agent.destination = destination;

            LeanTween.scale(_visualAsset.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutQuad);
            
            GameMaster.Instance.Gameplay.UI.SetupEnemyHp(this);
        }

        // Set inactive and send it back to pooling system
        private void TurnOff(bool instant = false)
        {
            _hpBar = null;
            if (_agent.enabled) _agent.isStopped = true;
            _agent.enabled = _collider.enabled = false;
            gameObject.SetActive(false);
            GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
        }

        // Setting exposed boolean if enemy already reached destination (used by towers to know if its a valid target.
        // we do not want wasted bullets in already gone enemies, dont we?)
        public EnemyController SetReachedDestination(bool val)
        {
            ReachedDestination = val;
            return this;
        }

        public void SetHpBar(EnemyHpController hpBar) { _hpBar = hpBar; }

        // Apply damage to enemy
        public EnemyController ApplyDamage(float dmg)
        {
            if (!ReachedDestination)
            {
                _hp -= dmg;

                // hit sound request, plays random sound in hit clips array
                GameMaster.Instance.Gameplay.PlayHitSFX();

                // If hp reaches 0, we dispose of enemy
                if (_hp <= 0)
                {
                    _hp = 0;

                    DisposeDeath();
                }
                // If enemy is not dead, we animate the color value just for visual fx eye candy
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

        // When the enemy reached its destination we shrink it and the call TurnOff to do the rest
        public void DisposeReached()
        {
            _hpBar.TurnOff();
            LeanTween.scale(_visualAsset.gameObject, Vector3.one * 1.25f, _reachedDuration * 0.33f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
                LeanTween.scale(_visualAsset.gameObject, Vector3.zero, _reachedDuration * 0.66f).setEase(LeanTweenType.easeInQuad).setOnComplete(() => { TurnOff(); });
            });
        }

        // If player killed this enemy we turn off collider and stop agent movement
        private void DisposeDeath()
        {
            _hpBar.TurnOff();
            _agent.isStopped = true;
            _agent.enabled = _collider.enabled = false;

            // Remove this enemy from active enemy counter in LevelComponent
            GameMaster.Instance.Gameplay.CurrentLevel.RemoveEnemy();
            // Add reward money to player for killing this enemy
            GameMaster.Instance.Gameplay.EnemyReward(_rewardPerKill, transform.position + Vector3.up * 2);

            // Fancy effect for disolving enemies, then let TurnOff do the rest.
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

