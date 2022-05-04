using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Towerino
{
    // Current level controller. In charge of spawning enemies, managing player health and letting UI to show
    // some visual stuff between Waves
    public class LevelController : MonoBehaviour
    {
        [System.Serializable]
        private struct Wave
        {
            public Enemy[] enemyQueue;
        }

        [System.Serializable]
        private struct Enemy
        {
            public GameObject gameObjectPrefab;
            public float spawnTimer;
            public Transform spawnPoint;
            public NavigationArea navMeshArea;
        }

        // Each level have a different start up money amount (to balance difficulty)
        [SerializeField]
        private int _levelStartMoney = 100;
        [SerializeField]
        private Transform _destination;
        // Total waves per LEVEL, I used 3 for this version. Each wave have an array of Enemy data structs
        [SerializeField]
        private Wave[] _enemyWaves = new Wave[0];
        [SerializeField, Space]
        private MeshRenderer[] _navPathRenderers = new MeshRenderer[0];
        [SerializeField]
        private ScenarioConfigSO _lightConfiguration = null;
        // I have a light object to be able to work! If not it looks pitch black when im not playing
        [SerializeField, Space]
        private GameObject _devLight = null;

        private int _currentWaveIndex, _health, _activeEnemyCounter;
        private bool _headStartTimerRunning, _waveActive, _waveCleared;
        private float _headStartTimer, _waveTimer, _nextToSpawnTimer;
        private List<Enemy> _currentEnemyWave;

        // You better be inside Game scene as an additive scene cuz if not, this wont do anything.
        private void Start()
        {
            _devLight.SetActive(false);
            for (int i = 0; i < _navPathRenderers.Length; i++)
            {
                _navPathRenderers[i].enabled = false;
            }

            if (GameMaster.Instance.Gameplay)
            {
                if (_lightConfiguration != null) GameMaster.Instance.Gameplay.ApplyLightConfig(_lightConfiguration);

                _health = 3;
                GameMaster.Instance.Gameplay.SetCurrentLevel(this);
                GameMaster.Instance.Gameplay.SetPlayerMoney(_levelStartMoney);
                GameMaster.Instance.Gameplay.UI.UpdateLevelText($"{GameMaster.Instance.CurrentLevel}-1");
                Invoke("StartNextWave", 0.75f); // Lets give a breather to our players...
            }
        }

        // Lots of things happening here...
        private void Update()
        {
            // If player is dead, we dont do anything.
            if (_health == 0) return;

            // Checks if headstart (warm up before wave) is running to keep doing that countdown
            // and updates UI elements. When done we set the wave as active
            if (_headStartTimerRunning)
            {
                _headStartTimer -= Time.deltaTime;
                GameMaster.Instance.Gameplay.UI.UpdateHeadStartTimer(1 - _headStartTimer / GameMaster.Instance.Gameplay.HeadStartDuration);
                if (_headStartTimer <= 0)
                {
                    GameMaster.Instance.Gameplay.UI.HideHeadStartTimer();

                    _headStartTimerRunning = false;
                    _waveActive = true;
                    _waveTimer = 0;
                }
            }
            // If the wave is active we jump here
            else if (_waveActive)
            {
                _waveTimer += Time.deltaTime;

                // Check if the wave is cleared (no more enemies to spawn and no active enemies on screen
                if (_waveCleared)
                {
                    // We do not go instantly to the next wave, we need the player to "process" he/she won!
                    // we wait a little before moving forward. This is were we taste victory.
                    if (_waveTimer >= GameMaster.Instance.Gameplay.WaveEndWaitDuration)
                    {
                        StartNextWave();
                    }
                }
                else
                {
                    if (_currentEnemyWave.Count != 0 && _nextToSpawnTimer <= _waveTimer)
                    {
                        // (Always afraid of do/whiles) Check next enemy to spawn timer 
                        // We spawn all of them that passes our time check vs time spawner validation
                        do
                        {
                            SpawnEnemy(_currentEnemyWave[0]);
                            _currentEnemyWave.RemoveAt(0);
                        } while (_currentEnemyWave.Count > 0 && _currentEnemyWave[0].spawnTimer <= _waveTimer);

                        if (_currentEnemyWave.Count > 0) _nextToSpawnTimer = _currentEnemyWave[0].spawnTimer;
                    }

                    // If no more enemies to spawn, no active enemies on screen and wave is not yet cleared
                    // We set it as cleared!
                    if (_currentEnemyWave.Count == 0 && _activeEnemyCounter == 0 && !_waveCleared)
                    {
                        _waveCleared = true;
                        _waveTimer = 0;

                        if (GameMaster.Instance.Gameplay.CurrentTowerSelection != null)
                        {
                            GameMaster.Instance.Gameplay.ReleaseTowerSelection();
                        }
                    }
                }
            }
        }

        private void StartNextWave()
        {
            _waveActive = _waveCleared = false;

            // If we are out of waves, we move forward to the next level
            if (_currentWaveIndex == _enemyWaves.Length)
            {
                GameMaster.Instance.Gameplay.LevelCleared();
            }
            // Else, we prepare everything for out next wave (increase wave index and update UI properly)
            else
            {
                _activeEnemyCounter = 0;
                StartHeadStartCountDown();
                _nextToSpawnTimer = ArraySorter();
                _currentWaveIndex++;
                GameMaster.Instance.Gameplay.UI.UpdateLevelText($"{GameMaster.Instance.CurrentLevel}-{_currentWaveIndex}");
                GameMaster.Instance.Gameplay.UI.ShowAnnouncer(GameMaster.Instance.CurrentLevel, _currentWaveIndex);
            }

            if (_currentWaveIndex == 1)
            {
                GameMaster.Instance.Gameplay.UI.UpdateHealthMeter(_health);
                GameMaster.Instance.Gameplay.UI.ShowHealthMeter();
            }
        }

        // First step in each wave. A warm up head start time for the player to make advantage moves.
        private void StartHeadStartCountDown()
        {
            _headStartTimerRunning = true;
            _headStartTimer = GameMaster.Instance.Gameplay.HeadStartDuration;
            GameMaster.Instance.Gameplay.UI.ShowHeadStartTimer();
        }

        // Spawners checks for available inactive enemy in our PoolingSystem and put it back to action!
        private void SpawnEnemy(Enemy enemyData)
        {
            EnemyController enemy = GameMaster.Instance.Gameplay.ActivePoolingSystem.GetObject(enemyData.gameObjectPrefab).GetComponent<EnemyController>();
            enemy.transform.SetPositionAndRotation(enemyData.spawnPoint.position, Quaternion.identity);
            enemy.TurnOn(_destination.position, enemyData.navMeshArea);

            _activeEnemyCounter++;
        }

        public void RemoveEnemy() { _activeEnemyCounter--; }

        // If enemy reaches destination we decrease health and update what we need to update
        public void EnemyReachedDestination(EnemyController enemy)
        {
            _health--;
            GameMaster.Instance.Gameplay.PlayHeartSFX();
            GameMaster.Instance.Gameplay.UI.UpdateHealthMeter(_health);

            enemy.DisposeReached();
            RemoveEnemy();

            if (_health == 0) GameMaster.Instance.Gameplay.GameOver();
        }

        // We sort current enemy waves by spawning time to make things easier for us.
        private float ArraySorter()
        {
            Enemy[] enemyWaveArray = _enemyWaves[_currentWaveIndex].enemyQueue;

            Array.Sort(enemyWaveArray, (x, y) => x.spawnTimer.CompareTo(y.spawnTimer));
            Array.Sort(enemyWaveArray, delegate (Enemy a, Enemy b)
            {
                return a.spawnTimer.CompareTo(b.spawnTimer);
            });

            _currentEnemyWave = enemyWaveArray.ToList();

            return _currentEnemyWave[0].spawnTimer;
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (_waveActive)
            {
                GUI.color = Color.black;
                GUI.Label(new Rect(10, 40, 250, 30), $"Wave timer: {_waveTimer.ToString("0.00")}");
                GUI.Label(new Rect(10, 60, 250, 30), $"Active Enemies: {_activeEnemyCounter}");
                GUI.Label(new Rect(10, 80, 250, 30), $"Health: {_health}");
            }
        }
#endif
    }
}
