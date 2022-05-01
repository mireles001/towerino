using UnityEngine;
using UnityEngine.SceneManagement;

namespace Towerino
{
    public class GameController : MonoBehaviour
    {
        public int PlayerMoney { get; private set; } = 10000;
        public PoolingSystem ActivePoolingSystem { get; private set; }
        public GameUIController UI { get { return _ui; } }
        public LevelController CurrentLevel { get { return _currentLevel; } }
        public TowerBaseController CurrentTowerSelection { get; private set; }

        [SerializeField] private GameObject _cameraWrapper = null;
        [SerializeField] private TowerData _towerBallista;
        [SerializeField] private TowerData _towerCannonBall;
        [SerializeField] private TowerData _towerFireBomb;
        [SerializeField] private AudioClip[] _hits = new AudioClip[0];

        private bool _paused;
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
                    CurrentTowerSelection.Deselect();
                    UI.CloseBuySell();
                    CurrentTowerSelection = null;
                }
                else UI.ToggleQuit();
            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject gameHit = hit.collider.gameObject;
                    if (gameHit.tag.Equals("Tower"))
                    {
                        TowerBaseController towerBase = gameHit.GetComponent<TowerBaseController>();
                        if (towerBase != null)
                        {
                            if (CurrentTowerSelection != null && CurrentTowerSelection != towerBase) CurrentTowerSelection.Deselect();

                            CurrentTowerSelection = towerBase;
                            CurrentTowerSelection.Select();
                            UI.OpenBuySell(CurrentTowerSelection);
                        }
                    }
                    else if (CurrentTowerSelection != null)
                    {
                        CurrentTowerSelection.Deselect();
                        UI.CloseBuySell();
                    }
                }
            }
        }

        public void GotoMenu()
        {
            GameUtils.SetVolume(_music, 0, GameMaster.Instance.Fader.FadeInOutDuration.y);
            GameMaster.Instance.LoadScene(0);
        }

        public void SetCurrentLevel(LevelController level)
        {
            _currentLevel = level;
        }

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

        public void LevelCleared()
        {
            bool gameCleared = GameMaster.Instance.CurrentLevel >= 3;
            GameMaster.Instance.NextLevel();

            if (gameCleared)
            {
                GameUtils.SetVolume(_music, 0, GameMaster.Instance.Fader.FadeInOutDuration.y);
                GameMaster.Instance.LoadScene(2);
            }
            else
            {
                LoadCurrentLevel();
            }
        }

        public void LoadCurrentLevel()
        {
            if (SceneManager.sceneCount > 1)
            {
                _cameraWrapper.transform.position = _cameraWrapperBasePosition;
                LeanTween.move(_cameraWrapper, _cameraWrapperMovePosition, GameMaster.Instance.Fader.FadeInOutDuration.x).setEase(LeanTweenType.easeInQuad);
            }
            else _cameraWrapper.transform.position = _cameraWrapperMovePosition;

            GameMaster.Instance.LoadCurrentLevel(this);
        }

        public void LoadLevelCompleted()
        {
            ActivePoolingSystem.TurnOffPooledObjects();

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

        public void PlayHitSFX()
        {
            if (_hits.Length > 0) _music.PlayOneShot(_hits[Random.Range(0, _hits.Length - 1)]);
        }

        public void BuyTower(TowerType towerType)
        {
            TowerData data = (TowerData)GetTowerData(towerType);

            if (CurrentTowerSelection.HasTower || PlayerMoney < data.BuyPrice) return;

            PlayerMoney -= data.BuyPrice;
            CurrentTowerSelection.SetTower(ActivePoolingSystem.GetObject(data.Prefab).GetComponent<TowerController>());
            UI.CloseBuySell();
        }

        public void SellTower()
        {
            if (!CurrentTowerSelection.HasTower) return;

            TowerData data = (TowerData)GetTowerData(CurrentTowerSelection.TowerType);
            PlayerMoney += data.SellPrice;
            UI.RewardMoney(data.SellPrice, CurrentTowerSelection.GetHUDPosition());
            CurrentTowerSelection.UnsetTower();
            UI.CloseBuySell();
        }

        public void EnemyReward(int reward, Vector3 enemyPosition)
        {
            PlayerMoney += reward;

            UI.RewardMoney(reward, enemyPosition);
        }

        public void GameOver()
        {
            Debug.Log("You lose");
        }

        private void OnDestroy()
        {
            ActivePoolingSystem.FlushData();
            GameMaster.Instance.RemoveGamePlay();
        }

        public void ApplyLightConfig(ScenarioConfigSO config)
        {

        }
    }
}
