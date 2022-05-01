using UnityEngine;
using UnityEngine.SceneManagement;

namespace Towerino
{
    public static class GameUtils
    {
        public static void LoadScene(int sceneIndex)
        {
            SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
        }

        public static void LoadLevel(string sceneName, GameController cb)
        {
            if (SceneManager.sceneCount > 1)
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));

            var loadProgress = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            loadProgress.completed += (op) => cb.LoadLevelCompleted();
        }

        public static void SetVolume(AudioSource audio, float goTo, float duration)
        {
            LeanTween.value(audio.gameObject, audio.volume, goTo, duration).setOnUpdate((float val) => { audio.volume = val; });
        }
    }

    public enum TowerType
    {
        ballistaTower, cannonTower, fireBombTower
    }

    public enum ProjectileType
    {
        ballista, cannonBall, fireBomb
    }

    public enum NavigationArea
    {
        pathA, pathB, both
    }

    [System.Serializable]
    public struct TowerData
    {
        public TowerType TowerType;
        public string Name;
        public GameObject Prefab;
        public int BuyPrice;
        public int SellPrice;
    }
}
