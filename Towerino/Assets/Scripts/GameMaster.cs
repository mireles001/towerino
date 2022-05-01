using UnityEngine;

namespace Towerino
{
    public class GameMaster : ScriptableObject
    {
        public static GameMaster Instance
        {
            get
            {
                if (_instance == null) _instance = CreateInstance<GameMaster>();

                return _instance;
            }
        }
        private static GameMaster _instance;

        public bool GameInitialized { get; private set; }
        public int CurrentLevel { get; private set; } = 3;
        public GameController Gameplay { get; private set; }
        public FaderController Fader
        {
            get
            {
                if (_fader == null)
                    _fader = Instantiate(Resources.Load<GameObject>("UI/FadeInOut")).GetComponent<FaderController>();

                return _fader;
            }
        }
        private FaderController _fader;

        public GameMaster Initialize()
        {
            GameInitialized = true;
            return this;
        }

        public void LoadScene(int sceneIndex)
        {
            Fader.FadeIn(() => { GameUtils.LoadScene(sceneIndex); });
        }

        public void LoadCurrentLevel(GameController cb)
        {
            Fader.FadeIn(() => { GameUtils.LoadLevel($"Level{CurrentLevel}", cb); });
        }

        public GameMaster SetCurrentLevel(int level)
        {
            CurrentLevel = level;
            return this;
        }

        public void SetGameplay(GameController gameplay)
        {
            Gameplay = gameplay;
        }
        public void RemoveGamePlay()
        {
            Gameplay = null;
        }

        public void NextLevel()
        {
            CurrentLevel++;

            if (CurrentLevel > 3)
            {
                CurrentLevel = 1;
            }
        }

        private void Awake() { Debug.Log("[GameMaster] Awake"); }
        private void OnDestroy() { Debug.Log("[GameMaster] OnDestroy"); }
    }
}