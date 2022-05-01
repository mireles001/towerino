using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Towerino
{
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

        [SerializeField] private Transform _destination;
        [SerializeField] private float _headStartDuration = 5;
        [SerializeField] private float _endOfWaveWait = 3;
        [SerializeField] private Wave[] _enemyWaves = new Wave[0];
        [SerializeField, Space] private MeshRenderer[] _navPathRenderers = new MeshRenderer[0];
        [SerializeField] private ScenarioConfigSO _lightConfiguration = null;

        private int _currentWaveIndex, _health, _activeEnemyCounter;
        private bool _headStartTimerRunning, _waveActive, _waveCleared;
        private float _headStartTimer, _waveTimer, _nextToSpawnTimer;
        private List<Enemy> _currentEnemyWave;

        private void Start()
        {
            for (int i = 0; i < _navPathRenderers.Length; i++)
            {
                _navPathRenderers[i].enabled = false;
            }

            if (GameMaster.Instance.Gameplay)
            {
                if (_lightConfiguration != null) GameMaster.Instance.Gameplay.ApplyLightConfig(_lightConfiguration);

                GameMaster.Instance.Gameplay.SetCurrentLevel(this);
                GameMaster.Instance.Gameplay.UI.UpdateLevelText($"{GameMaster.Instance.CurrentLevel}-1");
                Invoke("StartNextWave", 2); // Lets give a breather to our players...
            }
        }

        private void Update()
        {
            if (_health == 0) return;

            if (_headStartTimerRunning)
            {
                _headStartTimer -= Time.deltaTime;
                GameMaster.Instance.Gameplay.UI.UpdateHeadStartTimer(_headStartTimer / _headStartDuration);
                if (_headStartTimer <= 0)
                {
                    GameMaster.Instance.Gameplay.UI.HideHeadStartTimer();
                    GameMaster.Instance.Gameplay.UI.ShowHealthMeter();

                    _headStartTimerRunning = false;
                    _waveActive = true;
                    _waveTimer = 0;
                }
            }
            else if (_waveActive)
            {

                _waveTimer += Time.deltaTime;

                if (_waveCleared)
                {
                    if (_waveTimer >= _endOfWaveWait)
                    {
                        // TODO: Send UI first
                        StartNextWave();
                    }
                }
                else
                {
                    if (_currentEnemyWave.Count != 0 && _nextToSpawnTimer <= _waveTimer)
                    {
                        do
                        {
                            SpawnEnemy(_currentEnemyWave[0]);
                            _currentEnemyWave.RemoveAt(0);
                        } while (_currentEnemyWave.Count > 0 && _currentEnemyWave[0].spawnTimer <= _waveTimer);

                        if (_currentEnemyWave.Count > 0) _nextToSpawnTimer = _currentEnemyWave[0].spawnTimer;
                    }

                    if (_currentEnemyWave.Count == 0 && _activeEnemyCounter == 0 && !_waveCleared)
                    {
                        _waveCleared = true;
                        _waveTimer = 0;
                    }
                }
            }
        }

        private void StartNextWave()
        {
            _waveActive = _waveCleared = false;

            if (_currentWaveIndex == _enemyWaves.Length)
            {
                GameMaster.Instance.Gameplay.LevelCleared();
            }
            else
            {
                _health = 3;
                _activeEnemyCounter = 0;
                StartHeadStartCountDown();
                _nextToSpawnTimer = ArraySorter();
                _currentWaveIndex++;
                GameMaster.Instance.Gameplay.UI.UpdateLevelText($"{GameMaster.Instance.CurrentLevel}-{_currentWaveIndex}");
            }
        }

        private void StartHeadStartCountDown()
        {
            _headStartTimerRunning = true;
            _headStartTimer = _headStartDuration;

            GameMaster.Instance.Gameplay.UI.ShowHeadStartTimer();
        }

        public void EnemyReachedDestination(EnemyController enemy)
        {
            _health--;
            GameMaster.Instance.Gameplay.UI.UpdateHealthMeter(_health);
            enemy.DisposeReached();
            RemoveEnemy();

            if (_health == 0)
            {
                GameMaster.Instance.Gameplay.GameOver();
            }
        }

        private void SpawnEnemy(Enemy enemyData)
        {
            Debug.Log($"SpawnEnemy type: {enemyData.gameObjectPrefab.name}");
            //EnemyController enemy = GameMaster.Instance.Gameplay.ActivePoolingSystem.GetObject(enemyData.gameObjectPrefab).GetComponent<EnemyController>();
            //enemy.transform.SetPositionAndRotation(enemyData.spawnPoint.position, Quaternion.identity);
            //enemy.TurnOn(_destination.position, enemyData.navMeshArea);

            Instantiate(enemyData.gameObjectPrefab, enemyData.spawnPoint.position, Quaternion.identity).GetComponent<EnemyController>().TurnOn(_destination.position, enemyData.navMeshArea);

            _activeEnemyCounter++;
        }

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

        public void RemoveEnemy()
        {
            _activeEnemyCounter--;
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (_waveActive)
            {
                GUI.color = Color.black;
                GUI.Label(new Rect(10, 40, 250, 30), $"Wave timer: {_waveTimer}");
                GUI.Label(new Rect(10, 60, 250, 30), $"Active Enemies: {_activeEnemyCounter}");
            }
        }
#endif
    }
}
