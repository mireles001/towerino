using UnityEngine;

namespace Towerino
{
    // Singleton SO object to be refered by other objects to retrieve data across scenes and objects.
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
        // Current level progression is stored globaly, in case player goes to main menu
        // he/she can continue where player left off.
        public int CurrentLevel { get; private set; } = 1;
        public GameController Gameplay { get; private set; }
        // Fader object that once it is created it will stay as a non-destroyable on load object.
        // This is the main simple transition in-between scene and level loading.
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

        // Jump between scenes with single scene method loading
        public void LoadScene(int sceneIndex)
        {
            Fader.FadeIn(() => { GameUtils.LoadScene(sceneIndex); });
        }

        // Additive scene loading inside Game scene
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

#if UNITY_EDITOR
        private void Awake() { Debug.Log("[GameMaster] Awake"); }
        private void OnDestroy() { Debug.Log("[GameMaster] OnDestroy"); }
#endif
    }
}