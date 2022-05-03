using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace Towerino
{
    // Controller for the main Gameplay scene. This bad boy is in charge of requesting
    // additive scene loads. This is the main manager of the game, controlling UI, current level scopes
    // and determining if user is buying, selling, gaining money, losing or moving to next level.
    public class GameController : MonoBehaviour
    {
        public float HeadStartDuration { get { return _headStartDuration; } }
        public float WaveEndWaitDuration { get { return _waveEndWaitDuration; } }
        public Camera GamePlayCamera { get { return _camera; } }
        public PoolingSystem ActivePoolingSystem { get; private set; }
        public GameUIController UI { get { return _ui; } }
        public LevelController CurrentLevel { get { return _currentLevel; } }
        public TowerBaseController CurrentTowerSelection { get; private set; }

        [SerializeField]
        private GameObject _cameraWrapper = null;
        [SerializeField]
        private Light _mainLight = null;
        [SerializeField]
        private float _headStartDuration = 5;
        [SerializeField]
        private float _waveEndWaitDuration = 3;
        [SerializeField, Tooltip("Attack hit shuffled sounds")]
        private AudioClip[] _hits = new AudioClip[0];

        // IMPORTANT: These are the tower data structs, where we store what prefabs, human name and buy/sell prices
        // for each tower type
        [SerializeField, Space, Header("Tower Prefabs")]
        private TowerData _towerBallista;
        [SerializeField]
        private TowerData _towerCannonBall;
        [SerializeField]
        private TowerData _towerFireBomb;

        private bool _paused;
        private int _playerMoney;
        private Vector3 _cameraWrapperBasePosition;
        private Vector3 _cameraWrapperMovePosition;
        private Camera _camera;
        private GameUIController _ui;
        private AudioSource _music;
        private LevelController _currentLevel;

        private void Start()
        {
            GameMaster.Instance.Initialize().SetGameplay(this);
            _camera = Camera.main;
            _ui = GetComponent<GameUIController>().StartUp(this);
            ActivePoolingSystem = GetComponent<PoolingSystem>().StartUp();
            _music = GetComponent<AudioSource>();
            _cameraWrapperBasePosition = _cameraWrapper.transform.position;
            _cameraWrapperMovePosition = _cameraWrapperBasePosition + _cameraWrapper.transform.forward * 10;

            LoadCurrentLevel();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (CurrentTowerSelection != null)
                {
                    ReleaseTowerSelection();
                }
                else UI.ToggleQuit();
            }

            // Upon click down we ray cast and see if we hit a TowerBase (small box colliders in the map)
            // and openn the Buy/Sell panel. We do some validations if player clicks a new tower base
            // or just clicking the same towerbase without any purpose.
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
                {
                    GameObject gameHit = hit.collider.gameObject;
                    if (gameHit.tag.Equals("Tower"))
                    {
                        TowerBaseController towerBase = gameHit.GetComponent<TowerBaseController>();
                        if (towerBase != null)
                        {
                            if (CurrentTowerSelection != null && CurrentTowerSelection != towerBase)
                            {
                                CurrentTowerSelection.Deselect();
                            }

                            CurrentTowerSelection = towerBase;
                            CurrentTowerSelection.Select();
                            UI.OpenBuySell(CurrentTowerSelection);
                        }
                    }
                    // Raycasting no TowerBase? We close everything!
                    else if (CurrentTowerSelection != null)
                    {
                        CurrentTowerSelection.Deselect();
                        UI.CloseBuySell();
                    }
                }
            }
        }

        public void ReleaseTowerSelection()
        {
            CurrentTowerSelection.Deselect();
            UI.CloseBuySell();
            CurrentTowerSelection = null;
        }

        public void GotoMenu()
        {
            GameUtils.SetVolume(_music, 0, GameMaster.Instance.Fader.FadeInOutDuration.y);
            GameMaster.Instance.LoadScene(0);
        }

        // Additive loaded scene use this function to mark itself as the current level class
        public void SetCurrentLevel(LevelController level) { _currentLevel = level; }

        // Nullable return function that returns tower data struct info.
        // Used by Buy/Sell mechanics
        public TowerData? GetTowerData(TowerType towerType)
        {
            TowerData? data;
            switch (towerType)
            {
                case TowerType.ballistaTower:
                    data = _towerBallista;
                    break;
                case TowerType.cannonTower:
                    data = _towerCannonBall;
                    break;
                case TowerType.fireBombTower:
                    data = _towerFireBomb;
                    break;
                default:
                    data = null;
                    break;
            }

            return data;
        }

        // If user wins we call GameMaster to increase current level index, and then
        // Loads new level or if we detect that is bigger than 3 (total levels inn game)
        // we send player to credits scene.
        public void LevelCleared()
        {
            bool gameCleared = GameMaster.Instance.CurrentLevel >= 3;
            GameMaster.Instance.NextLevel();

            if (gameCleared)
            {
                GameUtils.SetVolume(_music, 0, GameMaster.Instance.Fader.FadeInOutDuration.y);
                GameMaster.Instance.LoadScene(2);
            }
            else LoadCurrentLevel();
        }

        // If user dies we just reset the same level
        public void GameOver() { LoadCurrentLevel(); }

        // This function only request for an additive load scene, and does some animation to the camera
        // this is only for the looks. Ah! we also close UI in case user had Buy/Sell panel opened.
        public void LoadCurrentLevel()
        {
            if (SceneManager.sceneCount > 1)
            {
                _cameraWrapper.transform.position = _cameraWrapperBasePosition;
                LeanTween.move(_cameraWrapper, _cameraWrapperMovePosition, GameMaster.Instance.Fader.FadeInOutDuration.x).setEase(LeanTweenType.easeInQuad);
            }
            else _cameraWrapper.transform.position = _cameraWrapperMovePosition;

            UI.CloseBuySell();
            GameMaster.Instance.LoadCurrentLevel(this);
        }

        // Used as a callback when additive level scene load is complete.
        // Flush pool is we have something there and we call FadeOut (now that the new level is loaded).
        public void LoadLevelCompleted()
        {
            ActivePoolingSystem.FlushData();
            UI.HideHealthMeter();

            LeanTween.move(_cameraWrapper, _cameraWrapperBasePosition, GameMaster.Instance.Fader.FadeInOutDuration.y).setEase(LeanTweenType.easeOutQuad);
            GameMaster.Instance.Fader.FadeOut();
        }

        public void PauseGame()
        {
            _paused = !_paused;
            Time.timeScale = _paused ? 0 : 1;

            if (_paused) _music.Pause();
            else _music.Play();
        }

        // PlayOneShot audio to use the same audiosource
        public void PlayHitSFX()
        {
            if (_hits.Length > 0) _music.PlayOneShot(_hits[Random.Range(0, _hits.Length - 1)]);
        }


        // Modify player money and request UI update
        private void ModifyPlayerMoney(int addMoney)
        {
            _playerMoney += addMoney;
            UI.UpdateMoney(_playerMoney);
        }
        
        // Overrides money (used only at start of level) and the updates UI
        public void SetPlayerMoney(int startUpMoney)
        {
            _playerMoney = startUpMoney;
            ModifyPlayerMoney(0);
        }

        public void BuyTower(TowerType towerType)
        {
            TowerData data = (TowerData)GetTowerData(towerType);
            if (CurrentTowerSelection.HasTower || _playerMoney < data.BuyPrice) return;
            ModifyPlayerMoney(-data.BuyPrice);
            // Creates or pulls an inactive object from PoolingSystem
            CurrentTowerSelection.SetTower(ActivePoolingSystem.GetObject(data.Prefab).GetComponent<TowerController>());
            UI.CloseBuySell();
        }

        public void SellTower()
        {
            if (!CurrentTowerSelection.HasTower) return;

            TowerData data = (TowerData)GetTowerData(CurrentTowerSelection.TowerType);
            ModifyPlayerMoney(data.SellPrice);
            UI.RewardMoney(data.SellPrice, CurrentTowerSelection.GetHUDPosition());
            // Prepares to set inactive and return object to PoolingSystem
            CurrentTowerSelection.UnsetTower();
            UI.CloseBuySell();
        }

        // When player kills enemies we reward with money! (amount of money defined in enemy prefab)
        // Reward money intends to create a HUD element temporaly floating on top of destroyed enemy
        // showing earnt money.
        public void EnemyReward(int reward, Vector3 enemyPosition)
        {
            ModifyPlayerMoney(reward);
            UI.RewardMoney(reward, enemyPosition);
        }

        // When exiting this scene we flush our pooling manager
        private void OnDestroy()
        {
            ActivePoolingSystem.FlushData();
            GameMaster.Instance.RemoveGamePlay();
        }

        // Retrieves from asigned Lighting SO asset to setup the lighting in current level
        public void ApplyLightConfig(ScenarioConfigSO config)
        {
            _mainLight.transform.eulerAngles = config.lightRotation;
            _mainLight.color = config.lightColor;
            RenderSettings.subtractiveShadowColor = config.shadowColor;
            RenderSettings.ambientSkyColor = config.skyColor;
            RenderSettings.ambientEquatorColor = config.midColor;
            RenderSettings.ambientGroundColor = config.lowColor;
        }
    }
}
